using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
            this.Text = "New Event";

            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            strPICPath = System.IO.Path.GetDirectoryName(strExeFilePath);

            /*comboBoxGates.DataSource = new BindingSource(EngineLogic.GatesList, null);
            comboBoxGates.DisplayMember = "Value";
            comboBoxGates.ValueMember = "Key";*/

            comboBoxGates.DataSource = Enum.GetValues(typeof(Gates));

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

            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            tabControlfailure_metrics.Appearance = TabAppearance.FlatButtons;
            tabControlfailure_metrics.ItemSize = new Size(0, 1);
            tabControlfailure_metrics.SizeMode = TabSizeMode.Fixed;

            int count = EngineLogic.SelectedEvents.Count;
            if (count == 1)
            {
                var singleEvent = EngineLogic.SelectedEvents[0];
                textBoxTag.Text = singleEvent.Tag;
                textBoxName.Text = singleEvent.Name;
                //comboBoxGates.SelectedValue = singleEvent.Gate;

                comboBoxGates.SelectedItem = singleEvent.Gate;
                comboBoxEventType.SelectedValue = singleEvent.ItemType;
                textBoxFrequency.Text = singleEvent.Frequency.ToString();
                textBoxDescription.Text = singleEvent.Description;
            }
            else
            {
                textBoxTag.Text = "NewTag";
                textBoxName.Text = "";
                textBoxFrequency.Text = "0";
            }
            button1.CausesValidation = false;
            errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            pictureBox1.BackColor = Color.Transparent;
            pictureBox2.BackColor = Color.Transparent;
        }

        private void comboBoxGates_SelectedIndexChanged(object sender, EventArgs e)
        {
            String selected_item_text = comboBoxGates.Text;
            if (selected_item_text.Length > 1)
            {
                String Filename = "pic\\gates\\gate" + selected_item_text.Replace(" ", "") + ".png";
                String Imagepath = System.IO.Path.Combine(strPICPath, Filename);
                if (File.Exists(Imagepath))
                {
                    Image image1 = Image.FromFile(Imagepath);
                    pictureBox1.Image = image1;
                }
            }
        }

        private void comboBoxEventType_SelectedIndexChanged(object sender, EventArgs e)
        {
            String selected_item_text = comboBoxEventType.Text;
            if (selected_item_text.Length > 1)
            {
                String Filename = "pic\\events\\event" + selected_item_text.Replace(" ", "") + ".png";
                String Imagepath = System.IO.Path.Combine(strPICPath, Filename);
                if (File.Exists(Imagepath))
                {
                    Image image1 = Image.FromFile(Imagepath);
                    pictureBox2.Image = image1;
                }
            }

            if (comboBoxEventType.SelectedIndex == 0)
                tabControl1.Enabled = false;
            else
            {
                tabControl1.Enabled = true;

                if (comboBoxEventType.SelectedIndex < 2)
                {
                    tabControl1.SelectedTab = tabControl1.TabPages["tabPageIntermediate"];
                    groupBoxDetailSettings.Text = "Gate definition";
                }
                else

                {
                    tabControl1.SelectedTab = tabControl1.TabPages["tabPageBasic"];
                    groupBoxDetailSettings.Text = "Reliability Metrics";
                }
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
                // Skontrolujeme, či text obsahuje medzeru
                if (textBoxTag.Text.Contains(" "))
                {
                    // Nastavíme chybovú správu na ErrorProvider
                    errorProvider1.SetError(textBoxTag, "TAG must be a single word; no spaces allowed.");
                    button1.Enabled = false;

                }
                else
                {
                    // Vymažeme chybovú správu
                    errorProvider1.SetError(textBoxTag, "");
                }
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
            tabControlfailure_metrics.SelectedIndex = comboBoxMetricType.SelectedIndex;
            textBoxFrequency.Parent = tabControlfailure_metrics.TabPages[comboBoxMetricType.SelectedIndex];
            comboBoxUnits.Parent = textBoxFrequency.Parent;

            if (comboBoxMetricType.SelectedIndex == 0 || comboBoxMetricType.SelectedIndex == 3)
                comboBoxUnits.Visible = true;
            else comboBoxUnits.Visible = false;

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
    }

    }
