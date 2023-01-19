using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;
using Marvin.DbAccess.EntityFramework.Models.ClanBroadcasts;
using Marvin.DbAccess.EntityFramework.Models.Clans;
using Marvin.DbAccess.EntityFramework.Models.DestinyUsers;
using Marvin.DbAccess.EntityFramework.Models.Guilds;
using Marvin.DbAccess.EntityFramework.Models.UserBroadcasts;
using Marvin.DbAccess.EntityFramework.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Marvin.Bot.Extensions;

public static class DbContextCallerExtensions
{
    public static async Task<string?> GetDestinyUserDisplayNameAsync(
        this IDbContextCaller dbContextCaller,
        long membershipId,
        CancellationToken cancellationToken)
    {
        return await dbContextCaller.GetFromDbContext<DestinyUserDbModel, string?>(
            async (context, ct) =>
            {
                return await context
                    .Set1
                    .Where(x => x.MembershipId == membershipId)
                    .Select(x => x.DisplayName)
                    .FirstOrDefaultAsync(ct);
            }, cancellationToken);
    }

    public static async Task<ClanDbModel?> GetClanAsync(
        this IDbContextCaller dbContextCaller,
        long clanId,
        CancellationToken cancellationToken)
    {
        return await dbContextCaller.GetFromDbContext<ClanDbModel, ClanDbModel?>(
            async (context, ct) =>
            {
                return await context
                    .Set1
                    .Where(x => x.ClanId == clanId)
                    .FirstOrDefaultAsync(ct);
            }, cancellationToken);
    }

    public static async Task<ulong?> GetGuildBroadcastChannelAsync(
        this IDbContextCaller dbContextCaller,
        ulong guildId,
        CancellationToken cancellationToken)
    {
        return await dbContextCaller.GetFromDbContext<GuildDbModel, ulong?>(async (context, ct) =>
        {
            var channelId = await context
                .Set1
                .Where(x => x.GuildId == guildId)
                .Select(x => x.BroadcastsConfig.ChannelId)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrEmpty(channelId))
                return null;

            if (!ulong.TryParse(channelId, out var parsedChannelId))
            {
                return null;
            }

            return parsedChannelId;
        }, cancellationToken);
    }

    public static async Task<List<UserBroadcastDbModel>> GetAllUnannouncedUserBroadcasts(
        this IDbContextCaller dbContextCaller,
        CancellationToken cancellationToken)
    {
        return await dbContextCaller.GetFromDbContext<UserBroadcastDbModel, List<UserBroadcastDbModel>>(
            async (db, ct) => { return await db.Set1.Where(x => x.WasAnnounced == false).ToListAsync(ct); },
            cancellationToken);
    }
    
    public static async Task<List<ClanBroadcastDbModel>> GetAllUnannouncedClanBroadcasts(
        this IDbContextCaller dbContextCaller,
        CancellationToken cancellationToken)
    {
        return await dbContextCaller.GetFromDbContext<ClanBroadcastDbModel, List<ClanBroadcastDbModel>>(
            async (db, ct) => { return await db.Set1.Where(x => x.WasAnnounced == false).ToListAsync(ct); },
            cancellationToken);
    }

    public static async Task MarkUserBroadcastSent(
        this IDbContextCaller dbContextCaller, 
        UserBroadcastDbModel broadcastDbModel,
        CancellationToken cancellationToken)
    {
        await dbContextCaller.ExecuteWithinDbContext<UserBroadcastDbModel>(async (db, ct) =>
        {
            broadcastDbModel.WasAnnounced = true;
            db.Set1.Attach(broadcastDbModel);
            db.Set1.Entry(broadcastDbModel).Property(x => x.WasAnnounced).IsModified = true;
            await db.Context.SaveChangesAsync(ct);
        }, cancellationToken);
    }
    
    public static async Task MarkClanBroadcastSent(
        this IDbContextCaller dbContextCaller, 
        ClanBroadcastDbModel broadcastDbModel,
        CancellationToken cancellationToken)
    {
        await dbContextCaller.ExecuteWithinDbContext<ClanBroadcastDbModel>(async (db, ct) =>
        {
            broadcastDbModel.WasAnnounced = true;
            db.Set1.Attach(broadcastDbModel);
            db.Set1.Entry(broadcastDbModel).Property(x => x.WasAnnounced).IsModified = true;
            await db.Context.SaveChangesAsync(ct);
        }, cancellationToken);
    }
}