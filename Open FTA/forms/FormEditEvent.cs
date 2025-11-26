using Open_FTA.engine;
using Open_FTA.forms;
using Open_FTA.Properties;
using System.ComponentModel;
using System.Data;

namespace OpenFTA
{


    public partial class FormEditEvent : Form
    {
        string strPICPath;
        public FTAlogic EngineLogic;
        private string previousUnit = "y⁻¹";
        private bool isLoading = false;

        public FormEditEvent(FTAlogic EngineLogic)
        {
            InitializeComponent();
            this.Icon = Resources.Main;
            Width = 450;
            Height = 550;


            this.Text = "New Event";


        

            comboBoxGates.DataSource = Enum.GetValues(typeof(Gates))
                           .Cast<Gates>()
                           .Where(g => g != Gates.NotSet) // filtrovanie NotSet
                           .ToList();

            comboBoxEventType.DataSource = new BindingSource(EngineLogic.EventsList, null);
            comboBoxEventType.DisplayMember = "Value";
            comboBoxEventType.ValueMember = "Key";

            comboBoxMetricType.DataSource = new BindingSource(EngineLogic.MetricList, null);
            comboBoxMetricType.DisplayMember = "Value";
            comboBoxMetricType.ValueMember = "Key";

            comboBoxUnits.DataSource = new BindingSource(EngineLogic.MetricUnitsList, null);
            comboBoxUnits.DisplayMember = "Value";
            comboBoxUnits.ValueMember = "Key";

            comboBoxEventType.SelectedValue = 2;
            comboBoxGates.SelectedIndex = 1;
            comboBoxMetricType.SelectedIndex = 0;
            comboBoxUnits.SelectedIndex = 0;




            button1.CausesValidation = false;
            errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            pictureBox1.BackColor = Color.Transparent;
            pictureBox2.BackColor = Color.Transparent;

            UIEngine u = new UIEngine(EngineLogic);
            u.MakeTabControlModern(tabControlMain);




            u.MakeGroupBoxModern(groupBoxDetailSettings);
            u.MakeGroupBoxModern(groupBox3);
            u.MakeGroupBoxModern(groupBoxReference);



            //panelShowGates.Parent = groupBoxDetailSettings;
            panelShowGates.Left = 10;
            panelShowGates.Top = 20;

            //  panelShowMetric.Parent = panelShowGates;
            panelShowMetric.Left = panelShowGates.Left;
            panelShowMetric.Top = panelShowGates.Top;



        }

        private void comboBoxGates_SelectedIndexChanged(object sender, EventArgs e)
        {
            String selected_item_text = comboBoxGates.Text;
            if (selected_item_text.Length > 1)
            {
               
                Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.Image = bmp;

                string t = selected_item_text.Replace(" ", "");
                Rectangle r = new Rectangle((int)(-1.8*pictureBox1.Width)+63 , -1*pictureBox1.Height, (int)(2.9* pictureBox1.Width)+10, pictureBox1.Height);
            

                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    if(t.Contains("AND"))
                    BitmapDrawingEngine.Instance.DrawANDGate(g, r);
                    if (t.Contains("OR"))
                        BitmapDrawingEngine.Instance.DrawOrGate(g, r);
                }
                pictureBox1.Refresh();
            }
        }

