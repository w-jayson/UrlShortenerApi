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
│       │   ├── Dtos.cs
│       │   ├── Result.cs
│       │   └── ShortenedUrlMappings.cs
│       ├── Services/
│       │   ├── ShortCodeGenerator.cs
│       │   ├── UrlValidator.cs
│       │   ├── CreateShortUrlService.cs
│       │   ├── ResolveUrlService.cs
│       │   └── GetUrlStatsService.cs
│       ├── Controllers/
│       │   └── UrlsController.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
└── tests/
    └── UrlShortenerApi.Tests/
        ├── CustomWebApplicationFactory.cs
        ├── ShortCodeGeneratorTests.cs
        ├── UrlValidatorTests.cs
        ├── CreateShortUrlServiceTests.cs
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

### 2.2. `Models/` (Separados por Papel)
- `ShortenedUrl.cs`: entidade EF Core (`Id`, `OriginalUrl`, `ShortCode`, `ClickCount`, `CreatedAtUtc`, `LastAccessedAtUtc`).
- `Dtos.cs`: somente os contratos HTTP (`CreateUrlRequest` com `[Required]` e `[MaxLength(2048)]`, `ShortUrlResponse` e `UrlStatsResponse`).
- `Result.cs`: `ErrorType` (`Validation`, `NotFound`, `Unexpected`) e `Result<T>` enxuto para representar sucesso ou erro esperado sem lançar exceções de fluxo.
- `ShortenedUrlMappings.cs`: métodos de extensão para mapeamento manual (`ToShortUrlResponse` e `ToStatsResponse`).

### 2.3. `Services/` (Classes de Responsabilidade Única)
- `ShortCodeGenerator`: gera 7 caracteres Base62 (`0-9A-Za-z`) usando `RandomNumberGenerator.GetInt32(62)`.
- `UrlValidator`: aplica `Trim()` e valida URL absoluta (`http`/`https`, obrigatória, `<= 2048` chars), retornando `Result<string>`.
- `CreateShortUrlService`: valida a URL com `UrlValidator`, gera `ShortCode` com até 5 tentativas em caso de colisão no índice único e salva no `AppDbContext`.
- `ResolveUrlService`: executa `ExecuteUpdateAsync` atômico incrementando `ClickCount` (`+1`) e atualizando `LastAccessedAtUtc`, retornando a `OriginalUrl` se encontrada.
- `GetUrlStatsService`: consulta somente leitura (`AsNoTracking()`) sem incrementar cliques e mapeia para `UrlStatsResponse`.

### 2.4. `Data/AppDbContext.cs` (EF Core + SQLite)
- Pacotes: `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.EntityFrameworkCore.Design`.
- Connection string configurada em `appsettings.json` (`ConnectionStrings:DefaultConnection`, padrão `Data Source=urlshortener.db`).
- Índice `UNIQUE` em `ShortCode`, `OriginalUrl` com `MaxLength(2048)` e `ClickCount` com default `0`.

---

## 3. Estratégia de Testes

1. **Testes Unitários:**
   - `ShortCodeGeneratorTests`: comprimento `7` e caracteres Base62.
   - `UrlValidatorTests`: `Trim()`, aceitação de `http`/`https`, rejeição de `javascript:`, relativas, vazias e `> 2048` chars.
   - `CreateShortUrlServiceTests`: criação de URL e retry em colisão de `ShortCode`.
2. **Testes de Integração (`CustomWebApplicationFactory` + `UrlsControllerIntegrationTests`):**
   - `CustomWebApplicationFactory.cs`: encapsula a configuração do `WebApplicationFactory<Program>` com SQLite em memória (`DataSource=:memory:` com conexão aberta).
   - `UrlsControllerIntegrationTests.cs`: cobre `POST /api/urls`, `GET /{shortCode}`, `GET /api/urls/{shortCode}/stats`, `GET /health` e case-sensitivity de `shortCode`.
