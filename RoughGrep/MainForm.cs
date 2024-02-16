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

            behindDisposer = TrivialBehinds.CreateBehind(
                this,
                new MainFormUi
                {
                    searchControl = searchControl,
                    form = this,
                    dirSelector = dirSelector,
                    searchTextBox = searchTextBox,
                    btnAbort = btnAbort,
                    btnCls = btnCls,
                    tableLayout = tableLayoutPanel1,
                    statusLabel = statusLabel1,
                    statusLabelCurrentArgs = statusLabelCurrentArgs
                }
            );
            this.Deactivate += (o, e) => behindDisposer.Dispose();
        }
    }
}
