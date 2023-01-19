using System.Text.Json;
using Marvin.ClanQueueServer.Models.Database;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Options;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Marvin.ClanQueueServer.Services.Hosted;

public class HostedClanFirstTimeEventHandler : BackgroundService
{
    private readonly IOptions<DatabaseOptions> _databaseOptions;
    private readonly ILogger<HostedClanFirstTimeEventHandler> _logger;
    private readonly IOptions<JsonOptions> _jsonOptions;
    private readonly ClanScanningTrackerService _clanQueueService;

    public HostedClanFirstTimeEventHandler(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<HostedClanFirstTimeEventHandler> logger,
        IOptions<JsonOptions> jsonOptions,
        ClanScanningTrackerService clanQueueService)
    {
        _databaseOptions = databaseOptions;
        _logger = logger;
        _jsonOptions = jsonOptions;
        _clanQueueService = clanQueueService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var dbConnection = new NpgsqlConnection(_databaseOptions.Value.ConnectionString);
        await dbConnection.OpenAsync(stoppingToken);

        dbConnection.Notification += OnNotificationReceived;

        await using (var cmd = new NpgsqlCommand("LISTEN datainsert;", dbConnection))
            await cmd.ExecuteNonQueryAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
            await dbConnection.WaitAsync(stoppingToken);
    }

    private void OnNotificationReceived(object obj, NpgsqlNotificationEventArgs eventArgs)
    {
        var eventData = eventArgs.Payload;
        var payload = JsonSerializer.Deserialize<PostgresqlNotificationPayload<InsertedClanToScanPayload>>(
            eventData,
            _jsonOptions.Value.SerializerOptions);

        _clanQueueService.AddNewFirstTimeScanClan(new FirstTimeScanEntry()
        {
            ChannelId = payload!.Data.ChannelId.GetValueOrDefault(),
            ClanId = payload.Data.ClanId,
            GuildId = payload.Data.GuildId
        });
    }
}