# Stage 20 Report: MVP Production Visuals

Stage 20 adds the first player-facing visual replacement layer for the MVP actors without changing authoritative gameplay. The new assets are still proxy art, but they are built as true 3D tabletop miniatures instead of plain blockouts so the Quest/MR player can view them from top, front, sides, rear, and roof angles.

## Scope

- Added Stage 20 production visual standards and validation marker types.
- Added generated MVP production proxy prefabs under `unity/Assets/Rts/Art/Prefabs/Actors/ProductionProxies/`.
- Updated MVP `ActorVisualDefinition` assets during validation/generation to prefer production proxy prefabs while preserving Stage 8 blockout fallback.
- Added `Stage20_MvpProductionVisuals.unity` showcase generation and validation.
- Added explicit `PCDesktop`, `QuestXR`, and `DebugHybrid` UI modes while keeping Stage 19.5 compatibility fields.
- Added fast, medium, player-facing, Unity-only, and full Stage 20 validation scripts.

## MVP Proxy Set

- `fabrication_hub`
- `power_plant`
- `refinery`
- `barracks`
- `war_factory`
- `gun_tower`
- `rifle_infantry`
- `light_tank`
- `harvester`

## Platform UI Split

`PCDesktop` is the Windows player default. It shows the right-side C&C/OpenRA-style sidebar, minimap above production, Esc pause menu, and hides Quest fallback hand UI by default.

`QuestXR` preserves the left-hand build/selection flow and the right-hand tactical command flow, and hides the PC sidebar by default.

`DebugHybrid` remains an explicit editor/development mode for testing both UI systems together.

## Validation Tiers

- Iteration: `.\tools\run-stage20-fast-checks.ps1`
- Before commit: `.\tools\run-stage20-medium-checks.ps1`
- Player-facing confidence: `.\tools\run-stage20-player-facing-checks.ps1 -SkipPlayerBuild`
- Final acceptance: `.\tools\run-stage20-checks.ps1`

The medium tier is intentionally non-recursive and is covered by `.\tools\audit-medium-validation-recursion.ps1`.

## Known Limitations

- These are generated Unity primitive proxies, not final artist-authored meshes.
- The refinery uses the new appended `DockPumpRoot` socket for pump/dock readability.
- The Stage 20 Windows player still uses the Stage 16 boot and vertical slice scenes; the Stage 20 scene is a visual showcase, not a replacement game scene.

## Stage 21 Recommendation

Use Stage 21 to start replacing the generated proxies with artist-authored source models one actor at a time, keeping the Stage 20 sockets, footprint envelopes, and platform UI validation as the acceptance gate.
