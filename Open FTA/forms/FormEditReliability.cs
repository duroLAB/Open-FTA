using Open_FTA.Properties;
using System.Data;

namespace Open_FTA.forms
{
    public partial class FormEditReliability : Form
    {
        public FormEditReliability()
        {
            InitializeComponent();
            this.Icon = Resources.Main;

            var data = DBEngine.Instance.MetricUnitsList;

            comboBoxMetricUnits.DataSource = new BindingSource(data, null);
            comboBoxMetricUnits.DisplayMember = "Value";
            comboBoxMetricUnits.ValueMember = "Key";


            DataTable references = DBEngine.Instance.GetReferenceDataTable(false);
            comboBox2.DataSource = references;
            comboBox2.DisplayMember = "Title";
            comboBox2.ValueMember = "Id";
        }

        private void FormEditReliability_Load(object sender, EventArgs e)
        {

        }
    }
}
