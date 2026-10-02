import { Routes } from '@angular/router';

/**
 * Rotas da aplicação (shell).
 *
 * As features standalone são carregadas via lazy loading (`loadComponent`):
 * - `decks` (DecksComponent) — listar/criar/excluir decks (Task 10).
 * - `decks/:deckId/generate` (CardGenerateComponent) — gerar cards (Task 11).
 * - `decks/:deckId/review` (ReviewComponent) — sessão de revisão (Task 12).
 *
 * O caminho raiz redireciona para `decks`.
 */
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'decks' },
  {
    path: 'decks',
    loadComponent: () =>
      import('./features/decks/decks.component').then((m) => m.DecksComponent),
  },
  {
    path: 'decks/:deckId/generate',
    loadComponent: () =>
      import('./features/card-generate/card-generate.component').then(
        (m) => m.CardGenerateComponent,
      ),
  },
  {
    path: 'decks/:deckId/review',
    loadComponent: () =>
      import('./features/review/review.component').then((m) => m.ReviewComponent),
  },
];
