# Stage 22 Command Interaction Design

Stage 22 adds a classic RTS command matrix to the PCDesktop flow without moving gameplay authority into Unity.

## PC Input

- Left-click selects owned actors.
- Double-click selects visible owned actors of the same type.
- Drag-select creates a selection marquee using actor screen positions.
- Shift or Ctrl during drag adds actors to the current selection.
- Right-click issues a move order through the existing router.
- `Ctrl+1` through `Ctrl+9` assigns client-local control groups.
- `1` through `9` recalls client-local control groups.
- `S`, `M`, `A`, and `X` map to Stop, Move, Attack, and Attack Move through the command bar.

## Command Bar

The PC command bar is part of the right-sidebar layout in PCDesktop mode. It uses a compact four-column matrix so the required buttons fit inside the sidebar:

- Stop
- Move
- Attack
- Attack Move
- Guard
- Patrol
- Scatter
- Deploy
- Repair
- Sell
- Power

Repair and Sell remain reserved commands. Deploy is a deterministic placeholder in `Rts.Core`.

## Core Authority

Unity sends all gameplay-affecting commands through `RtsSimulationDriver` and `RtsCommandAdapter`. `Rts.Core` owns validation and state mutation. Selection, box selection, double-click selection, and control groups remain client-local.

## Deterministic Command Foundations

Attack Move validates owned armed mobile units, sets a distinct `AttackMove` order, paths like move, and fires opportunistically at deterministic in-range targets.

Guard validates armed owned actors, holds position, and fires opportunistically at deterministic in-range targets.

Patrol validates owned armed mobile units and paths to the clicked destination. Full back-and-forth patrol looping is reserved.

Scatter validates owned mobile units and selects deterministic nearby cells based on actor id.

Deploy validates owned actors and records a placeholder `Deploy` order without implementing transform or unpack behavior.
