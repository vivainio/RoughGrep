using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using TrivialBehind;

namespace RoughGrep
{
    public class CmdRunner
    {
        public string Bin;
        public string Arg;
        public string Workdir;
    }

    public class ExternalCommand
    {
        public string Pattern;
        public CmdRunner Runner;
    }

    public static class Logic
    {
        public static string WorkDir = null;
        public static string RipGrepExecutable = null;
        public static string RgExtraArgs = "";

        // if set, will trigger search for this on launch
        public static string InitialSearchString = null;
        static List<string> Lines = new List<string>();
        public static BindingList<string> DirHistory;
        public static BindingList<string> SearchHistory;

        public static Process CurrentSearchProcess = null;

        public static List<ExternalCommand> ExternalCommands = new List<ExternalCommand>();
        public static Lazy<string> Tutorial = new Lazy<string>(() => "RoughGrep version " + GetVersion() + "\n\n" +
            "Tutorial: space=preview, enter=edit, p=edit parent project dir,\nd=containing dir, n=take note, \ng=git history, f=find in results\nF12=open selected word");
        public static string RgNotFoundError =
            "RipGrep executable (rg.exe) not found in path. Install it by running:\nwinget install --id=BurntSushi.ripgrep.MSVC";

        public static SettingsStorage<StoredSettings> SettingsStorage =
            new SettingsStorage<StoredSettings>("roughgrep", "settings.json");

        public static string[] AvailableFlags = new string[]
        {
            "--context 2",
            "--files",
            "--fixed-strings",
            "-g *.cs -g *.csproj",
            "-M 1000",
            "-m 5 --smart-case",
            "--no-ignore",
            "--pcre2",
        };

        public static void SetupShellIntegration()
        {
            var appPath = Application.ExecutablePath;
            var iconPath = $"{Application.StartupPath}\\roughgrep.ico";
            const string keyPrefix = @"HKEY_CURRENT_USER\Software\Classes\directory";

            CreateRegistryEntry(
                $@"{keyPrefix}\shell\RoughGrep",
                $"\"{appPath}\" \"--launch\" \"%V\"",
                iconPath
            );
            CreateRegistryEntry($@"{keyPrefix}\Background\shell\RoughGrep", appPath, iconPath);
        }

        private static void CreateRegistryEntry(string keyPath, string appPath, string iconPath)
        {
            Registry.SetValue(keyPath, "", "");
            Registry.SetValue($@"{keyPath}\command", "", appPath);
            Registry.SetValue(keyPath, "Icon", iconPath);
        }

        private static string SearchExecutable(string exeName)
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var paths = path.Split(';');
            foreach (var p in paths)
            {
                var exePath = Path.Combine(p, exeName);
                if (File.Exists(exePath))
                {
                    return exePath;
                }
            }
            return null;
        }

        public static void InitApp()
        {
            var extraArgs = Environment.GetCommandLineArgs().Skip(1).ToList();

            var launchDir = extraArgs.FirstOrDefault() == "--launch" ? extraArgs[1] : null;
            if (launchDir != null)
            {
                // launch from shell is "special", nothing else to consider
                extraArgs.Clear();
            }
            var lastArg = extraArgs.LastOrDefault();
            if (lastArg != null && !lastArg.StartsWith("-"))
            {
                InitialSearchString = lastArg;
                extraArgs.Remove(lastArg);
            }

            // --smart-case is a sensible default behavior
            if (extraArgs.Count == 0)
            {
                extraArgs = new List<string> { "--smart-case" };
            }

            RipGrepExecutable = SearchExecutable("rg.exe");
            Logic.RgExtraArgs = string.Join(" ", extraArgs);
            Logic.WorkDir = launchDir == null ? Directory.GetCurrentDirectory() : launchDir;
            var rc = ScriptRunner.FindScript();
            SettingsStorage.LoadAndModify(s =>
            {
                PrependIfNew(s.DirHistory, Logic.WorkDir);
                DirHistory = new BindingList<string>(s.DirHistory);
                SearchHistory = new BindingList<string>(s.SearchHistory);
            });

            if (rc != null)
            {
                ScriptRunner.RunScript(rc);
            }
            TrivialBehinds.RegisterBehind<MainFormUi, MainFormBehind>();
        }

