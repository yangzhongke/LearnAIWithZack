using Microsoft.Extensions.AI;
using System.ClientModel;
using HttpMataki.NET.Auto;
using OpenAI;

// 配置
var apiKey = "";
var baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/";
var model = "qwen3-max";
//HttpClientAutoInterceptor.StartInterception();
// 构建消息
ChatMessage[] messages =
[
    new ChatMessage(ChatRole.User,
        @"请讲一个关于人工智能的小故事，大约200字。")
];

// 创建聊天客户端
IChatClient client =
    new OpenAI.Chat.ChatClient(model, new ApiKeyCredential(apiKey),
        new OpenAIClientOptions() { Endpoint = new Uri(baseUrl) }).AsIChatClient();

// 使用流式输出
await foreach (var update in client.GetStreamingResponseAsync(messages))
{
    Console.Write(update.Text);
}

Console.WriteLine("\n输出完成！");