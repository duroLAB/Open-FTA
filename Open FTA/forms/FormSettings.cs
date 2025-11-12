namespace Open_FTA.forms
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = MainAppSettings.Instance;

            propertyGrid1.SelectedObject = MainAppSettings.Instance;
            propertyGrid1.Refresh();
        }
    }
}
