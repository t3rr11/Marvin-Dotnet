using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class UserAccountDbAccess : PostgresDbAccessBase, IUserAccountDbAccess
{
    public UserAccountDbAccess(
        IOptions<DatabaseOptions> databaseOptions, 
        ILogger<UserAccountDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    private const string UpsertUserAccountQuery = @"
INSERT INTO registered_users (user_id, username, membership_id, platform, created_at)
VALUES (@UserId, @Username, @MembershipId, @Platform, @CreatedAt)
ON CONFLICT (user_id)
DO UPDATE SET
    username = @Username,
    membership_id = @MembershipId,
    platform = @Platform,
    created_at = @CreatedAt";
    
    public async Task UpsertUserAccountAsync(
        UserAccountDbModel userAccountDbModel, 
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            UpsertUserAccountQuery,
            new
            {
                UserId = userAccountDbModel.DiscordId,
                Username = userAccountDbModel.Username,
                MembershipId = userAccountDbModel.MembershipId,
                Platform = userAccountDbModel.Platform,
                CreatedAt = userAccountDbModel.CreatedAt
            },
            cancellationToken);
    }
}