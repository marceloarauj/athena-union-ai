using AthenaUnionAI.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace AthenaUnionAI.Infrastructure.Data
{
    public class AthenaDbContext(DbContextOptions<AthenaDbContext> options) : DbContext(options)
    {
        public DbSet<DocumentationChunkEntity> DocumentationChunks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<DocumentationChunkEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Embedding).HasColumnType("vector(768)");
            });
        }
    }
}
