using System.Text;

namespace AI_2;

public class HttpLoggingHandler : DelegatingHandler
{
    private readonly Action<string> _logAction;

    public HttpLoggingHandler()
    {
        _logAction = Console.WriteLine;
    }

    public HttpLoggingHandler(Action<string> logAction)
    {
        _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var requestMediaType = request.Content.Headers.ContentType?.MediaType;
            var requestCharset = request.Content.Headers.ContentType?.CharSet;
            var requestEncoding = !string.IsNullOrEmpty(requestCharset)
                ? Encoding.GetEncoding(requestCharset)
                : Encoding.UTF8;

            if (requestMediaType != null &&
                (requestMediaType.StartsWith("text/") || requestMediaType == "application/json"))
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                _logAction($"Request: {request}\nBody: {requestBody}");
                request.Content =
                    new StringContent(requestBody, requestEncoding, requestMediaType);
            }
            else
            {
                _logAction($"Request: {request}\nContent-Type: {requestMediaType}");
            }
        }
        else
        {
            _logAction($"Request: {request}\n Empty Content");
        }

        var response = await base.SendAsync(request, cancellationToken);

        var respContentType = response.Content.Headers.ContentType;
        var respMediaType = respContentType?.MediaType;
        var respCharset = respContentType?.CharSet;
        var respEncoding = !string.IsNullOrEmpty(respCharset) ? Encoding.GetEncoding(respCharset) : Encoding.UTF8;

        if (respMediaType != null && (respMediaType.StartsWith("text/") || respMediaType == "application/json"))
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logAction($"Response: {response}\nBody: {responseBody}");
            response.Content = new StringContent(responseBody, respEncoding, respMediaType);
        }
        else
        {
            _logAction($"Response: {response}\nContent-Type: {respMediaType}");
        }

        return response;
    }
}