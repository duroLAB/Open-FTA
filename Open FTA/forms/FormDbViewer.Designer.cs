namespace Open_FTA.forms
{
    partial class FormDbViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDbViewer));
            tabControlMain = new TabControl();
            tabPage1 = new TabPage();
            dataGridView1 = new DataGridView();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            tabPage2 = new TabPage();
            dataGridView2 = new DataGridView();
            toolStrip2 = new ToolStrip();
            toolStripButtonAddFrequency = new ToolStripButton();
            toolStripButtonEditFrequency = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            panelButtons = new Panel();
            button2 = new Button();
            button1 = new Button();
            tabControlMain.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            toolStrip1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            toolStrip2.SuspendLayout();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPage1);
            tabControlMain.Controls.Add(tabPage2);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Location = new Point(0, 0);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(797, 476);
            tabControlMain.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(dataGridView1);
            tabPage1.Controls.Add(toolStrip1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(789, 448);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Reference Table";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 28);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(783, 417);
            dataGridView1.TabIndex = 1;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.MouseDoubleClick += dataGridView1_MouseDoubleClick;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton4 });
            toolStrip1.Location = new Point(3, 3);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(783, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(23, 22);
            toolStripButton1.Text = "toolStripButtonReferenceAdd";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(23, 22);
            toolStripButton2.Text = "toolStripButton2";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(23, 22);
            toolStripButton4.Text = "toolStripButton_DEL_REF";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(dataGridView2);
            tabPage2.Controls.Add(toolStrip2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(789, 448);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Reliability Database";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Dock = DockStyle.Fill;
            dataGridView2.Location = new Point(3, 28);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(783, 417);
            dataGridView2.TabIndex = 1;
            // 
            // toolStrip2
            // 
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripButtonAddFrequency, toolStripButtonEditFrequency, toolStripButton3 });
            toolStrip2.Location = new Point(3, 3);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(783, 25);
            toolStrip2.TabIndex = 0;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButtonAddFrequency
            // 
            toolStripButtonAddFrequency.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonAddFrequency.Image = (Image)resources.GetObject("toolStripButtonAddFrequency.Image");
            toolStripButtonAddFrequency.ImageTransparentColor = Color.Magenta;
            toolStripButtonAddFrequency.Name = "toolStripButtonAddFrequency";
            toolStripButtonAddFrequency.Size = new Size(23, 22);
            toolStripButtonAddFrequency.Text = "toolStripButton3";
            toolStripButtonAddFrequency.Click += toolStripButtonAddFrequency_Click;
            // 
            // toolStripButtonEditFrequency
            // 
            toolStripButtonEditFrequency.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonEditFrequency.Image = (Image)resources.GetObject("toolStripButtonEditFrequency.Image");
            toolStripButtonEditFrequency.ImageTransparentColor = Color.Magenta;
            toolStripButtonEditFrequency.Name = "toolStripButtonEditFrequency";
            toolStripButtonEditFrequency.Size = new Size(23, 22);
            toolStripButtonEditFrequency.Text = "toolStripButton4";
            toolStripButtonEditFrequency.Click += toolStripButtonEditFrequency_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(23, 22);
            toolStripButton3.Text = "toolStripButton_REL_DEL";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(button2);
            panelButtons.Controls.Add(button1);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 476);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(797, 34);
            panelButtons.TabIndex = 1;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.Cancel;
            button2.Location = new Point(634, 6);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 1;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.OK;
            button1.Location = new Point(715, 6);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "Select";
            button1.UseVisualStyleBackColor = true;
            // 
            // FormDbViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(797, 510);
            Controls.Add(tabControlMain);
            Controls.Add(panelButtons);
            Name = "FormDbViewer";
            Text = "FormDbViewer";
            Load += FormDbViewer_Load;
            tabControlMain.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButtonAddFrequency;
        private ToolStripButton toolStripButtonEditFrequency;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;
        public Panel panelButtons;
        private Button button2;
        private Button button1;
        public TabPage tabPage1;
        public TabPage tabPage2;
        public TabControl tabControlMain;
        public DataGridView dataGridView1;
        public DataGridView dataGridView2;
    }
}