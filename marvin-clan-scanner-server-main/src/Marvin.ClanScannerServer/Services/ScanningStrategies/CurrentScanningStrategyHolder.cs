namespace Marvin.ClanScannerServer.Services.ScanningStrategies;

public class CurrentScanningStrategyHolder
{
    public GeneralScanningStrategy GeneralScanningStrategy { get; }
    public PatreonScanningStrategy PatreonScanningStrategy { get; }
    public BaseScanningStrategy? CurrentScanningStrategy { get; set; }
    
    public CurrentScanningStrategyHolder(
        GeneralScanningStrategy generalScanningStrategy,
        PatreonScanningStrategy patreonScanningStrategy)
    {
        GeneralScanningStrategy = generalScanningStrategy;
        PatreonScanningStrategy = patreonScanningStrategy;
    }
}