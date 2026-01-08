using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.AI;
using System.ClientModel;
using OpenAI;

//HttpClientAutoInterceptor.StartInterception();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var chatApiKey = Environment.GetEnvironmentVariable("OpenAI__ChatApiKey");

var completeChatClient = new CompleteChatClient("https://yangz-mf8s64eg-eastus2.cognitiveservices.azure.com/openai/v1/",
    "gpt-5-nano", chatApiKey);

var response = await completeChatClient.GenerateWithFunctionCallingAsync("北京今天的天气适合于穿什么衣服?");
Console.WriteLine(response);

public class CompleteChatClient(string endpoint, string deploymentName, string? apiKey = null)
{
    public async Task<string> GenerateWithFunctionCallingAsync(string input,
        CancellationToken cancellationToken = default)
    {
        // Use OpenAI ChatClient directly for function calling
        var chatClient = new OpenAI.Chat.ChatClient(deploymentName, new ApiKeyCredential(apiKey),
            new OpenAIClientOptions() { Endpoint = new Uri(endpoint) }).AsIChatClient();

        List<ChatMessage> messages =
        [
            new ChatMessage(ChatRole.System,
                "You are a helpful assistant that can help users with the given function tools."),
            new ChatMessage(ChatRole.User, input)
        ];
        var options = new ChatOptions
        {
            Tools = [AIFunctionFactory.Create(GetWeatherInfo)]
        };
        var client = new ChatClientBuilder(chatClient)
            .UseFunctionInvocation()
            .Build();
        var response = await client.GetResponseAsync(messages, options, cancellationToken);
        return response.Text;
    }

    [Description("Get weather information for the specified city")]
    private string GetWeatherInfo([Description("City name, for example: Beijing, Shanghai")] string city)
    {
        // Simulate weather API call - in real scenario, this would call actual weather service
        var weatherData = new
        {
            city,
            temperature = "22°C",
            condition = "Sunny",
            humidity = "65%",
            windSpeed = "10 km/h"
        };

        return JsonSerializer.Serialize(weatherData, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}