/**
 * Resumo de um deck para listagem.
 *
 * Espelha `DeckSummaryDto` do backend (`GET /api/decks`, `POST /api/decks`).
 */
export interface DeckSummary {
  readonly id: number;
  readonly name: string;
  readonly cardCount: number;
}

/**
 * Dados de entrada para a criação de um deck (`POST /api/decks`).
 *
 * Espelha `CreateDeckDto`.
 */
export interface CreateDeck {
  readonly name: string;
}