        private void comboBoxEventType_SelectedIndexChanged(object sender, EventArgs e)
        {
            String selected_item_text = comboBoxEventType.Text;
            if (selected_item_text.Length > 1)
            {
           

                Bitmap bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);
                pictureBox2.Image = bmp;

                string t = selected_item_text.Replace(" ", "");
                Rectangle r = new Rectangle((int)(-1.8 * pictureBox2.Width) + 33, -1 * pictureBox2.Height, (int)(3 * pictureBox2.Width) + 60, pictureBox2.Height);


                using (Graphics g = Graphics.FromImage(pictureBox2.Image))
                {

                    if (t.Contains("Basic")) BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Basic");
                    if (t.Contains("House")) BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "House");
                    if (t.Contains("Undeveloped")) BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Undeveloped");

                }
                pictureBox2.Refresh();
            }


            if (comboBoxEventType.SelectedIndex == 0)
            {
                // tabControl1.SelectedTab = tabControl1.TabPages["tabPageIntermediate"];
                panelShowGates.Visible = true;
                panelShowMetric.Visible = false;
                groupBoxDetailSettings.Text = "Gate definition";
            }
            else

            {
                panelShowGates.Visible = false;
                panelShowMetric.Visible = true;
                //tabControl1.SelectedTab = tabControl1.TabPages["tabPageBasic"];
                groupBoxDetailSettings.Text = "Reliability Metrics";
            }



        }

        private void buttonDatabase_Click(object sender, EventArgs e)
        {
            {
                // Vytvoríme novú inštanciu formulára pre databázové rozhranie.
                /*   Database dbForm = new Database();
                   // Otvoríme formulár ako modálny dialóg, takže používateľ musí s ním interagovať, kým sa nevráti do hlavného okna.
                   dbForm.ShowDialog();*/
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /* if (textBoxTag.Text.Contains(" "))
             {
                 MessageBox.Show("TAG must be a single word; no spaces allowed.", "Invalid TAG", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 // Nastavíme DialogResult ako Cancel, aby sa vrátilo späť do hlavného (editovacieho) okna
                 this.DialogResult = DialogResult.Cancel;
                 this.Close();
                 return;
             }*/

            bool res = ValidateAll();
            if (res)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.None;
            }


        }

        private void textBoxTag_TextChanged(object sender, EventArgs e)
        {
            {
                
              /*  if (textBoxTag.Text.Contains(" "))
                {
                   
                    errorProvider1.SetError(textBoxTag, "TAG must be a single word; no spaces allowed.");
                    button1.Enabled = false;

                }
                else
                {
                    button1.Enabled = true;
                    errorProvider1.SetError(textBoxTag, "");
                }*/
            }
        }

        private void comboBoxUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading) return;

            try
            {
                isLoading = true;

                string newUnit = comboBoxUnits.SelectedItem.ToString();
                if (string.IsNullOrWhiteSpace(textBoxFrequency.Text)) return;

                // Podpora vedeckého zápisu (napr. 1e-4)
                if (!double.TryParse(textBoxFrequency.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double originalValue))
                {
                    MessageBox.Show("Invalid frequency format.");
                    return;
                }

                // Prevod na roky
                double valueInYears = ConvertToYears(originalValue, previousUnit);
                double newValue = ConvertFromYears(valueInYears, newUnit);

                // Zobraziť vhodný formát (vedecký, ak je hodnota malá)
                if (Math.Abs(newValue) < 0.001)
                    textBoxFrequency.Text = newValue.ToString("0.000E0", System.Globalization.CultureInfo.InvariantCulture);
                else
                    textBoxFrequency.Text = newValue.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);

                previousUnit = newUnit;
            }
            catch
            {
                MessageBox.Show("Conversion failed.");
            }
            finally
            {
                isLoading = false;
            }
        }
        private double ConvertToYears(double freq, string unit)
        {
            switch (unit)
            {
                case "h⁻¹": return freq * 24 * 365;
                case "s⁻¹": return freq * 60 * 60 * 24 * 365;
                case "y⁻¹":
                default: return freq;
            }
        }

        private double ConvertFromYears(double freqPerYear, string unit)
        {
            switch (unit)
            {
                case "h⁻¹": return freqPerYear / (24 * 365);
                case "s⁻¹": return freqPerYear / (60 * 60 * 24 * 365);
                case "y⁻¹":
                default: return freqPerYear;
            }
        }

        private void FormEditEvent_Load(object sender, EventArgs e)
        {

        }

        private void comboBoxUnits_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void buttonDatabase_Click_1(object sender, EventArgs e)
        {
            FormDbViewer dbForm = new FormDbViewer(EngineLogic);

            dbForm.tabPage1.Visible = false;
            dbForm.tabControlMain.TabPages.Remove(dbForm.tabPage1);

            dbForm.ShowDialog();



            if (dbForm.DialogResult == DialogResult.OK)
            {
                try
                {

                    string id = dbForm.dataGridView2.CurrentRow.Cells["Id"].Value.ToString();
                    bool res = DBEngine.Instance.GetReliabilityById(id, out string title, out double R_Val, out int MetricUnitId, out string ReferenceId);

                    textBoxFrequency.Text = R_Val.ToString();
                    comboBoxUnits.SelectedValue = MetricUnitId;
                    comboBoxMetricType.SelectedIndex = 0; // Frequency    
                    textBoxDescription.Text += title + " f=" + R_Val.ToString() + EngineLogic.MetricUnitsList[MetricUnitId];

                    DBEngine.Instance.GetReferenceById(ReferenceId, out string title2);
                    textBoxReference.Text = title2;
                }
                catch
                {
                    // ignorujeme chyby
                }
            }


        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBoxFrequency_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxMetricType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //tabControlfailure_metrics.SelectedIndex = comboBoxMetricType.SelectedIndex;
            //textBoxFrequency.Parent = tabControlfailure_metrics.TabPages[comboBoxMetricType.SelectedIndex];
            //comboBoxUnits.Parent = textBoxFrequency.Parent;

            if (comboBoxMetricType.SelectedIndex == 0 || comboBoxMetricType.SelectedIndex == 3)
                comboBoxUnits.Visible = true;
            else comboBoxUnits.Visible = false;

            if (comboBoxMetricType.SelectedIndex == 0)
                labelMetricType.Text = "Frequency f =";
            if (comboBoxMetricType.SelectedIndex == 1)
                labelMetricType.Text = "Probability  P =";
            if (comboBoxMetricType.SelectedIndex == 2)
                labelMetricType.Text = "Reliability R =";
            if (comboBoxMetricType.SelectedIndex == 3)
                labelMetricType.Text = "Failure rate λ =";





        }

        private void FormEditEvent_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void textBoxFrequency_Validating(object sender, CancelEventArgs e)
        {
            /*  if(!string.IsNullOrWhiteSpace(textBoxFrequency.Text))
              {
                  if (!double.TryParse(textBoxFrequency.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
                  {
                      errorProvider1.SetError(textBoxFrequency, "Invalid frequency format.");
                  }
              }*/
            // Pokúsime sa previesť text na číslo (double = umožní aj desatinné)
            /* if (!double.TryParse(textBoxFrequency.Text, out _))
             {
               //  e.Cancel = true; // zastaví presun fokusu
                 errorProvider1.SetError(textBoxFrequency, "Zadaj platné číslo.");
             }
             else
             {
                 errorProvider1.SetError(textBoxFrequency, "");
             }*/
        }

        public bool ValidateAll()
        {
            bool res = true;

            if (Convert.ToInt32(comboBoxEventType.SelectedValue) == 1)
            {
                errorProvider1.SetError(textBoxFrequency, "");                
            }


            if (Convert.ToInt32(comboBoxEventType.SelectedValue) > 1)
            {
                bool temp = Validatefrequency();
                if (temp == false) res = false;

            }

            if (textBoxTag.Text.Contains(" "))
            {

                errorProvider1.SetError(textBoxTag, "TAG must be a single word; no spaces allowed.");
                textBoxTag.BackColor = Color.MistyRose;
                res = false;

            }
            else
            {
                textBoxTag.BackColor = Color.White;
                errorProvider1.SetError(textBoxTag, "");
            }


            

            return (res);
        }

        public bool Validatefrequency()
        {
            string text = textBoxFrequency.Text.Trim();

            bool valid = double.TryParse(text,
                                         System.Globalization.NumberStyles.Float,
                                         System.Globalization.CultureInfo.CurrentCulture,
                                         out double value);

            if (!valid)
            {
                valid = double.TryParse(text,
                                        System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        out value);
            }

            if (!valid)
            {
                errorProvider1.SetError(textBoxFrequency, "The TextBox contains a value that cannot be interpreted.");
                textBoxFrequency.BackColor = Color.MistyRose; // vizuálna spätná väzba

            }
            else
            {
                textBoxFrequency.BackColor = Color.White; // vizuálna spätná väzba
                errorProvider1.SetError(textBoxFrequency, "");
            }

            return (valid);

        }

        private void buttonAddreference_Click(object sender, EventArgs e)
        {
            FormDbViewer dbForm = new FormDbViewer(EngineLogic);

            dbForm.tabPage1.Visible = false;
            dbForm.tabControlMain.TabPages.Remove(dbForm.tabPage2);

            dbForm.ShowDialog();



            if (dbForm.DialogResult == DialogResult.OK)
            {
                string id = dbForm.dataGridView1.CurrentRow.Cells["Id"].Value.ToString();
                bool res = DBEngine.Instance.GetReferenceById(id, out string title);
                textBoxReference.Text = title;


            }

        }
    }

}
