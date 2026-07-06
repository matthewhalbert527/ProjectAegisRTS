# Unity AI Building Texture Prompts

## Purpose

The building slate supports Unity AI-authored material textures without making the deterministic art generator depend on Unity AI services at runtime.

Unity AI generated materials or Texture2D outputs should be promoted or copied into:

`unity/Assets/Rts/Art/UnityAIBuildingSlate/SourceTextures/`

The building generator looks for these filename patterns with common Unity texture extensions such as `.png`, `.jpg`, `.jpeg`, `.tga`, or `.psd`:

- `<material_id>_ai_albedo.<ext>`
- `<material_id>_ai_normal.<ext>`
- `<material_id>_ai_occlusion.<ext>`

Non-`_ai_` names are also accepted as fallback source art:

- `<material_id>_albedo.<ext>`
- `<material_id>_normal.<ext>`
- `<material_id>_occlusion.<ext>`

If no promoted texture exists, the generator uses a deterministic procedural fallback so validation remains runnable on a clean machine.

## Unity AI Workflow

1. In Unity, open the Material Generator or Texture2D Generator.
2. Generate one material at a time using the prompts below.
3. Promote the result into the project.
4. Rename/copy the promoted texture maps into `SourceTextures`.
5. Run `ProjectAegisRTS > Unity AI > Build And Capture Building Slate`.
6. Run `.\tools\run-unity-ai-building-slate-checks.ps1`.

To enforce that promoted AI textures are present for a quality pass:

```powershell
.\tools\run-unity-ai-building-slate-checks.ps1 -RequireAiSourceTextures
```

## Global Negative Prompt

No logos, no letters, no UI, no text, no faction symbols, no copyrighted IP, no recognizable Command and Conquer or Red Alert insignia, no vehicles, no characters, no weapons as standalone objects, no perspective scene render, no full building image, no flat card, no unreadable noisy collage.

## Material Prompts

### worn_green_gray_metal

Generate a seamless PBR material for a classic military RTS building exterior: worn green-gray painted steel panels, chipped edges, subtle rivets, grime in panel seams, sun-faded paint, small rust scratches, top-down readable, game-ready, tileable, realistic but stylized.

### dark_oiled_metal

Generate a seamless PBR material for dark oiled industrial metal used inside vehicle bays: blackened steel, oily smears, worn highlights on raised ribs, soot in seams, subtle mechanical grime, tileable, game-ready RTS material.

### scraped_edge_metal

Generate a seamless PBR material for scraped exposed steel armor edges: gray steel, worn bevel highlights, chipped paint remnants, tiny scratches, muted yellow-gray edge wear, tileable, game-ready.

### gunmetal_mechanics

Generate a seamless PBR material for gunmetal mechanical parts: dark blue-gray steel, machined wear, circular scuffs, bolts, oil stains, subtle roughness variation, tileable, game-ready RTS asset material.

### ribbed_bay_door

Generate a seamless PBR material for a ribbed roll-up industrial bay door: dark steel slats, vertical grime streaks, worn lower edge, scraped paint, recessed grooves, readable from overhead RTS camera, tileable.

### small_service_panels

Generate a seamless PBR material for small military service panels: muted green-gray panels, tiny bolts, access hatches, warning labels implied as shapes only with no text, chipped corners, grime, tileable.

### weathered_concrete

Generate a seamless PBR material for weathered military concrete: gray concrete slab, hairline cracks, darker stains, chipped aggregate, dust, subtle square panel seams, tileable, game-ready.

### foundation_concrete

Generate a seamless PBR material for heavy base foundation concrete: reinforced slab, worn edges, embedded aggregate, oil stains, faint construction seams, muted battlefield color, tileable.

### dark_worn_concrete

Generate a seamless PBR material for dark worn concrete interior floors: black-gray concrete, oil stains, dust, scuffed vehicle tracks, shallow cracks, tileable, game-ready.

### worn_asphalt_ramp

Generate a seamless PBR material for worn asphalt and vehicle ramp surfaces: dark asphalt, tire scuffs, aggregate speckles, cracks, dusty edge wear, tileable, top-down RTS readable.

### classic_red_command_accent

Generate a seamless PBR material for classic red military command accent paint on metal panels: deep red painted steel, chipped edges, soot stains, faded sun-worn paint, subtle rivets, no symbols or text, tileable.

### defense_red_brown_accent

Generate a seamless PBR material for red-brown defensive armor paint: dark red oxide painted steel, scratches, impact chips, soot, exposed metal on edges, no symbols or text, tileable.

## Current Fallback

The fallback texture generator now adds panel seams, scratches, grime, rust, cracks, aggregate, stains, and other surface breakup. It is still a fallback. Player-facing quality should use promoted Unity AI or artist-authored source textures when available.
