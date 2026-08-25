using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WPF.Lib.Api;

public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<TResponse?> GetAsync<TResponse>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Get, requestUri, null, cancellationToken);
    }

    public Task<TResponse?> PostAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Post, requestUri, request, cancellationToken);
    }

    public Task<TResponse?> PutAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Put, requestUri, request, cancellationToken);
    }

    public async Task DeleteAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, requestUri, null);
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, request, cancellationToken);
    }

    private async Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string requestUri,
        object? content,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, requestUri, content);
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, request, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "API 요청 시간이 초과되었습니다. {Method} {RequestUri}",
                request.Method,
                request.RequestUri);
            throw;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "API 요청에 실패했습니다. {Method} {RequestUri}",
                request.Method,
                request.RequestUri);
            throw;
        }
    }

    private static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string requestUri,
        object? content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);

        var request = new HttpRequestMessage(method, requestUri);
        if (content is not null)
        {
            request.Content = JsonContent.Create(content, options: JsonOptions);
        }

        return request;
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning(
            "API가 오류 상태 코드를 반환했습니다. {StatusCode} {Method} {RequestUri}",
            (int)response.StatusCode,
            request.Method,
            request.RequestUri);

        throw new ApiException(
            response.StatusCode,
            responseContent,
            $"API request failed with status code {(int)response.StatusCode} ({response.StatusCode}).");
    }
}
