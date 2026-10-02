# StudyForge 🎓

Transforme suas notas de estudo em **flashcards** e revise-os com **repetição espaçada**.

StudyForge é um app de estudo que recebe material de texto (notas, resumos, trechos de PDF),
gera flashcards e agenda revisões de forma inteligente usando um algoritmo de repetição
espaçada (SM-2 / Leitner). O objetivo é estudar menos tempo, lembrando por mais tempo.

> Projeto construído durante o **Kiro University Challenge 2026**.

---

## ✨ Funcionalidades (MVP)

- **Decks**: criar e organizar baralhos de flashcards por assunto.
- **Geração de cards**: colar texto no formato `pergunta :: resposta` (uma por linha) e gerar cards.
- **Sessão de revisão**: revisar os cards devidos hoje com botões estilo Anki (Errei / Difícil / Bom / Fácil).
- **Repetição espaçada (SM-2)**: o app agenda a próxima revisão de cada card com base no seu desempenho.
- **Progresso**: acompanhar quantos cards estão pendentes, aprendidos e dominados.

## 🧩 Funcionalidades futuras / extras

- Geração de cards assistida por IA (perguntas melhores a partir do texto).
- Estatísticas e gráficos de progresso ao longo do tempo.

---

## 🏗️ Arquitetura

```
studyforge/
├── .kiro/
│   ├── steering/            → convenções do projeto (stack, estilo, estrutura)
│   ├── specs/               → specs de features (spaced-repetition, study-core)
│   └── hooks/               → automações (ex.: rodar testes ao salvar)
├── StudyForge.sln           → solução .NET (API + domínio + testes)
├── src/
│   ├── StudyForge.Domain/   → algoritmo SM-2 puro (ISpacedRepetitionService)
│   └── StudyForge.Api/      → API REST em C# (.NET 8) + EF Core + SQLite
│       ├── Controllers/     → endpoints REST finos
│       ├── Services/        → regra de negócio (Deck/Card/Review)
│       ├── Entities/        → Deck, Card (EF Core)
│       ├── Dtos/            → contratos de entrada/saída da API
│       └── Data/            → AppDbContext + migrations
├── tests/
│   ├── StudyForge.Domain.Tests/ → testes do SM-2 (unidade + property-based)
│   └── StudyForge.Api.Tests/    → testes dos services (SQLite in-memory)
└── frontend/                → SPA em Angular + TypeScript
    └── src/app/
        ├── core/            → ApiService tipado + modelos compartilhados
        └── features/
            ├── decks/        → listagem e gestão de decks
            ├── card-generate/→ editor/geração de cards
            └── review/       → sessão de revisão
```

### Stack

| Camada    | Tecnologia                          |
|-----------|-------------------------------------|
| Backend   | C# / .NET Web API, EF Core, SQLite  |
| Frontend  | Angular, TypeScript                 |
| Dados     | SQLite (arquivo local, zero setup)  |

**Por que SQLite?** Sem infraestrutura externa para configurar — mais simples de rodar
e de manter durante o challenge.

---

## 🚀 Como rodar

### Pré-requisitos

- **.NET SDK 8.0** (ver `global.json` — a versão fixada é a `8.0.131`).
- **Node.js 18+** e **npm** (para o frontend Angular 15).
- Nenhum banco externo: o SQLite é um arquivo local criado automaticamente.

### 1. Backend (API REST)

Na raiz do repositório:

```bash
# restaurar e compilar a solução inteira
dotnet build

# rodar a API (perfil http → http://localhost:5200)
dotnet run --project src/StudyForge.Api --launch-profile http
```

- A API sobe em **http://localhost:5200** com o Swagger em **http://localhost:5200/swagger**.
- Na primeira execução, as migrations do EF Core são aplicadas automaticamente e o
  arquivo **`studyforge.db`** (SQLite) é criado em `src/StudyForge.Api/`.
- O CORS de desenvolvimento já libera a origem do Angular (`http://localhost:4200`).

### 2. Frontend (Angular)

Em outro terminal:

```bash
cd frontend
npm install
npm start          # equivale a "ng serve" → http://localhost:4200
```

- O app abre em **http://localhost:4200** e consome a API em `http://localhost:5200/api`
  (configurado em `frontend/src/environments/environment.ts`).

### 3. Fluxo ponta a ponta (MVP)

Com a API e o frontend rodando:

1. **Criar um deck** (ex.: "Biologia").
2. **Gerar cards**: cole um texto com uma linha por card no formato
   `pergunta :: resposta` (linhas sem `::` ou vazias são ignoradas; espaços sofrem trim).
3. **Revisar**: inicie a sessão, revele a resposta e avalie com os botões estilo Anki
   (**Errei / Difícil / Bom / Fácil** → grades 0 / 3 / 4 / 5).
4. O card é **reagendado** pelo SM-2 e sai da lista de devidos até a próxima data.
5. Acompanhe o **progresso** do deck (pendentes / aprendendo / dominados / devidos).

### Testes

```bash
# testes do backend (domínio SM-2 + services da API)
dotnet test

# testes do frontend
cd frontend
npm test
```

---

## 📚 Kiro University Challenge

Este projeto é o "exame final" do challenge e busca demonstrar as capacidades do Kiro:

| Capacidade Kiro   | Onde é demonstrada no StudyForge                          |
|-------------------|-----------------------------------------------------------|
| Steering              | Convenções em `.kiro/steering/`                                   |
| Specs                 | Spec do algoritmo de repetição espaçada em `.kiro/specs/`         |
| Hooks                 | `.kiro/hooks/` roda os testes do backend ao salvar arquivos `.cs` |
| Property-based testing| Invariantes do SM-2 verificadas no Kiro IDE (ver spec SM-2)       |
| MCP servers           | Filesystem MCP lê notas de `study-notes/` (ver `docs/mcp-and-powers.md`) |
| Powers                | Power de testes de contrato de API para validar os endpoints REST |
| Custom agents         | Agente restrito `card-generator` (ver `docs/custom-agents-and-cloud.md`) |
| Kiro Web / cloud      | Projeto planejado em cloud session; config no repo (Configuration Sync) |
| Superfícies           | Planejado no Kiro Web (cloud) + implementado no Kiro IDE          |
| **Bônus: criar Power**| Power `spaced-repetition-power` empacotado em `power/` (Agent Plugins v1) |

> As lições oficiais são reveladas dia a dia durante o challenge; este mapeamento
> será ajustado conforme o conteúdo real de cada lição.
>
> O diretório `power/` empacota um **Kiro Power** próprio (lição bônus) e pode ser publicado
> como repositório GitHub público independente. Ver `power/README.md`.

## 📄 Licença

MIT
