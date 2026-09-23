# MCP e Powers no StudyForge

Este documento descreve como o StudyForge usa **Model Context Protocol (MCP)** e **Kiro Powers**
— duas capacidades do Kiro demonstradas neste projeto. As configurações vivem no repositório;
a ativação (instalar Power, aprovar o MCP server) acontece no **Kiro IDE**.

---

## 🔌 MCP — importar notas de arquivos locais

### Caso de uso
O StudyForge gera cards a partir de texto no formato `pergunta :: resposta`. Em vez de colar o
texto manualmente, usamos um **MCP filesystem server** para dar ao Kiro acesso de leitura a uma
pasta de notas (`study-notes/`). O agente pode então ler suas notas de estudo e ajudar a
transformá-las nas linhas `pergunta :: resposta` que o endpoint de geração consome.

Escolhemos o filesystem server porque é **local, sem autenticação e sem API externa** — simples
de rodar e barato em iterações.

### Configuração
Ver `.kiro/settings/mcp.json`:

```json
{
  "mcpServers": {
    "notes-filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "./study-notes"],
      "env": {},
      "disabled": false,
      "autoApprove": ["list_directory", "read_file"]
    }
  }
}
```

- **command/args**: sobe o servidor de filesystem apontando para `./study-notes`.
- **autoApprove**: só operações de leitura (`list_directory`, `read_file`) — nunca escrita.
- **disabled: false**: ativo por padrão.

### Como usar no IDE
1. `git pull` no clone local.
2. Abrir no Kiro IDE; o Kiro detecta `.kiro/settings/mcp.json` e sobe o server.
3. Aprovar o server na primeira execução (ícone "ghost" → MCP Servers).
4. Pedir ao Kiro algo como: "leia as notas em study-notes e gere linhas pergunta :: resposta
   para eu colar no StudyForge".

### Restrição de acesso por agente
A lição de MCP destaca que é possível **definir quais agentes** podem acessar um MCP server.
Quando criarmos o custom agent de geração de cards (lição de custom agents), daremos a ele
acesso a este server `notes-filesystem` — e apenas a ele — mantendo o escopo mínimo.

---

## ⚡ Powers — conhecimento e ferramentas empacotados

### O que é
Um Power empacota tools, agent skills e boas práticas que o Kiro carrega **sob demanda** quando
você menciona um keyword relacionado. Powers seguem a especificação aberta **Agent Plugins**.
Quando um Power inclui um MCP server, o Kiro faz o namespacing automático dele.

### Power escolhido para o StudyForge
Como o backend expõe uma **API REST**, um Power de **testes de contrato de API (ex.: Swagger/
contract testing)** agrega conhecimento diretamente útil: ao mencionar keywords como "API",
"endpoint" ou "contrato", o Kiro traz as boas práticas e ferramentas desse Power para validar
que nossos endpoints (decks, cards, review, progress) batem com o contrato documentado na spec
`study-core`.

> Alternativas igualmente válidas já disponíveis: o Power **github-cli** (fluxo de PRs/branches)
> ou **playwright** (testes de UI da SPA Angular). Qualquer um demonstra a lição; escolha o que
> você for realmente usar no projeto.

### Como instalar no IDE
1. Abrir o painel de Powers no Kiro IDE (ícone "Ghosty" com o raio ⚡) — ou navegar em
   `kiro.dev/powers` e clicar **Add to Kiro**.
2. Escolher o Power e clicar **+ Install**.
3. Depois, ao fazer perguntas com keywords do Power, o Kiro ativa o Power automaticamente.

> A instalação de Powers é feita no IDE; este documento registra a decisão e o racional.
