using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public static string RgExtraArgs = "";
        // if set, will trigger search for this on launch
        public static string InitialSearchString = null;
        static List<string> Lines = new List<string>();
        public static BindingList<string> DirHistory = new BindingList<string>();
        public static BindingList<string> SearchHistory = new BindingList<string>();
        public static Process CurrentSearchProcess = null;

        public static List<ExternalCommand> ExternalCommands = new List<ExternalCommand>();
        public static string Tutorial = "Tutorial: space=preview, enter=edit, p=edit parent project dir, d=containing dir, n=take note, g=git history, f=find in results";

        public static void InitApp()
        {
            var extraArgs = Environment.GetCommandLineArgs().Skip(1).ToList();
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

            Logic.RgExtraArgs = string.Join(" ", extraArgs);
            Logic.WorkDir = Directory.GetCurrentDirectory();
            var rc = ScriptRunner.FindScript();
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
                } else
                {
                    ;
                    // skipping;
                }
            };
        }

        private static string CreateArgsForRg(string text)
        {
            if (RgExtraArgs.StartsWith("--files"))
            {
                var globs = string.Join(" ", text.Split(' ').Select(t => $"-g {t}"));
                // special handling for --files, interpret search text as -g glob
                return $"{RgExtraArgs} {globs}";
            }

            var maxcount = RgExtraArgs.Contains("-m ") ?  "" :  "-m 1000";
            var maxlen = RgExtraArgs.Contains("-M ") ? "" : "-M 300";
            
            return $"{RgExtraArgs} {maxcount} {maxlen} --heading -n \"{text}\"";
        }
        public static void StartSearch(MainFormUi ui)
        {
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
            text = text.Replace("\"", "\\\"");
            var p = new Process();

            var args = CreateArgsForRg(text);
            ui.statusLabelCurrentArgs.Text = args;
            AssignStartInfo(p.StartInfo, "rg.exe", args);
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
            void RichFlush(IEnumerable<string> lines)
            {
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    
                    if (line.Length == 0)
                    {
                        sb.Append("\r\n");
                        continue;
                    }

                    if (char.IsDigit(line[0]))
                    {
                        var parts = line.Split(new[] { ":", "-" }, 2, StringSplitOptions.None);
                        // context
                        if (parts.Length == 1)
                        {
                            sb.Append(line + "\r\n");
                        } else
                        {
                            sb.Append(" ").Append(parts[1].Trim()).Append("\r\n");
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
                ui.resultBox.AppendText(sb.ToString());
            }

            Action doFlush = () =>
            {
                var fl = toFlush;
                toFlush = new List<string>();
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
            p.Exited += (o, ev) =>
            {
                CurrentSearchProcess = null;
                ui.btnAbort.Invoke(hideAbort);
                ui.resultBox.Invoke(doFlush);
                ui.resultBox.Invoke(searchReady);
            };
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            CurrentSearchProcess = p;
            ui.btnAbort.Visible = true;
            PrependIfNew(Logic.DirHistory, WorkDir);
            PrependIfNew(Logic.SearchHistory, text);
            ui.dirSelector.SelectedIndex = 0;
            ui.searchTextBox.SelectedIndex = 0;
        }
        static void PrependIfNew<T>(IList<T> coll, T entry ) where T: IComparable<T>
        {
            if (coll.Count > 0 && coll.ElementAt(0).CompareTo(entry) == 0)
            {
                return;
            }
            coll.Insert(0, entry);
        }

        internal static void RunExternal(string file, int lineNum)
        {
            var cmd = Logic.ExternalCommands.FirstOrDefault(c => Regex.IsMatch(file, c.Pattern));
            if (cmd == null)
            {
                return;
            }

            var arg = cmd.Runner.Arg.Replace("[[file]]", file).Replace("[[line]]", lineNum.ToString());

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

        public static (string, int) LookupFileAtLine(int lineNumber, bool relative=false)
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

            for (var idx = lineNumber;  idx >= 0; idx--)
            {
                var linetext = Lines[idx];
                if (linetext.Length == 0)
                {
                    continue;
                }            
                if (char.IsDigit(linetext[0]))
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

    }
}
