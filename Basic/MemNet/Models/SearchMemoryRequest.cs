namespace MemNet.Models;

/// <summary>
/// 搜索记忆请求
/// </summary>
public class SearchMemoryRequest
{
    public string Query { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int Limit { get; set; } = 100;
}

