namespace Marvin.ClanScannerServer.Options;

public class BungieNetOptions
{
    public string ApiKey { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public int MaxRequestsPerSecond { get; set; }
    public int MaxConcurrentRequestsAtOnce { get; set; }
    public int RateLimitPerInterval { get; set; }
    public int RateLimitInterval { get; set; }
}