# Stage 17 Report

## Scope

Stage 17 polishes the Stage 16.5 player-facing vertical slice without changing gameplay authority. It adds clearer boot/menu flow, controls/help, prototype options, in-match objective and prompt HUDs, a match result screen, player-facing validation, log inspection, and fast/medium/full validation tiers.

## Implementation

- Added `OptionsMenuHud` to the boot flow.
- Updated `MainMenuHud`, `ControlsHelpHud`, and `GameBootController` for Start, Controls, Options, and Quit flow.
- Added `PlayerObjectiveHud`, `PlayerPromptHud`, `PlayerControlsOverlay`, and `MatchResultHud`.
- Wired player HUDs through `RtsGameBootstrapper`, `PlayerBuildSceneInitializer`, `Stage16SceneCreator`, and `Stage16_5BuildFlowConfigurator`.
- Extended `DebugHudVisibilityController` with validation hooks for debug panels, placement UI, and player HUD visibility.
- Extended Stage 16 and Stage 16.5 validators/smoke checks to prove player-facing defaults and win/loss result display.
- Added Stage 17 editor creator, scene validator, and play-mode smoke validator.
- Added Stage 17 fast, medium, player-facing, Unity validation, and full acceptance scripts.
- Extended the medium recursion audit to cover Stage 17.
- After EXE testing showed a dim, cramped view, corrected player-build camera framing, scene lighting, fog opacity, board/grid contrast, desktop HUD anchoring, and default status-log visibility.

## Validation Tiers

```powershell
.\tools\run-stage17-fast-checks.ps1
.\tools\run-stage17-medium-checks.ps1
.\tools\run-stage17-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage17-checks.ps1
```

Fast is for iteration. Medium is for pre-commit confidence and does not call earlier medium scripts. Full is the slow Stage 0-through-Stage 17 acceptance gate.

## Acceptance Commands

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
.\tools\run-stage17-fast-checks.ps1
.\tools\run-stage17-medium-checks.ps1
.\tools\run-stage17-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
git diff --check
```

## Latest Validation

- `.\tools\run-stage17-fast-checks.ps1`: passed.
- `.\tools\run-stage17-medium-checks.ps1`: passed.
- `.\tools\run-stage17-player-facing-checks.ps1`: passed, including Windows player build and Unity/Player log inspection.
- `dotnet run --no-restore --project src/Rts.Core.Tests`: passed 65/65 through the Stage 17 player-facing runner.
- Rts.Core UnityEngine-free scan: passed.
- Windows player: `E:\OpenRA Mod\ProjectAegisRTS\build\windows-player-stage16\ProjectAegisRTS.exe`.
- Player log inspected: `C:\Users\matth\AppData\LocalLow\ProjectAegisRTS\ProjectAegisRTS\Player.log`.

## Known Limitations

- Options are prototype placeholders.
- HUDs use simple IMGUI/uGUI-era presentation, not final UI art.
- No final tutorial, campaign, multiplayer, replay, Quest device build, or final production art is added in this stage.
- Player.log exists only after the player executable has been launched on the machine.

## Stage 18 Recommendation

Stage 18 should focus on tester-guided playability: a short scripted tutorial/checklist, clearer build-order affordances, stronger sidebar readability, and a repeatable scenario completion path without relying on debug actions.
