using Marvin.DbAccess.Services;

namespace Marvin.DbAccess.Tests;

public class DestinyProfileDbAccessTests : IClassFixture<DestinyProfileDbAccess>
{
    private readonly DestinyProfileDbAccess _destinyProfileDbAccess;

    public DestinyProfileDbAccessTests(DestinyProfileDbAccess destinyProfileDbAccess)
    {
        _destinyProfileDbAccess = destinyProfileDbAccess;
    }

    [Fact]
    public async Task GetDestinyProfileByMembershipId()
    {
        try
        {
            var data = await _destinyProfileDbAccess.GetDestinyProfileByMembershipId(4611686018483306402, default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task GetClanMemberReferences()
    {
        try
        {
            var data = await _destinyProfileDbAccess.GetClanMemberReferencesAsync(4394229, default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task SearchProfilesByName()
    {
        try
        {
            var data = await _destinyProfileDbAccess.SearchProfilesByNameAsync("megl", default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}