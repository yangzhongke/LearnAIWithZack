namespace MemNet.Config;

/// <summary>
/// 记忆服务配置
/// </summary>
public class MemoryConfig
{
    public LLMConfig LLM { get; set; } = new();
    public EmbedderConfig Embedder { get; set; } = new();

    /// <summary>
    /// 去重阈值（余弦相似度）
    /// </summary>
    public float DuplicateThreshold { get; set; } = 0.9f;
    
}
public class LLMConfig
{
    public string Model { get; set; } = "gpt-4";
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
}

public class EmbedderConfig
{
    public string Model { get; set; } = "text-embedding-3-small";
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
}