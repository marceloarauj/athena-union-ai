namespace AthenaUnionAI.Application.Interfaces.Services
{
    /// <summary>
    /// Exposes data from the current incoming HTTP request to lower layers
    /// (Infrastructure plugins) without leaking AspNetCore.Http concerns.
    /// </summary>
    public interface IRequestContext
    {
        /// <summary>
        /// The raw "Authorization" header (e.g. "Bearer eyJ...") of the current
        /// request, or <c>null</c> when the caller is anonymous. Used to forward
        /// the user's identity/permissions to downstream microservices.
        /// </summary>
        string? AuthorizationHeader { get; }
    }
}
