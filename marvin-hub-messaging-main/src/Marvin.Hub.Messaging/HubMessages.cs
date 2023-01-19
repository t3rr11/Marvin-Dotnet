namespace Marvin.Hub.Messaging;

public static class HubMessages
{
    public static class ServerMethods
    {
        public const string RegisterServerId = nameof(RegisterServerId);
        public const string DeregisterServerId = nameof(DeregisterServerId);
        public const string GetClansForScanning = nameof(GetClansForScanning);
        public const string GetClansForFirstTimeScanning = nameof(GetClansForFirstTimeScanning);
        public const string SuccessfullyScannedClans = nameof(SuccessfullyScannedClans);
        public const string FirstTimeScanDone = nameof(FirstTimeScanDone);
        public const string StartedClanScan = nameof(StartedClanScan);
    }

    public static class ServerEvents
    {
        public const string ClanScanAborted = nameof(ClanScanAborted);
        public const string AbortAllClanScans = nameof(AbortAllClanScans);
        public const string ManifestUpdated = nameof(ManifestUpdated);
    }
}