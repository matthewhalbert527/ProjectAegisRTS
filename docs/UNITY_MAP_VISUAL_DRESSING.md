# Unity Map Visual Dressing

## Purpose

Project Aegis maps remain authored and validated as `.aegismap.json`. The Unity visual dressing pass is an editor-side renderer that turns that logical document into a richer RTS battlefield scene for inspection and iteration.

The visual pass does not change gameplay data. It reads player starts, terrain, blockers, resources, and map metadata from the selected `.aegismap.json` file and creates deterministic scene objects.

## Implemented Now

Use:

`Project Aegis > Map Editor > Build Visual Terrain From Aegis Map`

Workflow:

1. Select a `.aegismap.json` asset in Unity.
2. Run the menu item.
3. Unity creates an `Aegis Visual Map - <mapId>` scene object.
4. The generated scene includes:
   - a blended terrain texture with grass, rough ground, water, cliffs, ore tinting, soft dirt routes, and muddy water banks
   - concrete base pads at player starts
   - deterministic cliff rock chains on blocker/cliff boundaries
   - ore chunk clusters on resource cells
   - deterministic scatter for boulders, vegetation, road pebbles, and craters
   - generated material and texture assets under `Assets/Rts/MapEditor/VisualAssets` and `Assets/Rts/MapEditor/VisualBuilds`

The visual seed is derived from the map identity and dimensions, so the same map produces the same dressing layout unless the source map changes.

## Biome Profiles

The builder includes original Project Aegis color/material profiles for:

- forest / grassland
- desert
- tundra
- volcanic
- rocky / wasteland

Profiles currently drive terrain colors, mud banks, water tones, cliff colors, path colors, ore tinting, vegetation, concrete, and crater materials.

## Current Limits

- The first pass uses procedural proxy geometry and generated materials; it does not yet use final hand-authored rock, tree, river, road, crater, or base-pad art.
- Roads are generated as deterministic soft terrain routes between player starts and the map center. A later pass should read explicit road/region/path metadata when map documents include it.
- Water is rendered through the generated terrain texture with muddy-bank blending. A later pass can replace this with spline meshes, animated water materials, reeds, foam, and shoreline decals.
- Cliff ridges are proxy capsule clusters placed on blocker/cliff boundaries. A later pass should swap these for modular original cliff meshes.

## Asset Rules

Use the provided reference image only as a quality target. Do not copy protected art, textures, names, UI, map data, or faction identifiers from Command & Conquer, Red Alert, OpenRA, or any other reference project.

Production-quality final art should be original Project Aegis content, generated under clear rights, purchased/licensed, or created in-house.
