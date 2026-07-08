#if UNITY_EDITOR
using System;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisResourceFieldVisualCompiler : IAegisVisualLayerCompiler
    {
        const int MaxVisualsPerField = 72;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Resource Field Visuals");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Resource Field Visuals");
            var dustMaterial = AegisVisualCompilerPrimitives.Material(context, "terrain.ore_stained_soil");
            var glintMaterial = AegisVisualCompilerPrimitives.Material(context, "resource.energy");

            for (var i = 0; i < context.ResourceFields.Count; i++)
            {
                var field = context.ResourceFields[i];
                if (field.Cells.Count == 0)
                    continue;

                summary.ResourceFields++;
                if (field.CurrentAmount <= 0)
                {
                    summary.ResourceDepletedCount++;
                    continue;
                }

                if (field.Regenerating)
                    summary.ResourceRegeneratingCount++;

                var center = field.Center;
                var radius = Mathf.Clamp(Mathf.Sqrt(field.Cells.Count) * 0.72f + 1.4f, 1.8f, 7.5f);
                AegisVisualCompilerPrimitives.CreateCylinder(layer, "resource_field_dust_" + field.FieldId, new Vector3(center.x, 0.045f, center.y), new Vector3(radius, 0.018f, radius), dustMaterial);

                var material = AegisVisualCompilerPrimitives.Material(context, ResourceRole(field.ResourceKind));
                var visualCount = Mathf.Clamp(Mathf.RoundToInt(field.Cells.Count * Mathf.Lerp(0.55f, 2.4f, field.FillRatio)), 1, MaxVisualsPerField);
                for (var j = 0; j < visualCount; j++)
                {
                    var angle = context.Hash01(i, j, 1001) * Mathf.PI * 2f;
                    var distance = Mathf.Pow(context.Hash01(i, j, 1002), 0.72f) * radius * 0.86f;
                    var x = center.x + Mathf.Cos(angle) * distance;
                    var y = center.y + Mathf.Sin(angle) * distance;
                    var cellX = Mathf.Clamp(Mathf.FloorToInt(x), 0, context.Width - 1);
                    var cellY = Mathf.Clamp(Mathf.FloorToInt(y), 0, context.Height - 1);
                    if (context.IsStartProtected(cellX, cellY) || context.IsWater(cellX, cellY))
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    var centerWeight = 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, radius));
                    var scale = Mathf.Lerp(0.24f, 0.68f, centerWeight) * Mathf.Lerp(0.55f, 1f, field.FillRatio);
                    var position = new Vector3(x, 0.12f + scale * 0.12f, y);
                    var rotation = Quaternion.Euler(0f, context.Hash01(cellX, cellY, 1003 + j) * 360f, 0f);
                    var prefabPath = PickResourcePrefab(field.ResourceKind, context.Seed, cellX, cellY);
                    if (!AegisMapArtPack.TryInstantiatePrefab(layer, "resource_" + field.FieldId + "_" + j, prefabPath, position, rotation, Vector3.one * scale, material))
                        AegisVisualCompilerPrimitives.CreateCube(layer, "resource_" + field.FieldId + "_" + j, position, new Vector3(scale, scale * 0.55f, scale), rotation, material);
                    summary.ResourceVisualInstances++;

                    if (field.FillRatio > 0.75f && context.Hash01(cellX, cellY, 1004 + j) < 0.12f)
                    {
                        AegisVisualCompilerPrimitives.CreateQuad(layer, "resource_glint_" + field.FieldId + "_" + j, new Vector2(x, y), scale * 1.4f, scale * 0.32f, 0.22f, glintMaterial, context.Hash01(cellX, cellY, 1005 + j) * 180f);
                        summary.ResourceVisualInstances++;
                    }
                }
            }

            return summary;
        }

        static string ResourceRole(string resourceKind)
        {
            if (string.IsNullOrEmpty(resourceKind))
                return "resource.ore";

            var kind = resourceKind.ToLowerInvariant();
            if (kind.Contains("crystal"))
                return "resource.crystal";
            if (kind.Contains("salvage"))
                return "resource.salvage";
            if (kind.Contains("energy"))
                return "resource.energy";
            return "resource.ore";
        }

        static string PickResourcePrefab(string resourceKind, int seed, int x, int y)
        {
            if (string.IsNullOrEmpty(resourceKind))
                return AegisMapArtPack.Pick(AegisMapArtPack.OreMeshes, seed, x, y);

            var kind = resourceKind.ToLowerInvariant();
            if (kind.Contains("crystal"))
                return AegisMapArtPack.Pick(AegisMapArtPack.CrystalMeshes, seed, x, y);
            if (kind.Contains("salvage"))
                return AegisMapArtPack.Pick(AegisMapArtPack.SalvageMeshes, seed, x, y);
            if (kind.Contains("energy"))
                return AegisMapArtPack.Pick(AegisMapArtPack.EnergyMeshes, seed, x, y);

            return AegisMapArtPack.Pick(AegisMapArtPack.OreMeshes, seed, x, y);
        }
    }
}
#endif
