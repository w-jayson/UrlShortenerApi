# UrlShortenerApi

Uma API simples para encurtamento de URLs, desenvolvida em **.NET** para fins de estudo de arquitetura de back-end, **Dependency Injection** e persistência em banco relacional baseado em arquivo (**SQLite**).

---

## 🚀 Sobre o Projeto

O objetivo deste projeto é fornecer um serviço que recebe uma URL longa e gera um link curto e único. Ao acessar o link curto, o usuário é redirecionado automaticamente para o destino original.

Atualmente, o projeto utiliza a Base62 para gerar Url a partir do Id e armazena os dados utilizando o **Entity Framework Core** com o provedor **SQLite**.

---

## 🛠 Tecnologias Utilizadas

* **.NET 10 (SDK)**
* **ASP.NET Core Web API**
* **Entity Framework Core SQLite**: Persistência baseada em arquivo (`urlshortener.db`).
* **EF Core Migrations**: Gerenciamento de schema e versionamento de banco.
* **NSwag**: Para documentação automática e interface de testes (**Swagger UI**).

---

## 📋 Funcionalidades

* **Encurtamento de URL**: Recebe uma URL absoluta e retorna um código único.
* **Redirecionamento**: Realiza o redirecionamento **HTTP 302** a partir do código encurtado.
* **Validação**: Verifica se a entrada é uma URL válida antes de processar.

---

## ⚙️ Como Executar

1.  Certifique-se de ter o **.NET SDK** instalado em sua máquina.
2.  Clone o repositório:
    ```bash
    git clone https://github.com/w-jayson/UrlShortenerApi.git
    ```
3.  Navegue até a pasta do projeto:
    ```bash
    cd UrlShortenerApi
    ```
4.  Execute a aplicação (as migrações do banco serão aplicadas automaticamente):
    ```bash
    dotnet run
    ```
5.  Acesse a interface do **Swagger** para testar os **Endpoints**:
    ```text
    http://localhost:5070/swagger
    ```

---

## 🛣 Endpoints

### 1. Criar URL Encurtada
* **Rota**: `POST /shorten`
* **Corpo da Requisição (Body)**: 
    ```json
    "https://www.google.com"
    ```
* **Resposta**:
    ```json
    {
      "shortUrl": "http://localhost:5070/1"
    }
    ```

### 2. Redirecionar
* **Rota**: `GET /{code}`
* **Descrição**: Busca a URL original no banco de dados e executa o redirecionamento.

---

## 📌 Próximos Passos (Roadmap)
* [X] Substituir a geração de string aleatório por Base62.
* [X] Substituir o armazenamento **In-Memory** por um banco de dados relacional baseado em arquivo (**SQLite**).
* [X] Implementar **Migrations** para gerenciamento do **schema** do banco.
* [ ] Adicionar suporte a **Custom Aliases** (links personalizados).
* [ ] Implementar métricas de cliques por link.
* [ ] Adicionar cache com **Redis** para otimizar os redirecionamentos.

---

**Desenvolvido por [Weslley](https://github.com/w-jayson)**
