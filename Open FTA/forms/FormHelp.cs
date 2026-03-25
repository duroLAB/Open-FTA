using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Open_FTA.forms
{
    public partial class FormHelp : Form
    {
        private string helpFolder;
        public FormHelp()
        {
            InitializeComponent();
            helpFolder = Path.Combine(Application.StartupPath, "Help");
            var pipeline = new MarkdownPipelineBuilder()
             .UseAdvancedExtensions()        // toto aktivuje veľa vecí vrátane atribútov
             .UseGenericAttributes()         // a toto explicitne zapne {#kotva}
             .Build();
        }

        private async void HelpForm_Load(object sender, EventArgs e)
        {
            // Naplniť strom kapitol
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(new TreeNode("Main Window Overview") { Tag = "MainWindowOverview.md" });
            treeView1.Nodes.Add(new TreeNode("Settings") { Tag = "Settings.md" });

            treeView1.ExpandAll();

            // inicializácia WebView2
            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.NavigationStarting += Core_NavigationStarting;


            await LoadMarkdown("Main Window Overview.md");
        }

        private async void Core_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            string uri = e.Uri;


            if (uri.Contains(".md"))
            {
                e.Cancel = true;

                // Získať názov súboru
                var fullPart = uri.Split('#')[0];
                string file = Path.GetFileName(fullPart);

                // Získať anchor (sekciu)
                string anchor = null;
                if (uri.Contains("#"))
                    anchor = uri.Split('#').Last();

                await LoadMarkdown(file, anchor);
            }
        }

        private async Task LoadMarkdown(string fileName, string anchor = null)
        {
            string path = Path.Combine(helpFolder, fileName);
            if (!File.Exists(path)) return;

            string markdown = File.ReadAllText(path);


            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)   // ← generuje id="moja-sekcia"
                .UseGenericAttributes()                             // ← podporuje {#vlastnaKotva}               
                .Build();

            string htmlContent = Markdig.Markdown.ToHtml(markdown, pipeline);

            string finalHtml = $@"
<html>
<head>
<meta charset='UTF-8'>
<base href='file:///{helpFolder.Replace("\\", "/")}/' />
<style>
body {{ font-family:'Segoe UI', sans-serif; margin:20px; line-height:1.6; }}
h1,h2,h3 {{ color:#2a72d6; }}
img {{ max-width:100%; border-radius:6px; }}
code {{ background:#f0f0f0; padding:3px 5px; border-radius:4px; }}
pre {{ background:#272822; color:#f8f8f2; padding:15px; border-radius:6px; overflow-x:auto; }}
</style>
</head>
<body>
{htmlContent}
</body>
</html>";

            string tempFile = Path.Combine(Path.GetTempPath(), "help_temp.html");
            File.WriteAllText(tempFile, finalHtml);

            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.Navigate(tempFile);

            // Po načítaní skoč na anchor
            if (!string.IsNullOrEmpty(anchor))
            {
                webView21.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    try
                    {
                        webView21.CoreWebView2.ExecuteScriptAsync(
                            $"location.hash = '#{anchor}';");
                    }
                    catch { }
                };
            }
        }

        private async void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string file = e.Node.Tag.ToString();
            await LoadMarkdown(file);
        }
    }
}
