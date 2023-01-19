using Marvin.DbAccess.Services;

namespace Marvin.DbAccess.Tests;

public class ClanDbAccessTests : IClassFixture<ClansDbAccess>
{
    private readonly ClansDbAccess _clansDbAccess;

    public ClanDbAccessTests(ClansDbAccess clansDbAccess)
    {
        _clansDbAccess = clansDbAccess;
    }

    [Fact]
    public async Task GetClanBroadcastSettings()
    {
        try
        {
            var settings = await _clansDbAccess.GetAllLinkedDiscordGuilds(4394229, default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task GetClan()
    {
        try
        {
            var data = await _clansDbAccess.GetClan(4394229, default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task GetClanNames()
    {
        try
        {
            var data = await _clansDbAccess.GetClanNamesAsync(new List<long>() { 4394229 }, default);
            var data2 = await _clansDbAccess.GetClanNamesAsync(new List<long>() { 4394229, 2603670 }, default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task GetClanUserNames()
    {
        try
        {
            var data = await _clansDbAccess.GetClanUserNamesAsync(4394229, default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task GetClansForFirstTimeScanning()
    {
        try
        {
            var data = await _clansDbAccess.GetClansForFirstTimeScanning(default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task GetGeneralClanIdsForScanning()
    {
        try
        {
            var data = await _clansDbAccess.GetGeneralClanIdsForScanning(default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact]
    public async Task GetPatreonClanIdsForScanning()
    {
        try
        {
            var data = await _clansDbAccess.GetPatreonClanIdsForScanning(default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}