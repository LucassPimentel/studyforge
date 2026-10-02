import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * Componente raiz (standalone) — shell da aplicação.
 *
 * Hospeda o `<router-outlet>` onde as features (decks/revisão) serão renderizadas.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  readonly title = 'StudyForge';
}
