using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Performance Budget Profile", fileName = "PerformanceBudgetProfile")]
    public sealed class PerformanceBudgetProfile : ScriptableObject
    {
        public string profileId = "quest";
        public string displayName = "Quest Target";
        public int targetFrameRate = 72;
        public int maxSceneGameObjects = 900;
        public int maxActiveRenderers = 450;
        public int maxActorViews = 120;
        public int maxProjectileViews = 48;
        public int maxFeedbackMarkers = 32;
        public int maxPoolInactiveObjects = 128;
        public int qualityLevel = 0;
        public int antiAliasing = 2;
        public int pixelLightCount = 1;
        public float shadowDistance = 18f;
        public float lodBias = 0.7f;
        public bool preferVSyncOff = true;
        [TextArea]
        public string notes = "Placeholder Stage 15 performance budget. Tune after device profiling.";
    }
}
