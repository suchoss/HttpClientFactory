namespace HttpClientFactory;

public class HttpClientFactory: IDisposable
{
    private static int _oneSecond = 1000;
    // based on this discussion https://github.com/dotnet/aspnetcore/issues/28385#issuecomment-853766480
    private readonly SocketsHttpHandler _httpSocketsHandler;
    private readonly SemaphoreSlim _throttlingSemaphore;
    
    public HttpClientFactory(int maxRequestsPerSecond, TimeSpan? pooledConnectionLifetime = null)
    {
        _httpSocketsHandler = new SocketsHttpHandler();
        //how often DNS records should be refreshed
        pooledConnectionLifetime ??= TimeSpan.FromMinutes(1);
        
        _httpSocketsHandler.PooledConnectionLifetime = pooledConnectionLifetime.Value;
                
        //todo: check what maximum count means
        _throttlingSemaphore = new SemaphoreSlim(maxRequestsPerSecond);
        
    }
    
    public HttpClient CreateClient()
    {
        //1 second timeout to properly throttle requests
        _throttlingSemaphore.Wait(_oneSecond); 
        
        return new HttpClient(_httpSocketsHandler, false);
    }

    public async Task<HttpClient> CreateClientAsync()
    {
        await _throttlingSemaphore.WaitAsync(_oneSecond);
        
        return new HttpClient(_httpSocketsHandler, false);
    }
    
    public TurboHttpClient CreateTurboClient()
    {
        _throttlingSemaphore.Wait(_oneSecond);
        return new TurboHttpClient(_httpSocketsHandler);
    }
    
    public TurboHttpClient CreateTurboClientAsync()
    {
        _throttlingSemaphore.Wait(_oneSecond);
        return new TurboHttpClient(_httpSocketsHandler);
    }

    public void Dispose()
    {
        _throttlingSemaphore.Dispose();
        _httpSocketsHandler.Dispose();
    }
    
}