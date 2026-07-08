#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisTerrainLayerCompiler : IAegisVisualLayerCompiler
    {
        const int ChunkSize = 16;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Base Terrain Surface");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Base Terrain Surface");

            for (var y = 0; y < context.Height; y += ChunkSize)
            {
                for (var x = 0; x < context.Width; x += ChunkSize)
                {
                    var width = Mathf.Min(ChunkSize, context.Width - x);
                    var height = Mathf.Min(ChunkSize, context.Height - y);
                    var role = DominantRole(context, x, y, width, height);
                    var material = AegisVisualCompilerPrimitives.Material(context, role);
                    var center = new Vector2(x + width * 0.5f, y + height * 0.5f);
                    var chunk = AegisVisualCompilerPrimitives.CreateQuad(layer, "terrain_chunk_" + x + "_" + y + "_" + role.Replace('.', '_'), center, width, height, 0f, material, 0f);
                    chunk.isStatic = true;
                    summary.TerrainChunks++;
                }
            }

            return summary;
        }

        static string DominantRole(AegisMapVisualCompileContext context, int startX, int startY, int width, int height)
        {
            var counts = new Dictionary<string, int>();
            for (var y = startY; y < startY + height; y++)
                for (var x = startX; x < startX + width; x++)
                {
                    var role = context.TerrainRoleAt(x, y);
                    int count;
                    counts.TryGetValue(role, out count);
                    counts[role] = count + 1;
                }

            var bestRole = "terrain.grass";
            var bestCount = -1;
            foreach (var pair in counts)
            {
                if (pair.Value > bestCount)
                {
                    bestRole = pair.Key;
                    bestCount = pair.Value;
                }
            }

            return bestRole;
        }
    }
}
#endif
