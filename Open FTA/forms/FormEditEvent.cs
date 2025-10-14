using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

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

            comboBoxGates.DataSource = new BindingSource(EngineLogic.GatesList, null);
            comboBoxGates.DisplayMember = "Value";
            comboBoxGates.ValueMember = "Key";

            comboBoxEventType.DataSource = new BindingSource(EngineLogic.EventsList, null);
            comboBoxEventType.DisplayMember = "Value";
            comboBoxEventType.ValueMember = "Key";

            comboBoxEventType.SelectedValue = 2;
            comboBoxGates.SelectedValue = 2;

            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            int count = EngineLogic.SelectedEvents.Count;
            if (count == 1)
            {
                var singleEvent = EngineLogic.SelectedEvents[0];
                textBoxTag.Text = singleEvent.Tag;
                textBoxName.Text = singleEvent.Name;
                comboBoxGates.SelectedValue = singleEvent.GateType;
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
            comboBoxUnits.Items.Add("y⁻¹");
            comboBoxUnits.Items.Add("h⁻¹");
            comboBoxUnits.Items.Add("s⁻¹");
            comboBoxUnits.SelectedIndex = 0;
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
                    tabControl1.SelectedTab = tabControl1.TabPages["tabPageIntermediate"];
                else
                    tabControl1.SelectedTab = tabControl1.TabPages["tabPageBasic"];
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
            if (textBoxTag.Text.Contains(" "))
            {
                MessageBox.Show("TAG must be a single word; no spaces allowed.", "Invalid TAG", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Nastavíme DialogResult ako Cancel, aby sa vrátilo späť do hlavného (editovacieho) okna
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
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
    }

}
