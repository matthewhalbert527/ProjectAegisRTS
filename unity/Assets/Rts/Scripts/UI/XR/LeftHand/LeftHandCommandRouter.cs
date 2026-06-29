using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Selection;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class LeftHandCommandRouter : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public RtsStatusLog statusLog;
        public LeftHandBuildMenuController buildMenu;
        public LeftHandSelectionController selectionController;
        public Stage4ModeCoordinator modeCoordinator;

        public string LastCommandResult { get; private set; }

        public void Initialize(RtsSimulationDriver simulationDriver, RtsStatusLog log)
        {
            driver = simulationDriver;
            statusLog = log;
        }

        public RtsCommandResult QueueProduction(string actorTypeId)
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);

            var result = driver.TryQueueProduction(actorTypeId);
            if (result.Success && driver.HasPlacementMode && modeCoordinator != null)
                modeCoordinator.SetMode(LeftHandCommandMode.Placement);
            return LogAndReturn(result);
        }

        public RtsCommandResult EnterPlacementMode(string actorTypeId)
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);

            RtsCommandResult result;
            if (!string.IsNullOrEmpty(actorTypeId))
                result = driver.TryQueueProduction(actorTypeId);
            else
                result = driver.TryEnterPlacementModeForFirstPending();

            if (result.Success && driver.HasPlacementMode && modeCoordinator != null)
                modeCoordinator.SetMode(LeftHandCommandMode.Placement);
            return LogAndReturn(result);
        }

        public RtsCommandResult ConfirmPlacementAtHoveredCell()
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);
            if (!driver.HasHoveredCell)
                return LogAndReturn(RtsCommandResult.Fail("NoHoveredCell", "Hover a board cell before placing."));

            return ConfirmPlacementAtCell(driver.HoveredCell);
        }

        public RtsCommandResult ConfirmPlacementAtCell(Int2 cell)
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);

            var result = driver.TryPlacePendingBuildingAtCell(cell);
            if (result.Success && modeCoordinator != null)
                modeCoordinator.SetMode(buildMenu != null && buildMenu.IsOpen ? LeftHandCommandMode.BuildItemSelect : LeftHandCommandMode.Idle);
            return LogAndReturn(result);
        }

        public RtsCommandResult CancelPlacement()
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);

            var result = driver.TryCancelPlacement();
            if (modeCoordinator != null)
                modeCoordinator.SetMode(buildMenu != null && buildMenu.IsOpen ? LeftHandCommandMode.BuildItemSelect : LeftHandCommandMode.Idle);
            return LogAndReturn(result);
        }

        public RtsCommandResult SelectCandidate(LeftHandSelectionCandidate candidate)
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);
            if (candidate == null)
                return LogAndReturn(driver.ClearSelection());

            return LogAndReturn(driver.SetSelectedActorIds(new[] { candidate.ActorId }));
        }

        public RtsCommandResult AddOrRemoveCandidate(LeftHandSelectionCandidate candidate)
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);
            if (candidate == null)
                return LogAndReturn(RtsCommandResult.Fail("NoCandidate", "No selection candidate is under the left-hand ray."));

            return LogAndReturn(driver.AddOrRemoveSelectedActor(candidate.ActorId));
        }

        public RtsCommandResult ClearSelection()
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);

            return LogAndReturn(driver.ClearSelection());
        }

        public void CycleSelectionCandidate(int direction)
        {
            if (selectionController != null)
                selectionController.CycleCandidate(direction);
        }

        public void ToggleMenu()
        {
            if (buildMenu != null)
                buildMenu.ToggleMenu();
        }

        public void SetCategory(LeftHandBuildCategory category)
        {
            if (buildMenu != null)
                buildMenu.SetCategory(category);
        }

        public RtsCommandResult TriggerLowPowerDemo()
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);

            return LogAndReturn(driver.TryForceLowPowerOrCreateLowPowerDemoCondition());
        }

        bool EnsureDriver(out RtsCommandResult failure)
        {
            failure = null;
            if (driver != null)
                return true;

            failure = RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");
            return false;
        }

        RtsCommandResult LogAndReturn(RtsCommandResult result)
        {
            LastCommandResult = result == null ? string.Empty : result.ToString();
            if (statusLog != null)
                statusLog.AddResult(result);
            else if (result != null)
                Debug.Log(result.ToString());
            return result;
        }
    }
}
