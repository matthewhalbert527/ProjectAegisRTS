#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisCliffTopologyCompiler : IAegisVisualLayerCompiler
    {
        const int MaxCliffPieces = 1800;

        static readonly int[] DirX = { 1, 0, -1, 0 };
        static readonly int[] DirY = { 0, 1, 0, -1 };
        static readonly float[] DirAngle = { 0f, 90f, 180f, 270f };

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Topology Driven Cliffs");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Topology Driven Cliffs");
            var straightMaterial = AegisVisualCompilerPrimitives.Material(context, "cliff.edge.straight");
            var innerMaterial = AegisVisualCompilerPrimitives.Material(context, "cliff.edge.corner_inner");
            var outerMaterial = AegisVisualCompilerPrimitives.Material(context, "cliff.edge.corner_outer");
            var endcapMaterial = AegisVisualCompilerPrimitives.Material(context, "cliff.edge.endcap");
            var blockerMaterial = AegisVisualCompilerPrimitives.Material(context, "blocker.rock");
            var placed = 0;

            for (var y = 0; y < context.Height; y++)
            {
                for (var x = 0; x < context.Width; x++)
                {
                    if (!context.IsCliffLike(x, y))
                        continue;

                    if (context.IsStartProtected(x, y))
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    var exposedCount = 0;
                    var exposedMask = 0;
                    for (var d = 0; d < DirX.Length; d++)
                    {
                        var nx = x + DirX[d];
                        var ny = y + DirY[d];
                        var exposed = !context.InBounds(nx, ny) || !context.IsCliffLike(nx, ny);
                        if (!exposed)
                            continue;

                        exposedCount++;
                        exposedMask |= 1 << d;
                        if (placed >= MaxCliffPieces)
                        {
                            summary.SkippedPlacementCount++;
                            continue;
                        }

                        var center = new Vector3(x + 0.5f + DirX[d] * 0.42f, 0.62f, y + 0.5f + DirY[d] * 0.42f);
                        var rotation = Quaternion.Euler(0f, DirAngle[d], 0f);
                        var prefabPath = AegisMapArtPack.Pick(AegisMapArtPack.CliffMeshes, context.Seed, x + d * 11, y);
                        if (!AegisMapArtPack.TryInstantiatePrefab(layer, "cliff_straight_" + x + "_" + y + "_" + d, prefabPath, center, rotation, new Vector3(0.9f, 0.75f, 0.9f), straightMaterial))
                            AegisVisualCompilerPrimitives.CreateCube(layer, "cliff_straight_" + x + "_" + y + "_" + d, center, new Vector3(0.88f, 1.08f, 0.30f), rotation, straightMaterial);
                        summary.CliffStraightSegments++;
                        placed++;
                    }

                    if (placed >= MaxCliffPieces)
                        continue;

                    if (exposedCount == 1)
                    {
                        var direction = FirstDirection(exposedMask);
                        var center = new Vector3(x + 0.5f, 0.68f, y + 0.5f);
                        var rotation = Quaternion.Euler(0f, DirAngle[direction], 0f);
                        if (!AegisMapArtPack.TryInstantiatePrefab(layer, "cliff_endcap_" + x + "_" + y, "Meshes/Cliffs/cliff_endcap_01.glb", center, rotation, Vector3.one, endcapMaterial))
                            AegisVisualCompilerPrimitives.CreateCube(layer, "cliff_endcap_" + x + "_" + y, center, new Vector3(0.78f, 0.86f, 0.78f), rotation, endcapMaterial);
                        summary.CliffEndcaps++;
                        placed++;
                    }
                    else if (HasAdjacentExposedEdges(exposedMask))
                    {
                        var roleMaterial = exposedCount >= 3 ? outerMaterial : innerMaterial;
                        var center = new Vector3(x + 0.5f, 0.70f, y + 0.5f);
                        var rotation = Quaternion.Euler(0f, CornerAngle(exposedMask), 0f);
                        var prefabPath = exposedCount >= 3 ? "Meshes/Cliffs/cliff_corner_outer_01.glb" : "Meshes/Cliffs/cliff_corner_inner_01.glb";
                        if (!AegisMapArtPack.TryInstantiatePrefab(layer, "cliff_corner_" + x + "_" + y, prefabPath, center, rotation, Vector3.one, roleMaterial))
                            AegisVisualCompilerPrimitives.CreateCube(layer, "cliff_corner_" + x + "_" + y, center, new Vector3(0.92f, 0.96f, 0.92f), rotation, roleMaterial);
                        summary.CliffCorners++;
                        placed++;
                    }
                    else if (!context.IsStartProtected(x, y) && context.Hash01(x, y, 707) < 0.08f)
                    {
                        AegisVisualCompilerPrimitives.CreateCube(layer, "cliff_blocker_core_" + x + "_" + y, context.CellCenter(x, y, 0.35f), new Vector3(0.55f, 0.70f, 0.55f), Quaternion.Euler(0f, context.Hash01(x, y, 708) * 360f, 0f), blockerMaterial);
                    }
                }
            }

            if (placed >= MaxCliffPieces)
                summary.AddWarning("Cliff compiler hit the deterministic piece cap; remaining cliff cells were summarized but not all rendered.");

            return summary;
        }

        static int FirstDirection(int mask)
        {
            for (var i = 0; i < 4; i++)
                if ((mask & (1 << i)) != 0)
                    return i;
            return 0;
        }

        static bool HasAdjacentExposedEdges(int mask)
        {
            return (mask & 0x3) == 0x3 ||
                   (mask & 0x6) == 0x6 ||
                   (mask & 0xC) == 0xC ||
                   (mask & 0x9) == 0x9;
        }

        static float CornerAngle(int mask)
        {
            if ((mask & 0x3) == 0x3)
                return 45f;
            if ((mask & 0x6) == 0x6)
                return 135f;
            if ((mask & 0xC) == 0xC)
                return 225f;
            if ((mask & 0x9) == 0x9)
                return 315f;
            return DirAngle[FirstDirection(mask)];
        }
    }
}
#endif
