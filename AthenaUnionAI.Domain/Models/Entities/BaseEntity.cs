using System.ComponentModel.DataAnnotations.Schema;

namespace AthenaUnionAI.Domain.Models.Entities
{
    public class BaseEntity
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}