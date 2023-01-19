using DotNetBungieAPI.Models.Destiny;

namespace Marvin.DefinitionProvider.Postgresql;

public class PostgresqlDefinitionProviderConfiguration
{
    public string ConnectionString { get; set; }
    public bool CleanUpOldManifestsAfterUpdate { get; set; } = true;
    public int MaxAmountOfLeftoverManifests { get; set; } = 1;
    public bool AutoUpdateOnStartup { get; set; }

    public List<DefinitionsEnum> DefinitionsToLoad { get; set; } = new();
}