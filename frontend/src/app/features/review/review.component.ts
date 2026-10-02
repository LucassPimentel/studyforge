import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { Card, ReviewResult } from '../../core/models';
import { ApiService } from '../../core/services/api.service';

/**
 * Opção de avaliação estilo Anki apresentada ao usuário.
 *
 * O rótulo é o texto do botão; `grade` é o valor 0..5 enviado ao backend
 * (SM-2), conforme o mapeamento definido no design: Errei/Difícil/Bom/Fácil
 * correspondem a 0/3/4/5.
 */
interface OpcaoAvaliacao {
  readonly rotulo: string;
  readonly grade: number;
  readonly classe: string;
}

/**
 * Sessão de revisão de um deck.
 *
 * Fluxo (Requirement 3):
 * 1. Ao iniciar, carrega os cards devidos agora do deck da rota
 *    (`decks/:deckId/review`).
 * 2. Mostra um card por vez: primeiro a pergunta e um botão "Mostrar resposta".
 * 3. Revelada a resposta, exibe os botões estilo Anki (Errei/Difícil/Bom/Fácil
 *    → grade 0/3/4/5). Ao avaliar, aplica o SM-2 via `reviewCard` e avança para
 *    o próximo card devido.
 * 4. Ao terminar os cards, mostra uma mensagem de conclusão com volta aos decks.
 *
 * Standalone (Angular 15): declara suas próprias dependências em `imports`.
 * Consome o backend apenas via `ApiService` tipado (sem `any`).
 */
@Component({
  selector: 'app-review',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './review.component.html',
  styleUrls: ['./review.component.css'],
})
export class ReviewComponent implements OnInit {
  /** Id do deck em revisão, obtido da rota. */
  deckId = 0;

  /** Cards devidos a revisar nesta sessão (na ordem retornada pelo backend). */
  cards: Card[] = [];

  /** Índice do card atualmente exibido. */
  indiceAtual = 0;

  /** Indica se a resposta do card atual já foi revelada. */
  respostaVisivel = false;

  /** Carregamento inicial dos cards devidos. */
  carregando = false;

  /** Indica que uma avaliação está sendo enviada (desabilita os botões). */
  avaliando = false;

  /** Mensagem de erro a ser exibida ao usuário (null quando não há erro). */
  erro: string | null = null;

  /** True quando o deck da rota não foi encontrado (404). */
  deckNaoEncontrado = false;

  /**
   * Resultado da última avaliação (null quando nenhum feedback está em exibição).
   * Enquanto preenchido, a tela mostra em quanto tempo o card será revisado de novo
   * antes de avançar para o próximo.
   */
  ultimaAvaliacao: ReviewResult | null = null;

  /** Handle do timer de avanço automático, para poder cancelá-lo ao avançar manualmente. */
  private avancoTimer: ReturnType<typeof setTimeout> | null = null;

  /** Tempo (ms) que o feedback da próxima revisão fica visível antes do avanço automático. */
  private readonly feedbackMs = 2000;

  /**
   * Botões de avaliação estilo Anki, com o mapeamento de grade do design:
   * Errei=0, Difícil=3, Bom=4, Fácil=5.
   */
  readonly opcoes: readonly OpcaoAvaliacao[] = [
    { rotulo: 'Errei', grade: 0, classe: 'review-grades__button--errei' },
    { rotulo: 'Difícil', grade: 3, classe: 'review-grades__button--dificil' },
    { rotulo: 'Bom', grade: 4, classe: 'review-grades__button--bom' },
    { rotulo: 'Fácil', grade: 5, classe: 'review-grades__button--facil' },
  ];

  constructor(
    private readonly api: ApiService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    const parametro = this.route.snapshot.paramMap.get('deckId');
    const id = Number(parametro);

    // Rota sem id válido não deveria ocorrer; redireciona de volta aos decks por segurança.
    if (!Number.isInteger(id) || id <= 0) {
      void this.router.navigate(['/decks']);
      return;
    }

    this.deckId = id;
    this.carregarDevidos();
  }

  /** Card atualmente em revisão (undefined quando a sessão terminou). */
  get cardAtual(): Card | undefined {
    return this.cards[this.indiceAtual];
  }

  /** True quando todos os cards devidos já foram avaliados. */
  get sessaoConcluida(): boolean {
    return this.cards.length > 0 && this.indiceAtual >= this.cards.length;
  }

  /** Carrega os cards devidos agora do deck da rota. */
  carregarDevidos(): void {
    this.carregando = true;
    this.erro = null;
    this.deckNaoEncontrado = false;

    this.api
      .getDueCards(this.deckId)
      .pipe(finalize(() => (this.carregando = false)))
      .subscribe({
        next: (cards) => {
          this.cards = cards;
          this.indiceAtual = 0;
          this.respostaVisivel = false;
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 404) {
            this.deckNaoEncontrado = true;
          } else {
            this.erro = this.descreverErro(err);
          }
        },
      });
  }

  /** Revela a resposta do card atual (habilita os botões de avaliação). */
  mostrarResposta(): void {
    this.respostaVisivel = true;
  }

  /**
   * Avalia o card atual com a grade informada, aplica o SM-2 no backend e
   * avança para o próximo card devido.
   */
  avaliar(grade: number): void {
    const card = this.cardAtual;
    if (!card || this.avaliando) {
      return;
    }

    this.avaliando = true;
    this.erro = null;

    this.api
      .reviewCard(card.id, { grade })
      .pipe(finalize(() => (this.avaliando = false)))
      .subscribe({
        next: (resultado) => this.mostrarFeedbackEAvancar(resultado),
        error: (err: HttpErrorResponse) => {
          this.erro = this.descreverErro(err);
        },
      });
  }

  /**
   * Exibe por um curto período em quanto tempo o card será revisado novamente e,
   * em seguida, avança automaticamente. O usuário também pode avançar na hora
   * via {@link avancarAgora}.
   */
  private mostrarFeedbackEAvancar(resultado: ReviewResult): void {
    this.ultimaAvaliacao = resultado;
    this.avancoTimer = setTimeout(() => this.avancar(), this.feedbackMs);
  }

  /** Avança imediatamente, cancelando o avanço automático pendente. */
  avancarAgora(): void {
    if (this.avancoTimer !== null) {
      clearTimeout(this.avancoTimer);
      this.avancoTimer = null;
    }
    this.avancar();
  }

  /**
   * Descreve em linguagem natural quando o card será revisado novamente,
   * a partir do intervalo (em dias) calculado pelo SM-2.
   */
  descreverProximaRevisao(resultado: ReviewResult): string {
    const dias = resultado.interval;
    const quando = dias <= 1 ? 'em 1 dia' : `em ${dias} dias`;
    const data = new Date(resultado.nextReview).toLocaleDateString('pt-BR');
    return `Próxima revisão ${quando} (${data}).`;
  }

  /** Avança para o próximo card e esconde a resposta. */
  private avancar(): void {
    this.avancoTimer = null;
    this.ultimaAvaliacao = null;
    this.indiceAtual += 1;
    this.respostaVisivel = false;
  }

  /** Deriva uma mensagem amigável a partir do erro HTTP. */
  private descreverErro(err: HttpErrorResponse): string {
    switch (err.status) {
      case 0:
        return 'Falha de conexão com o servidor.';
      case 400:
        return 'Avaliação inválida.';
      case 404:
        return 'Card não encontrado. Ele pode ter sido excluído.';
      default:
        return 'Não foi possível concluir a operação.';
    }
  }
}
