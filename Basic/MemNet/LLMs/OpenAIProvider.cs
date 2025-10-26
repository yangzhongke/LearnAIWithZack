using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MemNet.Config;
using MemNet.Internals;
using MemNet.Models;
using Microsoft.Extensions.Options;

namespace MemNet.LLMs;

/// <summary>
///     OpenAI LLM 提供者实现
/// </summary>
public class OpenAIProvider
{
    private readonly LLMConfig _config;
    private readonly HttpClient _httpClient;

    public OpenAIProvider(HttpClient httpClient, IOptions<MemoryConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value.LLM;

        // 配置 HttpClient
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_config.Endpoint ?? "https://api.openai.com/v1/");
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        }
    }

    public async Task<string> GenerateTextAsync(string systemPrompt, string message, CancellationToken ct = default)
    {
        var request = new
        {
            model = _config.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = message }
            },
            response_format = new { type = "json_object" }
        };

        var response = await _httpClient.PostAsJsonAsync("chat/completions", request, ct);
        await response.EnsureSuccessWithContentAsync(ct);

        var result =
            await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(ct);
        return result?.Choices?[0].Message.Content?.Trim() ?? string.Empty;
    }

    public async Task<List<ExtractedMemory>> ExtractMemoriesAsync(string message, CancellationToken ct = default)
    {
        var systemPrompt = """
                           You are a memory extraction expert. Extract key facts, preferences, and important context from the conversation.
                           Return a JSON object with this structure:
                           {
                               "memories": [
                                   {"data": "extracted fact 1"},
                                   {"data": "extracted fact 2"}
                               ]
                           }

                           Only extract factual statements, user preferences, personal information, and important context.
                           Be concise and specific. Each memory should be a standalone fact.
                           """;

        var content = await GenerateTextAsync(systemPrompt, message, ct);
        var extraction =
            JsonSerializer.Deserialize<MemoryExtractionResult>(content);
        return extraction?.Memories ?? new List<ExtractedMemory>();
    }

    public async Task<string> MergeMemoriesAsync(string existing, string newMemory, CancellationToken ct = default)
    {
        var systemPrompt = $"""
                            Merge these two memory statements into one coherent, accurate statement:

                            Existing memory: {existing}
                            New memory: {newMemory}

                            Rules:
                            1. Preserve all factual information from both
                            2. If they conflict, prefer the newer information
                            3. Be concise but complete
                            4. Return ONLY the merged memory text, no explanation
                            """;

        var content = await GenerateTextAsync(systemPrompt, "", ct);
        return content;
    }

    public async Task<List<MemorySearchResult>> RerankAsync(string query, List<MemorySearchResult> results,
        CancellationToken ct = default)
    {
        if (results.Count == 0)
        {
            return results;
        }

        var memoriesText = string.Join("\n", results.Select((r, i) =>
            $"{i}. {r.Memory.Data}"));

        var systemPrompt = $$"""
                             Given the query: "{{query}}"

                             Rank these memories by relevance (most relevant first):
                             {{memoriesText}}

                             Return a JSON object with this structure:
                             {
                                 "ranked_indices": [0, 2, 1, ...]
                             }

                             Only return indices of relevant memories, omit irrelevant ones.
                             """;

        var content = await GenerateTextAsync(systemPrompt, "", ct);

        if (string.IsNullOrWhiteSpace(content))
        {
            return results;
        }

        var ranking = JsonSerializer.Deserialize<RankingResult>(content);
        if (ranking?.RankedIndices == null)
        {
            return results;
        }

        var reranked = new List<MemorySearchResult>();
        foreach (var index in ranking.RankedIndices)
        {
            if (index >= 0 && index < results.Count)
            {
                reranked.Add(results[index]);
            }
        }

        return reranked;
    }

    public async Task<List<EntityRelation>> ExtractEntitiesAsync(string text, CancellationToken ct = default)
    {
        var systemPrompt = """
                           You are an entity and relationship extraction expert. Extract entities and their relationships from the text.
                           Return a JSON object with this structure:
                           {
                               "relations": [
                                   {
                                       "source": {"name": "entity1", "type": "Person"},
                                       "target": {"name": "entity2", "type": "Location"},
                                       "relationType": "lives_in"
                                   }
                               ]
                           }

                           Extract meaningful entities (Person, Organization, Location, Concept, etc.) and their relationships.
                           """;


        var content = await GenerateTextAsync(systemPrompt, text, ct);
        var extraction = JsonSerializer.Deserialize<EntityExtractionResult>(content);
        return extraction?.Relations ?? new List<EntityRelation>();
    }

    // 内部类用于 JSON 反序列化
    private class ChatCompletionResponse
    {
        [JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = new();
    }

    private class Choice
    {
        [JsonPropertyName("message")] public Message Message { get; set; } = new();
    }

    private class Message
    {
        [JsonPropertyName("content")] public string Content { get; set; }
    }

    private class MemoryExtractionResult
    {
        [JsonPropertyName("memories")] public List<ExtractedMemory> Memories { get; set; } = new();
    }

    private class RankingResult
    {
        [JsonPropertyName("ranked_indices")] public List<int> RankedIndices { get; set; } = new();
    }

    private class EntityExtractionResult
    {
        [JsonPropertyName("relations")] public List<EntityRelation> Relations { get; set; } = new();
    }
}