using DotNetBungieAPI.Models;

namespace Marvin.ClanScannerServer.Models;

public class BungieNetApiMetrics
{
    public ulong SuccessfulRequests { get; set; }

    public ulong TotalErrors { get; set; }
    public ulong TimeoutsAmount { get; set; }

    public Dictionary<PlatformErrorCodes, ulong> PerErrorRequests { get; set; }

    public ulong TotalRequestsMade { get; set; }

    public double RequestSpeed { get; set; }

    public double ErrorPercentage { get; set; }

    public TimeSpan Uptime { get; set; }
}