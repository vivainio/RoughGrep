using System.Windows.Forms;
using ScintillaNET;

namespace RoughGrep
{
    public class MainFormUi
    {
        public ComboBox searchTextBox;
        public Scintilla resultBox;
        public ComboBox dirSelector;
        public MainForm form;
        public SearchControl searchControl;
        public Button btnAbort;
        public TableLayoutPanel tableLayout;
        public ToolStripStatusLabel statusLabel;
        public ToolStripStatusLabel statusLabelCurrentArgs;
        public Button btnCls;
        public ToolTip toolTip;
        internal ToolStripStatusLabel helpLink;
    }
}
