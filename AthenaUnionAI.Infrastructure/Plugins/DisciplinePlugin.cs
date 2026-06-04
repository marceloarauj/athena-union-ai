using System.ComponentModel;
using System.Net.Http.Json;
using AthenaUnionAI.Infrastructure.Plugins.Contracts;
using AthenaUnionAI.Infrastructure.Plugins.Shared;
using Microsoft.SemanticKernel;

namespace AthenaUnionAI.Infrastructure.Plugins
{
    public class DisciplinePlugin(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        private const string DisciplineEndpoint = "api/Discipline";
        private const int NameMinLength = 3;
        private const int NameMaxLength = 300;
        private const int MaxStudyHours = 10_000;
        private const int MaxCredits = 100;

        [KernelFunction("create_discipline")]
        [Description("Cria/cadastra uma nova disciplina (matéria) na instituição do usuário. " +
                     "Use apenas quando o usuário pedir explicitamente para criar ou cadastrar uma disciplina.")]
        public async Task<string> CreateDisciplineAsync
        (
            [Description("Nome da disciplina. Exemplo: 'Matemática'. Entre 3 e 300 caracteres.")] string name,
            [Description("Carga horária total da disciplina, em horas. Número inteiro positivo.")] int studyHours,
            [Description("Quantidade de créditos da disciplina. Número inteiro positivo.")] int credits,
            [Description("Indica se a disciplina gera cobrança/pagamento para o aluno. Padrão: false.")] bool chargePayment = false,
            CancellationToken cancellationToken = default
        )
        {
            if (ValidateCreateInput(name, studyHours, credits) is { } validationError)
                return validationError;

            var payload = new
            {
                name = name.Trim(),
                studyHours,
                credits,
                chargePayment,
                topics = Array.Empty<object>()
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(DisciplineEndpoint, payload, cancellationToken);
            }
            catch (Exception ex)
            {
                return "Não consegui me comunicar com o serviço de instituição para criar a disciplina no momento. " +
                       $"Tente novamente em instantes. (Detalhe técnico: {ex.Message})";
            }

            if (PluginResponseHelper.IsPermissionDenied(response.StatusCode))
                return PluginResponseHelper.PermissionDeniedMessage("criar disciplinas nesta instituição");

            var body = await PluginResponseHelper.ReadEnvelopeAsync<DisciplineData>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || body is { Success: false })
                return PluginResponseHelper.FailureMessage("criar a disciplina", body);

            var created = body?.Data;
            return created is null
                ? $"Disciplina '{name.Trim()}' criada com sucesso."
                : $"Disciplina '{created.Name}' criada com sucesso! " +
                  $"(ID: {created.Id}, carga horária: {created.StudyHours}h, créditos: {created.Credits}.)";
        }

        private static string? ValidateCreateInput(string name, int studyHours, int credits)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Para criar a disciplina, preciso que você informe o nome dela.";

            var trimmed = name.Trim();
            if (trimmed.Length < NameMinLength)
                return $"O nome da disciplina é muito curto. Use ao menos {NameMinLength} caracteres.";
            if (trimmed.Length > NameMaxLength)
                return $"O nome da disciplina é muito longo. Use no máximo {NameMaxLength} caracteres.";

            if (studyHours <= 0)
                return "A carga horária deve ser um número inteiro positivo (maior que zero). " +
                       "Por exemplo: 60 horas.";
            if (studyHours > MaxStudyHours)
                return $"A carga horária informada parece inválida. Use no máximo {MaxStudyHours} horas.";

            if (credits <= 0)
                return "A quantidade de créditos deve ser um número inteiro positivo (maior que zero). " +
                       "Por exemplo: 4 créditos.";
            if (credits > MaxCredits)
                return $"A quantidade de créditos informada parece inválida. Use no máximo {MaxCredits} créditos.";

            return null;
        }
    }
}
