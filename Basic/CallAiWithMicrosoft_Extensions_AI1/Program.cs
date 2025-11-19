using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

// 配置
var apiKey = "你的API密钥";
var baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/";
var model = "qwen3-max";

// 创建 OpenAI 客户端（使用兼容模式）
var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
{
    Endpoint = new Uri(baseUrl)
});

// 创建 IChatClient
IChatClient chatClient = openAiClient.AsChatClient(model);

// 构建消息
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User,
        @"请从下面的英语句子中把所有的中国初中教材级别以上单词提取出来，复数、过去式等转换为原型形式：
        Far out in the uncharted backwaters of the unfashionable end of the Western Spiral arm of the Galaxy lies a small unregarded yellow sun.
        以Json格式返回，例如：
        {""words"": [""uncharted"", ""backwater""]}
        ")
};

// 发送请求并获取响应
var response = await chatClient.CompleteAsync(messages);

// 输出结果
Console.WriteLine($"\nAI 回复: {response.Message.Text}");

// 输出 token 使用情况
if (response.Usage != null)
{
    Console.WriteLine($"\n使用 tokens: {response.Usage.TotalTokenCount}");
    Console.WriteLine($"  - Prompt tokens: {response.Usage.InputTokenCount}");
    Console.WriteLine($"  - Completion tokens: {response.Usage.OutputTokenCount}");
}