        public static Action Debounce(int delayms, Action action)
        {
            Stopwatch sw = new Stopwatch();
            return () =>
            {
                var runIt = !sw.IsRunning ? true : sw.ElapsedMilliseconds > delayms;
                if (runIt)
                {
                    action();
                    sw.Restart();
                }
                else
                {
                    ;
                    // skipping;
                }
            };
        }

        internal static void KillOtherInstancesOfProcess()
        {
            var currentProcess = Process.GetCurrentProcess();
            var pid = currentProcess.Id;
            var procs = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (var p in procs)
            {
                if (p.Id != pid)
                {
                    p.Kill();
                }
            }
        }

        private static string CreateArgsForRg(string text)
        {
            if (RgExtraArgs.StartsWith("--files"))
            {
                var globs = string.Join(" ", text.Split(' ').Select(t => $"-g {t}"));
                // special handling for --files, interpret search text as -g glob
                return $"{RgExtraArgs} {globs}";
            }

            var maxcount = RgExtraArgs.Contains("-m ") ? "" : "-m 1000";
            var maxlen = RgExtraArgs.Contains("-M ") ? "" : "-M 300";

            return $"{RgExtraArgs} {maxcount} {maxlen} --heading -n -- \"{text}\"";
        }

        private static int CurrentSearchSession = 0;
        private static Regex rgLineRegex = new Regex(@"^\d+[:-]", RegexOptions.Compiled);
        // use zero width space for great justice
        private const string fakeStartComment = "/" + "\u200B" + "*";
        private const string fakeEndComment = "*" + "\u200B" + "/";
        private static string SanitizeLineWithMultiLineComments(string line)
        {
            return line.Replace("/*", fakeStartComment).Replace("*/", fakeEndComment);
        }
        public static void StartSearch(MainFormUi ui)
        {
            CurrentSearchSession++;
            var session = CurrentSearchSession;
            var text = ui.searchTextBox.Text;
            if (text == null || text.Trim().Length == 0)
            {
                return;
            }
            WorkDir = ui.dirSelector.Text;
            if (!Directory.Exists(WorkDir))
            {
                ui.resultBox.Text = $"Directory does not exist: '{WorkDir}'";
                return;
            }
            var originalText = text;
            text = text.Replace("\"", "\\\"");
            var p = new Process();

            PrependHistoryListEntries(WorkDir, originalText);
            ui.dirSelector.SelectedIndex = 0;
            ui.searchTextBox.SelectedIndex = 0;

            var args = CreateArgsForRg(text);
            Debugger.Log(0, "", args);
            ui.statusLabelCurrentArgs.Text = args;
            AssignStartInfo(p.StartInfo, RipGrepExecutable, args);
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            ui.resultBox.ReadOnly = false;
            ui.resultBox.ClearAll();
            Lines.Clear();
            p.EnableRaisingEvents = true;
            var toFlush = new List<string>();
            var flushlock = new Object();
            // do not decorate
            var emitCommentsForFiles = !RgExtraArgs.StartsWith("--files");
            void appendText(string textToAppend)
            {
                ui.resultBox.AppendText(textToAppend);
            }
            void RichFlush(IEnumerable<string> lines)
            {
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    if (line.Length == 0)
                    {
                        Console.WriteLine(line);
                        sb.Append("\r\n");
                        continue;
                    }

                    if (rgLineRegex.IsMatch(line))
                    {
                        var parts = line.Split(new[] { ":", "-" }, 2, StringSplitOptions.None);
                        // context
                        if (parts.Length == 1)
                        {
                            sb.Append(line + "\r\n");
                        }
                        else
                        {
                            sb.Append(" ").Append(SanitizeLineWithMultiLineComments(parts[1]).Trim()).Append("\r\n");
                        }
                    }
                    else
                    {
                        if (emitCommentsForFiles)
                        {
                            sb.Append("//- ");
                        }
                        sb.Append(line).Append("\r\n");
                    }
                }
                var toAdd = sb.ToString();
                appendText(toAdd);
            }

            Action doFlush = () =>
            {
                var fl = toFlush;
                toFlush = new List<string>();
                // do not write ANYTHING if it's coming from older search session

                if (session != CurrentSearchSession)
                {
                    return;
                }

                RichFlush(fl);
            };
            Action debouncedFlush = Debounce(100, doFlush);

