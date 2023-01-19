namespace Marvin.ClanQueueServer.Services.Interfaces;

public interface IBungieNetManifestUpdater
{
    bool IsUpdating { get; }
    event Func<Task> ManifestUpdateStarted;
}