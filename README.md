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
│   ├── steering/     → convenções do projeto (stack, estilo, estrutura)
│   ├── specs/        → specs de features (ex.: algoritmo de repetição espaçada)
│   └── hooks/        → automações (ex.: rodar testes ao salvar)
├── backend/          → API REST em C# (.NET) + SQLite
│   ├── Models/       → Card, Deck, ReviewSchedule
│   ├── Services/     → SpacedRepetitionService (núcleo lógico)
│   ├── Controllers/  → endpoints REST
│   └── Data/         → contexto de dados (EF Core + SQLite)
└── frontend/         → SPA em Angular + TypeScript
    ├── decks/        → listagem e gestão de decks
    ├── cards/        → editor/geração de cards
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

## 🚀 Como rodar (será detalhado conforme o projeto evolui)

### Backend
```bash
cd backend
dotnet restore
dotnet run
```

### Frontend
```bash
cd frontend
npm install
ng serve
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
