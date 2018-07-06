using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RoughGrep
{
    public class MainFormBehind
    {
        private readonly MainFormUi Ui;
        public MainFormBehind(MainFormUi ui)
        {

            this.Ui = ui;
            ui.searchTextBox.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SearchStartEvent(ui, e);
                }
            };
            ui.dirSelector.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SearchStartEvent(ui, e);
                }
            };

            ui.resultBox.KeyDown += (o, e) =>
            {
                var idx = ui.resultBox.GetFirstCharIndexOfCurrentLine();
                var line = ui.resultBox.GetLineFromCharIndex(idx);
                HandleKeyDownOnResults(e, line);
            };
            ui.resultBox.KeyPress += (o, e) =>
            {
                // prevent PLING sound
                e.Handled = true;
            };
            ui.previewBox.Text = "Tutorial: space=preview, enter=edit, p=edit parent project dir";

            ui.dirSelector.DataSource = Logic.DirHistory;
            ui.searchTextBox.DataSource = Logic.SearchHistory;
            ui.dirSelector.Text = Logic.WorkDir;
            ui.form.Load += (o, e) =>
            {
                ui.searchTextBox.Select();
            };
            LiveSearchEvents(ui);
            
        }

        private void LiveSearchEvents(MainFormUi ui)
        {
            var ctrl = ui.searchControl;

            void SearchForward() =>             
                ui.resultBox.Find(ctrl.searchTextBox.Text, ui.resultBox.SelectionStart + 1, RichTextBoxFinds.None);

            void SearchBack() => 
                ui.resultBox.Find(ctrl.searchTextBox.Text, 0, ui.resultBox.SelectionStart - 1, RichTextBoxFinds.Reverse);

            ctrl.searchTextBox.TextChanged += (o, e) =>
            {
                SearchForward();
            };
            /*
            {
                ui.resultBox.Find(ctrl.searchTextBox.Text);
            };
            */
            ctrl.btnNext.Click += (o, e) => SearchForward();
            ctrl.searchTextBox.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (e.Shift)
                    {
                        SearchBack();
                    } else
                    {
                        SearchForward();
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            };

            ctrl.btnPrev.Click += (o, e) => SearchBack();
        }

        private static void SearchStartEvent(MainFormUi ui, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
            Logic.StartSearch(ui);
        }

        internal void HandleKeyDownOnResults(KeyEventArgs e, int line)
        {
            var supress = true;


            switch (e.KeyCode)
            {
                case Keys.Space:
                    {
                        var (file, lineNum) = Logic.LookupFileAtLine(line);
                        if (file != null)
                        {
                            PreviewFile(file, lineNum);
                        }
                        break;

                    }
                case Keys.Enter:
                    {
                        var (file, lineNum) = Logic.LookupFileAtLine(line);
                        if (file != null)
                        {
                            EditFile(file, lineNum);
                        }
                        break;

                    }
                case Keys.P:
                    {
                        var (file, lineNum) = Logic.LookupFileAtLine(line);
                        if (file != null)
                        {
                            OpenProject(file, lineNum);
                        }
                        break;

                    }

                default:
                    {
                        supress = false;
                    }
                    break;
            }
            if (supress)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

        }

        private static string ScanParentsForFiles(string startPath, string[] globs)
        {
            var cur = startPath;
            while (true)
            {
                var parent = Directory.GetParent(cur);
                if (parent == null)
                {
                    return null;
                }
                cur = parent.FullName;
                foreach (var trie in globs)
                {
                    var files = Directory.GetFiles(cur, trie);
                    if (files.Length > 0)
                    {
                        return cur;
                    }
                }
            }
        }
        private void OpenProject(string file, int lineNum)
        {
            var tries = new[] { "package.json", "*.csproj", "*.fsproj", ".gitignore", "*.sln" };
            var dir = ScanParentsForFiles(file, tries);
            if (dir != null)
            {
                LaunchEditorWithArgs($"{dir} -g {file}:{lineNum}");
            }
        }

        void PreviewFile(string path, int linenum)
        {
            var lines = File.ReadLines(path).Skip(Math.Max(linenum - 2, 0)).Take(10);
            var asText = string.Join("\r\n", lines);
            asText = asText.Substring(0, Math.Min(5000, asText.Length));
            Ui.previewBox.Text = asText;
            Ui.previewBox.SelectionStart = 0;
            Ui.previewBox.SelectionLength = 1;
            Ui.previewBox.ScrollToCaret();
        }

        void LaunchEditorWithArgs(string args)
        {
            var psi = Logic.CreateStartInfo("code", args);
            psi.UseShellExecute = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi);
        }
        void EditFile(string file, int lineNum)
        {
            LaunchEditorWithArgs($"-g {file}:{lineNum}");
        }
    }
}
