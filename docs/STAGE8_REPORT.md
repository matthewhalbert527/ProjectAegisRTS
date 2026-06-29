# Stage 8 Report

## Summary

Stage 8 creates the Unity-side art pipeline layer for replacing placeholder visuals with production prefabs. It imports concept references, generates 27 blockout prefabs, creates 27 actor visual definitions, validates required sockets, and adds `Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity` for review.

## Branch And Commits

- Branch: `codex/stage-8-art-pipeline-prefabs`
- Base Stage 7 implementation commit: `0b399dee35a6be291608b644a91f9b10f673d824`
- Stage 8 implementation commit: `7f60b7cc22fe2dbb15d437f83c26b20d0ac41102`

## Systems Created

- Runtime art catalog: `ActorVisualDefinition`, `ConceptArtReference`, definition/concept libraries, prefab descriptors, sockets, and prefab resolver.
- Generated blockout prefabs: one per current safe actor id under `Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/`.
- Concept import pipeline: copies source concept PNGs into `Assets/Rts/Art/Concepts/` and creates concept reference assets.
- Icon pipeline: creates 128x128 icon sprites under `Assets/Rts/Art/Icons/`.
- Validation pipeline: writes `Assets/Rts/Art/Validation/stage8_prefab_validation.json` and `docs/STAGE8_PREFAB_VALIDATION.md`.
- Showcase scene and HUD: F11 art pipeline HUD, generated blockout grid, concept cards, socket labels, and resolver stats.

## Architecture Boundary

Stage 8 is Unity presentation only. `Rts.Core` remains deterministic and UnityEngine-free. Actor visual definitions, concept references, prefab sockets, icons, and production-art replacement state are not written back into gameplay simulation.

## Counts

- Actor visual definitions: 27
- Concept references: 27
- Generated blockout prefabs: 27
- Generated icons: 27
- IP review flags preserved: 3 (`field_hospital`, `attack_aircraft`, `heavy_lifter_aircraft`)

## Validation

- Stage 0 tests: passed 10/10 with `dotnet run --no-restore --project src/Rts.Core.Tests`.
- Stage 1-7 validation: preserved; the full Stage 8 acceptance gate replayed the earlier validation chain and Stage 7 Play Mode smoke passed in batchmode.
- Stage 8 scene validation: passed and validated scene objects, libraries, generated assets, icons, and previous scene paths.
- Stage 8 smoke: passed in batchmode and validated runtime-equivalent bootstrap, tick advance, actor visuals, resolver coverage, prefab instantiation, descriptors, sockets, showcase, HUD, and red console errors.
- Stage 8 prefab validation: passed with 27 definitions, 27 concept references, 27 blockout prefabs, 27 icons, 27 socket-validated prefabs, 3 IP-review flags, and zero errors.
- Rts.Core UnityEngine-free check: passed with the PowerShell fallback scan.
- `git diff --check`: passed after normalizing Unity-generated whitespace.

## Manual Play Mode Checklist

Open `Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity`, press Play, and verify:

- Board and actors are visible.
- F11 toggles the art pipeline debug HUD.
- Concept reference cards appear.
- Actor visual definitions can be cycled.
- Generated blockout prefabs appear and differ by category.
- Vehicle, infantry, building, defense, and aircraft sockets are present.
- `docs/STAGE8_PREFAB_VALIDATION.md` exists.
- ActorRenderSystem resolves prefabs or gracefully falls back.
- Stage 7 scene still opens and validates.
- No repeating red console errors.

## Known Limits

- Blockouts are review placeholders, not final art.
- Stage 2/Stage 4 production cards do not consume icons yet; the icon assets and definitions are ready for that later UI pass.
- Production prefabs should be placed under `Assets/Rts/Art/Prefabs/Actors/Production/` and assigned through actor visual definitions.
- Final combat, destruction, economy harvesting loops, AI, multiplayer, and release packaging remain out of scope.

## Commands

Acceptance commands:

```powershell
dotnet run --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage1-checks.ps1
.\tools\run-stage2-checks.ps1
.\tools\run-stage2-playmode-smoke.ps1
.\tools\run-stage3-checks.ps1
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
.\tools\run-stage6-checks.ps1
.\tools\run-stage7-checks.ps1
.\tools\run-unity-stage8-validation.ps1
.\tools\run-stage8-checks.ps1
git diff --check
```

`tools/run-stage8-checks.ps1` remains the full final acceptance gate. It can take a long time because the current stage scripts recursively replay earlier stage checks; a future validation-tier pass should add faster Stage 8 iteration checks without weakening final acceptance.

## Stage 9 Recommendation

Stage 9 should build combat, weapons, projectiles, damage, death, and destruction presentation on top of deterministic command/snapshot boundaries.
