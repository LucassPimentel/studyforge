/**
 * Configuração de ambiente de desenvolvimento.
 *
 * `apiBaseUrl` aponta para o backend StudyForge.Api rodando localmente. O CORS de
 * desenvolvimento da API libera `http://localhost:4200` (origem padrão do `ng serve`).
 */
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5200/api',
};
