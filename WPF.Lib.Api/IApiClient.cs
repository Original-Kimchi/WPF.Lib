namespace WPF.Lib.Api;

public interface IApiClient
{
    Task<TResponse?> GetAsync<TResponse>(
        string requestUri,
        CancellationToken cancellationToken = default);

    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default);

    Task<TResponse?> PutAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string requestUri,
        CancellationToken cancellationToken = default);
}
