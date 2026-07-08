#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisWaterAndShorelineCompiler : IAegisVisualLayerCompiler
    {
        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Water And Shoreline");
            var waterLayer = AegisVisualCompilerPrimitives.CreateLayer(context, "Water Surface");
            var shoreLayer = AegisVisualCompilerPrimitives.CreateLayer(context, "Shoreline Mud And Wetness");
            var waterMaterial = AegisVisualCompilerPrimitives.Material(context, "river.water");
            var shoreMaterial = AegisVisualCompilerPrimitives.Material(context, "river.shoreline");
            var riverPropMaterial = AegisVisualCompilerPrimitives.Material(context, "vegetation.grass");

            EmitMergedWater(context, summary, waterLayer, waterMaterial);
            EmitMergedShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial);
            return summary;
        }

        static void EmitMergedWater(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform waterLayer, Material waterMaterial)
        {
            for (var y = 0; y < context.Height; y++)
            {
                var x = 0;
                while (x < context.Width)
                {
                    if (!context.IsWater(x, y))
                    {
                        x++;
                        continue;
                    }

                    var startX = x;
                    while (x < context.Width && context.IsWater(x, y))
                    {
                        summary.WaterCells++;
                        x++;
                    }

                    var width = x - startX;
                    var center = new Vector2(startX + width * 0.5f, y + 0.5f);
                    AegisVisualCompilerPrimitives.CreateQuad(waterLayer, "water_strip_" + startX + "_" + y + "_" + width, center, width + 0.34f, 1.34f, 0.026f, waterMaterial, 0f);
                    summary.WaterStrips++;
                }
            }
        }

        static void EmitMergedShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material riverPropMaterial)
        {
            EmitHorizontalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, true);
            EmitHorizontalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, false);
            EmitVerticalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, true);
            EmitVerticalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, false);
        }

        static void EmitHorizontalShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material riverPropMaterial, bool north)
        {
            for (var y = 0; y < context.Height; y++)
            {
                var x = 0;
                while (x < context.Width)
                {
                    if (!IsHorizontalShore(context, x, y, north))
                    {
                        x++;
                        continue;
                    }

                    var startX = x;
                    while (x < context.Width && IsHorizontalShore(context, x, y, north))
                        x++;

                    var width = x - startX;
                    var centerY = north ? y + 1.03f : y - 0.03f;
                    var center = new Vector2(startX + width * 0.5f, centerY);
                    AegisVisualCompilerPrimitives.CreateQuad(shoreLayer, "shoreline_h_" + startX + "_" + y + "_" + north, center, width + 1.1f, 0.86f, 0.044f, shoreMaterial, 0f);
                    summary.ShorelineEdges++;
                    MaybeEmitRiverProp(context, shoreLayer, summary, riverPropMaterial, Mathf.RoundToInt(center.x), y, center);
                }
            }
        }

        static void EmitVerticalShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material riverPropMaterial, bool east)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var y = 0;
                while (y < context.Height)
                {
                    if (!IsVerticalShore(context, x, y, east))
                    {
                        y++;
                        continue;
                    }

                    var startY = y;
                    while (y < context.Height && IsVerticalShore(context, x, y, east))
                        y++;

                    var height = y - startY;
                    var centerX = east ? x + 1.03f : x - 0.03f;
                    var center = new Vector2(centerX, startY + height * 0.5f);
                    AegisVisualCompilerPrimitives.CreateQuad(shoreLayer, "shoreline_v_" + x + "_" + startY + "_" + east, center, 0.86f, height + 1.1f, 0.045f, shoreMaterial, 0f);
                    summary.ShorelineEdges++;
                    MaybeEmitRiverProp(context, shoreLayer, summary, riverPropMaterial, x, Mathf.RoundToInt(center.y), center);
                }
            }
        }

        static bool IsHorizontalShore(AegisMapVisualCompileContext context, int x, int y, bool north)
        {
            if (!context.IsWater(x, y))
                return false;
            var ny = north ? y + 1 : y - 1;
            return !context.InBounds(x, ny) || !context.IsWater(x, ny);
        }

        static bool IsVerticalShore(AegisMapVisualCompileContext context, int x, int y, bool east)
        {
            if (!context.IsWater(x, y))
                return false;
            var nx = east ? x + 1 : x - 1;
            return !context.InBounds(nx, y) || !context.IsWater(nx, y);
        }

        static void MaybeEmitRiverProp(AegisMapVisualCompileContext context, Transform shoreLayer, AegisVisualLayerSummary summary, Material riverPropMaterial, int x, int y, Vector2 center)
        {
            if (context.Hash01(x, y, 4010) >= 0.12f)
                return;

            var propPosition = new Vector3(center.x, 0.08f, center.y);
            var rotation = Quaternion.Euler(0f, context.Hash01(x, y, 4020) * 360f, 0f);
            var scale = Vector3.one * Mathf.Lerp(0.55f, 1.0f, context.Hash01(x, y, 4030));
            var prefabPath = AegisMapArtPack.Pick(AegisMapArtPack.RiverMeshes, context.Seed, x, y);
            if (AegisMapArtPack.TryInstantiatePrefab(shoreLayer, "river_bank_prop_" + x + "_" + y, prefabPath, propPosition, rotation, scale, riverPropMaterial))
                summary.ScatterCount++;
            else
                summary.SkippedPlacementCount++;
        }
    }
}
#endif
