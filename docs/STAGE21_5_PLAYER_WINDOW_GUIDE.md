# Stage 21.5 Player Window Guide

The Windows player is built at:

```text
build\windows-player-stage16\ProjectAegisRTS.exe
```

## Build

```powershell
.\tools\build-windows-player-stage16.ps1
```

Optional build defaults:

```powershell
.\tools\build-windows-player-stage16.ps1 -WindowWidth 1920 -WindowHeight 1080 -Windowed
.\tools\build-windows-player-stage16.ps1 -Fullscreen
```

## Manual Tests

Normal launch:

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

1920x1080 windowed helper:

```powershell
.\tools\run-player-windowed-1080p.ps1
```

Fullscreen launch:

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe -screen-fullscreen 1
```

## Checklist

- Window is usable, not tiny.
- Boot menu is readable.
- Options menu display section is visible.
- Reset Display Settings returns to 1600x900 windowed defaults.
- Start Vertical Slice works.
- Board and fine grid are visible.
- PCDesktop right sidebar and minimap are visible.
- Debug panels are hidden by default.
- Placement UI is hidden until placement mode.
- Player.log contains Stage 21.5 display startup diagnostics.
