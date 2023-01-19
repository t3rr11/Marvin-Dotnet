using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny.Definitions.PresentationNodes;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.DefinitionProvider.Postgresql.Tests.Fixtures;

namespace Marvin.DefinitionProvider.Postgresql.Tests;

public class DefinitionUpdateTest : IClassFixture<DefinitionProviderFixture>
{
    private readonly DefinitionProviderFixture _definitionProviderFixture;
    private readonly IBungieClient _bungieClient;

    public DefinitionUpdateTest(
        DefinitionProviderFixture definitionProviderFixture,
        IBungieClient bungieClient)
    {
        _definitionProviderFixture = definitionProviderFixture;
        _bungieClient = bungieClient;
    }

    //[Fact]
    public async Task UpdateTest()
    {
        await _definitionProviderFixture.DefinitionProvider.Update();
    }

    //[Fact]
    public async Task LoadDefsTest()
    {
        await _definitionProviderFixture.DefinitionProvider.ReadToRepository(_bungieClient.Repository);
    }

    [Fact]
    public async Task ReadDefinition()
    {
        var def = await _definitionProviderFixture
            .DefinitionProvider
            .LoadDefinition<DestinyPresentationNodeDefinition>(
                616318467,
                BungieLocales.EN);
    }
}