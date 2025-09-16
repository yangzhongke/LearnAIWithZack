using System.ClientModel.Primitives;

namespace AI_2;

public class HttpLoggingPipelineTransport : HttpClientPipelineTransport
{
    private readonly HttpClient _httpClient;
    private readonly HttpLoggingHandler _loggingHandler;
    private bool _disposed;

    public HttpLoggingPipelineTransport() : this(Console.WriteLine)
    {
    }

    public HttpLoggingPipelineTransport(Action<string> logAction) : base(
        CreateHttpClientWithLogging(logAction, out var httpClient, out var loggingHandler))
    {
        _httpClient = httpClient;
        _loggingHandler = loggingHandler;
    }

    public HttpLoggingPipelineTransport(string logFilePath) : this(message => WriteToFile(logFilePath, message))
    {
    }

    private static void WriteToFile(string logFilePath, string message)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            throw new ArgumentException("Log file path cannot be null or empty.", nameof(logFilePath));
        }

        File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
    }

    private static HttpClient CreateHttpClientWithLogging(Action<string> logAction, out HttpClient httpClient,
        out HttpLoggingHandler loggingHandler)
    {
        loggingHandler = new HttpLoggingHandler(logAction);
        loggingHandler.InnerHandler = new HttpClientHandler();
        httpClient = new HttpClient(loggingHandler);
        return httpClient;
    }

    protected override void Dispose(bool disposing)
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