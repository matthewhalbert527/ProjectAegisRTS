using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class AudioFeedbackController : MonoBehaviour
    {
        public FeedbackEventBus eventBus;
        public FeedbackProfileLibrary profileLibrary;
        public AudioSource audioSource;

        public int SilentCueCount { get; private set; }
        public int AudibleCueCount { get; private set; }
        public FeedbackEventType LastCueType { get; private set; }

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
            EnsureAudioSource();
            Subscribe();
        }

        void HandleFeedback(FeedbackEvent feedbackEvent)
        {
            if (feedbackEvent == null)
                return;

            EnsureAudioSource();
            var profile = profileLibrary == null ? null : profileLibrary.GetProfile(feedbackEvent.eventType);
            LastCueType = feedbackEvent.eventType;

            if (profile == null || profile.silentAudioCue)
            {
                SilentCueCount++;
                return;
            }

            audioSource.pitch = Mathf.Max(0.1f, profile.audioPitch);
            audioSource.volume = Mathf.Clamp01(profile.audioVolume * Mathf.Max(0.2f, feedbackEvent.intensity));
            AudibleCueCount++;
        }

        void EnsureAudioSource()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
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
