using Microsoft.Extensions.AI;
using System.ClientModel;
using OpenAI;

// 配置
var apiKey = Environment.GetEnvironmentVariable("ApiKey");
var baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/";
var model = "qwen3-max";
using IChatClient client =
    new OpenAI.Chat.ChatClient(model, new ApiKeyCredential(apiKey),
        new OpenAIClientOptions() { Endpoint = new Uri(baseUrl) }).AsIChatClient();

//1: Stateless Chat
while (true)
{
    Console.Write("You:");
    string userInput = Console.ReadLine();
    var chatResponse = await client.GetResponseAsync(userInput);
    Console.WriteLine($"\nAI: {chatResponse.Text}");
}

//2: Stateful Chat
List<ChatMessage> messages = new List<ChatMessage>();
while (true)
{
    Console.Write("You:");
    string userInput = Console.ReadLine();
    messages.Add(new ChatMessage(ChatRole.User, userInput));
    var chatResponse = await client.GetResponseAsync(messages);
    Console.WriteLine($"\nAI: {chatResponse.Text}");
    messages.Add(new ChatMessage(ChatRole.System, chatResponse.Text));
}