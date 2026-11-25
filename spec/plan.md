# Plano Técnico de Implementação — UrlShortenerApi

Este documento descreve como os requisitos definidos em [spec.md](./spec.md) serão implementados respeitando os princípios de [constitution.md](./constitution.md).

---

## 1. Estrutura do Projeto

```text
UrlShortenerApi/
├── spec/
│   ├── constitution.md
│   ├── spec.md
│   ├── plan.md
│   └── task.md
├── src/
│   └── UrlShortenerApi/
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── Migrations/
│       ├── Models/
│       │   ├── ShortenedUrl.cs
│       │   └── Dtos.cs
│       ├── Services/
│       │   ├── ShortCodeGenerator.cs
│       │   └── UrlShortenerService.cs
│       ├── Controllers/
│       │   └── UrlsController.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
└── tests/
    └── UrlShortenerApi.Tests/
        ├── ShortCodeGeneratorTests.cs
        ├── UrlShortenerServiceTests.cs
        └── UrlsControllerIntegrationTests.cs
```

---

## 2. Decisões Técnicas Detalhadas

### 2.1. `Controllers/UrlsController.cs`
- Controller único com `[ApiController]` para validação automática via **Data Annotations** em `CreateUrlRequest`.
- Rotas explícitas nos métodos:
  - `[HttpPost("api/urls")]` -> cria URL encurtada (`201 Created` ou `400 ProblemDetails`).
  - `[HttpGet("{shortCode}")]` -> resolve e redireciona (`302 Found` ou `404 ProblemDetails`).
  - `[HttpGet("api/urls/{shortCode}/stats")]` -> retorna estatísticas (`200 OK` ou `404 ProblemDetails`).
- `AddOpenApi()`, `MapOpenApi()` + `MapScalarApiReference()` (dev via `Scalar.AspNetCore`), `AddProblemDetails()`, `UseExceptionHandler()` e `MapHealthChecks("/health")` configurados no `Program.cs` (**.NET 10**).

### 2.2. `Models/ShortenedUrl.cs` e `Models/Dtos.cs`
- `ShortenedUrl`: entidade EF Core (`Id`, `OriginalUrl`, `ShortCode`, `ClickCount`, `CreatedAtUtc`, `LastAccessedAtUtc`).
- `Dtos.cs`:
  - `CreateUrlRequest` (record com `[Required]`, `[MaxLength(2048)]`).
  - `ShortUrlResponse` e `UrlStatsResponse` (records simples + extensão/método de mapeamento manual).
  - `Result<T>` enxuto para representar sucesso ou erro esperado (`Validation`, `NotFound`, `Unexpected`) sem lançar exceções de fluxo.

### 2.3. `Services/ShortCodeGenerator.cs` e `Services/UrlShortenerService.cs`
- `ShortCodeGenerator`: gera 7 caracteres Base62 (`0-9A-Za-z`) usando `RandomNumberGenerator.GetInt32(62)`.
- `UrlShortenerService`:
  - **Criar:** `Trim()`, valida URL absoluta (`http`/`https`, `<= 2048` chars), gera `ShortCode` com até 5 tentativas em caso de colisão e salva no `AppDbContext`.
  - **Resolver (`+1` clique):** executa `ExecuteUpdateAsync` atômico incrementando `ClickCount` e atualizando `LastAccessedAtUtc`, retornando a `OriginalUrl` se encontrada.
  - **Stats:** consulta somente leitura (`AsNoTracking()`) sem incrementar cliques.

### 2.4. `Data/AppDbContext.cs` (EF Core + SQLite)
- Pacotes: `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.EntityFrameworkCore.Design`.
- Connection string configurada em `appsettings.json` (`ConnectionStrings:DefaultConnection`, padrão `Data Source=urlshortener.db`).
- Índice `UNIQUE` em `ShortCode`, `OriginalUrl` com `MaxLength(2048)` e `ClickCount` com default `0`.

---

## 3. Estratégia de Testes

1. **Testes Unitários:**
   - `ShortCodeGeneratorTests`: comprimento `7` e caracteres Base62.
   - `UrlShortenerServiceTests`: validação de URL (`http`, `https`, `javascript:`, relativas, `> 2048` chars) e retry em colisão.
2. **Testes de Integração (`WebApplicationFactory<Program>`):**
   - SQLite em memória (`DataSource=:memory:` com conexão aberta) cobrindo `POST /api/urls`, `GET /{shortCode}`, `GET /api/urls/{shortCode}/stats`, `GET /health` e case-sensitivity de `shortCode`.
