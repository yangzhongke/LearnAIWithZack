namespace MemNet.Internals;

public static class HttpResponseExtensions
{
    public static async Task EnsureSuccessWithContentAsync(this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string content = await response.Content.ReadAsStringAsync(ct);
        string message = $"Error: {(int)response.StatusCode} {response.ReasonPhrase}\nResponse Body：{content}";
        throw new HttpRequestException(message);
    }
}