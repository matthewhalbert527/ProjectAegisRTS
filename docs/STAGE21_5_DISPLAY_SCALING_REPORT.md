# Stage 21.5 Display Scaling Report

Stage 21.5 fixes the Windows player presentation issue where the EXE could open or appear at an unusably small size. The fix keeps `Rts.Core` unchanged and keeps the PCDesktop/QuestXR UI split intact.

## Root Cause

- Unity project defaults were set to 1920x1080, but `defaultIsNativeResolution` was enabled and there was no runtime display clamp.
- The Boot options menu only stored fullscreen/audio/debug preferences; it had no width/height reset path.
- The Windows build script did not configure display settings before building and only suggested a normal launch.
- `Player.log` did not record startup display metrics, so the window-size problem was hard to inspect after the fact.

## Fix

- `PlayerDisplaySettings` now owns default display values, minimum clamping, saved display preferences, reset support, and startup display logging.
- `PlayerDisplaySettingsInitializer` is attached to Boot and Stage16 startup paths.
- Default window: 1600x900.
- Minimum usable window: 1280x720.
- Preferred mode: Windowed by default, with Fullscreen Window available through options or build flags.
- Invalid or tiny saved/current resolution is clamped to the default.
- Boot Options now includes Windowed, Fullscreen Window, 1280x720, 1600x900, 1920x1080, Apply Display, and Reset Display Settings.
- Player-facing canvases use `ResponsiveCanvasScalerEnforcer` with 1920x1080 reference resolution and 0.5 width/height matching.
- Build settings keep `Stage16_5_Boot.unity` first and `Stage16_PlayableVerticalSlice.unity` second.

## Validation

Use fast checks while iterating:

```powershell
.\tools\run-stage21-5-fast-checks.ps1
```

Use medium checks before committing:

```powershell
.\tools\run-stage21-5-medium-checks.ps1
```

Use full checks before accepting the stage:

```powershell
.\tools\run-stage21-5-checks.ps1
```

Player-facing checks can skip the player build for faster iteration:

```powershell
.\tools\run-stage21-5-player-facing-checks.ps1 -SkipPlayerBuild
```

## Player.log Diagnostics

After launching the rebuilt player, run:

```powershell
.\tools\inspect-latest-player-log.ps1 -RequireDisplayStartup -CopyToDebugLogs
```

Expected display lines include `Screen.width`, `Screen.height`, `Screen.fullScreen`, `Screen.fullScreenMode`, `requestedResolution`, and `clampedDisplaySetting`.

## Known Limitations

- The Options menu remains prototype IMGUI, not final production UI.
- Runtime clamping intentionally preserves valid player display preferences.
- Stage 21.5 does not add final art, multiplayer, replay, checksum, or gameplay rule changes.
