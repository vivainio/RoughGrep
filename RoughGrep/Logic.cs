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
        private readonly RichTextBox rt;
        private readonly Font normFont;
        private readonly Font italicFont;

        public RichTextRenderer(RichTextBox rt)
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
        public RichTextRenderer Right(string s)
        {
            this.rt.SelectionAlignment = HorizontalAlignment.Right;
            this.Feed(s + "\r\n");
            this.rt.SelectionAlignment = HorizontalAlignment.Left;
            return this;
        }
        public RichTextRenderer WithFont(Font font, string s)
        {
            this.rt.SelectionFont = font;
            this.Feed(s);
            this.rt.SelectionFont = this.normFont;
            return this;
        }
        public RichTextRenderer Italic(string s) => WithFont(italicFont, s);
    }
    public static class Logic
    {
        public static string WorkDir = null;
        public static string RgExtraArgs = "";
        static List<string> Lines = new List<string>();
        public static BindingList<string> DirHistory = new BindingList<string>();
        public static BindingList<string> SearchHistory = new BindingList<string>();

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
                ui.previewBox.Text = $"Directory does not exist: '{WorkDir}'";
                return;
            }
            text = text.Replace("\"", "\\\"");
            var p = new Process();

            AssignStartInfo(p.StartInfo, "rg.exe", $"{RgExtraArgs}--heading -M 200 -n \"{text}\"");
            p.StartInfo.RedirectStandardOutput = true;
            ui.previewBox.Text = $"{p.StartInfo.Arguments} [{WorkDir}]";

            ui.resultBox.Clear();
            Lines.Clear();
            p.EnableRaisingEvents = true;
            Action updateRows = () => ui.resultBox.Lines = Lines.ToArray();
            var toFlush = new List<string>();
            var flushlock = new Object();


            var render = new RichTextRenderer(ui.resultBox);
            Action doFlush = () =>
            {
                var fl = toFlush;
                toFlush = new List<string>();
                foreach (var line in fl)
                {
                    if (line.Length == 0)
                    {
                        render.Lf();
                        continue;
                    }

                    if (char.IsDigit(line[0]))
                    {
                        var parts = line.Split(new[] { ":" }, 2,  StringSplitOptions.None);
                        render.Feed(parts[1].TrimStart() + "\r\n");
                    } else
                    {
                        render.Right(line);
                    }
                }
                //ui.resultBox.AppendText(string.Join("\r\n", fl) + "\r\n");
            };
            Action debouncedFlush = Debounce(100, doFlush);

            p.OutputDataReceived += (o,ev) =>            
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
            Action moveToStart = () => ui.resultBox.SelectionStart = 0;
            p.Exited += (o, ev) =>
            {
                ui.resultBox.Invoke(doFlush);
                ui.resultBox.Invoke(moveToStart);
            };
            p.Start();
            p.BeginOutputReadLine();
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

        public static (string, int) LookupFileAtLine(int lineNumber)
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
                return (Path.Combine(WorkDir, Lines[idx]), resLineNum);
            }
            return (null, 0);
        }
    }
}
