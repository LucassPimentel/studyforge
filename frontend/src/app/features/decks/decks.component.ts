import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { DeckSummary, Progress } from '../../core/models';

/**
 * Tela de decks: lista, cria e exclui decks, exibindo o progresso de cada um
 * e um link para a sessão de revisão.
 *
 * Standalone (Angular 15): declara suas próprias dependências em `imports`.
 * Consome o backend exclusivamente via `ApiService` tipado.
 */
@Component({
  selector: 'app-decks',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './decks.component.html',
  styleUrls: ['./decks.component.css'],
})
export class DecksComponent implements OnInit {
  /** Decks carregados do backend. */
  decks: DeckSummary[] = [];

  /** Progresso por deck (chave = id do deck). Carregado sob demanda após a listagem. */
  progressByDeck = new Map<number, Progress>();

  /** Campo do nome do novo deck; nome não vazio (sem espaços) é obrigatório. */
  readonly nameControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, this.naoVazioValidator],
  });

  /**
   * Formulário de criação. Usar um `FormGroup` ligado ao `<form>` via `[formGroup]`
   * garante que o `FormGroupDirective` do ReactiveFormsModule controle o submit e
   * chame `preventDefault()` — sem isso o botão `type="submit"` dispara o submit
   * nativo do navegador e recarrega a página.
   */
  readonly form = new FormGroup({
    name: this.nameControl,
  });

  /** Estado de carregamento da listagem. */
  carregando = false;

  /** Indica que uma criação está em andamento (desabilita o formulário). */
  criando = false;

  /** Id do deck em processo de exclusão (para desabilitar o botão específico). */
  excluindoId: number | null = null;

  /** Mensagem de erro a ser exibida ao usuário (null quando não há erro). */
  erro: string | null = null;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.carregarDecks();
  }

  /** Carrega a lista de decks e, em seguida, o progresso de cada um. */
  carregarDecks(): void {
    this.carregando = true;
    this.erro = null;

    this.api
      .listDecks()
      .pipe(finalize(() => (this.carregando = false)))
      .subscribe({
        next: (decks) => {
          this.decks = decks;
          this.carregarProgressos(decks);
        },
        error: (err: HttpErrorResponse) => {
          this.erro = this.descreverErro(err, 'Não foi possível carregar os decks.');
        },
      });
  }

  /** Cria um deck a partir do nome informado e recarrega a lista no sucesso. */
  criarDeck(): void {
    if (this.nameControl.invalid || this.criando) {
      this.nameControl.markAsTouched();
      return;
    }

    const nome = this.nameControl.value.trim();
    this.criando = true;
    this.erro = null;

    this.api
      .createDeck({ name: nome })
      .pipe(finalize(() => (this.criando = false)))
      .subscribe({
        next: () => {
          this.nameControl.reset('');
          this.carregarDecks();
        },
        error: (err: HttpErrorResponse) => {
          this.erro =
            err.status === 400
              ? 'Nome de deck inválido. Informe um nome não vazio.'
              : this.descreverErro(err, 'Não foi possível criar o deck.');
        },
      });
  }

  /** Exclui um deck (e seus cards em cascata) e recarrega a lista no sucesso. */
  excluirDeck(deck: DeckSummary): void {
    if (this.excluindoId !== null) {
      return;
    }

    const confirmado = confirm(
      `Excluir o deck "${deck.name}"? Todos os seus ${deck.cardCount} card(s) também serão removidos.`,
    );
    if (!confirmado) {
      return;
    }

    this.excluindoId = deck.id;
    this.erro = null;

    this.api
      .deleteDeck(deck.id)
      .pipe(finalize(() => (this.excluindoId = null)))
      .subscribe({
        next: () => this.carregarDecks(),
        error: (err: HttpErrorResponse) => {
          this.erro = this.descreverErro(err, 'Não foi possível excluir o deck.');
        },
      });
  }

  /** Retorna o progresso já carregado de um deck, se disponível. */
  progressoDe(deckId: number): Progress | undefined {
    return this.progressByDeck.get(deckId);
  }

  /** Carrega o progresso de cada deck de forma independente (falhas são silenciosas). */
  private carregarProgressos(decks: DeckSummary[]): void {
    this.progressByDeck.clear();
    for (const deck of decks) {
      this.api.getProgress(deck.id).subscribe({
        next: (progresso) => this.progressByDeck.set(deck.id, progresso),
        // Progresso é informação auxiliar: uma falha pontual não deve bloquear a tela.
        error: () => void 0,
      });
    }
  }

  /** Validador: rejeita valores compostos apenas por espaços. */
  private naoVazioValidator(control: FormControl<string>): { [key: string]: boolean } | null {
    return control.value.trim().length === 0 ? { vazio: true } : null;
  }

  /** Deriva uma mensagem amigável a partir do erro HTTP. */
  private descreverErro(err: HttpErrorResponse, padrao: string): string {
    return err.status === 0 ? 'Falha de conexão com o servidor.' : padrao;
  }
}
