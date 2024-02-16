namespace RoughGrep
{
    partial class FlagsForm
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
            this.lbAvailableFlags = new System.Windows.Forms.CheckedListBox();
            this.inpRenderedFlags = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lbAvailableFlags
            // 
            this.lbAvailableFlags.FormattingEnabled = true;
            this.lbAvailableFlags.Location = new System.Drawing.Point(12, 45);
            this.lbAvailableFlags.Name = "lbAvailableFlags";
            this.lbAvailableFlags.Size = new System.Drawing.Size(776, 395);
            this.lbAvailableFlags.TabIndex = 0;
            // 
            // inpRenderedFlags
            // 
            this.inpRenderedFlags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inpRenderedFlags.Location = new System.Drawing.Point(12, 12);
            this.inpRenderedFlags.Name = "inpRenderedFlags";
            this.inpRenderedFlags.Size = new System.Drawing.Size(776, 22);
            this.inpRenderedFlags.TabIndex = 1;
            // 
            // FlagsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.inpRenderedFlags);
            this.Controls.Add(this.lbAvailableFlags);
            this.Name = "FlagsForm";
            this.Text = "Command line flags";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox lbAvailableFlags;
        private System.Windows.Forms.TextBox inpRenderedFlags;
    }
}