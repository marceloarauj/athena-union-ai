using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;

namespace AthenaUnionAI.Infrastructure.Plugins
{
    public class TestePlugin(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        [KernelFunction("list_tests")]
        [Description("Lists all tests from the API.")]
        public async Task<List<string>> GetTests()
        {
            var tests = await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:7169/api/teste");

            return tests ?? [];
        }
    }
}