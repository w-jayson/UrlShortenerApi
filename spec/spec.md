# Especificação Funcional e Técnica — UrlShortenerApi

## 1. Visão Geral

A **UrlShortenerApi** permite que clientes encurtem URLs longas em códigos curtos compartilháveis, redirecionem usuários através desses códigos e consultem métricas básicas de acesso (analytics).

---

## 2. Requisitos Funcionais (RF)

### RF01 — Encurtar URL
- **Descrição:** O sistema deve receber uma URL longa válida e retornar uma URL encurtada com um código único.
- **Endpoint:** `POST /api/urls`
- **Entrada (JSON):**
  ```json
  {
    "originalUrl": "https://exemplo.com/artigo/muito-longo?param=1"
  }
  ```
- **Regras de Negócio:**
  - `originalUrl` é obrigatória, não pode ser vazia e deve ter no máximo `2048` caracteres.
  - Deve ser uma URL absoluta válida com esquema `http` ou `https`.
  - O sistema gera um `shortCode` único (ex: 7 caracteres alfanuméricos Base62 `[a-zA-Z0-9]`).
  - O código gerado é persistido junto com a `originalUrl`, data de criação (`createdAtUtc`) e contador de cliques inicializado em `0`.
- **Resposta de Sucesso (`201 Created`):**
  ```json
  {
    "id": "a1b2c3d4-...",
    "originalUrl": "https://exemplo.com/artigo/muito-longo?param=1",
    "shortCode": "aB3xY9z",
    "shortUrl": "https://localhost:5001/aB3xY9z",
    "createdAtUtc": "2026-11-25T14:00:00Z"
  }
  ```
  - Deve incluir o header `Location` apontando para o recurso de detalhes ou URL encurtada.
- **Respostas de Erro:**
  - `400 Bad Request` (`ProblemDetails`) quando `originalUrl` estiver ausente, malformada, exceder `2048` caracteres ou usar esquema diferente de `http`/`https`.

---

### RF02 — Redirecionar para a URL Original
- **Descrição:** Ao acessar o código curto, o sistema deve redirecionar o cliente para a URL original e contabilizar o clique.
- **Endpoint:** `GET /{shortCode}`
- **Regras de Negócio:**
  - Busca o registro correspondente ao `shortCode` (comparação *case-sensitive* para preservar o espaço de caracteres Base62).
  - Se encontrado:
    - Incrementa o contador de cliques (`clickCount`) e atualiza `lastAccessedAtUtc`.
    - Retorna redirecionamento HTTP (`302 Found` por padrão, evitando cache agressivo de `301` no navegador para que cliques subsequentes continuem sendo contabilizados).
    - Define o header `Location` com a `originalUrl`.
  - Se não encontrado:
    - Retorna `404 Not Found` (`ProblemDetails`).

---

### RF03 — Consultar Analytics Básico da URL
- **Descrição:** O sistema deve permitir consultar os dados e estatísticas básicas de acesso de uma URL encurtada.
- **Endpoint:** `GET /api/urls/{shortCode}/stats`
- **Regras de Negócio:**
  - Retorna os metadados da URL encurtada e métricas acumuladas de cliques.
  - Não incrementa o contador de cliques ao consultar as estatísticas.
- **Resposta de Sucesso (`200 OK`):**
  ```json
  {
    "id": "a1b2c3d4-...",
    "originalUrl": "https://exemplo.com/artigo/muito-longo?param=1",
    "shortCode": "aB3xY9z",
    "shortUrl": "https://localhost:5001/aB3xY9z",
    "clickCount": 42,
    "createdAtUtc": "2026-11-25T14:00:00Z",
    "lastAccessedAtUtc": "2026-11-25T15:30:00Z"
  }
  ```
- **Respostas de Erro:**
  - `404 Not Found` (`ProblemDetails`) se o `shortCode` não existir.

---

### RF04 — Health Check
- **Descrição:** Endpoint para verificação de saúde da API e conectividade com o banco SQLite.
- **Endpoint:** `GET /health`
- **Resposta de Sucesso (`200 OK`):** Status `Healthy`.

---

## 3. Requisitos Não-Funcionais (RNF)

- **RNF01 — Persistência:** Os dados devem ser persistidos em arquivo SQLite local (`urlshortener.db` configurável via `ConnectionStrings:DefaultConnection`).
- **RNF02 — Unicidade e Concorrência:** O campo `ShortCode` deve possuir índice `UNIQUE` no banco de dados. O incremento de cliques deve ser atômico para evitar perda de contagem sob requisições simultâneas.
- **RNF03 — Padronização de Erros:** Todas as respostas `4xx`/`5xx` devem usar `ProblemDetails`.
- **RNF04 — Observabilidade:** Logs estruturados para criação de URLs, falhas de validação, redirecionamentos não encontrados e erros inesperados.
- **RNF05 — Documentação:** Especificação OpenAPI disponível em `/openapi/v1.json` (ou `/swagger`).

---

## 4. Modelo de Dados Conceitual

### Entidade `ShortenedUrl`
| Campo | Tipo | Restrições | Descrição |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | Identificador único interno |
| `OriginalUrl` | `string` | Required, MaxLength(2048) | URL original informada |
| `ShortCode` | `string` | Required, MaxLength(16), Unique Index | Código curto Base62 |
| `ClickCount` | `long` | Required, Default 0 | Total de redirecionamentos realizados |
| `CreatedAtUtc` | `DateTime` | Required | Data/hora de criação em UTC |
| `LastAccessedAtUtc` | `DateTime?` | Nullable | Data/hora do último redirecionamento em UTC |

---

## 5. Casos de Borda (Edge Cases)

1. **URL com espaços nas pontas:** Realizar `Trim()` antes de validar.
2. **Colisão de `shortCode`:** Caso o código gerado já exista no banco, tentar gerar novo código até `5` vezes; se persistir, retornar `500` ou `503` com log de erro.
3. **Conflito de rotas:** Garantir que códigos curtos não colidam com rotas reservadas (`/api/...`, `/health`, `/swagger`, `/openapi`).
4. **Acesso simultâneo ao mesmo `shortCode`:** Garantir que dois redirecionamentos paralelos resultem em `clickCount = 2`.
