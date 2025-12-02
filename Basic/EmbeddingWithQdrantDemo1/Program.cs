using System.ClientModel;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;

Console.WriteLine("=== Embedding with Qdrant Demo ===\n");

var apiKey = Environment.GetEnvironmentVariable("AI__EmbeddingApiKey");
var endpoint = "https://personalopenai1.openai.azure.com/openai/v1/";
var deploymentName = "text-embedding-3-large";

var qdrantHost = "127.0.0.1";

// Initialize OpenAI embedding client
var openAiClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
);
var embeddingClient = openAiClient.GetEmbeddingClient(deploymentName);

var qdrantClient = new QdrantClient(host: qdrantHost, https: false);
var collectionName = "sample_texts";

Console.WriteLine("Please select an option:");
Console.WriteLine("1 - Insert sample texts into vector database");
Console.WriteLine("2 - Query directly from vector database");
Console.Write("\nYour choice (1 or 2): ");

var choice = Console.ReadLine();

if (choice == "1")
{
    await InsertRecords();
}
else if (choice == "2")
{
    await RunQuery();
}
else
{
    Console.WriteLine("\nInvalid choice. Exiting...");
}

async Task RunQuery()
{
    // Check if collection exists
    bool collectionExists = await qdrantClient.CollectionExistsAsync(collectionName);

    if (!collectionExists)
    {
        Console.WriteLine($"\nError: Collection '{collectionName}' does not exist!");
        Console.WriteLine("Please choose option 1 first to insert sample texts.");
        return;
    }

    Console.WriteLine($"\nCollection '{collectionName}' found. Ready to query.\n");
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

        // Search in Qdrant
        var searchResults = await qdrantClient.SearchAsync(
            collectionName: collectionName,
            vector: queryEmbedding,
            limit: 3
        );

        Console.WriteLine("\nTop 3 Most Similar Texts:");
        Console.WriteLine(new string('=', 70));

        for (int i = 0; i < searchResults.Count; i++)
        {
            var result = searchResults[i];
            var text = result.Payload["text"].StringValue;
            var similarity = result.Score;

            Console.WriteLine($"\n{i + 1}. Similarity: {similarity:F4} ({similarity * 100:F2}%)");
            Console.WriteLine($"   Text: {text}");
        }
    }
}

async Task InsertRecords()
{
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

    // Get vector dimension from first embedding
    Console.WriteLine("\nGetting embedding dimensions...");
    var sampleEmbeddingResult = await embeddingClient.GenerateEmbeddingAsync(sampleTexts[0]);
    var vectorSize = (ulong)sampleEmbeddingResult.Value.ToFloats().Length;

    Console.WriteLine($"Vector dimension: {vectorSize}");

    // Check if collection exists and compare vector size
    bool needRecreate = false;
    bool collectionExists = await qdrantClient.CollectionExistsAsync(collectionName);

    if (collectionExists)
    {
        var collectionInfo = await qdrantClient.GetCollectionInfoAsync(collectionName);
        var existingVectorSize = collectionInfo.Config.Params.VectorsConfig.Params.Size;

        if (existingVectorSize != vectorSize)
        {
            Console.WriteLine(
                $"Collection exists but vector size mismatch (existing: {existingVectorSize}, new: {vectorSize})");
            needRecreate = true;
        }
        else
        {
            Console.WriteLine(
                $"Collection '{collectionName}' already exists with matching vector size ({vectorSize})");
        }
    }
    else
    {
        Console.WriteLine($"Collection '{collectionName}' does not exist");
        needRecreate = true;
    }

    if (needRecreate)
    {
        if (collectionExists)
        {
            await qdrantClient.DeleteCollectionAsync(collectionName);
            Console.WriteLine($"Deleted existing collection '{collectionName}'");
        }

        await qdrantClient.CreateCollectionAsync(
            collectionName: collectionName,
            vectorsConfig: new VectorParams
            {
                Size = vectorSize,
                Distance = Distance.Cosine
            }
        );

        Console.WriteLine($"Created collection '{collectionName}'");
    }

    Console.WriteLine();
    Console.WriteLine("Generating embeddings and storing in Qdrant...\n");

    // Generate embeddings and insert into Qdrant
    var points = new List<PointStruct>();

    for (int i = 0; i < sampleTexts.Length; i++)
    {
        var text = sampleTexts[i];
        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(text);
        var embedding = embeddingResult.Value.ToFloats().ToArray();

        var point = new PointStruct
        {
            Id = new PointId { Num = (ulong)i },
            Vectors = embedding,
            Payload =
            {
                ["text"] = text
            }
        };

        points.Add(point);
        Console.WriteLine($"✓ {text}");
    }

    // Upsert all points to Qdrant
    await qdrantClient.UpsertAsync(collectionName, points);
    Console.WriteLine($"\nStored {points.Count} embeddings in Qdrant\n");
}