# Design — Spaced Repetition Scheduling (SM-2)

## Overview

Este componente encapsula o algoritmo SM-2 em um serviço puro e determinístico. Ele não conhece
EF Core, HTTP nem UI — recebe o estado de agendamento de um card e uma avaliação (grade), e
devolve o novo estado. Isso mantém a regra de negócio testável isoladamente e reutilizável.

## Architecture

```
Controllers ──> ReviewService ──> ISpacedRepetitionService (esta spec)
                                        │
                                        └── SM2SchedulingService (implementação pura)
```

- `ISpacedRepetitionService` é a fronteira testável.
- A implementação não tem dependências (sem DB/HTTP/clock global); "now" chega por parâmetro.

## Components and Interfaces

### `ReviewGrade` (enum → int)
Mapeia os botões estilo Anki para uma grade 0..5:
- `Again = 0`  (Errei)
- `Hard = 3`   (Difícil)
- `Good = 4`   (Bom)
- `Easy = 5`   (Fácil)

### `SchedulingState` (record imutável)
```csharp
public record SchedulingState(double EaseFactor, int Interval, int Repetitions, DateTime NextReview);
```

### `ISpacedRepetitionService`
```csharp
public interface ISpacedRepetitionService
{
    // Estado de um card recém-criado (devido imediatamente).
    SchedulingState CreateInitialState(DateTime now);

    // Aplica uma avaliação e retorna o novo estado de agendamento.
    SchedulingState ApplyReview(SchedulingState current, int grade, DateTime now);
}
```

## Algorithm (SM-2) — passo a passo do `ApplyReview`

Entrada: `current` (EaseFactor, Interval, Repetitions), `grade` (0..5), `now`.

1. **Validar** grade em 0..5 e valores não negativos; senão lançar `ArgumentOutOfRangeException`.
2. **Atualizar easeFactor**:
   `ef' = current.EaseFactor + (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02))`
   `ef' = max(ef', 1.3)`
3. **Ramo por acerto/erro**:
   - Se `grade < 3` (erro): `repetitions = 0`, `interval = 1`.
   - Se `grade >= 3` (acerto): `repetitions = current.Repetitions + 1`; então:
     - `repetitions == 1` → `interval = 1`
     - `repetitions == 2` → `interval = 6`
     - `repetitions > 2`  → `interval = round(current.Interval * ef')`
4. **nextReview** = `now.AddDays(interval)`.
5. Retornar `new SchedulingState(ef', interval, repetitions, nextReview)`.

`CreateInitialState(now)` → `new SchedulingState(2.5, 0, 0, now)`.

## Data Models

O estado de agendamento é persistido junto ao Card (ver spec `study-core`), mas o cálculo
vive aqui. Campos: `EaseFactor (double)`, `Interval (int, dias)`, `Repetitions (int)`,
`NextReview (DateTime, UTC)`.

## Error Handling

- Grade fora de 0..5 → `ArgumentOutOfRangeException`.
- Interval/Repetitions negativos no estado de entrada → `ArgumentException`.
- Datas em UTC para evitar ambiguidade de fuso.

## Testing Strategy

Testes de unidade em xUnit para `SM2SchedulingService`:
- Card novo: `CreateInitialState` retorna (2.5, 0, 0, now).
- Primeiro acerto (grade 4, repetitions 0→1): interval = 1.
- Segundo acerto (repetitions 1→2): interval = 6.
- Terceiro acerto (repetitions 2→3): interval = round(6 * ef).
- Erro (grade 0 ou <3): repetitions = 0, interval = 1.
- easeFactor nunca cai abaixo de 1.3 (vários erros seguidos).
- nextReview = now + interval dias (checar com "now" fixo).
- Grade inválida (−1, 6) lança exceção.
- Determinismo: mesma entrada → mesma saída.

## Property-based testing (IDE)

Além dos testes por exemplo acima, o `SM2SchedulingService` é um alvo ideal para
**property-based testing** (feature exclusiva do Kiro IDE): em vez de listar casos, declaramos
regras gerais (invariantes) que devem valer para *quaisquer* entradas válidas, e a ferramenta
gera muitas entradas aleatórias tentando violá-las.

### Gerador de entradas válidas
- `easeFactor` ∈ [1.3, 3.0], `interval` ∈ [0, 3650], `repetitions` ∈ [0, 500],
  `grade` ∈ {0,1,2,3,4,5}, `now` qualquer `DateTime` em UTC.

### Invariantes (propriedades)
1. **easeFactor nunca abaixo do piso**: para qualquer entrada válida, o `easeFactor` de saída >= 1.3.
2. **Erro reinicia**: para qualquer estado e qualquer grade < 3, a saída tem `repetitions == 0` e `interval == 1`.
3. **Acerto não decresce repetitions**: para qualquer grade >= 3, `repetitions_out == repetitions_in + 1`.
4. **Interval não-negativo**: para qualquer entrada válida, `interval` de saída >= 1 após uma revisão.
5. **nextReview coerente**: `nextReview == now.AddDays(interval)` (mesma diferença em dias, em UTC).
6. **Monotonicidade de facilidade**: para o mesmo estado, uma grade maior nunca produz um
   `easeFactor` de saída menor do que uma grade menor (a fórmula SM-2 é monotônica na grade).
7. **Determinismo**: aplicar a mesma (estado, grade, now) duas vezes produz saídas idênticas.
8. **Grade fora de 0..5 sempre lança** (propriedade de validação).

> Estas propriedades devem ser implementadas no IDE usando o suporte a property-based testing do
> Kiro. Elas complementam — não substituem — os testes por exemplo.
