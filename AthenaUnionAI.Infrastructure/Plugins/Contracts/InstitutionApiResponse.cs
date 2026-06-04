using System.Text.Json.Serialization;

namespace AthenaUnionAI.Infrastructure.Plugins.Contracts
{
    /// <summary>
    /// Mirrors the <c>AthenaApiResponse&lt;T&gt;</c> envelope returned by the
    /// Athena microservices, so plugins can read the standardized payload without
    /// taking a hard reference on AthenaUnionLibrary.
    /// </summary>
    public class InstitutionApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }
}
