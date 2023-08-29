using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepIdentifier
{
    internal partial class DynamicProgressBar : Form
    {
        private ProgressBar progressBar;
        private Label titleLabel;
        private Label progressLabel;

        public DynamicProgressBar()
        {
            InitializeProgressBar();

        }

        private void InitializeProgressBar()
        {
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Width = 400;
            Height = 200;

            titleLabel = new Label();
            titleLabel.Text = "Progress";
            titleLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Top;
            Controls.Add(titleLabel);

            progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Height = Height / 4;
            progressBar.Width = Width - 100;
            progressBar.Location = new Point((Width - progressBar.Width) / 2, titleLabel.Bottom + 10);
            Controls.Add(progressBar);

            progressLabel = new Label();
            progressLabel.Text = "0%";
            progressLabel.TextAlign = ContentAlignment.MiddleCenter;
            progressLabel.Dock = DockStyle.Bottom;
            Controls.Add(progressLabel);
        }

        public void UpdateProgress(int progressValue)
        {
            progressBar.Value = progressValue;
            progressLabel.Text = $"Progress: {progressValue}%";
            if (progressValue == progressBar.Maximum)
            {
                Close();
                Dispose();
            }
        }

        public void SetMinAndMax(int minumum, int maximum)
        {
            progressBar.Minimum = minumum;
            progressBar.Maximum = maximum;
        }
    }
}
