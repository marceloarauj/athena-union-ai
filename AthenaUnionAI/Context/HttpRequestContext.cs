using AthenaUnionAI.Application.Interfaces.Services;

namespace AthenaUnionAI.Context
{
    public class HttpRequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string? AuthorizationHeader
        {
            get
            {
                var header = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
                return string.IsNullOrWhiteSpace(header) ? null : header;
            }
        }
    }
}
