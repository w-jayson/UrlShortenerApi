# Tarefas de Implementação — UrlShortenerApi

Checklist de estudo e execução baseado em [spec.md](./spec.md) e [plan.md](./plan.md).
Cada fase termina com um **Checkpoint** para conferência e entendimento antes de avançar.

---

## Fase 1 — Setup do Projeto e Pipeline HTTP

- [x] **1.1. Criar solução e projetos**
  - Criar `UrlShortenerApi.sln`, projeto Web API `src/UrlShortenerApi` em **.NET 10** (`net10.0`, `<Nullable>enable</Nullable>`) e projeto xUnit `tests/UrlShortenerApi.Tests` referenciando a API.
  - Criar `.gitignore` padrão .NET (`bin/`, `obj/`, `*.db`, `*.db-shm`, `*.db-wal`).
  - *Conceito:* separação entre solução (`.sln`), código da aplicação (`src/`) e testes (`tests/`), além de `Nullable` habilitado para evitar `NullReferenceException` em tempo de compilação.
- [x] **1.2. Instalar pacotes NuGet**
  - `Microsoft.EntityFrameworkCore.Sqlite` (provedor de banco SQLite para EF Core).
  - `Microsoft.EntityFrameworkCore.Design` (ferramenta de design-time para gerar migrations).
  - `Microsoft.AspNetCore.OpenApi` (geração nativa do documento OpenAPI no .NET 10).
  - `Scalar.AspNetCore` (interface interativa Scalar para explorar e testar os endpoints).
- [x] **1.3. Configurar `Program.cs`**
  - Registrar `AddControllers()`, `AddOpenApi()`, `AddProblemDetails()` e `AddHealthChecks()`.
  - Configurar pipeline: `UseExceptionHandler()` (converte exceções não tratadas em RFC 9457 `ProblemDetails`), HTTPS, `MapOpenApi()` + `MapScalarApiReference()` (em `IsDevelopment()`), `MapControllers()` e `MapHealthChecks("/health")`.
  - Expor `public partial class Program;` ao final para permitir `WebApplicationFactory<Program>` nos testes de integração.
  - *Conceito:* diferença entre registro de serviços (`builder.Services`) e pipeline de middlewares (`app.Use...` / `app.Map...`), e por que `UseExceptionHandler()` + `AddProblemDetails()` padronizam respostas de erro.

**Checkpoint Fase 1:**
- Rodar `dotnet build` sem erros.
- Subir a API (`dotnet run`), testar `GET /health` (`200 Healthy`) e abrir o Scalar (`/scalar/v1`).
- Conferir e explicar o papel de cada serviço e middleware no `Program.cs`.

---

## Fase 2 — Dados, Modelos e Banco (EF Core + SQLite)

- [x] **2.1. Criar `Models/ShortenedUrl.cs`**
  - Entidade com `Id` (`int`), `OriginalUrl` (`string`), `ShortCode` (`string`), `ClickCount` (`long`), `CreatedAtUtc` (`DateTime`) e `LastAccessedAtUtc` (`DateTime?`).
  - *Conceito:* modelagem simples de entidade EF Core e uso de UTC (`DateTime.UtcNow`) para consistência de datas.
- [ ] **2.2. Separar `Models/Dtos.cs`, `Models/Result.cs` e `Models/ShortenedUrlMappings.cs`**
  - `Models/Dtos.cs`: manter apenas os contratos HTTP (`CreateUrlRequest` com `[Required]` e `[MaxLength(2048)]`, `ShortUrlResponse` e `UrlStatsResponse`).
  - `Models/Result.cs`: `ErrorType` (`Validation`, `NotFound`, `Unexpected`) e `Result<T>` para falhas de negócio sem exceções de fluxo.
  - `Models/ShortenedUrlMappings.cs`: métodos de extensão `ToShortUrlResponse` e `ToStatsResponse`.
  - *Conceito:* separação por papel em arquivos pequenos (contratos HTTP vs tipo auxiliar de fluxo vs mapeamento manual), uso de `record` para imutabilidade, Data Annotations para validação de borda e Result Pattern.
