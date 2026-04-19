using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AthenaUnionAI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AthenaUnionAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(IGenerativeAIService generativeAIService) : ControllerBase
    {
        private readonly IGenerativeAIService _generativeAIService = generativeAIService;

        [HttpGet("stream")]
        public async Task Stream([FromQuery] string prompt, CancellationToken ct)
        {
            Response.Headers.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?.DisableBuffering();

            try
            {
                await foreach (var message in _generativeAIService.StreamAsync(prompt, ct))
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