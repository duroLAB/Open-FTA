namespace OpenFTA
{
    partial class FormEditEvent
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            label4 = new Label();
            errorProvider1 = new ErrorProvider(components);
            textBoxTag = new TextBox();
            TAG = new Label();
            comboBoxUnits = new ComboBox();
            buttonDatabase = new Button();
            textBoxFrequency = new TextBox();
            labelMetricType = new Label();
            comboBoxMetricType = new ComboBox();
            label8 = new Label();
            pictureBox1 = new PictureBox();
            comboBoxGates = new ComboBox();
            textBoxDescription = new TextBox();
            label2 = new Label();
            comboBoxEventType = new ComboBox();
            pictureBox2 = new PictureBox();
            label1 = new Label();
            textBoxName = new TextBox();
            button2 = new Button();
            button1 = new Button();
            panel1 = new Panel();
            groupBox3 = new GroupBox();
            groupBoxDetailSettings = new GroupBox();
            panelShowMetric = new Panel();
            panelShowGates = new Panel();
            label3 = new Label();
            tabControlMain = new TabControl();
            tabPageMain1 = new TabPage();
            tabPageMain2 = new TabPage();
            groupBoxReference = new GroupBox();
            textBoxReference = new TextBox();
            buttonAddreference = new Button();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBoxDetailSettings.SuspendLayout();
            panelShowMetric.SuspendLayout();
            panelShowGates.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabPageMain1.SuspendLayout();
            tabPageMain2.SuspendLayout();
            groupBoxReference.SuspendLayout();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(30, 146);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(67, 15);
            label4.TabIndex = 23;
            label4.Text = "Description";
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            // 
            // textBoxTag
            // 
            textBoxTag.Location = new Point(124, 64);
            textBoxTag.Margin = new Padding(4, 3, 4, 3);
            textBoxTag.Name = "textBoxTag";
            textBoxTag.Size = new Size(120, 23);
            textBoxTag.TabIndex = 21;
            textBoxTag.TextChanged += textBoxTag_TextChanged;
            // 
            // TAG
            // 
            TAG.AutoSize = true;
            TAG.Location = new Point(30, 67);
            TAG.Margin = new Padding(4, 0, 4, 0);
            TAG.Name = "TAG";
            TAG.Size = new Size(28, 15);
            TAG.TabIndex = 20;
            TAG.Text = "TAG";
            // 
            // comboBoxUnits
            // 
            comboBoxUnits.FormattingEnabled = true;
            comboBoxUnits.Location = new Point(242, 75);
            comboBoxUnits.Margin = new Padding(2);
            comboBoxUnits.Name = "comboBoxUnits";
            comboBoxUnits.Size = new Size(45, 23);
            comboBoxUnits.TabIndex = 3;
            comboBoxUnits.SelectedIndexChanged += comboBoxUnits_SelectedIndexChanged_1;
            // 
            // buttonDatabase
            // 
            buttonDatabase.Location = new Point(301, 74);
            buttonDatabase.Margin = new Padding(2);
            buttonDatabase.Name = "buttonDatabase";
            buttonDatabase.Size = new Size(88, 23);
            buttonDatabase.TabIndex = 2;
            buttonDatabase.Text = "Database";
            buttonDatabase.UseVisualStyleBackColor = true;
            buttonDatabase.Click += buttonDatabase_Click_1;
            // 
            // textBoxFrequency
            // 
            textBoxFrequency.Location = new Point(120, 75);
            textBoxFrequency.Margin = new Padding(4, 3, 4, 3);
            textBoxFrequency.Name = "textBoxFrequency";
            textBoxFrequency.Size = new Size(116, 23);
            textBoxFrequency.TabIndex = 1;
            textBoxFrequency.TextChanged += textBoxFrequency_TextChanged;
            textBoxFrequency.Validating += textBoxFrequency_Validating;
            // 
            // labelMetricType
            // 
            labelMetricType.AutoSize = true;
            labelMetricType.Location = new Point(18, 78);
            labelMetricType.Margin = new Padding(4, 0, 4, 0);
            labelMetricType.Name = "labelMetricType";
            labelMetricType.Size = new Size(83, 15);
            labelMetricType.TabIndex = 0;
            labelMetricType.Text = "Frequency f = ";
            labelMetricType.Click += label3_Click;
            // 
            // comboBoxMetricType
            // 
            comboBoxMetricType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMetricType.FormattingEnabled = true;
            comboBoxMetricType.Location = new Point(120, 27);
            comboBoxMetricType.Name = "comboBoxMetricType";
            comboBoxMetricType.Size = new Size(174, 23);
            comboBoxMetricType.TabIndex = 6;
            comboBoxMetricType.SelectedIndexChanged += comboBoxMetricType_SelectedIndexChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(14, 30);
            label8.Name = "label8";
            label8.Size = new Size(87, 15);
            label8.TabIndex = 5;
            label8.Text = "Select a metric:";
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(250, 30);
            pictureBox1.Margin = new Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(82, 82);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // comboBoxGates
            // 
            comboBoxGates.Location = new Point(108, 30);
            comboBoxGates.Name = "comboBoxGates";
            comboBoxGates.Size = new Size(121, 23);
            comboBoxGates.TabIndex = 0;
            comboBoxGates.SelectedIndexChanged += comboBoxGates_SelectedIndexChanged;
            // 
            // textBoxDescription
            // 
            textBoxDescription.Location = new Point(124, 146);
            textBoxDescription.Margin = new Padding(2);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.Size = new Size(292, 63);
            textBoxDescription.TabIndex = 22;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(30, 105);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 18;
            label2.Text = "Event type";
            // 
            // comboBoxEventType
            // 
            comboBoxEventType.FormattingEnabled = true;
            comboBoxEventType.Location = new Point(124, 102);
            comboBoxEventType.Margin = new Padding(4, 3, 4, 3);
            comboBoxEventType.Name = "comboBoxEventType";
            comboBoxEventType.Size = new Size(140, 23);
            comboBoxEventType.TabIndex = 17;
            comboBoxEventType.SelectedIndexChanged += comboBoxEventType_SelectedIndexChanged;
            // 
            // pictureBox2
            // 
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.Location = new Point(336, 56);
            pictureBox2.Margin = new Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(80, 80);
            pictureBox2.TabIndex = 16;
            pictureBox2.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 25);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 15;
            label1.Text = "Event name:";
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(124, 22);
            textBoxName.Margin = new Padding(4, 3, 4, 3);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(292, 23);
            textBoxName.TabIndex = 14;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.Cancel;
            button2.Dock = DockStyle.Right;
            button2.Location = new Point(652, 0);
            button2.Margin = new Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new Size(88, 40);
            button2.TabIndex = 1;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.OK;
            button1.Dock = DockStyle.Right;
            button1.Location = new Point(740, 0);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(88, 40);
            button1.TabIndex = 0;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 627);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(828, 40);
            panel1.TabIndex = 13;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(textBoxName);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(label1);
            groupBox3.Controls.Add(textBoxTag);
            groupBox3.Controls.Add(pictureBox2);
            groupBox3.Controls.Add(TAG);
            groupBox3.Controls.Add(comboBoxEventType);
            groupBox3.Controls.Add(textBoxDescription);
            groupBox3.Controls.Add(label2);
            groupBox3.Dock = DockStyle.Top;
            groupBox3.Location = new Point(3, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(814, 228);
            groupBox3.TabIndex = 24;
            groupBox3.TabStop = false;
            groupBox3.Text = "   Identification";
            // 
            // groupBoxDetailSettings
            // 
            groupBoxDetailSettings.BackColor = SystemColors.Control;
            groupBoxDetailSettings.Controls.Add(panelShowMetric);
            groupBoxDetailSettings.Controls.Add(panelShowGates);
            groupBoxDetailSettings.Dock = DockStyle.Top;
            groupBoxDetailSettings.Location = new Point(3, 231);
            groupBoxDetailSettings.Name = "groupBoxDetailSettings";
            groupBoxDetailSettings.Size = new Size(814, 215);
            groupBoxDetailSettings.TabIndex = 25;
            groupBoxDetailSettings.TabStop = false;
            groupBoxDetailSettings.Text = "groupBox4";
            // 
            // panelShowMetric
            // 
            panelShowMetric.Controls.Add(comboBoxUnits);
            panelShowMetric.Controls.Add(textBoxFrequency);
            panelShowMetric.Controls.Add(buttonDatabase);
            panelShowMetric.Controls.Add(label8);
            panelShowMetric.Controls.Add(labelMetricType);
            panelShowMetric.Controls.Add(comboBoxMetricType);
            panelShowMetric.Location = new Point(409, 22);
            panelShowMetric.Name = "panelShowMetric";
            panelShowMetric.Size = new Size(409, 155);
            panelShowMetric.TabIndex = 1;
            // 
            // panelShowGates
            // 
            panelShowGates.Controls.Add(label3);
            panelShowGates.Controls.Add(pictureBox1);
            panelShowGates.Controls.Add(comboBoxGates);
            panelShowGates.Location = new Point(20, 22);
            panelShowGates.Name = "panelShowGates";
            panelShowGates.Size = new Size(362, 155);
            panelShowGates.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(30, 33);
            label3.Name = "label3";
            label3.Size = new Size(73, 15);
            label3.TabIndex = 2;
            label3.Text = "Choose gate";
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPageMain1);
            tabControlMain.Controls.Add(tabPageMain2);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Location = new Point(0, 0);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(828, 627);
            tabControlMain.TabIndex = 26;
            // 
            // tabPageMain1
            // 
            tabPageMain1.Controls.Add(groupBoxDetailSettings);
            tabPageMain1.Controls.Add(groupBox3);
            tabPageMain1.Location = new Point(4, 24);
            tabPageMain1.Name = "tabPageMain1";
            tabPageMain1.Padding = new Padding(3);
            tabPageMain1.Size = new Size(820, 599);
            tabPageMain1.TabIndex = 0;
            tabPageMain1.Text = "General";
            tabPageMain1.UseVisualStyleBackColor = true;
            // 
            // tabPageMain2
            // 
            tabPageMain2.Controls.Add(groupBoxReference);
            tabPageMain2.Location = new Point(4, 24);
            tabPageMain2.Name = "tabPageMain2";
            tabPageMain2.Padding = new Padding(3);
            tabPageMain2.Size = new Size(820, 599);
            tabPageMain2.TabIndex = 1;
            tabPageMain2.Text = "Reference";
            tabPageMain2.UseVisualStyleBackColor = true;
            // 
            // groupBoxReference
            // 
            groupBoxReference.Controls.Add(textBoxReference);
            groupBoxReference.Controls.Add(buttonAddreference);
            groupBoxReference.Dock = DockStyle.Top;
            groupBoxReference.Location = new Point(3, 3);
            groupBoxReference.Name = "groupBoxReference";
            groupBoxReference.Size = new Size(814, 165);
            groupBoxReference.TabIndex = 0;
            groupBoxReference.TabStop = false;
            groupBoxReference.Text = "Reference";
            // 
            // textBoxReference
            // 
            textBoxReference.Location = new Point(6, 51);
            textBoxReference.Multiline = true;
            textBoxReference.Name = "textBoxReference";
            textBoxReference.Size = new Size(396, 108);
            textBoxReference.TabIndex = 1;
            // 
            // buttonAddreference
            // 
            buttonAddreference.Location = new Point(6, 22);
            buttonAddreference.Name = "buttonAddreference";
            buttonAddreference.Size = new Size(91, 23);
            buttonAddreference.TabIndex = 0;
            buttonAddreference.Text = "Add new";
            buttonAddreference.UseVisualStyleBackColor = true;
            buttonAddreference.Click += buttonAddreference_Click;
            // 
            // FormEditEvent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(828, 667);
            Controls.Add(tabControlMain);
            Controls.Add(panel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormEditEvent";
            Text = "FormEditEvent";
            FormClosing += FormEditEvent_FormClosing;
            Load += FormEditEvent_Load;
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBoxDetailSettings.ResumeLayout(false);
            panelShowMetric.ResumeLayout(false);
            panelShowMetric.PerformLayout();
            panelShowGates.ResumeLayout(false);
            panelShowGates.PerformLayout();
            tabControlMain.ResumeLayout(false);
            tabPageMain1.ResumeLayout(false);
            tabPageMain2.ResumeLayout(false);
            groupBoxReference.ResumeLayout(false);
            groupBoxReference.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        public System.Windows.Forms.TextBox textBoxTag;
        private System.Windows.Forms.Label TAG;
        public System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox comboBoxEventType;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.ComboBox comboBoxGates;
        public System.Windows.Forms.ComboBox comboBoxUnits;
        private System.Windows.Forms.Button buttonDatabase;
        public System.Windows.Forms.TextBox textBoxFrequency;
        private System.Windows.Forms.Label labelMetricType;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private Label label8;
        private GroupBox groupBox3;
        private GroupBox groupBoxDetailSettings;
        public ComboBox comboBoxMetricType;
        private TabControl tabControlMain;
        private TabPage tabPageMain1;
        private TabPage tabPageMain2;
        private Panel panelShowGates;
        private Panel panelShowMetric;
        private Label label3;
        private GroupBox groupBoxReference;
        private Button buttonAddreference;
        public TextBox textBoxReference;
    }
}