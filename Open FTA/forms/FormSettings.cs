using Open_FTA.Properties;

namespace Open_FTA.forms
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
            this.Icon = Resources.Main;
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = MainAppSettings.Instance;

            propertyGrid1.SelectedObject = MainAppSettings.Instance;
            propertyGrid1.Refresh();
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }
    }
}