- [x] **2.3. Criar `Data/AppDbContext.cs`**
  - Configurar `DbSet<ShortenedUrl>` e Fluent API em `OnModelCreating`:
    - Índice `UNIQUE` em `ShortCode` (garante unicidade no banco e busca rápida `O(log n)`).
    - `MaxLength(2048)` e `IsRequired()` em `OriginalUrl`.
    - `MaxLength(7)` e `IsRequired()` em `ShortCode`.
    - Valor default `0` em `ClickCount`.
  - *Conceito:* Fluent API vs Data Annotations, impacto do índice único na prevenção de colisão em concorrência e diferenciação de case no SQLite.
- [x] **2.4. Connection string e Migration inicial**
  - Configurar `ConnectionStrings:DefaultConnection` em `appsettings.json` (`Data Source=urlshortener.db`).
  - Registrar `AddDbContext<AppDbContext>` no `Program.cs` (lendo a connection string).
  - Gerar migration `InitialCreate` (`dotnet ef migrations add InitialCreate`) e aplicar (`dotnet ef database update`).
  - *Conceito:* como o EF Core versiona o schema via Migrations e o que o arquivo de migration gerado cria no SQLite.

**Checkpoint Fase 2:**
- Inspecionar o arquivo de migration gerado em `Data/Migrations/` (`Up` e `Down`, criação da tabela e do índice `UNIQUE`).
- Confirmar criação do `.db` e `dotnet build` limpo.
- Saber explicar o fluxo DTO -> Entidade -> `AppDbContext` -> SQLite.

---

## Fase 3 — Regras de Negócio e Controller

- [x] **3.1. Criar `Services/ShortCodeGenerator.cs`**
  - Gerar código de 7 caracteres usando alfabeto Base62 (`0-9A-Za-z`) com `RandomNumberGenerator.GetInt32(62)`.
  - *Conceito:* por que usar `RandomNumberGenerator` (criptograficamente seguro e thread-safe) em vez de `System.Random`, e por que $62^7$ (~3,5 trilhões) reduz drasticamente colisões.
- [x] **3.2. Criar `Services/UrlValidator.cs`**
  - Aplicar `Trim()` e validar obrigatoriedade, limite `<= 2048` chars e `Uri.TryCreate` absoluto com esquema `http` ou `https`, retornando `Result<string>`.
  - *Conceito:* validação pura e isolada (classe estática sem dependência de banco ou DI), fácil de ler e testar unitariamente.
- [x] **3.3. Criar `Services/CreateShortUrlService.cs`**
  - Validar a URL via `UrlValidator`, gerar `ShortCode` com `ShortCodeGenerator` em até 5 tentativas caso haja violação de índice único no `SaveChangesAsync` (desanexando a entidade que falhou) e retornar `Result<ShortUrlResponse>`.
  - *Conceito:* tratamento de colisão com retry limitado e comportamento do Change Tracker do EF Core em falhas de persistência.
- [x] **3.4. Criar `Services/ResolveUrlService.cs`**
  - Usar `ExecuteUpdateAsync` filtrando por `ShortCode` para incrementar `ClickCount` (`+ 1`) e atualizar `LastAccessedAtUtc` atomicamente no banco; se `rowsAffected == 0`, retornar `NotFound`; caso contrário buscar `OriginalUrl` com `AsNoTracking()` e retornar `Result<string>`.
  - *Conceito:* prevenção de race condition na contagem de cliques (`ExecuteUpdateAsync` vs read-modify-write).
- [x] **3.5. Criar `Services/GetUrlStatsService.cs`**
  - Buscar com `AsNoTracking()` (somente leitura, sem rastreamento do Change Tracker e sem incrementar cliques) e mapear para `UrlStatsResponse`.
  - *Conceito:* `AsNoTracking()` para performance em consultas somente leitura.
