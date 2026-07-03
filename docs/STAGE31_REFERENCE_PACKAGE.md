# Stage 31 Screenshot And Reference Package

The Stage 31 handoff package uses generated screenshot artifacts plus existing docs as reference material for real model replacement.

## Screenshot Artifacts

- `build/screenshots/stage29_battlefield_visual_review.png`
- `build/screenshots/stage30_visual_readability_qa.png`

Regenerate these with:

```powershell
.\tools\run-unity-stage29-validation.ps1 -SkipCoreBuild
.\tools\run-unity-stage30-validation.ps1 -SkipCoreBuild
```

## Reference Docs

- `docs/STAGE20_360_VISUAL_STANDARDS.md`
- `docs/STAGE20_MVP_VISUAL_REPLACEMENT_GUIDE.md`
- `docs/STAGE21_ARTIST_ASSET_REPLACEMENT_CHECKLIST.md`
- `docs/STAGE21_MVP_VISUAL_QA.md`
- `docs/STAGE29_REALISTIC_BATTLEFIELD_VISUAL_DIRECTION.md`
- `docs/STAGE29_TERRAIN_MATERIALS.md`
- `docs/STAGE30_REPORT.md`
- `docs/STAGE30_VISUAL_READABILITY_QA.md`
- `docs/STAGE31_ARTIST_HANDOFF_PACKAGE.md`
- `docs/STAGE31_MVP_ART_REPLACEMENT_GUIDE.md`
- `docs/STAGE31_PER_ACTOR_PRODUCTION_CHECKLIST.md`
- `docs/STAGE31_QUEST_LOD_BUDGETS.md`

## Package Rule

The screenshots are generated artifacts, not source assets. Do not import them into Unity as production textures. They are for review, comparison, and artist briefing only.
