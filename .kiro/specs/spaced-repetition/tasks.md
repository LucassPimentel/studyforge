# Tasks — Spaced Repetition Scheduling (SM-2)

- [ ] 1. Criar o projeto de domínio/serviço no backend
  - Definir `ReviewGrade` (enum: Again=0, Hard=3, Good=4, Easy=5)
  - Definir `SchedulingState` (record: EaseFactor, Interval, Repetitions, NextReview)
  - Definir a interface `ISpacedRepetitionService`
  - _Requirements: 1.1, 4.1_

- [ ] 2. Implementar `CreateInitialState`
  - Retornar (EaseFactor=2.5, Interval=0, Repetitions=0, NextReview=now)
  - _Requirements: 4.1, 4.2_

- [ ] 3. Implementar validação de entrada em `ApplyReview`
  - Rejeitar grade fora de 0..5 (ArgumentOutOfRangeException)
  - Rejeitar interval/repetitions negativos (ArgumentException)
  - _Requirements: 5.1, 5.2_

- [ ] 4. Implementar atualização do easeFactor
  - Aplicar fórmula SM-2 e fixar piso em 1.3
  - _Requirements: 2.1, 2.2, 2.3_

- [ ] 5. Implementar o cálculo de repetitions e interval
  - Erro (grade < 3): repetitions=0, interval=1
  - Acerto: incrementar repetitions; interval 1 / 6 / round(interval*ef)
  - _Requirements: 1.2, 1.3, 1.4, 1.5, 1.6_

- [ ] 6. Implementar cálculo de nextReview
  - nextReview = now + interval dias (UTC, now por parâmetro)
  - _Requirements: 3.1, 3.2_

- [ ] 7. Escrever testes de unidade (xUnit)
  - Estado inicial; 1º/2º/3º acerto; erro reinicia; piso do easeFactor;
    nextReview correto; grade inválida lança; determinismo
  - _Requirements: 1.*, 2.*, 3.*, 4.*, 5.*, 6.*_

- [ ] 8. Escrever testes property-based (Kiro IDE)
  - Implementar as invariantes descritas em design.md (piso do easeFactor, erro reinicia,
    acerto incrementa repetitions, interval não-negativo, nextReview coerente,
    monotonicidade na grade, determinismo, grade inválida lança)
  - Rodar via suporte a property-based testing do IDE
  - _Requirements: 1.*, 2.1, 2.2, 3.*, 5.*, 6.1_
