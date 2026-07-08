using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class DesktopSidebarController : MonoBehaviour
    {
        RtsSimulationDriver driver;
        VerticalSliceMissionFlowController missionFlowController;
        VerticalSliceProgressTracker progressTracker;
        Text titleText;
        Text readoutText;
        float cncReadoutTop = 266f;
        float cncReadoutHeight = 74f;

        public void Initialize(
            RtsSimulationDriver simulationDriver,
            DesktopUiCommandRouter router,
            ProductionCategoryTabs tabs,
            ProductionGridController grid,
            ProductionQueuePanel queue,
            PlacementModePanel placement,
            SelectionPanelController selection,
            MinimapPlaceholderController minimap,
            VerticalSliceProgressTracker tracker = null,
            VerticalSliceMissionFlowController missionFlow = null)
        {
            driver = simulationDriver;
            progressTracker = tracker;
            missionFlowController = missionFlow;
            BuildIfNeeded();
        }

        void Update()
        {
            Refresh();
        }

        void BuildIfNeeded()
        {
            if (titleText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.05f, 0.06f, 0.08f, 0.92f));

            titleText = GetOrCreateText("Title", "ProjectAegisRTS", 18, Color.white, TextAnchor.MiddleLeft);
            titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.offsetMin = new Vector2(14f, -42f);
            titleText.rectTransform.offsetMax = new Vector2(-14f, -8f);

            readoutText = GetOrCreateText("Readout", "Waiting for simulation...", 14, new Color(0.88f, 0.94f, 0.98f, 1f), TextAnchor.UpperLeft);
            readoutText.rectTransform.anchorMin = new Vector2(0f, 1f);
            readoutText.rectTransform.anchorMax = new Vector2(1f, 1f);
            ApplyCncReadoutLayout(cncReadoutTop, cncReadoutHeight);
        }

        public void ApplyCncReadoutLayout(float top, float height)
        {
            cncReadoutTop = top;
            cncReadoutHeight = height;
            if (titleText != null)
            {
                titleText.text = "ProjectAegisRTS";
                titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
                titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
                titleText.rectTransform.offsetMin = new Vector2(14f, -34f);
                titleText.rectTransform.offsetMax = new Vector2(-14f, -6f);
            }

            if (readoutText == null)
                return;

            readoutText.rectTransform.anchorMin = new Vector2(0f, 1f);
            readoutText.rectTransform.anchorMax = new Vector2(1f, 1f);
            readoutText.rectTransform.offsetMin = new Vector2(14f, -top - height);
            readoutText.rectTransform.offsetMax = new Vector2(-14f, -top);
        }

        Text GetOrCreateText(string objectName, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var child = transform.Find(objectName);
            var existing = child != null ? child.GetComponent<Text>() : null;
            return existing != null ? existing : RtsUiFactory.CreateText(transform, objectName, text, fontSize, color, anchor);
        }

        void Refresh()
        {
            if (driver == null || driver.LatestSnapshot == null || readoutText == null)
                return;

            var snapshot = driver.LatestSnapshot;
            var player = driver.GetLocalPlayerSnapshot();
            if (player == null)
            {
                readoutText.text = "No local player.";
                return;
            }

            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            if (progressTracker != null)
                progressTracker.Refresh();
            if (missionFlowController == null)
                missionFlowController = FindAnyObjectByType<VerticalSliceMissionFlowController>();
            if (missionFlowController != null)
                missionFlowController.Refresh();

            var guidance = missionFlowController != null ? missionFlowController.CurrentInstructionText : (progressTracker == null ? "Follow the checklist." : progressTracker.currentChecklistPrompt);
            var action = missionFlowController != null ? missionFlowController.NextRecommendedAction : (progressTracker == null ? "Follow checklist." : ("Recommended: " + progressTracker.recommendedTypeId));

            readoutText.text =
                "Credits: " + player.Credits + "   Power: " + player.Power.Generated + "/" + player.Power.Consumed + " " + player.Power.State + "\n" +
                "Mode: " + driver.CommandMode + "\n" +
                action + "\n" +
                Shorten(guidance, 82);
        }

        static string Shorten(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;
            return value.Substring(0, maxLength - 3) + "...";
        }
    }
}
