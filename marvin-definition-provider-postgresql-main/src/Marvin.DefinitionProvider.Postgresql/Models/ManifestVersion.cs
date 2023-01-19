using DotNetBungieAPI.Models.Destiny.Config;

namespace Marvin.DefinitionProvider.Postgresql.Models;

public class ManifestVersion
{
    public string Version { get; set; }
    public DestinyManifest DestinyManifest { get; set; }
    public DateTime DownloadDate { get; set; }
}