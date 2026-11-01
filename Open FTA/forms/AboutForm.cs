using System.Diagnostics;
using Open_FTA.Properties;

namespace Open_FTA.forms
{
    /// <summary>Version, author, licence and links to the source code and to the thesis with the first version.</summary>
    internal sealed class AboutForm : Form
    {
        private const string RepositoryUrl = "https://github.com/duroLAB/Open-FTA";
        private const string LicenseUrl = "https://github.com/duroLAB/Open-FTA/blob/master/LICENSE";
        private const string ThesisUrl = "https://kis.cvt.stuba.sk/arl-stu/sk/detail-stu_us_cat-stuzp105108-Navrh-softveroveho-systemu-na-analyzu-stromu-poruch/";
        private const int TextWidth = 400;

        private static readonly Color GreyText = Color.FromArgb(82, 92, 105);

        public AboutForm()
        {
            Text = "About Open FTA";
            Icon = Resources.Main;
            Font = new Font("Segoe UI", 9f);
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var content = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(20, 18, 20, 12),
            };

            // header: icon, name, version
            var header = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 10) };
            header.Controls.Add(new PictureBox
            {
                Image = new Icon(Resources.Main, 48, 48).ToBitmap(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(48, 48),
                Margin = new Padding(0, 0, 12, 0),
            });
            var title = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = Padding.Empty };
            title.Controls.Add(new Label { Text = "Open FTA", AutoSize = true, Font = new Font("Segoe UI Semibold", 16f), Margin = Padding.Empty });
            title.Controls.Add(new Label
            {
                Text = $"Version {SaveTreeClass.CurrentBuildVersion}   ·   .NET {Environment.Version}",
                AutoSize = true, ForeColor = GreyText, Margin = Padding.Empty,
            });
            header.Controls.Add(title);
            content.Controls.Add(header);

            content.Controls.Add(Paragraph(
                "Fault tree analysis for the safety assessment in the chemical and process industry. " +
                "A teaching tool and a free, open-source alternative to commercial systems."));

            content.Controls.Add(Caption("Source code"));
            content.Controls.Add(Link("github.com/duroLAB/Open-FTA", RepositoryUrl));

            content.Controls.Add(Caption("Author"));
            content.Controls.Add(Paragraph("Juraj Labovský, Slovak University of Technology in Bratislava – concept and design", 6));
            content.Controls.Add(Paragraph("The first version was created within the diploma thesis of M. Doller, Design of a Software System for Fault Tree Analysis, supervised by J. Labovský.", 2));
            content.Controls.Add(Link("Thesis in the university library", ThesisUrl));

            content.Controls.Add(Caption("License"));
            content.Controls.Add(Paragraph("Copyright © 2025 Juraj Labovský", 2));
            content.Controls.Add(Link("MIT License", LicenseUrl));

            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true, MinimumSize = new Size(80, 0), Anchor = AnchorStyles.Right };
            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Width = TextWidth,
                Height = ok.PreferredSize.Height + 4,
                Margin = new Padding(0, 16, 0, 0),
            };
            buttons.Controls.Add(ok);
            content.Controls.Add(buttons);

            Controls.Add(content);
            AcceptButton = ok;
            CancelButton = ok;
        }

        private static Label Paragraph(string text, int bottomMargin = 10) => new()
        {
            Text = text,
            AutoSize = true,
            MaximumSize = new Size(TextWidth, 0),
            Margin = new Padding(0, 0, 0, bottomMargin),
        };

        private static Label Caption(string text) => new()
        {
            Text = text.ToUpperInvariant(),
            AutoSize = true,
            ForeColor = GreyText,
            Font = new Font("Segoe UI Semibold", 8f),
            Margin = new Padding(0, 6, 0, 2),
        };

        private static LinkLabel Link(string text, string url)
        {
            var link = new LinkLabel
            {
                Text = text,
                AutoSize = true,
                LinkColor = ModernUi.Accent,
                ActiveLinkColor = ModernUi.Accent,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Margin = new Padding(0, 0, 0, 4),
            };
            link.LinkClicked += (_, _) =>
            {
                try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
                catch (Exception ex) { MessageBox.Show("The link could not be opened: " + ex.Message, "Open FTA"); }
            };
            return link;
        }
    }
}
