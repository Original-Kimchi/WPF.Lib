using Microsoft.Extensions.DependencyInjection;

namespace WPF.Lib.Api;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiClient(
        this IServiceCollection services,
        Action<ApiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ApiOptions();
        configure?.Invoke(options);

        if (!options.BaseAddress.IsAbsoluteUri)
        {
            throw new ArgumentException("API base address must be an absolute URI.", nameof(configure));
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(configure), "API timeout must be greater than zero.");
        }

        services.AddSingleton(options);
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = options.BaseAddress;
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
