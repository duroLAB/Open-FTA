namespace Open_FTA.forms
{
    partial class FormEditReliability
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
            textBoxR_Title = new TextBox();
            textBoxR_Val = new TextBox();
            comboBoxMetricUnits = new ComboBox();
            comboBox2 = new ComboBox();
            button1 = new Button();
            buttonSave = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // textBoxR_Title
            // 
            textBoxR_Title.Location = new Point(95, 22);
            textBoxR_Title.Name = "textBoxR_Title";
            textBoxR_Title.Size = new Size(405, 23);
            textBoxR_Title.TabIndex = 0;
            // 
            // textBoxR_Val
            // 
            textBoxR_Val.Location = new Point(94, 59);
            textBoxR_Val.Name = "textBoxR_Val";
            textBoxR_Val.Size = new Size(100, 23);
            textBoxR_Val.TabIndex = 1;
            // 
            // comboBoxMetricUnits
            // 
            comboBoxMetricUnits.FormattingEnabled = true;
            comboBoxMetricUnits.Location = new Point(202, 59);
            comboBoxMetricUnits.Name = "comboBoxMetricUnits";
            comboBoxMetricUnits.Size = new Size(71, 23);
            comboBoxMetricUnits.TabIndex = 2;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(95, 98);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(406, 23);
            comboBox2.TabIndex = 3;
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.Cancel;
            button1.Location = new Point(344, 143);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 7;
            button1.Text = "Cancel";
            button1.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            buttonSave.DialogResult = DialogResult.OK;
            buttonSave.Location = new Point(425, 143);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 6;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 25);
            label1.Name = "label1";
            label1.Size = new Size(70, 15);
            label1.TabIndex = 8;
            label1.Text = "Description:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 62);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 9;
            label2.Text = "Value:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(15, 101);
            label3.Name = "label3";
            label3.Size = new Size(62, 15);
            label3.TabIndex = 10;
            label3.Text = "Reference:";
            // 
            // FormEditReliability
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(527, 180);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(buttonSave);
            Controls.Add(comboBox2);
            Controls.Add(comboBoxMetricUnits);
            Controls.Add(textBoxR_Val);
            Controls.Add(textBoxR_Title);
            Name = "FormEditReliability";
            Text = "FormEditReliability";
            Load += FormEditReliability_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button1;
        private Button buttonSave;
        public TextBox textBoxR_Title;
        public TextBox textBoxR_Val;
        public ComboBox comboBoxMetricUnits;
        public ComboBox comboBox2;
        private Label label1;
        private Label label2;
        private Label label3;
    }
}