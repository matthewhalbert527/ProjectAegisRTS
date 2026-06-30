using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Feedback
{
    public sealed class FeedbackDebugHud : MonoBehaviour
    {
        public FeedbackEventBus eventBus;
        public FeedbackProfileLibrary profileLibrary;
        public AudioFeedbackController audioController;
        public VfxFeedbackController vfxController;
        public UiFeedbackController uiController;
        public HapticFeedbackAdapter hapticAdapter;
        public bool visible = true;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible)
                return;

            EnsureReferences();
            GUILayout.BeginArea(new Rect(850, 345, 430, 220), GUI.skin.box);
            GUILayout.Label("Stage 14 Feedback");
            if (eventBus == null)
            {
                GUILayout.Label("No feedback event bus.");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label("Events: " + eventBus.TotalEventCount + " last=" + eventBus.LastEventType);
            GUILayout.Label("Profiles: " + (profileLibrary == null ? 0 : profileLibrary.ProfileCount));
            GUILayout.Label("Audio silent/audible: " + (audioController == null ? 0 : audioController.SilentCueCount) + "/" + (audioController == null ? 0 : audioController.AudibleCueCount));
            GUILayout.Label("VFX markers: " + (vfxController == null ? 0 : vfxController.SpawnedMarkerCount));
            GUILayout.Label("Haptic placeholders: " + (hapticAdapter == null ? 0 : hapticAdapter.PlaceholderPulseCount));
            GUILayout.Label(uiController == null ? string.Empty : uiController.Summary);

            if (GUILayout.Button("Pulse Selection"))
                eventBus.EmitManual(FeedbackEventType.SelectionChanged, "Manual selection pulse.");
            if (GUILayout.Button("Pulse Invalid"))
                eventBus.EmitManual(FeedbackEventType.InvalidCommand, "Manual invalid command pulse.");
            if (GUILayout.Button("Pulse Impact"))
                eventBus.EmitManual(FeedbackEventType.ProjectileImpact, "Manual impact pulse.");
            GUILayout.EndArea();
        }

        void EnsureReferences()
        {
            if (eventBus == null)
                eventBus = Object.FindFirstObjectByType<FeedbackEventBus>();
            if (profileLibrary == null)
                profileLibrary = Object.FindFirstObjectByType<FeedbackProfileLibrary>();
            if (audioController == null)
                audioController = Object.FindFirstObjectByType<AudioFeedbackController>();
            if (vfxController == null)
                vfxController = Object.FindFirstObjectByType<VfxFeedbackController>();
            if (uiController == null)
                uiController = Object.FindFirstObjectByType<UiFeedbackController>();
            if (hapticAdapter == null)
                hapticAdapter = Object.FindFirstObjectByType<HapticFeedbackAdapter>();
        }
    }
}
