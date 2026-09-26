# SM-2 — Details and Worked Examples

This reference expands the SM-2 algorithm summarized in the skill.

## State

| Field        | Meaning                                  | Initial |
|--------------|------------------------------------------|---------|
| easeFactor   | How easy the card is for this user       | 2.5     |
| interval     | Days until the next review               | 0       |
| repetitions  | Consecutive successful reviews (grade>=3)| 0       |
| nextReview   | When the card is due (UTC)               | now     |

## Ease factor update

```
EF' = EF + (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02))
EF' = max(EF', 1.3)
```

The floor of 1.3 prevents difficult cards from shrinking their intervals indefinitely.

## Interval progression (grade >= 3)

| repetitions (after increment) | interval                         |
|-------------------------------|----------------------------------|
| 1                             | 1 day                            |
| 2                             | 6 days                           |
| > 2                           | round(previousInterval * EF')    |

On any `grade < 3`, reset: `repetitions = 0`, `interval = 1`.

## Worked example

Start: easeFactor = 2.5, interval = 0, repetitions = 0.

1. Review with grade 4 (Good): repetitions -> 1, interval -> 1 day.
2. Review with grade 4: repetitions -> 2, interval -> 6 days.
3. Review with grade 5 (Easy): repetitions -> 3, EF rises,
   interval -> round(6 * EF') (about 16 days).
4. Review with grade 1 (Again, failed): repetitions -> 0, interval -> 1 day; EF drops
   but not below 1.3.

## Why property-based testing fits

The scheduler is a pure function with clear universal rules (invariants), so random-input
testing is very effective at finding edge cases that hand-written examples miss. See the
invariants listed in the skill.

## Notes-to-flashcards format

One card per line:

```
What organelle produces most of the cell's ATP? :: The mitochondrion
Where is DNA stored in a eukaryotic cell? :: In the nucleus
```

Ignore blank lines and lines without `::`; trim both sides of the separator.
