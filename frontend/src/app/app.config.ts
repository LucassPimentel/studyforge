import { ApplicationConfig } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';

/**
 * Configuração raiz da aplicação standalone.
 *
 * - `provideHttpClient`: habilita o `HttpClient` injetado pelo `ApiService`.
 * - `provideRouter`: registra as rotas do shell (ver `app.routes.ts`).
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideRouter(routes),
  ],
};
