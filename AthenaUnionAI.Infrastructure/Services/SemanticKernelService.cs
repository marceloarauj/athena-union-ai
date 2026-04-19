using System.Runtime.CompilerServices;
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
            var endpoint = configuration["AutomationModel:Endpoint"] ?? throw new ArgumentNullException("AutomationModel endpoint is not configured.");
            var model = configuration["AutomationModel:Model"] ?? throw new ArgumentNullException("AutomationModel model is not configured.");

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

        public async IAsyncEnumerable<string> StreamAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var history = new ChatHistory();
            history.AddUserMessage(prompt);

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
    }
}