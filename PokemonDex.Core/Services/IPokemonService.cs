using PokemonDex.Core.Models;

namespace PokemonDex.Core.Services;

public interface IPokemonService
{
    Task<PokemonPage> SearchAsync(
        PokemonSearchCriteria criteria,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
}
