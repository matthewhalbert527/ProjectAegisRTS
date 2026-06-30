using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Scenario
{
    public sealed class VerticalSliceScenarioController : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public MatchObjectiveHud objectiveHud;
        public IntegratedSystemsStatusHud statusHud;
        public VerticalSliceDebugActions debugActions;
        public bool startOnInitialize = true;
        public bool resetWorldOnInitialize = true;

        public string LastStatus { get; private set; }

        public void Initialize(RtsSimulationDriver simulationDriver, MatchObjectiveHud matchHud, IntegratedSystemsStatusHud systemsHud, VerticalSliceDebugActions actions)
        {
            driver = simulationDriver;
            objectiveHud = matchHud;
            statusHud = systemsHud;
            debugActions = actions;

            if (driver == null)
            {
                LastStatus = "Driver missing.";
                return;
            }

            if (resetWorldOnInitialize)
                LastStatus = driver.TryCreateVerticalSliceWorld().ToString();

            if (startOnInitialize)
                StartScenario();
        }

        public RtsCommandResult StartScenario()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            if (driver.LatestSnapshot == null || driver.LatestSnapshot.Match.ScenarioId != "stage16_vertical_slice")
                Remember(driver.TryCreateVerticalSliceWorld());

            return Remember(driver.TryStartMatch());
        }

        public RtsCommandResult ResetScenario()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            var reset = Remember(driver.TryCreateVerticalSliceWorld());
            if (reset.Success && startOnInitialize)
                return StartScenario();

            return reset;
        }

        public RtsCommandResult TogglePause()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TogglePause());
        }

        public RtsCommandResult StepOneTick()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.StepOneTick());
        }

        RtsCommandResult Remember(RtsCommandResult result)
        {
            LastStatus = result == null ? string.Empty : result.ToString();
            return result;
        }
    }
}
