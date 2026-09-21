---
inclusion: always
---

# StudyForge — Stack e Convenções Técnicas

## Stack
- **Backend**: C# / .NET Web API, Entity Framework Core, SQLite.
- **Frontend**: Angular + TypeScript.
- **Dados**: SQLite (arquivo local — sem infraestrutura externa).

## Estrutura de pastas
```
backend/
  Models/        → entidades (Card, Deck, ReviewSchedule)
  Services/      → regra de negócio (SpacedRepetitionService)
  Controllers/   → endpoints REST finos, sem lógica de negócio
  Data/          → DbContext e configuração do EF Core
frontend/
  src/app/
    decks/       → listagem/gestão de decks
    cards/       → editor e geração de cards
    review/      → sessão de revisão
    core/        → serviços de API tipados, modelos compartilhados
```

## Convenções de backend (C#)
- Controllers **finos**: delegam para Services; nenhuma regra de negócio no controller.
- Regra de negócio isolada em Services testáveis (sem dependência de HTTP/DB direta).
- Nomes de classes em PascalCase; interfaces prefixadas com `I` (ex.: `ISpacedRepetitionService`).
- Async/await para I/O; sufixo `Async` em métodos assíncronos.
- Usar DTOs para entrada/saída da API — não expor entidades do EF diretamente.

## Convenções de frontend (Angular + TS)
- Componentes standalone quando possível.
- Serviços tipados para chamadas à API; nada de `any`.
- Modelos/interfaces em `core/`, compartilhados entre features.
- Estado local simples; evitar bibliotecas de estado pesadas no MVP.

## Testes
- Backend: xUnit para os Services (o `SpacedRepetitionService` deve ter testes de unidade).
- Priorizar testes na lógica do algoritmo de repetição espaçada.

## Simplicidade / custo
- Preferir soluções simples e sem infraestrutura externa.
- Não introduzir dependências novas sem necessidade clara.
