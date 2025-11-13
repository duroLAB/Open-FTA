using Open_FTA.Properties;
using System.Data;

namespace Open_FTA.forms
{
    public partial class ErrorDialog : Form
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        private void ErrorDialog_Load(object sender, EventArgs e)
        {

        }

        private ListView listView;
        private Button btnCopy;
        private Button btnClose;
        private Label lblTitle;
        private ImageList imageList;

        public enum MessageType
        {
            Error,
            Warning,
            Info
        }



        public ErrorDialog(IEnumerable<MessageItem> messages)
        {
            Text = "Validation Results";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Size = new Size(800, 400);
            BackColor = Color.White;

            lblTitle = new Label()
            {
                Text = "Validation issues detected:",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10, 10, 10, 5),
                AutoSize = false,
                Height = 40
            };

            imageList = new ImageList();
            imageList.ImageSize = new Size(18, 18);
            imageList.ColorDepth = ColorDepth.Depth32Bit;


            //  imageList.Images.Add("Error", SystemIcons.Error.ToBitmap());
            imageList.Images.Add("Error", Resources.StatusError_18_18);
            imageList.Images.Add("Warning", Resources.StatusWarning_18_18);
            imageList.Images.Add("Info", Resources.StatusInformation_18_18);

            listView = new ListView()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                FullRowSelect = true,
                View = View.Details,
                HeaderStyle = ColumnHeaderStyle.None,
                SmallImageList = imageList,
                BackColor = Color.WhiteSmoke,
            };

            listView.Columns.Add("", 40);

            listView.Columns.Add("Message", 520);

            foreach (var msg in messages)
            {
                string key = msg.Type.ToString();
                var item = new ListViewItem("", key);

                item.SubItems.Add(msg.Text);
                listView.Items.Add(item);
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            btnCopy = new Button()
            {
                Text = "Copy All",
                Dock = DockStyle.Bottom,
                Height = 35,
                FlatStyle = FlatStyle.Flat
            };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.BackColor = Color.FromArgb(230, 230, 230);
            btnCopy.Click += (s, e) =>
            {
                string all = string.Join(Environment.NewLine, listView.Items.Cast<ListViewItem>().Select(i => i.SubItems[1].Text));
                Clipboard.SetText(all);
                MessageBox.Show("All messages copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnClose = new Button()
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 35,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.FromArgb(230, 230, 230);
            btnClose.Click += (s, e) => Close();

            Controls.Add(listView);
            Controls.Add(btnCopy);
            Controls.Add(btnClose);
            Controls.Add(lblTitle);
        }



        public static void ShowMessages(IEnumerable<MessageItem> messages, IWin32Window owner = null)
        {
            using (var dlg = new ErrorDialog(messages))
            {
                dlg.ShowDialog(owner);
            }
        }


    }
}
