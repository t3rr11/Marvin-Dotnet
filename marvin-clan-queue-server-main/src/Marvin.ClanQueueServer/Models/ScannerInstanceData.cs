using Marvin.Hub.Messaging.Models;

namespace Marvin.ClanQueueServer.Models;

public class ScannerInstanceData
{
    public string ConnectionId { get; set; }
    public ServerRunMode ServerRunMode { get; set; }
}