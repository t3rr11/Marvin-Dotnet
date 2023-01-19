using Marvin.DbAccess.Models.Clan;
using Marvin.Hub.Messaging.Models;

namespace Marvin.Hub.Messaging.Interfaces;

public interface IClanQueueHub
{
    /// <summary>
    ///     Registers scanner instance and gets the mode it should scan in
    /// </summary>
    /// <returns></returns>
    Task<ServerRunMode> RegisterServerIdAsync();

    /// <summary>
    ///     Deregisters scanner instance, releasing all of its clans
    /// </summary>
    /// <returns></returns>
    Task DeregisterServerIdAsync();

    /// <summary>
    ///     Returns a list of scans for connection
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<long>> GetClansForScanningAsync();

    /// <summary>
    ///     Returns a list of first-time scans for connection
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<FirstTimeScanEntry>> GetClansForFirstTimeScanningAsync();

    /// <summary>
    ///     Marks clans as successfully scanned
    /// </summary>
    /// <param name="clanIds">List of Destiny 2 clan Ids</param>
    /// <returns></returns>
    Task SuccessfullyScannedClansAsync(long[] clanIds);

    /// <summary>
    ///     Clan scan had started
    /// </summary>
    /// <param name="clanId"></param>
    /// <returns></returns>
    Task StartedClanScanAsync(long clanId);

    /// <summary>
    ///     Clan first time scan was finished
    /// </summary>
    /// <param name="clanId"></param>
    /// <returns></returns>
    Task FirstTimeScanDoneAsync(long clanId);
}