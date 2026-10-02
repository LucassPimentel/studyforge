/**
 * Progresso de um deck em contagens por estágio de aprendizado.
 *
 * Espelha `ProgressDto` (`GET /api/decks/{id}/progress`):
 * - `pending`: nunca revisados (repetitions == 0)
 * - `learning`: repetitions entre 1 e 2
 * - `mastered`: repetitions >= 3
 * - `due`: devidos agora (nextReview <= agora)
 */
export interface Progress {
  readonly pending: number;
  readonly learning: number;
  readonly mastered: number;
  readonly due: number;
}
