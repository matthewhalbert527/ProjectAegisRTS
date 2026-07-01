using ProjectAegisRTS.Match;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Scenario
{
    public sealed class VerticalSliceProgressTracker : MonoBehaviour
    {
        public const int MissionStepCount = 15;

        public RtsSimulationDriver driver;

        public bool hasMissionStarted;
        public bool hasSelectedFabricationHub;
        public bool hasPowerPlant;
        public bool hasQueuedPowerPlant;
        public bool hasPlacedPowerPlant;
        public bool hasRefinery;
        public bool hasQueuedRefinery;
        public bool hasPlacedRefinery;
        public bool hasHarvester;
        public bool hasHarvesterCargoOrUnload;
        public bool hasBarracks;
        public bool hasQueuedBarracks;
        public bool hasPlacedBarracks;
        public bool hasInfantry;
        public bool hasTrainedInfantry;
        public bool hasWarFactory;
        public bool hasQueuedWarFactory;
        public bool hasPlacedWarFactory;
        public bool hasCombatVehicle;
        public bool hasProducedLightTank;
        public bool hasDiscoveredEnemy;
        public bool hasEngagedEnemy;
        public bool hasDamagedEnemyBase;
        public bool enemyBaseDestroyed;
        public bool hasWon;
        public bool hasLost;

        public string recommendedTypeId = "power_plant";
        public string currentChecklistPrompt = "Select your Fabrication Hub.";

        int initialPowerPlantCount;
        int initialRefineryCount;
        int initialBarracksCount;
        int initialInfantryCount;
        int initialWarFactoryCount;
        int initialLightTankCount;
        bool capturedInitialCounts;
        int lastSnapshotTick = -1;
        string lastScenarioId = string.Empty;

        public void Initialize(RtsSimulationDriver simulationDriver)
        {
            driver = simulationDriver;
            capturedInitialCounts = false;
            lastSnapshotTick = -1;
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
            var scenarioId = snapshot.Match == null ? string.Empty : snapshot.Match.ScenarioId;
            if (snapshot.Tick < lastSnapshotTick || scenarioId != lastScenarioId)
            {
                capturedInitialCounts = false;
                lastScenarioId = scenarioId;
            }

            if (!capturedInitialCounts)
                CaptureInitialCounts(snapshot, localPlayerId);

            lastSnapshotTick = snapshot.Tick;

            var powerPlantCount = CountOwnedActors(snapshot, localPlayerId, "power_plant");
            var refineryCount = CountOwnedActors(snapshot, localPlayerId, "refinery");
            var barracksCount = CountOwnedActors(snapshot, localPlayerId, "barracks");
            var infantryCount = CountOwnedActors(snapshot, localPlayerId, "rifle_infantry");
            var warFactoryCount = CountOwnedActors(snapshot, localPlayerId, "war_factory");
            var lightTankCount = CountOwnedActors(snapshot, localPlayerId, "light_tank");

            hasMissionStarted = snapshot.Match.Phase == MatchPhase.Running ||
                snapshot.Match.Phase == MatchPhase.Won ||
                snapshot.Match.Phase == MatchPhase.Lost;
            hasSelectedFabricationHub = IsSelectedOwnedType("fabrication_hub", localPlayerId);
            hasPowerPlant = powerPlantCount > 0;
            hasQueuedPowerPlant = HasProductionProgress(snapshot, localPlayerId, "power_plant");
            hasPlacedPowerPlant = powerPlantCount > initialPowerPlantCount;
            hasRefinery = refineryCount > 0;
            hasQueuedRefinery = HasProductionProgress(snapshot, localPlayerId, "refinery");
            hasPlacedRefinery = refineryCount > initialRefineryCount;
            hasHarvester = HasOwnedActor(snapshot, localPlayerId, "harvester");
            hasBarracks = barracksCount > 0;
            hasQueuedBarracks = HasProductionProgress(snapshot, localPlayerId, "barracks");
            hasPlacedBarracks = barracksCount > initialBarracksCount;
            hasInfantry = infantryCount > 0;
            hasTrainedInfantry = infantryCount > initialInfantryCount;
            hasWarFactory = warFactoryCount > 0;
            hasQueuedWarFactory = HasProductionProgress(snapshot, localPlayerId, "war_factory");
            hasPlacedWarFactory = warFactoryCount > initialWarFactoryCount;
            hasCombatVehicle = lightTankCount > 0;
            hasProducedLightTank = lightTankCount > initialLightTankCount;
            hasDiscoveredEnemy = HasVisibleEnemy(snapshot, localPlayerId);
            hasDamagedEnemyBase = HasVisibleDamagedEnemyBase(snapshot, localPlayerId) || ObjectiveState(snapshot, "destroy_enemy_base") == ScenarioObjectiveState.Completed;
            hasEngagedEnemy = HasPlayerCombatDamage(snapshot, localPlayerId) || hasDamagedEnemyBase;
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
                case 0: return hasMissionStarted;
                case 1: return hasSelectedFabricationHub;
                case 2: return hasQueuedPowerPlant || hasPlacedPowerPlant;
                case 3: return hasPlacedPowerPlant;
                case 4: return hasQueuedRefinery || hasPlacedRefinery;
                case 5: return hasPlacedRefinery;
                case 6: return hasHarvesterCargoOrUnload;
                case 7: return hasQueuedBarracks || hasPlacedBarracks;
                case 8: return hasTrainedInfantry;
                case 9: return hasQueuedWarFactory || hasPlacedWarFactory;
                case 10: return hasProducedLightTank;
                case 11: return hasDiscoveredEnemy;
                case 12: return hasEngagedEnemy;
                case 13: return enemyBaseDestroyed || hasWon;
                case 14: return hasWon;
                default: return false;
            }
        }

        public int GetCurrentStepIndex()
        {
            for (var i = 0; i < MissionStepCount; i++)
                if (!IsStepComplete(i))
                    return i;
            return MissionStepCount - 1;
        }

        public string GetStepLabel(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0: return "Welcome: camera overview.";
                case 1: return "Select your Fabrication Hub.";
                case 2: return "Build Power Plant.";
                case 3: return "Place Power Plant on the fine grid.";
                case 4: return "Build Refinery.";
                case 5: return "Place Refinery near resources.";
                case 6: return "Observe the harvester loop.";
                case 7: return "Build Barracks.";
                case 8: return "Train infantry.";
                case 9: return "Build War Factory.";
                case 10: return "Produce a light tank.";
                case 11: return "Scout toward enemy base.";
                case 12: return "Attack enemy forces or base.";
                case 13: return "Destroy the enemy base.";
                case 14: return "Victory screen.";
                default: return string.Empty;
            }
        }

        public string GetCurrentStepLabel()
        {
            return GetStepLabel(GetCurrentStepIndex());
        }

        public string GetNextStepLabel()
        {
            var current = GetCurrentStepIndex();
            for (var i = current + 1; i < MissionStepCount; i++)
                if (!IsStepComplete(i))
                    return GetStepLabel(i);
            return hasWon ? "Mission complete." : "Keep attacking until victory.";
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
                return "Enemy base scouted. Build combat units and attack.";
            return "Build economy, scout east, and destroy the enemy base.";
        }

        void ResetProgress()
        {
            hasMissionStarted = false;
            hasSelectedFabricationHub = false;
            hasPowerPlant = false;
            hasQueuedPowerPlant = false;
            hasPlacedPowerPlant = false;
            hasRefinery = false;
            hasQueuedRefinery = false;
            hasPlacedRefinery = false;
            hasHarvester = false;
            hasHarvesterCargoOrUnload = false;
            hasBarracks = false;
            hasQueuedBarracks = false;
            hasPlacedBarracks = false;
            hasInfantry = false;
            hasTrainedInfantry = false;
            hasWarFactory = false;
            hasQueuedWarFactory = false;
            hasPlacedWarFactory = false;
            hasCombatVehicle = false;
            hasProducedLightTank = false;
            hasDiscoveredEnemy = false;
            hasEngagedEnemy = false;
            hasDamagedEnemyBase = false;
            enemyBaseDestroyed = false;
            hasWon = false;
            hasLost = false;
            recommendedTypeId = "power_plant";
            currentChecklistPrompt = "Select your Fabrication Hub.";
        }

        void CaptureInitialCounts(WorldSnapshot snapshot, int localPlayerId)
        {
            initialPowerPlantCount = CountOwnedActors(snapshot, localPlayerId, "power_plant");
            initialRefineryCount = CountOwnedActors(snapshot, localPlayerId, "refinery");
            initialBarracksCount = CountOwnedActors(snapshot, localPlayerId, "barracks");
            initialInfantryCount = CountOwnedActors(snapshot, localPlayerId, "rifle_infantry");
            initialWarFactoryCount = CountOwnedActors(snapshot, localPlayerId, "war_factory");
            initialLightTankCount = CountOwnedActors(snapshot, localPlayerId, "light_tank");
            capturedInitialCounts = true;
            lastScenarioId = snapshot.Match == null ? string.Empty : snapshot.Match.ScenarioId;
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
            return CountOwnedActors(snapshot, ownerId, typeId) > 0;
        }

        static int CountOwnedActors(WorldSnapshot snapshot, int ownerId, string typeId)
        {
            var count = 0;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId == ownerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    count++;
            }

            return count;
        }

        static bool HasProductionProgress(WorldSnapshot snapshot, int ownerId, string typeId)
        {
            var player = FindPlayer(snapshot, ownerId);
            if (player == null)
                return false;

            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                if (item.TypeId == typeId && item.State != "Cancelled")
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

            for (var i = 0; i < snapshot.Economy.Events.Count; i++)
            {
                var eventSnapshot = snapshot.Economy.Events[i];
                if (eventSnapshot.EventType == "ResourceHarvested" || eventSnapshot.EventType == "HarvesterUnloaded")
                    return true;
            }

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

        static bool HasPlayerCombatDamage(WorldSnapshot snapshot, int localPlayerId)
        {
            if (snapshot.CombatEvents == null)
                return false;

            for (var i = 0; i < snapshot.CombatEvents.Count; i++)
            {
                var combatEvent = snapshot.CombatEvents[i];
                if (combatEvent.EventType != "DamageApplied" || combatEvent.Damage <= 0)
                    continue;

                var sourceOwner = ActorOwner(snapshot, combatEvent.SourceActorId);
                var targetOwner = ActorOwner(snapshot, combatEvent.TargetActorId);
                if (sourceOwner == localPlayerId && targetOwner != 0 && targetOwner != localPlayerId)
                    return true;
            }

            return false;
        }

        static int ActorOwner(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].ActorId == actorId)
                    return snapshot.Actors[i].OwnerId;
            return 0;
        }

        static PlayerSnapshot FindPlayer(WorldSnapshot snapshot, int ownerId)
        {
            for (var i = 0; i < snapshot.Players.Count; i++)
                if (snapshot.Players[i].PlayerId == ownerId)
                    return snapshot.Players[i];
            return null;
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
            if (!hasQueuedPowerPlant && !hasPlacedPowerPlant)
                return "power_plant";
            if (!hasPlacedPowerPlant)
                return "power_plant";
            if (!hasQueuedRefinery && !hasPlacedRefinery)
                return "refinery";
            if (!hasPlacedRefinery)
                return "refinery";
            if (!hasQueuedBarracks && !hasPlacedBarracks)
                return "barracks";
            if (!hasTrainedInfantry)
                return "rifle_infantry";
            if (!hasQueuedWarFactory && !hasPlacedWarFactory)
                return "war_factory";
            if (!hasProducedLightTank)
                return "light_tank";
            return string.Empty;
        }

        string DeterminePrompt()
        {
            if (hasWon || enemyBaseDestroyed)
                return "Victory reached. Use the result screen to restart or return to menu.";
            if (hasLost)
                return "Your base was destroyed. Restart from the result screen.";
            if (driver.HasPlacementMode)
                return "Buildings snap to the fine placement grid. Green footprint is valid; red footprint is blocked.";
            if (!hasMissionStarted)
                return "Start the vertical slice and review the board.";
            if (!hasSelectedFabricationHub)
                return "Select your Fabrication Hub, then use the right sidebar to build.";
            if (!hasQueuedPowerPlant && !hasPlacedPowerPlant)
                return "Build Power Plant first.";
            if (!hasPlacedPowerPlant)
                return "Power Plant ready. Place it on green fine-grid cells.";
            if (!hasQueuedRefinery && !hasPlacedRefinery)
                return "Power online. Next: build Refinery near the green resource field.";
            if (!hasPlacedRefinery)
                return "Place Refinery near resources so harvesters unload faster.";
            if (!hasHarvesterCargoOrUnload)
                return "Refinery placed. Let the harvester gather and unload resources.";
            if (!hasQueuedBarracks && !hasPlacedBarracks)
                return "Build Barracks to train infantry.";
            if (!hasTrainedInfantry)
                return "Train infantry, then scout east.";
            if (!hasQueuedWarFactory && !hasPlacedWarFactory)
                return "Build War Factory to unlock vehicles.";
            if (!hasProducedLightTank)
                return "Produce a light tank for the attack group.";
            if (!hasDiscoveredEnemy)
                return "Scout east/northeast until the enemy base appears.";
            if (!hasEngagedEnemy)
                return "Enemy base located. Build combat units and attack.";
            if (!hasDamagedEnemyBase)
                return "Attack the enemy Fabrication Hub.";
            return "Keep pressure on the enemy base until the victory screen appears.";
        }
    }
}
