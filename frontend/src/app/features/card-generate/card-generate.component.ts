import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ApiService } from '../../core/services/api.service';

/**
 * Tela de geração de cards: o usuário cola um bloco de texto no formato
 * `pergunta :: resposta` (um por linha) e o backend cria os cards do deck.
 *
 * Standalone (Angular 15): declara suas próprias dependências em `imports`.
 * O id do deck vem da rota (`decks/:deckId/generate`), consistente com a
 * navegação por rota já usada para a revisão. Consome o backend apenas via
 * `ApiService` tipado (sem `any`).
 */
@Component({
  selector: 'app-card-generate',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './card-generate.component.html',
  styleUrls: ['./card-generate.component.css'],
})
export class CardGenerateComponent implements OnInit {
  /** Id do deck alvo, obtido da rota. */
  deckId = 0;

  /** Campo do texto a ser interpretado; precisa ter conteúdo não vazio. */
  readonly textControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, this.naoVazioValidator],
  });

  /**
   * Formulário de geração. Ligar este `FormGroup` ao `<form>` via `[formGroup]`
   * garante que o `FormGroupDirective` do ReactiveFormsModule intercepte o submit
   * e chame `preventDefault()` — sem isso o botão `type="submit"` dispara o submit
   * nativo do navegador e recarrega a página.
   */
  readonly form = new FormGroup({
    text: this.textControl,
  });

  /** Indica que uma geração está em andamento (desabilita o formulário). */
  gerando = false;

  /** Quantidade de cards criados na última geração bem-sucedida (null se ainda não gerou). */
  criados: number | null = null;

  /** Mensagem de erro a ser exibida ao usuário (null quando não há erro). */
  erro: string | null = null;

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
  }

  /** Envia o texto para o backend e exibe a contagem de cards criados no sucesso. */
  gerarCards(): void {
    if (this.textControl.invalid || this.gerando) {
      this.textControl.markAsTouched();
      return;
    }

    const texto = this.textControl.value;
    this.gerando = true;
    this.erro = null;
    this.criados = null;

    this.api
      .generateCards(this.deckId, { text: texto })
      .pipe(finalize(() => (this.gerando = false)))
      .subscribe({
        next: (resultado) => {
          this.criados = resultado.created;
          this.textControl.reset('');
        },
        error: (err: HttpErrorResponse) => {
          this.erro = this.descreverErro(err);
        },
      });
  }

  /** Validador: rejeita valores compostos apenas por espaços/quebras de linha. */
  private naoVazioValidator(control: FormControl<string>): { [key: string]: boolean } | null {
    return control.value.trim().length === 0 ? { vazio: true } : null;
  }

  /** Deriva uma mensagem amigável a partir do erro HTTP. */
  private descreverErro(err: HttpErrorResponse): string {
    switch (err.status) {
      case 0:
        return 'Falha de conexão com o servidor.';
      case 400:
        return 'Texto inválido. Informe ao menos uma linha no formato "pergunta :: resposta".';
      case 404:
        return 'Deck não encontrado. Ele pode ter sido excluído.';
      default:
        return 'Não foi possível gerar os cards.';
    }
  }
}
