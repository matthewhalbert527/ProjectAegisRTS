#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisScatterVisualCompiler : IAegisVisualLayerCompiler
    {
        const int MaxScatter = 900;

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

            for (var y = 2; y < context.Height - 2; y += 4)
            {
                for (var x = 2; x < context.Width - 2; x += 4)
                {
                    if (placed >= MaxScatter)
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    if (!CanPlaceScatter(context, x, y))
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    if (AegisVisualCompilerPrimitives.IsRoadNear(context, x, y, 2.1f))
                    {
                        if (context.Hash01(x, y, 1501) < 0.025f)
                        {
                            EmitGroundDecal(layer, context, summary, x, y, "road_edge_rubble", rubbleMaterial, 0.85f, 1.9f, 0.28f, 0.72f, 1502);
                            placed++;
                        }
                        else
                        {
                            summary.SkippedPlacementCount++;
                        }
                        continue;
                    }

                    var role = context.TerrainRoleAt(x, y);
                    var cliffNear = IsNearCliff(context, x, y);
                    if (cliffNear && context.Hash01(x, y, 1510) < 0.24f)
                    {
                        var prefab = AegisMapArtPack.Pick(AegisMapArtPack.BoulderMeshes, context.Seed, x, y);
                        var position = context.CellCenter(x, y, 0.22f);
                        var scale = Vector3.one * Mathf.Lerp(0.62f, 1.25f, context.Hash01(x, y, 1511));
                        if (!AegisMapArtPack.TryInstantiatePrefab(layer, "cliff_edge_rock_" + x + "_" + y, prefab, position, Quaternion.Euler(0f, context.Hash01(x, y, 1512) * 360f, 0f), scale, rockMaterial))
                            CreateScatterCube(layer, context, x, y, "cliff_edge_rock", rockMaterial, 0.36f, 0.84f, 1513);
                        placed++;
                        summary.ScatterCount++;
                        summary.RockCount++;
                        continue;
                    }

                    if ((role == "terrain.dark_grass" || role == "terrain.grass") && context.Hash01(x, y, 1520) < 0.20f)
                    {
                        placed += CreateVegetationCluster(layer, context, summary, x, y, role == "terrain.dark_grass", treeMaterial, bushMaterial);
                        continue;
                    }

                    if (role == "terrain.grass" && context.Hash01(x, y, 1530) < 0.10f)
                    {
                        AegisVisualCompilerPrimitives.CreateCylinder(layer, "grass_tuft_" + x + "_" + y, context.CellCenter(x, y, 0.08f), new Vector3(0.32f, 0.22f, 0.32f), grassMaterial);
                        placed++;
                        summary.ScatterCount++;
                        summary.GrassCount++;
                        continue;
                    }

                    if ((role == "terrain.dirt" || role == "terrain.gravel") && context.Hash01(x, y, 1540) < 0.012f)
                    {
                        EmitCrater(layer, context, summary, x, y, craterMaterial);
                        placed++;
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

        static bool CanPlaceScatter(AegisMapVisualCompileContext context, int x, int y)
        {
            return !context.IsStartProtected(x, y) &&
                !context.IsWater(x, y) &&
                !context.HasResource(x, y) &&
                !AegisVisualCompilerPrimitives.IsRoadNear(context, x, y, 1.8f);
        }

        static int CreateVegetationCluster(Transform layer, AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, int x, int y, bool preferTrees, Material treeMaterial, Material bushMaterial)
        {
            var placed = 0;
            var count = preferTrees ? 2 + context.HashRange(x, y, 1524, 3) : 2 + context.HashRange(x, y, 1525, 2);
            for (var i = 0; i < count; i++)
            {
                var angle = context.Hash01(x, y, 1530 + i) * Mathf.PI * 2f;
                var distance = Mathf.Lerp(0.2f, 1.75f, context.Hash01(x, y, 1540 + i));
                var px = x + Mathf.Cos(angle) * distance;
                var py = y + Mathf.Sin(angle) * distance;
                var cellX = Mathf.Clamp(Mathf.FloorToInt(px), 0, context.Width - 1);
                var cellY = Mathf.Clamp(Mathf.FloorToInt(py), 0, context.Height - 1);
                if (!CanPlaceScatter(context, cellX, cellY))
                {
                    summary.SkippedPlacementCount++;
                    continue;
                }

                var material = preferTrees || i == 0 ? treeMaterial : bushMaterial;
                var prefab = AegisMapArtPack.Pick(AegisMapArtPack.VegetationMeshes, context.Seed, cellX, cellY);
                var scale = Vector3.one * Mathf.Lerp(preferTrees ? 0.82f : 0.55f, preferTrees ? 1.35f : 0.95f, context.Hash01(cellX, cellY, 1521 + i));
                var position = new Vector3(px, 0.05f, py);
                if (!AegisMapArtPack.TryInstantiatePrefab(layer, "vegetation_cluster_" + x + "_" + y + "_" + i, prefab, position, Quaternion.Euler(0f, context.Hash01(cellX, cellY, 1522 + i) * 360f, 0f), scale, material))
                    AegisVisualCompilerPrimitives.CreateCylinder(layer, "vegetation_cluster_" + x + "_" + y + "_" + i, new Vector3(px, 0.32f, py), new Vector3(scale.x * 0.35f, scale.y * 0.62f, scale.z * 0.35f), material);

                placed++;
                summary.ScatterCount++;
                if (preferTrees || i == 0)
                    summary.TreeCount++;
                else
                    summary.BushCount++;
            }

            return placed;
        }

        static void CreateScatterCube(Transform layer, AegisMapVisualCompileContext context, int x, int y, string prefix, Material material, float minScale, float maxScale, int salt)
        {
            var scale = Mathf.Lerp(minScale, maxScale, context.Hash01(x, y, salt));
            var offset = new Vector3(context.Hash01(x, y, salt + 1) - 0.5f, 0f, context.Hash01(x, y, salt + 2) - 0.5f) * 0.55f;
            AegisVisualCompilerPrimitives.CreateCube(layer, prefix + "_" + x + "_" + y, context.CellCenter(x, y, scale * 0.18f) + offset, new Vector3(scale, scale * 0.36f, scale), Quaternion.Euler(0f, context.Hash01(x, y, salt + 3) * 360f, 0f), material);
        }

        static void EmitCrater(Transform layer, AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, int x, int y, Material material)
        {
            var center = new Vector2(x + 0.5f + (context.Hash01(x, y, 1550) - 0.5f) * 1.1f, y + 0.5f + (context.Hash01(x, y, 1551) - 0.5f) * 1.1f);
            var scale = Mathf.Lerp(0.68f, 1.18f, context.Hash01(x, y, 1552));
            var prefab = AegisMapArtPack.Pick(AegisMapArtPack.CraterMeshes, context.Seed, x, y);
            if (AegisMapArtPack.TryInstantiatePrefab(layer, "crater_mesh_" + x + "_" + y, prefab, new Vector3(center.x, 0.052f, center.y), Quaternion.Euler(0f, context.Hash01(x, y, 1553) * 360f, 0f), Vector3.one * scale, material))
            {
                summary.ScatterCount++;
                return;
            }

            EmitGroundDecal(layer, context, summary, x, y, "crater_decal", material, 1.15f, 2.25f, 0.72f, 1.12f, 1554);
        }

        static void EmitGroundDecal(Transform layer, AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, int x, int y, string prefix, Material material, float minWidth, float maxWidth, float minAspect, float maxAspect, int salt)
        {
            var width = Mathf.Lerp(minWidth, maxWidth, context.Hash01(x, y, salt));
            var aspect = Mathf.Lerp(minAspect, maxAspect, context.Hash01(x, y, salt + 1));
            var center = new Vector2(x + 0.5f + (context.Hash01(x, y, salt + 2) - 0.5f) * 1.1f, y + 0.5f + (context.Hash01(x, y, salt + 3) - 0.5f) * 1.1f);
            var angle = context.Hash01(x, y, salt + 4) * 180f;
            AegisVisualCompilerPrimitives.CreateOrganicQuad(layer, prefix + "_" + x + "_" + y, center, width, width * aspect, 0.087f, material, angle, context, x, y, salt + 17, Mathf.Min(width, width * aspect) * 0.16f);
            summary.ScatterCount++;
        }
    }
}
#endif
