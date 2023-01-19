using Discord;
using Discord.Interactions;
using DotNetBungieAPI.Models.Destiny.Definitions.Seasons;
using DotNetBungieAPI.Service.Abstractions;

namespace Marvin.Bot.Commands.AutocompleteHandlers;

public class SeasonAutocompleteHandler : AutocompleteHandler
{
    private readonly IBungieClient _bungieClient;

    public SeasonAutocompleteHandler(IBungieClient bungieClient)
    {
        _bungieClient = bungieClient;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var seasonName = (string)autocompleteInteraction.Data.Options.First(x => x.Focused).Value;

        var searchResults = _bungieClient
            .Repository
            .GetAll<DestinySeasonDefinition>()
            .Where(x => x.DisplayProperties.Name.Contains(seasonName, StringComparison.InvariantCultureIgnoreCase))
            .Take(20);

        var results = searchResults
            .Where(x => x.DisplayProperties.Name.Length > 0)
            .Select(x => new AutocompleteResult(x.DisplayProperties.Name, x.Hash.ToString()));

        return !results.Any() ? AutocompletionResult.FromSuccess() : AutocompletionResult.FromSuccess(results);
    }
}