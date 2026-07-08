# Map Visual Compiler Audit

## Summary

The previous Unity map preview improved on the first debug pass, but it still behaved like a flat stamp compositor. `AegisMapVisualBuilder` loaded a `.aegismap.json`, baked most terrain detail into one generated texture on one large quad, then scattered road, river, ore, rock, cliff, crater, vegetation, and base-pad objects over that surface.

That is useful for proving the deterministic generator can be seen in Unity. It is not enough for a production RTS battlefield because the visual system has no durable layer contracts, no topology model, weak asset semantics, and only limited QA evidence about which layer produced which detail.

## Why The Screenshot Looked Flat

- Most terrain identity was compressed into one albedo texture, so grass, mud, roads, ore staining, riverbanks, and rough terrain were paint decisions instead of scene/layer decisions.
- Road ruts, scuffs, muddy banks, grime, and ore dust were mostly surface quads without a shared terrain-transition model.
- Cliffs were placed as rock chains near blocker cells, which made them read as scattered props instead of high-to-low terrain edges.
- Ore was rendered as clusters around resource cells, but the field itself did not own center/radius/fill/depletion visual behavior.
- Base pads were improved with panels and grime, but they were still generated inside the monolithic builder rather than as a modular pad system with explicit panel/trim/corner/wear roles.
- Scatter was deterministic, but the rules lived inline with unrelated terrain and mesh code.

## Current Builder Behavior

`unity/Assets/Rts/Scripts/MapEditor/Editor/AegisMapVisualBuilder.cs` currently does all of the following in one static class:

- Loads and normalizes `AegisVisualMapDocument`.
- Builds lookup helpers for terrain, blockers, starts, and resources.
- Creates one large generated terrain texture and applies it to a quad.
- Derives visual road segments from player starts and map center.
- Derives visual river segments from water cells.
- Adds road dust/rut/scuff decals.
- Adds river dressing and shoreline props.
- Adds concrete base-pad geometry and decals.
- Adds cliff rock chains around cliff-like cells.
- Groups ore into visual fields and creates chunks/glints.
- Adds deterministic scatter for rocks, vegetation, craters, pebbles, and bank details.
- Creates and caches materials from the art pack.
- Captures batch preview screenshots.

The code works as a prototype, but it mixes data extraction, layer compilation, material binding, scene construction, and QA capture. That makes it difficult to replace prototype art with production assets without risking gameplay map semantics.

## Why One Generated Terrain Texture Is Insufficient

A single quad texture cannot express durable terrain layers, material weights, terrain chunk metadata, or shader-swappable surfaces. It also hides the reason behind each visual decision. A production compiler needs explicit terrain chunks, semantic roles, transition edges, water bodies, shorelines, road segments, cliff edges, resource fields, and scatter placement summaries.

The new compiler starts that move by creating a base terrain chunk layer plus explicit transition masks. It remains prototype-rendered, but the data and scene hierarchy now match a future shader/material-layer terrain system.

## Why Road/Rut/Scuff Quads Are Insufficient Alone

Road quads are useful only after a road system has chosen segments, widths, edge wear, and forbidden zones. Randomly adding more ruts would still look noisy. The visual compiler now treats roads as a layer derived from path segments with body and tire-track summaries.

## Why Resource Clusters Need Field-Level Composition

Resource visuals need to communicate field ownership and state: center, radius, current amount, max amount, depleted/regenerating state, high-value glints, and edge falloff. Individual ore props cannot show depletion or density correctly. The compiler now groups resource cells by stable field ID and renders field dust plus center-weighted chunks.

## Why Base Pads Need Modular Composition

Base pads are gameplay-important reading anchors. They need panel areas, trims, corners, seams, wear, dirt integration, and future footprint alignment. One slab looks like a debug marker. The compiler adds a modular base-pad layer with explicit component roles.

## Why Cliffs Need Topology-Driven Placement

Cliffs communicate blocked height and movement constraints. They must follow exposed edges, rotate toward the lower neighbor, and distinguish straight runs, corners, and endcaps. Random rock chains make blockers ambiguous. The compiler now detects exposed cliff-like edges and emits straight, corner, and endcap counts.

## Boundary Between Core And Unity

`src/Rts.Core` owns deterministic gameplay map data:

- terrain
- blockers
- resource cells and fields
- player starts
- build pads/regions
- pathability and fairness summaries
- map metadata

Unity owns visual interpretation:

- materials
- meshes
- prefabs
- decals
- visual themes
- cameras
- scene hierarchy
- preview windows
- screenshot capture
- editor QA overlays

`.aegismap.json` remains the runtime source of truth. Tiled remains authoring/interchange only. The visual compiler must never make Unity scene output authoritative gameplay data.

## Files Refactored Or Added

- Keep `AegisMapVisualBuilder.cs` as a compatibility wrapper and batch entry point.
- Add runtime visual contracts under `unity/Assets/Rts/Scripts/MapEditor/Runtime/Visuals/`.
- Add editor compiler layers under `unity/Assets/Rts/Scripts/MapEditor/Editor/Visuals/`.
- Add `AegisMapVisualCompilerWindow` for direct designer use.
- Add docs for art direction, asset contracts, compiler plan, and screenshot QA.

## New Architecture

`AegisMapVisualCompiler` orchestrates layers in this order:

1. base terrain surface
2. terrain transitions
3. water surface
4. shoreline mud/wetness
5. roads and tire tracks
6. cliffs and elevation edges
7. blockers/rocks
8. resources
9. base pads
10. craters/scorch/rubble
11. vegetation scatter
12. debug overlays

The current implementation groups blockers/rocks, craters/rubble, and vegetation into topology/scatter layers, but the compile result reports counts and warnings so those can be split further without changing the map format.
