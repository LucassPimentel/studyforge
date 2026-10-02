/**
 * Configuração de ambiente de produção.
 *
 * Em produção o frontend normalmente é servido pela mesma origem da API, então usamos um
 * caminho relativo (`/api`). Ajuste conforme o deploy.
 */
export const environment = {
  production: true,
  apiBaseUrl: '/api',
};
