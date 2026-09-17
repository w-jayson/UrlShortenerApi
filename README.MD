# UrlShortenerApi

Uma API robusta e performática para encurtamento de URLs desenvolvida em **.NET 10** e **C#**, utilizando **Entity Framework Core**, banco relacional baseado em arquivo (**SQLite**) com modo **WAL** ativo, suporte a **links personalizados (custom slugs)** e geração aleatória criptograficamente segura resiliente a colisões.

---

## 🚀 Sobre o Projeto

O objetivo deste projeto é fornecer um serviço confiável de encurtamento de URLs com:
- **Persistência Explícita de Código Curto**: Código curto (`Code`) persistido diretamente na entidade com restrição e índice de unicidade (`UNIQUE INDEX`) a nível de banco de dados.
- **Custom Slugs**: Possibilidade de o cliente definir seu próprio alias amigável para a URL encurtada.
- **Geração Aleatória Segura e Resiliente**: Criação de códigos de 7 caracteres Base62 via `System.Security.Cryptography.RandomNumberGenerator` e inserção otimista com tratamento de retry em caso de colisões (`DbUpdateException`).
- **SQLite com WAL Mode**: Modo *Write-Ahead Logging* ativado para permitir concorrência eficiente entre operações de leitura (`AsNoTracking`) e gravação sem travamento de arquivo.

---

## 🛠 Tecnologias Utilizadas

* **.NET 10 (SDK)** & **C#**
* **ASP.NET Core Web API** (Controllers & Minimal/CQRS Handlers)
* **Entity Framework Core SQLite**: Persistência baseada em arquivo (`urlshortener.db`) com migrações automáticas no startup.
* **SQLite WAL (Write-Ahead Logging)**: Habilitado para concorrência de leituras simultâneas.
* **System.Security.Cryptography**: `RandomNumberGenerator.GetString` para geração de códigos alfanuméricos Base62 não previsíveis.
* **NSwag**: Documentação OpenAPI e Swagger UI.

---

## 📋 Regras de Negócio e Arquitetura

### 1. Slugs Personalizados (`CustomSlug`)
- O cliente pode fornecer um slug personalizado opcional na requisição.
- **Validação de Formato**: Tamanho entre 3 e 50 caracteres; são permitidos apenas caracteres alfanuméricos, hífen (`-`) e sublinhado/underscore (`_`).
- **Slugs Reservados**: Nomes protegidos pelo sistema são rejeitados de forma *case-insensitive* (ex: `swagger`, `api`, `health`, `metrics`, `favicon.ico`, `robots.txt`).
- **Garantia de Unicidade**: Se o slug já estiver cadastrado no banco, a API retorna **409 Conflict** imediatamente, sem sobrescrever ou gerar um código alternativo.

### 2. Geração Aleatória Resiliente a Colisões
- Quando nenhum `CustomSlug` é fornecido, a API utiliza o serviço `IShortCodeGenerator` para gerar um código Base62 de 7 caracteres.
- **Tratamento Otimista de Colisão**: A entidade é inserida e o EF Core tenta salvar com `SaveChangesAsync`. Caso ocorra colisão de chave única (`DbUpdateException`), a entidade é desanexada do rastreador (`_context.Entry(entity).State = EntityState.Detached`) e uma nova tentativa é realizada (até o limite de 3 tentativas).

### 3. Redirecionamento de Alta Performance
- A resolução de URLs realiza consulta direta com `AsNoTracking()` e projeção (`Select(x => x.OriginalUrl)`), minimizando alocação de memória e overhead de tracking do EF Core.
- Responde com **HTTP 302 Found** apontando para a URL de destino.

---

## ⚙️ Como Executar

1. Certifique-se de ter o **.NET 10 SDK** instalado em sua máquina.
2. Clone o repositório:
   ```bash
   git clone https://github.com/w-jayson/UrlShortenerApi.git
   ```
3. Navegue até a pasta do projeto:
   ```bash
   cd UrlShortenerApi
   ```
4. Execute a aplicação (as migrações e o modo WAL serão aplicados automaticamente na inicialização):
   ```bash
   dotnet run
   ```
5. Acesse a interface interativa do **Swagger UI**:
   ```text
   http://localhost:5070/swagger
   ```

---

## 🛣 Endpoints

### 1. Criar URL Encurtada
* **Rota**: `POST /shorten`
* **Headers**: `Content-Type: application/json`

#### Exemplo 1: Geração Aleatória (sem slug customizado)
**Request Body**:
```json
{
  "url": "https://www.google.com"
}
```

**Response (`201 Created`)**:
```json
{
  "shortUrl": "http://localhost:5070/aB3x9Zq",
  "code": "aB3x9Zq"
}
```

#### Exemplo 2: Com Custom Slug
**Request Body**:
```json
{
  "url": "https://github.com",
  "customSlug": "meu-link"
}
```

**Response (`201 Created`)**:
```json
{
  "shortUrl": "http://localhost:5070/meu-link",
  "code": "meu-link"
}
```

#### Respostas de Erro:
* **`400 Bad Request`**: URL malformada, slug com tamanho inválido (< 3 ou > 50 caracteres), caracteres proibidos ou slug reservado pelo sistema.
  ```json
  {
    "message": "O slug 'swagger' é reservado pelo sistema."
  }
  ```
* **`409 Conflict`**: Slug customizado já cadastrado por outro link.
  ```json
  {
    "message": "O slug customizado informado já está em uso."
  }
  ```

---

### 2. Redirecionar para URL Original
* **Rota**: `GET /{code}`
* **Exemplo**: `GET /meu-link`
* **Comportamento**:
  - Se encontrado: Retorna **HTTP 302 Found** com o cabeçalho `Location: https://github.com`.
  - Se não encontrado: Retorna **HTTP 404 Not Found**.

---

## 📌 Roadmap
* [X] Substituir a conversão Base62 a partir de Id sequencial por persistência explícita de `Code`.
* [X] Suporte a **Custom Slugs** com validações de tamanho, regex e lista negra de palavras reservadas.
* [X] Geração aleatória criptograficamente segura (`RandomNumberGenerator`) resiliente a colisões.
* [X] Persistência em **SQLite** com **EF Core Migrations** e **WAL Mode** ativo.
* [ ] Implementar métricas de cliques e data do último acesso por link.
* [ ] Adicionar camada de cache distribuído com **Redis** para otimizar os redirecionamentos em alta escala.

---

**Desenvolvido por [Weslley](https://github.com/w-jayson)**
