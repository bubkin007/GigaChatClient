namespace GigaChatClient;

public sealed class GigaChatClientFactory
{
    private readonly Func<HttpClient> _httpClientFactory;
    private readonly Action<HttpClient>? _configureClient;

    public GigaChatClientFactory(Func<HttpClient>? httpClientFactory = null, Action<HttpClient>? configureClient = null)
    {
        _httpClientFactory = httpClientFactory ?? (() => new HttpClient());
        _configureClient = configureClient;
    }

    public async Task<IGigaChatClient> CreateAsync(GigaChatOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var httpClient = _httpClientFactory();
        if (httpClient == null)
        {
            throw new InvalidOperationException("HTTP client factory returned null instance");
        }
        _configureClient?.Invoke(httpClient);
        var client = new GigaChat(httpClient, options);
        await client.InitializeAsync(cancellationToken).ConfigureAwait(false);
        return client;
    }

    public async Task<IGigaChatClient> CreateAsync(GigaChatOptions options, HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        _configureClient?.Invoke(httpClient);
        var client = new GigaChat(httpClient, options);
        await client.InitializeAsync(cancellationToken).ConfigureAwait(false);
        return client;
    }
}
