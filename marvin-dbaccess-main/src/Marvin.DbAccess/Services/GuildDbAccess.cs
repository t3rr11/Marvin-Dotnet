using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class GuildDbAccess : PostgresDbAccessBase, IGuildDbAccess
{
    public GuildDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<GuildDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    private const string GetGuildsQuery = @"
SELECT 
    guild_id,
    guild_name,
    owner_id,
    owner_avatar,
    is_tracking,
    clans,
    joined_on,
    broadcasts_config
FROM guild;";

    public async Task<IEnumerable<GuildDbModel>> GetGuildsAsync(CancellationToken cancellationToken)
    {
        return await QueryAsync<GuildDbModel>(GetGuildsQuery, null, cancellationToken);
    }

    private const string UpsertGuildQuery = @"
INSERT INTO guild (guild_id, guild_name, owner_id, owner_avatar, is_tracking, clans, joined_on, broadcasts_config, announcements_config)
VALUES (@GuildId, @GuildName, @OwnerId, @OwnerAvatar, @IsTracking, CAST(@Clans as json), @JoinedOn, CAST(@BroadcastsConfig as json), CAST(@AnnouncementsConfig as json))
ON CONFLICT (guild_id)
    DO UPDATE SET
        guild_name = @GuildName,
        owner_id = @OwnerId,
        owner_avatar = @OwnerAvatar,
        is_tracking = @IsTracking,
        clans = CAST(@Clans as json),
        joined_on = @JoinedOn,
        broadcasts_config = CAST(@BroadcastsConfig as json),
        announcements_config = CAST(@AnnouncementsConfig as json)";

    public async Task UpsertGuildAsync(
        GuildDbModel guildDbModel,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            UpsertGuildQuery,
            new
            {
                GuildId = guildDbModel.GuildId,
                GuildName = guildDbModel.GuildName,
                OwnerId = guildDbModel.OwnerId.GetValueOrDefault().ToString(),
                OwnerAvatar = guildDbModel.AvatarHash,
                IsTracking = guildDbModel.IsTracking,
                Clans = guildDbModel.ClanIds,
                JoinedOn = guildDbModel.JoinedOn,
                BroadcastsConfig = guildDbModel.BroadcastsConfig,
                AnnouncementsConfig = guildDbModel.AnnouncementsConfig
            },
            cancellationToken);
    }
}