---
name: spaced-repetition
description: Design and implement spaced-repetition study apps. Provides the SM-2 scheduling algorithm as a pure, testable function, a simple notes-to-flashcards workflow, and testing guidance including property-based invariants. Use when building flashcards, review scheduling, SM-2 or Leitner systems, or study/learning apps.
license: MIT
metadata:
  version: "1.0.0"
---

# Spaced Repetition

Guidance for building study apps that schedule flashcard reviews with spaced repetition.

## When to use

Activate this skill when the task involves flashcards, review scheduling, spaced repetition,
the SM-2 or Leitner algorithms, or building study/learning apps.

## Core algorithm: SM-2

Model each card's scheduling state as: `easeFactor` (starts at 2.5), `interval` (days),
`repetitions` (consecutive correct answers). A review supplies a `grade` from 0 to 5.

Apply a review as a **pure function** `(state, grade, now) -> newState`:

1. Validate `grade` is in 0..5; reject otherwise.
2. Update ease factor:
   `EF' = EF + (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02))`, then clamp to a floor of 1.3.
3. Branch on the grade:
   - `grade < 3` (failed): `repetitions = 0`, `interval = 1`.
   - `grade >= 3` (passed): increment `repetitions`; then
     `repetitions == 1 -> interval = 1`, `repetitions == 2 -> interval = 6`,
     otherwise `interval = round(previousInterval * EF')`.
4. `nextReview = now + interval days`.

Keep the function free of database, network, and global-clock access; pass `now` in as a
parameter so it stays deterministic and testable.

### Grade mapping (Anki-style buttons)

- Again = 0 (resets the card)
- Hard = 3
- Good = 4
- Easy = 5

## Notes-to-flashcards workflow

Generate cards from plain text with a simple, no-AI rule: treat each line in the form
`question :: answer` as one card. Trim both sides; ignore blank lines and lines without `::`.
New cards start with the initial scheduling state and are due immediately.

For richer sources, expose a notes folder to the agent through a filesystem MCP server
(see this power's `mcp.json`), then ask the agent to turn the notes into `question :: answer`
lines.

## Testing guidance

Cover the algorithm with example-based unit tests, and — where the client supports it
(for example the Kiro IDE) — with **property-based tests** for these invariants:

- Ease factor output is never below 1.3.
- Any `grade < 3` resets to `repetitions == 0` and `interval == 1`.
- Any `grade >= 3` yields `repetitions_out == repetitions_in + 1`.
- Interval after a review is always >= 1.
- `nextReview == now + interval days`.
- A higher grade never produces a lower output ease factor than a lower grade.
- The function is deterministic; a grade outside 0..5 always throws.

## References

- [SM-2 details and worked examples](references/sm2-algorithm.md)
