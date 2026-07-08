# Stage 28.1 Full Gate And Layout Report

Stage 28.1 fixes two player-facing problems without starting Stage 29: the full Stage 28 acceptance gate was replaying older full gates recursively, and the PCDesktop board camera could render underneath the right-side CnC/OpenRA sidebar.

## Root Causes

- Full validation timeout: `tools\run-stage28-checks.ps1` called earlier full gates, which called their own earlier full gates. A 30-minute run was still progressing through repeated lower-stage validation/build work and did not report a validation failure before timeout.
- PC layout overlap: the right sidebar is a screen-space overlay, but the world camera rendered the board full-screen. At 1600x900 this allowed the playable board to sit behind the sidebar.
- Left HUD pressure: the objective/checklist/prompt stack reserves the left side, but the camera had no matching safe area for the player-facing PC layout.

## Strategy

- Keep `tools\run-stage28-checks.ps1` as the Stage 0-through-Stage 28 final gate, but flatten it so it calls direct validation dependencies once instead of recursively replaying old full gates.
- Add `tools\audit-full-validation-recursion.ps1` to fail if Stage 28 or Stage 28.1 reintroduces recursive full-gate calls.
- Add Stage 28.1 fast, medium, player-facing, Unity-only, and full scripts so layout/tooling fixes can be validated without starting Stage 29.
- Add `PcGameplaySafeAreaController` and `PlayerFacingCameraFramer` so PCDesktop and DebugHybrid reserve the left objective stack and right sidebar before framing the board.
- Keep QuestXR full-screen camera behavior unchanged.
- Preserve deterministic `Rts.Core` authority. The additional diagonal movement fix remains UnityEngine-free and uses integer fixed-position stepping.

## Player-Facing Layout Result

At common Windows player sizes, the gameplay camera now uses a normalized camera rect inside the PC safe area:

- 1280x720
- 1600x900
- 1920x1080
- 2560x1440

The safe area reserves the right sidebar and left objective/checklist column, then the camera frames the board inside the remaining center area. The right sidebar, minimap, production stack, placement panel, selection panel, and command buttons remain screen-space UI.

## Diagonal Movement Result

Pathfinding already allowed eight-way paths and blocked corner cutting. Stage 28.1 changes the simulation tick movement from independent X/Y clamping to deterministic integer vector stepping toward the next path cell. Units now advance visibly along diagonal path steps without gaining extra diagonal speed.

## Validation Expectations

Fast iteration:

```powershell
.\tools\run-stage28-1-fast-checks.ps1
```

Before commit:

```powershell
.\tools\run-stage28-1-medium-checks.ps1
```

Player-facing confidence:

```powershell
.\tools\run-stage28-1-player-facing-checks.ps1 -SkipPlayerBuild
```

Full acceptance:

```powershell
.\tools\run-stage28-1-checks.ps1
```

The full gate still performs final acceptance coverage. The change is that repeated lower-stage full gates are no longer nested inside each other.
