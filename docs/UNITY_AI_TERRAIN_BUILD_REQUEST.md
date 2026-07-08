# Unity AI Terrain Build Request

Last updated: 2026-07-05

## Request

Ask Unity AI, backed by Codex CLI through Unity MCP, to create a higher-quality player-facing terrain composition for ProjectAegisRTS.

## Prompt

```text
ProjectAegisRTS is a deterministic RTS prototype. Rts.Core is the gameplay authority and must remain UnityEngine-free.

Use Unity-side presentation only to improve the Stage16 player-facing battlefield terrain.

Create a quality terrain composition for the map:
- Use the imported Unity Terrain Sample Asset Pack terrain layers and foliage/detail prefabs where possible.
- Keep the heather board material as the broad ground surface.
- Keep static grid lines hidden so the ground reads smooth.
- Add visual-only terrain set dressing that forms a readable RTS map: base hardstand, road/scouting route, resource field, rocky/foliage edges, battlefield scars, and small logistics props.
- Do not use concept sheets or flat cropped image cards as final runtime terrain.
- Do not modify Rts.Core gameplay.
- Preserve PCDesktop right sidebar, QuestXR hand controls, Stage27.1 placement HUD separation, and board/sidebar safe area.
- Keep debug panels hidden by default.

Validation:
- Regenerate Stage16.
- Validate Stage16 scene.
- Capture a player-facing screenshot.
- Confirm terrain is visual-only and Player-facing Stage16 loads.
```

## Current Connection Status

Codex CLI can launch the configured `unity-mcp` server, but Unity currently returns:

```text
Connection revoked. Go to Unity Editor > Project Settings > AI > Unity MCP to change approval.
```

Until that approval is changed in Unity, this terrain pass is implemented through local Unity editor code and can be refined by Unity AI later using the prompt above.

## Implemented Local Equivalent

- `Stage32TerrainPieceGenerator.ConfigureStage16SetDressing` now runs Terrain Sample ground tile integration when the package is installed.
- The Stage32 player-facing terrain profile now renders the full 75-piece terrain composition.
- Stage16 keeps the heather board surface and hidden static grid lines.
