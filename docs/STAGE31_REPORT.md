# Stage 31 Report

Stage 31 is an artist handoff and package-cleanup checkpoint for the Stage 29-30 visual pass. It does not add gameplay, imported artist models, final VFX/audio, Quest device work, or changes to deterministic `Rts.Core`.

## Implemented

- Added an artist handoff package with export/modeling briefs, material naming rules, trim-sheet guidance, LOD targets, Quest budgets, and screenshot/reference paths.
- Added updated MVP replacement guidance that preserves the Stage 20/21 socket, pivot, fallback, LOD, and metadata contract while accounting for the Stage 29 realistic material pass and Stage 30 readability overlays.
- Added a per-actor production checklist for the nine MVP actors.
- Added Stage 31 fast, medium, player-facing, and full validation scripts plus a docs/package validator.
- Updated validation-tier docs and recursion audits so Stage 31 remains non-recursive at the medium tier and delegates only to the flattened Stage 30 gate for full acceptance.

## Validation Tiers

- Fast iteration: `.\tools\run-stage31-fast-checks.ps1`
- Pre-commit: `.\tools\run-stage31-medium-checks.ps1`
- Player-facing/log preservation: `.\tools\run-stage31-player-facing-checks.ps1`
- Full acceptance: `.\tools\run-stage31-checks.ps1`

## Non-Goals

- No final artist-authored FBX/GLB models were imported.
- No proxy was replaced with a real production model.
- No gameplay, pathing, production, AI, combat, UI mode, or `Rts.Core` behavior changed.

## Stage 32 Follow-Up

Stage 32 adds generated terrain-piece prefabs, catalogs, and visual-only set dressing. Future artist handoff work should treat those pieces like environment modules: preserve the catalog IDs, material roles, Quest budgets, footprint hints, and visual-only boundary while replacing generated primitives with authored terrain art.
