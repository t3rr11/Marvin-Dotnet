using Marvin.DbAccess.Services;

namespace Marvin.DbAccess.Tests;

public class BroadcastsDbAccessTests : IClassFixture<BroadcastsDbAccess>
{
    private readonly BroadcastsDbAccess _broadcastsDbAccess;

    public BroadcastsDbAccessTests(BroadcastsDbAccess broadcastsDbAccess)
    {
        _broadcastsDbAccess = broadcastsDbAccess;
    }

    [Fact]
    public async Task GetAllDestinyUserBroadcasts()
    {
        try
        {
            var data = await _broadcastsDbAccess.GetAllDestinyUserBroadcastsAsync(default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
    
    [Fact]
    public async Task GetAllClanBroadcasts()
    {
        try
        {
            var data = await _broadcastsDbAccess.GetAllClanBroadcastsAsync(default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}