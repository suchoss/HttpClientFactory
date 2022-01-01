namespace HttpClientFactory;

public class HttpClientOptions
{
    public HttpClientOptions(int clientLifeSpan, int retryCount, int retryDelay)
    {
        ClientLifeSpan = clientLifeSpan;
        RetryCount = retryCount;
        RetryDelay = retryDelay;
    }

    public int ClientLifeSpan { get; private set; }
    public int RetryCount { get; private set; }
    public int RetryDelay { get; private set; }
}