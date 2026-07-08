using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Visuals/Lighting Profile")]
    public sealed class LightingProfile : ScriptableObject
    {
        public string profileId = "stage29_realistic_battlefield";
        public string displayName = "Stage 29 Realistic Battlefield";
        public Color ambientColor = new Color(0.50f, 0.54f, 0.50f, 1f);
        public Color directionalColor = new Color(1.00f, 0.94f, 0.82f, 1f);
        public Color cameraBackground = new Color(0.06f, 0.08f, 0.08f, 1f);
        public Vector3 directionalEuler = new Vector3(52f, -38f, 0f);
        public float directionalIntensity = 1.65f;
        public bool fogEnabled = true;
        public Color fogColor = new Color(0.13f, 0.16f, 0.15f, 1f);
        public float fogDensity = 0.012f;
        [TextArea(2, 5)] public string notes;

        public void ConfigureStage29Default()
        {
            profileId = "stage29_realistic_battlefield";
            displayName = "Stage 29 Realistic Battlefield";
            ambientColor = new Color(0.50f, 0.54f, 0.50f, 1f);
            directionalColor = new Color(1.00f, 0.94f, 0.82f, 1f);
            cameraBackground = new Color(0.06f, 0.08f, 0.08f, 1f);
            directionalEuler = new Vector3(52f, -38f, 0f);
            directionalIntensity = 1.65f;
            fogEnabled = true;
            fogColor = new Color(0.13f, 0.16f, 0.15f, 1f);
            fogDensity = 0.012f;
            notes = "Warm directional light plus low-density fog improves proxy silhouettes without hiding the fine placement grid.";
        }
    }
}
