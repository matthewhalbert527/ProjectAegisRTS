using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class VfxFeedbackController : MonoBehaviour
    {
        public FeedbackEventBus eventBus;
        public FeedbackProfileLibrary profileLibrary;

        public int SpawnedMarkerCount { get; private set; }
        public FeedbackEventType LastMarkerType { get; private set; }

        void OnEnable()
        {
            Subscribe();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        public void Initialize(FeedbackEventBus bus, FeedbackProfileLibrary library)
        {
            Unsubscribe();
            eventBus = bus;
            profileLibrary = library;
            Subscribe();
        }

        void HandleFeedback(FeedbackEvent feedbackEvent)
        {
            if (feedbackEvent == null)
                return;

            var profile = profileLibrary == null ? null : profileLibrary.GetProfile(feedbackEvent.eventType);
            if (profile == null || !profile.spawnVisualMarker)
                return;

            var marker = GameObject.CreatePrimitive(profile.markerPrimitive);
            marker.name = "Feedback " + feedbackEvent.eventType;
            marker.transform.SetParent(transform, false);
            marker.transform.position = feedbackEvent.worldPosition;
            marker.transform.localScale = Vector3.one * Mathf.Max(0.05f, profile.markerScale * Mathf.Max(0.35f, feedbackEvent.intensity));

            var collider = marker.GetComponent<Collider>();
            if (collider != null)
                FeedbackObjectUtility.DestroyObject(collider);

            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                renderer.sharedMaterial.color = profile.color;
            }

            var timed = marker.AddComponent<FeedbackVisualMarker>();
            timed.durationSeconds = Mathf.Max(0.05f, profile.durationSeconds);
            timed.floatSpeed = profile.floatSpeed;

            SpawnedMarkerCount++;
            LastMarkerType = feedbackEvent.eventType;
        }

        void Subscribe()
        {
            if (eventBus != null)
                eventBus.EventEmitted += HandleFeedback;
        }

        void Unsubscribe()
        {
            if (eventBus != null)
                eventBus.EventEmitted -= HandleFeedback;
        }
    }
}