            p.OutputDataReceived += (o, ev) =>
            {
                if (ev.Data == null)
                {
                    return;
                }
                lock (flushlock)
                {
                    Lines.Add(ev.Data);
                    toFlush.Add(ev.Data);
                    ui.resultBox.Invoke(debouncedFlush);
                }
            };
            Action hideAbort = () => ui.btnAbort.Visible = false;
            Action searchReady = () =>
            {
                ui.resultBox.SelectionStart = 0;
                ui.resultBox.SelectionEnd = 0;
                ui.resultBox.ReadOnly = true;
            };
            p.ErrorDataReceived += (o, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }
                toFlush.Add(e.Data);

                ui.resultBox.Invoke(doFlush);
            };

            void doProcessExit()
            {
                // we should watForExit since the output may still be coming when running this.
                // beats my why
                p.WaitForExit();
                ui.btnAbort.Invoke(hideAbort);
                ui.resultBox.Invoke(doFlush);
                ui.resultBox.Invoke(searchReady);
                CurrentSearchProcess = null;
            }

            p.Exited += (o, ev) => doProcessExit();
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            CurrentSearchProcess = p;
            ui.btnAbort.Visible = true;
            SettingsStorage.LoadAndModify(s =>
            {
                s.DirHistory = Logic.DirHistory.Take(20).ToList();
                s.SearchHistory = Logic.SearchHistory.Take(20).ToList();
            });
        }

        static void PrependIfNew<T>(IList<T> coll, T entry)
            where T : IComparable<T>
        {
            coll.RemoveAll(x => x.Equals(entry));
            coll.Insert(0, entry);
        }

        static void PrependHistoryListEntries(string dirEntry, string searchHistory)
        {
            Logic.DirHistory.RaiseListChangedEvents = false;
            Logic.SearchHistory.RaiseListChangedEvents = false;
            PrependIfNew(Logic.DirHistory, dirEntry);
            PrependIfNew(Logic.SearchHistory, searchHistory);
            Logic.DirHistory.RaiseListChangedEvents = true;
            Logic.SearchHistory.RaiseListChangedEvents = true;
            Logic.DirHistory.ResetBindings();
            Logic.SearchHistory.ResetBindings();
        }

        internal static void RunExternal(string file, int lineNum)
        {
            var cmd = Logic.ExternalCommands.FirstOrDefault(c => Regex.IsMatch(file, c.Pattern));
            if (cmd == null)
            {
                return;
            }

            var arg = cmd
                .Runner.Arg.Replace("[[file]]", file)
                .Replace("[[line]]", lineNum.ToString());

            var p = new Process();
            AssignStartInfo(p.StartInfo, cmd.Runner.Bin, arg);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = false;
            if (!string.IsNullOrEmpty(cmd.Runner.Workdir))
                p.StartInfo.WorkingDirectory = cmd.Runner.Workdir;

            p.Start();
        }

        public static void AssignStartInfo(ProcessStartInfo psi, string fname, string arguments)
        {
            psi.FileName = fname;
            psi.Arguments = arguments;
            psi.WorkingDirectory = WorkDir;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
        }

        public static ProcessStartInfo CreateStartInfo(string fname, string arguments)
        {
            var psi = new ProcessStartInfo();
            AssignStartInfo(psi, fname, arguments);
            return psi;
        }

        public static (string, int) LookupFileAtLine(int lineNumber, bool relative = false)
        {
            if (lineNumber > Lines.Count - 1)
            {
                return (null, 0);
            }
            var split = Lines[lineNumber].Split(new[] { ':', '-' });
            var resLineNum = 0;
            if (split.Length > 1)
            {
                Int32.TryParse(split[0], out resLineNum);
            }

            for (var idx = lineNumber; idx >= 0; idx--)
            {
                var linetext = Lines[idx];
                if (linetext.Length == 0)
                {
                    continue;
                }
                if (rgLineRegex.IsMatch(linetext))
                {
                    continue;
                }
                if (linetext == "--")
                {
                    continue;
                }
                return (relative ? Lines[idx] : Path.Combine(WorkDir, Lines[idx]), resLineNum);
            }
            return (null, 0);
        }

        public static void KillSearch()
        {
            if (CurrentSearchProcess != null)
            {
                CurrentSearchProcess.CancelOutputRead();
            }
        }

        public static string GetVersion()
        {
            var appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var versionFile = Path.Combine(appDir, "version.txt");
            if (File.Exists(versionFile))
            {
                return File.ReadAllText(versionFile).Trim();
            }
            return "0.0.0";
        }

    }

    public static class IListExtensions
    {
        public static void RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
