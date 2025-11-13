using System.Text;
using AgenticRAG1;
using HttpMataki.NET.Auto;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
HttpClientAutoInterceptor.StartInterception();

var chatApiKey = Environment.GetEnvironmentVariable("AI__ChatApiKey");

if (string.IsNullOrEmpty(chatApiKey))
{
    Console.WriteLine("Error: AI__ChatApiKey environment variable is not set.");
    return;
}

Console.WriteLine("=== Agentic RAG Demo ===\n");

// Part 1: Initialize Database and Insert Sample Articles
Console.WriteLine("Part 1: Initializing database and inserting sample articles...\n");
using var context = new DatabaseContext();
var articleService = new ArticleService(context);
await articleService.InitializeDatabaseAsync();
Console.WriteLine();

// Part 2: Agentic RAG Chat
Console.WriteLine("Part 2: Agentic RAG - Ask questions about the articles\n");
var ragClient = new AgenticRAGClient(
    "https://yangz-mf8s64eg-eastus2.cognitiveservices.azure.com/openai/v1/",
    "gpt-5-nano",
    chatApiKey,
    articleService
);

// Interactive mode
Console.WriteLine("\nEntering interactive mode. Type 'exit' to quit.\n");
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var response = await ragClient.ChatAsync(userInput);
    Console.WriteLine($"Assistant: {response}\n");
}