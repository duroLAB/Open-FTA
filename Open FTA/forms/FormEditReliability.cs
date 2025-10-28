using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Open_FTA.forms
{
    public partial class FormEditReliability : Form
    {
        public FormEditReliability()
        {
            InitializeComponent();
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
