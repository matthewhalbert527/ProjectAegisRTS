using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class HapticFeedbackAdapter : MonoBehaviour
    {
        public FeedbackEventBus eventBus;
        public FeedbackProfileLibrary profileLibrary;

        public int PlaceholderPulseCount { get; private set; }
        public FeedbackEventType LastPulseType { get; private set; }
        public float LastPulseIntensity { get; private set; }

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
            if (profile == null || !profile.hapticPulse)
                return;

            PlaceholderPulseCount++;
            LastPulseType = feedbackEvent.eventType;
            LastPulseIntensity = Mathf.Clamp01(profile.hapticIntensity * Mathf.Max(0.25f, feedbackEvent.intensity));
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
