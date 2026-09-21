# Tasks — Study Core (Decks, Card Generation, Review Session)

> Depende da spec `spaced-repetition` (ISpacedRepetitionService).

- [ ] 1. Configurar o backend .NET Web API
  - Criar projeto Web API + referência ao serviço SM-2
  - Configurar EF Core + SQLite (`studyforge.db`) e CORS para o Angular
  - _Requirements: 5.1, 6.3_

- [ ] 2. Definir entidades e DbContext
  - `Deck`, `Card` (com campos de agendamento), relação 1..* com cascade delete
  - `AppDbContext` + migration inicial
  - _Requirements: 5.1, 5.2, 5.3_

- [ ] 3. Implementar DeckService + endpoints de deck
  - Criar (nome não vazio), listar (com cardCount), excluir (cascade)
  - Validação de nome vazio (400)
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [ ] 4. Implementar CardService.GenerateFromText + endpoint generate
  - Parsear `pergunta :: resposta` por linha; ignorar vazias e sem `::`; trim
  - Estado inicial via ISpacedRepetitionService; retornar contagem criada
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [ ] 5. Implementar ReviewService + endpoints de revisão
  - GET due cards (nextReview <= now); POST grade aplica SM-2 e persiste
  - Validar grade 0..5 (400); 404 se card/deck não existir
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 6.2_

- [ ] 6. Implementar progresso do deck
  - Contagens: pending / learning / mastered / due
  - _Requirements: 4.1, 4.2_

- [ ] 7. Definir DTOs e garantir que entidades EF não sejam expostas
  - _Requirements: 6.1_

- [ ] 8. Testes de unidade do backend (xUnit)
  - GenerateFromText (casos válidos/inválidos/trim/contagem)
  - GradeCard delega ao SM-2 e persiste (SQLite in-memory)
  - Buckets de progresso
  - _Requirements: 2.*, 3.2, 4.*_

- [ ] 9. Frontend Angular — estrutura e ApiService tipado
  - Modelos em `core/`, `ApiService` sem `any`
  - _Requirements: 6.1_

- [ ] 10. Frontend — DecksComponent (listar/criar/excluir)
  - _Requirements: 1.*_

- [ ] 11. Frontend — CardGenerateComponent (textarea → generate)
  - _Requirements: 2.*_

- [ ] 12. Frontend — ReviewComponent (pergunta → resposta → botões Anki → grade)
  - Botões Errei/Difícil/Bom/Fácil mapeiam para grade 0/3/4/5
  - _Requirements: 3.*_

- [ ] 13. Verificação ponta a ponta (MVP)
  - criar deck → gerar cards → revisar → card reagendado; README atualizado
  - _Requirements: 1-6 (fluxo completo)_
