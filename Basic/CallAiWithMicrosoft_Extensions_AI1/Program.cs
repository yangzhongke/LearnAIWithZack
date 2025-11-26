using Microsoft.Extensions.AI;
using System.ClientModel;
using OpenAI;

// 配置
var apiKey = "";
var baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/";
var model = "qwen3-max";

// 构建消息
ChatMessage[] messages =
[
    new ChatMessage(ChatRole.User,
        @"请从下面的英语句子中把所有的中国初中教材级别以上单词提取出来，复数、过去式等转换为原型形式：
        Far out in the uncharted backwaters of the unfashionable end of the Western Spiral arm of the Galaxy lies a small unregarded yellow sun.
        ")
];

// 发送请求并获取响应
IChatClient client =
    new OpenAI.Chat.ChatClient(model, new ApiKeyCredential(apiKey),
        new OpenAIClientOptions() { Endpoint = new Uri(baseUrl) }).AsIChatClient();
var chatResponse = await client.GetResponseAsync(messages);

// 输出结果
Console.WriteLine($"\nAI 回复: {chatResponse.Text}");

// 输出 token 使用情况
if (chatResponse.Usage != null)
{
    Console.WriteLine($"\n使用 tokens: {chatResponse.Usage.TotalTokenCount}");
    Console.WriteLine($"  - Prompt tokens: {chatResponse.Usage.InputTokenCount}");
    Console.WriteLine($"  - Completion tokens: {chatResponse.Usage.OutputTokenCount}");
}