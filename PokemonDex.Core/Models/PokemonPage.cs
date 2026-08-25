namespace PokemonDex.Core.Models;

public sealed record PokemonPage(IReadOnlyList<PokemonCard> Items, int Total, int Offset, int Limit);
