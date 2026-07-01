using ProjectAegisRTS.Match;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Scenario
{
    public sealed class VerticalSliceProgressTracker : MonoBehaviour
    {
        public RtsSimulationDriver driver;

        public bool hasSelectedFabricationHub;
        public bool hasPowerPlant;
        public bool hasRefinery;
        public bool hasHarvester;
        public bool hasHarvesterCargoOrUnload;
        public bool hasBarracks;
        public bool hasInfantry;
        public bool hasWarFactory;
        public bool hasCombatVehicle;
        public bool hasDiscoveredEnemy;
        public bool hasDamagedEnemyBase;
        public bool enemyBaseDestroyed;
        public bool hasWon;
        public bool hasLost;

        public string recommendedTypeId = "power_plant";
        public string currentChecklistPrompt = "Select your Fabrication Hub.";

        public void Initialize(RtsSimulationDriver simulationDriver)
        {
            driver = simulationDriver;
            Refresh();
        }

        void Awake()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
        }

        void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (driver == null || driver.LatestSnapshot == null)
            {
                ResetProgress();
                return;
            }

            var snapshot = driver.LatestSnapshot;
            var localPlayerId = snapshot.Match.LocalPlayerId != 0 ? snapshot.Match.LocalPlayerId : driver.PlayerId;

            hasSelectedFabricationHub = IsSelectedOwnedType("fabrication_hub", localPlayerId);
            hasPowerPlant = HasOwnedActor(snapshot, localPlayerId, "power_plant");
            hasRefinery = HasOwnedActor(snapshot, localPlayerId, "refinery");
            hasHarvester = HasOwnedActor(snapshot, localPlayerId, "harvester");
            hasBarracks = HasOwnedActor(snapshot, localPlayerId, "barracks");
            hasInfantry = HasOwnedActor(snapshot, localPlayerId, "rifle_infantry");
            hasWarFactory = HasOwnedActor(snapshot, localPlayerId, "war_factory");
            hasCombatVehicle = HasOwnedActor(snapshot, localPlayerId, "light_tank");
            hasDiscoveredEnemy = HasVisibleEnemy(snapshot, localPlayerId);
            hasDamagedEnemyBase = HasVisibleDamagedEnemyBase(snapshot, localPlayerId) || ObjectiveState(snapshot, "destroy_enemy_base") == ScenarioObjectiveState.Completed;
            enemyBaseDestroyed = snapshot.Match.Phase == MatchPhase.Won ||
                snapshot.Match.LocalPlayerOutcome == PlayerOutcome.Victory ||
                ObjectiveState(snapshot, "destroy_enemy_base") == ScenarioObjectiveState.Completed;
            hasWon = snapshot.Match.Phase == MatchPhase.Won || snapshot.Match.LocalPlayerOutcome == PlayerOutcome.Victory;
            hasLost = snapshot.Match.Phase == MatchPhase.Lost || snapshot.Match.LocalPlayerOutcome == PlayerOutcome.Defeat;
            hasHarvesterCargoOrUnload = HasEconomyProgress(snapshot);

            recommendedTypeId = DetermineRecommendedType();
            currentChecklistPrompt = DeterminePrompt();
        }

        public bool IsStepComplete(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0: return hasSelectedFabricationHub;
                case 1: return hasPowerPlant;
                case 2: return hasRefinery;
                case 3: return hasHarvesterCargoOrUnload;
                case 4: return hasBarracks;
                case 5: return hasInfantry;
                case 6: return hasWarFactory;
                case 7: return hasCombatVehicle || hasHarvester;
                case 8: return hasDiscoveredEnemy;
                case 9: return hasDamagedEnemyBase;
                case 10: return enemyBaseDestroyed || hasWon;
                default: return false;
            }
        }

        public string GetStepLabel(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0: return "Select your Fabrication Hub.";
                case 1: return "Build Power Plant.";
                case 2: return "Build Refinery near resources.";
                case 3: return "Harvest resources.";
                case 4: return "Build Barracks.";
                case 5: return "Train infantry.";
                case 6: return "Build War Factory.";
                case 7: return "Produce light tank or harvester.";
                case 8: return "Scout toward enemy base.";
                case 9: return "Attack enemy base.";
                case 10: return "Destroy enemy base to win.";
                default: return string.Empty;
            }
        }

        public string ObjectiveStatus()
        {
            if (hasWon)
                return "Victory: enemy base destroyed.";
            if (hasLost)
                return "Defeat: player base destroyed.";
            if (enemyBaseDestroyed)
                return "Enemy base objective complete.";
            if (hasDamagedEnemyBase)
                return "Enemy base damaged. Keep attacking.";
            if (hasDiscoveredEnemy)
                return "Enemy base scouted. Prepare an attack.";
            return "Enemy base not scouted yet.";
        }

        void ResetProgress()
        {
            hasSelectedFabricationHub = false;
            hasPowerPlant = false;
            hasRefinery = false;
            hasHarvester = false;
            hasHarvesterCargoOrUnload = false;
            hasBarracks = false;
            hasInfantry = false;
            hasWarFactory = false;
            hasCombatVehicle = false;
            hasDiscoveredEnemy = false;
            hasDamagedEnemyBase = false;
            enemyBaseDestroyed = false;
            hasWon = false;
            hasLost = false;
            recommendedTypeId = "power_plant";
            currentChecklistPrompt = "Select your Fabrication Hub.";
        }

        bool IsSelectedOwnedType(string typeId, int ownerId)
        {
            for (var i = 0; i < driver.SelectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                if (driver.TryGetActorSnapshot(driver.SelectedActorIds[i], out actor) &&
                    actor.OwnerId == ownerId &&
                    actor.TypeId == typeId &&
                    !actor.IsDestroyed)
                    return true;
            }

            return false;
        }

        static bool HasOwnedActor(WorldSnapshot snapshot, int ownerId, string typeId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId == ownerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    return true;
            }

            return false;
        }

        static bool HasVisibleEnemy(WorldSnapshot snapshot, int localPlayerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != localPlayerId && !actor.IsDestroyed)
                    return true;
            }

            return false;
        }

        static bool HasVisibleDamagedEnemyBase(WorldSnapshot snapshot, int localPlayerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != localPlayerId &&
                    actor.TypeId == "fabrication_hub" &&
                    !actor.IsDestroyed &&
                    actor.MaxHealth > 0 &&
                    actor.Health < actor.MaxHealth)
                    return true;
            }

            return false;
        }

        static bool HasEconomyProgress(WorldSnapshot snapshot)
        {
            if (snapshot.Economy == null)
                return false;

            for (var i = 0; i < snapshot.Economy.Harvesters.Count; i++)
            {
                var harvester = snapshot.Economy.Harvesters[i];
                if (harvester.CargoAmount > 0 || harvester.State == "Harvesting" || harvester.State == "Unloading")
                    return true;
            }

            for (var i = 0; i < snapshot.Economy.Refineries.Count; i++)
            {
                var refinery = snapshot.Economy.Refineries[i];
                if (refinery.IsUnloading || refinery.TotalResourcesReceived > 0)
                    return true;
            }

            return false;
        }

        static ScenarioObjectiveState ObjectiveState(WorldSnapshot snapshot, string objectiveId)
        {
            if (snapshot.Scenario == null)
                return ScenarioObjectiveState.Inactive;

            for (var i = 0; i < snapshot.Scenario.Objectives.Count; i++)
            {
                var objective = snapshot.Scenario.Objectives[i];
                if (objective.ObjectiveId == objectiveId)
                    return objective.State;
            }

            return ScenarioObjectiveState.Inactive;
        }

        string DetermineRecommendedType()
        {
            if (!hasPowerPlant)
                return "power_plant";
            if (!hasRefinery)
                return "refinery";
            if (!hasBarracks)
                return "barracks";
            if (!hasInfantry)
                return "rifle_infantry";
            if (!hasWarFactory)
                return "war_factory";
            if (!hasCombatVehicle)
                return "light_tank";
            if (!hasHarvester)
                return "harvester";
            return string.Empty;
        }

        string DeterminePrompt()
        {
            if (hasWon || enemyBaseDestroyed)
                return "Victory reached. Use the result screen to restart or return to menu.";
            if (hasLost)
                return "Your base was destroyed. Restart from the result screen.";
            if (driver.HasPlacementMode)
                return "Place " + driver.PendingPlacementTypeId + " on a clear highlighted footprint.";
            if (!hasSelectedFabricationHub)
                return "Select your Fabrication Hub, then use the right sidebar to build.";
            if (!hasPowerPlant)
                return "Build a Power Plant first so the base stays powered.";
            if (!hasRefinery)
                return "Build a Refinery near the green resource field.";
            if (!hasHarvesterCargoOrUnload)
                return "Send a harvester to resources and let it unload at the refinery.";
            if (!hasBarracks)
                return "Build Barracks to unlock infantry.";
            if (!hasInfantry)
                return "Train rifle infantry for early scouting and attacks.";
            if (!hasWarFactory)
                return "Build War Factory to unlock vehicles.";
            if (!hasCombatVehicle)
                return "Produce a light tank, then group it with infantry.";
            if (!hasDiscoveredEnemy)
                return "Scout east/northeast until the enemy base appears.";
            if (!hasDamagedEnemyBase)
                return "Attack the enemy Fabrication Hub.";
            return "Keep pressure on the enemy base until the victory screen appears.";
        }
    }
}
