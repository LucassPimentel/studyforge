# Custom Agents e Kiro Web / Cloud Sessions no StudyForge

Este documento cobre duas capacidades do Kiro demonstradas no StudyForge:
**custom agents** (agente restrito de geração de cards) e **Kiro Web / cloud sessions /
cloud configuration** (onde e como este projeto foi construído).

---

## 🤖 Custom agent — `card-generator`

### Objetivo
Um agente de **escopo mínimo** cuja única função é transformar notas de estudo em cards no
formato `pergunta :: resposta` usado pelo StudyForge. Ele **não** pode alterar o código da
aplicação — apenas ler notas e escrever cards.

### Configuração
Ver `.kiro/agents/card-generator.json`. Pontos de escopo restrito:

- **tools**: apenas `fs_read` e `fs_write` (sem `execute_bash`, sem rede).
- **fs_read.allowedPaths**: `study-notes/**` e a spec `study-core` (para conhecer o formato).
- **fs_read.deniedPaths** / **fs_write.deniedPaths**: bloqueiam `backend/**`, `frontend/**`,
  `.env` — o agente nunca toca no código da aplicação.
- **fs_write.allowedPaths**: só `study-notes/**`.
- **mcpServers**: apenas `notes-filesystem` — conecta a lição de MCP (restringir quais agentes
  acessam um MCP server) com a de custom agents.
- **resources**: inclui o design da spec `study-core` como contexto automático.

### Como usar no IDE
1. `git pull` no clone local.
2. Selecionar o agente `card-generator` no seletor de agentes do Kiro IDE.
3. Pedir: "leia study-notes e gere cards" → ele lê via MCP e devolve linhas
   `pergunta :: resposta` para colar no endpoint de geração do StudyForge.

> Por que isso é boa prática: dar ao agente só o que ele precisa (least privilege) evita que
> uma tarefa de geração de conteúdo altere código por engano.

---

## ☁️ Kiro Web, Cloud Sessions e Cloud Configuration

### O que foi demonstrado
Todo o planejamento do StudyForge (README, steering, specs, hook, config de MCP, este doc e o
custom agent) foi construído em uma **cloud session do Kiro Web**: um sandbox isolado que o Kiro
provisiona na nuvem e no qual clona o repositório **server-side** pelo provedor de código
conectado — a cópia local nunca é enviada. A sessão fica ligada à conta Kiro, então dá para
começar no navegador e continuar no IDE, CLI ou celular.

Fluxo real usado neste projeto:
1. Sessão cloud no Kiro Web criou e evoluiu os arquivos de configuração e specs.
2. Cada etapa foi commitada e enviada ao GitHub (a ponte entre a nuvem e a máquina local).
3. A implementação em C#/Angular acontece no **Kiro IDE** local (com .NET SDK), após `git pull`.

### Cloud Configuration (Configuration Sync)
Como a sessão cloud roda em sandbox gerenciado que **não acessa sua máquina**, configurações
locais (ex.: pastas de settings) não se aplicam automaticamente. A **Configuration Sync** cobre
essa lacuna: você faz upload das pastas de configuração suportadas do seu ambiente local para a
nuvem, para que a sessão cloud use os mesmos ajustes. Em uma sessão cloud, a configuração aparece
como originada da nuvem (cloud-sourced).

Para o StudyForge, as configs relevantes já vivem **no repositório** (`.kiro/steering`,
`.kiro/specs`, `.kiro/hooks`, `.kiro/settings/mcp.json`, `.kiro/agents`), então são clonadas
junto com o repo em qualquer sessão — a forma mais portátil de "sincronizar" configuração entre
superfícies.

### Superfícies usadas
- **Kiro Web (cloud session)**: planejamento, specs, configs — trabalho barato em tickets.
- **Kiro IDE**: implementação, execução dos testes (hook), property-based testing, MCP e Powers.
