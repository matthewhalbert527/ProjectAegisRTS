# Stage 27 Skirmish Playability Design

Stage 27 turns the Stage 16 vertical slice into a more playable skirmish prototype without adding multiplayer or full advanced AI.

## Core Boundary

`Rts.Core` remains authoritative and Unity-free. The AI now has named Easy, Normal, and Hard profiles with deterministic decision intervals, first-wave delays, wave intervals, production targets, and Hard-only building repair. Attack waves use reachable staging cells near priority targets so the planner avoids repeatedly issuing invalid attack-move commands.

Target priority is intentionally conservative for the player-facing prototype:

- defenses first,
- economy second,
- base hub last.

That gives visible pressure without turning the first attack wave into an unavoidable base rush.

## Player Flow

The Boot Options menu saves the selected skirmish difficulty to `PlayerPrefs`. Stage16 reads that preference when the simulation driver creates or restarts the vertical slice. The in-match objective HUD shows the active difficulty and the next enemy pressure timing while the AI debug timeline remains hidden by default.

## Validation

Stage 27 keeps the validation tiers flat:

- `tools/run-stage27-fast-checks.ps1`
- `tools/run-stage27-medium-checks.ps1`
- `tools/run-stage27-player-facing-checks.ps1`
- `tools/run-stage27-checks.ps1`

Medium validation calls direct Stage 26 dependencies, Stage 4/5 hand-control checks, and Stage 27 validation without invoking prior medium scripts.
