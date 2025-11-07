using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using OpenAI;
using OpenAI.Chat;

namespace StatefulChat1;

public class CompleteChatClient(string endpoint, string deploymentName, string apiKey = null)
{
    public async Task<string> GenerateTextAsync(string input, string memory,
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
            new SystemChatMessage($"根据提供的内容回答用户的问题。如果对这个用户的记忆：\n{memory}。"),
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

    public async IAsyncEnumerable<string> GenerateStreamingTextAsync(string input, string memory,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ChatClient client = new(
            credential: new ApiKeyCredential(apiKey),
            model: deploymentName,
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri($"{endpoint}")
            });

        var asyncCollectionResult = client.CompleteChatStreamingAsync(
            [
                new SystemChatMessage($"根据提供的内容回答用户的问题。如果对这个用户的记忆：\n{memory}。"),
                new UserChatMessage(input)
            ],
            cancellationToken: cancellationToken);

        await foreach (var update in asyncCollectionResult)
        foreach (var contentPart in update.ContentUpdate)
        {
            yield return contentPart.Text ?? string.Empty;
        }
    }
}