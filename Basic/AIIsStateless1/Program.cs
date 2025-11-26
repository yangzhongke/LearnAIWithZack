using Microsoft.Extensions.AI;
using System.ClientModel;
using OpenAI;

// 配置
var apiKey = "";
var baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/";
var model = "qwen3-max";
using IChatClient client =
    new OpenAI.Chat.ChatClient(model, new ApiKeyCredential(apiKey),
        new OpenAIClientOptions() { Endpoint = new Uri(baseUrl) }).AsIChatClient();

/*
while (true)
{
    Console.Write("You:");
    string userInput = Console.ReadLine();
    var chatResponse = await client.GetResponseAsync(userInput);
    Console.WriteLine($"\nAI: {chatResponse.Text}");
}*/

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