namespace Open_FTA.forms
{
    public partial class ExportDialogForm : Form
    {
        public enum ExportOption { None, Bitmap, Metafile }
        public ExportOption SelectedOption { get; private set; } = ExportOption.None;

        public ExportDialogForm()
        {
            this.Text = "Export Options";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 220);
            this.BackColor = Color.White;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Panel pre Bitmap
            var panelBitmap = CreateOptionPanel(
                "Bitmap",
                "Raster image (.bmp/.png)\nBest for pixel-based graphics. Scaling may blur.",
                 Properties.Resources.Copy_24, // pridaj ikonu do Resources
                () => { SelectedOption = ExportOption.Bitmap; this.DialogResult = DialogResult.OK; },
                new Point(20, 30)
            );

            // Panel pre Metafile
            var panelMetafile = CreateOptionPanel(
                "Metafile",
                "Vector image (.emf/.wmf)\nPerfect for scaling without quality loss.",
                Properties.Resources.Add_BE_24, // pridaj ikonu do Resources
                () => { SelectedOption = ExportOption.Metafile; this.DialogResult = DialogResult.OK; },
                new Point(230, 30)
            );

            this.Controls.Add(panelBitmap);
            this.Controls.Add(panelMetafile);
        }


        private Panel CreateOptionPanel(string title, string description, Image icon, Action onClick, Point location)
        {
            var panel = new Panel
            {
                Size = new Size(180, 140),
                Location = location,
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            // Hover efekt
            panel.MouseEnter += (s, e) => panel.BackColor = Color.LightGray;
            panel.MouseLeave += (s, e) => panel.BackColor = Color.WhiteSmoke;

            panel.Click += (s, e) => onClick();

            // Ikona
            var pic = new PictureBox
            {
                Image = icon,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(10, 10),
                Size = new Size(40, 40)
            };

            // Názov
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(60, 10),
                AutoSize = true
            };

            // Popis
            var lblDesc = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 9),
                Location = new Point(60, 35),
                Size = new Size(110, 80),
                AutoEllipsis = true
            };

            // Klikateľnosť na celom paneli
            foreach (Control ctrl in new Control[] { pic, lblTitle, lblDesc })
                ctrl.Click += (s, e) => onClick();

            panel.Controls.Add(pic);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblDesc);

            return panel;
        }

        private void ExportDialogForm_Load(object sender, EventArgs e)
        {

        }
    }

}

