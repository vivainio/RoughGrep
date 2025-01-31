using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoughGrep
{
    public partial class FlagsForm : Form
    {
        private MainFormBehind ui;

        public FlagsForm(MainFormBehind ui)
        {
            this.ui = ui;
            InitializeComponent();
            lbAvailableFlags.CheckOnClick = true;
            lbAvailableFlags.Items.Clear();
            lbAvailableFlags.Items.AddRange(Logic.DefaultFlags.ToArray());
            for (int i = 0; i < lbAvailableFlags.Items.Count; i++)
            {
                lbAvailableFlags.SetItemChecked(i, true);
            }
            lbAvailableFlags.Items.AddRange(Logic.AvailableFlags.ToArray());

            lbAvailableFlags.ItemCheck += LbAvailableFlags_ItemCheck;
            inpRenderedFlags.TextChanged += InpRenderedFlags_TextChanged;
            this.FormClosing += FlagsForm_FormClosing;
        }

        private void FlagsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void InpRenderedFlags_TextChanged(object sender, EventArgs e)
        {
            Logic.RgExtraArgs = inpRenderedFlags.Text;
            ui.UpdateStatusBar();
        }

        private void RenderCurrentSelections()
        {
            var rendered = new StringBuilder();
            foreach (var item in lbAvailableFlags.CheckedItems)
            {
                var s = (string)item;
                rendered.Append(s);
                rendered.Append(" ");
            }
            inpRenderedFlags.Text = rendered.ToString();
        }

        private void LbAvailableFlags_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // checkedlistbox sucks ass
            Task.Delay(100)
                .ContinueWith(
                    (t) =>
                    {
                        this.Invoke(
                            new Action(() =>
                            {
                                RenderCurrentSelections();
                            })
                        );
                    }
                );
        }
    }
}
