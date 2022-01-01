using System.Text.Json;

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
}