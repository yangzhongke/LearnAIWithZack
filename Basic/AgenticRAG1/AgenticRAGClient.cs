using System.ClientModel;
using System.Text.Json;
using AgenticRAG1.Models;
using OpenAI;
using OpenAI.Chat;

namespace AgenticRAG1;

public class AgenticRAGClient
{
    private readonly ChatClient _client;
    private readonly ArticleService _articleService;

    public AgenticRAGClient(string endpoint, string deploymentName, string apiKey, ArticleService articleService)
    {
        _client = new ChatClient(
            credential: new ApiKeyCredential(apiKey),
            model: deploymentName,
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            });

        _articleService = articleService;
    }

    public async Task<string> ChatAsync(string userInput, CancellationToken cancellationToken = default)
    {
        var chats = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful AI assistant with access to a knowledge base of articles. " +
                                  "Use the provided functions to search and retrieve relevant information to answer user questions. " +
                                  "Always cite the article titles when providing information from the knowledge base."),
            new UserChatMessage(userInput)
        };

        var options = new ChatCompletionOptions
        {
            Tools =
            {
                CreateSearchArticlesTool()
            }
        };

        const int maxIterations = 10;
        var iteration = 0;

        while (iteration < maxIterations)
        {
            iteration++;
            var response = await _client.CompleteChatAsync(chats, options, cancellationToken);

            if (response.Value.FinishReason == ChatFinishReason.Stop)
            {
                var finalAnswer = response.Value.Content[0].Text;
                return finalAnswer;
            }

            if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                chats.Add(new AssistantChatMessage(response.Value));
                foreach (var toolCall in response.Value.ToolCalls)
                {
                    var functionResult = await ExecuteFunctionAsync(toolCall.FunctionName, toolCall.FunctionArguments);
                    chats.Add(new ToolChatMessage(toolCall.Id, functionResult));
                }
            }
            else
            {
                return "I encountered an unexpected error. Please try again.";
            }
        }

        return "I'm sorry, but I reached the maximum number of iterations while processing your request.";
    }

    private async Task<string> ExecuteFunctionAsync(string functionName, BinaryData arguments)
    {
        var args = JsonDocument.Parse(arguments.ToString());

        try
        {
            switch (functionName)
            {
                case "search_articles":
                    {
                        var keyword = args.RootElement.GetProperty("keyword").GetString() ?? "";
                        var articles = await _articleService.SearchArticlesAsync(keyword);
                        return SerializeArticles(articles, $"Found {articles.Count} articles matching '{keyword}'");
                    }
                default:
                    return JsonSerializer.Serialize(new { error = $"Unknown function: {functionName}" });
            }
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private string SerializeArticles(List<Article> articles, string message)
    {
        var result = new
        {
            message,
            count = articles.Count,
            articles = articles.Select(a => new
            {
                a.Id,
                a.Title,
                a.Category,
                a.PublishDate,
                ContentPreview = a.Content.Length > 200 ? a.Content.Substring(0, 200) + "..." : a.Content
            }).ToList()
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    private ChatTool CreateSearchArticlesTool()
    {
        return ChatTool.CreateFunctionTool(
            "search_articles",
            "Search for articles using semantic similarity. Finds articles related to the keyword even without exact matches - supports fuzzy/semantic search.",
            BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "keyword": {
                            "type": "string",
                            "description": "The keyword or phrase to search for semantically in article titles and content"
                        }
                    },
                    "required": ["keyword"]
                }
                """));
    }
}
