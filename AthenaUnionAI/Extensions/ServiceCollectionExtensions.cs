using AthenaUnionAI.Application.Interfaces.Services;
using AthenaUnionAI.Infrastructure.Services;

namespace AthenaUnionAI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public void AddServices()
            {
                services.AddHttpClient();
                services.AddScoped<IGenerativeAIService, SemanticKernelService>();
            }
        }
    }
}