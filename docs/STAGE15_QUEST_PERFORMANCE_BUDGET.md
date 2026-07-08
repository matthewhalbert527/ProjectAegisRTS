# Stage 15 Quest Performance Budget

## Placeholder Quest Budget

The Stage 15 Quest profile is a conservative starting point for Meta Quest 3S readiness:

- target frame rate: 72 FPS
- maximum scene GameObjects: 900
- maximum active renderers: 450
- maximum actor views: 120
- maximum projectile views: 48
- maximum feedback markers: 32
- maximum inactive pooled objects: 128
- anti-aliasing: 2x
- pixel lights: 1
- shadow distance: 18 meters
- LOD bias: 0.7
- VSync: off, with `Application.targetFrameRate` controlled by the profile

These are not final shipping targets. They are early guardrails so future stages can notice when placeholder scenes become too heavy for standalone VR/MR.

## What Stage 15 Measures

- runtime frame timing through `RuntimePerformanceStats`
- visible actor/projectile/feedback presentation counts
- object pool create/reuse/release counters
- scene object, renderer, material, light, camera, canvas, and behaviour counts
- whether the current scene fits the placeholder profile budget

## What Stage 15 Does Not Do

- no physical Quest profiling
- no APK generation
- no Meta XR package requirement
- no final render pipeline optimization
- no final LOD, occlusion, batching, or shader budget

Future Quest profiling should replace these placeholder numbers with device captures from Unity Profiler, Android GPU Inspector, OVR Metrics Tool, or an equivalent device-side capture.
