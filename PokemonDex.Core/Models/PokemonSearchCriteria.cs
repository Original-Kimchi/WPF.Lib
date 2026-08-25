namespace PokemonDex.Core.Models;

public sealed record PokemonSearchCriteria(
    string Query,
    string? TypeName,
    int? GenerationId);
