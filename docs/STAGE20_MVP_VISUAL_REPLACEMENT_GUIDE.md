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
8. Run `.\tools\run-stage20-fast-checks.ps1`.

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

## Artist Notes

Do not copy protected C&C/Red Alert art, names, UI art, audio, or trade dress. Use the safe internal IDs from the asset registry and keep release-facing names under IP review until final naming is approved.
