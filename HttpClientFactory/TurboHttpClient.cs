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
    
    public (T? result, bool IsSuccessStatusCode, HttpStatusCode StatusCode, string? ReasonPhrase) Send<T>(HttpMethod method,
        Uri requestUri,
        HttpContent? content = null,
        HttpRequestHeaders? headers = null,
        int retryCount = 3,
        int retryDelay = 100,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        SetRequestHeaders(request, headers);
        SetRequestContent(request, content);
        
        using var responseMessage = _httpClient.SendWithRetry(request, retryCount, retryDelay, cancellationToken);

        JsonSerializerOptions? options = null;
        var result = responseMessage.DeserializeContent<T>(options, cancellationToken);
        
        return (result, responseMessage.IsSuccessStatusCode, responseMessage.StatusCode, responseMessage.ReasonPhrase);
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