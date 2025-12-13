using System.ClientModel;
using OpenAI;
using OpenAI.Embeddings;

Console.WriteLine("=== Embedding Similarity Demo ===\n");

var apiKey = Environment.GetEnvironmentVariable("AI__EmbeddingApiKey");
var endpoint =
    "https://personalopenai1.openai.azure.com/openai/v1/"; // "https://dashscope.aliyuncs.com/compatible-mode/v1/"
var deploymentName = "text-embedding-3-large"; //"text-embedding-v4"

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error: AI__EmbeddingApiKey environment variable not set.");
    return;
}

// Initialize embedding client
var openAiClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
);
var embeddingClient = openAiClient.GetEmbeddingClient(deploymentName);

// Sample texts on different topics
var sampleTexts = new[]
{
    "C# is a popular programming language for data science and machine learning.",
    "I love cooking Italian pasta with fresh tomatoes and basil.",
    "The football match was exciting, with the final score being 3-2.",
    "Machine learning algorithms can identify patterns in large datasets.",
    "The recipe calls for two cups of flour and three eggs.",
    "Basketball requires good coordination and teamwork skills.",
    "Neural networks are inspired by biological brain structures.",
    "Baking bread at home requires patience and the right temperature.",
    "The soccer team won the championship after months of training.",
    "Deep learning has revolutionized computer vision and natural language processing."
};

Console.WriteLine("Generating embeddings for sample texts...\n");

// Generate and store embeddings
var textEmbeddings = new List<(string text, float[] embedding)>();

foreach (var text in sampleTexts)
{
    ClientResult<OpenAIEmbedding> embeddingResult = await embeddingClient.GenerateEmbeddingAsync(text);
    float[] embedding = embeddingResult.Value.ToFloats().ToArray();
    textEmbeddings.Add((text, embedding));
    Console.WriteLine($"✓ {text}");
}

Console.WriteLine(
    $"\nStored {textEmbeddings.Count} text embeddings (dimension: {textEmbeddings[0].embedding.Length})\n");

// Interactive query loop
while (true)
{
    Console.WriteLine("\n" + new string('-', 70));
    Console.Write("Enter your query (or 'quit' to exit): ");
    var query = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(query) || query.ToLower() == "quit")
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    Console.WriteLine($"\nSearching for: \"{query}\"");
    Console.WriteLine("Generating query embedding...");

    // Generate embedding for query
    var queryEmbeddingResult = await embeddingClient.GenerateEmbeddingAsync(query);
    var queryEmbedding = queryEmbeddingResult.Value.ToFloats().ToArray();

    // Calculate similarities
    var similarities = new List<(string text, double similarity)>();

    foreach (var (text, embedding) in textEmbeddings)
    {
        var similarity = CalculateCosineSimilarity(queryEmbedding, embedding);
        similarities.Add((text, similarity));
    }

    // Sort by similarity (highest first)
    var topResults = similarities.OrderByDescending(x => x.similarity).Take(3).ToList();

    Console.WriteLine("\nTop 3 Most Similar Texts:");
    Console.WriteLine(new string('=', 70));

    for (int i = 0; i < topResults.Count; i++)
    {
        var (text, similarity) = topResults[i];
        Console.WriteLine($"\n{i + 1}. Similarity: {similarity:F4} ({similarity * 100:F2}%)");
        Console.WriteLine($"   Text: {text}");
    }
}

// Helper function to calculate cosine similarity
static double CalculateCosineSimilarity(float[] vector1, float[] vector2)
{
    if (vector1.Length != vector2.Length)
    {
        throw new ArgumentException("Vectors must have the same dimension");
    }

    double dotProduct = 0;
    double magnitude1 = 0;
    double magnitude2 = 0;

    for (int i = 0; i < vector1.Length; i++)
    {
        dotProduct += vector1[i] * vector2[i];
        magnitude1 += vector1[i] * vector1[i];
        magnitude2 += vector2[i] * vector2[i];
    }

    magnitude1 = Math.Sqrt(magnitude1);
    magnitude2 = Math.Sqrt(magnitude2);

    if (magnitude1 == 0 || magnitude2 == 0)
    {
        return 0;
    }

    return dotProduct / (magnitude1 * magnitude2);
}