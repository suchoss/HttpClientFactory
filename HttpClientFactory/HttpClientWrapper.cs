using System.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace HttpClientFactory;

public class HttpClientWrapper
{
    private HttpClientOptions _options;
    private HttpClient _client;
    
    private readonly AsyncRetryPolicy<HttpResponseMessage> _asyncRetryPolicy; 
    private readonly RetryPolicy<HttpResponseMessage> _retryPolicy;
    
    internal Action<HttpClientWrapper> ReleaseClientAction;

    internal HttpClientWrapper(HttpClientOptions clientOptions, Action<HttpClientWrapper> releaseClientAction)
    {
        _options = clientOptions;
        
        ReleaseClientAction = releaseClientAction;

        //todo: question: should i move this to the factory? Because it should be thread safe.
            _asyncRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync(clientOptions.RetryCount);
            
            _retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Retry(clientOptions.RetryCount);
    
    }

    //todo: make this more universal
    public HttpResponseMessage Send(HttpMethod method,
        Uri requestUri,
        HttpContent? content = null,
        HttpRequestHeaders? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, requestUri);
        SetRequestHeaders(request, headers);
        SetRequestContent(request, content);
        var responseMessage = _retryPolicy.Execute(() => _client.Send(request, cancellationToken)); 
        return responseMessage;
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