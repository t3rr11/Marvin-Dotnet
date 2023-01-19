namespace Marvin.ClanQueueServer.Models;

public class ClanTrackerReport
{
    public Dictionary<string, ClanTrackerDetails> CurrentScanners { get; } = new();
}