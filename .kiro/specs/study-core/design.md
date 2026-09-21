# Design — Study Core (Decks, Card Generation, Review Session)

## Overview

Backend em C#/.NET Web API com EF Core + SQLite, expondo endpoints REST consumidos pelo
frontend Angular. A regra de agendamento é delegada ao `ISpacedRepetitionService`
(spec `spaced-repetition`). Controllers finos; regra de negócio nos services.

## Architecture

```
Angular (frontend)
      │  HTTP/JSON (DTOs)
      ▼
Controllers ──> DeckService / CardService / ReviewService
                        │                        │
                        │                        └── ISpacedRepetitionService (SM-2)
                        ▼
                 AppDbContext (EF Core) ──> SQLite (studyforge.db)
```

## Data Models (EF Core)

### `Deck`
```csharp
public class Deck
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Card> Cards { get; set; } = new();
}
```

### `Card`
```csharp
public class Card
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";

    // Estado de agendamento (SM-2)
    public double EaseFactor { get; set; } = 2.5;
    public int Interval { get; set; } = 0;
    public int Repetitions { get; set; } = 0;
    public DateTime NextReview { get; set; }  // UTC
}
```

Relação: `Deck 1..* Card`, exclusão em cascata.

## API Endpoints (DTOs)

| Método | Rota                                  | Descrição                                  |
|--------|---------------------------------------|--------------------------------------------|
| GET    | `/api/decks`                          | Lista decks (id, name, cardCount)          |
| POST   | `/api/decks`                          | Cria deck `{ name }`                        |
| DELETE | `/api/decks/{id}`                     | Exclui deck (cascade)                       |
| POST   | `/api/decks/{id}/cards/generate`      | Gera cards `{ text }` → `{ created }`       |
| GET    | `/api/decks/{id}/review`              | Cards devidos agora (sessão)                |
| POST   | `/api/cards/{id}/review`              | Avalia card `{ grade }` → novo agendamento  |
| GET    | `/api/decks/{id}/progress`            | `{ pending, learning, mastered, due }`      |

### DTOs principais
```csharp
public record CreateDeckDto(string Name);
public record DeckSummaryDto(int Id, string Name, int CardCount);
public record GenerateCardsDto(string Text);
public record GenerateResultDto(int Created);
public record CardDto(int Id, string Question, string Answer);
public record GradeDto(int Grade);
public record ProgressDto(int Pending, int Learning, int Mastered, int Due);
```

## Card Generation (regra simples)

`CardService.GenerateFromText(deckId, text, now)`:
1. Dividir `text` por quebras de linha.
2. Para cada linha: trim; ignorar vazias e as sem `::`.
3. Separar em `question :: answer` no primeiro `::`; trim de ambos os lados.
4. Criar `Card` com estado inicial (via `ISpacedRepetitionService.CreateInitialState(now)`).
5. Persistir todos e retornar a contagem criada.

## Review Flow

`ReviewService.GradeCard(cardId, grade, now)`:
1. Carregar card (404 se não existir).
2. Montar `SchedulingState` a partir dos campos do card.
3. `newState = spacedRepetition.ApplyReview(state, grade, now)`.
4. Gravar novos campos no card e salvar.
5. Retornar o novo agendamento.

## Progress Buckets

- **pending**: repetitions == 0
- **learning**: repetitions em 1..2
- **mastered**: repetitions >= 3
- **due**: nextReview <= now

## Error Handling

- Validação (nome vazio, grade inválida, texto vazio) → 400.
- Deck/card inexistente → 404.
- Datas sempre em UTC.

## Testing Strategy

- Unit: `CardService.GenerateFromText` (linhas válidas/ inválidas/ vazias, trim, contagem).
- Unit: `ReviewService.GradeCard` delega ao SM-2 e persiste (mock do DbContext ou SQLite in-memory).
- Unit: cálculo dos buckets de progresso.
- (SM-2 em si já é coberto pela spec `spaced-repetition`.)

## Frontend (Angular) — visão geral

- `DecksComponent`: listar/criar/excluir decks + link para revisão.
- `CardGenerateComponent`: textarea `pergunta :: resposta` → chama generate.
- `ReviewComponent`: mostra pergunta → revela resposta → botões Errei/Difícil/Bom/Fácil
  (mapeiam para grade 0/3/4/5) → chama `/review` → próximo card.
- `ApiService` tipado (sem `any`), modelos em `core/`.
