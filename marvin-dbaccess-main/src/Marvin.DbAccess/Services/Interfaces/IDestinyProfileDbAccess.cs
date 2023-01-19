using Marvin.DbAccess.Models.User;

namespace Marvin.DbAccess.Services.Interfaces;

public interface IDestinyProfileDbAccess
{
    ValueTask<DestinyProfile?> GetDestinyProfileByMembershipId(
        long membershipId,
        CancellationToken cancellationToken);

    Task UpsertDestinyProfileData(
        DestinyProfile destinyProfile,
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<DestinyProfileClanMemberReference>> GetClanMemberReferencesAsync(
        long clanId,
        CancellationToken cancellationToken);

    Task UpdateDestinyProfileClanId(
        long? clanId,
        long membershipId,
        CancellationToken cancellationToken);

    Task<IEnumerable<ProfileSearchEntry>> SearchProfilesByNameAsync(
        string input,
        CancellationToken cancellationToken);

    Task RemoveDestinyUserFromDbAsync(
        long membershipId,
        CancellationToken cancellationToken);
}