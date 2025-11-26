using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using System.Drawing;
    using System.Windows.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Open_FTA.forms
{
    using Open_FTA.Properties;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class ResultDialog : Form
    {
        private Label lblTitle;
        private Label lblMessage;
        private Button btnOK;
        private PictureBox iconPicture;
        private int borderThickness = 2;

        public ResultDialog(string title, string message, Image icon = null)
        {
            // --- Form settings ---
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.Width = 450;
            this.Height = 220;

            // --- HEADER PANEL ---
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(33, 150, 243) // Material Blue
            };

            // --- ICON ---
            if (icon != null)
            {
                iconPicture = new PictureBox
                {
                    Image = icon,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(32, 32),
                    Location = new Point(10, 14),
                    BackColor = headerPanel.BackColor
                };
                headerPanel.Controls.Add(iconPicture);
            }

            // --- TITLE LABEL ---
            lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 14),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = this.Width - 60,  // necháme miesto pre ikonu
                Location = new Point(50, 0), // odsadenie od ikony
                Height = 60
            };

            headerPanel.Controls.Add(lblTitle);
            this.Controls.Add(headerPanel);

            this.Controls.Add(headerPanel);

            // --- MESSAGE ---
            lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 12),
                AutoSize = false,
                Width = this.Width - 40,
                Height = 60,
                Location = new Point(20, 80)
            };
            this.Controls.Add(lblMessage);

            // --- OK BUTTON ---
            btnOK = new Button
            {
                Text = "OK",
                Width = 120,
                Height = 40,
                Font = new Font("Segoe UI Semibold", 11),
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Location = new Point(this.Width - btnOK.Width - 30, this.Height - 60);
            btnOK.Click += (s, e) => this.Close();
            this.Controls.Add(btnOK);

            // --- BORDER ---
            this.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), borderThickness))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };

            // --- COPY RESULT ---
            lblMessage.DoubleClick += (s, e) =>
            {
                Clipboard.SetText(lblMessage.Text);
                MessageBox.Show("Result copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        public static void ShowResult(string title, string message, Image icon = null)
        {
            using (var dlg = new ResultDialog(title, message, icon))
            {
                dlg.ShowDialog();
            }
        }
    }




}