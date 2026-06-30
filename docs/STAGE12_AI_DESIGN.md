# Stage 12 AI Design

Stage 12 adds a deterministic skirmish AI foundation to `Rts.Core`. The AI is not competitive yet; it is a small, inspectable loop that proves the project can generate gameplay commands from core world state without moving authority into Unity.

## Core Model

- `AiPlayerDefinition` registers an AI-controlled player.
- `AiDifficultyDefinition` defines deterministic interval, production cap, attack squad size, and seed.
- `AiPlanState` stores decision sequence, next decision tick, invalid command count, current plan, and recent intents.
- `AiIntent` records economy, production, attack, scouting, and defense decisions.
- `AiSystem` ticks registered AI players from inside `RtsWorld.Tick`.
- `AiCommandPlanner` submits real `ISimCommand` instances through `RtsWorld.IssueCommand`.

## First Functional Loop

Every decision interval, the Stage 12 planner:

- assigns an idle harvester to the nearest non-depleted resource cell when possible,
- places completed AI buildings at deterministic offsets from the fabrication hub,
- queues basic production if credits and producers allow,
- issues a basic attack-wave order when a small armed squad has an enemy in range,
- records scouting and defense placeholder intents for later planners.

The AI uses sorted actor/resource lists and fixed intervals. There is no nondeterministic randomness, Unity physics, machine learning, or Unity-side gameplay mutation.

## Snapshot Boundary

`WorldSnapshot.Ai` exposes `AiSnapshot`, `AiPlayerSnapshot`, and `AiIntentSnapshot` for Unity HUDs and validation. Unity reads this data for debug presentation only.

## Known Limits

- Building placement uses fixed candidate offsets rather than a base layout search.
- Attack waves require targets already in weapon range.
- Scouting and defense are explicit placeholder intents.
- The AI does not yet manage full build orders, tech progression, threat scoring, or long-range movement.
