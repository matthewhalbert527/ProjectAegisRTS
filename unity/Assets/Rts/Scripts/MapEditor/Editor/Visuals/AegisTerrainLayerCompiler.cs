#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisTerrainLayerCompiler : IAegisVisualLayerCompiler
    {
        public const int ProductionChunkSize = 4;
        const int DebugChunkSize = 16;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary(context.IsDebugOverlay ? "Debug Terrain Role Chunks" : "Production Terrain Surface");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, context.IsDebugOverlay ? "Debug Terrain Role Chunks" : "Production Terrain Surface");
            var chunkSize = context.IsDebugOverlay ? DebugChunkSize : ProductionChunkSize;

            for (var y = 0; y < context.Height; y += chunkSize)
            {
                for (var x = 0; x < context.Width; x += chunkSize)
                {
                    var width = Mathf.Min(chunkSize, context.Width - x);
                    var height = Mathf.Min(chunkSize, context.Height - y);
                    if (!context.IsDebugOverlay && IsMixed(context, x, y, width, height))
                    {
                        summary.MixedTerrainChunks++;
                        EmitMixedChunk(context, layer, summary, x, y, width, height);
                    }
                    else
                    {
                        EmitPatch(context, layer, summary, x, y, width, height, DominantRole(context, x, y, width, height), "terrain_chunk");
                    }
                }
            }

            return summary;
        }

        static void EmitMixedChunk(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int startX, int startY, int width, int height)
        {
            for (var y = startY; y < startY + height; y++)
                for (var x = startX; x < startX + width; x++)
                    EmitPatch(context, layer, summary, x, y, 1, 1, context.TerrainRoleAt(x, y), "terrain_cell");
        }

        static void EmitPatch(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int startX, int startY, int width, int height, string role, string prefix)
        {
            if (!context.IsDebugOverlay && IsWaterRole(role))
            {
                summary.HiddenDebugFillCount++;
                return;
            }

            var surfaceRole = context.IsDebugOverlay ? role : ProductionSurfaceRole(context, startX, startY, width, height, role);
            var material = AegisVisualCompilerPrimitives.Material(context, surfaceRole);
            var center = new Vector2(startX + width * 0.5f, startY + height * 0.5f);
            var elevation = role == "terrain.shallow_water" || role == "terrain.deep_water" ? 0.015f : 0f;
            var overlap = context.IsDebugOverlay ? 1f : 1.035f;
            var chunk = AegisVisualCompilerPrimitives.CreateQuad(
                layer,
                prefix + "_" + startX + "_" + startY + "_" + surfaceRole.Replace('.', '_'),
                center,
                width * overlap,
                height * overlap,
                elevation,
                material,
                0f);
            chunk.isStatic = true;
            summary.TerrainChunks++;
        }

        static string ProductionSurfaceRole(AegisMapVisualCompileContext context, int startX, int startY, int width, int height, string role)
        {
            if (role == "terrain.gravel")
            {
                if (PatchNearWater(context, startX, startY, width, height, 2))
                    return "terrain.dirt";
                if (PatchNearRoad(context, startX, startY, width, height))
                    return "terrain.dirt";
                if (IsSparseRoughCell(context, startX, startY, width, height))
                    return context.Hash01(startX, startY, 7320) < 0.35f ? "terrain.dark_grass" : "terrain.grass";
                return role;
            }

            if (role != "terrain.cliff_ground")
                return role;

            if (PatchNearWater(context, startX, startY, width, height, 2))
                return "terrain.dirt";

            if (PatchNearRoad(context, startX, startY, width, height))
                return "terrain.gravel";

            if (IsSparseRoughCell(context, startX, startY, width, height))
                return context.Hash01(startX, startY, 7330) < 0.50f ? "terrain.gravel" : "terrain.dirt";

            return context.Hash01(startX, startY, 7310) < 0.58f ? "terrain.gravel" : "terrain.dirt";
        }

        static bool IsSparseRoughCell(AegisMapVisualCompileContext context, int startX, int startY, int width, int height)
        {
            if (width != 1 || height != 1)
                return false;

            var softNeighbors = 0;
            var roughNeighbors = 0;
            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    var role = context.TerrainRoleAt(startX + dx, startY + dy);
                    if (role == "terrain.grass" || role == "terrain.dark_grass" || role == "terrain.dirt")
                        softNeighbors++;
                    if (role == "terrain.gravel" || role == "terrain.cliff_ground")
                        roughNeighbors++;
                }
            }

            return softNeighbors >= 4 && roughNeighbors <= 3;
        }

        static bool PatchNearWater(AegisMapVisualCompileContext context, int startX, int startY, int width, int height, int radius)
        {
            for (var y = startY - radius; y < startY + height + radius; y++)
                for (var x = startX - radius; x < startX + width + radius; x++)
                    if (context.IsWater(x, y))
                        return true;
            return false;
        }

        static bool PatchNearRoad(AegisMapVisualCompileContext context, int startX, int startY, int width, int height)
        {
            var center = new Vector2(startX + width * 0.5f, startY + height * 0.5f);
            for (var i = 0; i < context.RoadSegments.Count; i++)
            {
                var segment = context.RoadSegments[i];
                if (AegisVisualCompilerPrimitives.DistanceToSegment(center, segment.A, segment.B) <= Mathf.Max(width, height) + 2.2f)
                    return true;
            }

            return false;
        }

        static bool IsWaterRole(string role)
        {
            return role == "terrain.shallow_water" || role == "terrain.deep_water";
        }

        static bool IsMixed(AegisMapVisualCompileContext context, int startX, int startY, int width, int height)
        {
            var first = context.TerrainRoleAt(startX, startY);
            for (var y = startY; y < startY + height; y++)
                for (var x = startX; x < startX + width; x++)
                    if (context.TerrainRoleAt(x, y) != first)
                        return true;
            return false;
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
