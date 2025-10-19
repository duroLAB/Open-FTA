namespace Open_FTA.forms
{
    partial class ReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportForm));
            toolStrip1 = new ToolStrip();
            toolStripButtonSave = new ToolStripButton();
            toolStripButtonWord = new ToolStripButton();
            toolStripButtonExportToPDF = new ToolStripButton();
            toolStripButtonPrint = new ToolStripButton();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButtonSave, toolStripButtonWord, toolStripButtonExportToPDF, toolStripButtonPrint });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(800, 31);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonSave
            // 
            toolStripButtonSave.Image = (Image)resources.GetObject("toolStripButtonSave.Image");
            toolStripButtonSave.ImageTransparentColor = Color.Magenta;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new Size(59, 28);
            toolStripButtonSave.Text = "Save";
            toolStripButtonSave.Click += toolStripButton1_Click;
            // 
            // toolStripButtonWord
            // 
            toolStripButtonWord.Image = Properties.Resources.ExportToWord;
            toolStripButtonWord.ImageTransparentColor = Color.Magenta;
            toolStripButtonWord.Name = "toolStripButtonWord";
            toolStripButtonWord.Size = new Size(113, 28);
            toolStripButtonWord.Text = "Export to word";
            toolStripButtonWord.Click += toolStripButtonWord_Click;
            // 
            // toolStripButtonExportToPDF
            // 
            toolStripButtonExportToPDF.Image = Properties.Resources.ExportToWord;
            toolStripButtonExportToPDF.ImageTransparentColor = Color.Magenta;
            toolStripButtonExportToPDF.Name = "toolStripButtonExportToPDF";
            toolStripButtonExportToPDF.Size = new Size(104, 28);
            toolStripButtonExportToPDF.Text = "Export to pdf";
            toolStripButtonExportToPDF.Click += toolStripButtonExportToPDF_Click;
            // 
            // toolStripButtonPrint
            // 
            toolStripButtonPrint.Image = Properties.Resources.PrintDocument;
            toolStripButtonPrint.ImageTransparentColor = Color.Magenta;
            toolStripButtonPrint.Name = "toolStripButtonPrint";
            toolStripButtonPrint.Size = new Size(60, 28);
            toolStripButtonPrint.Text = "Print";
            toolStripButtonPrint.Click += toolStripButtonPrint_Click;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.Location = new Point(0, 31);
            webView21.Name = "webView21";
            webView21.Size = new Size(800, 419);
            webView21.TabIndex = 1;
            webView21.ZoomFactor = 1D;
            // 
            // ReportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webView21);
            Controls.Add(toolStrip1);
            Name = "ReportForm";
            Text = "ReportForm";
            Load += ReportForm_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        public Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private ToolStripButton toolStripButtonSave;
        private ToolStripButton toolStripButtonWord;
        private ToolStripButton toolStripButtonPrint;
        private ToolStripButton toolStripButtonExportToPDF;
    }
}