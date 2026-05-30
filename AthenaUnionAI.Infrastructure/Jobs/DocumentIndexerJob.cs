using AthenaUnionAI.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AthenaUnionAI.Infrastructure.Jobs
{
    public class DocumentIndexerJob(IServiceProvider serviceProvider) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<DocumentIndexingService>();

                    await service.IndexDocumentsAsync(stoppingToken);

                }catch (Exception ex)
                {
                    Console.WriteLine($"Error in DocumentIndexerJob: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}