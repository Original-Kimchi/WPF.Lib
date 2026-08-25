using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PokemonDex.Core.Models;
using PokemonDex.Core.Services;

namespace PokemonDex.Infrastructure;

public sealed class PokeApiPokemonService : IPokemonService
{
    private readonly HttpClient _httpClient;

    public PokeApiPokemonService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<PokemonPage> SearchAsync(
        PokemonSearchCriteria criteria,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            var card = await FindAsync(criteria.Query, cancellationToken);
            if (card is null || !await MatchesFiltersAsync(card.Id, criteria, cancellationToken))
            {
                return new PokemonPage([], 0, 0, limit);
            }

            return new PokemonPage([card], 1, 0, limit);
        }

        if (criteria.TypeName is not null || criteria.GenerationId is not null)
        {
            var candidates = await GetFilteredCandidatesAsync(criteria, cancellationToken);
            var selected = candidates.OrderBy(item => item.Key).Skip(offset).Take(limit).ToArray();
            var filteredCards = await Task.WhenAll(selected.Select(item =>
                GetCardAsync(item.Key.ToString(), cancellationToken)));
            return new PokemonPage(filteredCards, candidates.Count, offset, limit);
        }

        var page = await _httpClient.GetFromJsonAsync<PokemonListResponse>(
            $"pokemon?offset={offset}&limit={limit}", cancellationToken) ?? new();
        var cards = await Task.WhenAll(page.Results.Select(item => GetCardAsync(item.Name, cancellationToken)));
        return new PokemonPage(cards, page.Count, offset, limit);
    }

    private async Task<PokemonCard?> FindAsync(string query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        if (normalizedQuery.Length == 0) return null;
        try
        {
            return await GetCardAsync(normalizedQuery, cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private async Task<bool> MatchesFiltersAsync(
        int pokemonId,
        PokemonSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var candidates = await GetFilteredCandidatesAsync(criteria, cancellationToken);
        return candidates.Count == 0
            ? criteria.TypeName is null && criteria.GenerationId is null
            : candidates.ContainsKey(pokemonId);
    }

    private async Task<Dictionary<int, string>> GetFilteredCandidatesAsync(
        PokemonSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        Dictionary<int, string>? candidates = null;

        if (criteria.TypeName is not null)
        {
            var type = await _httpClient.GetFromJsonAsync<PokemonTypeResponse>(
                $"type/{Uri.EscapeDataString(criteria.TypeName)}", cancellationToken) ?? new();
            candidates = ToCandidateDictionary(type.Pokemon.Select(item => item.Pokemon));
        }

        if (criteria.GenerationId is int generationId)
        {
            var generation = await _httpClient.GetFromJsonAsync<PokemonGenerationResponse>(
                $"generation/{generationId}", cancellationToken) ?? new();
            var generationCandidates = ToCandidateDictionary(generation.PokemonSpecies);
            candidates = candidates is null
                ? generationCandidates
                : candidates.Where(item => generationCandidates.ContainsKey(item.Key))
                    .ToDictionary(item => item.Key, item => item.Value);
        }

        return candidates ?? [];
    }

    private static Dictionary<int, string> ToCandidateDictionary(IEnumerable<NamedResource> resources)
    {
        return resources
            .Select(resource => (Resource: resource, Id: GetResourceId(resource.Url)))
            .Where(item => item.Id is > 0 and < 10000)
            .GroupBy(item => item.Id!.Value)
            .ToDictionary(group => group.Key, group => group.First().Resource.Name);
    }

    private static int? GetResourceId(string url)
    {
        var segment = url.TrimEnd('/').Split('/').LastOrDefault();
        return int.TryParse(segment, out var id) ? id : null;
    }

    private async Task<PokemonCard> GetCardAsync(string idOrName, CancellationToken cancellationToken)
    {
        var pokemon = await _httpClient.GetFromJsonAsync<PokemonResponse>(
            $"pokemon/{Uri.EscapeDataString(idOrName)}", cancellationToken)
            ?? throw new InvalidOperationException("PokéAPI가 빈 포켓몬 응답을 반환했습니다.");
        var species = await _httpClient.GetFromJsonAsync<PokemonSpeciesResponse>(
            $"pokemon-species/{pokemon.Id}", cancellationToken);
        var koreanName = species?.Names.FirstOrDefault(item => item.Language.Name == "ko")?.Name ?? pokemon.Name;
        var artworkUrl = pokemon.Sprites.Other.OfficialArtwork.FrontDefault ?? pokemon.Sprites.FrontDefault ?? string.Empty;
        var types = pokemon.Types.OrderBy(item => item.Slot).Select(item => TranslateType(item.Type.Name)).ToArray();
        return new PokemonCard(pokemon.Id, pokemon.Name, koreanName, artworkUrl, types);
    }

    private static string TranslateType(string type) => type switch
    {
        "normal" => "노말", "fire" => "불꽃", "water" => "물", "electric" => "전기",
        "grass" => "풀", "ice" => "얼음", "fighting" => "격투", "poison" => "독",
        "ground" => "땅", "flying" => "비행", "psychic" => "에스퍼", "bug" => "벌레",
        "rock" => "바위", "ghost" => "고스트", "dragon" => "드래곤", "dark" => "악",
        "steel" => "강철", "fairy" => "페어리", _ => type
    };

    private sealed class PokemonListResponse { public int Count { get; init; } public List<NamedResource> Results { get; init; } = []; }
    private sealed class PokemonResponse { public int Id { get; init; } public string Name { get; init; } = string.Empty; public PokemonSprites Sprites { get; init; } = new(); public List<PokemonTypeSlot> Types { get; init; } = []; }
    private sealed class PokemonSpeciesResponse { public List<LocalizedName> Names { get; init; } = []; }
    private sealed class NamedResource { public string Name { get; init; } = string.Empty; public string Url { get; init; } = string.Empty; }
    private sealed class PokemonTypeResponse { public List<PokemonTypeEntry> Pokemon { get; init; } = []; }
    private sealed class PokemonTypeEntry { public NamedResource Pokemon { get; init; } = new(); }
    private sealed class PokemonGenerationResponse { [JsonPropertyName("pokemon_species")] public List<NamedResource> PokemonSpecies { get; init; } = []; }
    private sealed class LocalizedName { public string Name { get; init; } = string.Empty; public NamedResource Language { get; init; } = new(); }
    private sealed class PokemonTypeSlot { public int Slot { get; init; } public NamedResource Type { get; init; } = new(); }
    private sealed class PokemonSprites { [JsonPropertyName("front_default")] public string? FrontDefault { get; init; } public OtherSprites Other { get; init; } = new(); }
    private sealed class OtherSprites { [JsonPropertyName("official-artwork")] public ArtworkSprites OfficialArtwork { get; init; } = new(); }
    private sealed class ArtworkSprites { [JsonPropertyName("front_default")] public string? FrontDefault { get; init; } }
}
