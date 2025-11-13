namespace Open_FTA.forms
{
    partial class FormSettings
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
            propertyGrid1 = new PropertyGrid();
            panel1 = new Panel();
            panel2 = new Panel();
            buttonCancel = new Button();
            buttonOK = new Button();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(0, 0);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(649, 572);
            propertyGrid1.TabIndex = 0;
            propertyGrid1.Click += propertyGrid1_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(panel2);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 572);
            panel1.Name = "panel1";
            panel1.Size = new Size(649, 38);
            panel1.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.Controls.Add(buttonCancel);
            panel2.Controls.Add(buttonOK);
            panel2.Dock = DockStyle.Right;
            panel2.Location = new Point(425, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(224, 38);
            panel2.TabIndex = 0;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(56, 8);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(137, 8);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 0;
            buttonOK.Text = "Ok";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(649, 610);
            Controls.Add(propertyGrid1);
            Controls.Add(panel1);
            Name = "FormSettings";
            Text = "Application settings";
            Load += FormSettings_Load;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private PropertyGrid propertyGrid1;
        private Panel panel1;
        private Panel panel2;
        private Button buttonCancel;
        private Button buttonOK;
    }
}