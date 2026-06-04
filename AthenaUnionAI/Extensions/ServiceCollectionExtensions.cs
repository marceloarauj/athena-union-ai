using AthenaUnionAI.Application.Interfaces.Services;
using AthenaUnionAI.Application.Models.Enums;
using AthenaUnionAI.Context;
using AthenaUnionAI.Infrastructure.Data;
using AthenaUnionAI.Infrastructure.Jobs;
using AthenaUnionAI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AthenaUnionAI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public void AddServices(IConfiguration configuration)
            {
                services.AddHttpClient();

                services.AddHttpContextAccessor();
                services.AddScoped<IRequestContext, HttpRequestContext>();

                services.AddDbContext<AthenaDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"),
                        npgsql => npgsql.UseVector()
                    )
                );

                services.AddScoped<MarkdownChunkingService>();
                services.AddScoped<EmbeddingService>();
                services.AddScoped<DocumentSearchService>();
                services.AddScoped<DocumentIndexingService>();

                services.AddKeyedScoped<IGenerativeAIService>("assistant", (sp, _) =>
                    ActivatorUtilities.CreateInstance<SemanticKernelService>(sp, ServiceType.Assistant));

                services.AddKeyedScoped<IGenerativeAIService>("documentation", (sp, _) =>
                    ActivatorUtilities.CreateInstance<SemanticKernelService>(sp, ServiceType.Documentation));

                services.AddHostedService<DocumentIndexerJob>();
            }
        }
    }
}