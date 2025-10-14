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
            groupBox2 = new GroupBox();
            groupBox1 = new GroupBox();
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
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tabPageIntermediate.SuspendLayout();
            tabPageBasic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            tabControl1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(74, 147);
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
            textBoxTag.Location = new Point(167, 107);
            textBoxTag.Margin = new Padding(4, 3, 4, 3);
            textBoxTag.Name = "textBoxTag";
            textBoxTag.Size = new Size(139, 23);
            textBoxTag.TabIndex = 21;
            // 
            // TAG
            // 
            TAG.AutoSize = true;
            TAG.Location = new Point(74, 111);
            TAG.Margin = new Padding(4, 0, 4, 0);
            TAG.Name = "TAG";
            TAG.Size = new Size(25, 15);
            TAG.TabIndex = 20;
            TAG.Text = "TAG";
            // 
            // comboBoxUnits
            // 
            comboBoxUnits.FormattingEnabled = true;
            comboBoxUnits.Location = new Point(275, 32);
            comboBoxUnits.Margin = new Padding(2);
            comboBoxUnits.Name = "comboBoxUnits";
            comboBoxUnits.Size = new Size(45, 23);
            comboBoxUnits.TabIndex = 3;
            // 
            // buttonDatabase
            // 
            buttonDatabase.Location = new Point(240, 144);
            buttonDatabase.Margin = new Padding(2);
            buttonDatabase.Name = "buttonDatabase";
            buttonDatabase.Size = new Size(88, 22);
            buttonDatabase.TabIndex = 2;
            buttonDatabase.Text = "Database";
            buttonDatabase.UseVisualStyleBackColor = true;
            // 
            // textBoxFrequency
            // 
            textBoxFrequency.Location = new Point(153, 32);
            textBoxFrequency.Margin = new Padding(4, 3, 4, 3);
            textBoxFrequency.Name = "textBoxFrequency";
            textBoxFrequency.Size = new Size(116, 23);
            textBoxFrequency.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 32);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(92, 15);
            label3.TabIndex = 0;
            label3.Text = "Event frequency";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(comboBoxUnits);
            groupBox2.Controls.Add(buttonDatabase);
            groupBox2.Controls.Add(textBoxFrequency);
            groupBox2.Controls.Add(label3);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(4, 3);
            groupBox2.Margin = new Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 3, 4, 3);
            groupBox2.Size = new Size(332, 166);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "Frequency";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(pictureBox1);
            groupBox1.Controls.Add(comboBoxGates);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(4, 3);
            groupBox1.Margin = new Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 3, 4, 3);
            groupBox1.Size = new Size(332, 166);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Gate definition";
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(215, 37);
            pictureBox1.Margin = new Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(110, 110);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // comboBoxGates
            // 
            comboBoxGates.FormattingEnabled = true;
            comboBoxGates.Location = new Point(41, 37);
            comboBoxGates.Margin = new Padding(4, 3, 4, 3);
            comboBoxGates.Name = "comboBoxGates";
            comboBoxGates.Size = new Size(140, 23);
            comboBoxGates.TabIndex = 2;
            // 
            // textBoxDescription
            // 
            textBoxDescription.Location = new Point(167, 144);
            textBoxDescription.Margin = new Padding(2);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.Size = new Size(140, 36);
            textBoxDescription.TabIndex = 22;
            // 
            // tabPageIntermediate
            // 
            tabPageIntermediate.Controls.Add(groupBox1);
            tabPageIntermediate.Location = new Point(4, 24);
            tabPageIntermediate.Margin = new Padding(4, 3, 4, 3);
            tabPageIntermediate.Name = "tabPageIntermediate";
            tabPageIntermediate.Padding = new Padding(4, 3, 4, 3);
            tabPageIntermediate.Size = new Size(340, 172);
            tabPageIntermediate.TabIndex = 0;
            tabPageIntermediate.Text = "tabPage1";
            tabPageIntermediate.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(74, 70);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 18;
            label2.Text = "Event type";
            // 
            // tabPageBasic
            // 
            tabPageBasic.Controls.Add(groupBox2);
            tabPageBasic.Location = new Point(4, 24);
            tabPageBasic.Margin = new Padding(4, 3, 4, 3);
            tabPageBasic.Name = "tabPageBasic";
            tabPageBasic.Padding = new Padding(4, 3, 4, 3);
            tabPageBasic.Size = new Size(340, 172);
            tabPageBasic.TabIndex = 1;
            tabPageBasic.Text = "tabPage2";
            tabPageBasic.UseVisualStyleBackColor = true;
            // 
            // comboBoxEventType
            // 
            comboBoxEventType.FormattingEnabled = true;
            comboBoxEventType.Location = new Point(167, 67);
            comboBoxEventType.Margin = new Padding(4, 3, 4, 3);
            comboBoxEventType.Name = "comboBoxEventType";
            comboBoxEventType.Size = new Size(140, 23);
            comboBoxEventType.TabIndex = 17;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(334, 23);
            pictureBox2.Margin = new Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(110, 110);
            pictureBox2.TabIndex = 16;
            pictureBox2.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(74, 23);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 15;
            label1.Text = "Event name:";
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(167, 20);
            textBoxName.Margin = new Padding(4, 3, 4, 3);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(140, 23);
            textBoxName.TabIndex = 14;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.Cancel;
            button2.Dock = DockStyle.Right;
            button2.Location = new Point(314, 0);
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
            button1.Location = new Point(402, 0);
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
            tabControl1.Location = new Point(77, 196);
            tabControl1.Margin = new Padding(4, 3, 4, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(348, 200);
            tabControl1.TabIndex = 19;
            // 
            // panel1
            // 
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 433);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(490, 40);
            panel1.TabIndex = 13;
            // 
            // FormEditEvent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(490, 473);
            Controls.Add(label4);
            Controls.Add(textBoxTag);
            Controls.Add(TAG);
            Controls.Add(textBoxDescription);
            Controls.Add(label2);
            Controls.Add(comboBoxEventType);
            Controls.Add(pictureBox2);
            Controls.Add(label1);
            Controls.Add(textBoxName);
            Controls.Add(tabControl1);
            Controls.Add(panel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormEditEvent";
            Text = "FormEditEvent";
            Load += FormEditEvent_Load;
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tabPageIntermediate.ResumeLayout(false);
            tabPageBasic.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            tabControl1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.ComboBox comboBoxGates;
        private System.Windows.Forms.TabPage tabPageBasic;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.ComboBox comboBoxUnits;
        private System.Windows.Forms.Button buttonDatabase;
        public System.Windows.Forms.TextBox textBoxFrequency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}