using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using AthenaUnionAI.Application.Interfaces.Services;
using AthenaUnionAI.Application.Models.Enums;
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
        private readonly ServiceType _serviceType;

        public SemanticKernelService
        (
            ServiceType serviceType,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            EmbeddingService embeddingService,
            DocumentSearchService documentSearchService
        )
        {
            _serviceType = serviceType;

            var endpoint = configuration["AutomationModel:Endpoint"];
            var model = configuration["AutomationModel:Model"];

            if (endpoint == null || model == null)
                throw new InvalidOperationException("Endpoint and Model must be configured.");

            var builder = BuildKernel(httpClientFactory);
            builder.AddOpenAIChatCompletion(modelId: model, endpoint: new Uri(endpoint), apiKey: "none");

            _kernel = builder.Build();
            _chat = _kernel.GetRequiredService<IChatCompletionService>();
            _embeddingService = embeddingService;
            _documentSearchService = documentSearchService;
        }

        private IKernelBuilder BuildKernel(IHttpClientFactory httpClientFactory)
        {
            var builder = Kernel.CreateBuilder();

            if (_serviceType == ServiceType.Assistant)
                builder.Plugins.AddFromObject(new DisciplinePlugin(httpClientFactory.CreateClient()));

            return builder;
        }

        public async IAsyncEnumerable<string> StreamAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var assembly = Assembly.Load("AthenaUnionAI.Domain");
            var resourceName = _serviceType == ServiceType.Documentation
                ? "AthenaUnionAI.Domain.Models.Prompts.DocumentationPattern.txt"
                : "AthenaUnionAI.Domain.Models.Prompts.AssistantPattern.txt";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var template = reader.ReadToEnd();

            var context = _serviceType == ServiceType.Documentation
                ? await BuildContextAsync(prompt, ct)
                : string.Empty;

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
