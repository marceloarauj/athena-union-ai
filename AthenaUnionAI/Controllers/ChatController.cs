using AthenaUnionAI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace AthenaUnionAI.Controllers
{
    public record ChatRequest(string Prompt);

    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(
        [FromKeyedServices("assistant")] IGenerativeAIService assistantService,
        [FromKeyedServices("documentation")] IGenerativeAIService documentationService) : ControllerBase
    {
        [HttpPost("assistant")]
        public async Task Assistant([FromBody] ChatRequest request, CancellationToken ct)
            => await StreamResponse(assistantService, request.Prompt, ct);

        [HttpPost("documentation")]
        public async Task Documentation([FromBody] ChatRequest request, CancellationToken ct)
            => await StreamResponse(documentationService, request.Prompt, ct);

        private async Task StreamResponse(IGenerativeAIService service, string prompt, CancellationToken ct)
        {
            Response.Headers.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

            try
            {
                await foreach (var message in service.StreamAsync(prompt, ct))
                {
                    var lines = message.Split('\n');
                    var sseEvent = string.Join("\n", lines.Select(l => $"data: {l}")) + "\n\n";
                    await Response.WriteAsync(sseEvent, ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (Exception ex)
            {
                var details = ex.ToString().Replace("\n", " ").Replace("\r", "");
                await Response.WriteAsync($"event: error\ndata: {details}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
    }
}
