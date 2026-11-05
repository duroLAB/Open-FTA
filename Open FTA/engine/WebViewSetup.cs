using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

/// <summary>
/// Common start of the WebView2 controls (report, help).
/// User data are kept in %LocalAppData%\OpenFTA\WebView2, so WebView2 works also when the application
/// is in a read-only folder (the default folder is next to the exe). When the Microsoft Edge WebView2
/// Runtime is missing, the user gets an explanation with a link to the download page (and optionally
/// another way to see the content) instead of an unhandled exception.
/// </summary>
internal static class WebViewSetup
{
    /// <summary>Microsoft download page of the Evergreen WebView2 Runtime (bootstrapper).</summary>
    public const string RuntimeDownloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";

    private static Task<CoreWebView2Environment>? _environment;

    private static string UserDataFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFTA", "WebView2");

    /// <summary>
    /// Initializes the control. Returns false (after telling the user) when WebView2 cannot start;
    /// <paramref name="alternative"/> is offered as another way to see the content, e.g. in the web browser.
    /// </summary>
    public static async Task<bool> TryInitializeAsync(WebView2 view, IWin32Window owner, string content,
                                                      (string Text, Action Action)? alternative = null)
    {
        try
        {
            _environment ??= CoreWebView2Environment.CreateAsync(null, UserDataFolder);
            await view.EnsureCoreWebView2Async(await _environment);
            return true;
        }
        catch (Exception ex)
        {
            _environment = null;   // try again next time, e.g. after the runtime was installed
            ShowStartError(owner, ex, content, alternative);
            return false;
        }
    }

    private static bool IsRuntimeMissing(Exception ex) =>
        ex is WebView2RuntimeNotFoundException
        || ex is FileNotFoundException
        || ex.InnerException is FileNotFoundException
        || ex.HResult == unchecked((int)0x80070002);

    private static void ShowStartError(IWin32Window owner, Exception ex, string content, (string Text, Action Action)? alternative)
    {
        bool missing = IsRuntimeMissing(ex);
        var page = new TaskDialogPage
        {
            Caption = "Open FTA",
            Icon = TaskDialogIcon.Warning,
            AllowCancel = true,
            Heading = missing ? "Microsoft Edge WebView2 Runtime is missing" : "WebView2 could not be started",
            Text = missing
                ? $"Open FTA uses the Microsoft Edge WebView2 Runtime to show {content}. It is part of Windows 11 " +
                  "and of updated Windows 10, but it was not found on this computer."
                : $"Open FTA uses the Microsoft Edge WebView2 Runtime to show {content}, but it could not be started.",
            Expander = new TaskDialogExpander($"{ex.GetType().Name}: {ex.Message}") { CollapsedButtonText = "Details" },
        };

        var download = new TaskDialogCommandLinkButton(
            missing ? "Download the WebView2 Runtime" : "Download or repair the WebView2 Runtime",
            "Opens the Microsoft download page. Install the Evergreen Bootstrapper and try again.");
        page.Buttons.Add(download);

        TaskDialogCommandLinkButton? other = null;
        if (alternative is { } alt)
        {
            other = new TaskDialogCommandLinkButton(alt.Text);
            page.Buttons.Add(other);
        }
        page.Buttons.Add(TaskDialogButton.Close);

        TaskDialogButton result = TaskDialog.ShowDialog(owner, page);
        if (result == download)
            OpenInShell(RuntimeDownloadUrl);
        else if (other != null && result == other)
            alternative!.Value.Action();
    }

    /// <summary>Opens a web page or a file with the default application.</summary>
    public static void OpenInShell(string target)
    {
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show("It could not be opened: " + ex.Message, "Open FTA"); }
    }
}
