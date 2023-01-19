using Marvin.DbAccess.Models.User;

namespace Marvin.DbAccess.Services.Interfaces;

public interface IUserAccountDbAccess
{
    Task UpsertUserAccountAsync(
        UserAccountDbModel userAccountDbModel,
        CancellationToken cancellationToken);
}