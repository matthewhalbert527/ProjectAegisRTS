#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisScatterVisualCompiler : IAegisVisualLayerCompiler
    {
        const int MaxScatter = 1250;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Rule Based Scatter");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Rule Based Scatter");
            var treeMaterial = AegisVisualCompilerPrimitives.Material(context, "vegetation.tree");
            var bushMaterial = AegisVisualCompilerPrimitives.Material(context, "vegetation.bush");
            var grassMaterial = AegisVisualCompilerPrimitives.Material(context, "vegetation.grass");
            var rockMaterial = AegisVisualCompilerPrimitives.Material(context, "blocker.rock");
            var craterMaterial = AegisVisualCompilerPrimitives.Material(context, "decal.crater");
            var rubbleMaterial = AegisVisualCompilerPrimitives.Material(context, "decal.rubble");
            var placed = 0;

            for (var y = 1; y < context.Height - 1; y += 2)
            {
                for (var x = 1; x < context.Width - 1; x += 2)
                {
                    if (placed >= MaxScatter)
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    if (context.IsStartProtected(x, y) || context.IsWater(x, y) || context.HasResource(x, y))
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    if (AegisVisualCompilerPrimitives.IsRoadNear(context, x, y, 2.1f))
                    {
                        if (context.Hash01(x, y, 1501) < 0.025f)
                        {
                            CreateScatterCube(layer, context, x, y, "road_edge_rubble", rubbleMaterial, 0.18f, 0.42f, 1502);
                            placed++;
                            summary.ScatterCount++;
                        }
                        else
                        {
                            summary.SkippedPlacementCount++;
                        }
                        continue;
                    }

                    var role = context.TerrainRoleAt(x, y);
                    var cliffNear = IsNearCliff(context, x, y);
                    if (cliffNear && context.Hash01(x, y, 1510) < 0.16f)
                    {
                        var prefab = AegisMapArtPack.Pick(AegisMapArtPack.BoulderMeshes, context.Seed, x, y);
                        var position = context.CellCenter(x, y, 0.22f);
                        var scale = Vector3.one * Mathf.Lerp(0.38f, 0.95f, context.Hash01(x, y, 1511));
                        if (!AegisMapArtPack.TryInstantiatePrefab(layer, "cliff_edge_rock_" + x + "_" + y, prefab, position, Quaternion.Euler(0f, context.Hash01(x, y, 1512) * 360f, 0f), scale, rockMaterial))
                            CreateScatterCube(layer, context, x, y, "cliff_edge_rock", rockMaterial, 0.36f, 0.84f, 1513);
                        placed++;
                        summary.ScatterCount++;
                        continue;
                    }

                    if ((role == "terrain.dark_grass" || role == "terrain.grass") && context.Hash01(x, y, 1520) < 0.055f)
                    {
                        var material = role == "terrain.dark_grass" ? treeMaterial : bushMaterial;
                        var prefab = AegisMapArtPack.Pick(AegisMapArtPack.VegetationMeshes, context.Seed, x, y);
                        var scale = Vector3.one * Mathf.Lerp(0.42f, 1.05f, context.Hash01(x, y, 1521));
                        if (!AegisMapArtPack.TryInstantiatePrefab(layer, "vegetation_" + x + "_" + y, prefab, context.CellCenter(x, y, 0.05f), Quaternion.Euler(0f, context.Hash01(x, y, 1522) * 360f, 0f), scale, material))
                            AegisVisualCompilerPrimitives.CreateCylinder(layer, "vegetation_" + x + "_" + y, context.CellCenter(x, y, 0.32f), new Vector3(scale.x * 0.35f, scale.y * 0.62f, scale.z * 0.35f), material);
                        placed++;
                        summary.ScatterCount++;
                        continue;
                    }

                    if (role == "terrain.grass" && context.Hash01(x, y, 1530) < 0.036f)
                    {
                        AegisVisualCompilerPrimitives.CreateCylinder(layer, "grass_tuft_" + x + "_" + y, context.CellCenter(x, y, 0.08f), new Vector3(0.18f, 0.16f, 0.18f), grassMaterial);
                        placed++;
                        summary.ScatterCount++;
                        continue;
                    }

                    if ((role == "terrain.dirt" || role == "terrain.gravel") && context.Hash01(x, y, 1540) < 0.012f)
                    {
                        AegisVisualCompilerPrimitives.CreateCylinder(layer, "crater_" + x + "_" + y, context.CellCenter(x, y, 0.045f), new Vector3(1.25f, 0.018f, 1.25f), craterMaterial);
                        placed++;
                        summary.ScatterCount++;
                    }
                }
            }

            if (placed >= MaxScatter)
                summary.AddWarning("Scatter compiler reached the deterministic placement cap.");

            return summary;
        }

        static bool IsNearCliff(AegisMapVisualCompileContext context, int x, int y)
        {
            for (var dy = -2; dy <= 2; dy++)
                for (var dx = -2; dx <= 2; dx++)
                    if (context.IsCliffLike(x + dx, y + dy))
                        return true;
            return false;
        }

        static void CreateScatterCube(Transform layer, AegisMapVisualCompileContext context, int x, int y, string prefix, Material material, float minScale, float maxScale, int salt)
        {
            var scale = Mathf.Lerp(minScale, maxScale, context.Hash01(x, y, salt));
            var offset = new Vector3(context.Hash01(x, y, salt + 1) - 0.5f, 0f, context.Hash01(x, y, salt + 2) - 0.5f) * 0.55f;
            AegisVisualCompilerPrimitives.CreateCube(layer, prefix + "_" + x + "_" + y, context.CellCenter(x, y, scale * 0.18f) + offset, new Vector3(scale, scale * 0.36f, scale), Quaternion.Euler(0f, context.Hash01(x, y, salt + 3) * 360f, 0f), material);
        }
    }
}
#endif
