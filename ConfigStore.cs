using System.Text.Json;

namespace FeimaoTunnel;

internal sealed class ConfigStore
{
    private readonly string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FMTClient", "Configurations");
    private readonly string _legacyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FeimaoTunnel", "tunnels.json");

    public List<TunnelConfig> Load()
    {
        Directory.CreateDirectory(_folder);
        var configs = Directory.EnumerateFiles(_folder, "*.conf").Select(TryLoad).Where(c => c is not null).Cast<TunnelConfig>().ToList();
        if (configs.Count > 0 || !File.Exists(_legacyPath)) return configs;
        try { configs = JsonSerializer.Deserialize<List<TunnelConfig>>(File.ReadAllText(_legacyPath)) ?? []; Save(configs); return configs; }
        catch { return []; }
    }

    public void Save(IEnumerable<TunnelConfig> configs) { Directory.CreateDirectory(_folder); foreach (var config in configs) File.WriteAllText(PathFor(config.Name), config.Serialize()); }
    public void Delete(TunnelConfig config) { var path = PathFor(config.Name); if (File.Exists(path)) File.Delete(path); }
    private TunnelConfig? TryLoad(string path) { try { return TunnelConfig.Parse(Path.GetFileNameWithoutExtension(path), File.ReadAllText(path)); } catch { return null; } }
    private string PathFor(string name) { var safe = string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim(); return Path.Combine(_folder, (string.IsNullOrWhiteSpace(safe) ? "FMT-Connection" : safe) + ".conf"); }
}
