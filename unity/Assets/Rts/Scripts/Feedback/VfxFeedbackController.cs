using ProjectAegisRTS.UnityClient.Performance;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class VfxFeedbackController : MonoBehaviour
    {
        public FeedbackEventBus eventBus;
        public FeedbackProfileLibrary profileLibrary;
        public ObjectPoolService objectPoolService;

        public int SpawnedMarkerCount { get; private set; }
        public int ActiveMarkerCount { get; private set; }
        public FeedbackEventType LastMarkerType { get; private set; }

        void OnEnable()
        {
            Subscribe();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        public void Initialize(FeedbackEventBus bus, FeedbackProfileLibrary library, ObjectPoolService poolService = null)
        {
            Unsubscribe();
            eventBus = bus;
            profileLibrary = library;
            objectPoolService = poolService;
            Subscribe();
        }

        void HandleFeedback(FeedbackEvent feedbackEvent)
        {
            if (feedbackEvent == null)
                return;

            var profile = profileLibrary == null ? null : profileLibrary.GetProfile(feedbackEvent.eventType);
            if (profile == null || !profile.spawnVisualMarker)
                return;

            var marker = objectPoolService == null
                ? CreateMarkerObject(profile.markerPrimitive)
                : objectPoolService.Acquire("FeedbackMarker." + profile.markerPrimitive, () => CreateMarkerObject(profile.markerPrimitive), transform);
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

            var timed = marker.GetComponent<FeedbackVisualMarker>();
            if (timed == null)
                timed = marker.AddComponent<FeedbackVisualMarker>();
            timed.Configure(Mathf.Max(0.05f, profile.durationSeconds), profile.floatSpeed, objectPoolService, this);

            SpawnedMarkerCount++;
            ActiveMarkerCount++;
            LastMarkerType = feedbackEvent.eventType;
        }

        public void NotifyMarkerReleased()
        {
            ActiveMarkerCount = Mathf.Max(0, ActiveMarkerCount - 1);
        }

        static GameObject CreateMarkerObject(PrimitiveType primitive)
        {
            var marker = GameObject.CreatePrimitive(primitive);
            var collider = marker.GetComponent<Collider>();
            if (collider != null)
                FeedbackObjectUtility.DestroyObject(collider);
            return marker;
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
