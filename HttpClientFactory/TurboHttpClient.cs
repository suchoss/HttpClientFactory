using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HttpClientFactory;

public class TurboHttpClient
{
    private readonly HttpClient _httpClient;
    public TurboHttpClient(SocketsHttpHandler httpSocketsHandler)
    {
        _httpClient = new HttpClient(httpSocketsHandler, false);
    }
    
    public TurboResult<T?> Send<T>(HttpMethod method,
        Uri requestUri,
        HttpContent? content = null,
        HttpRequestHeaders? headers = null,
        int retryCount = 3,
        int retryDelay = 100,
        CancellationToken cancellationToken = default)
    {
        TurboResult<T?> result;
        try
        {
            using var request = new HttpRequestMessage(method, requestUri);
            SetRequestHeaders(request, headers);
            SetRequestContent(request, content);

            using var responseMessage = _httpClient.SendWithRetry(request, retryCount, retryDelay, cancellationToken);

            JsonSerializerOptions? options = null;
            var deserializeContent = responseMessage.DeserializeContent<T>(options, cancellationToken);
            result = new TurboResult<T?>(deserializeContent,
                responseMessage.IsSuccessStatusCode,
                responseMessage.StatusCode,
                responseMessage.ReasonPhrase);
        }
        catch
        {
            throw;
        }
        finally
        {
            _httpClient.Dispose();
        }

        return result;
    }
    
    private void SetRequestHeaders(HttpRequestMessage request, HttpRequestHeaders? headers)
    {
        if (headers is null)
            return;
            
        foreach (var header in headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
    
    private void SetRequestContent(HttpRequestMessage request, HttpContent? content)
    {
        if (content != null)
            request.Content = content;
    }
}