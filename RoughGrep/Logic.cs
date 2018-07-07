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
using System.Threading.Tasks;
using System.Windows.Forms;
using TrivialBehind;

namespace RoughGrep
{

    class RichTextRenderer
    {
        private readonly Scintilla rt;
        private readonly Font normFont;
        private readonly Font italicFont;

        public RichTextRenderer(Scintilla rt)
        {
            this.rt = rt;
            this.normFont = new Font(rt.Font, FontStyle.Regular);
            this.italicFont = new Font(rt.Font, FontStyle.Italic);
           
        }

        public RichTextRenderer Feed(string s)
        {
            rt.AppendText(s);
            return this;
        }
        public RichTextRenderer Lf()
        {
            rt.AppendText("\r\n");
            return this;
        }
        public RichTextRenderer Bullet(string s)
        {
            this.Feed(s + "\r\n");
            return this;
        }

    }
    public static class Logic
    {
        public static string WorkDir = null;
        public static string RgExtraArgs = "";
        static List<string> Lines = new List<string>();
        public static BindingList<string> DirHistory = new BindingList<string>();
        public static BindingList<string> SearchHistory = new BindingList<string>();
        public static Process CurrentSearchProcess = null;
        public static void InitApp()
        {
            var extraArgs = string.Join(" ", Environment.GetCommandLineArgs().Skip(1));
            Logic.RgExtraArgs = extraArgs == "" ? "-i " : extraArgs + " ";
            Logic.WorkDir = Directory.GetCurrentDirectory();
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
                // special handling for --files, interpret search text as -g glob
                return $"{RgExtraArgs} -g {text}";
            }
            return $"{RgExtraArgs}--heading -m 1000 -M 300 -n \"{text}\"";
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
            AssignStartInfo(p.StartInfo, "rg.exe", args);
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            ui.resultBox.ClearAll();
            Lines.Clear();
            p.EnableRaisingEvents = true;
            var toFlush = new List<string>();
            var flushlock = new Object();
            var render = new RichTextRenderer(ui.resultBox);
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
                        var parts = line.Split(new[] { ":" }, 2, StringSplitOptions.None);
                        sb.Append(" ").Append(parts[1].Trim()).Append("\r\n");
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
                render.Feed(sb.ToString());
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
            Action moveToStart = () =>
            {
                ui.resultBox.SelectionStart = 0;
                ui.resultBox.SelectionEnd = 0;
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
                ui.resultBox.Invoke(moveToStart);
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
            var split = Lines[lineNumber].Split(':');
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
