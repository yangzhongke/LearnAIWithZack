using System.ClientModel.Primitives;

namespace AI_2;

public class HttpLoggingPipelineTransport : HttpClientPipelineTransport, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HttpLoggingHandler _loggingHandler;
    private bool _disposed;

    public HttpLoggingPipelineTransport() : this(null)
    {
    }

    public HttpLoggingPipelineTransport(Action<string>? logAction = null) : base(
        CreateHttpClientWithLogging(logAction, out var httpClient, out var loggingHandler))
    {
        _httpClient = httpClient;
        _loggingHandler = loggingHandler;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static HttpClient CreateHttpClientWithLogging(Action<string>? logAction, out HttpClient httpClient,
        out HttpLoggingHandler loggingHandler)
    {
        loggingHandler = logAction != null
            ? new HttpLoggingHandler(logAction)
            : new HttpLoggingHandler();

        loggingHandler.InnerHandler = new HttpClientHandler();

        httpClient = new HttpClient(loggingHandler);
        return httpClient;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
                _loggingHandler?.Dispose();
            }

            _disposed = true;
        }
    }
}