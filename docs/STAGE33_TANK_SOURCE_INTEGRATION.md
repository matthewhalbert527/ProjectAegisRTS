# Stage 33 Tank Source Integration Overlay

This overlay adds a Unity source generator for first-pass tank production-source prefabs.

It is intended for:

- `light_tank`
- `medium_tank`
- `heavy_tank`

The generated prefabs are not final art. They are structured, socketed, Quest-safe tank source/proxy prefabs that can be cleanly replaced by artist-authored models later.

## Key requirements preserved

- Rts.Core remains authoritative and UnityEngine-free.
- Tank visuals are Unity presentation only.
- Pivot is at the footprint/base center.
- Sockets are named for existing art/animation/combat pipelines.
- Turret, barrel, muzzle, track, smoke, explosion, selection, and UI anchors are separate transforms.
- Visual movement is handled by `TankVisualRigController` and does not affect deterministic gameplay.
- Compatible `ActorVisualDefinition` assets point at the tank source prefab as the production prefab while generated blockout prefabs remain safe fallbacks.
- The PCDesktop right sidebar, board/sidebar safe area, QuestXR Stage4/Stage5 hand controls, and Stage27.1 placement HUD separation are preserved by existing validation gates.

## Generated paths

The generator creates:

```text
Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/light_tank_tank_source.prefab
Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/medium_tank_tank_source.prefab
Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/heavy_tank_tank_source.prefab
Assets/Rts/Scenes/Stage33_TankSourceReview.unity
docs/STAGE33_TANK_SOURCE_REPORT.md
```

## Usage

Run in Unity:

```text
ProjectAegisRTS > Art > Generate Tank Source Prefabs
```

Or from PowerShell:

```powershell
.\tools\run-stage33-tank-source-generator.ps1
```

The script opens Unity in batch mode, runs the generator, validates the generated prefabs, normalizes Unity YAML whitespace, and writes the report. It uses the repo's Unity discovery helper, so it works with the existing `E:\Unity\Hub\Editor\...\Unity.exe` installation layout.

## Validation checklist

Use this focused check while iterating on the tank source prefabs:

```powershell
.\tools\run-stage33-tank-source-generator.ps1
```

Before committing Stage33 work, run the existing broad gates that guard prior behavior:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
.\tools\audit-full-validation-recursion.ps1
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
.\tools\run-stage32-medium-checks.ps1
```

## Artist replacement rule

When real models arrive, preserve these socket names:

```text
Root
BodyRoot
VisualRoot
SelectionAnchor
HealthBarAnchor
UiAnchor
TurretRoot
BarrelRoot
MuzzlePrimary
TrackLeft
TrackRight
VfxSmoke
VfxExplosion
```

Also preserve the root pivot at the footprint center, keep the socket transforms in local prefab space, keep `ActorPrefabDescriptor` metadata and declared sockets, keep `LODGroup` coverage, and retain the visual-only `TankVisualRigController` hook points for turret yaw, barrel pitch/recoil, and track scrolling.
