using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class VerticalSliceChecklistHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public VerticalSliceMissionFlowController missionFlowController;
        public VerticalSliceProgressTracker progressTracker;
        public bool visible = true;
        public bool expanded;
        public KeyCode toggleKey = KeyCode.C;
        public KeyCode expandKey = KeyCode.Tab;
        public Rect area = PlayerHudLayout.ChecklistArea;

        public void Initialize(RtsSimulationDriver simulationDriver, VerticalSliceProgressTracker tracker)
        {
            driver = simulationDriver;
            progressTracker = tracker;
        }

        void Awake()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            if (missionFlowController == null)
                missionFlowController = FindAnyObjectByType<VerticalSliceMissionFlowController>();
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
            if (Input.GetKeyDown(expandKey))
                expanded = !expanded;
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            if (progressTracker == null)
                return;
            if (missionFlowController == null)
                missionFlowController = FindAnyObjectByType<VerticalSliceMissionFlowController>();

            progressTracker.Refresh();
            if (missionFlowController != null)
                missionFlowController.Refresh();
            var current = progressTracker.GetCurrentStepIndex();
            var start = expanded ? 0 : Mathf.Max(0, current - 1);
            var end = expanded ? VerticalSliceProgressTracker.MissionStepCount - 1 : Mathf.Min(VerticalSliceProgressTracker.MissionStepCount - 1, current + 3);

            var previousMatrix = PlayerHudLayout.BeginArea(area);
            GUILayout.Label("Mission Flow");
            GUILayout.Label(progressTracker.ObjectiveStatus());
            GUILayout.Label("Step " + (current + 1) + "/" + VerticalSliceProgressTracker.MissionStepCount + ": " + progressTracker.GetCurrentStepLabel());
            GUILayout.Space(4f);
            for (var i = start; i <= end; i++)
            {
                var state = progressTracker.IsStepComplete(i) ? "[x] " : (i == current ? ">>  " : "[ ] ");
                GUILayout.Label(state + progressTracker.GetStepLabel(i));
            }
            GUILayout.Space(4f);
            var recommendation = missionFlowController == null ? progressTracker.recommendedTypeId : missionFlowController.NextRecommendedAction;
            GUILayout.Label(string.IsNullOrEmpty(recommendation) ? "Next: follow the prompt." : recommendation);
            GUILayout.Label("C hides checklist. Tab expands. F1/H controls.");
            PlayerHudLayout.EndArea(previousMatrix);
        }
    }
}
