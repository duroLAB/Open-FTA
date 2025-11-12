using Open_FTA.engine;
using System.Data;

namespace Open_FTA.forms
{

    public partial class FormDbViewer : Form
    {
        UIEngine u;

        public FormDbViewer(FTAlogic EngineLogic)
        {
            InitializeComponent();

            u = new UIEngine(EngineLogic);
            u.MakeTabControlModern(tabControlMain);
            //            u.SetupModernGrid(dataGridView1);
            //  u.SetupModernGrid(dataGridView2);

            FillDGV();
        }

        private void FillDGV()
        {
            DataTable data = DBEngine.Instance.GetReferenceDataTable(true);
            dataGridView1.DataSource = data;


            u.SetupModernGrid(dataGridView2);
            u.SetupModernGrid(dataGridView1);


            Application.DoEvents();

            if (dataGridView1.Columns.Contains("Id"))
            {
                dataGridView1.Columns["Id"].Visible = false;
            }

            if (dataGridView1.Columns.Contains("Year"))
                dataGridView1.Columns["Year"].FillWeight = 30;


            DataTable data2 = DBEngine.Instance.GetReliabilityDataTablev2();
            dataGridView2.DataSource = data2;


            Application.DoEvents();

            try
            {

                dataGridView2.Columns["Id"].Visible = false;
                dataGridView2.Columns["MetricUnit"].FillWeight = 30;
                dataGridView2.Columns["Val"].FillWeight = 30;


            }
            catch { }





        }

        private void FormDbViewer_Load(object sender, EventArgs e)
        {





        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {


            /* string update = @"UPDATE ReferenceTable 
                           SET Title = @Title, Publisher = @Publisher, Authors = @Authors, Year = @Year
                           WHERE Id = @Id";

             _db.ExecuteNonQuery(update,
                 ("@Id", id),
                 ("@Title", txtTitle.Text),
                 ("@Publisher", txtPublisher.Text),
                 ("@Authors", txtAuthors.Text),
                 ("@Year", int.TryParse(txtYear.Text, out int y) ? y : DBNull.Value)
             );

             LoadData();*/
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            FormEditReference form = new FormEditReference();
            form.Text = "Add New Reference";
            form.ShowDialog();

            if (form.DialogResult == DialogResult.OK)
            {

                int year = int.TryParse(form.txtYear.Text, out int y2) ? y2 : 1990;

                DBEngine.Instance.InsertReference(Guid.NewGuid().ToString(),
                    form.txtTitle.Text,
                    form.txtPublisher.Text,
                    form.txtAuthors.Text,
                    year
                    );

                FillDGV();
            }

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

            if (dataGridView1.CurrentRow == null) return;
            string id = dataGridView1.CurrentRow.Cells["Id"].Value.ToString();

            bool res = DBEngine.Instance.GetReferenceById(id, out string title, out string publisher, out string authors, out int? Year);

            if (res == false) return;
            FormEditReference form = new FormEditReference();
            form.Text = "Edit Reference";
            form.txtTitle.Text = title;
            form.txtAuthors.Text = authors;
            form.txtPublisher.Text = publisher;
            form.txtYear.Text = Year.ToString();

            form.ShowDialog();

            if (form.DialogResult == DialogResult.OK)
            {


                DBEngine.Instance.UpdateReference(id,
                    form.txtTitle.Text,
                    form.txtPublisher.Text,
                    form.txtAuthors.Text,
                    int.TryParse(form.txtYear.Text, out int y2) ? y2 : null
                    );

                FillDGV();
            }


        }




        private void toolStripButtonAddFrequency_Click(object sender, EventArgs e)
        {
            FormEditReliability form = new FormEditReliability();
            form.Text = "Add New Reliability Entry";
            form.ShowDialog();

            if (form.DialogResult == DialogResult.OK)
            {
                double val = double.TryParse(form.textBoxR_Val.Text, out double v) ? v : 0.0;
                int unit = (int)form.comboBoxMetricUnits.SelectedValue;
                string title = form.textBoxR_Title.Text;
                DBEngine.Instance.InsertReliability(Guid.NewGuid().ToString(),
                    title,
                    val,
                    unit,
                    form.comboBox2.SelectedValue.ToString()
                    );
                FillDGV();
            }
        }

        private void toolStripButtonEditFrequency_Click(object sender, EventArgs e)
        {


            if (dataGridView2.CurrentRow == null) return;
            string id = dataGridView2.CurrentRow.Cells["Id"].Value.ToString();

            bool res = DBEngine.Instance.GetReliabilityById(id, out string title, out double R_Val, out int MetricUnitId, out string ReferenceId);

            if (res == false) return;

            FormEditReliability form = new FormEditReliability();
            form.Text = "Edit Reliability Entry";

            form.textBoxR_Title.Text = title;
            form.textBoxR_Val.Text = R_Val.ToString();
            form.comboBoxMetricUnits.SelectedValue = MetricUnitId;
            form.comboBox2.SelectedValue = ReferenceId;

            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                double val = double.TryParse(form.textBoxR_Val.Text, out double v) ? v : 0.0;
                int unit = (int)form.comboBoxMetricUnits.SelectedValue;
                string title2 = form.textBoxR_Title.Text;
                DBEngine.Instance.UpdateReliability(id,
                    title2,
                    val,
                    unit,
                    form.comboBox2.SelectedValue.ToString()
                    );
                FillDGV();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null) return;
            string id = dataGridView2.CurrentRow.Cells["Id"].Value.ToString();

            if (MessageBox.Show("Are you sure you want to delete this reliability entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool res = DBEngine.Instance.DeleteReliability(id);
                FillDGV();
            }




        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;
            string id = dataGridView1.CurrentRow.Cells["Id"].Value.ToString();

            if (DBEngine.Instance.CanDeleteReference(id) == false)
            {
                MessageBox.Show("This reference cannot be deleted because it is being used by one or more reliability entries.", "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (MessageBox.Show("Are you sure you want to delete this reference?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool res = DBEngine.Instance.DeleteReference(id);
                FillDGV();
            }

        }

        private void FormDbViewer_Shown(object sender, EventArgs e)
        {

        }

        private void dataGridView2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
        }
    }
}
