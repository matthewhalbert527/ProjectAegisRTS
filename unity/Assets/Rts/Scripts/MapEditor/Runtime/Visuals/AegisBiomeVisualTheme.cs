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
                .Add("road.dirt", new Color(0.47f, 0.36f, 0.25f, 0.80f), transparent: true)
                .Add("road.gravel", new Color(0.48f, 0.46f, 0.40f, 0.66f), transparent: true)
                .Add("river.water", new Color(0.07f, 0.27f, 0.34f, 0.90f), transparent: true)
                .Add("river.shoreline", new Color(0.26f, 0.18f, 0.12f, 0.62f), transparent: true)
                .Add("cliff.edge.straight", new Color(0.36f, 0.36f, 0.34f, 1f))
                .Add("cliff.edge.corner_inner", new Color(0.30f, 0.30f, 0.28f, 1f))
                .Add("cliff.edge.corner_outer", new Color(0.42f, 0.42f, 0.39f, 1f))
                .Add("cliff.edge.endcap", new Color(0.32f, 0.32f, 0.30f, 1f))
                .Add("blocker.rock", new Color(0.35f, 0.35f, 0.32f, 1f))
                .Add("resource.ore", new Color(0.96f, 0.70f, 0.20f, 1f))
                .Add("resource.crystal", new Color(0.35f, 0.88f, 0.95f, 1f))
                .Add("resource.salvage", new Color(0.45f, 0.47f, 0.43f, 1f))
                .Add("resource.energy", new Color(0.40f, 0.65f, 1.00f, 1f))
                .Add("vegetation.tree", new Color(0.10f, 0.28f, 0.10f, 1f))
                .Add("vegetation.bush", new Color(0.16f, 0.38f, 0.15f, 1f))
                .Add("vegetation.grass", new Color(0.25f, 0.50f, 0.20f, 1f))
                .Add("decal.crater", new Color(0.06f, 0.05f, 0.04f, 0.82f), transparent: true)
                .Add("decal.scorch", new Color(0.04f, 0.035f, 0.025f, 0.68f), transparent: true)
                .Add("decal.rubble", new Color(0.28f, 0.27f, 0.25f, 0.76f), transparent: true)
                .Add("basepad.panel", new Color(0.84f, 0.86f, 0.82f, 1f))
                .Add("basepad.trim", new Color(0.70f, 0.63f, 0.42f, 1f))
                .Add("basepad.corner", new Color(0.78f, 0.80f, 0.76f, 1f))
                .Add("basepad.grime", new Color(0.10f, 0.09f, 0.07f, 0.42f), transparent: true);
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
                .Add("road.dirt", new Color(0.57f, 0.43f, 0.26f, 0.80f), transparent: true)
                .Add("road.gravel", new Color(0.52f, 0.47f, 0.39f, 0.66f), transparent: true)
                .Add("river.water", new Color(0.08f, 0.31f, 0.36f, 0.88f), transparent: true)
                .Add("river.shoreline", new Color(0.35f, 0.25f, 0.15f, 0.62f), transparent: true)
                .Add("cliff.edge.straight", new Color(0.46f, 0.41f, 0.34f, 1f))
                .Add("cliff.edge.corner_inner", new Color(0.38f, 0.34f, 0.28f, 1f))
                .Add("cliff.edge.corner_outer", new Color(0.52f, 0.46f, 0.38f, 1f))
                .Add("cliff.edge.endcap", new Color(0.42f, 0.37f, 0.31f, 1f))
                .Add("blocker.rock", new Color(0.47f, 0.42f, 0.35f, 1f))
                .Add("resource.ore", new Color(0.96f, 0.68f, 0.18f, 1f))
                .Add("resource.crystal", new Color(0.34f, 0.82f, 0.90f, 1f))
                .Add("resource.salvage", new Color(0.46f, 0.45f, 0.40f, 1f))
                .Add("resource.energy", new Color(0.42f, 0.66f, 0.98f, 1f))
                .Add("vegetation.tree", new Color(0.25f, 0.30f, 0.13f, 1f))
                .Add("vegetation.bush", new Color(0.28f, 0.34f, 0.16f, 1f))
                .Add("vegetation.grass", new Color(0.36f, 0.40f, 0.19f, 1f))
                .Add("decal.crater", new Color(0.08f, 0.06f, 0.04f, 0.82f), transparent: true)
                .Add("decal.scorch", new Color(0.05f, 0.04f, 0.025f, 0.68f), transparent: true)
                .Add("decal.rubble", new Color(0.34f, 0.31f, 0.25f, 0.76f), transparent: true)
                .Add("basepad.panel", new Color(0.84f, 0.83f, 0.78f, 1f))
                .Add("basepad.trim", new Color(0.70f, 0.60f, 0.39f, 1f))
                .Add("basepad.corner", new Color(0.78f, 0.77f, 0.72f, 1f))
                .Add("basepad.grime", new Color(0.12f, 0.09f, 0.06f, 0.42f), transparent: true);
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
            SetTerrainTexture(theme, "river.shoreline", "muddy_bank");
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
