# Stage 15 Build Readiness

## Purpose

Stage 15 creates an audit layer for Quest and PC build readiness without requiring a physical headset, Meta packages, Android modules, or large generated build artifacts.

## Quest Readiness Audit

The Quest readiness reporter checks:

- the Quest performance budget profile exists
- runtime stats can refresh
- scene complexity can refresh
- current runtime and scene counts fit the placeholder Quest profile
- quality settings can be applied from the Quest profile
- the Stage 15 scene is generated and included in build settings

Android Build Support, OpenJDK, SDK, and NDK availability are intentionally advisory in this stage. Missing modules should not block Stage 15 because the goal is foundation and auditability, not producing an APK.

## PC Readiness Audit

The PC readiness reporter checks:

- the PC performance budget profile exists
- runtime stats can refresh
- scene complexity can refresh
- current runtime and scene counts fit the placeholder PC profile
- the Stage 15 scene is generated and included in build settings

The Stage 15 audit does not keep a generated Windows player in the repository.

## Validation Command

```powershell
.\tools\run-unity-stage15-validation.ps1
```

This command runs the Stage 15 profile generation, scene creation, scene validation, play-mode-style smoke validation, build-readiness audit, and generated YAML whitespace normalization.
