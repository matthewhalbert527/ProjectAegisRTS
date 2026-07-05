# Stage 32.8 Terrain Import Quality Report

- Albedo textures assigned: yes, on both primary final mesh materials.
- Normal maps assigned: yes, final mesh normal imports are forced to Normal Map.
- Roughness maps assigned: yes, roughness textures are assigned to supported Lit-map slots.
- UVs valid: yes enough for texture assignment, but the supplied ground OBJ remains low-detail and cannot match the reference art by geometry alone.
- Ground dark/flat root cause: the final OBJ is simple geometry with limited material slots, dark texture exposure, and an overpowering cyan grid helper in the old review composition.
- Cyan grid: helper only; Stage32.8 moves it behind the art and lowers opacity so it no longer dominates.
- Lighting/exposure: Stage32.8 review uses stronger warm key and fill lighting.
- Mesh detail: still limited; real production quality requires artist-authored 3D terrain meshes/textures.

## Results
- Final mesh prefabs: 4
- Image-card prefabs: 8
- Player-facing card replacements: 4
- Validation: passed.
