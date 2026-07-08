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
   - a blended terrain texture with grass, rough ground, water, cliffs, ore tinting, soft dirt routes, muddy water banks, and clustered terrain-color transitions that reduce one-cell debug-map artifacts
   - deterministic road/path decals with art-pack soft dust overlays, tire-rut strips, and gravel scuff patches over the terrain texture
   - concrete base pads at player starts with art-pack base-pad meshes when Unity can import `.glb`, plus terrain blend, inner panels, trim strips, seam lines, approach dust, and grime decals
   - deterministic art-pack cliff meshes on blocker/cliff boundaries when `.glb` import is available, with generated faceted fallback geometry otherwise
   - art-pack ore nugget/cluster meshes with soft ore-stained ground falloff around resource cells, plus generated faceted fallback ore if needed
   - deterministic scatter for art-pack boulders, pebbles, vegetation, river-edge meshes, and craters, again with generated proxy fallbacks where `.glb` is unavailable
   - generated material and texture assets under `Assets/Rts/MapEditor/VisualAssets` and `Assets/Rts/MapEditor/VisualBuilds`

The visual seed is derived from the map identity and dimensions, so the same map produces the same dressing layout unless the source map changes.

The batch preview validation uses `sample_ai_medium_forest_2p_river_chokepoint.aegismap.json`, a checked-in deterministic sample with river water, cliff/blocker bands, ore clusters, and two connected player starts. This gives the render check a map that exercises the terrain detail layers visible when zoomed in without overcrowding the preview with route lines.

The art-pack showcase sample is:

`unity/Assets/Rts/MapEditor/Samples/sample_art_pack_showcase_160_forest_river.aegismap.json`

It is a 160x160 forest river composition intended to exercise the imported art pack in one place: concrete base pads, soft roads, river bends, muddy banks, cliff and rock ridges, ore clusters, craters, vegetation, and navigation/region metadata. The batch screenshot method is `ProjectAegisRTS.UnityClient.EditorTools.AegisMapVisualBuilder.RenderArtPackShowcaseForBatch`, which writes `C:\Users\matth\AppData\Local\Temp\ProjectAegisRTS\aegis_art_pack_showcase.png`.

## Biome Profiles

The builder includes original Project Aegis color/material profiles for:

- forest / grassland
- desert
- tundra
- volcanic
- rocky / wasteland

Profiles currently drive terrain colors, mud banks, water tones, cliff colors, path colors, ore-stained soil, vegetation, concrete, pebble roughness, and crater materials.

## Imported Art Pack

`ProjectAegis_MapVisualArtPack_v1` is checked in under:

`unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/`

The visual builder reads the pack directly from this folder. It uses the original PNG terrain/material textures and transparent decals for road dust, tire ruts, gravel scuffs, muddy shorelines, water highlights, ore dust, concrete seams/grime, resource glints, and crater overlays. It also instantiates the pack's `.glb` meshes for cliffs, boulders, pebbles, ore clusters, vegetation, river-edge props, craters, and the 14x14 base pad.

This branch adds the Unity glTFast package (`com.unity.cloud.gltfast`) so those `.glb` files import directly in Unity. If the package is removed or unavailable on a future machine, the builder keeps working by falling back to deterministic generated meshes for those props. No Blender or Photoshop remake is required for the supplied pack.

## Roads And Base Pads

The logical map does not store Unity-only road meshes. The visual builder derives deterministic soft road decals from the same generated start-to-center route segments used for the terrain path texture. These overlays add dust, paired rut strips, and occasional gravel scuffs without changing pathability.

Base pads are also visual-only dressing on top of player start metadata. Each generated pad receives concrete panels, thin seam decals, trim strips, a dusty approach apron facing the map center, and deterministic grime marks so start areas read less like flat placeholder slabs.

## Water Rendering

The logical map still stores water as deterministic terrain cells. The visual builder now derives a smooth river centerline from those cells and renders water/bank influence from that line instead of drawing each water cell as a visible square. Short gaps in the logical watercourse can receive a shallow muddy ford connector so gameplay crossings remain readable without turning the runtime map into a Unity-only source of truth.

## Current Limits

- The builder now uses the original Project Aegis v1 art pack where Unity can import the asset type. GLB mesh rendering is provided by Unity glTFast in this branch; without that package, the builder intentionally falls back to generated proxy geometry.
- Roads are generated as deterministic soft terrain routes between player starts and the map center. A later pass should read explicit road/region/path metadata when map documents include it.
- Water is rendered through generated terrain texture watercourses with smooth derived centerlines, muddy-bank blending, shallow ford hints, and deterministic shore scatter. A later pass can replace this with spline meshes, animated water materials, reeds, foam, and shoreline decals.
- Ore, cliff, vegetation, crater, river-edge, and base-pad props use imported art-pack assets when available. A later art pass can add higher-poly sculpted meshes, animated water, and tuned LOD/prefab variants.

## Asset Rules

Use the provided reference image only as a quality target. Do not copy protected art, textures, names, UI, map data, or faction identifiers from Command & Conquer, Red Alert, OpenRA, or any other reference project.

Production-quality final art should be original Project Aegis content, generated under clear rights, purchased/licensed, or created in-house.
