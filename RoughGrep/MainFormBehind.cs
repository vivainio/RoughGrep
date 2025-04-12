using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace RoughGrep
{
    public class MainFormBehind
    {
        private readonly MainFormUi Ui;

        FullPreviewForm previewForm;

        public MainFormBehind(MainFormUi ui)
        {
            void SetupScintilla()
            {
                var scintilla = SciUtil.CreateScintilla();
                scintilla.UpdateUI += (o, e) =>
                {
                    if (e.Change == UpdateChange.Selection)
                    {
                        UpdateStatusBar();
                    }
                };
                //sci.ReadOnly = true;
                ui.resultBox = scintilla;
                ui.form.Controls.Add(ui.resultBox);
                ui.resultBox.Dock = DockStyle.Fill;
                ui.searchTextBox.Select();
                ui.tableLayout.Controls.Add(ui.resultBox, 0, 1);
            }

            this.Ui = ui;
            SetupScintilla();
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
                HandleKeyDownOnResults(e, ui.resultBox.CurrentLine);
            };
            ui.resultBox.KeyPress += (o, e) =>
            {
                // prevent PLING sound
                e.Handled = true;
            };
            if (Logic.RipGrepExecutable == null)
            {
                SciUtil.SetAllText(ui.resultBox, Logic.RgNotFoundError);
            }
            else
            {
                SciUtil.SetAllText(ui.resultBox, Logic.Tutorial.Value);
            }

            ui.dirSelector.DataSource = Logic.DirHistory;
            ui.searchTextBox.DataSource = Logic.SearchHistory;

            ui.dirSelector.Text = Logic.WorkDir;
            ui.btnAbort.Click += (o, e) =>
            {
                ui.btnAbort.Visible = false;
                Logic.KillSearch();
            };

            ui.btnCls.Click += (o, e) =>
            {
                KillAllInstances();
            };

            ui.statusLabelCurrentArgs.Click += (o, e) =>
            {
                ShowFlags();
            };
            LiveSearchEvents(ui);

            if (Logic.InitialSearchString != null)
            {
                ui.searchTextBox.Text = Logic.InitialSearchString;
                Logic.InitialSearchString = null;
                Logic.StartSearch(ui);
                SetFocusToResults();
            }
            ui.helpLink.Click += (o, e) =>
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            "https://github.com/vivainio/RoughGrep/blob/master/README.md#usage",
                        UseShellExecute = true,
                    }
                );
            };

            ui.toolTip.SetToolTip(ui.btnAbort, "Stop Searching");
            ui.toolTip.SetToolTip(ui.btnCls, "Close Other Windows");
            ui.toolTip.SetToolTip(ui.dirSelector, "Directory");
            ui.toolTip.SetToolTip(ui.searchControl.btnNext, "Next");
            ui.toolTip.SetToolTip(ui.searchControl.btnPrev, "Prev");
            ui.toolTip.SetToolTip(ui.searchControl.searchTextBox, "Search Within Results");
            UpdateStatusBar();
        }

        private void SetFocusToResults()
        {
            // without delay, focus gets stuck in search box
            Task.Delay(1)
                .ContinueWith(t =>
                {
                    Action a = () => Ui.resultBox.Focus();
                    Ui.resultBox.Invoke(a);
                });
        }

        private void KillAllInstances()
        {
            Logic.KillOtherInstancesOfProcess();
        }

        private FullPreviewForm Previewer()
        {
            if (previewForm == null)
            {
                previewForm = new FullPreviewForm(Ui);
                FormsUtil.FindVisiblePlaceForNewForm(Ui.form, previewForm);
            }
            return previewForm;
        }

        private readonly Lazy<FullPreviewForm> Notepad = new Lazy<FullPreviewForm>(() =>
        {
            var np = new FullPreviewForm(null);
            np.Text = "RoughGrep Notes";
            return np;
        });

        FlagsForm flagsForm = null;

        private void ShowFlags()
        {
            if (flagsForm == null)
            {
                flagsForm = new FlagsForm(this);
            }
            flagsForm.Show();
            flagsForm.BringToFront();
        }

        public void UpdateStatusBar()
        {
            var (file, line) = Logic.LookupFileAtLine(Ui.resultBox.CurrentLine, relative: true);
            Ui.statusLabel.Text = $"{file} +{line}";
            Ui.statusLabelCurrentArgs.Text = string.IsNullOrEmpty(Logic.RgExtraArgs)
                ? "No extra arguments"
                : Logic.RgExtraArgs;
        }

        private void LiveSearchEvents(MainFormUi ui)
        {
            var ctrl = ui.searchControl;
            var rb = ui.resultBox;
            ctrl.searchTextBox.TextChanged += (o, e) =>
                SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text);
            ctrl.btnNext.Click += (o, e) => SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text);
            ctrl.btnPrev.Click += (o, e) =>
                SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text, reverse: true);
            ctrl.searchTextBox.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (e.Shift)
                    {
                        SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text, reverse: true);
                    }
                    else
                    {
                        SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text);
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            };
        }

        private static void SearchStartEvent(MainFormUi ui, KeyEventArgs e)
        {
            if (Logic.CurrentSearchProcess != null)
            {
                Logic.KillSearch();
            }
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
                case Keys.N:
                {
                    CreateNote();
                    break;
                }
                case Keys.G:
                {
                    var (file, lineNum) = Logic.LookupFileAtLine(line);
                    if (file != null)
                    {
                        GitLog(file);
                    }
                    break;
                }
                case Keys.B:
                {
                    var (file, lineNum) = Logic.LookupFileAtLine(line);
                    if (file != null)
                    {
                        GitBlame(file, lineNum);
                    }
                    break;
                }
                case Keys.L:
                {
                    var (file, lineNum) = Logic.LookupFileAtLine(line);
                    if (file != null)
                    {
                        GitLog(file);
                    }
                    break;
                }

                case Keys.R:
                {
                    var (file, lineNum) = Logic.LookupFileAtLine(line);
                    RunExternal(file, lineNum);
                    break;
                }
                case Keys.D:
                {
                    var (file, lineNum) = Logic.LookupFileAtLine(line);
                    OpenContainingDirectory(file);
                    break;
                }
                case Keys.F:
                {
                    Ui.searchControl.searchTextBox.Focus();
                    break;
                }
                case Keys.F12:
                {
                    var selected = SciUtil.GetSelectionOrWordOnPosition(Ui.resultBox);
                    Ui.searchTextBox.Text = selected;
                    Ui.form.BringToFront();
                    Logic.StartSearch(Ui);
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

        private void OpenContainingDirectory(string file)
        {
            Process.Start(Path.GetDirectoryName(file));
        }

        private void GitLog(string file)
        {
            RunCommandAndPreviewOutput("git", "log -p " + file, "git log -p " + file, 0);
        }

        private void GitBlame(string file, int line)
        {
            RunCommandAndPreviewOutput(
                "git",
                "blame --date=short " + file,
                "git blame " + file,
                line
            );
        }

        private void RunCommandAndPreviewOutput(string command, string args, string title, int line)
        {
            var psi = Logic.CreateStartInfo(command, args);
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            var p = Process.Start(psi);
            // read all output
            var output = p.StandardOutput.ReadToEnd();
            var err = p.StandardError.ReadToEnd();
            PreviewText(output, line, title);
        }

        private void RunExternal(string file, int lineNum)
        {
            Logic.RunExternal(file, lineNum);
        }

        private void CreateNote()
        {
            var sci = Ui.resultBox;
            var np = Notepad.Value;
            var line = sci.CurrentLine;
            var file = Logic.LookupFileAtLine(line, relative: true);
            var selected = sci.SelectedText;
            if (selected.Length == 0)
            {
                selected = sci.Lines[line].Text;
            }

            np.scintilla.AppendText($"> {file}\r\n{selected}");
            np.Show();
            FormsUtil.BringFormToFront(np, Ui.form);
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
                LaunchEditorWithArgs($"{dir} -g \"{file}\":{lineNum}");
            }
        }

        void PreviewFile(string path, int linenum)
        {
            var text = File.ReadAllText(path);
            PreviewText(text, linenum, path);
        }

        void PreviewText(string text, int linenum, string title)
        {
            var fp = Previewer();
            SciUtil.SetAllText(fp.scintilla, text);
            fp.Text = title;

            var pos = fp.scintilla.Lines[linenum - 1].Position;
            fp.scintilla.GotoPosition(pos);
            SciUtil.RevealLine(fp.scintilla, linenum - 1);

            int found = SciUtil.SearchAndMove(fp.scintilla, Ui.searchTextBox.Text);
            // simple find didn't find it - so let's highlight the whole line
            if (found == -1)
            {
                fp.scintilla.SelectionStart = pos;
                fp.scintilla.SelectionEnd = fp.scintilla.Lines[linenum].Position;
            }

            fp.Show();
            FormsUtil.BringFormToFront(fp, Ui.form);
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
            LaunchEditorWithArgs($"-g \"{file}\":{lineNum}");
        }
    }
}
