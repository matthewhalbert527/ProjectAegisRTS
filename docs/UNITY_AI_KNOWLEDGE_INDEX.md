# Unity AI Knowledge Index

Last updated: 2026-07-05

This document indexes the Unity AI material installed with `com.unity.ai.assistant@2.13.0-pre.2` and maps it to ProjectAegisRTS work. It intentionally links to local package docs and official URLs instead of copying large third-party documentation into this repo.

## Installed Documentation Root

```text
unity\Library\PackageCache\com.unity.ai.assistant@6fee27370e6a\Documentation~
```

If Unity refreshes the package cache, the hash suffix can change. Use this command to find the current root:

```powershell
Get-ChildItem "unity\Library\PackageCache" -Directory | Where-Object Name -like "com.unity.ai.assistant*"
```

## High-Value Unity AI Docs

| Topic | Local doc |
| --- | --- |
| Package start page | `index.md` |
| Table of contents | `TableOfContents.md` |
| Install Assistant | `install\install-chat.md` |
| Work with Assistant | `install\getting-started.md` |
| Assistant modes | `about\assistant-modes.md` |
| Plan mode | `about\assistant-plan-mode.md` |
| Best practices | `best-practice\best-practice-assistant.md` |
| Prompt guidelines | `best-practice\prompts.md` |
| Model picker | `best-practice\model-picker.md` |
| MCP overview in Assistant | `integration\mcp-overview.md` |
| Configure MCP servers | `integration\mcp-configure.md` |
| Unity MCP overview | `integration\unity-mcp-overview.md` |
| Get started with Unity MCP | `integration\unity-mcp-get-started.md` |
| Register custom Unity MCP tools | `integration\unity-mcp-tool-registration.md` |
| AI Gateway overview | `integration\ai-gateway-intro.md` |
| AI Gateway setup | `integration\ai-gateway-get-started.md` |
| Use third-party agents | `integration\use-third-party-agents.md` |
| Assistant API | `integration\assistant-api.md` |
| Custom agent tools | `integration\custom-agent-tool.md` |
| Skills overview | `skills\skills-overview.md` |
| Skills filesystem | `skills\skills-filesystem.md` |
| Test skills | `skills\skills-test.md` |
| MCP troubleshooting | `troubleshoot\mcp-troubleshooting.md` |
| Unity MCP troubleshooting | `troubleshoot\unity-mcp-troubleshooting.md` |
| MCP panel reference | `reference\mcp-panel.md` |
| Unity MCP reference | `reference\unity-mcp-reference.md` |

## Asset Generation Docs Relevant To This RTS

| Project need | Local doc |
| --- | --- |
| Terrain layers | `3d-terrain\terrain-overview.md` |
| 3D object generation | `3d-terrain\3d-generator-overview.md` |
| Texture generation | `Texture\manage-texture2d.md` |
| Materials | `Material\material-manage.md` |
| Sprites/icons | `Sprite\manage-sprite.md` |
| Animation clips | `Animation\animation-create.md` |
| Sound effects | `Sound\sound-manage.md` |
| Reference images | `Texture\reference-image.md`, `Sprite\reference-image.md`, `Cubemap\reference-image.md` |
| Negative prompts | `Sprite\Reference\negative-prompt.md` |
| Custom seeds | `Sprite\Reference\custom-seed.md` |

## ProjectAegisRTS Usage Rules

- Use Unity AI for editor assistance, scene inspection, asset drafts, texture/material exploration, and non-authoritative presentation work.
- Do not use Unity AI outputs as deterministic simulation authority.
- Keep `src\Rts.Core` UnityEngine-free and deterministic.
- Generated or AI-assisted art must still pass the existing visual replacement and player-facing validation rules.
- Do not reintroduce flat concept-sheet cards as production terrain.
- AI-generated code changes must go through the same Stage validation scripts as hand-written code.
- Keep QuestXR controls, PCDesktop sidebar, Stage27.1 placement HUD separation, and board/sidebar safe area intact.

## Recommended Prompt Pattern

Use this structure when asking Unity AI or Codex to modify the project:

```text
Context:
ProjectAegisRTS is a Unity presentation layer over a deterministic Rts.Core simulation.

Goal:
Describe the player-facing outcome.

Constraints:
Do not modify Rts.Core unless explicitly required. Keep Rts.Core UnityEngine-free. Preserve PCDesktop sidebar, QuestXR controls, Stage27.1 placement HUD separation, and board/sidebar safe area.

Validation:
List the exact stage scripts to run, plus Player.log inspection.
```

## Official Links

- Unity AI package docs: `https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.13/manual/index.html`
- Unity AI feature overview: `https://unity.com/features/ai`
- Codex CLI docs: `https://developers.openai.com/codex/cli`
- Codex MCP docs: `https://developers.openai.com/codex/mcp`
