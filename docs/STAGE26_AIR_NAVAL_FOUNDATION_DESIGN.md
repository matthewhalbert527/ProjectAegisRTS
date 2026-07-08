# Stage 26 Air / Naval Foundation Design

Stage 26 adds deterministic airfield, aircraft, and naval passability foundations without adding final aircraft micro, naval production, or faction-specific presentation.

## Core Rules

- Aircraft-capable units expose `AircraftDefinition` metadata for cruise altitude, fuel, rearm ticks, and airfield requirements.
- Airfield buildings expose `AirfieldDefinition` metadata for pad count, pad offsets, and placeholder rearm timing.
- `dual_helipad` is the first two-pad airfield and can produce `attack_aircraft` and `heavy_lifter_aircraft`.
- Produced aircraft reserve a same-owner airfield pad, dock there, and expose `AircraftSnapshot` and `AirfieldSnapshot` data.
- Docked aircraft sit at zero core altitude; aircraft with active movement/attack/other orders release from the pad and move to cruise altitude.
- Fuel and rearm are deterministic placeholder state for later air combat/persistence work.
- Water terrain now allows `MovementClass.Aircraft` and `MovementClass.Naval`, while wheeled/tracked/ground units remain blocked.

## Unity Presentation

- The Stage 16 vertical slice includes a powered tech/helipad/attack-aircraft setup so the player-facing slice demonstrates air assets.
- Aircraft visuals read `AircraftSnapshot.AltitudeSubCells` through `ActorRenderSystem` and `AircraftVisualMotionController`.
- Docked aircraft sit on the helipad pad; airborne aircraft use deterministic core altitude plus existing visual hover/bank polish.
- Fallback helipad primitives draw simple pad markers when generated prefabs are unavailable.

## Validation

- `tools/run-stage26-fast-checks.ps1`
- `tools/run-stage26-medium-checks.ps1`
- `tools/run-stage26-player-facing-checks.ps1`
- `tools/run-stage26-checks.ps1`

Medium validation stays non-recursive and calls direct Stage 25, Stage 4, Stage 5, and Stage 26 dependencies.

## Deferred

- No final aircraft ammo, landing queues, refuel penalties, or air superiority balancing.
- No naval unit definitions, naval factories, ship combat, wake VFX, or water-specific player UI.
- No final aircraft/helipad animation, pad occupancy UI, or artist-authored air/naval assets beyond existing Stage 8+ placeholders.
