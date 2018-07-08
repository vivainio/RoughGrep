using System;
using System.Windows.Forms;
using TrivialBehind;

namespace RoughGrep
{

    public partial class MainForm : Form
    {
        IDisposable behindDisposer;
        public MainForm()
        {
            InitializeComponent();
            
            behindDisposer = TrivialBehinds.CreateForUi(new MainFormUi
            {
                searchControl = searchControl,
                form = this,
                dirSelector = dirSelector,
                searchTextBox = searchTextBox,
                btnAbort = btnAbort,
                tableLayout = tableLayoutPanel1,
                statusLabel = statusLabel1,
                rgArgsComboBox = rgArgsComboBox
            });
            this.Deactivate += (o, e) => behindDisposer.Dispose();
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
