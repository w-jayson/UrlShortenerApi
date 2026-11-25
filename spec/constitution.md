# Constituição do Projeto — UrlShortenerApi

Este documento define os princípios fundamentais, padrões técnicos e restrições que governam todas as decisões de arquitetura, implementação e evolução da **UrlShortenerApi**.

---

## 1. Propósito e Escopo

A **UrlShortenerApi** é uma API REST em **.NET (ASP.NET Core / C#)** para encurtamento de URLs, redirecionamento rápido e coleta de analytics básico (contagem de cliques e metadados essenciais de acesso), utilizando **SQLite** como banco de dados inicial para manter o setup simples e portátil.

---

## 2. Princípios Arquiteturais

1. **Arquitetura Enxuta (Projeto Único por Responsabilidade):**
   - `Data/`: `AppDbContext` (EF Core + SQLite) e `Migrations/`.
   - `Models/`: Entidade `ShortenedUrl` e `Dtos.cs` (`records` com Data Annotations e mapeamento manual).
   - `Services/`: `ShortCodeGenerator` e `UrlShortenerService` (regras de negócio: criar, resolver com `+1` clique atômico, consultar stats) usando `Result<T>` para falhas esperadas.
   - `Controllers/`: `UrlsController` (`[ApiController]`), validação automática via Data Annotations e conversão de falhas para `ProblemDetails`.
2. **Fluxo Explícito sem Over-Engineering:**
   - Sem múltiplos projetos, sem MediatR, sem AutoMapper/Mapster e sem abstrações especulativas.
   - Mapeamento manual entre Entidades e DTOs.
3. **Spec-Driven Development (SDD):**
   - Nenhuma funcionalidade é implementada sem estar descrita em `spec.md`, planejada em `plan.md` e quebrada em tarefas em `task.md`.
   - Mudanças de contrato ou comportamento exigem atualização prévia da especificação.
4. **Contratos Explícitos e Padronizados:**
   - Todas as respostas de erro devem seguir o padrão **RFC 9457 / ProblemDetails** (`application/problem+json`).
   - Schemas de entrada e saída devem ser documentados via **OpenAPI / Swagger**.

---

## 3. Padrões Técnicos e Stack

- **Runtime / Framework:** .NET (ASP.NET Core Web API com **Controllers**), C# com nullable reference types habilitado (`<Nullable>enable</Nullable>`).
- **Persistência:** SQLite com Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite`) e Migrations versionadas em `Data/Migrations/`.
- **Documentação da API:** OpenAPI nativo / Swagger UI habilitado em ambiente de desenvolvimento.
- **Validação:** Data Annotations + `[ApiController]` nos DTOs de entrada, complementados por validações de URL no `UrlShortenerService`.
- **Testes:** xUnit + `WebApplicationFactory<Program>` para testes de integração e testes unitários para `Services/`.

---

## 4. Qualidade, Segurança e Confiabilidade

1. **Validação Estrita de URLs:**
   - Aceitar apenas URLs absolutas válidas nos esquemas `http` e `https`.
   - Rejeitar esquemas inseguros ou inválidos (ex: `javascript:`, `data:`, `file:`) para prevenir *Open Redirect* malicioso / XSS.
2. **Geração de Código Curto (Short Code):**
   - Códigos curtos devem ser únicos, URL-safe (ex: Base62 `[a-zA-Z0-9]`) e não previsíveis sequencialmente quando possível, ou tratados com garantia de unicidade via índice `UNIQUE` no banco.
   - Em caso de colisão na geração, aplicar retry controlado antes de falhar.
3. **Sem Dados Sensíveis em Logs:**
   - Logs estruturados (`ILogger`) não devem expor informações sensíveis ou tokens.
4. **Concorrência na Contagem de Cliques:**
   - O incremento do contador de cliques deve ser seguro contra condições de corrida (atualização atômica no banco ou registro de evento).

---

## 5. Convenções de Código e Git

- **Idioma do Código:** Identificadores, classes, métodos, rotas e mensagens técnicas de código em **Inglês**; documentação SDD em **Português (BR)**.
- **Formatação:** Seguir as convenções padrão do C# (`.editorconfig` / `dotnet format`).
- **Critério de Conclusão (Definition of Done):**
  - Código compila sem warnings relevantes.
  - Testes unitários e de integração passam (`dotnet test`).
  - Documentação OpenAPI reflete os endpoints implementados.
  - Tarefa correspondente marcada como concluída em `spec/task.md`.

---

## Concisão de Resposta e Código (Estrito)

- **Zero Enrolação:** Responda apenas com o estritamente necessário. Elimine saudações, introduções, resumos do que foi feito e explicações não solicitadas.
- **Respostas Diretas:** Comece imediatamente pela solução, código ou dado solicitado.
- **Arquivos e Código Enxutos:** Escreva o código mais limpo e minimalista possível para atender aos requisitos. Não crie abstrações especulativas, boilerplate desnecessário ou over-engineering sem solicitação explícita.
- **Sem Comentários Óbvios:** Não comente *o que* o código faz. Comente apenas o *porquê* em regras de negócio complexas ou workarounds.
- **Edições Cirúrgicas:** Ao alterar arquivos existentes, modifique apenas as linhas necessárias, sem reformatar código não relacionado.
