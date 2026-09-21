---
inclusion: always
---

# StudyForge — Visão do Produto

## O que é
StudyForge transforma material de estudo em texto (notas, resumos, trechos) em
flashcards e os revisa com repetição espaçada. Objetivo: reter mais, estudando menos.

## Usuário-alvo
Estudantes que querem revisar conteúdo de forma eficiente, sem montar flashcards
manualmente e sem decidir por conta própria quando revisar cada card.

## Princípios de produto
- **Simplicidade primeiro**: o MVP deve funcionar ponta a ponta antes de qualquer extra.
- **O algoritmo é o coração**: a qualidade do agendamento de revisões é o diferencial.
- **Escopo enxuto**: preferir uma feature completa e funcional a várias pela metade.

## Escopo do MVP (nesta ordem)
1. Criar decks.
2. Gerar cards a partir de um texto colado usando uma **regra simples** (sem IA):
   cada linha no formato `pergunta :: resposta` vira um card. Linhas vazias são ignoradas.
3. Sessão de revisão dos cards devidos hoje.
4. Agendamento por repetição espaçada (SM-2) com base na avaliação do usuário.
5. Visão de progresso (pendentes / aprendendo / dominados).

## Avaliação na revisão (botões estilo Anki)
Durante a revisão, o usuário classifica o card com botões que mapeiam para uma grade 0–5:
- **Errei** → grade 0 (reinicia o agendamento)
- **Difícil** → grade 3
- **Bom** → grade 4
- **Fácil** → grade 5

## Fora do escopo do MVP (extras/bônus)
- Parsing de PDF.
- Geração de cards por IA.
- Gráficos avançados de estatística.
- Multiusuário / autenticação.
