using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

// 配置阿里云百炼平台参数
const string apiKey = "**"; // 替换为您的API密钥
const string baseUrl = "https://open.bigmodel.cn/api/paas/v4";
const string model = "glm-4.5"; // 或其他模型如 qwen-turbo, qwen-max

using var httpClient = new HttpClient()
{
    Timeout = TimeSpan.FromSeconds(600)
};
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

// 构建请求体
var requestBody = new
{
    model = model,
    messages = new[]
    {
        new
        {
            role = "system",
            content =
                "你是一个有经验的针对中文为母语的英语学习者提供英语学习服务的老师，你会把用户提供的英文句子中的中国初中英语教材之外的单词提取出来。把复数、过去式等转换为原型形式。超出英语学习范围的东西，请回复‘这个问题我回答不了，换个问题吧。’"
        },
        new
        {
            role = "user",
            content =
                "Far out in the uncharted backwaters of the unfashionable end of the Western Spiral arm of the Galaxy lies a small unregarded yellow sun."
            //"马云是谁？"
        }
    },
    temperature = 0.7,
    max_tokens = 1000,
    stream = false // 设置为true可以启用流式响应
};

var jsonContent = JsonSerializer.Serialize(requestBody);
var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

try
{
    Console.WriteLine("正在调用阿里云百炼平台...\n");

    // 发送POST请求
    var response = await httpClient.PostAsync($"{baseUrl}/chat/completions", content);
    response.EnsureSuccessStatusCode();

    // 读取响应
    var responseBody = await response.Content.ReadAsStringAsync();
    // Console.WriteLine("完整响应:");
    //Console.WriteLine(responseBody);
    // 解析响应
    using var doc = JsonDocument.Parse(responseBody);
    var root = doc.RootElement;

    // 提取AI回复内容
    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
    {
        var firstChoice = choices[0];
        if (firstChoice.TryGetProperty("message", out var message) &&
            message.TryGetProperty("content", out var aiContent))
        {
            Console.WriteLine("AI回复:");
            Console.WriteLine(aiContent.GetString());
            Console.WriteLine();
        }
    }

    // 显示使用统计
    if (root.TryGetProperty("usage", out var usage))
    {
        Console.WriteLine("使用统计:");
        Console.WriteLine($"  提示词令牌数: {usage.GetProperty("prompt_tokens").GetInt32()}");
        Console.WriteLine($"  完成令牌数: {usage.GetProperty("completion_tokens").GetInt32()}");
        Console.WriteLine($"  总令牌数: {usage.GetProperty("total_tokens").GetInt32()}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP请求错误: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"JSON解析错误: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"发生错误: {ex.Message}");
}