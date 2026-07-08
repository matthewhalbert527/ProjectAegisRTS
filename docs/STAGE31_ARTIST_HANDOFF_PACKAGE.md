# Stage 31 Artist Handoff Package

This package turns the Stage 29-30 visual work into practical instructions for modelers, technical artists, and future replacement validators. The active game still uses production proxies; real models should be introduced one actor at a time through the replacement workflow.

## Export And Modeling Brief

- Model around the existing proxy root and fine-grid footprint. The root pivot stays centered on the gameplay footprint at ground height.
- Preserve the actor's readable top-down silhouette first, then add side/rear detail for Quest walkaround inspection.
- Keep foundations, ramps, doors, pads, tracks, hoppers, turrets, barrels, and roof service details aligned with the current proxy identity.
- Export static MVP buildings and vehicles as FBX or GLB with transforms frozen to Unity scale: 1 Unity unit equals one coarse board cell.
- Export infantry as a simple static or lightly rig-ready model with stable aim and muzzle sockets; final locomotion/animation can come later.
- Do not bake gameplay footprints into mesh names. Footprints remain authoritative in `Rts.Core` and the Unity actor definition metadata.

## Material Naming Rules

Use lowercase safe actor IDs and shared material roles:

```text
mvp_<actor_id>_<role>_mat
terrain_<surface>_<role>_mat
trim_shared_<set>_<role>_mat
```

Recommended roles are `body`, `accent`, `dark_metal`, `rubber`, `glass`, `emissive`, `foundation`, `resource`, `warning`, and `readability_trim`.

Avoid per-object throwaway names such as `Material.001`, `DefaultMat`, or exporter-generated names. Shared materials are preferred over unique materials per mesh.

## Trim-Sheet Guidance

- Use one shared military-industrial trim sheet for panel lines, vents, rails, caution bands, edge wear, and small service labels.
- Keep a second small emissive/detail sheet for lights, status strips, resource glow, and power-state accents.
- Keep albedo/value contrast strong enough for the 1600x900 PC camera and restrained enough that Stage 30 readability trims still pop.
- Do not place critical identity only in tiny normal-map detail; identity must read in silhouette and top color/value blocks.

## Screenshot And Reference Package

Reference the current visual checkpoints before replacing any proxy:

- Stage 29 battlefield visual review: `build/screenshots/stage29_battlefield_visual_review.png`
- Stage 30 readability QA review: `build/screenshots/stage30_visual_readability_qa.png`
- Stage 32 terrain set-dressing review: `build/screenshots/stage32_terrain_set_dressing_review.png`
- Stage 32 player-facing terrain view: `build/screenshots/stage32_player_facing_terrain_view.png`
- Stage 31 terrain source references: `unity/Assets/Rts/Art/References/Terrain/Stage31TerrainSource/`
- Stage 29 direction: `docs/STAGE29_REALISTIC_BATTLEFIELD_VISUAL_DIRECTION.md`
- Stage 30 readability report: `docs/STAGE30_VISUAL_READABILITY_QA.md`
- Stage 31 terrain source guide: `docs/STAGE31_TERRAIN_SOURCE_REFERENCES.md`
- Stage 32 terrain piece library: `docs/STAGE32_TERRAIN_PIECE_LIBRARY.md`
- Stage 32 set dressing guide: `docs/STAGE32_SET_DRESSING_GUIDE.md`
- MVP visual QA baseline: `docs/STAGE21_MVP_VISUAL_QA.md`

The screenshot files are generated validation artifacts and may not be tracked by Git. Regenerate them with `.\tools\run-unity-stage29-validation.ps1 -SkipCoreBuild` and `.\tools\run-unity-stage30-validation.ps1 -SkipCoreBuild`.

Stage 32 screenshots are generated with `.\tools\run-unity-stage32-validation.ps1 -SkipCoreBuild`.

Terrain replacement work should use the Stage 31 terrain source sheets as visual direction for modular roads, base pads, cliffs, resources, wreckage, barriers, fences, vegetation, and board spacing. Preserve Stage 30 top-down readability and keep terrain visuals below gameplay-critical actor silhouettes and placement previews.

## Handoff Acceptance

Before a replacement can become active:

- Stage 21 import scan finds the candidate and reports no errors.
- Stage 20/21 socket, pivot, fallback, and LOD requirements remain intact.
- Stage 29 realistic material/detail intent is preserved or improved.
- Stage 30 readability overlays are replaced by equivalent real model readability cues or remain as additive children.
- PCDesktop right sidebar, minimap safe area, QuestXR hand-control split, and Stage27.1 placement HUD separation still validate.
- Player.log remains free of red-error signatures.
