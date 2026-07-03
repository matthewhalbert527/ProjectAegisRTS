# Stage 30 Report

Stage 30 is a visual-readability QA pass layered on top of the Stage 29 realistic battlefield visuals. It does not change gameplay, deterministic `Rts.Core` data, production rules, PCDesktop sidebar behavior, QuestXR controls, or debug-panel defaults.

## Implemented

- Added a Stage 30 readability profile with actor/terrain contrast, resource-pop, fine-grid dominance, and PC camera readability thresholds.
- Added an additive readability overlay for the nine MVP production proxies: dark ground cuts, top identity stripes, forward cues, resource relationship pops, and combat role accents.
- Added `Stage30_VisualReadabilityQa.unity`, a visible review HUD, screenshot capture, Play Mode smoke validation, and a generated QA report.
- Added fast, medium, player-facing, and full Stage 30 validation tiers while preserving the non-recursive medium validation rule.

## Validation Tiers

- Fast iteration: `.\tools\run-stage30-fast-checks.ps1`
- Pre-commit: `.\tools\run-stage30-medium-checks.ps1`
- Player-facing/log: `.\tools\run-stage30-player-facing-checks.ps1`
- Full acceptance: `.\tools\run-stage30-checks.ps1`

## Screenshot

Stage 30 validation captures:

```text
build/screenshots/stage30_visual_readability_qa.png
```

## Stage 32 Follow-Up

Stage 32 adds terrain-piece catalogs and player-facing set dressing on top of the Stage 29/30 visual baseline. Stage 30 readability rules still apply: actor silhouettes, resources, fine-grid placement previews, and the PCDesktop sidebar safe area must remain readable after terrain decoration is added.
