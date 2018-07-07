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
                //resultBox = resultBox,
                previewBox = previewBox,
                searchTextBox = searchTextBox,
                btnAbort = btnAbort,
                tableLayout = tableLayoutPanel1

            });
            this.Deactivate += (o, e) => behindDisposer.Dispose();
        }
    }
}
