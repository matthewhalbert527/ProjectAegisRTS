using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class DesktopUiCommandRouter : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public RtsStatusLog statusLog;

        public DesktopCommandMode CurrentMode { get; private set; }

        public void Initialize(RtsSimulationDriver simulationDriver, RtsStatusLog log)
        {
            driver = simulationDriver;
            statusLog = log;
            CurrentMode = DesktopCommandMode.Normal;
        }

        public RtsCommandResult QueueProduction(string typeId)
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryQueueProduction(typeId);
            Log(result);
            return result;
        }

        public RtsCommandResult EnterPlacementMode()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryEnterPlacementModeForFirstPending();
            Log(result);
            return result;
        }

        public RtsCommandResult CancelPlacement()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryCancelPlacement();
            CurrentMode = DesktopCommandMode.Normal;
            Log(result);
            return result;
        }

        public RtsCommandResult PlaceAtHoveredCell()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");
            if (!driver.HasHoveredCell)
                return LogAndReturn(RtsCommandResult.Fail("NoHoveredCell", "Hover a board cell before placing."));

            var result = driver.TryPlacePendingBuildingAtCell(driver.HoveredCell);
            if (result.Success)
                CurrentMode = DesktopCommandMode.Normal;
            Log(result);
            return result;
        }

        public void SetMoveMode()
        {
            CurrentMode = DesktopCommandMode.Move;
            Info("Move mode: left-click a board cell or right-click to issue a move order.");
        }

        public void SetAttackPlaceholderMode()
        {
            CurrentMode = DesktopCommandMode.AttackPlaceholder;
            Warning("Attack mode is a Stage 2 placeholder.");
        }

        public RtsCommandResult IssueMoveToCell(Int2 cell)
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryIssueMoveSelectedToCell(cell);
            if (result.Success)
                CurrentMode = DesktopCommandMode.Normal;
            Log(result);
            return result;
        }

        public RtsCommandResult SelectAtCell(Int2 cell)
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TrySelectActorAtCell(cell);
            Log(result);
            return result;
        }

        public RtsCommandResult StopSelected()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryStopSelected();
            Log(result);
            return result;
        }

        public RtsCommandResult TogglePause()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TogglePause();
            Log(result);
            return result;
        }

        public RtsCommandResult StepTick()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.StepOneTick();
            Log(result);
            return result;
        }

        public RtsCommandResult TriggerLowPowerDemo()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryForceLowPowerOrCreateLowPowerDemoCondition();
            Log(result);
            return result;
        }

        public RtsCommandResult CancelProduction(int queueItemId)
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryCancelProduction(queueItemId);
            Log(result);
            return result;
        }

        public RtsCommandResult TogglePowerSelected()
        {
            if (!EnsureDriver())
                return RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available.");

            var result = driver.TryTogglePowerSelected();
            Log(result);
            return result;
        }

        public void CancelActiveMode()
        {
            if (driver != null && driver.HasPlacementMode)
            {
                CancelPlacement();
                return;
            }

            if (CurrentMode != DesktopCommandMode.Normal)
            {
                CurrentMode = DesktopCommandMode.Normal;
                Info("Command mode cancelled.");
                return;
            }

            if (driver != null)
                Log(driver.ClearSelection());
        }

        public void Placeholder(string commandName)
        {
            Warning(commandName + " is not implemented in Stage 2.");
        }

        bool EnsureDriver()
        {
            return driver != null;
        }

        RtsCommandResult LogAndReturn(RtsCommandResult result)
        {
            Log(result);
            return result;
        }

        void Log(RtsCommandResult result)
        {
            if (statusLog != null)
                statusLog.AddResult(result);
            else
                Debug.Log(result.ToString());
        }

        void Info(string message)
        {
            if (statusLog != null)
                statusLog.AddInfo(message);
            else
                Debug.Log(message);
        }

        void Warning(string message)
        {
            if (statusLog != null)
                statusLog.AddWarning(message);
            else
                Debug.LogWarning(message);
        }
    }
}
