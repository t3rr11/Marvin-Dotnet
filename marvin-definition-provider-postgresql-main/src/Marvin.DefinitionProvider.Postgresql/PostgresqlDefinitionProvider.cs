using System.Text.Json;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Config;
using DotNetBungieAPI.Models.Destiny.Definitions.HistoricalStats;
using DotNetBungieAPI.Models.Destiny.Rendering;
using DotNetBungieAPI.Models.Extensions;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.DefinitionProvider.Postgresql.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Marvin.DefinitionProvider.Postgresql;

public class PostgresqlDefinitionProvider : IDefinitionProvider
{
    private readonly IBungieClientConfiguration _bungieClientConfiguration;
    private readonly IDotNetBungieApiHttpClient _dotNetBungieApiHttpClient;
    private readonly PostgresqlDefinitionProviderConfiguration _configuration;
    private readonly IBungieApiAccess _bungieApiAccess;
    private readonly IBungieNetJsonSerializer _bungieNetJsonSerializer;
    private readonly IDefinitionAssemblyData _definitionAssemblyData;
    private readonly ILogger<PostgresqlDefinitionProvider> _logger;
    
    private DestinyManifest? _currentLoadedManifest;

    public PostgresqlDefinitionProvider(
        IBungieClientConfiguration bungieClientConfiguration,
        IDotNetBungieApiHttpClient dotNetBungieApiHttpClient,
        PostgresqlDefinitionProviderConfiguration configuration,
        IBungieApiAccess bungieApiAccess,
        IBungieNetJsonSerializer bungieNetJsonSerializer,
        IDefinitionAssemblyData definitionAssemblyData,
        ILogger<PostgresqlDefinitionProvider> logger)
    {
        _bungieClientConfiguration = bungieClientConfiguration;
        _dotNetBungieApiHttpClient = dotNetBungieApiHttpClient;
        _configuration = configuration;
        _bungieApiAccess = bungieApiAccess;
        _bungieNetJsonSerializer = bungieNetJsonSerializer;
        _definitionAssemblyData = definitionAssemblyData;
        _logger = logger;
    }

