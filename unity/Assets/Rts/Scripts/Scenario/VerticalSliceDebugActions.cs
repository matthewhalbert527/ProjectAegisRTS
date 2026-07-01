using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Scenario
{
    public sealed class VerticalSliceDebugActions : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public int creditGrantAmount = 1000;
        public Int2 defaultHarvestCell = new Int2(14, 11);

        public string LastAction { get; private set; }

        public void Initialize(RtsSimulationDriver simulationDriver)
        {
            driver = simulationDriver;
        }

        public RtsCommandResult ResetVerticalSlice()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TryCreateVerticalSliceWorld());
        }

        public RtsCommandResult StartMatch()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TryStartMatch());
        }

        public RtsCommandResult GrantCredits()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TryGrantScenarioCredits(creditGrantAmount));
        }

        public RtsCommandResult RevealMap()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TryRevealScenarioMap());
        }

        public RtsCommandResult SelectHarvester()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TrySelectFirstOwnedActorOfType("harvester"));
        }

        public RtsCommandResult SelectCombatUnit()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TrySelectFirstOwnedCombatActor());
        }

        public RtsCommandResult QueueProduction(string typeId)
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TryQueueProduction(typeId));
        }

        public RtsCommandResult IssueHarvest()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            var select = driver.TrySelectFirstOwnedActorOfType("harvester");
            if (!select.Success)
                return Remember(select);

            return Remember(driver.TryIssueHarvestSelectedAtCell(FindHarvestCell()));
        }

        public RtsCommandResult IssueAttack()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            int targetActorId;
            if (!driver.TryFindFirstEnemyCombatActor(out targetActorId))
                return Remember(RtsCommandResult.Fail("EnemyMissing", "No enemy combat actor is available."));

            var select = driver.TrySelectOwnedCombatGroup();
            if (!select.Success)
                return Remember(select);

            return Remember(driver.TryIssueDebugAttackSelectedToKnownActor(targetActorId));
        }

        public RtsCommandResult DestroyEnemyBase()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TryDestroyEnemyBaseForScenario());
        }

        public RtsCommandResult DestroyPlayerBase()
        {
            if (driver == null)
                return Remember(RtsCommandResult.Fail("DriverMissing", "Simulation driver is not available."));

            return Remember(driver.TryDestroyPlayerBaseForScenario());
        }

        RtsCommandResult Remember(RtsCommandResult result)
        {
            LastAction = result == null ? string.Empty : result.ToString();
            return result;
        }

        Int2 FindHarvestCell()
        {
            var snapshot = driver == null ? null : driver.LatestSnapshot;
            if (snapshot == null)
                return defaultHarvestCell;

            for (var i = 0; i < snapshot.Economy.Resources.Count; i++)
            {
                var resource = snapshot.Economy.Resources[i];
                if (!resource.IsDepleted && resource.Amount > 0)
                    return resource.Cell;
            }

            return defaultHarvestCell;
        }
    }
}
