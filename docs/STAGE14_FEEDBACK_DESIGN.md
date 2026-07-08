# Stage 14 Feedback Design

Stage 14 adds a Unity-side feedback layer for placeholder audio, VFX, UI, and haptic cues. It responds to deterministic snapshots and command results without adding new gameplay authority.

## Boundary

- `Rts.Core` remains unchanged and UnityEngine-free.
- Unity reads existing `WorldSnapshot`, `CombatEventSnapshot`, `EconomyEventSnapshot`, production, power, and radar data.
- Unity command methods can report their already-computed success or failure result to the feedback bus.
- Feedback events never mutate deterministic actor, resource, production, combat, visibility, or pathing state.

## Event Flow

`FeedbackEventBus` is the central Unity presentation bus. It emits `FeedbackEvent` records for selection, move, invalid command, production, building placement, low power, harvest, unload, attack, projectile impact, damage, death/destruction, and fog/radar changes.

Snapshot-derived events come from bounded core snapshots:

- combat events become attack, impact, damage, and death/destruction feedback,
- economy events become harvest and unload feedback,
- production queue changes become production feedback,
- power/radar state changes become low-power and fog/radar feedback.

Command-derived events come from `RtsSimulationDriver` after the normal command path returns a result. Failed command results are mapped to `InvalidCommand`.

## Profiles And Controllers

`FeedbackProfile` assets define placeholder marker color, scale, duration, silent audio cue behavior, and haptic placeholder settings per event type. Stage 14 generates one profile per `FeedbackEventType` under:

`unity/Assets/Rts/ScriptableObjects/Feedback/`

The controllers are intentionally lightweight:

- `AudioFeedbackController` counts silent cue requests and owns an `AudioSource`, but ships no final audio.
- `VfxFeedbackController` spawns short-lived primitive markers only.
- `UiFeedbackController` records recent feedback messages.
- `HapticFeedbackAdapter` records placeholder pulse requests without XR package dependencies.
- `FeedbackDebugHud` displays counts and safe manual pulses.

## Limits

- No final audio assets.
- No final VFX or particle-heavy systems.
- No copyrighted audio or visual assets.
- No Quest haptic package integration yet.
- No gameplay logic changes.
