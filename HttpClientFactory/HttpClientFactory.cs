namespace HttpClientFactory;

public class HttpClientFactory: IDisposable
{

    // based on this discussion https://github.com/dotnet/aspnetcore/issues/28385#issuecomment-853766480
    private readonly SocketsHttpHandler _httpSocketsHandler;
    private readonly HttpClientOptions _httpClientOptions;
    private readonly SemaphoreSlim _semaphore;

    public HttpClientFactory(int maxParalelRequests, HttpClientOptions httpClientOptions)
    {
        _httpSocketsHandler = new SocketsHttpHandler();
        //how often DNS records should be refreshed
        _httpSocketsHandler.PooledConnectionLifetime = TimeSpan.FromMinutes(1);
        
        //todo: check what maximum count means
        _semaphore = new SemaphoreSlim(maxParalelRequests);
        _httpClientOptions = httpClientOptions;
    }
    
    public HttpClient CreateClient()
    {
        //1 second timeout to properly throttle requests
        _semaphore.Wait(1000); 
        
        return new HttpClient(_httpSocketsHandler, false);
    }

    public async Task<HttpClient> CreateClientAsync()
    {
        await _semaphore.WaitAsync();
        
        return new HttpClient(_httpSocketsHandler, false);
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        _httpSocketsHandler.Dispose();
    }
    
}