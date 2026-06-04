# Athena Union AI

Backend de inteligência artificial da plataforma Athena Students Union, responsável por orquestrar assistentes conversacionais, geração de embeddings e busca semântica sobre a documentação.

## Visão geral

O serviço expõe dois modos de chat via streaming SSE:

- **Assistant** (`POST /api/chat/assistant`) — executa ações no sistema (criar disciplinas, consultar dados) utilizando plugins via Semantic Kernel. Integrado ao frontend da plataforma.
- **Documentation** (`POST /api/chat/documentation`) — responde perguntas sobre a documentação técnica com base em RAG (Retrieval-Augmented Generation). Integrado ao portal de documentação.

## Funcionalidades

- Streaming de respostas token a token via Server-Sent Events (SSE)
- RAG com pgvector: indexação automática de documentação Markdown e busca semântica por cosseno
- Plugins Semantic Kernel para integração com a API Institution (criação de disciplinas, etc.)
- Indexação assíncrona de documentos em background via `IHostedService`
- Prompts separados por modo: `AssistantPattern.txt` (ações) e `DocumentationPattern.txt` (dúvidas)

## Stack

| Camada | Tecnologia |
|---|---|
| Framework | ASP.NET Core 10 |
| IA / Orquestração | Microsoft Semantic Kernel |
| Embeddings / Busca | PostgreSQL + pgvector |
| ORM | EF Core 10 |
| Arquitetura | Clean Architecture (Domain → Application → Infrastructure → API) |

## Estrutura

```
AthenaUnionAI/                  # Controllers, Program.cs, entry point
AthenaUnionAI.Application/      # Interfaces, DTOs, modelos
AthenaUnionAI.Domain/           # Entidades, prompts (embedded resources)
AthenaUnionAI.Infrastructure/   # EF DbContext, plugins, serviços de IA
```

## Configuração necessária

Copie `appsettings.json` e preencha os valores marcados com `YOUR_`:

```json
{
  "AutomationModel": { "Model": "<modelo>", "Endpoint": "<url-lm-studio>" },
  "EmbeddingModel":  { "Model": "<modelo>", "Endpoint": "<url-lm-studio>" },
  "Jira":            { "Token": "YOUR_JIRA_API_TOKEN" },
  "DocumentationSource": { "Path": "<caminho-absoluto-para-docs/docs>" }
}
```

## Repositórios relacionados

| Serviço | Repositório |
|---|---|
| Frontend | [athena-students-union-front](https://github.com/marceloarauj/athena-students-union-front) |
| Documentação | [athena-docs](https://github.com/marceloarauj/athena-docs) |
| Escola / Turmas | [athena-institution-service](https://github.com/marceloarauj/athena-institution-service) |
| Biblioteca compartilhada | [AthenaUnionLibrary](https://github.com/marceloarauj/AthenaUnionLibrary) |
