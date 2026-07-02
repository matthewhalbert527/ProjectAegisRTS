# Stage 20 MVP Visual Replacement Guide

Stage 20 proxies are generated placeholders with a production-facing contract. Artists can replace them with FBX/GLB/source models later, but replacements must keep the same gameplay footprint, socket names, and validation tags.

## Replacement Workflow

1. Open `Assets/Rts/Scenes/Stage20_MvpProductionVisuals.unity`.
2. Pick one MVP actor and inspect its generated proxy.
3. Replace geometry beneath the existing root/sockets or assign a new prefab as `productionPrefab`.
4. Preserve all required `ActorPrefabSocket` children.
5. Keep `ActorPrefabDescriptor.actorTypeId` matching the safe actor ID.
6. Keep `ProductionVisualValidationTag.visualTier` accurate.
7. Keep Stage 8 generated blockout assigned as fallback.
8. Run `.\tools\run-stage21-fast-checks.ps1` after Stage 20 checks pass.

## Required Components

- `ActorPrefabDescriptor`
- `ActorPrefabSocket` children for required hooks
- `ProductionVisualValidationTag`
- Compatible visual controllers such as building state, vehicle motion, infantry motion, or turret aim
- `LODGroup`

## Scale And Footprint

The gameplay footprint is authoritative. The visual base must align to the cell footprint used by Rts.Core snapshots and Unity placement previews. Upper geometry can be inset or tiered but should not imply a larger blocked area.

## Validation Commands

- Quick iteration: `.\tools\run-stage20-fast-checks.ps1`
- Pre-commit: `.\tools\run-stage20-medium-checks.ps1`
- Final acceptance: `.\tools\run-stage20-checks.ps1`
- Stage 21 replacement readiness: `.\tools\run-stage21-fast-checks.ps1`

## Stage 21 Replacement Readiness

Stage 21 adds an import scanner and stricter replacement-readiness validation. Drop candidate `.fbx`, `.glb`, `.gltf`, or `.obj` files under `unity/Assets/Rts/Art/Models/Source/MVP` using the safe actor ID in the filename, then run `.\tools\run-unity-stage21-validation.ps1 -SkipCoreBuild`.

The scanner writes `docs/STAGE21_ARTIST_MODEL_IMPORT_STATUS.md`. The MVP QA validator writes `docs/STAGE21_MVP_VISUAL_QA.md`. Keep generated proxies active until the Stage 21 QA report has zero failures, the player-facing checks pass, and the Windows Player.log is clean.

## Artist Notes

Do not copy protected C&C/Red Alert art, names, UI art, audio, or trade dress. Use the safe internal IDs from the asset registry and keep release-facing names under IP review until final naming is approved.
