using System.ClientModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

var chatApiKey = Environment.GetEnvironmentVariable("OpenAI__ChatApiKey");

var completeChatClient = new CompleteChatClient("https://yangz-mf8s64eg-eastus2.cognitiveservices.azure.com/openai/v1/",
    "gpt-5-nano", chatApiKey);

var response = await completeChatClient.GenerateWithFunctionCallingAsync("How the weather in Beijing?");
Console.WriteLine(response);

public class CompleteChatClient(string endpoint, string deploymentName, string apiKey = null)
{
    public async Task<string> GenerateWithFunctionCallingAsync(string input,
        CancellationToken cancellationToken = default)
    {
        ChatClient client = new(
            credential: new ApiKeyCredential(apiKey),
            model: deploymentName,
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri($"{endpoint}")
            });

        // Define the weather function
        var weatherFunction = ChatTool.CreateFunctionTool(
            "get_weather",
            "Get weather information for the specified city",
            BinaryData.FromString("""
                                  {
                                      "type": "object",
                                      "properties": {
                                          "city": {
                                              "type": "string",
                                              "description": "City name, for example: Beijing, Shanghai"
                                          }
                                      },
                                      "required": ["city"]
                                  }
                                  """));

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant that can help users with the given function tools."),
            new UserChatMessage(input)
        };

        var options = new ChatCompletionOptions
        {
            Tools = { weatherFunction }
        };

        var response = await client.CompleteChatAsync(messages, options, cancellationToken);


        // Check if the model wants to call a function
        if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            var toolCall = response.Value.ToolCalls[0];
            if (toolCall.FunctionName == "get_weather")
            {
                // Parse the function arguments
                var functionArgs = JsonDocument.Parse(toolCall.FunctionArguments);
                var city = functionArgs.RootElement.GetProperty("city").GetString();

                // Execute the function (simulate weather data)
                var weatherResult = GetWeatherInfo(city);

                // Add the function call and result to the conversation
                messages.Add(new AssistantChatMessage(response.Value));
                messages.Add(new ToolChatMessage(toolCall.Id, weatherResult));

                // Get final response with function result
                var finalResponse = await client.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                return finalResponse.Value.Content[0].Text;
            }
        }

        var responseMessage = response.Value.Content[0];
        return responseMessage.Text;
    }

    private string GetWeatherInfo(string city)
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