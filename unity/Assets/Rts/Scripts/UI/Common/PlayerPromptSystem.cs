using ProjectAegisRTS.Match;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class PlayerPromptSystem : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public VerticalSliceProgressTracker progressTracker;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.P;
        public string CurrentPrompt { get; private set; }

        string lastPrompt = string.Empty;
        float promptChangedAt;

        public void Initialize(RtsSimulationDriver simulationDriver, VerticalSliceProgressTracker tracker)
        {
            driver = simulationDriver;
            progressTracker = tracker;
            RefreshPrompt();
        }

        void Awake()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            RefreshPrompt();
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;

            RefreshPrompt();
        }

        public string GetPrompt()
        {
            RefreshPrompt();
            return CurrentPrompt;
        }

        public bool HasFreshPrompt(float seconds)
        {
            return Time.realtimeSinceStartup - promptChangedAt <= seconds;
        }

        void RefreshPrompt()
        {
            var next = BuildPrompt();
            if (next == lastPrompt)
            {
                CurrentPrompt = next;
                return;
            }

            lastPrompt = next;
            CurrentPrompt = next;
            promptChangedAt = Time.realtimeSinceStartup;
        }

        string BuildPrompt()
        {
            if (driver == null || driver.LatestSnapshot == null)
                return "Start the vertical slice to begin.";

            var snapshot = driver.LatestSnapshot;
            if (snapshot.Match.Phase == MatchPhase.Won || snapshot.Match.Phase == MatchPhase.Lost || snapshot.Match.Phase == MatchPhase.Draw)
                return "Match complete. Restart or return to the menu from the result screen.";

            if (driver.HasPlacementMode)
                return "Place " + driver.PendingPlacementTypeId + " on a clear footprint, or press Escape to cancel.";

            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            if (progressTracker != null)
            {
                progressTracker.Refresh();
                return progressTracker.currentChecklistPrompt;
            }

            if (driver.SelectedActorIds.Count == 0)
                return "Select your Fabrication Hub, then build from the right sidebar.";

            return "Use the right sidebar to build economy, train units, scout, and destroy the enemy base.";
        }
    }
}
