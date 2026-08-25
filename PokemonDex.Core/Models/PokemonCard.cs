namespace PokemonDex.Core.Models;

public sealed record PokemonCard(int Id, string Name, string KoreanName, string ArtworkUrl, IReadOnlyList<string> Types);
