using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;
using System.ClientModel;
using OpenAI;

//HttpClientAutoInterceptor.StartInterception();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var chatApiKey = Environment.GetEnvironmentVariable("OpenAI__ChatApiKey");

var completeChatClient = new CompleteChatClient("https://yangz-mf8s64eg-eastus2.cognitiveservices.azure.com/openai/v1/",
    "gpt-5-nano", chatApiKey);

var response =
    await completeChatClient.GenerateWithFunctionCallingAsync(
        "Find all my password files under E:\\主同步盘\\我的坚果云\\个人资料\\网络帐号");
Console.WriteLine(response);


public class CompleteChatClient(string endpoint, string deploymentName, string? apiKey = null)
{
    public async Task<string> GenerateWithFunctionCallingAsync(string input,
        CancellationToken cancellationToken = default)
    {
        // Use OpenAI ChatClient and convert to IChatClient
        var chatClient = new OpenAI.Chat.ChatClient(deploymentName, new ApiKeyCredential(apiKey ?? ""),
            new OpenAIClientOptions() { Endpoint = new Uri(endpoint) }).AsIChatClient();

        List<ChatMessage> messages =
        [
            new ChatMessage(ChatRole.System,
                "You are a helpful assistant that can help users with file operations. You can search for files, read their contents, and write to files. Use these tools to help users manage their files effectively."),
            new ChatMessage(ChatRole.User, input)
        ];

        var options = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(SearchFiles),
                AIFunctionFactory.Create(ReadTextFile),
                AIFunctionFactory.Create(WriteToTextFile),
                AIFunctionFactory.Create(GetAllDrives)
            ]
        };

        // Use ChatClientBuilder with UseFunctionInvocation for automatic function calling
        var client = new ChatClientBuilder(chatClient)
            .UseFunctionInvocation()
            .Build();

        var response = await client.GetResponseAsync(messages, options, cancellationToken);
        return response.Text;
    }

    [Description("Search all given file types under a directory and return matched files' full paths")]
    private string[] SearchFiles(
        [Description("Directory path to search in")]
        string directory,
        [Description("Array of file extensions to search for (e.g., ['.txt', '.cs', '.json'])")]
        string[] extensions)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        var allFiles = new List<string>();

        foreach (var extension in extensions)
        {
            var searchPattern = $"*{extension}";
            var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
            allFiles.AddRange(files);
        }

        return allFiles.Distinct().ToArray();
    }

    [Description("Read the text content of a given file")]
    private string ReadTextFile([Description("Full path to the file to read")] string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {fullPath}");
        }

        return File.ReadAllText(fullPath);
    }

    [Description("Write given content to a text file at the specified path")]
    private void WriteToTextFile(
        [Description("Full path where to write the file")]
        string fullPath,
        [Description("Content to write to the file")]
        string content)
    {
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content);
    }

    [Description("Get all available drives on the computer with their properties")]
    private object[] GetAllDrives()
    {
        var drives = DriveInfo.GetDrives()
            .Where(drive => drive.IsReady)
            .Select(drive => new
            {
                drive.Name,
                drive.DriveType,
                TotalSize = drive.TotalSize,
                AvailableSpace = drive.AvailableFreeSpace,
                drive.VolumeLabel
            })
            .ToArray();

        return drives.ToArray<object>();
    }
}