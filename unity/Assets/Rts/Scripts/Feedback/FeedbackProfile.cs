using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    [CreateAssetMenu(fileName = "FeedbackProfile", menuName = "ProjectAegisRTS/Feedback Profile")]
    public sealed class FeedbackProfile : ScriptableObject
    {
        public FeedbackEventType eventType = FeedbackEventType.Generic;
        public string displayName = "Feedback";
        public Color color = Color.white;
        public PrimitiveType markerPrimitive = PrimitiveType.Sphere;
        public float markerScale = 0.35f;
        public float durationSeconds = 0.7f;
        public float floatSpeed = 0.6f;
        public bool spawnVisualMarker = true;
        public bool silentAudioCue = true;
        public float audioVolume = 0.35f;
        public float audioPitch = 1f;
        public bool hapticPulse;
        public float hapticIntensity = 0.25f;
    }
}
