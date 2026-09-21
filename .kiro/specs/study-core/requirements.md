# Requirements — Study Core (Decks, Card Generation, Review Session)

## Introduction

Esta spec cobre o fluxo principal do StudyForge em torno do algoritmo SM-2 (ver spec
`spaced-repetition`): criar decks, gerar cards a partir de texto, e conduzir uma sessão de
revisão que atualiza o agendamento de cada card. Abrange backend (API REST + SQLite via EF Core)
e o contrato consumido pelo frontend Angular.

## Glossário

- **Deck**: coleção de cards de um assunto.
- **Card**: par pergunta/resposta com estado de agendamento embutido.
- **Due card**: card cujo `nextReview` <= agora.
- **Grade**: avaliação 0..5 (botões Errei/Difícil/Bom/Fácil).

## Requirements

### Requirement 1 — Gerenciar decks
**User Story:** Como estudante, quero criar e listar decks, para organizar meu estudo por assunto.

#### Acceptance Criteria
1. WHEN o usuário cria um deck com nome não vazio THEN o sistema SHALL persistir o deck e retornar seu id.
2. IF o nome do deck for vazio ou só espaços THEN o sistema SHALL rejeitar com erro de validação (400).
3. WHEN o usuário lista decks THEN o sistema SHALL retornar cada deck com nome e contagem de cards.
4. WHEN o usuário exclui um deck THEN o sistema SHALL remover o deck e todos os seus cards.

### Requirement 2 — Gerar cards a partir de texto (regra simples)
**User Story:** Como estudante, quero colar um texto e gerar vários cards de uma vez, sem montá-los
manualmente um a um.

#### Acceptance Criteria
1. WHEN o usuário envia um bloco de texto para um deck THEN o sistema SHALL interpretar cada linha no formato `pergunta :: resposta` como um card.
2. IF uma linha não contiver o separador `::` THEN o sistema SHALL ignorar essa linha.
3. WHEN uma linha estiver vazia ou só com espaços THEN o sistema SHALL ignorá-la.
4. WHEN os cards são criados THEN cada card SHALL iniciar com o estado de agendamento inicial (easeFactor 2.5, interval 0, repetitions 0, nextReview = agora).
5. WHEN a geração termina THEN o sistema SHALL retornar quantos cards foram criados.
6. WHERE ambos os lados existem, o sistema SHALL fazer trim de espaços em pergunta e resposta.

### Requirement 3 — Sessão de revisão
**User Story:** Como estudante, quero revisar os cards devidos hoje e avaliar minha lembrança,
para que o sistema reagende cada card.

#### Acceptance Criteria
1. WHEN o usuário inicia uma sessão de revisão de um deck THEN o sistema SHALL retornar apenas os cards com nextReview <= agora (due cards).
2. WHEN o usuário avalia um card com uma grade (0..5) THEN o sistema SHALL aplicar o SM-2 (spec spaced-repetition) e persistir o novo estado de agendamento.
3. IF a grade for inválida (fora de 0..5) THEN o sistema SHALL rejeitar com erro de validação (400).
4. WHEN não houver cards devidos THEN o sistema SHALL retornar uma sessão vazia.

### Requirement 4 — Progresso do deck
**User Story:** Como estudante, quero ver meu progresso em um deck, para saber quanto falta dominar.

#### Acceptance Criteria
1. WHEN o usuário consulta o progresso de um deck THEN o sistema SHALL retornar contagens de: pendentes (nunca revisados / repetitions=0), aprendendo (repetitions entre 1 e 2), e dominados (repetitions >= 3).
2. WHEN o usuário consulta o progresso THEN o sistema SHALL retornar também o total de cards devidos agora.

### Requirement 5 — Persistência
#### Acceptance Criteria
1. O sistema SHALL persistir decks e cards em SQLite via EF Core.
2. WHEN um deck é excluído THEN o sistema SHALL excluir seus cards em cascata.
3. O sistema SHALL armazenar datas em UTC.

### Requirement 6 — Contrato de API
#### Acceptance Criteria
1. O sistema SHALL expor endpoints REST com DTOs de entrada/saída (não expor entidades EF diretamente).
2. O sistema SHALL retornar 404 quando um deck/card referenciado não existir.
3. O sistema SHALL habilitar CORS para o frontend Angular em desenvolvimento.
