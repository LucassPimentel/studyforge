/**
 * Avaliação de lembrança de um card em uma revisão (0..5).
 *
 * Espelha `GradeDto` (`POST /api/cards/{id}/review`).
 */
export interface Grade {
  readonly grade: number;
}

/**
 * Novo estado de agendamento retornado após avaliar um card.
 *
 * `nextReview` é um instante UTC em ISO 8601 (string), conforme serializado pela API.
 * Espelha `ReviewResultDto`.
 */
export interface ReviewResult {
  readonly cardId: number;
  readonly easeFactor: number;
  readonly interval: number;
  readonly repetitions: number;
  readonly nextReview: string;
}
