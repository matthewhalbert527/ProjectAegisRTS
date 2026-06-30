using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.CoreBridge
{
    public sealed class RtsSimulationDriver : MonoBehaviour
    {
        [SerializeField] int playerId = 1;
        [SerializeField] bool useCombatDemoWorld;
        [SerializeField] bool useEconomyDemoWorld;
        [SerializeField] bool useFogRadarDemoWorld;
        [SerializeField] bool useAiSkirmishDemoWorld;
        [SerializeField] bool usePlayerPerspectiveSnapshot;

        readonly List<int> selectedActorIds = new List<int>();
        RtsWorld world;
        WorldSnapshot latestSnapshot;
        Int2 hoveredCell;
        bool hasHoveredCell;
        string pendingPlacementTypeId = string.Empty;
        bool forceLowPower;
        float tickAccumulator;

        public int TicksPerSecond { get; private set; }
        public bool IsPaused { get; private set; }
        public WorldSnapshot LatestSnapshot { get { return latestSnapshot; } }
        public RtsRules Rules { get { return world == null ? null : world.Rules; } }
        public IReadOnlyList<int> SelectedActorIds { get { return selectedActorIds; } }
        public bool HasHoveredCell { get { return hasHoveredCell; } }
        public Int2 HoveredCell { get { return hoveredCell; } }
        public bool HasPlacementMode { get { return !string.IsNullOrEmpty(pendingPlacementTypeId); } }
        public string PendingPlacementTypeId { get { return pendingPlacementTypeId; } }
        public int PlayerId { get { return playerId; } }
        public bool UseCombatDemoWorld { get { return useCombatDemoWorld; } set { useCombatDemoWorld = value; } }
        public bool UseEconomyDemoWorld { get { return useEconomyDemoWorld; } set { useEconomyDemoWorld = value; } }
        public bool UseFogRadarDemoWorld { get { return useFogRadarDemoWorld; } set { useFogRadarDemoWorld = value; } }
        public bool UseAiSkirmishDemoWorld { get { return useAiSkirmishDemoWorld; } set { useAiSkirmishDemoWorld = value; } }
        public bool UsePlayerPerspectiveSnapshot { get { return usePlayerPerspectiveSnapshot; } set { usePlayerPerspectiveSnapshot = value; } }

        public string CommandMode
        {
            get
            {
                if (HasPlacementMode)
                    return "Placement: " + pendingPlacementTypeId;

                return IsPaused ? "Paused" : "Command";
            }
        }

        public void Initialize(int ticksPerSecond, bool startPaused)
        {
            TicksPerSecond = Math.Max(1, ticksPerSecond);
            IsPaused = startPaused;
            ResetDemoWorld();
        }

        public void ManualUpdate(float deltaTime)
        {
            if (world == null || IsPaused)
                return;

            tickAccumulator += Mathf.Max(0f, deltaTime);
            var tickLength = 1f / Math.Max(1, TicksPerSecond);
            var ticksThisFrame = 0;
            while (tickAccumulator >= tickLength && ticksThisFrame < 8)
            {
                world.Tick();
                tickAccumulator -= tickLength;
                ticksThisFrame++;
            }

            if (ticksThisFrame > 0)
                RefreshSnapshot();
        }

        public RtsCommandResult TogglePause()
        {
            IsPaused = !IsPaused;
            return RtsCommandResult.Ok(IsPaused ? "Simulation paused." : "Simulation running.");
        }

        public RtsCommandResult StepOneTick()
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            world.Tick();
            RefreshSnapshot();
            return RtsCommandResult.Ok("Advanced one deterministic tick.");
        }

        public RtsCommandResult ResetDemoWorld()
        {
            if (useAiSkirmishDemoWorld)
                world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            else if (useFogRadarDemoWorld)
                world = DemoWorldFactory.CreateFogRadarDemoWorld();
            else if (useEconomyDemoWorld)
                world = DemoWorldFactory.CreateEconomyDemoWorld();
            else
                world = useCombatDemoWorld ? DemoWorldFactory.CreateCombatDemoWorld() : DemoWorldFactory.CreateMvpWorld();
            selectedActorIds.Clear();
            pendingPlacementTypeId = string.Empty;
            forceLowPower = false;
            tickAccumulator = 0f;
            RefreshSnapshot();
            return RtsCommandResult.Ok(useAiSkirmishDemoWorld ? "AI skirmish demo world reset." : (useFogRadarDemoWorld ? "Fog/radar demo world reset." : (useEconomyDemoWorld ? "Economy demo world reset." : (useCombatDemoWorld ? "Combat demo world reset." : "Demo world reset."))));
        }

        public RtsCommandResult TryCreateCombatDemoWorld()
        {
            useCombatDemoWorld = true;
            useEconomyDemoWorld = false;
            useFogRadarDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateEconomyDemoWorld()
        {
            useEconomyDemoWorld = true;
            useCombatDemoWorld = false;
            useFogRadarDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateFogRadarDemoWorld()
        {
            useFogRadarDemoWorld = true;
            useEconomyDemoWorld = false;
            useCombatDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            usePlayerPerspectiveSnapshot = true;
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateAiSkirmishDemoWorld()
        {
            useAiSkirmishDemoWorld = true;
            useFogRadarDemoWorld = false;
            useEconomyDemoWorld = false;
            useCombatDemoWorld = false;
            usePlayerPerspectiveSnapshot = false;
            return ResetDemoWorld();
        }

        public void SetHoveredCell(Int2 cell)
        {
            hoveredCell = cell;
            hasHoveredCell = true;
        }

        public void ClearHoveredCell()
        {
            hasHoveredCell = false;
        }

        public RtsCommandResult TrySelectActorAtCell(Int2 cell)
        {
            if (latestSnapshot == null)
                return RtsCommandResult.Fail("SnapshotMissing", "No world snapshot is available.");

            int actorId;
            if (!TryFindActorAtCell(cell, out actorId))
            {
                selectedActorIds.Clear();
                return RtsCommandResult.Ok("Selection cleared.");
            }

            selectedActorIds.Clear();
            selectedActorIds.Add(actorId);
            return RtsCommandAdapter.SelectActors(world, playerId, selectedActorIds);
        }

        public RtsCommandResult ClearSelection()
        {
            selectedActorIds.Clear();
            return RtsCommandResult.Ok("Selection cleared.");
        }

        public RtsCommandResult SetSelectedActorIds(IReadOnlyList<int> actorIds)
        {
            selectedActorIds.Clear();
            if (actorIds != null)
            {
                for (var i = 0; i < actorIds.Count; i++)
                {
                    ActorSnapshot actor;
                    if (TryGetActorSnapshot(actorIds[i], out actor) && actor.OwnerId == playerId && !selectedActorIds.Contains(actor.ActorId))
                        selectedActorIds.Add(actor.ActorId);
                }
            }

            if (world == null)
                return RtsCommandResult.Ok("Selected actors: " + SelectedActorIdsText() + ".");

            return RtsCommandAdapter.SelectActors(world, playerId, selectedActorIds);
        }

        public RtsCommandResult AddOrRemoveSelectedActor(int actorId)
        {
            ActorSnapshot actor;
            if (!TryGetActorSnapshot(actorId, out actor) || actor.OwnerId != playerId)
                return RtsCommandResult.Fail("ActorUnavailable", "Selectable actor is no longer available.");

            if (selectedActorIds.Contains(actorId))
                selectedActorIds.Remove(actorId);
            else
                selectedActorIds.Add(actorId);

            if (world == null)
                return RtsCommandResult.Ok("Selected actors: " + SelectedActorIdsText() + ".");

            return RtsCommandAdapter.SelectActors(world, playerId, selectedActorIds);
        }

        public RtsCommandResult TryIssueMoveSelectedToCell(Int2 cell)
        {
            if (selectedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoSelection", "Select a mobile unit before issuing a move command.");

            var mobileActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (definition is UnitDefinition)
                    mobileActorIds.Add(actor.ActorId);
            }

            if (mobileActorIds.Count == 0)
                return RtsCommandResult.Fail("NoMobileSelection", "The current selection has no mobile units.");

            var result = RtsCommandAdapter.IssueMoveOrder(world, playerId, mobileActorIds, cell);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryIssueAttackSelectedToActor(int targetActorId)
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
            if (selectedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoSelection", "Select an armed actor before issuing an attack command.");

            ActorSnapshot target;
            if (!TryGetActorSnapshot(targetActorId, out target))
                return RtsCommandResult.Fail("TargetMissing", "No target actor exists for the attack command.");
            if (target.OwnerId == playerId)
                return RtsCommandResult.Fail("TargetFriendly", "Select an enemy actor as the attack target.");

            var armedActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;
                if (definition.Weapon != null)
                    armedActorIds.Add(actor.ActorId);
            }

            if (armedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoArmedSelection", "The current selection has no armed actors.");

            var result = RtsCommandAdapter.IssueAttackOrder(world, playerId, armedActorIds, targetActorId);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryIssueAttackSelectedAtCell(Int2 cell)
        {
            int targetActorId;
            if (!TryFindTargetActorAtCell(cell, out targetActorId))
                return RtsCommandResult.Fail("NoAttackTarget", "No enemy actor is present at " + cell + ".");

            return TryIssueAttackSelectedToActor(targetActorId);
        }

        public RtsCommandResult TryIssueForceAttackSelectedAtCell(Int2 cell)
        {
            if (selectedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoSelection", "Select an actor before issuing force-attack.");

            var result = RtsCommandAdapter.IssueForceAttackCell(world, playerId, selectedActorIds, cell);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryIssueHarvestSelectedAtCell(Int2 cell)
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
            if (selectedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoSelection", "Select a harvester before issuing a harvest command.");

            var harvesterActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                if (TryGetActorSnapshot(selectedActorIds[i], out actor) && actor.OwnerId == playerId && actor.TypeId == "harvester" && !actor.IsDestroyed)
                    harvesterActorIds.Add(actor.ActorId);
            }

            if (harvesterActorIds.Count == 0)
                return RtsCommandResult.Fail("NoHarvesterSelection", "The current selection has no harvesters.");

            var result = RtsCommandAdapter.IssueHarvestOrder(world, playerId, harvesterActorIds, cell);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryReturnSelectedHarvesters()
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
            if (selectedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoSelection", "Select a harvester before returning to refinery.");

            var harvesterActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                if (TryGetActorSnapshot(selectedActorIds[i], out actor) && actor.OwnerId == playerId && actor.TypeId == "harvester" && !actor.IsDestroyed)
                    harvesterActorIds.Add(actor.ActorId);
            }

            if (harvesterActorIds.Count == 0)
                return RtsCommandResult.Fail("NoHarvesterSelection", "The current selection has no harvesters.");

            var result = RtsCommandAdapter.ReturnToRefinery(world, playerId, harvesterActorIds);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryStopSelectedCombat()
        {
            return TryStopSelected();
        }

        public RtsCommandResult TryQueueProduction(string typeId)
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            ActorDefinition targetDefinition;
            if (!Rules.TryGetDefinition(typeId, out targetDefinition))
                return RtsCommandResult.Fail("UnknownActorType", "Unknown production type: " + typeId);

            if (targetDefinition is BuildingDefinition && HasCompletedPendingPlacement(typeId))
            {
                pendingPlacementTypeId = typeId;
                return RtsCommandResult.Ok("Placement mode entered for " + typeId + ".");
            }

            var factoryTypeId = targetDefinition.Production.FactoryTypeId;
            if (string.IsNullOrEmpty(factoryTypeId))
                return RtsCommandResult.Fail("NoFactory", typeId + " is not buildable in the Stage 0 rules.");

            int producerActorId;
            if (!TryFindOwnedActorOfType(factoryTypeId, out producerActorId))
                return RtsCommandResult.Fail("ProducerMissing", "Build " + factoryTypeId + " before producing " + typeId + ".");

            var result = RtsCommandAdapter.BeginProduction(world, playerId, producerActorId, typeId);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryEnterPlacementModeForFirstPending()
        {
            var player = GetLocalPlayerSnapshot();
            if (player == null)
                return RtsCommandResult.Fail("PlayerMissing", "No local player snapshot is available.");

            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                ActorDefinition definition;
                if (item.State == "CompletedPendingPlacement" &&
                    Rules.TryGetDefinition(item.TypeId, out definition) &&
                    definition is BuildingDefinition)
                {
                    pendingPlacementTypeId = item.TypeId;
                    return RtsCommandResult.Ok("Placement mode entered for " + item.TypeId + ".");
                }
            }

            return RtsCommandResult.Fail("NoPendingPlacement", "No completed building is waiting for placement.");
        }

        public RtsCommandResult TryPlacePendingBuildingAtCell(Int2 cell)
        {
            if (!HasPlacementMode)
                return RtsCommandResult.Fail("PlacementInactive", "No pending building placement is active.");

            var typeId = pendingPlacementTypeId;
            var result = RtsCommandAdapter.PlaceBuilding(world, playerId, typeId, cell);
            if (result.Success)
                pendingPlacementTypeId = string.Empty;

            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryCancelPlacement()
        {
            if (!HasPlacementMode)
                return RtsCommandResult.Ok("No placement mode was active.");

            pendingPlacementTypeId = string.Empty;
            return RtsCommandResult.Ok("Placement mode cancelled.");
        }

        public bool IsPlacementValidAtCell(Int2 cell, out PlacementPreviewSnapshot preview)
        {
            preview = null;
            if (!HasPlacementMode || world == null)
                return false;

            preview = world.PreviewPlacement(playerId, pendingPlacementTypeId, cell);
            return preview != null && preview.CanPlace;
        }

        public RtsCommandResult TryCancelProduction(int queueItemId)
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var result = RtsCommandAdapter.CancelProduction(world, playerId, queueItemId);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryStopSelected()
        {
            if (selectedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoSelection", "Select actors before issuing Stop.");

            var result = RtsCommandAdapter.StopActors(world, playerId, selectedActorIds);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryTogglePowerSelected()
        {
            if (selectedActorIds.Count != 1)
                return RtsCommandResult.Fail("SelectionRequiresOne", "Select one building before toggling power.");

            var result = RtsCommandAdapter.TogglePower(world, playerId, selectedActorIds[0]);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryForceLowPowerOrCreateLowPowerDemoCondition()
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            forceLowPower = !forceLowPower;
            world.ForcePlayerPowerState(playerId, forceLowPower ? PlayerPowerState.LowPower : (PlayerPowerState?)null);
            RefreshSnapshot();
            return RtsCommandResult.Ok(forceLowPower ? "Forced low-power demo state." : "Returned power state to simulation rules.");
        }

        public bool TryGetPlacementPreview(out PlacementPreviewSnapshot preview)
        {
            preview = null;
            if (!HasPlacementMode || !hasHoveredCell || world == null)
                return false;

            preview = world.PreviewPlacement(playerId, pendingPlacementTypeId, hoveredCell);
            return true;
        }

        public bool IsActorSelected(int actorId)
        {
            return selectedActorIds.Contains(actorId);
        }

        public PlayerSnapshot GetLocalPlayerSnapshot()
        {
            if (latestSnapshot == null)
                return null;

            for (var i = 0; i < latestSnapshot.Players.Count; i++)
                if (latestSnapshot.Players[i].PlayerId == playerId)
                    return latestSnapshot.Players[i];

            return null;
        }

        public string SelectedActorIdsText()
        {
            return selectedActorIds.Count == 0 ? "none" : string.Join(", ", selectedActorIds);
        }

        public string HoveredCellText()
        {
            return hasHoveredCell ? hoveredCell.ToString() : "none";
        }

        public bool TryGetDefinition(string typeId, out ActorDefinition definition)
        {
            definition = null;
            return Rules != null && Rules.TryGetDefinition(typeId, out definition);
        }

        public bool TryGetActorSnapshot(int actorId, out ActorSnapshot snapshot)
        {
            snapshot = null;
            if (latestSnapshot == null)
                return false;

            for (var i = 0; i < latestSnapshot.Actors.Count; i++)
            {
                if (latestSnapshot.Actors[i].ActorId == actorId)
                {
                    snapshot = latestSnapshot.Actors[i];
                    return true;
                }
            }

            return false;
        }

        public bool HasOwnedActorOfType(string typeId)
        {
            int actorId;
            return TryFindOwnedActorOfType(typeId, out actorId);
        }

        void RefreshSnapshot()
        {
            latestSnapshot = world == null ? null : (usePlayerPerspectiveSnapshot ? world.CreateSnapshot(playerId) : world.CreateSnapshot());
        }

        bool TryFindActorAtCell(Int2 cell, out int actorId)
        {
            actorId = 0;
            if (latestSnapshot == null)
                return false;

            for (var i = latestSnapshot.Actors.Count - 1; i >= 0; i--)
            {
                var actor = latestSnapshot.Actors[i];
                ActorDefinition definition;
                if (actor.OwnerId != playerId || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (ActorCoversCell(actor, definition, cell))
                {
                    actorId = actor.ActorId;
                    return true;
                }
            }

            return false;
        }

        bool TryFindTargetActorAtCell(Int2 cell, out int actorId)
        {
            actorId = 0;
            if (latestSnapshot == null)
                return false;

            for (var i = latestSnapshot.Actors.Count - 1; i >= 0; i--)
            {
                var actor = latestSnapshot.Actors[i];
                ActorDefinition definition;
                if (actor.OwnerId == playerId || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (ActorCoversCell(actor, definition, cell))
                {
                    actorId = actor.ActorId;
                    return true;
                }
            }

            return false;
        }

        bool ActorCoversCell(ActorSnapshot actor, ActorDefinition definition, Int2 cell)
        {
            var building = definition as BuildingDefinition;
            if (building == null)
                return actor.CellPosition.Equals(cell);

            return cell.X >= actor.CellPosition.X &&
                   cell.Y >= actor.CellPosition.Y &&
                   cell.X < actor.CellPosition.X + building.FootprintCells.X &&
                   cell.Y < actor.CellPosition.Y + building.FootprintCells.Y;
        }

        bool TryFindOwnedActorOfType(string typeId, out int actorId)
        {
            actorId = 0;
            if (latestSnapshot == null)
                return false;

            for (var i = 0; i < latestSnapshot.Actors.Count; i++)
            {
                var actor = latestSnapshot.Actors[i];
                if (actor.OwnerId == playerId && actor.TypeId == typeId)
                {
                    actorId = actor.ActorId;
                    return true;
                }
            }

            return false;
        }

        bool HasCompletedPendingPlacement(string typeId)
        {
            var player = GetLocalPlayerSnapshot();
            if (player == null)
                return false;

            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                if (item.TypeId == typeId && item.State == "CompletedPendingPlacement")
                    return true;
            }

            return false;
        }
    }
}
