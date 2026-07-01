# Stage 16.5 Player Build Guide

## Configure

From Unity:

```text
ProjectAegisRTS > Stage 16.5 > Configure Player Build Flow
```

From PowerShell, run the check script:

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS"
.\tools\run-stage16-player-build-checks.ps1 -SkipPlayerBuild
```

Close the Unity editor before running batchmode validation or player builds. If Unity is open on this project, batchmode cannot own the project lock.

## Build Windows Player

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS"
.\tools\build-windows-player-stage16.ps1
```

Output:

```text
E:\OpenRA Mod\ProjectAegisRTS\build\windows-player-stage16\ProjectAegisRTS.exe
```

Launch:

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS\build\windows-player-stage16"
.\ProjectAegisRTS.exe
```

## Open In Unity

Open first:

```text
Assets/Rts/Scenes/Stage16_5_Boot.unity
```

Use the menu button to start the vertical slice. To inspect the gameplay scene directly, open:

```text
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

## Controls

- Left click: select units/buildings.
- Right click: issue movement or contextual command.
- F1 or H: toggle the Stage 17 in-match controls overlay.
- Space: pause/resume.
- Period or N: single-step.
- Escape: cancel placement or clear selection.
- B: enter placement for a completed building.
- O: toggle objective HUD.
- C: toggle the Stage 18 build-order checklist.
- P: toggle the Stage 18 next-step prompt.
- Y, Backquote, F3-F12: developer/debug HUD toggles.

Debug panels are hidden by default in the player-facing flow. Existing hotkeys still toggle them for development.

## Stage 17 Player-Facing Polish

Stage 17 keeps the same EXE path and boot scene but adds:

- Options screen placeholder.
- Player objective/status HUD.
- Prompt HUD.
- Hidden-by-default controls overlay.
- Win/loss result screen with Restart Scenario, Return to Main Menu, and Quit.
- Player-facing validation and log inspection scripts.

Use:

```powershell
.\tools\run-stage17-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
```

Player logs are normally written to:

```text
%USERPROFILE%\AppData\LocalLow\DefaultCompany\ProjectAegisRTS\Player.log
```

## Stage 18 Tester Playability

Stage 18 keeps the same EXE path and scenes but adds a visible build-order checklist, next-step prompt system, clearer sidebar card states, brighter player-build camera/fog defaults, and scaled HUD layout for high-resolution displays.

Use:

```powershell
.\tools\run-stage18-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
```
