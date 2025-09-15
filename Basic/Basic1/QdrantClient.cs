using System.Text;
using System.Text.Json;

namespace AI_2;

public class QdrantClient
{
    private readonly HttpClient _httpClient;

    public QdrantClient(HttpClient httpClient)
    {
        _httpClient = httpClient; //http://localhost:6333
    }

    // 检查并创建Qdrant collection
    public async Task EnsureQdrantCollectionExistsAsync(string collectionName, int vectorSize,
        CancellationToken cancellationToken = default)
    {
        var resp = await _httpClient.GetAsync($"/collections/{collectionName}", cancellationToken);
        if (resp.IsSuccessStatusCode)
        {
            // 已存在，检查维度
            var json = await resp.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var existingSize = doc.RootElement.GetProperty("result").GetProperty("config").GetProperty("params")
                .GetProperty("vectors").GetProperty("size").GetInt32();
            if (existingSize != vectorSize)
            {
                // 维度不符，删除重建
                var delResp = await _httpClient.DeleteAsync($"/collections/{collectionName}", cancellationToken);
                if (!delResp.IsSuccessStatusCode)
                    throw new Exception(
                        $"Failed to delete Qdrant collection: {await delResp.Content.ReadAsStringAsync(cancellationToken)}");
            }
            else
            {
                return; // 维度一致，直接返回
            }
        }

        // 创建collection
        var payload = new
        {
            vectors = new
            {
                size = vectorSize,
                distance = "Cosine"
            }
        };
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var createResp = await _httpClient.PutAsync($"/collections/{collectionName}", content, cancellationToken);
        if (!createResp.IsSuccessStatusCode)
            throw new Exception(
                $"Failed to create Qdrant collection: {await createResp.Content.ReadAsStringAsync(cancellationToken)}");
    }

    // 保存到Qdrant
    public async Task SaveToQdrantAsync(string collectionName, List<(string, float[])> docs,
        CancellationToken cancellationToken = default)
    {
        if (docs.Count == 0) return;
        await EnsureQdrantCollectionExistsAsync(collectionName, docs[0].Item2.Length, cancellationToken);
        foreach (var (text, embedding) in docs)
        {
            // 用text的md5值作为id
            var id = HashHelper.GetMd5Hash(text);

            var payload = new
            {
                points = new[]
                {
                    new
                    {
                        id,
                        vector = embedding,
                        payload = new { text }
                    }
                }
            };
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            // upsert: Qdrant的points接口本身就是upsert语义
            using var resp = await _httpClient.PutAsync($"/collections/{collectionName}/points?wait=true", content,
                cancellationToken);
            if (!resp.IsSuccessStatusCode)
                throw new Exception(
                    $"Failed to save to Qdrant: {await resp.Content.ReadAsStringAsync(cancellationToken)}");
        }
    }


    // Qdrant检索
    public async Task<List<string>> SearchQdrantAsync(string collectionName, float[] embedding,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            vector = embedding,
            limit = 3
        };
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"/collections/{collectionName}/points/search", content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var result = new List<string>();
        foreach (var point in doc.RootElement.GetProperty("result").EnumerateArray())
        {
            var idProp = point.GetProperty("id");
            var id = idProp.GetString();
            using var detailResp =
                await _httpClient.GetAsync($"/collections/{collectionName}/points/{id}", cancellationToken);
            if (detailResp.IsSuccessStatusCode)
            {
                var detailJson = await detailResp.Content.ReadAsStringAsync(cancellationToken);
                using var detailDoc = JsonDocument.Parse(detailJson);
                var detailRoot = detailDoc.RootElement.GetProperty("result");
                if (detailRoot.TryGetProperty("payload", out var detailPayload) &&
                    detailPayload.TryGetProperty("text", out var detailText)) result.Add(detailText.GetString());
            }
        }

        return result;
    }

    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        using var delResp = await _httpClient.DeleteAsync($"/collections/{collectionName}", cancellationToken);
        if (!delResp.IsSuccessStatusCode) throw new Exception("Failed to delete Qdrant collection");
    }
}