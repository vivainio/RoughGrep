using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace RoughGrep
{
    public partial class FullPreviewForm : Form
    {
        public FullPreviewForm(MainFormUi ui)
        {
            InitializeComponent();
            _mainUi = ui;
            this.KeyPreview = true;

            scintilla = SciUtil.CreateScintilla();
            Controls.Add(scintilla);
            scintilla.Dock = DockStyle.Fill;
            scintilla.Margins[0].Width = 48;
            FormClosing += (o, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    Hide();
                }
            };
        }

        public Scintilla scintilla;
        private readonly MainFormUi _mainUi;

        private void FullPreviewForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12 && _mainUi != null)
            {
                var selected = SciUtil.GetSelectionOrWordOnPosition(scintilla);
                _mainUi.searchTextBox.Text = selected;
                _mainUi.form.BringToFront();
                Logic.StartSearch(_mainUi);
            }
        }
    }
}
