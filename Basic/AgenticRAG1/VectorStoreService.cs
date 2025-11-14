using AgenticRAG1.Models;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI.Embeddings;


namespace AgenticRAG1;

public class VectorStoreService
{
    private readonly InMemoryVectorStoreRecordCollection<Guid, ArticleRecord> _collection;
    private readonly EmbeddingClient _embeddingClient;

    public VectorStoreService(string endpoint, string apiKey, string embeddingModel)
    {
        _collection = new InMemoryVectorStoreRecordCollection<Guid, ArticleRecord>("articles");
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
            Id = Guid.NewGuid(),
            OriginalId = article.Id,
            Title = article.Title,
            Content = article.Content,
            Category = article.Category,
            PublishDate = article.PublishDate,
            Vector = embedding
        };

        await _collection.UpsertAsync(record);
    }

    public async Task<List<ArticleRecord>> SearchArticlesAsync(string query, int limit)
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
}

public class ArticleRecord
{
    [VectorStoreRecordKey] public Guid Id { get; set; }

    public int OriginalId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public DateTime PublishDate { get; set; }

    [VectorStoreRecordVector]
    public ReadOnlyMemory<float> Vector { get; set; }
}
