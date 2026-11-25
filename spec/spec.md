# Especificação Funcional — UrlShortenerApi

## 1. Visão Geral

A **UrlShortenerApi** permite encurtar URLs longas em códigos curtos compartilháveis, redirecionar usuários através desses códigos e consultar estatísticas básicas de acesso.

---

## 2. Requisitos Funcionais (RF)

### RF01 — Encurtar URL
- **Objetivo:** Receber uma URL longa válida e gerar um código curto único associado a ela.
- **Entrada esperada:** URL original informada pelo cliente.
- **Regras de Negócio:**
  - A URL original é obrigatória, não pode ser vazia e deve ter no máximo 2048 caracteres após remoção de espaços nas extremidades.
  - A URL deve ser absoluta e utilizar exclusivamente os protocolos `http` ou `https`.
  - O sistema gera um código curto único e legível, diferenciando maiúsculas de minúsculas.
  - O registro nasce com data/hora de criação em UTC e contagem de cliques zerada.
- **Critérios de Aceite:**
  - Quando a URL for válida, o sistema persiste o vínculo, retorna os dados da URL encurtada (incluindo o link curto completo e a data de criação) e informa o endereço do recurso criado.
  - Quando a URL estiver ausente, vazia, malformada, exceder 2048 caracteres ou usar outro protocolo (ex: `javascript:`, `ftp:`), o sistema rejeita a solicitação com erro de validação sem persistir dados.

---

### RF02 — Redirecionar para a URL Original
- **Objetivo:** Resolver um código curto, contabilizar o acesso e redirecionar o cliente para a URL original.
- **Regras de Negócio:**
  - A busca pelo código curto é sensível a maiúsculas e minúsculas (*case-sensitive*).
  - Cada redirecionamento bem-sucedido incrementa a contagem total de cliques em 1 e atualiza a data/hora do último acesso (UTC).
  - O redirecionamento não deve ser armazenado em cache permanente pelo cliente, garantindo que acessos futuros continuem passando pelo sistema e sendo contabilizados.
- **Critérios de Aceite:**
  - Se o código existir, o cliente é redirecionado temporariamente para a URL original e o contador de cliques reflete o novo acesso.
  - Se o código não existir, o sistema informa que o recurso não foi encontrado.

---

### RF03 — Consultar Estatísticas da URL
- **Objetivo:** Permitir a consulta dos metadados e métricas acumuladas de uma URL encurtada.
- **Regras de Negócio:**
  - A consulta é somente leitura e **não** deve incrementar o contador de cliques nem alterar a data do último acesso.
- **Critérios de Aceite:**
  - Se o código existir, retorna a URL original, o código curto, o link encurtado, o total de cliques, a data de criação e a data do último acesso (nula se nunca acessada).
  - Se o código não existir, o sistema informa que o recurso não foi encontrado.

---

### RF04 — Verificação de Saúde
- **Objetivo:** Disponibilizar um ponto de verificação indicando se o serviço está operacional e apto a responder requisições.
- **Critérios de Aceite:**
  - Quando operacional, responde positivamente indicando estado saudável.

---

## 3. Requisitos Não-Funcionais (RNF)

- **RNF01 — Persistência:** Os vínculos entre URLs originais, códigos curtos e contagens de cliques devem sobreviver a reinicializações do serviço.
- **RNF02 — Unicidade:** Dois vínculos distintos nunca podem possuir o mesmo código curto.
- **RNF03 — Consistência sob Concorrência:** $N$ redirecionamentos simultâneos para o mesmo código curto devem resultar em exatamente $+N$ no contador de cliques, sem perda de contagem.
- **RNF04 — Padronização de Respostas de Erro:** Falhas de validação, recursos inexistentes e erros internos devem retornar respostas de erro estruturadas e consistentes.

---

## 4. Modelo Conceitual de Domínio

### Entidade: `URL Encurtada`
Representa o vínculo entre um endereço web original e seu identificador curto.

| Atributo | Descrição | Regras Conceituais |
| :--- | :--- | :--- |
| Identificador | Identificador interno do registro | Único |
| URL Original | Endereço web de destino | Obrigatória, absoluta (`http`/`https`), até 2048 caracteres |
| Código Curto | Identificador compartilhável | Obrigatório, único, *case-sensitive* |
| Contagem de Cliques | Quantidade acumulada de redirecionamentos | Obrigatória, inicia em zero, não negativa |
| Data de Criação | Momento do cadastro em UTC | Obrigatória, imutável após criação |
| Data do Último Acesso | Momento do último redirecionamento em UTC | Opcional (nula até o primeiro acesso) |

---

## 5. Casos de Borda (Edge Cases)

1. **Espaços nas extremidades:** Uma URL válida cercada por espaços deve ser normalizada antes da validação e persistida sem os espaços extras.
2. **Colisão de código gerado:** Caso um código gerado já esteja em uso, o sistema deve gerar outro código automaticamente antes de falhar a criação; se não conseguir após novas tentativas, deve retornar erro sem corromper dados existentes.
3. **Conflito com rotas do sistema:** Códigos curtos gerados não podem impedir o funcionamento das rotas de operação, estatísticas, saúde ou documentação da própria API.
4. **Acessos simultâneos:** Múltiplas requisições paralelas de redirecionamento ao mesmo código curto devem contabilizar todos os cliques corretamente.
