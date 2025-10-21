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
            label3 = new Label();
            comboBoxMetricType = new ComboBox();
            label8 = new Label();
            tabControlfailure_metrics = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            label5 = new Label();
            tabPage3 = new TabPage();
            label6 = new Label();
            tabPage4 = new TabPage();
            label7 = new Label();
            pictureBox1 = new PictureBox();
            comboBoxGates = new ComboBox();
            textBoxDescription = new TextBox();
            tabPageIntermediate = new TabPage();
            label2 = new Label();
            tabPageBasic = new TabPage();
            comboBoxEventType = new ComboBox();
            pictureBox2 = new PictureBox();
            label1 = new Label();
            textBoxName = new TextBox();
            button2 = new Button();
            button1 = new Button();
            tabControl1 = new TabControl();
            panel1 = new Panel();
            groupBox3 = new GroupBox();
            groupBoxDetailSettings = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            tabControlfailure_metrics.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tabPageIntermediate.SuspendLayout();
            tabPageBasic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            tabControl1.SuspendLayout();
            panel1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBoxDetailSettings.SuspendLayout();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(31, 149);
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
            textBoxTag.Location = new Point(124, 109);
            textBoxTag.Margin = new Padding(4, 3, 4, 3);
            textBoxTag.Name = "textBoxTag";
            textBoxTag.Size = new Size(139, 23);
            textBoxTag.TabIndex = 21;
            textBoxTag.TextChanged += textBoxTag_TextChanged;
            // 
            // TAG
            // 
            TAG.AutoSize = true;
            TAG.Location = new Point(31, 113);
            TAG.Margin = new Padding(4, 0, 4, 0);
            TAG.Name = "TAG";
            TAG.Size = new Size(28, 15);
            TAG.TabIndex = 20;
            TAG.Text = "TAG";
            // 
            // comboBoxUnits
            // 
            comboBoxUnits.FormattingEnabled = true;
            comboBoxUnits.Location = new Point(214, 17);
            comboBoxUnits.Margin = new Padding(2);
            comboBoxUnits.Name = "comboBoxUnits";
            comboBoxUnits.Size = new Size(45, 23);
            comboBoxUnits.TabIndex = 3;
            comboBoxUnits.SelectedIndexChanged += comboBoxUnits_SelectedIndexChanged_1;
            // 
            // buttonDatabase
            // 
            buttonDatabase.Location = new Point(309, 15);
            buttonDatabase.Margin = new Padding(2);
            buttonDatabase.Name = "buttonDatabase";
            buttonDatabase.Size = new Size(88, 22);
            buttonDatabase.TabIndex = 2;
            buttonDatabase.Text = "Database";
            buttonDatabase.UseVisualStyleBackColor = true;
            buttonDatabase.Click += buttonDatabase_Click_1;
            // 
            // textBoxFrequency
            // 
            textBoxFrequency.Location = new Point(92, 17);
            textBoxFrequency.Margin = new Padding(4, 3, 4, 3);
            textBoxFrequency.Name = "textBoxFrequency";
            textBoxFrequency.Size = new Size(116, 23);
            textBoxFrequency.TabIndex = 1;
            textBoxFrequency.TextChanged += textBoxFrequency_TextChanged;
            textBoxFrequency.Validating += textBoxFrequency_Validating;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(8, 20);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(83, 15);
            label3.TabIndex = 0;
            label3.Text = "Frequency f = ";
            label3.Click += label3_Click;
            // 
            // comboBoxMetricType
            // 
            comboBoxMetricType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMetricType.FormattingEnabled = true;
            comboBoxMetricType.Location = new Point(108, 16);
            comboBoxMetricType.Name = "comboBoxMetricType";
            comboBoxMetricType.Size = new Size(174, 23);
            comboBoxMetricType.TabIndex = 6;
            comboBoxMetricType.SelectedIndexChanged += comboBoxMetricType_SelectedIndexChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(15, 19);
            label8.Name = "label8";
            label8.Size = new Size(87, 15);
            label8.TabIndex = 5;
            label8.Text = "Select a metric:";
            // 
            // tabControlfailure_metrics
            // 
            tabControlfailure_metrics.Controls.Add(tabPage1);
            tabControlfailure_metrics.Controls.Add(tabPage2);
            tabControlfailure_metrics.Controls.Add(tabPage3);
            tabControlfailure_metrics.Controls.Add(tabPage4);
            tabControlfailure_metrics.Location = new Point(15, 45);
            tabControlfailure_metrics.Name = "tabControlfailure_metrics";
            tabControlfailure_metrics.SelectedIndex = 0;
            tabControlfailure_metrics.Size = new Size(366, 99);
            tabControlfailure_metrics.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(textBoxFrequency);
            tabPage1.Controls.Add(comboBoxUnits);
            tabPage1.Controls.Add(label3);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(358, 71);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(label5);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(358, 71);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(8, 20);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(88, 15);
            label5.TabIndex = 2;
            label5.Text = "Probability  P =";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(label6);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(358, 71);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "tabPage3";
            tabPage3.UseVisualStyleBackColor = true;
            tabPage3.Click += tabPage3_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(8, 20);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(76, 15);
            label6.TabIndex = 4;
            label6.Text = "Reliability R=";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(label7);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(358, 71);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "tabPage4";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(10, 18);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(82, 15);
            label7.TabIndex = 6;
            label7.Text = "Failure rate λ=";
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(200, 25);
            pictureBox1.Margin = new Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(110, 110);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // comboBoxGates
            // 
            comboBoxGates.Location = new Point(24, 25);
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
            textBoxDescription.Size = new Size(140, 36);
            textBoxDescription.TabIndex = 22;
            // 
            // tabPageIntermediate
            // 
            tabPageIntermediate.Controls.Add(comboBoxGates);
            tabPageIntermediate.Controls.Add(pictureBox1);
            tabPageIntermediate.Location = new Point(4, 24);
            tabPageIntermediate.Margin = new Padding(4, 3, 4, 3);
            tabPageIntermediate.Name = "tabPageIntermediate";
            tabPageIntermediate.Padding = new Padding(4, 3, 4, 3);
            tabPageIntermediate.Size = new Size(423, 168);
            tabPageIntermediate.TabIndex = 0;
            tabPageIntermediate.Text = "tabPage1";
            tabPageIntermediate.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(31, 72);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 18;
            label2.Text = "Event type";
            // 
            // tabPageBasic
            // 
            tabPageBasic.Controls.Add(tabControlfailure_metrics);
            tabPageBasic.Controls.Add(label8);
            tabPageBasic.Controls.Add(comboBoxMetricType);
            tabPageBasic.Controls.Add(buttonDatabase);
            tabPageBasic.Location = new Point(4, 24);
            tabPageBasic.Margin = new Padding(4, 3, 4, 3);
            tabPageBasic.Name = "tabPageBasic";
            tabPageBasic.Padding = new Padding(4, 3, 4, 3);
            tabPageBasic.Size = new Size(423, 168);
            tabPageBasic.TabIndex = 1;
            tabPageBasic.Text = "tabPage2";
            tabPageBasic.UseVisualStyleBackColor = true;
            // 
            // comboBoxEventType
            // 
            comboBoxEventType.FormattingEnabled = true;
            comboBoxEventType.Location = new Point(124, 69);
            comboBoxEventType.Margin = new Padding(4, 3, 4, 3);
            comboBoxEventType.Name = "comboBoxEventType";
            comboBoxEventType.Size = new Size(140, 23);
            comboBoxEventType.TabIndex = 17;
            comboBoxEventType.SelectedIndexChanged += comboBoxEventType_SelectedIndexChanged;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(291, 25);
            pictureBox2.Margin = new Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(110, 110);
            pictureBox2.TabIndex = 16;
            pictureBox2.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(31, 25);
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
            textBoxName.Size = new Size(140, 23);
            textBoxName.TabIndex = 14;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.Cancel;
            button2.Dock = DockStyle.Right;
            button2.Location = new Point(261, 0);
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
            button1.Location = new Point(349, 0);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(88, 40);
            button1.TabIndex = 0;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageIntermediate);
            tabControl1.Controls.Add(tabPageBasic);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(3, 19);
            tabControl1.Margin = new Padding(4, 3, 4, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(431, 196);
            tabControl1.TabIndex = 19;
            // 
            // panel1
            // 
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 422);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(437, 40);
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
            groupBox3.Location = new Point(0, 0);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(437, 197);
            groupBox3.TabIndex = 24;
            groupBox3.TabStop = false;
            groupBox3.Text = "groupBox3";
            // 
            // groupBoxDetailSettings
            // 
            groupBoxDetailSettings.Controls.Add(tabControl1);
            groupBoxDetailSettings.Dock = DockStyle.Top;
            groupBoxDetailSettings.Location = new Point(0, 197);
            groupBoxDetailSettings.Name = "groupBoxDetailSettings";
            groupBoxDetailSettings.Size = new Size(437, 218);
            groupBoxDetailSettings.TabIndex = 25;
            groupBoxDetailSettings.TabStop = false;
            groupBoxDetailSettings.Text = "groupBox4";
            // 
            // FormEditEvent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(437, 462);
            Controls.Add(groupBoxDetailSettings);
            Controls.Add(panel1);
            Controls.Add(groupBox3);
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormEditEvent";
            Text = "FormEditEvent";
            FormClosing += FormEditEvent_FormClosing;
            Load += FormEditEvent_Load;
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            tabControlfailure_metrics.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tabPageIntermediate.ResumeLayout(false);
            tabPageBasic.ResumeLayout(false);
            tabPageBasic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            tabControl1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBoxDetailSettings.ResumeLayout(false);
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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageIntermediate;
        private System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.ComboBox comboBoxGates;
        private System.Windows.Forms.TabPage tabPageBasic;
        public System.Windows.Forms.ComboBox comboBoxUnits;
        private System.Windows.Forms.Button buttonDatabase;
        public System.Windows.Forms.TextBox textBoxFrequency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private TabControl tabControlfailure_metrics;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label label5;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private Label label6;
        private Label label7;
        private Label label8;
        private GroupBox groupBox3;
        private GroupBox groupBoxDetailSettings;
        public ComboBox comboBoxMetricType;
    }
}