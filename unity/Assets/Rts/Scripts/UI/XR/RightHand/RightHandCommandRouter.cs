using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.XR.RightHand
{
    public sealed class RightHandCommandRouter : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public RtsStatusLog statusLog;
        public CommandPreviewRenderer previewRenderer;

        public RightHandCommandMode CurrentMode { get; private set; }
        public string LastCommandResult { get; private set; }

        public void Initialize(RtsSimulationDriver simulationDriver, RtsStatusLog log, CommandPreviewRenderer commandPreview)
        {
            driver = simulationDriver;
            statusLog = log;
            previewRenderer = commandPreview;
            CurrentMode = RightHandCommandMode.Idle;
        }

        public void EnterMoveMode()
        {
            CurrentMode = RightHandCommandMode.Move;
            Info("Right-hand move mode.");
        }

        public void EnterAttackMode()
        {
            CurrentMode = RightHandCommandMode.Attack;
            Info("Right-hand attack mode.");
        }

        public void EnterForceAttackMode()
        {
            CurrentMode = RightHandCommandMode.ForceAttack;
            Info("Right-hand force-attack placeholder mode.");
        }

        public void ToggleBoardManipulationMode()
        {
            CurrentMode = CurrentMode == RightHandCommandMode.BoardManipulation ? RightHandCommandMode.Idle : RightHandCommandMode.BoardManipulation;
            Info(CurrentMode == RightHandCommandMode.BoardManipulation ? "Right-hand board manipulation mode." : "Right-hand command mode.");
        }

        public RtsCommandResult IssueMoveToHoveredCell()
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);
            if (!driver.HasHoveredCell)
                return LogAndReturn(RtsCommandResult.Fail("NoHoveredCell", "Point the right-hand ray at a board cell."));

            return IssueMoveToCell(driver.HoveredCell);
        }

        public RtsCommandResult IssueMoveToCell(Int2 cell)
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);
            if (driver.HasPlacementMode)
                return LogAndReturn(RtsCommandResult.Fail("PlacementActive", "Finish or cancel building placement before issuing tactical commands."));

            var result = driver.TryIssueMoveSelectedToCell(cell);
            if (previewRenderer != null)
            {
                if (result.Success)
                    previewRenderer.ShowMoveTarget(cell);
                else
                    previewRenderer.ShowInvalidTarget(cell);
            }

            return LogAndReturn(result);
        }

        public RtsCommandResult IssueContextCommandAtCell(Int2 cell)
        {
            if (CurrentMode == RightHandCommandMode.Attack || CurrentMode == RightHandCommandMode.ForceAttack)
                return IssueAttackPlaceholderAtCell(cell, CurrentMode == RightHandCommandMode.ForceAttack);

            return IssueMoveToCell(cell);
        }

        public RtsCommandResult IssueAttackPlaceholderAtCell(Int2 cell, bool forceAttack)
        {
            if (!EnsureDriver(out var missing))
                return LogAndReturn(missing);
            if (driver.HasPlacementMode)
                return LogAndReturn(RtsCommandResult.Fail("PlacementActive", "Finish or cancel building placement before issuing tactical commands."));

            if (previewRenderer != null)
                previewRenderer.ShowAttackTarget(cell);

            if (forceAttack)
                return LogAndReturn(RtsCommandResult.Ok("Force-attack placeholder target " + cell + "."));

            var result = driver.TryIssueAttackSelectedAtCell(cell);
            if (!result.Success && !driver.UseCombatDemoWorld)
                return LogAndReturn(RtsCommandResult.Ok("Attack placeholder target " + cell + "."));

            return LogAndReturn(result);
        }

        public void CancelCommandMode()
        {
            CurrentMode = RightHandCommandMode.Idle;
            if (previewRenderer != null)
                previewRenderer.ClearPreview();
            Info("Right-hand command mode cancelled.");
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

        void Info(string message)
        {
            LastCommandResult = message;
            if (statusLog != null)
                statusLog.AddInfo(message);
            else
                Debug.Log(message);
        }
    }
}
