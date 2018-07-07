namespace RoughGrep
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.searchTextBox = new System.Windows.Forms.ComboBox();
            this.dirSelector = new System.Windows.Forms.ComboBox();
            this.resultBox = new System.Windows.Forms.RichTextBox();
            this.previewBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.searchControl = new RoughGrep.SearchControl();
            this.btnAbort = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(3, 3);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(269, 24);
            this.searchTextBox.TabIndex = 0;
            // 
            // dirSelector
            // 
            this.dirSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dirSelector.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.dirSelector.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.dirSelector.FormattingEnabled = true;
            this.dirSelector.Location = new System.Drawing.Point(604, 3);
            this.dirSelector.Name = "dirSelector";
            this.dirSelector.Size = new System.Drawing.Size(418, 24);
            this.dirSelector.TabIndex = 1;
            // 
            // resultBox
            // 
            this.resultBox.BackColor = System.Drawing.SystemColors.Window;
            this.resultBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.resultBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultBox.HideSelection = false;
            this.resultBox.Location = new System.Drawing.Point(3, 32);
            this.resultBox.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.resultBox.MaxLength = 200000;
            this.resultBox.Name = "resultBox";
            this.resultBox.ReadOnly = true;
            this.resultBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.resultBox.Size = new System.Drawing.Size(1022, 396);
            this.resultBox.TabIndex = 0;
            this.resultBox.Text = "";
            this.resultBox.WordWrap = false;
            // 
            // previewBox
            // 
            this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewBox.Location = new System.Drawing.Point(0, 428);
            this.previewBox.Margin = new System.Windows.Forms.Padding(0);
            this.previewBox.Multiline = true;
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(1025, 79);
            this.previewBox.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.previewBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.resultBox, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 83.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1025, 528);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.btnAbort);
            this.panel1.Controls.Add(this.searchControl);
            this.panel1.Controls.Add(this.dirSelector);
            this.panel1.Controls.Add(this.searchTextBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1025, 32);
            this.panel1.TabIndex = 3;
            // 
            // searchControl
            // 
            this.searchControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchControl.Location = new System.Drawing.Point(424, 0);
            this.searchControl.Name = "searchControl";
            this.searchControl.Size = new System.Drawing.Size(174, 29);
            this.searchControl.TabIndex = 2;
            // 
            // button1
            // 
            this.btnAbort.Location = new System.Drawing.Point(278, 3);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(18, 23);
            this.btnAbort.TabIndex = 3;
            this.btnAbort.Text = "x";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1025, 528);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "RoughGrep";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox searchTextBox;
        //private System.Windows.Forms.TextBox resultBox;
        private System.Windows.Forms.RichTextBox resultBox;

        private System.Windows.Forms.TextBox previewBox;
        private System.Windows.Forms.ComboBox dirSelector;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private SearchControl searchControl;
        private System.Windows.Forms.Button btnAbort;
    }
}

