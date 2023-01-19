using Marvin.DbAccess.Models.Guild;

namespace Marvin.DbAccess.Services.Interfaces;

public interface IGuildDbAccess
{
    Task<IEnumerable<GuildDbModel>> GetGuildsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Upserts all guild data with supplied db model
    /// </summary>
    /// <param name="guildDbModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpsertGuildAsync(
        GuildDbModel guildDbModel,
        CancellationToken cancellationToken = default);
}