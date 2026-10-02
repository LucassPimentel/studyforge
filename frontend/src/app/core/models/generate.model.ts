/**
 * Dados de entrada para gerar cards a partir de um bloco de texto.
 *
 * Cada linha no formato `pergunta :: resposta` vira um card.
 * Espelha `GenerateCardsDto` (`POST /api/decks/{id}/cards/generate`).
 */
export interface GenerateCards {
  readonly text: string;
}

/**
 * Resultado da geração de cards: quantidade criada.
 *
 * Espelha `GenerateResultDto`.
 */
export interface GenerateResult {
  readonly created: number;
}
