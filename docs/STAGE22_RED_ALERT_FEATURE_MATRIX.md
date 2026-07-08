# Stage 22 Classic RTS Feature Matrix

Stage 22 focuses on player-facing command controls needed for a classic RTS feel while preserving the deterministic `Rts.Core` command pipeline.

| Feature | Stage 22 Status | Notes |
| --- | --- | --- |
| Stop | Implemented | Existing stop command remains authoritative and clears movement, attack, and harvest state. |
| Move | Implemented | Existing deterministic move order remains unchanged. |
| Attack actor | Implemented | Existing deterministic actor-target attack order remains unchanged. |
| Attack Move | Implemented foundation | New core command paths armed mobile units and lets them fire at in-range enemies without becoming a plain attack order. |
| Guard | Implemented foundation | New core command puts armed actors in a deterministic guard state and opportunistically fires at in-range enemies. |
| Patrol | Implemented foundation | New core command paths armed mobile units to a patrol destination and opportunistically fires at in-range enemies. Return-loop patrol behavior is reserved. |
| Scatter | Implemented foundation | New core command sends mobile units toward deterministic nearby cells when possible. |
| Deploy | Placeholder command | New deterministic placeholder clears stale order state without adding faction-specific deploy behavior yet. |
| Repair | Reserved | Still a later base-management feature. |
| Sell | Reserved | Still a later base-management feature. |
| Control groups | Implemented client-local | `Ctrl+1` through `Ctrl+9` assigns, number keys recall. |
| Double-click select type | Implemented client-local | Double-clicking an owned actor selects visible owned actors of the same type. |
| Box selection | Implemented client-local | Drag-selects owned visible actors in the PC view; Shift/Ctrl adds to selection. |
| PC sidebar command buttons | Implemented | Command bar exposes Stop, Move, Attack, Attack Move, Guard, Patrol, Scatter, Deploy, Repair, Sell, and Power. |
| QuestXR controls | Preserved | Stage 4 and Stage 5 validation remain direct dependencies in Stage 22 medium. |

## Intentional Limits

Attack-move, guard, patrol, scatter, and deploy are deterministic command foundations. They establish command semantics, selection flow, UI affordances, and validation coverage without adding advanced tactical AI or faction-specific transform behavior.
