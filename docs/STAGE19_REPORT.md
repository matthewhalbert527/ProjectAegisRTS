# Stage 19 Report

## Summary

Stage 19 tunes the Stage 16/18.5 vertical slice into a short player-facing prototype mission. The same Boot scene and Stage 16 gameplay scene remain in use; no campaign, multiplayer, replay, final art, or Quest-device packaging scope was added.

## Implemented

- Added `VerticalSliceMissionFlowController` as a Unity-only guidance layer.
- Expanded `VerticalSliceProgressTracker` to track 15 mission beats from snapshots, local selection, production queue state, economy events, combat events, and match outcome.
- Updated checklist, prompt, sidebar, production cards, queue labels, and placement panel copy for current-step guidance and fine-grid placement clarity.
- Tuned the vertical-slice world with a closer ore field, less immediate enemy contact, and a small starter combat group.
- Added a deterministic `VerticalSliceCanReachVictoryWithNormalCombatPath` test using normal move and attack commands.
- Added Stage 19 scene and Play Mode smoke validators.
- Added Stage 19 fast, medium, player-facing, and full validation scripts.
- Updated Player.log inspection to preserve `debug-logs/latest-player.log`.

## Player-Facing Flow

The mission guidance walks through:

1. Welcome and camera overview.
2. Select Fabrication Hub.
3. Build Power Plant.
4. Place Power Plant on the fine grid.
5. Build Refinery.
6. Place Refinery near resources.
7. Observe harvester loop.
8. Build Barracks.
9. Train infantry.
10. Build War Factory.
11. Produce a light tank.
12. Scout toward enemy base.
13. Attack enemy forces or base.
14. Destroy enemy base.
15. Victory screen.

## Fine-Grid Guidance

Placement UI now uses the player-facing copy:

- Buildings snap to the fine placement grid.
- Green footprint is valid; red footprint is blocked.

The board keeps coarse grid boundaries emphasized while fine lines are lighter. Placement previews still show the exact fine footprint.

## Validation

Run from the repo root:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
.\tools\run-stage19-fast-checks.ps1
.\tools\run-stage19-medium-checks.ps1
.\tools\run-stage19-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
git diff --check
.\tools\inspect-latest-player-log.ps1
```

## Known Limitations

- Mission beat detection is best effort when a beat is already partly true at scene start.
- The build order still runs on placeholder art and prototype UI.
- Enemy pacing is deterministic and simple; advanced AI remains future scope.
- Fine placement only applies to buildings. Movement, attack, harvest, and selection remain coarse-cell commands.

## Next Recommendation

Stage 20 should start the 360-degree production model replacement pipeline: naming rules, import scale, pivots/sockets, material conventions, animation clips, VFX hooks, and model-replacement validation.
