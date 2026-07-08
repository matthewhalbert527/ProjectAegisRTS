# Stage 25 Engineer / Transport Design

Stage 25 adds deterministic foundations for engineer building capture, engineer field repair, and infantry transports.

## Core Rules

- Engineers can capture enemy or neutral captureable buildings.
- Successful capture changes building ownership through `Rts.Core`; the engineer is consumed on capture.
- Engineers can field-repair owned damaged buildings with a one-shot deterministic repair action.
- APC transports can carry infantry passengers up to their capacity.
- Loaded passengers are hidden from board actor snapshots and represented through `TransportSnapshot`.
- Unload places passengers on deterministic passable cells near the requested unload point.
- If a transport is destroyed, loaded passengers are destroyed deterministically and combat events record the result.

## Unity Routing

PCDesktop command-bar modes now include `Capture`, `Eng Repair`, `Load`, and `Unload`. Quest left-hand routing exposes matching pass-through methods without changing the Stage 4/5 hand-control split.

## Validation

- `tools/run-stage25-fast-checks.ps1`
- `tools/run-stage25-medium-checks.ps1`
- `tools/run-stage25-player-facing-checks.ps1`
- `tools/run-stage25-checks.ps1`

Medium validation stays non-recursive and calls direct Stage 24, Stage 4, Stage 5, and Stage 25 dependencies.
