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
            var panelDecalMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.panel_decal");
            var trimDecalMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.trim_decal");
            var crackMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.crack");
            var constructionWearMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.construction_wear");
            var panelRule = context.Theme == null ? null : context.Theme.RuleFor("basepad.panel");
            if (panelRule == null || string.IsNullOrEmpty(panelRule.AlbedoPath))
                summary.AddWarning("basepad.panel is missing a concrete texture path; production preview would fall back toward flat gray.");

            for (var i = 0; i < context.Starts.Count; i++)
            {
                var start = context.Starts[i];
                var parent = new GameObject("base_pad_player_" + start.PlayerId);
                parent.transform.SetParent(layer, false);
                var center = new Vector2(start.X + 0.5f, start.Y + 0.5f);
                summary.BasePadCount++;

                AegisVisualCompilerPrimitives.CreateOrganicQuad(parent.transform, "basepad_dirt_feather", center, PadSize + 4.8f, PadSize + 4.0f, 0.058f, dirtMaterial, 0f, context, start.X, start.Y, 1110, 0.92f, 9f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_center_panel", center, PadSize - TrimWidth * 2f, PadSize - TrimWidth * 2f, 0.105f, panelMaterial, 0f);

                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_panel_marking_center", center, PadSize - 5.4f, PadSize - 5.4f, 0.142f, panelDecalMaterial, 0f);
                summary.BasePadDetailDecalCount++;

                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_north_trim", center + new Vector2(0f, PadSize * 0.5f - TrimWidth * 0.5f), PadSize, TrimWidth, 0.13f, trimMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_south_trim", center + new Vector2(0f, -PadSize * 0.5f + TrimWidth * 0.5f), PadSize, TrimWidth, 0.13f, trimMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_east_trim", center + new Vector2(PadSize * 0.5f - TrimWidth * 0.5f, 0f), TrimWidth, PadSize, 0.13f, trimMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_west_trim", center + new Vector2(-PadSize * 0.5f + TrimWidth * 0.5f, 0f), TrimWidth, PadSize, 0.13f, trimMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_north_trim_decal", center + new Vector2(0f, PadSize * 0.5f - TrimWidth * 0.5f), PadSize, TrimWidth * 1.65f, 0.172f, trimDecalMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_south_trim_decal", center + new Vector2(0f, -PadSize * 0.5f + TrimWidth * 0.5f), PadSize, TrimWidth * 1.65f, 0.172f, trimDecalMaterial, 180f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_east_trim_decal", center + new Vector2(PadSize * 0.5f - TrimWidth * 0.5f, 0f), PadSize, TrimWidth * 1.65f, 0.172f, trimDecalMaterial, 90f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_west_trim_decal", center + new Vector2(-PadSize * 0.5f + TrimWidth * 0.5f, 0f), PadSize, TrimWidth * 1.65f, 0.172f, trimDecalMaterial, 270f);
                summary.BasePadDetailDecalCount += 4;

                var cornerOffset = PadSize * 0.5f - TrimWidth * 0.5f;
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_ne_corner", center + new Vector2(cornerOffset, cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_nw_corner", center + new Vector2(-cornerOffset, cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_se_corner", center + new Vector2(cornerOffset, -cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_sw_corner", center + new Vector2(-cornerOffset, -cornerOffset), TrimWidth, TrimWidth, 0.145f, cornerMaterial, 0f);

                for (var seam = -2; seam <= 2; seam += 2)
                {
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_vertical_seam_" + seam, center + new Vector2(seam, 0f), 0.08f, PadSize - 1.8f, 0.186f, grimeMaterial, 0f);
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_horizontal_seam_" + seam, center + new Vector2(0f, seam), PadSize - 1.8f, 0.08f, 0.186f, grimeMaterial, 0f);
                }

                for (var grime = 0; grime < 5; grime++)
                {
                    var gx = center.x + (context.Hash01(start.X, start.Y, 1200 + grime) - 0.5f) * (PadSize - 2f);
                    var gy = center.y + (context.Hash01(start.X, start.Y, 1220 + grime) - 0.5f) * (PadSize - 2f);
                    var size = Mathf.Lerp(0.8f, 2.4f, context.Hash01(start.X, start.Y, 1230 + grime));
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_grime_" + grime, new Vector2(gx, gy), size, size * 0.45f, 0.188f, grimeMaterial, context.Hash01(start.X, start.Y, 1240 + grime) * 180f);
                    summary.BasePadDetailDecalCount++;
                }

                for (var crack = 0; crack < 2; crack++)
                {
                    var cx = center.x + (context.Hash01(start.X, start.Y, 1270 + crack) - 0.5f) * (PadSize - 3.5f);
                    var cy = center.y + (context.Hash01(start.X, start.Y, 1280 + crack) - 0.5f) * (PadSize - 3.5f);
                    var crackLength = Mathf.Lerp(2.2f, 4.8f, context.Hash01(start.X, start.Y, 1290 + crack));
                    AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_hairline_crack_" + crack, new Vector2(cx, cy), crackLength, 0.42f, 0.192f, crackMaterial, context.Hash01(start.X, start.Y, 1300 + crack) * 180f);
                    summary.BasePadDetailDecalCount++;
                }

                var approach = (new Vector2(context.Width * 0.5f, context.Height * 0.5f) - center).normalized;
                var approachAngle = Mathf.Atan2(approach.y, approach.x) * Mathf.Rad2Deg;
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_construction_wear_approach", center + approach * (PadSize * 0.62f), 7.5f, 3.2f, 0.12f, dirtMaterial, approachAngle);
                AegisVisualCompilerPrimitives.CreateQuad(parent.transform, "basepad_construction_wear_decal", center + approach * (PadSize * 0.62f), 8.2f, 4.0f, 0.178f, constructionWearMaterial, approachAngle);
                summary.BasePadDetailDecalCount++;
            }

            return summary;
        }
    }
}
#endif
