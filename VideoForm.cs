using Microsoft.Web.WebView2.WinForms;

namespace FeimaoTunnel;

internal sealed class VideoForm : Form
{
    private readonly WebView2 _browser = new() { Dock = DockStyle.Fill };

    public VideoForm(string page, Icon? icon)
    {
        Text = $"FMT VPN 影像 — {page}"; Icon = icon; StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(800, 500); ClientSize = new Size(1100, 700); BackColor = Color.FromArgb(24, 36, 55);
        Controls.Add(_browser);
        Shown += async (_, _) => await OpenAsync(page);
    }

    private async Task OpenAsync(string page)
    {
        try { await _browser.EnsureCoreWebView2Async(); _browser.Source = new Uri($"http://10.0.1.20:8889/{Uri.EscapeDataString(page)}/"); }
        catch (Exception ex) { MessageBox.Show(this, $"無法開啟 VPN 影像介面：{ex.Message}", "FMT VPN 影像", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
