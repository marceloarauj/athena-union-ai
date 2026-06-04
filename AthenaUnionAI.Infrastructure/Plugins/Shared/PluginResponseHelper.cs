using System.Net;
using System.Net.Http.Json;
using AthenaUnionAI.Infrastructure.Plugins.Contracts;

namespace AthenaUnionAI.Infrastructure.Plugins.Shared
{
    /// <summary>
    /// Shared helpers for plugins that call the Athena microservices. Centralizes
    /// permission handling and error formatting so every action surfaces a
    /// consistent, user-friendly message that the assistant can relay verbatim.
    /// </summary>
    public static class PluginResponseHelper
    {
        /// <summary>
        /// True when the downstream service rejected the call because the user is
        /// not authenticated (401) or lacks the required permission (403).
        /// </summary>
        public static bool IsPermissionDenied(HttpStatusCode status)
            => status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

        /// <summary>
        /// Message shown when the user is not allowed to perform <paramref name="action"/>.
        /// </summary>
        public static string PermissionDeniedMessage(string action)
            => $"Você não possui permissão para {action}. " +
               "Caso precise dessa ação, solicite o acesso a um administrador da instituição.";

        /// <summary>
        /// Builds a human-readable failure message out of the standardized API envelope.
        /// </summary>
        public static string FailureMessage<T>(string action, InstitutionApiResponse<T>? body)
        {
            var reason = string.IsNullOrWhiteSpace(body?.Message)
                ? "ocorreu um erro inesperado"
                : body!.Message!.Trim();

            var details = body?.Errors is { Count: > 0 }
                ? $" Detalhes: {string.Join("; ", body.Errors)}."
                : string.Empty;

            return $"Não foi possível {action}: {reason}.{details}";
        }

        /// <summary>
        /// Safely deserializes the standardized API envelope; returns <c>null</c>
        /// when the body is missing or not in the expected format.
        /// </summary>
        public static async Task<InstitutionApiResponse<T>?> ReadEnvelopeAsync<T>(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            try
            {
                return await response.Content
                    .ReadFromJsonAsync<InstitutionApiResponse<T>>(cancellationToken);
            }
            catch
            {
                return null;
            }
        }
    }
}
