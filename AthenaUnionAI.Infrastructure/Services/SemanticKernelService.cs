using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using AthenaUnionAI.Application.Interfaces.Services;
using AthenaUnionAI.Infrastructure.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AthenaUnionAI.Infrastructure.Services
{
    public class SemanticKernelService : IGenerativeAIService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly EmbeddingService _embeddingService;
        private readonly DocumentSearchService _documentSearchService;

        public SemanticKernelService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            EmbeddingService embeddingService,
            DocumentSearchService documentSearchService)
        {
            var endpoint = configuration["AutomationModel:Endpoint"]
                ?? throw new ArgumentNullException("AutomationModel:Endpoint is not configured.");
                
            var model = configuration["AutomationModel:Model"]
                ?? throw new ArgumentNullException("AutomationModel:Model is not configured.");

            var builder = Kernel.CreateBuilder();
            builder.Plugins.AddFromObject(new TestePlugin(httpClientFactory.CreateClient()));
            builder.AddOpenAIChatCompletion(modelId: model, endpoint: new Uri(endpoint), apiKey: "none");

            _kernel = builder.Build();
            _chat = _kernel.GetRequiredService<IChatCompletionService>();
            _embeddingService = embeddingService;
            _documentSearchService = documentSearchService;
        }

        public async IAsyncEnumerable<string> StreamAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var assembly = Assembly.Load("AthenaUnionAI.Domain");
            using var stream = assembly.GetManifestResourceStream("AthenaUnionAI.Domain.Models.Prompts.ResponsePattern.txt");
            using var reader = new StreamReader(stream!);
            var template = reader.ReadToEnd();

            var context = await BuildContextAsync(prompt, ct);

            var finalPrompt = template
                .Replace("{{context}}", context)
                .Replace("{{prompt}}", prompt);

            var history = new ChatHistory();
            history.AddUserMessage(finalPrompt);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            await foreach (var chunk in _chat.GetStreamingChatMessageContentsAsync(history, settings, _kernel, ct))
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                    yield return chunk.Content;
            }
        }

        private async Task<string> BuildContextAsync(string prompt, CancellationToken ct)
        {
            try
            {
                var queryEmbedding = await _embeddingService.GenerateAsync(prompt, ct);
                var chunks = await _documentSearchService.SearchAsync(queryEmbedding, topK: 5, ct);

                if (chunks.Count == 0)
                    return "Nenhuma documentação relevante encontrada.";

                var sb = new StringBuilder();
                foreach (var chunk in chunks)
                {
                    sb.AppendLine($"### {chunk.HeadingPath}");
                    sb.AppendLine(chunk.Content);
                    sb.AppendLine();
                }

                return sb.ToString().Trim();
            }
            catch
            {
                return "Documentação não disponível no momento.";
            }
        }
    }
}
