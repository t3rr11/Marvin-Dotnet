using Marvin.Hub.Messaging.Models;

namespace Marvin.ClanQueueServer.Models;

public class ClanTrackerDetails
{
    public int ClanAmount { get; set; }
    public ServerRunMode ServerRunMode { get; set; }
    public Dictionary<ClanScanState, int> ClansAmountPerState { get; set; } = new();
}