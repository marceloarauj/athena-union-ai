using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace AthenaUnionAI.Domain.Models.Entities
{
    [Table("documentation_chunks")]
    public class DocumentationChunkEntity : BaseEntity
    {
        [Column("content")]
        public string Content { get; set; } = string.Empty; 

        [Column("embedding")]
        public Vector Embedding { get; set; } = default!; 
        
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty; 
        
        [Column("section")]
        public string Section { get; set; } = string.Empty; 

        [Column("heading_path")]
        public string HeadingPath { get; set; } = string.Empty;
    }
}