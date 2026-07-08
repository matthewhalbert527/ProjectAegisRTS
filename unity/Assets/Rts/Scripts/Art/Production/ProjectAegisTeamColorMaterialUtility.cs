using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public static class ProjectAegisTeamColorMaterialUtility
    {
        public static void ApplyTeamTint(Renderer[] renderers, Color teamColor, ref MaterialPropertyBlock block, float tintStrength)
        {
            if (renderers == null)
                return;

            tintStrength = Mathf.Clamp01(tintStrength);
            if (block == null)
                block = new MaterialPropertyBlock();

            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                var materialColor = ResolveMaterialColor(renderer.sharedMaterial);
                var finalColor = Color.Lerp(materialColor, teamColor, tintStrength);
                finalColor.a = materialColor.a;

                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", finalColor);
                block.SetColor("_Color", finalColor);
                renderer.SetPropertyBlock(block);
            }
        }

        static Color ResolveMaterialColor(Material material)
        {
            if (material == null)
                return new Color(0.45f, 0.48f, 0.46f, 1f);
            if (material.HasProperty("_BaseColor"))
                return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color"))
                return material.GetColor("_Color");
            return new Color(0.45f, 0.48f, 0.46f, 1f);
        }
    }
}
