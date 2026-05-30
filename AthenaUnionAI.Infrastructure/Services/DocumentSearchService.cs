using AthenaUnionAI.Domain.Models.Entities;
using AthenaUnionAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace AthenaUnionAI.Infrastructure.Services
{
    public class DocumentSearchService(AthenaDbContext dbContext)
    {
        public async Task<IReadOnlyList<DocumentationChunkEntity>> SearchAsync
        (
            Vector queryEmbedding,
            int topK = 5,
            CancellationToken ct = default
        )
        {
            return await dbContext.DocumentationChunks
                .OrderBy(c => c.Embedding.CosineDistance(queryEmbedding))
                .Take(topK)
                .ToListAsync(ct);
        }
    }
}
