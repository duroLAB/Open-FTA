using System.Text;

namespace Open_FTA.forms
{
    public partial class ReportForm : Form
    {

        public String html;
        public ReportForm()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();

            webView21.NavigateToString(html);
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

        private async void toolStripButtonPrint_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.ShowPrintUI();



        }

        private async Task ExportWebViewToPdfAsync()
        {
            // Ensure CoreWebView2 is initialized
            if (webView21.CoreWebView2 == null)
            {
                await webView21.EnsureCoreWebView2Async();
            }

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
