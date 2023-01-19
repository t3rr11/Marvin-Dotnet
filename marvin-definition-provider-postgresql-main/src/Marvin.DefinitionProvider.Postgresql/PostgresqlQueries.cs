namespace Marvin.DefinitionProvider.Postgresql;

internal static class PostgresqlQueries
{
    internal const string CreateManifestTable = @"
CREATE TABLE IF NOT EXISTS destiny_manifest (
    definition_type text NOT NULL,
    lang text,
    manifest_version text,
    definitions jsonb,
    PRIMARY KEY (definition_type, lang, manifest_version)
)";

    internal const string CreateManifestVersionTable = @"
CREATE TABLE IF NOT EXISTS destiny_manifest_version (
    version text,
    manifest text,
    download_date timestamp,
    PRIMARY KEY (version)
)";

    internal const string GetAvailableManifestVersions = @"
SELECT 
    version, 
    manifest,
    download_date 
FROM destiny_manifest_version";

    internal const string DeleteManifestVersion = @"
DELETE FROM 
    destiny_manifest_version 
WHERE version = @Version";

    internal const string DeleteManifestFiles = @"
DELETE FROM 
    destiny_manifest 
WHERE manifest_version = @Version";

    internal const string CheckIfManifestExists = @"
SELECT 1 
FROM destiny_manifest_version 
WHERE version = @Version";

    internal const string BulkGetDefinitions = @"
SELECT 
    definition_type,
    definitions
FROM 
    destiny_manifest
WHERE 
    lang = @Lang AND manifest_version = @ManifestVersion";

    internal const string InsertDefinitions = @"
INSERT INTO 
    destiny_manifest (definition_type, lang, manifest_version, definitions)
VALUES (@DefinitionType, @Lang, @ManifestVersion, CAST(@Definitions as jsonb))
ON CONFLICT DO NOTHING";

    internal const string InsertManifestVersion = @"
INSERT INTO 
    destiny_manifest_version (version, manifest, download_date)
VALUES (@Version, @Manifest, @DownloadDate)
ON CONFLICT DO NOTHING";

    internal const string GetDefinition = @"
SELECT destiny_manifest.definitions -> @Hash as d
FROM destiny_manifest
WHERE definition_type = @DefType
  AND manifest_version = @ManifestVer
  AND lang = @Lang";
}