using Marvin.ClanQueueServer.Services;
using Marvin.DbAccess.Models.Clan;

namespace Marvin.ClanQueueServer.Models;

public class ClanFirstTimeScanStateModel
{
    public FirstTimeScanEntry ScanEntry { get; set; }
    public ClanScanState State { get; set; }
    public string? AssignedScannerId { get; set; }
    public string? ScannedBy { get; set; }
    public DateTime? FetchedDate { get; set; }

    public bool IsOutdatedOrInvalid(
        DateTime currentTime, 
        ClanScanningTrackerService clanScanningTrackerService)
    {
        if (FetchedDate.HasValue && (currentTime - FetchedDate.Value).TotalMinutes > 5)
        {
            return true;
        }

        if (AssignedScannerId is null && ScannedBy is null)
        {
            return true;
        }
        
        

        return clanScanningTrackerService.IsScannerValid(AssignedScannerId);
    }
    
}