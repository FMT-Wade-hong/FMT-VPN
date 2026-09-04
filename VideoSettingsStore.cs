namespace FeimaoTunnel;

internal sealed class VideoSettingsStore
{
    private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FMTClient", "video-page.txt");
    public string Load() { try { return Normalize(File.ReadAllText(_path)); } catch { return ""; } }
    public bool Save(string value)
    {
        var page = Normalize(value);
        if (page.Length == 0 || page.Contains("..") || page.Contains('?') || page.Contains('#') || Uri.TryCreate(page, UriKind.Absolute, out _)) return false;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!); File.WriteAllText(_path, page); return true;
    }

    public static string Normalize(string value) => value.Trim().Replace('\\', '/').Trim('/');
}
