namespace AthenaUnionAI.Application.Interfaces.Services
{
    public interface IGenerativeAIService
    {
        IAsyncEnumerable<string> StreamAsync(string prompt, CancellationToken ct);
    }
}