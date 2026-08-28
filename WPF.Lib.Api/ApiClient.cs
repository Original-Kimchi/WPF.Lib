using System.Buffers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WPF.Lib.Api;

public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly int _maximumResponseContentBytes;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        : this(httpClient, logger, new ApiOptions())
    {
    }

    [ActivatorUtilitiesConstructor]
    public ApiClient(
        HttpClient httpClient,
        ILogger<ApiClient> logger,
        ApiOptions options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _maximumResponseContentBytes = checked((int)options.MaximumResponseContentBytes);
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

        using var responseContent = await ReadContentAsync(response.Content, cancellationToken);
        if (IsEmptyOrWhitespace(responseContent))
        {
            return default;
        }

        responseContent.Position = 0;
        return await JsonSerializer.DeserializeAsync<TResponse>(
            responseContent,
            JsonOptions,
            cancellationToken);
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

        using var responseStream = await ReadContentAsync(response.Content, cancellationToken);
        var responseContent = Encoding.UTF8.GetString(
            responseStream.GetBuffer(),
            0,
            checked((int)responseStream.Length));
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

    private async Task<MemoryStream> ReadContentAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength > _maximumResponseContentBytes)
        {
            throw new ApiResponseTooLargeException(_maximumResponseContentBytes);
        }

        var initialCapacity = content.Headers.ContentLength is > 0
            ? checked((int)Math.Min(content.Headers.ContentLength.Value, 64 * 1024))
            : 0;
        var destination = new MemoryStream(initialCapacity);
        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);

        try
        {
            using var source = await content.ReadAsStreamAsync(cancellationToken);
            while (true)
            {
                var bytesRead = await source.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    return destination;
                }

                if (destination.Length + bytesRead > _maximumResponseContentBytes)
                {
                    throw new ApiResponseTooLargeException(_maximumResponseContentBytes);
                }

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }
        }
        catch
        {
            destination.Dispose();
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static bool IsEmptyOrWhitespace(MemoryStream content)
    {
        var buffer = content.GetBuffer();
        for (var index = 0; index < content.Length; index++)
        {
            if (buffer[index] is not ((byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n'))
            {
                return false;
            }
        }

        return true;
    }
}
