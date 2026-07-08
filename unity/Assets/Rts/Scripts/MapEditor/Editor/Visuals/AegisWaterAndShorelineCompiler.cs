#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisWaterAndShorelineCompiler : IAegisVisualLayerCompiler
    {
        static readonly int[] DirX = { 1, -1, 0, 0 };
        static readonly int[] DirY = { 0, 0, 1, -1 };

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Water And Shoreline");
            var waterLayer = AegisVisualCompilerPrimitives.CreateLayer(context, "Water Surface");
            var shoreLayer = AegisVisualCompilerPrimitives.CreateLayer(context, "Shoreline Mud And Wetness");
            var waterMaterial = AegisVisualCompilerPrimitives.Material(context, "river.water");
            var shoreMaterial = AegisVisualCompilerPrimitives.Material(context, "river.shoreline");

            for (var y = 0; y < context.Height; y++)
                for (var x = 0; x < context.Width; x++)
                {
                    if (!context.IsWater(x, y))
                        continue;

                    AegisVisualCompilerPrimitives.CreateQuad(waterLayer, "water_cell_" + x + "_" + y, new Vector2(x + 0.5f, y + 0.5f), 1.08f, 1.08f, 0.03f, waterMaterial, 0f);
                    summary.WaterCells++;

                    for (var i = 0; i < DirX.Length; i++)
                    {
                        var nx = x + DirX[i];
                        var ny = y + DirY[i];
                        if (!context.InBounds(nx, ny) || context.IsWater(nx, ny))
                            continue;

                        var center = new Vector2(x + 0.5f + DirX[i] * 0.48f, y + 0.5f + DirY[i] * 0.48f);
                        var width = DirX[i] == 0 ? 1.18f : 0.62f;
                        var height = DirY[i] == 0 ? 1.18f : 0.62f;
                        AegisVisualCompilerPrimitives.CreateQuad(shoreLayer, "shoreline_" + x + "_" + y + "_" + i, center, width, height, 0.04f, shoreMaterial, 0f);
                        summary.ShorelineEdges++;
                    }
                }

            return summary;
        }
    }
}
#endif
