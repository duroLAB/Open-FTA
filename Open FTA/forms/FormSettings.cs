using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
