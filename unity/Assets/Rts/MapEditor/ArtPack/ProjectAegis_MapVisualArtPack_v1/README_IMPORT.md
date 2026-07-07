# Project Aegis Map Visual Art Pack v1

This zip contains an original, import-ready visual terrain asset pack for the Project Aegis RTS map generator.

## Unity Target

- Unity 6 / URP-compatible workflow.
- One Project Aegis map cell equals one Unity unit.
- Meshes are exported as `.glb` files with Unity-scale dimensions and base-centered pivots where applicable.
- Textures and decals are `.png` files.
- Terrain albedo textures are 2048x2048 and designed for tiling.
- Transparent decals are 1024x1024 PNGs unless otherwise noted.

## Recommended Import Path

Copy this folder into the game repository at:

```text
unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/
```

Then in Unity:

1. Import or refresh the project.
2. Create URP Lit materials from the descriptors in `Materials/`.
3. Assign terrain albedo/normal/roughness-AO textures as terrain layers or map-chunk materials.
4. Use `manifest.json` to map generated `.aegismap.json` material IDs to visual assets.
5. Use transparent decals for roads, craters, ore dust, shoreline, construction wear, and grime.
6. Use mesh assets under `Meshes/` for cliffs, rocks, resources, vegetation, river details, base pads, and crater mesh variants.

## Import Notes

- `.glb` import may require Unity glTF support depending on the installed Unity packages. If your Unity project does not import `.glb` automatically, add Unity's glTF importer/glTFast package or convert using your project pipeline. No Blender or Photoshop remake is required.
- The files are source-ready and stable-named so Codex or Unity tooling can map them by ID.
- No colliders are included. Gameplay collision/buildability should continue to come from `.aegismap.json` metadata.
- Pivot conventions are listed per asset in `manifest.json`.

## Runtime Mapping Assumption

`AegisMapDocument` remains the deterministic gameplay source of truth. These assets are visual mappings for terrain IDs, resource IDs, blocker/cliff metadata, roads, craters, base pads, and biome decorations.

## Previews

- `Preview/contact_sheet.png`
- `Preview/forest_river_cliffs_preview.png`
- `Preview/base_pad_ore_preview.png`
- `Preview/road_crater_scatter_preview.png`

## Asset Count

Total manifest entries: 114

See `manifest.json` for the complete list and placement rules.
