using System.Text;

namespace FeimaoTunnel;

internal sealed class TunnelConfig
{
    public string Name { get; set; } = "新隧道";
    public string PrivateKey { get; set; } = "";
    public string Address { get; set; } = "";
    public string Dns { get; set; } = "";
    public string PublicKey { get; set; } = "";
    public string AllowedIPs { get; set; } = "0.0.0.0/0";
    public string Endpoint { get; set; } = "";
    public int Keepalive { get; set; } = 25;

    public static TunnelConfig Parse(string name, string text)
    {
        var config = new TunnelConfig { Name = name };
        var section = "";
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(';')) continue;
            if (line.StartsWith('[') && line.EndsWith(']')) { section = line[1..^1]; continue; }
            var split = line.IndexOf('=');
            if (split < 1) continue;
            var key = line[..split].Trim();
            var value = line[(split + 1)..].Trim();
            if (section.Equals("Interface", StringComparison.OrdinalIgnoreCase))
            {
                if (key.Equals("PrivateKey", StringComparison.OrdinalIgnoreCase)) config.PrivateKey = value;
                else if (key.Equals("Address", StringComparison.OrdinalIgnoreCase)) config.Address = value;
                else if (key.Equals("DNS", StringComparison.OrdinalIgnoreCase)) config.Dns = value;
            }
            else if (section.Equals("Peer", StringComparison.OrdinalIgnoreCase))
            {
                if (key.Equals("PublicKey", StringComparison.OrdinalIgnoreCase)) config.PublicKey = value;
                else if (key.Equals("AllowedIPs", StringComparison.OrdinalIgnoreCase)) config.AllowedIPs = value;
                else if (key.Equals("Endpoint", StringComparison.OrdinalIgnoreCase)) config.Endpoint = value;
                else if (key.Equals("PersistentKeepalive", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var n)) config.Keepalive = n;
            }
        }
        return config;
    }

    public string Serialize()
    {
        var b = new StringBuilder().AppendLine("[Interface]").AppendLine($"PrivateKey = {PrivateKey}").AppendLine($"Address = {Address}");
        if (!string.IsNullOrWhiteSpace(Dns)) b.AppendLine($"DNS = {Dns}");
        b.AppendLine().AppendLine("[Peer]").AppendLine($"PublicKey = {PublicKey}").AppendLine($"AllowedIPs = {AllowedIPs}").AppendLine($"Endpoint = {Endpoint}");
        if (Keepalive > 0) b.AppendLine($"PersistentKeepalive = {Keepalive}");
        return b.ToString();
    }
}
