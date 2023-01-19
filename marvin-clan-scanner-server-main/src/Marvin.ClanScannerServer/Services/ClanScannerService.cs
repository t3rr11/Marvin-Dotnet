using Marvin.ClanScannerServer.Models;
using Marvin.ClanScannerServer.Services.Scanning.Scanners;
using Marvin.DbAccess.Models.Clan;

namespace Marvin.ClanScannerServer.Services;

public class ClanScannerService
{
    private readonly ClanScanner _clanScanner;
    private readonly FirstTimeClanScanner _firstTimeClanScanner;

    public ClanScannerService(
        ClanScanner clanScanner,
        FirstTimeClanScanner firstTimeClanScanner)
    {
        _clanScanner = clanScanner;
        _firstTimeClanScanner = firstTimeClanScanner;
    }

    public async Task ScanClanAssumedFirstTimeAsync(
        FirstTimeScanEntry firstTimeScanEntry,
        CancellationToken cancellationToken)
    {
        var clanUpdateContext = new ClanUpdateContext()
        {
            ClanId = firstTimeScanEntry.ClanId,
            MembersOnline = 0,
            MembersScanned = 0,
            MembersTotal = 0
        };

        await _firstTimeClanScanner.Scan(clanUpdateContext, firstTimeScanEntry, cancellationToken);
    }

    public async Task ScanClanAsUsualAsync(
        long clanId,
        CancellationToken cancellationToken)
    {
        var clanUpdateContext = new ClanUpdateContext
        {
            ClanId = clanId,
            MembersOnline = 0,
            MembersScanned = 0,
            MembersTotal = 0
        };

        await _clanScanner.Scan(clanUpdateContext, null, cancellationToken);
    }
}