using WPF.Lib.Api;
using WPFControls.UI.Models.DummyJson;

namespace WPFControls.UI.Services;

public sealed class DummyJsonProductService
{
    private readonly IApiClient _apiClient;

    public DummyJsonProductService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<DummyJsonProductPage> SearchAsync(
        string? query,
        int skip,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        var normalizedQuery = query?.Trim();
        var requestUri = string.IsNullOrEmpty(normalizedQuery)
            ? $"products?limit={limit}&skip={skip}"
            : $"products/search?q={Uri.EscapeDataString(normalizedQuery)}&limit={limit}&skip={skip}";

        return await _apiClient.GetAsync<DummyJsonProductPage>(requestUri, cancellationToken)
            ?? new DummyJsonProductPage();
    }
}
