using System.Text;

namespace AI_2;

public class HttpLoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Request: {request}\nBody: {requestBody}");

            request.Content =
                new StringContent(requestBody, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType);
        }

        var response = await base.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"Response: {response}\nBody: {responseBody}");

        response.Content = new StringContent(responseBody, Encoding.UTF8,
            response.Content.Headers.ContentType?.MediaType);

        return response;
    }
}