    public void Dispose()
    {
        return;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask<T> LoadDefinition<T>(uint hash, BungieLocales locale) where T : IDestinyDefinition
    {
        await using var postgreDbConnection = new NpgsqlConnection(_configuration.ConnectionString);
        await postgreDbConnection.OpenAsync();
        var reader = await QueryAsync(
            postgreDbConnection, 
            PostgresqlQueries.GetDefinition,
            (parameters) =>
            {
                parameters.AddWithValue("Hash", hash.ToString());
                parameters.AddWithValue("DefType", DefinitionHashPointer<T>.EnumValue.ToStringFast());
                parameters.AddWithValue("ManifestVer", _currentLoadedManifest!.Version);
                parameters.AddWithValue("Lang", locale.AsString());
            });

        if (await reader.ReadAsync())
        {
            var rawValue = await reader.GetFieldValueAsync<byte[]>(0);
            return await _bungieNetJsonSerializer.DeserializeAsync<T>(rawValue);
        }

        throw new Exception("Failed to load definition from db");
    }

    public ValueTask<DestinyHistoricalStatsDefinition> LoadHistoricalStatsDefinition(string id, BungieLocales locale)
    {
        throw new NotImplementedException();
    }

    public ValueTask<string> ReadDefinitionRaw(DefinitionsEnum enumValue, uint hash, BungieLocales locale)
    {
        throw new NotImplementedException();
    }

    public ValueTask<string> ReadHistoricalStatsDefinitionRaw(string id, BungieLocales locale)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IEnumerable<DestinyManifest>> GetAvailableManifests()
    {
        var entries = await GetAvailableManifestVersionsFromDbAsync();
        return entries.Select(x => x.DestinyManifest);
    }

    public ValueTask<DestinyManifest> GetCurrentManifest()
    {
        if (_currentLoadedManifest is null)
            throw new Exception("No manifest is loaded as of now");

        return ValueTask.FromResult(_currentLoadedManifest);
    }

    public async ValueTask<bool> CheckForUpdates()
    {
        var manifest = await _bungieApiAccess.Destiny2.GetDestinyManifest();

        var loadedVersions = await GetAvailableManifestVersionsFromDbAsync();

        return !loadedVersions.Any(x => x.Version == manifest.Response.Version);
    }

    public async Task Update()
    {
        var shouldUpdate = await CheckForUpdates();
        if (shouldUpdate)
        {
            var manifest = await _bungieApiAccess.Destiny2.GetDestinyManifest();
            await DownloadAndStoreLatestManifest(manifest.Response);
            _currentLoadedManifest = manifest.Response;
            if (_configuration.CleanUpOldManifestsAfterUpdate)
            {
                await DeleteOldManifestData();
            }
        }
    }

    public async Task DeleteOldManifestData()
    {
        var availableManifests = await GetAvailableManifestVersionsFromDbAsync();
        if (availableManifests.Count <= 1)
        {
            return;
        }

        var orderedManifests = availableManifests
            .OrderByDescending(x => x.DownloadDate)
            .ToList();

        var untouchedManifests = orderedManifests.Take(_configuration.MaxAmountOfLeftoverManifests).ToList();

        foreach (var manifest in availableManifests)
        {
            if (!untouchedManifests.Any(x => x.Version == manifest.Version))
            {
                await DeleteManifestData(manifest.Version);
            }
        }
    }

    public async Task DeleteManifestData(string version)
    {
        await using var postgreDbConnection = new NpgsqlConnection(_configuration.ConnectionString);
        await postgreDbConnection.OpenAsync();
        await ExecuteQueryAsync(
            postgreDbConnection,
            PostgresqlQueries.DeleteManifestVersion,
            (parameters) => { parameters.AddWithValue("Version", version); });

        await ExecuteQueryAsync(
            postgreDbConnection,
            PostgresqlQueries.DeleteManifestFiles,
            (parameters) => { parameters.AddWithValue("Version", version); });
    }

    public async ValueTask<bool> CheckExistingManifestData(string version)
    {
        await using var postgreDbConnection = new NpgsqlConnection(_configuration.ConnectionString);
        await postgreDbConnection.OpenAsync();
        var reader = await QueryAsync(
            postgreDbConnection,
            PostgresqlQueries.CheckIfManifestExists,
            (parameters) => { parameters.AddWithValue("Version", version); });

        // If there's entry, manifest is there
        return await reader.NextResultAsync();
    }

    public Task DownloadManifestData(DestinyManifest manifestData)
    {
        throw new NotImplementedException("Can't download files, since there is no locales files for this provider");
    }

    public async Task Initialize()
    {
        await EnsureAllTablesAreCreated();
        var latestManifest = await _bungieApiAccess.Destiny2.GetDestinyManifest();

        _currentLoadedManifest = latestManifest.Response;

        if (_configuration.AutoUpdateOnStartup)
        {
            await Update();
        }
    }

    public Task ChangeManifestVersion(string version)
    {
        throw new NotImplementedException();
    }

    public async ValueTask ReadToRepository(IDestiny2DefinitionRepository repository)
    {
        if (_currentLoadedManifest is null)
        {
            throw new Exception("Load manifest first");
        }

        await using var postgreDbConnection = new NpgsqlConnection(_configuration.ConnectionString);
        await postgreDbConnection.OpenAsync();
        foreach (var locale in _bungieClientConfiguration.UsedLocales)
        {
            var reader = await QueryAsync(
                postgreDbConnection,
                PostgresqlQueries.BulkGetDefinitions,
                (parameters) =>
                {
                    parameters.AddWithValue("Lang", locale.AsString());
                    parameters.AddWithValue("ManifestVersion", _currentLoadedManifest.Version);
                });

            while (await reader.ReadAsync())
            {
                var definitionType = Enum.Parse<DefinitionsEnum>(await reader.GetFieldValueAsync<string>(0));
                var type = _definitionAssemblyData.DefinitionsToTypeMapping[definitionType].DefinitionType;
                var definitionsJson = await reader.GetFieldValueAsync<string>(1);
                var document = JsonDocument.Parse(definitionsJson);

                var definitionEnumerator = document.RootElement.EnumerateObject();
                while (definitionEnumerator.MoveNext())
                {
                    var property = definitionEnumerator.Current;
                    var rawDefinition = property.Value.GetRawText();
                    var definition = _bungieNetJsonSerializer.Deserialize(rawDefinition, type);
                    repository.AddDefinition(locale, (IDestinyDefinition)definition);
                }
            }
        }
    }

    public ValueTask<DestinyGearAssetDefinition> GetGearAssetDefinition(uint itemHash)
    {
        throw new NotImplementedException();
    }

    private async Task<List<ManifestVersion>> GetAvailableManifestVersionsFromDbAsync()
    {
        await using var postgreDbConnection = new NpgsqlConnection(_configuration.ConnectionString);
        await postgreDbConnection.OpenAsync();
        var reader = await QueryAsync(postgreDbConnection, PostgresqlQueries.GetAvailableManifestVersions);

        var versions = new List<ManifestVersion>();
        while (await reader.ReadAsync())
        {
            var version = await reader.GetFieldValueAsync<string>(0);
            var manifest =
                _bungieNetJsonSerializer.Deserialize<DestinyManifest>(await reader.GetFieldValueAsync<string>(1));
            var downloadDate = await reader.GetFieldValueAsync<DateTime>(2);

            var versionModel = new ManifestVersion()
            {
                Version = version,
                DestinyManifest = manifest,
                DownloadDate = downloadDate
            };

            versions.Add(versionModel);
        }

        return versions;
    }

    private async Task EnsureAllTablesAreCreated()
    {
        await using var postgreDbConnection = new NpgsqlConnection(_configuration.ConnectionString);
        await postgreDbConnection.OpenAsync();
        await ExecuteQueryAsync(postgreDbConnection, PostgresqlQueries.CreateManifestTable);
        await ExecuteQueryAsync(postgreDbConnection, PostgresqlQueries.CreateManifestVersionTable);
    }

    private async Task DownloadAndStoreLatestManifest(DestinyManifest latestManifest)
    {
        await using var postgreDbConnection = new NpgsqlConnection(_configuration.ConnectionString);
        await postgreDbConnection.OpenAsync();
        foreach (var locale in _bungieClientConfiguration.UsedLocales)
        {
            var strLocale = locale.AsString();
            var jsonPath = latestManifest.JsonWorldContentPaths[strLocale];

            var (stream, _) = await _dotNetBungieApiHttpClient.GetStreamFromWebSourceAsync(jsonPath);

            var jsonDocument = await JsonDocument.ParseAsync(stream);

            var definitionsEnumerator = jsonDocument.RootElement.EnumerateObject();

            while (definitionsEnumerator.MoveNext())
            {
                var definitionsNode = definitionsEnumerator.Current;

                var definitionType = definitionsNode.Name;

                if (!Enum.TryParse<DefinitionsEnum>(definitionType, out var enumValue))
                {
                    continue;
                }

                if (!_configuration.DefinitionsToLoad.Contains(enumValue))
                {
                    continue;
                }

                var definitions = definitionsNode.Value.GetRawText();

                await ExecuteQueryAsync(
                    postgreDbConnection,
                    PostgresqlQueries.InsertDefinitions,
                    (parameters) =>
                    {
                        parameters.AddWithValue("DefinitionType", definitionType);
                        parameters.AddWithValue("Lang", strLocale);
                        parameters.AddWithValue("ManifestVersion", latestManifest.Version);
                        parameters.AddWithValue("Definitions", definitions);
                    });
            }
        }

        await ExecuteQueryAsync(
            postgreDbConnection,
            PostgresqlQueries.InsertManifestVersion,
            (parameters) =>
            {
                parameters.AddWithValue("Version", latestManifest.Version);
                parameters.AddWithValue("Manifest", _bungieNetJsonSerializer.Serialize(latestManifest));
                parameters.AddWithValue("DownloadDate", DateTime.UtcNow);
            });
    }

    private async Task ExecuteQueryAsync(
        NpgsqlConnection connection,
        string query,
        Action<NpgsqlParameterCollection>? parameters = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = query;
        parameters?.Invoke(command.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<NpgsqlDataReader> QueryAsync(
        NpgsqlConnection connection,
        string query,
        Action<NpgsqlParameterCollection>? parameters = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = query;
        parameters?.Invoke(command.Parameters);
        return await command.ExecuteReaderAsync();
    }
}