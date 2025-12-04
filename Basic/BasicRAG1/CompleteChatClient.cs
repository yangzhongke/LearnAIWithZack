using System.ClientModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AI_2;

public class CompleteChatClient(string endpoint, string deploymentName, string apiKey = null)
{
    private IChatClient CreateClient()
    {
        return new OpenAI.Chat.ChatClient(
            model: deploymentName,
            credential: new ApiKeyCredential(apiKey),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri($"{endpoint}")
            }).AsIChatClient();
    }

    public async Task<string> GenerateTextAsync(string input, string context,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, $"根据提供的内容使用简体中文回答用户的问题。注意：不要在提供的内容之外进行回答。内容如下：\n{context}。"),
            new ChatMessage(ChatRole.User, input)
        };

        var response = await client.GetResponseAsync(messages, cancellationToken: cancellationToken);

        return response.Text ?? string.Empty;
    }

    public async IAsyncEnumerable<string> GenerateStreamingTextAsync(string input, string context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, $"根据提供的内容使用简体中文回答用户的问题。注意：不要在提供的内容之外进行回答。内容如下：\n{context}。"),
            new ChatMessage(ChatRole.User, input)
        };

        await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cancellationToken))
        {
            yield return update.Text ?? string.Empty;
        }
    }
}