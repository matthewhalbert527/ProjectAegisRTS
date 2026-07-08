#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisTerrainDetailOverlayCompiler : IAegisVisualLayerCompiler
    {
        const int SampleStride = 2;
        const int MaxDetailDecals = 4200;
        const float ExpectedPlacementPressure = 0.78f;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Production Terrain Detail Decals");
            if (context.IsDebugOverlay)
                return summary;

            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Production Terrain Detail Decals");
            var grassMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.grass_mottle");
            var grassMicroMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.grass_micro_mottle");
            var dryGrassMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.dry_grass_mottle");
            var surfaceShadowMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.surface_shadow_mottle");
            var dirtMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.dirt_mottle");
            var dirtPebbleMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.dirt_pebble_mottle");
            var gravelMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.gravel_mottle");
            var wetMud = AegisVisualCompilerPrimitives.Material(context, "terrain.wet_mud_detail");
            var waterHighlight = AegisVisualCompilerPrimitives.Material(context, "water.highlight");

            var densityScale = DetailDensityScale(context);
            var placed = 0;
            for (var y = 2; y < context.Height - 2; y += SampleStride)
            {
                var rowOffset = ((y / SampleStride) & 1);
                for (var x = 2 + rowOffset; x < context.Width - 2; x += SampleStride)
                {
                    if (placed >= MaxDetailDecals)
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    if (context.IsWater(x, y))
                    {
                        if (Chance(context, x, y, 2320, 0.13f, densityScale))
                        {
                            EmitDecal(layer, context, summary, "water_highlight", x, y, waterHighlight, 3.8f, 8.6f, 0.12f, 0.34f, 0.086f, 2321);
                            placed++;
                        }
                        continue;
                    }

                    if (context.IsBlocked(x, y) || context.IsStartProtected(x, y) || context.HasResource(x, y))
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    var roadNear = AegisVisualCompilerPrimitives.IsRoadNear(context, x, y, 4.5f);
                    if (roadNear && IsNearWater(context, x, y, 3) && Chance(context, x, y, 2380, 0.66f, densityScale))
                    {
                        EmitDecal(layer, context, summary, "wet_road_bank_smear", x, y, wetMud, 3.0f, 7.2f, 0.24f, 0.68f, 0.081f, 2381);
                        summary.ShorelineDetailDecalCount++;
                        placed++;
                        continue;
                    }

                    if (IsNearWater(context, x, y, 2) && Chance(context, x, y, 2330, 0.54f, densityScale))
                    {
                        EmitDecal(layer, context, summary, "wet_bank_mottle", x, y, wetMud, 2.2f, 5.8f, 0.42f, 1.0f, 0.079f, 2331);
                        summary.ShorelineDetailDecalCount++;
                        placed++;
                        continue;
                    }

                    if (roadNear && Chance(context, x, y, 2340, 0.26f, densityScale))
                    {
                        EmitDecal(layer, context, summary, "roadside_dust_mottle", x, y, dirtMottle, 2.4f, 6.5f, 0.38f, 1.0f, 0.078f, 2341);
                        placed++;
                        continue;
                    }

                    var role = context.TerrainRoleAt(x, y);
                    if (IsGrassRole(role))
                    {
                        if (Chance(context, x, y, 2350, 0.26f, densityScale))
                            placed += TryEmitDecal(layer, context, summary, "grass_mottle", x, y, grassMottle, 0.85f, 2.45f, 0.42f, 1.0f, 0.073f, 2351, placed);
                        if (Chance(context, x, y, 2390, 0.72f, densityScale))
                            placed += TryEmitDecal(layer, context, summary, "grass_micro_mottle", x, y, grassMicroMottle, 0.24f, 0.82f, 0.18f, 0.48f, 0.092f, 2391, placed);
                        if (Chance(context, x, y, 2400, role == "terrain.dark_grass" ? 0.12f : 0.24f, densityScale))
                            placed += TryEmitDecal(layer, context, summary, "dry_grass_mottle", x, y, dryGrassMottle, 0.32f, 1.18f, 0.22f, 0.58f, 0.089f, 2401, placed);
                        if (Chance(context, x, y, 2410, 0.055f, densityScale))
                            placed += TryEmitDecal(layer, context, summary, "grass_surface_shadow", x, y, surfaceShadowMottle, 0.95f, 2.55f, 0.46f, 1.0f, 0.070f, 2411, placed);
                        continue;
                    }

                    if (role == "terrain.dirt" || role == "terrain.mud")
                    {
                        if (Chance(context, x, y, 2360, 0.38f, densityScale))
                            placed += TryEmitDecal(layer, context, summary, "dirt_mottle", x, y, dirtMottle, 1.0f, 3.2f, 0.42f, 1.0f, 0.074f, 2361, placed);
                        if (Chance(context, x, y, 2420, 0.32f, densityScale))
                            placed += TryEmitDecal(layer, context, summary, "dirt_pebble_mottle", x, y, dirtPebbleMottle, 0.35f, 1.15f, 0.40f, 1.0f, 0.091f, 2421, placed);
                        continue;
                    }

                    if ((role == "terrain.gravel" || role == "terrain.cliff_ground") && Chance(context, x, y, 2370, 0.38f, densityScale))
                    {
                        EmitDecal(layer, context, summary, "gravel_mottle", x, y, gravelMottle, 1.5f, 4.2f, 0.50f, 1.0f, 0.075f, 2371);
                        placed++;
                    }
                }
            }

            if (placed >= MaxDetailDecals)
                summary.AddWarning("Terrain detail decal compiler reached its deterministic placement cap.");

            return summary;
        }

        static int TryEmitDecal(Transform layer, AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, string prefix, int x, int y, Material material, float minWidth, float maxWidth, float minAspect, float maxAspect, float elevation, int salt, int alreadyPlaced)
        {
            if (alreadyPlaced >= MaxDetailDecals)
            {
                summary.SkippedPlacementCount++;
                return 0;
            }

            EmitDecal(layer, context, summary, prefix, x, y, material, minWidth, maxWidth, minAspect, maxAspect, elevation, salt);
            return 1;
        }

        static void EmitDecal(Transform layer, AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, string prefix, int x, int y, Material material, float minWidth, float maxWidth, float minAspect, float maxAspect, float elevation, int salt)
        {
            var width = Mathf.Lerp(minWidth, maxWidth, context.Hash01(x, y, salt));
            var aspect = Mathf.Lerp(minAspect, maxAspect, context.Hash01(x, y, salt + 1));
            var offsetX = (context.Hash01(x, y, salt + 2) - 0.5f) * SampleStride * 0.78f;
            var offsetY = (context.Hash01(x, y, salt + 3) - 0.5f) * SampleStride * 0.78f;
            var center = new Vector2(x + 0.5f + offsetX, y + 0.5f + offsetY);
            var angle = context.Hash01(x, y, salt + 4) * 180f;
            var height = width * aspect;
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, prefix + "_" + x + "_" + y, center, width, height, elevation, material, angle, context, x, y, salt + 83, Mathf.Min(width, height) * 0.16f);
            summary.TerrainDetailDecalCount++;
        }

        static bool Chance(AegisMapVisualCompileContext context, int x, int y, int salt, float chance, float densityScale)
        {
            return context.Hash01(x, y, salt) < chance * densityScale;
        }

        static float DetailDensityScale(AegisMapVisualCompileContext context)
        {
            var sampledCells = Mathf.Max(1f, (context.Width / (float)SampleStride) * (context.Height / (float)SampleStride));
            var desiredPressure = Mathf.Max(0.001f, ExpectedPlacementPressure);
            return Mathf.Clamp(MaxDetailDecals / (sampledCells * desiredPressure), 0.14f, 1f);
        }

        static bool IsGrassRole(string role)
        {
            return role == "terrain.grass" || role == "terrain.dark_grass";
        }

        static bool IsNearWater(AegisMapVisualCompileContext context, int x, int y, int radius)
        {
            for (var dy = -radius; dy <= radius; dy++)
                for (var dx = -radius; dx <= radius; dx++)
                    if (context.IsWater(x + dx, y + dy))
                        return true;
            return false;
        }
    }
}
#endif
