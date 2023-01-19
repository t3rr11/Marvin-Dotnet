using Marvin.DbAccess.Services;

namespace Marvin.DbAccess.Tests;

public class GuildDbAccessTests : IClassFixture<GuildDbAccess>
{
    private readonly GuildDbAccess _guildDbAccess;

    public GuildDbAccessTests(GuildDbAccess guildDbAccess)
    {
        _guildDbAccess = guildDbAccess;
    }

    [Fact]
    public async Task GetGuilds()
    {
        try
        {
            var data = await _guildDbAccess.GetGuildsAsync(default);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}