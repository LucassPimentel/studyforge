# Spaced Repetition Power ⚡

A Kiro Power that packages the knowledge and tooling for building **spaced-repetition study
apps**: the SM-2 scheduling algorithm, a simple notes-to-flashcards workflow, testing guidance
(including property-based invariants), and a filesystem MCP server for reading study notes.

Built as the bonus lesson of the **Kiro University Challenge 2026**, extracted from the
[StudyForge](../README.md) project.

## What it provides

- **Skill `spaced-repetition`** — SM-2 as a pure, testable function; the `question :: answer`
  card-generation rule; and property-based testing invariants.
- **MCP server `notes-filesystem`** — read-only access to a `study-notes/` folder so the agent
  can turn notes into flashcards.

Follows the [Agent Plugins v1.0.0](https://agent-plugins.org/specification) specification.

## Layout

```
power/
├── plugin.json                         → Agent Plugins manifest
├── mcp.json                            → filesystem MCP server
└── skills/
    └── spaced-repetition/
        ├── SKILL.md                    → the skill (activated by keywords)
        └── references/
            └── sm2-algorithm.md        → SM-2 details and worked examples
```

## Activation

Kiro loads this power's context automatically when you mention keywords such as
**flashcards**, **spaced repetition**, **SM-2**, **Leitner**, **study**, or **Anki**.

## Install

- **From a public GitHub repo**: publish this `power/` folder as its own public repository,
  then install it in the Kiro IDE (Powers panel ⚡ → install from repository).
- **From a local path**: clone the repository and install from the local folder.

Powers can be shared as public GitHub repositories without special approval, and optionally
submitted for review to be considered for the curated Kiro powers registry.

## License

MIT
