using System.Text.Json.Serialization;
using AgenticRAG1.Models;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI.Embeddings;
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0050

namespace AgenticRAG1;

public class VectorStoreService
{
    private readonly InMemoryVectorStore _vectorStore;
    private readonly InMemoryVectorStoreRecordCollection<string, ArticleRecord> _collection;
    private readonly EmbeddingClient _embeddingClient;

    public VectorStoreService(string endpoint, string apiKey, string embeddingModel = "text-embedding-3-small")
    {
        _vectorStore = new InMemoryVectorStore();
        _collection = new InMemoryVectorStoreRecordCollection<string, ArticleRecord>("articles");
        var azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        _embeddingClient = azureOpenAIClient.GetEmbeddingClient(embeddingModel);
    }

    public async Task InitializeAsync()
    {
        await _collection.CreateCollectionIfNotExistsAsync();
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
    {
        var response = await _embeddingClient.GenerateEmbeddingAsync(text);
        return response.Value.ToFloats();
    }

    public async Task AddArticleAsync(Article article)
    {
        var embedding = await GenerateEmbeddingAsync($"{article.Title} {article.Content}");
        
        var record = new ArticleRecord
        {
            Id = article.Id.ToString(),
            Title = article.Title,
            Content = article.Content,
            Category = article.Category,
            PublishDate = article.PublishDate,
            Embedding = embedding
        };

        await _collection.UpsertAsync(record);
    }

    public async Task<List<ArticleRecord>> SearchArticlesAsync(string query, int limit = 100)
    {
        var queryEmbedding = await GenerateEmbeddingAsync(query);
        
        var searchResults = await _collection.VectorizedSearchAsync(queryEmbedding, new() { Top = limit });
        
        var articles = new List<ArticleRecord>();
        await foreach (var result in searchResults.Results)
        {
            articles.Add(result.Record);
        }
        
        return articles;
    }

    public async Task<ArticleRecord?> GetArticleByIdAsync(int id)
    {
        return await _collection.GetAsync(id.ToString());
    }

    public async Task<List<ArticleRecord>> GetAllArticlesAsync()
    {
        var allRecords = new List<ArticleRecord>();
        
        // Get all records from the in-memory collection
        await foreach (var record in _collection.GetBatchAsync(Enumerable.Range(1, 1000).Select(i => i.ToString())))
        {
            if (record != null)
            {
                allRecords.Add(record);
            }
        }
        
        return allRecords;
    }
}

public class ArticleRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("publishDate")]
    public DateTime PublishDate { get; set; }
    
    [JsonPropertyName("embedding")]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
