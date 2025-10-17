using System.ClientModel;
using System.Text;
using System.Text.Json;
using HttpMataki.NET.Auto;
using OpenAI;
using OpenAI.Chat;

HttpClientAutoInterceptor.StartInterception();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var chatApiKey = Environment.GetEnvironmentVariable("OpenAI__ChatApiKey");

var completeChatClient = new CompleteChatClient("https://yangz-mf8s64eg-eastus2.cognitiveservices.azure.com/openai/v1/",
    "gpt-5-nano", chatApiKey);

var response =
    await completeChatClient.GenerateWithFunctionCallingAsync(
        "Find all my password files under E:\\主同步盘\\我的坚果云\\个人资料\\网络帐号");
Console.WriteLine(response);


public class CompleteChatClient(string endpoint, string deploymentName, string apiKey = null)
{
    public async Task<string> GenerateWithFunctionCallingAsync(string input,
        CancellationToken cancellationToken = default)
    {
        ChatClient client = new(
            credential: new ApiKeyCredential(apiKey),
            model: deploymentName,
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri($"{endpoint}")
            });

        // Define the SearchFiles function
        var searchFilesFunction = ChatTool.CreateFunctionTool(
            "SearchFiles",
            "Search all given file types under a directory and return matched files' full paths",
            BinaryData.FromString("""
                                  {
                                      "type": "object",
                                      "properties": {
                                          "directory": {
                                              "type": "string",
                                              "description": "Directory path to search in"
                                          },
                                          "extensions": {
                                              "type": "array",
                                              "items": {
                                                  "type": "string"
                                              },
                                              "description": "Array of file extensions to search for (e.g., ['.txt', '.cs', '.json'])"
                                          }
                                      },
                                      "required": ["directory", "extensions"]
                                  }
                                  """));

        // Define the ReadTextFile function
        var readTextFileFunction = ChatTool.CreateFunctionTool(
            "ReadTextFile",
            "Read the text content of a given file",
            BinaryData.FromString("""
                                  {
                                      "type": "object",
                                      "properties": {
                                          "fullPath": {
                                              "type": "string",
                                              "description": "Full path to the file to read"
                                          }
                                      },
                                      "required": ["fullPath"]
                                  }
                                  """));

        // Define the WriteToTextFile function
        var writeToTextFileFunction = ChatTool.CreateFunctionTool(
            "WriteToTextFile",
            "Write given content to a text file at the specified path",
            BinaryData.FromString("""
                                  {
                                      "type": "object",
                                      "properties": {
                                          "fullPath": {
                                              "type": "string",
                                              "description": "Full path where to write the file"
                                          },
                                          "content": {
                                              "type": "string",
                                              "description": "Content to write to the file"
                                          }
                                      },
                                      "required": ["fullPath", "content"]
                                  }
                                  """));

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a helpful assistant that can help users with file operations. You can search for files, read their contents, and write to files. Use these tools to help users manage their files effectively."),
            new UserChatMessage(input)
        };

        var options = new ChatCompletionOptions
        {
            Tools = { searchFilesFunction, readTextFileFunction, writeToTextFileFunction }
        };

        // Chain function calls in a loop
        while (true)
        {
            var response = await client.CompleteChatAsync(messages, options, cancellationToken);

            if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                // Add the assistant's response with tool calls
                messages.Add(new AssistantChatMessage(response.Value));

                // Process all tool calls in the response
                foreach (var toolCall in response.Value.ToolCalls)
                {
                    var functionResult = toolCall.FunctionName switch
                    {
                        "SearchFiles" => HandleSearchFilesFunction(toolCall),
                        "ReadTextFile" => HandleReadTextFileFunction(toolCall),
                        "WriteToTextFile" => HandleWriteToTextFileFunction(toolCall),
                        _ => "Function not found"
                    };

                    messages.Add(new ToolChatMessage(toolCall.Id, functionResult));
                }

                // Continue the loop to get the next response
            }
            else
            {
                // No more function calls needed, return final response
                return response.Value.Content[0].Text;
            }
        }
    }

    private string HandleSearchFilesFunction(ChatToolCall toolCall)
    {
        try
        {
            var functionArgs = JsonDocument.Parse(toolCall.FunctionArguments);
            var directory = functionArgs.RootElement.GetProperty("directory").GetString();
            var extensionsArray = functionArgs.RootElement.GetProperty("extensions").EnumerateArray()
                .Select(x => x.GetString()).ToArray();

            var result = SearchFiles(directory, extensionsArray);

            return JsonSerializer.Serialize(new { success = true, files = result },
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { success = false, error = ex.Message },
                new JsonSerializerOptions { WriteIndented = true });
        }
    }

    private string HandleReadTextFileFunction(ChatToolCall toolCall)
    {
        try
        {
            var functionArgs = JsonDocument.Parse(toolCall.FunctionArguments);
            var fullPath = functionArgs.RootElement.GetProperty("fullPath").GetString();

            var content = ReadTextFile(fullPath);

            return JsonSerializer.Serialize(new { success = true, content },
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { success = false, error = ex.Message },
                new JsonSerializerOptions { WriteIndented = true });
        }
    }

    private string HandleWriteToTextFileFunction(ChatToolCall toolCall)
    {
        try
        {
            var functionArgs = JsonDocument.Parse(toolCall.FunctionArguments);
            var fullPath = functionArgs.RootElement.GetProperty("fullPath").GetString();
            var content = functionArgs.RootElement.GetProperty("content").GetString();

            WriteToTextFile(fullPath, content);

            return JsonSerializer.Serialize(new { success = true, message = $"Successfully wrote to {fullPath}" },
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { success = false, error = ex.Message },
                new JsonSerializerOptions { WriteIndented = true });
        }
    }

    // File operation implementations
    private string[] SearchFiles(string directory, string[] extensions)
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

    private string ReadTextFile(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {fullPath}");
        }

        return File.ReadAllText(fullPath);
    }

    private void WriteToTextFile(string fullPath, string content)
    {
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content);
    }
}