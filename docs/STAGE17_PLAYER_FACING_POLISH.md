# Stage 17 Player-Facing Polish

Stage 17 keeps the Stage 16 vertical slice prototype but makes it easier for a tester to understand and exercise.

## Player Flow

The Windows player starts in `Assets/Rts/Scenes/Stage16_5_Boot.unity`.

The boot menu now exposes:

- Start Vertical Slice
- Controls
- Options
- Quit

The menu labels the build as `ProjectAegisRTS`, `Development Prototype`, and `Vertical Slice Build`.

## Options

`OptionsMenuHud` is a safe prototype placeholder. It stores local preferences with `PlayerPrefs` for:

- fullscreen preference
- master volume
- show debug panels by default

It also displays whether developer/debug hotkeys are enabled. This is not final settings infrastructure.

## In-Match HUD

Stage 17 adds player-facing HUD components:

- `PlayerObjectiveHud`: primary objective, credits, power, selected actors, production state, enemy base status, and objective state.
- `PlayerPromptHud`: first-time guidance and next-step prompts.
- `PlayerControlsOverlay`: hidden by default, toggled with `F1` or `H`.
- `MatchResultHud`: victory/defeat/draw screen with Restart Scenario, Return to Main Menu, and Quit.

Debug/status panels remain available for development but are hidden by default in the player-facing flow.

The Windows player uses a closer, centered orthographic camera with brighter lighting, lighter fog, and corrected desktop HUD anchors so the board, sidebar, and command bar are readable in the EXE.

## Win And Loss

Win/loss still comes from deterministic `Rts.Core` match state. Unity does not mutate internal gameplay state directly.

- Victory: enemy fabrication hub destroyed.
- Defeat: player fabrication hub destroyed.
- Draw: both victory and defeat conditions resolve on the same deterministic update.

Validation uses existing safe scenario actions to trigger victory and defeat paths.

## Manual Test

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS"
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

Verify:

- Boot menu appears.
- Controls screen opens and returns.
- Options screen opens and returns.
- Start Vertical Slice loads the board.
- Board, buildings, actors, resources, objective HUD, and prompt HUD are visible.
- Debug panels are hidden by default.
- Placement UI is hidden until placement mode is active.
- `F1` or `H` opens the controls overlay.
- Win/loss screen appears after the scenario resolves.
- Restart and Return to Main Menu work.

## Player.log

The Windows player log is usually under:

```text
%USERPROFILE%\AppData\LocalLow\DefaultCompany\ProjectAegisRTS\Player.log
```

Stage 17 player-facing validation scans Player.log when a player build is produced and a matching log exists.

## Stage 18 Follow-Up

Stage 18 builds on this player-facing flow with a checklist/progress tracker, next-step prompt system, sidebar readability pass, brighter EXE view defaults, and stricter validation that debug/status panels stay hidden by default. See `docs/STAGE18_REPORT.md` and `docs/STAGE18_TESTER_PLAYABILITY_GUIDE.md`.
