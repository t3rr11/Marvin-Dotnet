namespace Marvin.ClanQueueServer.Models;

public enum ClanScanState
{
    /// <summary>
    ///     This clan is yet to be fetched
    /// </summary>
    WaitingForFetch,

    /// <summary>
    ///     This clan was already fetched but still is waiting it's turn to be scanned
    /// </summary>
    WaitingForScan,

    /// <summary>
    ///     This clan is already being scanned
    /// </summary>
    CurrentlyScanning
}