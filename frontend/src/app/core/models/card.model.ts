/**
 * Card exposto em uma sessão de revisão, sem os campos internos de agendamento.
 *
 * Espelha `CardDto` do backend (`GET /api/decks/{id}/review`).
 */
export interface Card {
  readonly id: number;
  readonly question: string;
  readonly answer: string;
}
