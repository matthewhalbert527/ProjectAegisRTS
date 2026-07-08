using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    [DisallowMultipleComponent]
    public sealed class ProjectAegisTankTeamColorRig : MonoBehaviour
    {
        public Renderer[] teamColorRenderers;
        public Color fallbackTeamColor = new Color(0.16f, 0.72f, 0.82f, 1f);
        [Range(0f, 1f)] public float teamTintStrength = 0.45f;

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

            ProjectAegisTeamColorMaterialUtility.ApplyTeamTint(teamColorRenderers, color, ref block, teamTintStrength);
        }
    }
}
