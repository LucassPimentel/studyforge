# Requirements — Spaced Repetition Scheduling (SM-2)

## Introduction

O coração do StudyForge é o agendamento por repetição espaçada. Após revisar um card, o
usuário informa quão bem lembrou (via botões estilo Anki), e o sistema calcula quando esse
card deve ser revisado novamente. O algoritmo adotado é o **SM-2** (o mesmo usado, em variação,
pelo Anki). Esta spec cobre apenas a lógica de agendamento — pura, determinística e testável —
independente de banco de dados, HTTP ou UI.

## Glossário

- **easeFactor**: fator de facilidade do card (quão fácil ele é para o usuário). Começa em 2.5.
- **interval**: número de dias até a próxima revisão.
- **repetitions**: número de acertos consecutivos (grade >= 3).
- **grade**: avaliação do usuário na revisão, inteiro de 0 a 5.
- **nextReview**: data/hora da próxima revisão.

## Requirements

### Requirement 1 — Cálculo do novo estado a partir de uma avaliação
**User Story:** Como estudante, quero que o sistema recalcule o agendamento de um card com base
em como eu avaliei minha lembrança, para que cards fáceis apareçam menos e difíceis mais.

#### Acceptance Criteria
1. WHEN o serviço recebe o estado atual de um card (easeFactor, interval, repetitions) e uma grade THEN o serviço SHALL retornar um novo estado (easeFactor, interval, repetitions) e uma nextReview.
2. IF grade < 3 THEN o serviço SHALL definir repetitions = 0 e interval = 1 (reinício do card).
3. WHEN grade >= 3 THEN o serviço SHALL incrementar repetitions em 1.
4. WHEN grade >= 3 AND repetitions resultante == 1 THEN o serviço SHALL definir interval = 1.
5. WHEN grade >= 3 AND repetitions resultante == 2 THEN o serviço SHALL definir interval = 6.
6. WHEN grade >= 3 AND repetitions resultante > 2 THEN o serviço SHALL definir interval = round(interval_anterior * easeFactor).

### Requirement 2 — Atualização do fator de facilidade
**User Story:** Como estudante, quero que cards que eu acho difíceis cresçam de intervalo mais
devagar, para revisá-los com mais frequência.

#### Acceptance Criteria
1. WHEN o serviço processa uma grade THEN o serviço SHALL atualizar o easeFactor segundo a fórmula SM-2: `EF' = EF + (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02))`.
2. IF o easeFactor calculado for menor que 1.3 THEN o serviço SHALL fixá-lo em 1.3.
3. O serviço SHALL atualizar o easeFactor tanto para acertos quanto para erros.

### Requirement 3 — Cálculo da próxima data de revisão
**User Story:** Como estudante, quero saber a data em que cada card deve ser revisado.

#### Acceptance Criteria
1. WHEN o serviço calcula o novo interval THEN o serviço SHALL definir nextReview = now + interval dias.
2. O serviço SHALL receber o instante "now" como parâmetro (não usar relógio global), para ser determinístico e testável.

### Requirement 4 — Estado inicial de um card novo
**User Story:** Como estudante, quero criar cards novos que já entrem no ciclo de revisão.

#### Acceptance Criteria
1. WHEN um card é criado THEN o card SHALL iniciar com easeFactor = 2.5, interval = 0, repetitions = 0.
2. WHEN um card novo é criado THEN nextReview SHALL ser now (fica devido imediatamente).

### Requirement 5 — Validação de entrada
#### Acceptance Criteria
1. IF grade estiver fora do intervalo 0..5 THEN o serviço SHALL rejeitar a operação com erro de validação.
2. IF o estado do card contiver valores negativos de interval ou repetitions THEN o serviço SHALL rejeitar a operação com erro de validação.

### Requirement 6 — Determinismo e pureza
#### Acceptance Criteria
1. O serviço SHALL ser uma função pura: mesma entrada (estado + grade + now) produz sempre a mesma saída.
2. O serviço SHALL NOT acessar banco de dados, rede ou relógio do sistema diretamente.
