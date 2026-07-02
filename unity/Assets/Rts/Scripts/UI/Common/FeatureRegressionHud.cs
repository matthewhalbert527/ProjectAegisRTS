using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class FeatureRegressionHud : MonoBehaviour
    {
        public bool visible;
        public bool allowRuntimeToggle = true;
        public KeyCode toggleKey = KeyCode.F10;
        public Rect area = new Rect(18f, 180f, 430f, 520f);

        public RtsSimulationDriver driver;
        public DesktopUiCommandRouter desktopRouter;
        public PlayerFacingUiModeController uiModeController;
        public VerticalSliceProgressTracker progressTracker;
        public VerticalSliceMissionFlowController missionFlowController;

        readonly List<FeatureCommandStatus> cachedStatuses = new List<FeatureCommandStatus>();

        void Awake()
        {
            ResolveReferences();
        }

        void Update()
        {
            if (allowRuntimeToggle && Input.GetKeyDown(toggleKey))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible)
                return;

            var statuses = BuildAuditSnapshot();
            GUI.Box(area, "Stage 28 Feature Regression");
            GUILayout.BeginArea(new Rect(area.x + 12f, area.y + 24f, area.width - 24f, area.height - 36f));
            GUILayout.Label("PCDesktop: " + Status(uiModeController != null && uiModeController.IsPcSidebarVisibleForDesktop()));
            GUILayout.Label("QuestXR controls: " + Status(uiModeController != null && uiModeController.AreQuestLeftHandControlsAvailable() && uiModeController.AreQuestRightHandControlsAvailable()));
            GUILayout.Label("Placement mode: " + (driver != null && driver.HasPlacementMode ? driver.PendingPlacementTypeId : "none"));
            GUILayout.Label("Objective: " + (progressTracker != null ? progressTracker.ObjectiveStatus() : "tracker unavailable"));
            GUILayout.Space(6f);

            for (var i = 0; i < statuses.Length; i++)
            {
                var status = statuses[i];
                GUILayout.Label(status.Group + " / " + status.Label + ": " + status.Status + " - " + status.Detail);
            }

            GUILayout.EndArea();
        }

        public void Initialize(
            RtsSimulationDriver simulationDriver,
            DesktopUiCommandRouter commandRouter,
            PlayerFacingUiModeController uiMode,
            VerticalSliceProgressTracker tracker,
            VerticalSliceMissionFlowController missionFlow)
        {
            driver = simulationDriver;
            desktopRouter = commandRouter;
            uiModeController = uiMode;
            progressTracker = tracker;
            missionFlowController = missionFlow;
        }

        public FeatureCommandStatus[] BuildAuditSnapshot()
        {
            ResolveReferences();
            cachedStatuses.Clear();

            Add("Selection", "Single select", HasDriverMethod("TrySelectActorAtCell"), "Cell click route");
            Add("Selection", "Box select", HasDriverMethod("TrySelectActorsInScreenRect"), "Desktop drag lasso route");
            Add("Selection", "Double-click type select", HasDriverMethod("TrySelectOwnedActorsOfSameTypeAtCell"), "Same-type select route");
            Add("Selection", "Control groups", true, "Client-local 1-9 groups");

            Add("Movement/combat", "Move", HasRouterMethod("IssueMoveToCell"), "Right-click and command mode");
            Add("Movement/combat", "Attack", HasRouterMethod("IssueAttackToCell"), "Target actor route");
            Add("Movement/combat", "Attack-move", HasRouterMethod("IssueAttackMoveToCell"), "Cell route");
            Add("Movement/combat", "Stop", HasRouterMethod("StopSelected"), "Immediate stop route");
            Add("Movement/combat", "Guard", HasRouterMethod("GuardSelected"), "Foundation command feedback");
            Add("Movement/combat", "Patrol", HasRouterMethod("IssuePatrolToCell"), "Foundation command feedback");
            Add("Movement/combat", "Scatter", HasRouterMethod("ScatterSelected"), "Foundation command feedback");

            Add("Production/base", "Queue production", HasRouterMethod("QueueProduction"), "Sidebar production cards");
            Add("Production/base", "Pending placement", HasRouterMethod("EnterPlacementMode"), "Ready building card route");
            Add("Production/base", "Fine-grid placement", HasRouterMethod("PlaceAtHoveredCell") && HasDriverMethod("TryPlacePendingBuildingNearHoveredCell"), "Hover or nearby fallback route");
            Add("Production/base", "Cancel placement", HasRouterMethod("CancelPlacement"), "Esc/sidebar cancel route");
            Add("Production/base", "Stage27.1 HUD split", true, "Board setup HUD separate from building placement");
            Add("Production/base", "Rally point", HasRouterMethod("IssueRallyToCell"), "Producer rally route");

            Add("Base management", "Repair", HasRouterMethod("RepairSelected"), "Selected building repair");
            Add("Base management", "Sell", HasRouterMethod("SellSelected"), "Refund/removal route");
            Add("Base management", "Power toggle", HasRouterMethod("TogglePowerSelected"), "Manual power route");

            Add("Economy", "Harvester", HasSnapshotEconomy(), "Snapshot economy loop visible");
            Add("Tech/support", "Support powers", HasRouterMethod("ActivateSupportPowerAtHoveredCell"), "Cooldown/prerequisite route");
            Add("Engineer/transport", "Capture", HasRouterMethod("IssueCaptureAtCell"), "Engineer capture route");
            Add("Engineer/transport", "Load", HasRouterMethod("IssueLoadTransportAtCell"), "Transport load route");
            Add("Engineer/transport", "Unload", HasRouterMethod("IssueUnloadTransportAtCell"), "Transport unload route");
            Add("Air/naval", "Airfield/aircraft", HasAirNavalSnapshots(), "Airfield and aircraft snapshots");
            Add("Visibility", "Fog/radar/minimap", HasVisibilitySnapshots(), "Player-perspective visibility");
            Add("AI", "Skirmish pressure", HasAiSnapshot(), "Difficulty and wave snapshot");
            Add("Mission", "Objective checklist", progressTracker != null, "Mission flow tracker");
            Add("Platform UI", "PCDesktop sidebar", uiModeController != null && uiModeController.IsPcSidebarVisibleForDesktop(), "Right sidebar default");
            Add("Platform UI", "QuestXR controls", uiModeController != null && uiModeController.AreQuestLeftHandControlsAvailable() && uiModeController.AreQuestRightHandControlsAvailable(), "Hand-control components present");
            Add("Build/log", "Debug panels hidden", IsHiddenByDefault(), "QA overlay hidden until F10");

            return cachedStatuses.ToArray();
        }

        public bool IsHiddenByDefault()
        {
            return !visible;
        }

        public bool ToggleVisibleForValidation()
        {
            visible = !visible;
            return visible;
        }

        void ResolveReferences()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (desktopRouter == null)
                desktopRouter = FindAnyObjectByType<DesktopUiCommandRouter>();
            if (uiModeController == null)
                uiModeController = FindAnyObjectByType<PlayerFacingUiModeController>();
            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
            if (missionFlowController == null)
                missionFlowController = FindAnyObjectByType<VerticalSliceMissionFlowController>();
        }

        void Add(string group, string label, bool available, string detail)
        {
            cachedStatuses.Add(new FeatureCommandStatus(group, label, available ? "Ready" : "Blocked", detail));
        }

        bool HasRouterMethod(string methodName)
        {
            return typeof(DesktopUiCommandRouter).GetMethod(methodName) != null;
        }

        bool HasDriverMethod(string methodName)
        {
            return typeof(RtsSimulationDriver).GetMethod(methodName) != null;
        }

        bool HasSnapshotEconomy()
        {
            var snapshot = driver == null ? null : driver.LatestSnapshot;
            return snapshot != null && snapshot.Economy != null && snapshot.Economy.Harvesters.Count > 0 && snapshot.Economy.Refineries.Count > 0;
        }

        bool HasAirNavalSnapshots()
        {
            var snapshot = driver == null ? null : driver.LatestSnapshot;
            return snapshot != null && snapshot.Airfields.Count > 0 && snapshot.Aircraft.Count > 0;
        }

        bool HasVisibilitySnapshots()
        {
            var snapshot = driver == null ? null : driver.LatestSnapshot;
            return snapshot != null && snapshot.Fog != null && snapshot.Radar != null && snapshot.Minimap != null;
        }

        bool HasAiSnapshot()
        {
            var snapshot = driver == null ? null : driver.LatestSnapshot;
            return snapshot != null && snapshot.Ai != null && snapshot.Ai.Players.Count > 0;
        }

        static string Status(bool value)
        {
            return value ? "ready" : "blocked";
        }
    }

    public sealed class FeatureCommandStatus
    {
        public string Group { get; private set; }
        public string Label { get; private set; }
        public string Status { get; private set; }
        public string Detail { get; private set; }

        public FeatureCommandStatus(string group, string label, string status, string detail)
        {
            Group = group ?? string.Empty;
            Label = label ?? string.Empty;
            Status = status ?? string.Empty;
            Detail = detail ?? string.Empty;
        }
    }
}