- [ ] **3.6. Criar `Controllers/UrlsController.cs`**
  - Decorado com `[ApiController]` (ativa validação automática de `ModelState` retornando `400 ValidationProblemDetails`).
  - `[HttpPost("api/urls")]`: recebe `CreateUrlRequest`, monta URL base da requisição (`Request.Scheme` + `Request.Host`), chama `CreateShortUrlService` e retorna `201 Created` (com header `Location`) ou `Problem(...)` em erro.
  - `[HttpGet("{shortCode}")]`: chama `ResolveUrlService` e retorna `Redirect(originalUrl)` (`302 Found`) ou `404 ProblemDetails`.
  - `[HttpGet("api/urls/{shortCode}/stats")]`: chama `GetUrlStatsService` e retorna `200 OK` ou `404 ProblemDetails`.
  - Registrar `ShortCodeGenerator`, `CreateShortUrlService`, `ResolveUrlService` e `GetUrlStatsService` no container DI (`Program.cs`).
  - *Conceito:* ciclo de vida no DI (`Scoped` vs `Singleton`), diferença entre `301` (permanente/cacheado pelo navegador) e `302` (temporário, permite contar todos os cliques) e mapeamento de `Result<T>` para códigos HTTP.

**Checkpoint Fase 3:**
- Testar manualmente via Scalar (`/scalar/v1`) / `.http` / `curl`:
  1. `POST /api/urls` com URL válida (`201`) e inválida (`400 ProblemDetails`).
  2. `GET /{shortCode}` verificando resposta `302` e header `Location`.
  3. `GET /api/urls/{shortCode}/stats` confirmando incremento de `clickCount` e preenchimento de `lastAccessedAtUtc`, sem alterar `clickCount` ao consultar stats.
  4. `GET /{inexistente}` retornando `404 ProblemDetails`.
- Saber explicar por que `302` é essencial para analytics e como `ExecuteUpdateAsync` evita perda de cliques concorrentes.

---

## Fase 4 — Testes Automatizados e Verificação Final

- [ ] **4.1. Testes unitários (`tests/UrlShortenerApi.Tests/`)**
  - `ShortCodeGeneratorTests`: valida comprimento exato `7` e se todos os caracteres pertencem ao conjunto Base62 (`0-9A-Za-z`).
  - `UrlValidatorTests`: valida `Trim()`, aceitação de `http`/`https` e rejeição de `javascript:`, URLs relativas, vazias e `> 2048` chars.
  - `CreateShortUrlServiceTests`: valida criação de URL e comportamento de retry em colisão de `ShortCode`.
  - *Conceito:* isolamento de regra de negócio em testes unitários rápidos.
- [ ] **4.2. Testes de integração (`CustomWebApplicationFactory` e `UrlsControllerIntegrationTests`)**
  - `CustomWebApplicationFactory.cs`: isolar a configuração do `WebApplicationFactory<Program>` substituindo o SQLite em disco por SQLite em memória (`DataSource=:memory:` com conexão aberta durante o teste).
  - `UrlsControllerIntegrationTests.cs`: cobrir fluxo completo (`POST /api/urls` -> `GET /{shortCode}` `302` sem seguir redirect -> `GET /api/urls/{shortCode}/stats` `clickCount == 1` -> `404` em código inexistente -> case-sensitivity do `shortCode` -> `GET /health`).
  - *Conceito:* separação entre infraestrutura de host de teste e casos de teste, e diferença entre teste unitário e teste de integração end-to-end no pipeline ASP.NET Core real.
- [ ] **4.3. Verificação final**
  - Executar `dotnet build` e `dotnet test` garantindo 100% de aprovação.
  - Garantir que todos os arquivos `.cs` possuam comentário de propósito no topo e explicações objetivas nos trechos principais, conforme [constitution.md](./constitution.md).
  - Revisar código contra [constitution.md](./constitution.md) e [spec.md](./spec.md) e marcar todos os checkboxes de `spec/task.md`.

**Checkpoint Fase 4:**
- Rodar `dotnet test` e confirmar todos os testes passando.
- Revisar o fluxo completo da requisição HTTP até o SQLite para consolidar o entendimento.
