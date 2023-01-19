using Marvin.ClanScannerServer.Models;
using Marvin.Hub.Messaging.Models;

namespace Marvin.ClanScannerServer.Options;

public class InstanceMetadataOptions
{
    public ServerRunMode RunMode { get; set; }
    public int Number { get; set; }
}