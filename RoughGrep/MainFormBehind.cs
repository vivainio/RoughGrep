using ScintillaNET;
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

        FullPreviewForm previewForm;
        FullPreviewForm notepad;

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
            SciUtil.SetAllText(ui.resultBox, "Tutorial: space=preview, enter=edit, p=edit parent project dir, n=take note, g=git history");

            ui.dirSelector.DataSource = Logic.DirHistory;
            ui.searchTextBox.DataSource = Logic.SearchHistory;
            ui.rgArgsComboBox.Items.AddRange(new[]
            {
                Logic.RgExtraArgs,
                "--files",
                "-m 5 --smart-case",
                "-M 1000",
                "-g *.cs -g *.csproj",
                "--no-ignore",
                "--context 2"

            });
            ui.rgArgsComboBox.TextChanged += (o, e) =>
            {
                Logic.RgExtraArgs = ui.rgArgsComboBox.Text;
                UpdateStatusBar();
            };

            ui.dirSelector.Text = Logic.WorkDir;
            ui.btnAbort.Click += (o, e) =>
            {
                ui.btnAbort.Visible = false;
                Logic.KillSearch();
            };
            LiveSearchEvents(ui);

            if (Logic.InitialSearchString != null)
            {
                ui.searchTextBox.Text = Logic.InitialSearchString;
                Logic.InitialSearchString = null;
                Logic.StartSearch(ui);
            }
            UpdateStatusBar();
        }

        private FullPreviewForm Previewer()
        {
            if (previewForm == null)
            {
                previewForm = new FullPreviewForm();
                FormsUtil.FindVisiblePlaceForNewForm(Ui.form, previewForm);
            }
            return previewForm;

        }

        private readonly Lazy<FullPreviewForm> Notepad = new Lazy<FullPreviewForm>(() =>
        {
            var np = new FullPreviewForm();
            np.Text = "RoughGrep Notes";
            return np;
        });

        private void UpdateStatusBar()
        {
            var (file, line) = Logic.LookupFileAtLine(Ui.resultBox.CurrentLine, relative: true);
            Ui.statusLabel.Text = $"{file} +{line}";
            Ui.statusLabelCurrentArgs.Text = Logic.RgExtraArgs;
        }
        private void LiveSearchEvents(MainFormUi ui)
        {
            var ctrl = ui.searchControl;
            var rb = ui.resultBox;
            ctrl.searchTextBox.TextChanged += (o, e) => SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text);
            ctrl.btnNext.Click += (o, e) => SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text);
            ctrl.btnPrev.Click += (o, e) => SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text, reverse: true);
            ctrl.searchTextBox.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (e.Shift)
                    {
                        SciUtil.SearchAndMove(rb, ctrl.searchTextBox.Text, reverse: true);
                    } else
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

        private void GitLog(string file)
        {
            var psi = Logic.CreateStartInfo("gitk", file);
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;
            Process.Start(psi);
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
                LaunchEditorWithArgs($"{dir} -g {file}:{lineNum}");
            }
        }

        void PreviewFile(string path, int linenum)
        {
            var text = File.ReadAllText(path);
            var fp = Previewer();
            SciUtil.SetAllText(fp.scintilla, text);
            fp.Text = path;

            var pos = fp.scintilla.Lines[linenum-1].Position;
            SciUtil.RevealLine(fp.scintilla, linenum - 1);
            fp.scintilla.GotoPosition(pos);
            SciUtil.SearchAndMove(fp.scintilla, Ui.searchTextBox.Text);
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
            LaunchEditorWithArgs($"-g {file}:{lineNum}");
        }
    }
}
