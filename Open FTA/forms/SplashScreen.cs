using System;
using System.Drawing;
using System.Windows.Forms;


namespace Open_FTA.forms
{
    public partial class SplashScreen : Form
    {
        private System.Windows.Forms.Timer fadeTimer;
        private System.Windows.Forms.Timer progressTimer;
        private ProgressBar progressBar;
        private int progressValue = 0;

        public bool IsReadyToClose { get; private set; }

        public SplashScreen(Screen targetScreen)
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.Manual;

            var centerX = targetScreen.WorkingArea.Left +
                          (targetScreen.WorkingArea.Width - this.Width) / 2;
            var centerY = targetScreen.WorkingArea.Top +
                          (targetScreen.WorkingArea.Height - this.Height) / 2;

            this.Location = new Point(centerX, centerY);

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.TopMost = true;
            this.Opacity = 0;  

             
            var picture = new PictureBox
            {
                Image = Properties.Resources.splash, // pridaj svoje logo do Resources
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(350, 300),
                Location = new Point(1, 1)
            };
            this.Controls.Add(picture);

           
            var label = new Label
            {
                Text = "Loading...",
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                Location = new Point(60, 130)
            };
            this.Controls.Add(label);
 
            progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Location = new Point(30, 290),
                Width = 270,
                Height = 15,
                Value = 0
            };
            this.Controls.Add(progressBar);

            this.ClientSize = new Size(350, 330);

            // Fade-in efekt
            fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 50;
            fadeTimer.Tick += FadeIn;
            fadeTimer.Start();

            // Simulovaný progress
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 10;
            progressTimer.Tick += ProgressTick;
            progressTimer.Start();
        }

        private void FadeIn(object sender, EventArgs e)
        {
            if (this.Opacity < 1)
                this.Opacity += 0.05;
            else
                fadeTimer.Stop();
        }

        private void ProgressTick(object sender, EventArgs e)
        {
            progressValue += 2;
            if (progressValue > 100)
            {
                progressTimer.Stop();
                IsReadyToClose = true;
                StartFadeOut();
            }
            else
            {
                progressBar.Value = progressValue;
            }
        }

        private void StartFadeOut()
        {
            fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 50;
            fadeTimer.Tick += FadeOut;
            fadeTimer.Start();
        }

        private void FadeOut(object sender, EventArgs e)
        {
            if (this.Opacity > 0)
                this.Opacity -= 0.05;
            else
            {
                fadeTimer.Stop();
                this.Close();
            }
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {

        }
    }
}
