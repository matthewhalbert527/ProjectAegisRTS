#if UNITY_EDITOR
using System;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisResourceFieldVisualCompiler : IAegisVisualLayerCompiler
    {
        const int MaxVisualsPerField = 24;
        const int MaxGlintsPerField = 4;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Resource Field Visuals");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Resource Field Visuals");
            var dustMaterial = AegisVisualCompilerPrimitives.Material(context, "resource.ore_dust");
            var glintMaterial = AegisVisualCompilerPrimitives.Material(context, "resource.glint");

            for (var i = 0; i < context.ResourceFields.Count; i++)
            {
                var field = context.ResourceFields[i];
                if (field.Cells.Count == 0)
                    continue;

                summary.ResourceFields++;
                if (field.Regenerating)
                    summary.ResourceRegeneratingCount++;

                var center = field.Center;
                var radius = Mathf.Clamp(Mathf.Sqrt(field.Cells.Count) * 0.72f + 1.4f, 1.8f, 7.5f);
                AegisVisualCompilerPrimitives.CreateQuad(layer, "resource_field_dust_" + field.FieldId, center, radius * 2.35f, radius * 1.72f, 0.092f, dustMaterial, context.Hash01(i, field.Cells.Count, 990) * 180f);
                summary.ResourceDustDecalCount++;

                if (field.CurrentAmount <= 0)
                {
                    summary.ResourceDepletedCount++;
                    continue;
                }

                var material = AegisVisualCompilerPrimitives.Material(context, ResourceRole(field.ResourceKind));
                var visualCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt(field.Cells.Count) * Mathf.Lerp(2.4f, 5.2f, field.FillRatio)), 2, MaxVisualsPerField);
                var glints = 0;
                for (var j = 0; j < visualCount; j++)
                {
                    var angle = context.Hash01(i, j, 1001) * Mathf.PI * 2f;
                    var distance = Mathf.Pow(context.Hash01(i, j, 1002), 1.18f) * radius * 0.70f;
                    var x = center.x + Mathf.Cos(angle) * distance;
                    var y = center.y + Mathf.Sin(angle) * distance;
                    var cellX = Mathf.Clamp(Mathf.FloorToInt(x), 0, context.Width - 1);
                    var cellY = Mathf.Clamp(Mathf.FloorToInt(y), 0, context.Height - 1);
                    if (context.IsStartProtected(cellX, cellY) || context.IsWater(cellX, cellY) || AegisVisualCompilerPrimitives.IsRoadNear(context, cellX, cellY, 1.4f))
                    {
                        summary.SkippedPlacementCount++;
                        continue;
                    }

                    var centerWeight = 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, radius));
                    var scale = Mathf.Lerp(0.52f, 1.05f, centerWeight) * Mathf.Lerp(0.72f, 1.1f, field.FillRatio);
                    var position = new Vector3(x, 0.12f + scale * 0.12f, y);
                    var rotation = Quaternion.Euler(0f, context.Hash01(cellX, cellY, 1003 + j) * 360f, 0f);
                    var prefabPath = PickResourcePrefab(field.ResourceKind, context.Seed, cellX, cellY);
                    if (!AegisMapArtPack.TryInstantiatePrefab(layer, "resource_" + field.FieldId + "_" + j, prefabPath, position, rotation, Vector3.one * scale, material))
                        AegisVisualCompilerPrimitives.CreateCube(layer, "resource_" + field.FieldId + "_" + j, position, new Vector3(scale, scale * 0.55f, scale), rotation, material);
                    summary.ResourceVisualInstances++;

                    if (field.FillRatio > 0.75f && glints < MaxGlintsPerField && context.Hash01(cellX, cellY, 1004 + j) < 0.055f)
                    {
                        AegisVisualCompilerPrimitives.CreateQuad(layer, "resource_glint_" + field.FieldId + "_" + j, new Vector2(x, y), scale * 0.86f, scale * 0.14f, 0.22f, glintMaterial, context.Hash01(cellX, cellY, 1005 + j) * 180f);
                        summary.ResourceGlintCount++;
                        glints++;
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
