using System.Text.Json.Serialization;

namespace AthenaUnionAI.Infrastructure.Plugins.Contracts
{
    public record DisciplineData
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("studyHours")]
        public int StudyHours { get; init; }

        [JsonPropertyName("credits")]
        public int Credits { get; init; }
    }
}
