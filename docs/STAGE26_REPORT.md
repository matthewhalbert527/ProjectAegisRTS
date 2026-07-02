# Stage 26 Report

Stage 26 is complete after local validation.

Implemented:

- `AircraftDefinition`, `AirfieldDefinition`, aircraft state, airfield pad state, and air/naval snapshots in `Rts.Core`.
- Dual-helipad two-pad metadata and aircraft metadata for attack and heavy-lifter aircraft.
- Deterministic aircraft docking, placeholder rearm/fuel state, pad release on orders, and altitude state.
- `MovementClass.Naval` plus water terrain passability for aircraft/naval movement while preserving ground blocking.
- Stage 16 vertical-slice tech, advanced power, dual helipad, and attack aircraft setup.
- Unity aircraft visual altitude integration from core `AircraftSnapshot` data.
- Fallback helipad pad markers for generated primitive visuals.
- Stage 26 Unity validators and fast/medium/player-facing/full validation scripts.

Validation target:

- `Rts.Core` tests: 103/103.
- Stage 26 Unity validation verifies airfield snapshots, aircraft docking, airborne movement, visual altitude, and hidden debug defaults.
- `Rts.Core` remains UnityEngine-free.
- Medium validation remains non-recursive through Stage 26.

Known limitations:

- Air behavior is a foundation pass: fuel/rearm are deterministic placeholders and do not yet constrain sorties.
- Naval support is passability/pathing foundation only; no naval actors or production chain are introduced.
- Aircraft docking uses simple first-free pad reservation without final landing animation or pad queue UX.
