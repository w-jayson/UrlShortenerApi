# UrlShortenerApi

Projeto de estudo para criar um encurtador de URLs com contagem de cliques usando **ASP.NET Core** e **SQLite**.

## O que ele faz

- **Encurtar URL** (`POST /api/urls`): recebe um link longo e devolve um código curto.
- **Redirecionar** (`GET /{shortCode}`): redireciona para o link original e soma 1 clique.
- **Estatísticas** (`GET /api/urls/{shortCode}/stats`): mostra quantos cliques o link recebeu e quando foi o último acesso.

## Estrutura

- `spec/` — Documentação do projeto (`constitution.md`, `spec.md`, `plan.md` e `task.md`).

## Como rodar (após a implementação)

```bash
dotnet run --project src/UrlShortenerApi
```
