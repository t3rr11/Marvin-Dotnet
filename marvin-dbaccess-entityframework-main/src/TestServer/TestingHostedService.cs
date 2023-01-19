using DotNetBungieAPI.HashReferences;
using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;
using Marvin.DbAccess.EntityFramework.Models.ClanBroadcasts;
using Marvin.DbAccess.EntityFramework.Models.DestinyUsers;
using Marvin.DbAccess.EntityFramework.Models.Guilds;
using Marvin.DbAccess.EntityFramework.Models.UserBroadcasts;
using Marvin.DbAccess.EntityFramework.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TestServer;

public class TestingHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextCaller _dbContextCaller;

    public TestingHostedService(
        IServiceProvider serviceProvider,
        IDbContextCaller dbContextCaller)
    {
        _serviceProvider = serviceProvider;
        _dbContextCaller = dbContextCaller;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var entries = await _dbContextCaller.GetFromDbContext(
            async (IDbContext<GuildDbModel> db, CancellationToken ct) =>
            {
                var result = await db
                    .Set1
                    .Where(x => x.GuildId == 886500502060302357)
                    .Select(x => x.Clans)
                    .Take(1)
                    .ToListAsync(ct);
                return result;
            },
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}