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

            await foreach (var message in _generativeAIService.StreamAsync(prompt, ct))
            {
                await Response.WriteAsync($"data: {message}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
    }
}