# Stage 31 Quest LOD Budgets

These are handoff targets for artist-authored MVP replacements. They are conservative planning budgets, not final Quest device profiling results.

## Shared Targets

| Asset type | LOD0 triangles | LOD1 triangles | LOD2 triangles | Materials | Notes |
| --- | ---: | ---: | ---: | ---: | --- |
| Large building | 3500-6000 | 1600-2500 | 500-900 | 4-6 | Fabrication hub, refinery, war factory. |
| Small building | 1800-3500 | 800-1500 | 250-600 | 3-5 | Power plant, barracks, gun tower. |
| Vehicle | 1200-2500 | 500-1000 | 150-350 | 3-4 | Light tank, harvester. |
| Infantry | 600-1200 | 250-500 | 80-180 | 2-3 | Keep silhouette and weapon readable. |

## Quest Budget Rules

- Prefer shared atlases and trim sheets over unique material sets.
- Keep transparent materials rare and isolated.
- Use emissive accents sparingly for role/status readability.
- LOD1 should preserve top-down silhouette and role identity.
- LOD2 can simplify side/rear detail but must keep footprint and selection readability.
- Avoid dense hidden interior geometry inside buildings.
- Keep collider and gameplay proxy data separate from rendered mesh complexity.

## Review Thresholds

An asset can exceed these planning targets only with a written reason in its replacement checklist and clean Stage 15/21/30/31 validation. Final Quest approval still requires device profiling in a later stage.
