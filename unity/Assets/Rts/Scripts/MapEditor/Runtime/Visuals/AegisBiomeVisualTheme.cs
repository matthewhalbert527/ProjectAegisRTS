using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    public static class AegisBiomeVisualTheme
    {
        public static AegisMapVisualTheme Create(string biome)
        {
            if (!string.IsNullOrEmpty(biome) && biome.ToLowerInvariant().Contains("desert"))
                return DesertPrototypeVisualTheme();

            if (!string.IsNullOrEmpty(biome) && biome.ToLowerInvariant().Contains("debug"))
                return DebugVisualTheme();

            return ForestPrototypeVisualTheme();
        }

        public static AegisMapVisualTheme DebugVisualTheme()
        {
            var theme = BaseTheme("debug", "Debug Visual Theme", "debug")
                .Add("terrain.grass", new Color(0.20f, 0.48f, 0.20f, 1f))
                .Add("terrain.dark_grass", new Color(0.10f, 0.32f, 0.13f, 1f))
                .Add("terrain.dirt", new Color(0.47f, 0.36f, 0.24f, 1f))
                .Add("terrain.gravel", new Color(0.38f, 0.37f, 0.33f, 1f))
                .Add("terrain.mud", new Color(0.25f, 0.18f, 0.13f, 1f))
                .Add("terrain.shallow_water", new Color(0.12f, 0.36f, 0.44f, 0.86f), transparent: true)
                .Add("terrain.deep_water", new Color(0.05f, 0.18f, 0.27f, 0.92f), transparent: true)
                .Add("terrain.cliff_ground", new Color(0.34f, 0.34f, 0.31f, 1f))
                .Add("terrain.ore_stained_soil", new Color(0.45f, 0.34f, 0.13f, 0.76f), transparent: true)
                .Add("terrain.concrete_base_pad", new Color(0.72f, 0.74f, 0.70f, 1f))
                .Add("road.dirt", new Color(0.47f, 0.36f, 0.25f, 0.62f), transparent: true)
                .Add("road.gravel", new Color(0.48f, 0.46f, 0.40f, 0.66f), transparent: true)
                .Add("river.water", new Color(0.07f, 0.27f, 0.34f, 0.90f), transparent: true)
                .Add("river.shoreline", new Color(0.36f, 0.27f, 0.17f, 0.08f), transparent: true)
                .Add("river.shoreline_feather", new Color(0.42f, 0.35f, 0.23f, 0.06f), transparent: true)
                .Add("cliff.edge.straight", new Color(0.36f, 0.36f, 0.34f, 1f))
                .Add("cliff.edge.corner_inner", new Color(0.30f, 0.30f, 0.28f, 1f))
                .Add("cliff.edge.corner_outer", new Color(0.42f, 0.42f, 0.39f, 1f))
                .Add("cliff.edge.endcap", new Color(0.32f, 0.32f, 0.30f, 1f))
                .Add("blocker.rock", new Color(0.35f, 0.35f, 0.32f, 1f))
                .Add("resource.ore", new Color(0.96f, 0.70f, 0.20f, 1f))
                .Add("resource.crystal", new Color(0.35f, 0.88f, 0.95f, 1f))
                .Add("resource.salvage", new Color(0.45f, 0.47f, 0.43f, 1f))
                .Add("resource.energy", new Color(0.40f, 0.65f, 1.00f, 1f))
                .Add("resource.ore_dust", new Color(0.48f, 0.32f, 0.10f, 0.48f), transparent: true, albedo: "Decals/Resources/ore_dust_soft_01.png")
                .Add("resource.glint", new Color(1.0f, 0.82f, 0.30f, 0.74f), transparent: true, albedo: "Decals/Resources/resource_glint_01.png")
                .Add("vegetation.tree", new Color(0.10f, 0.28f, 0.10f, 1f))
                .Add("vegetation.bush", new Color(0.16f, 0.38f, 0.15f, 1f))
                .Add("vegetation.grass", new Color(0.25f, 0.50f, 0.20f, 1f))
                .Add("decal.crater", new Color(0.06f, 0.05f, 0.04f, 0.82f), transparent: true)
                .Add("decal.scorch", new Color(0.04f, 0.035f, 0.025f, 0.50f), transparent: true, albedo: "Decals/Battlefield/scorch_mark_01.png")
                .Add("decal.rubble", new Color(0.28f, 0.27f, 0.25f, 0.76f), transparent: true)
                .Add("basepad.panel", new Color(0.84f, 0.86f, 0.82f, 1f))
                .Add("basepad.trim", new Color(0.70f, 0.63f, 0.42f, 1f))
                .Add("basepad.corner", new Color(0.78f, 0.80f, 0.76f, 1f))
                .Add("basepad.grime", new Color(0.10f, 0.09f, 0.07f, 0.42f), transparent: true)
                .Add("terrain.grass_mottle", new Color(0.10f, 0.22f, 0.08f, 0.28f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.dirt_mottle", new Color(0.42f, 0.31f, 0.20f, 0.38f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.gravel_mottle", new Color(0.28f, 0.27f, 0.24f, 0.42f), transparent: true, albedo: "Decals/Battlefield/rubble_scatter_01.png")
                .Add("terrain.wet_mud_detail", new Color(0.16f, 0.11f, 0.07f, 0.48f), transparent: true, albedo: "Decals/River/muddy_shoreline_01.png")
                .Add("terrain.blend_grass", new Color(0.10f, 0.20f, 0.07f, 0.22f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.blend_dirt", new Color(0.44f, 0.31f, 0.20f, 0.34f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.blend_gravel", new Color(0.30f, 0.28f, 0.24f, 0.28f), transparent: true, albedo: "Decals/Battlefield/rubble_scatter_01.png")
                .Add("terrain.blend_mud", new Color(0.16f, 0.11f, 0.07f, 0.30f), transparent: true, albedo: "Decals/River/muddy_shoreline_01.png")
                .Add("water.highlight", new Color(0.32f, 0.72f, 0.78f, 0.28f), transparent: true, albedo: "Decals/River/water_highlight_streaks.png")
                .Add("road.soft_dust", new Color(0.55f, 0.43f, 0.30f, 0.50f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("road.worn_edge", new Color(0.62f, 0.54f, 0.42f, 0.46f), transparent: true, albedo: "Decals/Roads/worn_path_edge_01.png")
                .Add("road.mud_track", new Color(0.24f, 0.17f, 0.11f, 0.48f), transparent: true, albedo: "Decals/Roads/mud_track_01.png")
                .Add("road.tire_left", new Color(0.18f, 0.14f, 0.10f, 0.44f), transparent: true, albedo: "Decals/Roads/tire_rut_left.png")
                .Add("road.tire_right", new Color(0.18f, 0.14f, 0.10f, 0.44f), transparent: true, albedo: "Decals/Roads/tire_rut_right.png")
                .Add("basepad.panel_decal", new Color(0.82f, 0.84f, 0.80f, 0.62f), transparent: true, albedo: "Decals/BasePads/base_pad_panel_center.png")
                .Add("basepad.trim_decal", new Color(0.70f, 0.62f, 0.38f, 0.66f), transparent: true, albedo: "Decals/BasePads/base_pad_trim_straight.png")
                .Add("basepad.crack", new Color(0.08f, 0.075f, 0.065f, 0.52f), transparent: true, albedo: "Decals/BasePads/concrete_crack_01.png")
                .Add("basepad.construction_wear", new Color(0.22f, 0.17f, 0.12f, 0.42f), transparent: true, albedo: "Decals/BasePads/construction_wear_01.png");
            ApplyProductionProxyTexturePaths(theme);
            return theme;
        }

        public static AegisMapVisualTheme ForestPrototypeVisualTheme()
        {
            var theme = DebugVisualTheme();
            theme.ThemeId = "forest_prototype";
            theme.DisplayName = "Forest Prototype Visual Theme";
            theme.Biome = "forest";
            theme.Add("terrain.grass_detail", new Color(0.18f, 0.34f, 0.15f, 1f), albedo: "Terrain/forest_grass_albedo.png", normal: "Terrain/forest_grass_normal.png", mask: "Terrain/forest_grass_roughness_ao.png");
            return theme;
        }

        public static AegisMapVisualTheme DesertPrototypeVisualTheme()
        {
            var theme = BaseTheme("desert_prototype", "Desert Prototype Visual Theme", "desert")
                .Add("terrain.grass", new Color(0.47f, 0.40f, 0.25f, 1f))
                .Add("terrain.dark_grass", new Color(0.40f, 0.34f, 0.20f, 1f))
                .Add("terrain.dirt", new Color(0.62f, 0.49f, 0.30f, 1f))
                .Add("terrain.gravel", new Color(0.46f, 0.41f, 0.34f, 1f))
                .Add("terrain.mud", new Color(0.34f, 0.25f, 0.17f, 1f))
                .Add("terrain.shallow_water", new Color(0.15f, 0.42f, 0.47f, 0.82f), transparent: true)
                .Add("terrain.deep_water", new Color(0.07f, 0.25f, 0.32f, 0.90f), transparent: true)
                .Add("terrain.cliff_ground", new Color(0.42f, 0.37f, 0.30f, 1f))
                .Add("terrain.ore_stained_soil", new Color(0.57f, 0.39f, 0.14f, 0.76f), transparent: true)
                .Add("terrain.concrete_base_pad", new Color(0.76f, 0.75f, 0.69f, 1f))
                .Add("road.dirt", new Color(0.57f, 0.43f, 0.26f, 0.62f), transparent: true)
                .Add("road.gravel", new Color(0.52f, 0.47f, 0.39f, 0.66f), transparent: true)
                .Add("river.water", new Color(0.08f, 0.31f, 0.36f, 0.88f), transparent: true)
                .Add("river.shoreline", new Color(0.45f, 0.31f, 0.18f, 0.09f), transparent: true)
                .Add("river.shoreline_feather", new Color(0.50f, 0.39f, 0.24f, 0.06f), transparent: true)
                .Add("cliff.edge.straight", new Color(0.46f, 0.41f, 0.34f, 1f))
                .Add("cliff.edge.corner_inner", new Color(0.38f, 0.34f, 0.28f, 1f))
                .Add("cliff.edge.corner_outer", new Color(0.52f, 0.46f, 0.38f, 1f))
                .Add("cliff.edge.endcap", new Color(0.42f, 0.37f, 0.31f, 1f))
                .Add("blocker.rock", new Color(0.47f, 0.42f, 0.35f, 1f))
                .Add("resource.ore", new Color(0.96f, 0.68f, 0.18f, 1f))
                .Add("resource.crystal", new Color(0.34f, 0.82f, 0.90f, 1f))
                .Add("resource.salvage", new Color(0.46f, 0.45f, 0.40f, 1f))
                .Add("resource.energy", new Color(0.42f, 0.66f, 0.98f, 1f))
                .Add("resource.ore_dust", new Color(0.54f, 0.36f, 0.12f, 0.48f), transparent: true, albedo: "Decals/Resources/ore_dust_soft_01.png")
                .Add("resource.glint", new Color(1.0f, 0.78f, 0.26f, 0.74f), transparent: true, albedo: "Decals/Resources/resource_glint_01.png")
                .Add("vegetation.tree", new Color(0.25f, 0.30f, 0.13f, 1f))
                .Add("vegetation.bush", new Color(0.28f, 0.34f, 0.16f, 1f))
                .Add("vegetation.grass", new Color(0.36f, 0.40f, 0.19f, 1f))
                .Add("decal.crater", new Color(0.08f, 0.06f, 0.04f, 0.82f), transparent: true)
                .Add("decal.scorch", new Color(0.05f, 0.04f, 0.025f, 0.50f), transparent: true, albedo: "Decals/Battlefield/scorch_mark_01.png")
                .Add("decal.rubble", new Color(0.34f, 0.31f, 0.25f, 0.76f), transparent: true)
                .Add("basepad.panel", new Color(0.84f, 0.83f, 0.78f, 1f))
                .Add("basepad.trim", new Color(0.70f, 0.60f, 0.39f, 1f))
                .Add("basepad.corner", new Color(0.78f, 0.77f, 0.72f, 1f))
                .Add("basepad.grime", new Color(0.12f, 0.09f, 0.06f, 0.42f), transparent: true)
                .Add("terrain.grass_mottle", new Color(0.38f, 0.31f, 0.18f, 0.28f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.dirt_mottle", new Color(0.56f, 0.40f, 0.24f, 0.38f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.gravel_mottle", new Color(0.35f, 0.31f, 0.25f, 0.42f), transparent: true, albedo: "Decals/Battlefield/rubble_scatter_01.png")
                .Add("terrain.wet_mud_detail", new Color(0.22f, 0.15f, 0.09f, 0.48f), transparent: true, albedo: "Decals/River/muddy_shoreline_01.png")
                .Add("terrain.blend_grass", new Color(0.40f, 0.31f, 0.18f, 0.22f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.blend_dirt", new Color(0.56f, 0.39f, 0.23f, 0.34f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("terrain.blend_gravel", new Color(0.36f, 0.31f, 0.25f, 0.28f), transparent: true, albedo: "Decals/Battlefield/rubble_scatter_01.png")
                .Add("terrain.blend_mud", new Color(0.22f, 0.15f, 0.09f, 0.30f), transparent: true, albedo: "Decals/River/muddy_shoreline_01.png")
                .Add("water.highlight", new Color(0.35f, 0.70f, 0.74f, 0.26f), transparent: true, albedo: "Decals/River/water_highlight_streaks.png")
                .Add("road.soft_dust", new Color(0.62f, 0.48f, 0.29f, 0.50f), transparent: true, albedo: "Decals/Roads/soft_dust_overlay.png")
                .Add("road.worn_edge", new Color(0.66f, 0.54f, 0.36f, 0.46f), transparent: true, albedo: "Decals/Roads/worn_path_edge_01.png")
                .Add("road.mud_track", new Color(0.28f, 0.18f, 0.10f, 0.48f), transparent: true, albedo: "Decals/Roads/mud_track_01.png")
                .Add("road.tire_left", new Color(0.18f, 0.13f, 0.08f, 0.44f), transparent: true, albedo: "Decals/Roads/tire_rut_left.png")
                .Add("road.tire_right", new Color(0.18f, 0.13f, 0.08f, 0.44f), transparent: true, albedo: "Decals/Roads/tire_rut_right.png")
                .Add("basepad.panel_decal", new Color(0.82f, 0.81f, 0.75f, 0.62f), transparent: true, albedo: "Decals/BasePads/base_pad_panel_center.png")
                .Add("basepad.trim_decal", new Color(0.70f, 0.58f, 0.36f, 0.66f), transparent: true, albedo: "Decals/BasePads/base_pad_trim_straight.png")
                .Add("basepad.crack", new Color(0.08f, 0.07f, 0.055f, 0.52f), transparent: true, albedo: "Decals/BasePads/concrete_crack_01.png")
                .Add("basepad.construction_wear", new Color(0.25f, 0.17f, 0.10f, 0.42f), transparent: true, albedo: "Decals/BasePads/construction_wear_01.png");
            ApplyProductionProxyTexturePaths(theme);
            return theme;
        }

        static void ApplyProductionProxyTexturePaths(AegisMapVisualTheme theme)
        {
            SetTerrainTexture(theme, "terrain.grass", "forest_grass");
            SetTerrainTexture(theme, "terrain.dark_grass", "forest_grass_dark_patch");
            SetTerrainTexture(theme, "terrain.dirt", "dirt_path");
            SetTerrainTexture(theme, "terrain.gravel", "gravel_path");
            SetTerrainTexture(theme, "terrain.mud", "muddy_bank");
            SetTerrainTexture(theme, "terrain.shallow_water", "shallow_water");
            SetTerrainTexture(theme, "terrain.deep_water", "deep_water");
            SetTerrainTexture(theme, "terrain.cliff_ground", "cliff_ground");
            SetTerrainTexture(theme, "terrain.ore_stained_soil", "ore_stained_soil");
            SetTerrainTexture(theme, "terrain.concrete_base_pad", "concrete_base_pad");
            SetTerrainTexture(theme, "road.dirt", "dirt_path");
            SetTerrainTexture(theme, "road.gravel", "gravel_path");
            SetTerrainTexture(theme, "river.water", "shallow_water");
            SetDecalTexture(theme, "river.shoreline", "Decals/River/muddy_shoreline_01.png");
            SetDecalTexture(theme, "river.shoreline_feather", "Decals/River/muddy_shoreline_02.png");
            SetTerrainTexture(theme, "cliff.edge.straight", "cliff_ground");
            SetTerrainTexture(theme, "cliff.edge.corner_inner", "cliff_ground");
            SetTerrainTexture(theme, "cliff.edge.corner_outer", "cliff_ground");
            SetTerrainTexture(theme, "cliff.edge.endcap", "cliff_ground");
            SetTerrainTexture(theme, "blocker.rock", "rough_ground");
            SetTerrainTexture(theme, "basepad.panel", "concrete_panel");
            SetTerrainTexture(theme, "basepad.trim", "concrete_trim");
            SetTerrainTexture(theme, "basepad.corner", "concrete_trim");
        }

        static void SetTerrainTexture(AegisMapVisualTheme theme, string role, string textureBaseName)
        {
            var rule = theme.RuleFor(role);
            if (rule == null)
                return;

            rule.AlbedoPath = "Terrain/" + textureBaseName + "_albedo.png";
            rule.NormalPath = "Terrain/" + textureBaseName + "_normal.png";
            rule.MaskPath = "Terrain/" + textureBaseName + "_roughness_ao.png";
        }

        static void SetDecalTexture(AegisMapVisualTheme theme, string role, string texturePath)
        {
            var rule = theme.RuleFor(role);
            if (rule == null)
                return;

            rule.AlbedoPath = texturePath;
            rule.NormalPath = null;
            rule.MaskPath = null;
        }

        static AegisMapVisualTheme BaseTheme(string id, string displayName, string biome)
        {
            return new AegisMapVisualTheme
            {
                ThemeId = id,
                DisplayName = displayName,
                Biome = biome,
                PrototypeOnly = true
            };
        }
    }
}
