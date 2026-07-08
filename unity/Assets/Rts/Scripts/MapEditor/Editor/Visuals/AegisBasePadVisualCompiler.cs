#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisBasePadVisualCompiler : IAegisVisualLayerCompiler
    {
        const float PadSize = 14f;
        const float TrimWidth = 0.55f;

        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Modular Base Pads");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Modular Base Pads");
            var panelMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.panel");
            var trimMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.trim");
            var cornerMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.corner");
            var grimeMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.grime");
            var dirtMaterial = AegisVisualCompilerPrimitives.Material(context, "terrain.dirt");

            for (var i = 0; i < context.Starts.Count; i++)
            {
                var start = context.Starts[i];
                var parent = new GameObject("base_pad_player_" + start.PlayerId);
                parent.transform.SetParent(layer, false);
                var center = new Vector2(start.X + 0.5f, start.Y + 0.5f);
                summary.BasePadCount++;

                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_dirt_integration", center, PadSize + 4.2f, PadSize + 4.2f, 0.058f, dirtMaterial, 0f);
                var importedPadPosition = new Vector3(center.x, 0.11f, center.y);
                if (!AegisMapArtPack.TryInstantiatePrefab(parent.transform, "basepad_imported_14x14", AegisMapArtPack.BasePadMesh, importedPadPosition, Quaternion.identity, Vector3.one, panelMaterial))
                {
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_center_panel", center, PadSize - TrimWidth * 2f, PadSize - TrimWidth * 2f, 0.105f, panelMaterial, 0f);
                    summary.SkippedPlacementCount++;
                }

                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_north_trim", center + new Vector2(0f, PadSize * 0.5f - TrimWidth * 0.5f), PadSize, TrimWidth, 0.13f, trimMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_south_trim", center + new Vector2(0f, -PadSize * 0.5f + TrimWidth * 0.5f), PadSize, TrimWidth, 0.13f, trimMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_east_trim", center + new Vector2(PadSize * 0.5f - TrimWidth * 0.5f, 0f), TrimWidth, PadSize, 0.13f, trimMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_west_trim", center + new Vector2(-PadSize * 0.5f + TrimWidth * 0.5f, 0f), TrimWidth, PadSize, 0.13f, trimMaterial, 0f);

                var cornerOffset = PadSize * 0.5f - TrimWidth * 0.5f;
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_ne_corner", center + new Vector2(cornerOffset, cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_nw_corner", center + new Vector2(-cornerOffset, cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_se_corner", center + new Vector2(cornerOffset, -cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_sw_corner", center + new Vector2(-cornerOffset, -cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);

                for (var seam = -2; seam <= 2; seam += 2)
                {
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_vertical_seam_" + seam, center + new Vector2(seam, 0f), 0.08f, PadSize - 1.8f, 0.15f, grimeMaterial, 0f);
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_horizontal_seam_" + seam, center + new Vector2(0f, seam), PadSize - 1.8f, 0.08f, 0.15f, grimeMaterial, 0f);
                }

                for (var grime = 0; grime < 5; grime++)
                {
                    var gx = center.x + (context.Hash01(start.X, start.Y, 1200 + grime) - 0.5f) * (PadSize - 2f);
                    var gy = center.y + (context.Hash01(start.X, start.Y, 1220 + grime) - 0.5f) * (PadSize - 2f);
                    var size = Mathf.Lerp(0.8f, 2.4f, context.Hash01(start.X, start.Y, 1230 + grime));
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_grime_" + grime, new Vector2(gx, gy), size, size * 0.45f, 0.16f, grimeMaterial, context.Hash01(start.X, start.Y, 1240 + grime) * 180f);
                }

                var approach = (new Vector2(context.Width * 0.5f, context.Height * 0.5f) - center).normalized;
                var approachAngle = Mathf.Atan2(approach.y, approach.x) * Mathf.Rad2Deg;
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_construction_wear_approach", center + approach * (PadSize * 0.62f), 7.5f, 3.2f, 0.12f, dirtMaterial, approachAngle);
            }

            return summary;
        }
    }
}
#endif
