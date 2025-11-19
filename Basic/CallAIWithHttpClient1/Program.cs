using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// Get API key from environment variable
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error: OPENAI_API_KEY environment variable is not set");
    return;
}

// Configure HttpClient with base URL (must end with trailing slash)
var baseUrl = "https://api.openai.com/v1/";
using var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

// Create request payload
var requestBody = new
{
    model = "gpt-3.5-turbo",
    messages = new[]
    {
        new { role = "user", content = "Hello, how are you?" }
    }
};

var jsonContent = JsonSerializer.Serialize(requestBody);
var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

try
{
    // Make API call - Fixed: use "chat/completions" without leading slash
    // When BaseAddress ends with /v1, the relative path should not start with /
    var response = await httpClient.PostAsync("chat/completions", content);
    response.EnsureSuccessStatusCode();

    // Read and deserialize response
    var responseJson = await response.Content.ReadAsStringAsync();
    var chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);

    if (chatResponse != null && chatResponse.Choices.Length > 0)
    {
        Console.WriteLine("AI Response:");
        Console.WriteLine(chatResponse.Choices[0].Message.Content);
        Console.WriteLine();
        Console.WriteLine("Token Usage:");
        Console.WriteLine($"  Prompt tokens: {chatResponse.Usage.PromptTokens}");
        Console.WriteLine($"  Completion tokens: {chatResponse.Usage.CompletionTokens}");
        Console.WriteLine($"  Total tokens: {chatResponse.Usage.TotalTokens}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
    Console.WriteLine("Make sure OPENAI_API_KEY is valid and the endpoint is correct.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Strong-typed response models
public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public Choice[] Choices { get; set; } = Array.Empty<Choice>();

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new Usage();
}

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; } = new Message();

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
