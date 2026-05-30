using AthenaUnionAI.Application.Interfaces.Services;
using AthenaUnionAI.Infrastructure.Data;
using AthenaUnionAI.Infrastructure.Jobs;
using AthenaUnionAI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AthenaUnionAI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public void AddServices(IConfiguration configuration)
            {
                services.AddHttpClient();

                services.AddDbContext<AthenaDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"),
                        npgsql => npgsql.UseVector()
                    )
                );

                services.AddScoped<IGenerativeAIService, SemanticKernelService>();
                services.AddScoped<MarkdownChunkingService>();
                services.AddScoped<EmbeddingService>();
                services.AddScoped<DocumentSearchService>();
                services.AddScoped<DocumentIndexingService>();

                services.AddHostedService<DocumentIndexerJob>();
            }
        }
    }
}