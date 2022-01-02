using System.Text.Json;
using Polly;
using Polly.Extensions.Http;

namespace HttpClientFactory;

public static class HttpExtensions
{
    public static async Task<T?> DeserializeContentAsync<T>(this HttpResponseMessage responseMessage,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            await using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(contentStream, options, cancellationToken);
        }

        return default;
    }
    
    public static async Task<string?> ReadContentAsStringAsync(this HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            return await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        }

        return default;
    }
    
    public static T? DeserializeContent<T>(this HttpResponseMessage responseMessage,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            using var contentStream = responseMessage.Content.ReadAsStream(cancellationToken);
            return JsonSerializer.Deserialize<T>(contentStream, options);
        }

        return default;
    }
    
    public static string? ReadContentAsString(this HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            using var contentStream = responseMessage.Content.ReadAsStream(cancellationToken);
            contentStream.Position = 0;
            using var sr = new StreamReader(contentStream);

            string result = sr.ReadToEnd();
            return result;
        }

        return default;
    }
    
    public static HttpResponseMessage SendWithRetry(this HttpClient client, HttpRequestMessage request,
        int retryCount = 3,
        int retryDelay = 500,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetry(retryCount, attempt => TimeSpan.FromMilliseconds(retryDelay));
        
        return retryPolicy.Execute(() => client.Send(request, cancellationToken));
    }
    
    public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient client, HttpRequestMessage request,
        int retryCount = 3,
        int retryDelay = 500,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromMilliseconds(retryDelay));
        
        return await retryPolicy.ExecuteAsync(() => client.SendAsync(request, cancellationToken));
    }
}