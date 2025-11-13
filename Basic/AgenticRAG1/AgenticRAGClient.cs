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
    private readonly List<ChatMessage> _conversationHistory;

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
        _conversationHistory = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful AI assistant with access to a knowledge base of articles. " +
                                  "Use the provided functions to search and retrieve relevant information to answer user questions. " +
                                  "Always cite the article titles when providing information from the knowledge base.")
        };
    }

    public async Task<string> ChatAsync(string userInput, CancellationToken cancellationToken = default)
    {
        _conversationHistory.Add(new UserChatMessage(userInput));

        var options = new ChatCompletionOptions
        {
            Tools =
            {
                CreateSearchArticlesTool(),
                CreateGetArticleByIdTool(),
                CreateListArticlesByCategoryTool(),
                CreateGetArticleSummaryTool()
            }
        };

        const int maxIterations = 10;
        var iteration = 0;

        while (iteration < maxIterations)
        {
            iteration++;
            var response = await _client.CompleteChatAsync(_conversationHistory, options, cancellationToken);

            if (response.Value.FinishReason == ChatFinishReason.Stop)
            {
                var finalAnswer = response.Value.Content[0].Text;
                _conversationHistory.Add(new AssistantChatMessage(finalAnswer));
                return finalAnswer;
            }

            if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                _conversationHistory.Add(new AssistantChatMessage(response.Value));

                foreach (var toolCall in response.Value.ToolCalls)
                {
                    var functionResult = await ExecuteFunctionAsync(toolCall.FunctionName, toolCall.FunctionArguments);
                    _conversationHistory.Add(new ToolChatMessage(toolCall.Id, functionResult));
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

                case "get_article_by_id":
                    {
                        var id = args.RootElement.GetProperty("id").GetInt32();
                        var article = await _articleService.GetArticleByIdAsync(id);
                        if (article == null)
                            return JsonSerializer.Serialize(new { error = $"Article with ID {id} not found" });
                        return JsonSerializer.Serialize(article, new JsonSerializerOptions { WriteIndented = true });
                    }

                case "list_articles_by_category":
                    {
                        var category = args.RootElement.GetProperty("category").GetString() ?? "";
                        var articles = await _articleService.GetArticlesByCategoryAsync(category);
                        return SerializeArticles(articles, $"Found {articles.Count} articles in category '{category}'");
                    }

                case "get_article_summary":
                    {
                        var articles = await _articleService.GetAllArticlesAsync();
                        var summary = articles
                            .GroupBy(a => a.Category)
                            .Select(g => new
                            {
                                Category = g.Key,
                                Count = g.Count(),
                                Articles = g.Select(a => new { a.Id, a.Title, a.PublishDate }).ToList()
                            });
                        return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
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
            "Search for articles by keyword in title or content",
            BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "keyword": {
                            "type": "string",
                            "description": "The keyword or phrase to search for in article titles and content"
                        }
                    },
                    "required": ["keyword"]
                }
                """));
    }

    private ChatTool CreateGetArticleByIdTool()
    {
        return ChatTool.CreateFunctionTool(
            "get_article_by_id",
            "Get the full content of a specific article by its ID",
            BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "id": {
                            "type": "integer",
                            "description": "The unique ID of the article to retrieve"
                        }
                    },
                    "required": ["id"]
                }
                """));
    }

    private ChatTool CreateListArticlesByCategoryTool()
    {
        return ChatTool.CreateFunctionTool(
            "list_articles_by_category",
            "List all articles in a specific category (Technology, Health, Business, Science, Environment)",
            BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "category": {
                            "type": "string",
                            "description": "The category to filter articles by (Technology, Health, Business, Science, Environment)"
                        }
                    },
                    "required": ["category"]
                }
                """));
    }

    private ChatTool CreateGetArticleSummaryTool()
    {
        return ChatTool.CreateFunctionTool(
            "get_article_summary",
            "Get a summary of all articles grouped by category with their titles and IDs",
            BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {}
                }
                """));
    }
}
