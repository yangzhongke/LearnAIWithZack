using System.ClientModel;
using System.Text;
using OpenAI;
using OpenAI.Chat;

namespace AI_2;

public class CompleteChatClient(string endpoint, string deploymentName, string apiKey = null)
{
    public async Task<string> GenerateTextAsync(string input, string context)
    {
        ChatClient client = new(
            credential: new ApiKeyCredential(apiKey),
            model: deploymentName,
            options: new OpenAIClientOptions()
            {
                Endpoint = new($"{endpoint}"),
            });

        ChatCompletion completion = client.CompleteChat(
        [
            new SystemChatMessage($"请根据以下内容使用简体中文回答用户的问题：\n{{context}}。注意不要基于除了提供的内容之外进行回答。"),
            new UserChatMessage(input)
        ]);

        StringBuilder sb = new StringBuilder();
        foreach (ChatMessageContentPart contentPart in completion.Content)
        {
            string message = contentPart.Text;
            sb.AppendLine(message);
        }

        return sb.ToString();
    }
}