using AthenaUnionAI.Domain.Models.Entities;
using AthenaUnionAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AthenaUnionAI.Infrastructure.Services
{
    public class DocumentIndexingService
    (
        MarkdownChunkingService chunkingService,
        EmbeddingService embeddingService,
        AthenaDbContext dbContext,
        IConfiguration configuration,
        ILogger<DocumentIndexingService> logger
    )
    {
        public async Task IndexDocumentsAsync(CancellationToken cancellationToken)
        {
            var docsPath = configuration["DocumentationSource:Path"]
                ?? throw new InvalidOperationException("DocumentationSource:Path is not configured.");

            if (!Directory.Exists(docsPath))
            {
                logger.LogWarning("Documentation source path not found: {Path}", docsPath);
                return;
            }

            var chunks = chunkingService.ChunkDirectory(docsPath).ToList();

            var indexed = 0;
            var skipped = 0;

            foreach (var chunk in chunks)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    var exists = await dbContext.DocumentationChunks
                        .AnyAsync(chunk => chunk.FileName == chunk.FileName && chunk.Section == chunk.Section, cancellationToken);

                    if (exists)
                    {
                        skipped++;
                        continue;
                    }

                    var embedding = await embeddingService.GenerateAsync(chunk.Content, cancellationToken);

                    dbContext.DocumentationChunks.Add(new DocumentationChunkEntity
                    {
                        Content = chunk.Content,
                        Embedding = embedding,
                        FileName = chunk.FileName,
                        Section = chunk.Section,
                        HeadingPath = chunk.HeadingPath
                    });

                    indexed++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to index chunk: {File} > {Section}", chunk.FileName, chunk.Section);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
