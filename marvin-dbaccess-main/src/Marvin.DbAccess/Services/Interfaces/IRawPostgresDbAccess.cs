using System.Runtime.CompilerServices;

namespace Marvin.DbAccess.Services.Interfaces;

public interface IRawPostgresDbAccess
{
    ValueTask ExecuteAsync(
        string query,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? methodName = null,
        bool logErrorParameters = true);

    ValueTask<IEnumerable<T>> QueryAsync<T>(
        string query,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? methodName = null,
        bool logErrorParameters = true);

    ValueTask<T?> QueryFirstAsync<T>(
        string query,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? methodName = null,
        bool logErrorParameters = true);
}