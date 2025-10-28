namespace Open_FTA.engine
{
    partial class FormEditReference
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
            txtTitle = new TextBox();
            txtPublisher = new TextBox();
            txtAuthors = new TextBox();
            txtYear = new TextBox();
            buttonSave = new Button();
            button1 = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            SuspendLayout();
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(103, 12);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(473, 23);
            txtTitle.TabIndex = 0;
            // 
            // txtPublisher
            // 
            txtPublisher.Location = new Point(103, 70);
            txtPublisher.Name = "txtPublisher";
            txtPublisher.Size = new Size(473, 23);
            txtPublisher.TabIndex = 1;
            // 
            // txtAuthors
            // 
            txtAuthors.Location = new Point(103, 41);
            txtAuthors.Name = "txtAuthors";
            txtAuthors.Size = new Size(473, 23);
            txtAuthors.TabIndex = 2;
            // 
            // txtYear
            // 
            txtYear.Location = new Point(103, 99);
            txtYear.Name = "txtYear";
            txtYear.Size = new Size(60, 23);
            txtYear.TabIndex = 3;
            // 
            // buttonSave
            // 
            buttonSave.DialogResult = DialogResult.OK;
            buttonSave.Location = new Point(501, 127);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 4;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.Cancel;
            button1.Location = new Point(420, 127);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 5;
            button1.Text = "Cancel";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(24, 15);
            label1.Name = "label1";
            label1.Size = new Size(32, 15);
            label1.TabIndex = 6;
            label1.Text = "Title:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(24, 44);
            label2.Name = "label2";
            label2.Size = new Size(52, 15);
            label2.TabIndex = 7;
            label2.Text = "Authors:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(24, 73);
            label3.Name = "label3";
            label3.Size = new Size(59, 15);
            label3.TabIndex = 8;
            label3.Text = "Publisher:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(24, 102);
            label4.Name = "label4";
            label4.Size = new Size(32, 15);
            label4.TabIndex = 9;
            label4.Text = "Year:";
            // 
            // FormEditReference
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(583, 166);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(buttonSave);
            Controls.Add(txtYear);
            Controls.Add(txtAuthors);
            Controls.Add(txtPublisher);
            Controls.Add(txtTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormEditReference";
            Text = "FormEditReference";
            Load += FormEditReference_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public TextBox txtTitle;
        public TextBox txtPublisher;
        public TextBox txtAuthors;
        public TextBox txtYear;
        private Button buttonSave;
        private Button button1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
    }
}