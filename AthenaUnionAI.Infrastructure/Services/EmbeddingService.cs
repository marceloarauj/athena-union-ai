using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Pgvector;

namespace AthenaUnionAI.Infrastructure.Services
{
    public class EmbeddingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        private readonly string _endpoint = configuration["EmbeddingModel:Endpoint"]
            ?? throw new InvalidOperationException("EmbeddingModel:Endpoint is not configured.");

        private readonly string _model = configuration["EmbeddingModel:Model"]
            ?? throw new InvalidOperationException("EmbeddingModel:Model is not configured.");

        public async Task<Vector> GenerateAsync(string text, CancellationToken ct = default)
        {
            var client = httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync
            (
                $"{_endpoint}/embeddings",
                new { model = _model, input = text },
                ct
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ct)
                ?? throw new InvalidOperationException("Empty response from embedding API.");

            return new Vector(result.Data[0].Embedding);
        }

        private sealed class EmbeddingResponse
        {
            [JsonPropertyName("data")]
            public required EmbeddingData[] Data { get; set; }
        }

        private sealed class EmbeddingData
        {
            [JsonPropertyName("embedding")]
            public required float[] Embedding { get; set; }
        }
    }
}
