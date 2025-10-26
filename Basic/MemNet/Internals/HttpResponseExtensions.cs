namespace MemNet.Internals;

public static class HttpResponseExtensions
{
    public static async Task<HttpResponseMessage> EnsureSuccessWithContentAsync(this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        string content = await response.Content.ReadAsStringAsync(ct);
        string message = $"Error: {(int)response.StatusCode} {response.ReasonPhrase}\nResponse Body：{content}";
        throw new HttpRequestException(message);
    }
}