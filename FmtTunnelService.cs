using System.Diagnostics;

namespace FeimaoTunnel;

internal static class FmtTunnelService
{
    private static string SafeName(TunnelConfig config) => string.Concat(config.Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    private static string TunnelName(TunnelConfig config) => $"feimao-{SafeName(config)}";

    private static string? FindExe()
    {
        var installed = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WireGuard", "wireguard.exe");
        if (File.Exists(installed)) return installed;
        return (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(folder => Path.Combine(folder.Trim(), "wireguard.exe"))
            .FirstOrDefault(File.Exists);
    }

    public static async Task<(bool Ok, string Message)> SetStateAsync(TunnelConfig config, bool connect)
    {
        var exe = FindExe();
        if (exe is null) return (false, "找不到 FMT 網路元件，請先安裝 FMT 用戶端元件。");
        var tunnelName = TunnelName(config);
        var configPath = Path.Combine(Path.GetTempPath(), $"{tunnelName}.conf");
        await File.WriteAllTextAsync(configPath, config.Serialize());
        var args = connect ? $"/installtunnelservice \"{configPath}\"" : $"/uninstalltunnelservice \"{tunnelName}\"";
        try
        {
            using var process = Process.Start(new ProcessStartInfo(exe, args) { UseShellExecute = false, CreateNoWindow = true });
            if (process is null) return (false, "無法啟動 FMT 網路元件。");
            await process.WaitForExitAsync();
            return process.ExitCode == 0 ? (true, connect ? "隧道已連線。" : "隧道已中斷。") : (false, $"FMT 網路元件回傳錯誤碼 {process.ExitCode}。");
        }
        catch (System.ComponentModel.Win32Exception) { return (false, "FMT 飛貓科技 VPN 客戶端沒有足夠的系統權限。"); }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static bool IsConnected(TunnelConfig config)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("sc.exe", $"query \"WireGuardTunnel${TunnelName(config)}\"") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true });
            if (process is null) return false;
            var output = process.StandardOutput.ReadToEnd(); process.WaitForExit();
            return process.ExitCode == 0 && (output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase) || output.Contains("STOP_PENDING", StringComparison.OrdinalIgnoreCase));
        }
        catch { return false; }
    }

    public static bool IsInstalled(TunnelConfig config)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("sc.exe", $"query \"WireGuardTunnel${TunnelName(config)}\"") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true });
            if (process is null) return false;
            process.StandardOutput.ReadToEnd(); process.WaitForExit(); return process.ExitCode == 0;
        }
        catch { return false; }
    }

    public static async Task<(bool Ok, string Message)> DisconnectAllAsync(IEnumerable<TunnelConfig> configs)
    {
        foreach (var config in configs.Where(IsInstalled))
        {
            var result = await SetStateAsync(config, false);
            if (!result.Ok) return result;
        }
        return (true, "所有 FMT VPN 連線均已中斷。");
    }
}
