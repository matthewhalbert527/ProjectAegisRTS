using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    [DisallowMultipleComponent]
    public sealed class ProjectAegisTankTeamColorRig : MonoBehaviour
    {
        public Renderer[] teamColorRenderers;
        public Color fallbackTeamColor = new Color(0.16f, 0.72f, 0.82f, 1f);

        MaterialPropertyBlock block;
        Color appliedColor;
        bool hasAppliedColor;

        void OnEnable()
        {
            ApplyTeamColor(fallbackTeamColor);
        }

        public void ApplyTeamColor(Color color)
        {
            if (teamColorRenderers == null)
                return;

            if (hasAppliedColor && appliedColor == color)
                return;

            appliedColor = color;
            hasAppliedColor = true;
            if (block == null)
                block = new MaterialPropertyBlock();

            for (var i = 0; i < teamColorRenderers.Length; i++)
            {
                var renderer = teamColorRenderers[i];
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", color);
                block.SetColor("_Color", color);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
