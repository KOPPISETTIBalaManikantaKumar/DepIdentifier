﻿namespace DepIdentifier
{
    partial class HelpForm
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
            HelpRichTextBox = new RichTextBox();
            SuspendLayout();
            // 
            // HelpRichTextBox
            // 
            HelpRichTextBox.Location = new Point(1, 2);
            HelpRichTextBox.Name = "HelpRichTextBox";
            HelpRichTextBox.Size = new Size(798, 447);
            HelpRichTextBox.TabIndex = 0;
            HelpRichTextBox.Text = "";
            // 
            // HelpForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(HelpRichTextBox);
            Name = "HelpForm";
            Text = "HelpForm";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox HelpRichTextBox;
    }
}