namespace Marvin.ClanScannerServer.Services;

public class ScanMetricsService
{
    public int ClansScanned { get; set; }
    public int MembersScanned { get; set; }
    public DateTime Started { get; set; } = DateTime.UtcNow;

    public ScanMetricsService()
    {
    }
}