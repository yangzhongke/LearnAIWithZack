using System.ClientModel;
using System.ClientModel.Primitives;
using System.Runtime.CompilerServices;
using System.Text;
using OpenAI;
using OpenAI.Chat;

namespace AI_2;

public class CompleteChatClient(string endpoint, string deploymentName, string apiKey = null)
{
    public async Task<string> GenerateTextAsync(string input, string context,
        CancellationToken cancellationToken = default)
    {
        ChatClient client = new(
            credential: new ApiKeyCredential(apiKey),
            model: deploymentName,
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri($"{endpoint}")
            });

        ChatCompletion completion = await client.CompleteChatAsync(
        [
            new SystemChatMessage("请根据以下内容使用简体中文回答用户的问题：\n{context}。注意不要基于除了提供的内容之外进行回答。"),
            new UserChatMessage(input)
        ], cancellationToken: cancellationToken);

        var sb = new StringBuilder();
        foreach (var contentPart in completion.Content)
        {
            var message = contentPart.Text;
            sb.AppendLine(message);
        }

        return sb.ToString();
    }

    public async IAsyncEnumerable<string> GenerateStreamingTextAsync(string input, string context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var httpClient = new HttpClient(new HttpLoggingHandler { InnerHandler = new HttpClientHandler() });
        var transport = new HttpClientPipelineTransport(httpClient);

        ChatClient client = new(
            credential: new ApiKeyCredential(apiKey),
            model: deploymentName,
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri($"{endpoint}"),
                Transport = transport,
            });

        var asyncCollectionResult = client.CompleteChatStreamingAsync(
            [
                new SystemChatMessage($"请根据以下内容使用简体中文回答用户的问题：\n{context}。注意不要基于除了提供的内容之外进行回答。"),
                new UserChatMessage(input)
            ],
            cancellationToken: cancellationToken);

        await foreach (var update in asyncCollectionResult)
        foreach (var contentPart in update.ContentUpdate)
            yield return contentPart.Text ?? string.Empty;
    }
}