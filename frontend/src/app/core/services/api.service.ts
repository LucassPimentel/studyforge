import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  Card,
  CreateDeck,
  DeckSummary,
  GenerateCards,
  GenerateResult,
  Grade,
  Progress,
  ReviewResult,
} from '../models';

/**
 * Cliente HTTP tipado para o backend StudyForge.Api.
 *
 * Cada método corresponde a um endpoint REST. A base da URL vem de
 * `environment.apiBaseUrl`, permitindo configuração por ambiente (dev/prod).
 * Não usa `any`: todas as entradas e saídas são tipadas pelos modelos em `core/models`.
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  // --- Decks -------------------------------------------------------------

  /** Lista todos os decks com nome e contagem de cards. `GET /api/decks`. */
  listDecks(): Observable<DeckSummary[]> {
    return this.http.get<DeckSummary[]>(`${this.baseUrl}/decks`);
  }

  /** Cria um deck. Retorna 400 se o nome for vazio. `POST /api/decks`. */
  createDeck(payload: CreateDeck): Observable<DeckSummary> {
    return this.http.post<DeckSummary>(`${this.baseUrl}/decks`, payload);
  }

  /** Exclui um deck e seus cards em cascata. 204/404. `DELETE /api/decks/{id}`. */
  deleteDeck(deckId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/decks/${deckId}`);
  }

  // --- Geração de cards --------------------------------------------------

  /**
   * Gera cards de um deck a partir de um bloco de texto (`pergunta :: resposta` por linha).
   * `POST /api/decks/{id}/cards/generate`.
   */
  generateCards(deckId: number, payload: GenerateCards): Observable<GenerateResult> {
    return this.http.post<GenerateResult>(
      `${this.baseUrl}/decks/${deckId}/cards/generate`,
      payload,
    );
  }

  // --- Revisão -----------------------------------------------------------

  /**
   * Retorna os cards devidos agora de um deck (lista vazia se não houver).
   * `GET /api/decks/{id}/review`.
   */
  getDueCards(deckId: number): Observable<Card[]> {
    return this.http.get<Card[]>(`${this.baseUrl}/decks/${deckId}/review`);
  }

  /**
   * Avalia um card com uma grade (0..5), aplica o SM-2 e retorna o novo agendamento.
   * `POST /api/cards/{id}/review`.
   */
  reviewCard(cardId: number, payload: Grade): Observable<ReviewResult> {
    return this.http.post<ReviewResult>(
      `${this.baseUrl}/cards/${cardId}/review`,
      payload,
    );
  }

  // --- Progresso ---------------------------------------------------------

  /** Retorna o progresso do deck (pending/learning/mastered/due). `GET /api/decks/{id}/progress`. */
  getProgress(deckId: number): Observable<Progress> {
    return this.http.get<Progress>(`${this.baseUrl}/decks/${deckId}/progress`);
  }
}
