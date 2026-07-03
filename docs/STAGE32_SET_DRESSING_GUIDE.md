# Stage 32 Set Dressing Guide

Stage32 set dressing places a conservative number of terrain pieces around the Stage16 player-facing board. The goal is battlefield atmosphere without hiding the fine placement grid, buildings, resources, units, the right sidebar, or the objective stack.

## Runtime Components

- `TerrainSetDressingRuntimeLayer`: Stage16 entry point that owns catalog/profile references.
- `TerrainPieceRuntimeResolver`: resolves visual piece IDs to prefabs.
- `TerrainSetDressingRenderer`: instantiates visual-only pieces under `BoardRoot/Stage32 Terrain Set Dressing`.
- `TerrainSetDressingProfile`: deterministic placement list.
- `TerrainSetDressingLibrary`: default profile catalog.

## Player-Facing Profile

The default profile is `stage32_player_facing`.

It places visual pieces in these zones:

- base pads around starting buildings,
- production apron and rally/exit markings near exits,
- road/path texture along the scouting route,
- resource clusters and decals near the resource field,
- rocks, ridges, foliage, wrecks, and debris near map edges,
- small props outside the main placement and mission path.

## Rules

- Keep visual pieces low enough that fine grid lines remain readable.
- Keep resource pieces distinct but do not cover harvestable areas.
- Keep blockers and wrecks at map edges unless `Rts.Core` terrain already supports the blocker.
- Keep player-owned buildings and units visually dominant.
- Do not place any Stage32 piece over the PCDesktop sidebar/minimap or objective/checklist HUD. These are UI-safe-area concerns, not world-space decoration targets.
- Do not add colliders or input handlers to set-dressing pieces.
- Do not mutate snapshots, actor positions, map terrain, passability, fog, AI, or production state.

## Manual Checklist

Launch:

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe -screen-width 1600 -screen-height 900 -screen-fullscreen 0
```

Verify:

- Boot menu appears.
- Stage16 starts.
- Right sidebar and minimap are visible and docked right.
- Board remains inside the PC safe area.
- Terrain pieces improve the board without cluttering the mission path.
- Resources remain clear.
- Buildings and units remain visible.
- Fine grid remains readable.
- Power Plant placement works.
- Stage3 board-placement HUD does not appear during building placement.
- `Player.log` has no repeating red errors.
