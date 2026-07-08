# Stage 15 Report: Quest Performance / Build Readiness Foundation

## Summary

Stage 15 adds a Unity-only performance and build-readiness foundation on top of the Stage 14 feedback scene. It introduces placeholder Quest and PC performance budgets, runtime render/stat tracking, object pooling for short-lived projectile and feedback marker visuals, scene complexity reporting, quality profile application, and build-readiness reporters.

No physical Quest, Meta XR package, APK build, store submission, final optimization pass, or Rts.Core gameplay change is required for this stage.

## Unity Additions

- `ObjectPoolService` leases and releases short-lived Unity GameObjects without changing deterministic simulation state.
- `ProjectileRenderSystem` can return expired projectile views to the pool.
- `VfxFeedbackController` can spawn feedback markers from the pool and return them through `FeedbackVisualMarker`.
- `RuntimePerformanceStats` records frame timing, snapshot actor counts, projectile counts, feedback marker counts, and pool counters.
- `SceneComplexityReporter` counts scene objects, renderers, materials, lights, cameras, canvases, and behaviours.
- `PerformanceBudgetProfile` and `PerformanceBudgetLibrary` define placeholder Quest and PC budgets.
- `QualityProfileApplier` applies conservative runtime quality settings from a budget profile.
- `QuestBuildReadinessReporter` and `PcBuildReadinessReporter` summarize budget and build-readiness state without creating build artifacts.
- `RenderStatsHud` shows Stage 15 performance stats in the Unity scene.

## Scene

- `Assets/Rts/Scenes/Stage15_PerformanceBuildReadiness.unity`

The scene is generated from Stage 14 so it preserves the feedback stack, then adds the Stage 15 pool, budgets, stats, quality, readiness reporters, and HUD.

## Validation

Stage 15 validation creates/updates performance budget profiles, creates the scene, validates scene wiring, exercises pooled projectile/feedback visuals, refreshes runtime stats and scene complexity, runs the build-readiness audit, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`.

Use:

```powershell
.\tools\run-stage15-fast-checks.ps1
.\tools\run-stage15-medium-checks.ps1
.\tools\run-stage15-checks.ps1
```

Stage 15.1 keeps the fast/medium/full split explicit:

- fast: current Stage 15 iteration only,
- medium: `Rts.Core` tests once, one Unity DLL build, direct Stage 14 Unity validation, direct Stage 15 Unity validation, UnityEngine-free scan, and `git diff --check`,
- full: Stage 0 through Stage 15 final acceptance.

The Stage 15 medium script must not call `run-stage14-medium-checks.ps1`; it calls `run-unity-stage14-validation.ps1 -SkipCoreBuild` instead. Windows CRLF conversion warnings are not failures when `git diff --check` passes.

Stage 15.1 required a corrective hardening pass after runtime output still showed prior medium sections. `tools\audit-medium-validation-recursion.ps1` now scans Stage 9 through Stage 15 medium scripts and fails if a medium script calls another medium script or uses the old medium-dependency wording. Stage 15 medium and Stage 15 full run this audit before continuing.

The pushed Stage 15 checkpoint is `codex/overnight-stage10-stage15` at `04c6c768bd6cdda74c6593a7d046de62ac27a39b`. Stage 15.1 tooling cleanup is on `codex/stage-15-1-validation-flattening`.

## Limitations

- Quest budgets are placeholders until device profiling exists.
- Android/Quest module detection is advisory; Stage 15 does not require Android Build Support.
- The PC build audit validates configuration and scene inclusion but does not create a Windows player.
- Pooling is intentionally limited to short-lived projectile and feedback marker presentation objects.
