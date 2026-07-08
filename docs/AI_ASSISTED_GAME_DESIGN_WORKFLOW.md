# AI-Assisted Game Design Workflow

Last updated: 2026-07-05

This workflow keeps AI useful for ProjectAegisRTS without letting it blur gameplay authority, platform UX, or art-quality acceptance.

## What AI Should Help With

- Scene inspection and hierarchy summaries.
- Unity console and Player.log triage.
- Drafting editor scripts, validators, prefab generators, and documentation.
- Creating or evaluating visual direction for terrain, buildings, units, effects, icons, and UI readability.
- Producing first-pass materials, texture prompts, animation notes, and model replacement checklists.
- Generating test scenarios and validation checklists.
- Comparing screenshots against stage acceptance criteria.

## What AI Must Not Become

- The authoritative gameplay simulation.
- A replacement for deterministic Rts.Core tests.
- A reason to skip player-facing validation.
- A place to copy protected IP, commercial art, or large external documentation.
- A source of flat concept-sheet cards for final runtime terrain.

## Design Loop

1. Define the player-visible outcome.
2. List protected constraints: PCDesktop sidebar, QuestXR controls, Stage27.1 HUD separation, safe areas, and deterministic core boundaries.
3. Ask AI for options or a narrow implementation plan.
4. Implement only the smallest useful slice.
5. Capture screenshots or logs.
6. Compare against the acceptance checklist.
7. Run stage validation scripts.
8. Document what changed, what remains placeholder, and what must be replaced by real art.

## RTS-Specific AI Design Prompts

### Terrain

```text
Design terrain set dressing for an RTS board that must read clearly from an isometric/top-down camera. Avoid flat image cards. Use modular mesh prefabs with consistent pivots, grid alignment, LOD-safe material counts, and clear passability/buildability metadata.
```

### Base Building

```text
Improve base-building readability. Preserve deterministic placement rules. Make valid placement, blocked placement, power state, construction readiness, and production progress obvious without debug panels.
```

### Unit Motion

```text
Improve unit presentation over deterministic snapshots. Add visual-only acceleration, turn anticipation, turret lag, track/wheel motion, selection anchors, and readable formation spacing. Do not change authoritative movement rules unless requested.
```

### QuestXR Controls

```text
Review the left-hand build/selection controls and right-hand tactical controls. Preserve existing bindings and add only presentation or affordance improvements unless a validation failure proves a deeper issue.
```

### PC Sidebar

```text
Review the PCDesktop right sidebar for OpenRA-style production readability. Preserve the board safe area, category semantics, placement HUD separation, and non-debug player flow.
```

## Screenshot Review Checklist

- The first screen shows the actual game or tool, not a marketing page.
- Text is readable at Windows player resolution.
- PC sidebar remains visible and anchored to the right.
- Board does not slide under the sidebar or objective stack.
- Terrain reads as depth/material, not flat cards or pale primitives.
- Resource fields, roads, blockers, and buildable space are visually distinct.
- Debug panels are hidden by default.
- Victory/progression path is visible without console/debug knowledge.

## Validation Expectations

Use the closest current stage scripts, then add targeted checks for any new AI-assisted work:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
if (Test-Path .\tools\audit-full-validation-recursion.ps1) { .\tools\audit-full-validation-recursion.ps1 }
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
.\tools\build-windows-player-stage16.ps1
.\tools\inspect-latest-player-log.ps1
git diff --check
```

Add stage-specific scripts when a stage owns the affected feature.

## Documentation Standard

Every AI-assisted stage should record:

- What AI generated or suggested.
- What was accepted, edited, or rejected.
- Which assets are reference-only.
- Which assets are production/runtime.
- Which validators prove the behavior.
- Known limitations and the next replacement target.
