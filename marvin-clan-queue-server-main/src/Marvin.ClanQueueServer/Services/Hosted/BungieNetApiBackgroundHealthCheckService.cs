using DotNetBungieAPI.Models.Common;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanQueueServer.Services.Interfaces;
using Marvin.HostedServices.Extensions;

namespace Marvin.ClanQueueServer.Services.Hosted;

public class BungieNetApiBackgroundHealthCheckService : PeriodicBackgroundService, IBungieNetHealthCheck
{
    private readonly ILogger<BungieNetApiBackgroundHealthCheckService> _logger;
    private readonly IBungieClient _bungieClient;

    public bool IsLive { get; private set; }
    public CoreSettingsConfiguration? LatestSettingsResponse { get; private set; }
    public DateTime? LatestSettingsResponseDate { get; private set; }
    public event Func<bool, Task>? StatusChanged;

    public BungieNetApiBackgroundHealthCheckService(
        ILogger<BungieNetApiBackgroundHealthCheckService> logger,
        IBungieClient bungieClient) : base(logger)
    {
        _logger = logger;
        _bungieClient = bungieClient;
    }

    protected override Task BeforeExecutionAsync(CancellationToken stoppingToken)
    {
        ChangeTimerSafe(TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    protected override async Task OnTimerExecuted(CancellationToken cancellationToken)
    {
        try
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            var settings = await _bungieClient
                .ApiAccess
                .Misc
                .GetCommonSettings(cts.Token);

            if (!settings.IsSuccessfulResponseCode)
            {
                await SetStatusAndReport(status: false);
                return;
            }

            LatestSettingsResponse = settings.Response;
            LatestSettingsResponseDate = DateTime.UtcNow;

            var settingsConfiguration = settings.Response;

            if (!settingsConfiguration.Systems.TryGetValue("D2Profiles", out var profilesSystem))
            {
                await SetStatusAndReport(status: false);
                return;
            }

            await SetStatusAndReport(status: profilesSystem.IsEnabled);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Encountered error while checking for bungie.net api availability");
            await SetStatusAndReport(status: false);
        }
    }

    private async Task SetStatusAndReport(bool status)
    {
        if (IsLive != status)
        {
            IsLive = status;
            if (StatusChanged != null)
            {
                await StatusChanged(IsLive);
            }
        }
    }
}