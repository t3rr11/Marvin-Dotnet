namespace Marvin.ClanQueueServer.Models;

public class ClanScanStateModel
{
    public long ClanId { get; set; }
    public ClanScanState State { get; set; }
    public string? ScanningBy { get; set; }
    public string? AssignedScannerId { get; set; }
    public DateTime? LastScanStarted { get; set; }
    public DateTime? LastUpdated { get; set; }

    public void ResetToWaitingState()
    {
        AssignedScannerId = null;
        State = ClanScanState.WaitingForFetch;
    }
}