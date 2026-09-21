---
inclusion: always
---

# StudyForge — Fluxo de Trabalho e Convenções de Projeto

## Fluxo de desenvolvimento
- Features não triviais começam por uma **spec** em `.kiro/specs/` (requisitos → design → tasks)
  antes da implementação. A prioritária é o **algoritmo de repetição espaçada**.
- Implementar seguindo as tasks da spec, uma de cada vez.

## Git
- Mensagens de commit no imperativo e concisas (ex.: "Add deck model", "Implement SM-2 scheduling").
- Primeiro commit do projeto deve ocorrer dentro da janela do challenge (em ou após 21/09/2026).
- Commits pequenos e focados por feature.

## Domínio — conceitos-chave
- **Deck**: coleção de cards de um assunto.
- **Card**: par pergunta/resposta.
- **ReviewSchedule**: estado de agendamento de um card (intervalo atual, facilidade, próxima revisão).
- **Avaliação (grade)**: quão bem o usuário lembrou (0–5, botões estilo Anki), alimenta o algoritmo.

## Algoritmo de repetição espaçada — SM-2
- Usar o algoritmo **SM-2** (o mesmo usado pelo Anki, em variação).
- Cada card mantém: `easeFactor` (fator de facilidade, começa em 2.5), `interval` (dias até a
  próxima revisão), `repetitions` (nº de acertos consecutivos).
- A cada revisão o usuário dá uma **avaliação (grade 0–5)** vinda dos botões estilo Anki
  (ver `product.md`). Regras:
  - grade < 3 (errou): `repetitions = 0`, `interval = 1` (reinicia).
  - grade >= 3 (acertou): incrementa `repetitions`; intervalo cresce
    (1º acerto → 1 dia, 2º → 6 dias, depois `interval = round(interval * easeFactor)`).
  - Atualizar `easeFactor` conforme a grade (não deixar cair abaixo de 1.3).
  - `nextReview = agora + interval dias`.
- Deve ser **puro e testável**: dado (estado do card + grade + "agora") → novo estado + próxima data.
- Sem dependência de relógio global dentro da função pura (passar "agora" como parâmetro).

## Definição de pronto (MVP)
- App roda ponta a ponta: criar deck → gerar cards → revisar → card reagendado.
- `SpacedRepetitionService` coberto por testes de unidade.
- README atualizado com instruções de execução.
