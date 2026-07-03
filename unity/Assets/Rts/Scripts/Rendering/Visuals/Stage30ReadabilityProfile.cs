using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class Stage30ReadabilityProfile : ScriptableObject
    {
        public float minimumActorTerrainLuminanceDelta = 0.22f;
        public float minimumResourceAccentLuminance = 0.48f;
        public float maximumFineGridDominance = 0.45f;
        public float recommendedPcOrthographicSize = 8.9f;
        public Color actorOutlineColor = new Color(0.035f, 0.045f, 0.04f, 1f);
        public Color playerTrimColor = new Color(0.20f, 0.72f, 0.68f, 1f);
        public Color enemyTrimColor = new Color(0.78f, 0.26f, 0.18f, 1f);
        public Color resourceAccentColor = new Color(0.18f, 0.95f, 0.74f, 1f);
        public string notes;

        public void ConfigureStage30Default()
        {
            minimumActorTerrainLuminanceDelta = 0.22f;
            minimumResourceAccentLuminance = 0.48f;
            maximumFineGridDominance = 0.45f;
            recommendedPcOrthographicSize = 8.9f;
            actorOutlineColor = new Color(0.035f, 0.045f, 0.04f, 1f);
            playerTrimColor = new Color(0.20f, 0.72f, 0.68f, 1f);
            enemyTrimColor = new Color(0.78f, 0.26f, 0.18f, 1f);
            resourceAccentColor = new Color(0.18f, 0.95f, 0.74f, 1f);
            notes = "Stage 30 visual readability profile: keep terrain readable, actor silhouettes separated from ground, fine-grid guidance visible but subdued, and resource accents visible from the PC camera.";
        }
    }
}
