using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AthenaUnionAI.Application.Interfaces.Services;
using AthenaUnionAI.Infrastructure.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AthenaUnionAI.Infrastructure.Services
{
    public class SemanticKernelService: IGenerativeAIService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;

        public SemanticKernelService(IConfiguration configuration, IHttpClientFactory httpClientFactory) 
        {
            var endpoint = configuration["Ollama:Endpoint"] ?? throw new ArgumentNullException("Ollama endpoint is not configured.");
            var model = configuration["Ollama:Model"] ?? throw new ArgumentNullException("Ollama model is not configured.");

            var builder = Kernel.CreateBuilder();

            builder.Plugins.AddFromObject(new TestePlugin(httpClientFactory.CreateClient()));

            builder.AddOpenAIChatCompletion
            (
                modelId: model,
                endpoint: new Uri(endpoint),
                apiKey: "none"
            );

            _kernel = builder.Build();
            _chat = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async IAsyncEnumerable<string> StreamAsync(string prompt, [EnumeratorCancellation] CancellationToken ct=default)
        {
            var request = new
            {
                model = "llama3",
                prompt = prompt,
                stream = true
            };
            
            HttpClient http = new();
            
            var response = await http.PostAsJsonAsync
            (
                "http://localhost:11434/api/generate",
                request,
                ct
            );

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (true)
            {
                var line = await reader.ReadLineAsync();

                if (line is null)
                    yield break;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var json = JsonDocument.Parse(line);

                var token = json.RootElement
                    .GetProperty("response")
                    .GetString();

                if (!string.IsNullOrEmpty(token))
                {
                    yield return token;
                }
            }
        }
    }
}