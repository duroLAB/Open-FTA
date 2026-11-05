using Open_FTA.Properties;
using System.Text;

namespace Open_FTA.forms
{
    public partial class ReportForm : Form
    {

        public String html;
        public ReportForm()
        {
            InitializeComponent();
            this.Icon = Resources.Main;
            this.Load += Form1_Load;
            AppIcons.Apply(toolStripButtonSave, AppIcon.Save, this);
            AppIcons.Apply(toolStripButtonWord, AppIcon.Word, this);
            AppIcons.Apply(toolStripButtonExportToPDF, AppIcon.Export, this);
            AppIcons.Apply(toolStripButtonPrint, AppIcon.Print, this);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            bool ready = await WebViewSetup.TryInitializeAsync(webView21, this, "the report",
                ("Open the report in the web browser", OpenInBrowser));
            if (!ready)
            {
                Close();
                return;
            }

            webView21.NavigateToString(html);
        }

        /// <summary>Report as an HTML file in the default web browser (when WebView2 is not available).</summary>
        private void OpenInBrowser()
        {
            string path = Path.Combine(Path.GetTempPath(), "OpenFTA-report.html");
            File.WriteAllText(path, html, Encoding.UTF8);
            WebViewSetup.OpenInShell(path);
        }


        private void ReportForm_Load(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {


            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save HTML Document",
                Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                DefaultExt = "html",
                FileName = "document.html"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {


                try
                {
                    File.WriteAllText(saveFileDialog.FileName, html);
                    MessageBox.Show("HTML file was saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while saving the file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButtonWord_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save Word Document",
                Filter = "Word files (*.doc)|*.doc|All files (*.*)|*.*",
                DefaultExt = "doc",
                FileName = "document.doc"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    // Save as .doc with UTF-8 encoding
                    File.WriteAllText(saveFileDialog.FileName, html, Encoding.UTF8);

                    MessageBox.Show("Word document was saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while saving the file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButtonPrint_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2?.ShowPrintUI();



        }

        private async Task ExportWebViewToPdfAsync()
        {
            if (webView21.CoreWebView2 == null)
                return;

            // Show Save File dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = "document.pdf",
                Title = "Save PDF"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    bool result = await webView21.CoreWebView2.PrintToPdfAsync(filePath);

                    if (result)
                    {
                        MessageBox.Show("PDF was saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to print to PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while exporting:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void toolStripButtonExportToPDF_Click(object sender, EventArgs e)
        {
            await ExportWebViewToPdfAsync();
        }
    }
}
