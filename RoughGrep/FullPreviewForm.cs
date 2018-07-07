using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoughGrep
{
    public partial class FullPreviewForm : Form
    {
        public FullPreviewForm()
        {
            InitializeComponent();
            scintilla = SciUtil.CreateScintilla();
            Controls.Add(scintilla);
            scintilla.Dock = DockStyle.Fill;
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
    }
}
