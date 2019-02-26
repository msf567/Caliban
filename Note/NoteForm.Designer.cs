namespace Note
{
    partial class NoteForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NoteForm));
            this.noteText = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // noteText
            // 
            this.noteText.BackColor = System.Drawing.Color.Black;
            this.noteText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.noteText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.noteText.Font = new System.Drawing.Font("Perfect DOS VGA 437", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noteText.ForeColor = System.Drawing.Color.White;
            this.noteText.Location = new System.Drawing.Point(0, 0);
            this.noteText.Margin = new System.Windows.Forms.Padding(10);
            this.noteText.Name = "noteText";
            this.noteText.ReadOnly = true;
            this.noteText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.noteText.Size = new System.Drawing.Size(382, 614);
            this.noteText.TabIndex = 0;
            this.noteText.Text = resources.GetString("noteText.Text");
            this.noteText.WordWrap = false;
            this.noteText.TextChanged += new System.EventHandler(this.noteText_TextChanged);
            // 
            // NoteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(382, 614);
            this.Controls.Add(this.noteText);
            this.Font = new System.Drawing.Font("Perfect DOS VGA 437 Win", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(255)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NoteForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NoteForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NoteForm_FormClosing);
            this.Load += new System.EventHandler(this.NoteForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox noteText;
    }
}