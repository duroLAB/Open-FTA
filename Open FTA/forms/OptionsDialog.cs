using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class OptionItem<T>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Image Icon { get; set; }
    public T Value { get; set; }
}

namespace Open_FTA.forms
{

    public partial class OptionsDialog<T> : Form
    {
        public T SelectedValue { get; private set; }
        private readonly List<OptionItem<T>> _options;

        public OptionsDialog(string title, List<OptionItem<T>> options)
        {
            _options = options;

            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;

            this.Height = 220; // pevná výška ako pôvodný dialóg
            this.Width = 40 + _options.Count * 200; // šírka podľa počtu možností + rezerva

            this.KeyDown += OptionsDialog_KeyDown;

            GenerateUI();
        }

        private void GenerateUI()
        {
            int panelWidth = 180;
            int panelHeight = 140;
            int spacing = 20;
            int topOffset = 30;
            int leftOffset = 20;

            for (int i = 0; i < _options.Count; i++)
            {
                var p = CreateOptionPanel(_options[i]);
                p.Location = new Point(leftOffset + i * (panelWidth + spacing), topOffset);
                this.Controls.Add(p);
            }
        }

        private Panel CreateOptionPanel(OptionItem<T> opt)
        {
            var panel = new Panel
            {
                Size = new Size(180, 140),
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            // Ikona
            var pic = new PictureBox
            {
                Image = opt.Icon,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(10, 10),
                Size = new Size(40, 40)
            };

            // Názov
            var lblTitle = new Label
            {
                Text = opt.Title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(60, 10),
                AutoSize = true
            };

            // Popis
            var lblDesc = new Label
            {
                Text = opt.Description,
                Font = new Font("Segoe UI", 9),
                Location = new Point(60, 35),
                Size = new Size(110, 80),
                AutoEllipsis = true
            };

            panel.Controls.Add(pic);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblDesc);

            // Hover funkcia pre celý panel vrátane všetkých detí
            ApplyHover(panel, pic, lblTitle, lblDesc);

            // Klik na celom paneli
            panel.Click += (s, e) => SelectOption(opt);
            foreach (Control c in new Control[] { pic, lblTitle, lblDesc })
                c.Click += (s, e) => SelectOption(opt);

            return panel;
        }

        private void ApplyHover(Panel panel, params Control[] children)
        {
            void SetHover(object s, EventArgs e) => panel.BackColor = Color.LightGray;
            void RemoveHover(object s, EventArgs e)
            {
                if (!panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position)))
                    panel.BackColor = Color.WhiteSmoke;
            }

            panel.MouseEnter += SetHover;
            panel.MouseLeave += RemoveHover;

            foreach (var c in children)
            {
                c.MouseEnter += SetHover;
                c.MouseLeave += RemoveHover;
            }
        }

        private void SelectOption(OptionItem<T> opt)
        {
            SelectedValue = opt.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OptionsDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }






}
