using Marvin.HostedServices.Extensions;

namespace Marvin.ClanScannerServer.Services.Hosted;

public class BackgroundBungieApiMetricReporter : PeriodicBackgroundService
{
    private readonly ILogger<BackgroundBungieApiMetricReporter> _logger;
    private readonly BungieNetApiCallLogger _bungieNetApiCallLogger;

    private readonly TimeSpan ApiReportInterval = TimeSpan.FromMinutes(10);

    public BackgroundBungieApiMetricReporter(
        ILogger<BackgroundBungieApiMetricReporter> logger,
        BungieNetApiCallLogger bungieNetApiCallLogger) : base(logger)
    {
        _logger = logger;
        _bungieNetApiCallLogger = bungieNetApiCallLogger;
    }

    protected override Task BeforeExecutionAsync(CancellationToken stoppingToken)
    {
        ChangeTimerSafe(ApiReportInterval);
        return Task.CompletedTask;
    }

    protected override async Task OnTimerExecuted(CancellationToken cancellationToken)
    {
        var snapshot = _bungieNetApiCallLogger.TakeSnapshotAndClear();

        _logger.LogInformation("Api metrics for {ApiReportInterval}: {@Snapshot}", 
            ApiReportInterval.ToString(),
            snapshot);
    }
}