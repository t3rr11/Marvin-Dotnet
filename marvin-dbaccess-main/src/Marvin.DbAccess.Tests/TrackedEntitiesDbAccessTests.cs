using Marvin.DbAccess.Services;

namespace Marvin.DbAccess.Tests;

public class TrackedEntitiesDbAccessTests : IClassFixture<TrackedEntitiesDbAccess>
{
    private readonly TrackedEntitiesDbAccess _trackedEntitiesDbAccess;

    public TrackedEntitiesDbAccessTests(TrackedEntitiesDbAccess trackedEntitiesDbAccess)
    {
        _trackedEntitiesDbAccess = trackedEntitiesDbAccess;
    }

    [Fact]
    public async Task GetTrackedMetricHashes()
    {
        try
        {
            var data = await _trackedEntitiesDbAccess.GetTrackedMetricHashesCachedAsync(default);
        }
        catch (Exception exception)
        {
            Assert.Fail(exception.Message);
        }
    }

    [Fact]
    public async Task GetTrackedProgressionHashes()
    {
        try
        {
            var data = await _trackedEntitiesDbAccess.GetTrackedProgressionHashesCachedAsync(default);
        }
        catch (Exception exception)
        {
            Assert.Fail(exception.Message);
        }
    }

    [Fact]
    public async Task GetProfileTrackedCollectibleHashes()
    {
        try
        {
            var data = await _trackedEntitiesDbAccess.GetProfileTrackedCollectibleHashesCachedAsync(default);
        }
        catch (Exception exception)
        {
            Assert.Fail(exception.Message);
        }
    }

    [Fact]
    public async Task GetTrackedProfileRecordHashes()
    {
        try
        {
            var data = await _trackedEntitiesDbAccess.GetTrackedProfileRecordHashesCachedAsync(default);
        }
        catch (Exception exception)
        {
            Assert.Fail(exception.Message);
        }
    }

    [Fact]
    public async Task GetTrackedCharacterRecordHashes()
    {
        try
        {
            var data = await _trackedEntitiesDbAccess.GetTrackedCharacterRecordHashesCachedAsync(default);
        }
        catch (Exception exception)
        {
            Assert.Fail(exception.Message);
        }
    }
}