using ScintillaNET;
using System.Windows.Forms;

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
        public Scintilla sci;
        public TableLayoutPanel tableLayout;
        public ToolStripStatusLabel statusLabel;
        public ToolStripComboBox rgArgsComboBox;
        public ToolStripStatusLabel statusLabelCurrentArgs;
        public Button btnCls;
    }
}
