using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

// 配置

var apiKey = "你的API密钥";
var baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/";
var model = "qwen3-max";

// 创建 HttpClient
using var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

// 构建请求
var request = new
{
    model = model,
    messages = new[]
    {
        new
        {
            role = "user",
            content =
                @"请从下面的英语句子中把所有的中国初中教材级别以上单词提取出来，复数、过去式等转换为原型形式：
        Far out in the uncharted backwaters of the unfashionable end of the Western Spiral arm of the Galaxy lies a small unregarded yellow sun.
        以Json格式返回，例如：
        {""words"": [""uncharted"", ""backwater""]}
        "
        }
    }
};

// 发送请求
var response = await httpClient.PostAsJsonAsync("chat/completions", request);
response.EnsureSuccessStatusCode();

// 解析响应
var chatResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
if (chatResponse?.Choices != null && chatResponse.Choices.Length > 0)
{
    var messageContent = chatResponse.Choices[0].Message.Content;
    Console.WriteLine($"\nAI 回复: {messageContent}");
    Console.WriteLine($"\n使用 tokens: {chatResponse.Usage.TotalTokens}");
    Console.WriteLine($"  - Prompt tokens: {chatResponse.Usage.PromptTokens}");
    Console.WriteLine($"  - Completion tokens: {chatResponse.Usage.CompletionTokens}");
}
else
{
    Console.WriteLine("未收到有效的回复。");
}

// 定义响应模型
public class ChatCompletionResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")] public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")] public long Created { get; set; }

    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")] public Choice[] Choices { get; set; } = Array.Empty<Choice>();

    [JsonPropertyName("usage")] public Usage Usage { get; set; } = new Usage();
}

public class Choice
{
    [JsonPropertyName("index")] public int Index { get; set; }

    [JsonPropertyName("message")] public Message Message { get; set; } = new Message();

    [JsonPropertyName("finish_reason")] public string FinishReason { get; set; } = string.Empty;
}

public class Message
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
}