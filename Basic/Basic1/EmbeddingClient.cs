using System.ClientModel;
using OpenAI;

namespace AI_2;

public class EmbeddingClient(string endpoint, string deploymentName, string apiKey = null)
{
    public async Task<float[]> GetEmbeddingAsync(string input)
    {
        OpenAIClient client = new(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            });

        var embeddingResult  = await client.GetEmbeddingClient(deploymentName).GenerateEmbeddingAsync(input);

        if (embeddingResult.Value!=null)
        {
            var embedding = embeddingResult.Value.ToFloats().ToArray();
            return embedding;
        }
        else
        {
            throw new Exception("Failed to generate embedding or received null value.");
        }
    }
}