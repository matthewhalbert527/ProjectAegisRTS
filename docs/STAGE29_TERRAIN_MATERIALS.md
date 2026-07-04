# Stage 29 Terrain Materials

Stage 29 adds terrain/material profiles for a more realistic battlefield board while preserving placement readability.

The Stage 31 terrain source-reference sheets now live under:

```text
unity/Assets/Rts/Art/References/Terrain/Stage31TerrainSource
```

Those sheets inform the Stage 29-32 material and proxy-terrain direction: worn asphalt roads with shoulders and lane paint, weathered concrete/base pads, moss-capped cliffs, mineral clusters, wreckage/debris, barriers/fences, and restrained foliage. They are reference assets, not gameplay data.

| Terrain | Purpose | Placement/Pathing Read |
| --- | --- | --- |
| Grass / Dirt Field | Default battlefield surface with natural color breakup. | Buildable/readable when the fine grid is visible. |
| Compacted Base Ground | Starting-base construction zone. | Helps buildings feel grounded without changing rules. |
| Concrete Pad | Hardstand/foundation visual for base layout. | Highlights buildable placement scale. |
| Road / Path | Scouting and enemy-pressure route cue. | Non-authoritative visual guide. |
| Resource Field | Harvestable area with mineral tint and shards. | Distinguishes resource spacing from normal ground. |
| Rock / Blocked Terrain | No-build/no-ground-move readability. | Dark clusters and contrast edges signal blockage. |
| Water | Visually separate water/naval foundation terrain. | Ground-blocking read for the current prototype. |
| Fog / Explored Tint | Muted explored-space overlay. | Keeps actors and placement previews readable. |

Generated material assets live under:

```text
unity/Assets/Rts/Art/Materials/Terrain
unity/Assets/Rts/Art/Materials/Environment
```

Generated profile assets live under:

```text
unity/Assets/Rts/ScriptableObjects/Art/TerrainMaterialProfiles
```
