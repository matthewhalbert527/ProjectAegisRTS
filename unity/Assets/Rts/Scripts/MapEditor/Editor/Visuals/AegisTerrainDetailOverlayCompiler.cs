#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisTerrainDetailOverlayCompiler : IAegisVisualLayerCompiler
    {
        const int SampleStride = 3;
        const int MaxDetailDecals = 1700;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Production Terrain Detail Decals");
            if (context.IsDebugOverlay)
                return summary;

            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Production Terrain Detail Decals");
            var grassMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.grass_mottle");
            var dirtMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.dirt_mottle");
            var gravelMottle = AegisVisualCompilerPrimitives.Material(context, "terrain.gravel_mottle");
            var wetMud = AegisVisualCompilerPrimitives.Material(context, "terrain.wet_mud_detail");
            var waterHighlight = AegisVisualCompilerPrimitives.Material(context, "water.highlight");

            var placed = 0;
            for (var y = 2; y < context.Height - 2; y += SampleStride)
            {
                var rowOffset = ((y / SampleStride) & 1) * 2;
                for (var x = 2 + rowOffset; x < context.Width - 2; x += SampleStride)
                {
                    if (placed >= MaxDetailDecals)
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    if (context.IsWater(x, y))
                    {
                        if (context.Hash01(x, y, 2320) < 0.11f)
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

                    if (IsNearWater(context, x, y, 2) && context.Hash01(x, y, 2330) < 0.48f)
                    {
                        EmitDecal(layer, context, summary, "wet_bank_mottle", x, y, wetMud, 2.2f, 5.8f, 0.42f, 1.0f, 0.079f, 2331);
                        summary.ShorelineDetailDecalCount++;
                        placed++;
                        continue;
                    }

                    var roadNear = AegisVisualCompilerPrimitives.IsRoadNear(context, x, y, 4.5f);
                    if (roadNear && context.Hash01(x, y, 2340) < 0.22f)
                    {
                        EmitDecal(layer, context, summary, "roadside_dust_mottle", x, y, dirtMottle, 2.4f, 6.5f, 0.38f, 1.0f, 0.078f, 2341);
                        placed++;
                        continue;
                    }

                    var role = context.TerrainRoleAt(x, y);
                    if ((role == "terrain.grass" || role == "terrain.dark_grass") && context.Hash01(x, y, 2350) < 0.34f)
                    {
                        EmitDecal(layer, context, summary, "grass_mottle", x, y, grassMottle, 1.4f, 3.8f, 0.46f, 1.0f, 0.073f, 2351);
                        placed++;
                        continue;
                    }

                    if ((role == "terrain.dirt" || role == "terrain.mud") && context.Hash01(x, y, 2360) < 0.40f)
                    {
                        EmitDecal(layer, context, summary, "dirt_mottle", x, y, dirtMottle, 1.8f, 5.2f, 0.42f, 1.0f, 0.074f, 2361);
                        placed++;
                        continue;
                    }

                    if ((role == "terrain.gravel" || role == "terrain.cliff_ground") && context.Hash01(x, y, 2370) < 0.34f)
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
