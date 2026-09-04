namespace FeimaoTunnel;

internal sealed class MainForm : Form
{
    private readonly ConfigStore _store = new();
    private readonly List<TunnelConfig> _configs;
    private readonly ComboBox _profiles = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly Label _statusIcon = new() { Text = "●", AutoSize = true, Font = new Font("Segoe UI", 22), ForeColor = Color.Gray };
    private readonly Label _statusText = new() { Text = "尚未連線", AutoSize = true, Font = new Font("Microsoft JhengHei UI", 15, FontStyle.Bold) };
    private readonly Button _connect = new() { Text = "連線", Width = 160, Height = 46, Enabled = false };
    private readonly Button _import = new() { Text = "匯入 CONF", Width = 100, Height = 32 };
    private readonly Button _delete = new() { Text = "刪除", Width = 65, Height = 32, Enabled = false };
    private readonly NotifyIcon _trayIcon;
    private bool _connected;
    private bool _reallyExit;

    public MainForm()
    {
        Text = "FMT 飛貓科技 VPN 客戶端";
        Font = new Font("Microsoft JhengHei UI", 10);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(540, 300);
        BackColor = Color.FromArgb(248, 250, 252);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _configs = _store.Load();
        _trayIcon = new NotifyIcon { Icon = Icon, Text = "FMT 飛貓科技 VPN 客戶端", Visible = true };
        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("開啟 FMT 飛貓科技 VPN 客戶端", null, (_, _) => RestoreFromTray());
        trayMenu.Items.Add("結束", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = trayMenu;
        _trayIcon.DoubleClick += (_, _) => RestoreFromTray();

        var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(24, 36, 55) };
        header.Controls.Add(new PictureBox { Image = LoadBrandImage(), SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(42, 42), Location = new Point(16, 9), BackColor = Color.Transparent });
        header.Controls.Add(new Label { Text = "FMT 飛貓科技 VPN 客戶端", ForeColor = Color.White, Font = new Font("Microsoft JhengHei UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(68, 16) });
        Controls.Add(header);

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 20, 30, 16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = "連線設定", AutoSize = true, ForeColor = Color.DimGray }, 0, 0);
        var profileRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        profileRow.Controls.Add(_profiles); profileRow.Controls.Add(_import); profileRow.Controls.Add(_delete); layout.Controls.Add(profileRow, 0, 1);
        var statusRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(108, 12, 0, 0) };
        statusRow.Controls.Add(_statusIcon); statusRow.Controls.Add(_statusText); layout.Controls.Add(statusRow, 0, 2);
        var buttonRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(150, 5, 0, 0) };
        buttonRow.Controls.Add(_connect); layout.Controls.Add(buttonRow, 0, 3);
        content.Controls.Add(layout); Controls.Add(content); content.BringToFront();

        _import.Click += (_, _) => ImportConfig();
        _delete.Click += (_, _) => DeleteConfig();
        _connect.Click += async (_, _) => await ToggleConnectionAsync();
        _profiles.SelectedIndexChanged += (_, _) => RefreshProfile();
        FormClosing += OnFormClosing;
        ReloadProfiles();
    }

    private TunnelConfig? Selected => _profiles.SelectedIndex >= 0 ? _configs[_profiles.SelectedIndex] : null;
    private static Image? LoadBrandImage() { using var stream = typeof(MainForm).Assembly.GetManifestResourceStream("FMTClient.Assets.fmt-client.png"); return stream is null ? null : new Bitmap(stream); }
    private void ReloadProfiles(int selected = 0) { _profiles.Items.Clear(); _profiles.Items.AddRange(_configs.Select(c => c.Name).ToArray()); if (_profiles.Items.Count > 0) _profiles.SelectedIndex = Math.Clamp(selected, 0, _profiles.Items.Count - 1); else RefreshProfile(); }
    private void RefreshProfile() { var c = Selected; _connect.Enabled = c is not null; _delete.Enabled = c is not null && !_connected; }

    private void ImportConfig()
    {
        using var picker = new OpenFileDialog { Filter = "FMT CONF (*.conf)|*.conf", Title = "匯入 FMT 設定檔" };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var config = TunnelConfig.Parse(Path.GetFileNameWithoutExtension(picker.FileName), File.ReadAllText(picker.FileName));
            if (string.IsNullOrWhiteSpace(config.PrivateKey) || string.IsNullOrWhiteSpace(config.PublicKey) || string.IsNullOrWhiteSpace(config.Endpoint)) throw new InvalidDataException("CONF 缺少 PrivateKey、PublicKey 或 Endpoint。");
            var index = _configs.FindIndex(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0) _configs[index] = config; else { _configs.Add(config); index = _configs.Count - 1; }
            _store.Save(_configs); ReloadProfiles(index); SetStatus(false, "已匯入，尚未連線");
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "無法匯入 CONF", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DeleteConfig()
    {
        if (Selected is not { } config || _connected) return;
        if (MessageBox.Show(this, $"確定刪除「{config.Name}」及系統內保存的 CONF？", "刪除連線設定", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var index = _profiles.SelectedIndex;
        _store.Delete(config); _configs.RemoveAt(index); ReloadProfiles(Math.Max(0, index - 1));
        SetStatus(false, _configs.Count == 0 ? "尚未匯入設定" : "尚未連線");
    }

    private async Task ToggleConnectionAsync()
    {
        if (Selected is not { } config) return;
        _connect.Enabled = false; _import.Enabled = false; _delete.Enabled = false; _profiles.Enabled = false;
        SetStatus(null, _connected ? "正在中斷…" : "正在連線…");
        var result = await FmtTunnelService.SetStateAsync(config, !_connected);
        if (result.Ok) _connected = !_connected;
        SetStatus(_connected, result.Ok ? (_connected ? "已連線" : "尚未連線") : "連線失敗");
        _connect.Text = _connected ? "中斷連線" : "連線";
        _connect.Enabled = true; _import.Enabled = true; _delete.Enabled = !_connected; _profiles.Enabled = !_connected;
        if (!result.Ok) MessageBox.Show(this, result.Message, "FMT 飛貓科技 VPN 客戶端", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SetStatus(bool? connected, string text) { _statusText.Text = text; _statusIcon.ForeColor = connected switch { true => Color.FromArgb(26, 160, 93), false => Color.Gray, null => Color.FromArgb(234, 150, 32) }; }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_reallyExit) { _trayIcon.Visible = false; return; }
        e.Cancel = true; Hide();
        _trayIcon.ShowBalloonTip(2000, "FMT 飛貓科技 VPN 客戶端仍在執行", _connected ? "安全連線持續運作中。" : "可從系統匣重新開啟。", ToolTipIcon.Info);
    }

    private void RestoreFromTray() { Show(); WindowState = FormWindowState.Normal; Activate(); }
    private void ExitApplication() { _reallyExit = true; _trayIcon.Visible = false; Close(); }
}
