using System.Text;
using AI_2;

var collectionName = "my_documents";
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var files = Directory.GetFiles("E:\\主同步盘\\我的坚果云\\读书笔记及我的文章\\历史记录\\2010年之前的笔记\\blog随笔", "*.txt",
    SearchOption.AllDirectories);

string apiKey =  Environment.GetEnvironmentVariable("AI__EmbeddingApiKey");

/*
string embeddingEndpoint = "https://personalopenai1.openai.azure.com/openai/v1/";
string embeddingDeploymentName = "text-embedding-3-large";
string textGenEndpoint = "https://yangz-mf8s64eg-eastus2.cognitiveservices.azure.com/openai/v1/";
var extGenDeploymentName = "gpt-5-nano";
*/
string embeddingEndpoint = "http://127.0.0.1:11434/v1/";//"https://personalopenai1.openai.azure.com/openai/v1/";
string embeddingDeploymentName = "mxbai-embed-large:latest";//""text-embedding-3-large";

string textGenEndpoint = "http://127.0.0.1:11434/v1/";//"https://yangz-mf8s64eg-eastus2.cognitiveservices.azure.com/openai/v1/";
var extGenDeploymentName = "llama3:latest";//"gpt-5-nano";

using var httpClientOllama = new HttpClient
    { Timeout = TimeSpan.FromMinutes(50), BaseAddress = new Uri("http://127.0.0.1:11434") };
using var httpClientQdrant = new HttpClient
    { Timeout = TimeSpan.FromMinutes(50), BaseAddress = new Uri("http://localhost:6333") };
var qdrantClient = new QdrantClient(httpClientQdrant);

var embeddingClient = new EmbeddingClient(embeddingEndpoint, embeddingDeploymentName, apiKey);
var completeChatClient = new CompleteChatClient(textGenEndpoint, extGenDeploymentName, apiKey);

var documents = new List<(string, float[])>();

foreach (var file in files)
{
    var text = await FileHelpers.ReadAllTextAnyEncodingAsync(file);
    var chunks = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    foreach (var chunk in chunks)
    {
        string substring;
        if (chunk.Length < 20) continue; // 太短的跳过
        if (chunk.Length > 1000)
            substring = chunk.Substring(0, 1000); // 太长的截断
        else
            substring = chunk;

        // 2. 使用Ollama做embedding
        var embedding = await embeddingClient.GetEmbeddingAsync(substring);
        documents.Add((substring, embedding));
    }
}

await qdrantClient.DeleteCollectionAsync(collectionName);
// 3. 保存到Qdrant
await qdrantClient.SaveToQdrantAsync(collectionName, documents);

// 4. RAG检索+AI生成
while (true)
{
    Console.WriteLine("请输入你的问题：");
    var question = Console.ReadLine();
    var questionEmbedding = await embeddingClient.GetEmbeddingAsync(question);
    var relevantDocs = await qdrantClient.SearchQdrantAsync(collectionName, questionEmbedding);
    for (var i = 0; i < relevantDocs.Count; i++) Console.WriteLine($"相关内容片段{i}：{relevantDocs[i]}");
    var context = string.Join("\n", relevantDocs);
    var answer = await completeChatClient.GenerateTextAsync(question, context);
    Console.WriteLine($"AI回答：{answer}");
}