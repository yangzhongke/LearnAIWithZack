using System.Text;
using StatefulChat1;
using MemNet;
using MemNet.Abstractions;
using MemNet.Config;
using MemNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json") // Requires NuGet Package: Microsoft.Extensions.Configuration.Json
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddMemNet(configuration);

await using var serviceProvider = services.BuildServiceProvider();
var memoryService = serviceProvider.GetRequiredService<IMemoryService>();
var memConfig = serviceProvider.GetRequiredService<IOptions<MemoryConfig>>().Value;
await memoryService.InitializeAsync();

var chatApiKey = memConfig.LLM.ApiKey;
var textGenEndpoint = memConfig.LLM.Endpoint;
var extGenDeploymentName = memConfig.LLM.Model;
var completeChatClient = new CompleteChatClient(textGenEndpoint, extGenDeploymentName, chatApiKey);
while (true)
{
    Console.Write("你：");
    string question = Console.ReadLine();
    var searchResults = await memoryService.SearchAsync(new SearchMemoryRequest
    {
        Query = question,
        UserId = "user001"
    });

    string memory = string.Join('\n', searchResults.Select(e => e.Memory.UpdatedAt?.ToString() + e.Memory.Data));
    Console.WriteLine("Memory:");
    Console.WriteLine(memory);
    var answer = await completeChatClient.GenerateTextAsync(question, memory);
    Console.Write("AI：");
    Console.WriteLine(answer);

    // 保存新的记忆
    await memoryService.AddAsync(new AddMemoryRequest
    {
        Messages =
        [
            new MessageContent
            {
                Role = "User",
                Content = question
            }
        ],
        UserId = "user001"
    });
}