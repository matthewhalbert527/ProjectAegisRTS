using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class VerticalSliceChecklistHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public VerticalSliceProgressTracker progressTracker;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.C;
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
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            if (progressTracker == null)
                return;

            progressTracker.Refresh();
            var previousMatrix = PlayerHudLayout.BeginArea(area);
            GUILayout.Label("Build-Order Checklist");
            GUILayout.Label(progressTracker.ObjectiveStatus());
            GUILayout.Space(4f);
            for (var i = 0; i < 11; i++)
                GUILayout.Label((progressTracker.IsStepComplete(i) ? "[x] " : "[ ] ") + progressTracker.GetStepLabel(i));
            GUILayout.Space(4f);
            GUILayout.Label("C hides checklist. F1/H shows controls.");
            PlayerHudLayout.EndArea(previousMatrix);
        }
    }
}
