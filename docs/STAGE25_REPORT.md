# Stage 25 Report

Stage 25 is complete after local validation.

Implemented:

- `CaptureDefinition`, `CaptureableDefinition`, and `TransportDefinition` in `Rts.Core`.
- `CaptureBuildingCommand`, `EngineerRepairBuildingCommand`, `LoadTransportCommand`, and `UnloadTransportCommand`.
- Deterministic engineer capture, engineer field repair, transport load/unload, passenger snapshots, and passenger destruction on transport death.
- Engineer and APC rule metadata, plus one engineer and one APC in the vertical-slice world for player-facing validation.
- PCDesktop command-bar buttons and modes for capture, engineer repair, load, and unload.
- Selection-panel passenger/capacity readout.
- Quest left-hand compatible routing methods.
- Stage 25 Unity validators and fast/medium/player-facing/full validation scripts.

Validation target:

- `Rts.Core` tests: 96/96.
- `Rts.Core` remains UnityEngine-free.
- Medium validation remains non-recursive through Stage 25.

Known limitations:

- Transport behavior is a foundation pass: no passenger formation UI, no final loading animations, and no transport-specific art/audio.
- Capture is generic and safe-named; no protected faction names, art, audio, or UI trade dress were added.
