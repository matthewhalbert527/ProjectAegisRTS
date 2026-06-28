using System;
using System.Collections.Generic;
using System.Text;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Pathfinding;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Production;
using ProjectAegisRTS.Snapshots;

namespace ProjectAegisRTS.Simulation
{
    public sealed class RtsWorld
    {
        readonly Dictionary<int, PlayerState> players;
        readonly Dictionary<int, ActorState> actors;
        readonly GridPathfinder pathfinder;
        int nextActorId;
        int nextQueueItemId;

        public RtsRules Rules { get; private set; }
        public GridMap Map { get; private set; }
        public int TickNumber { get; private set; }

        public RtsWorld(RtsRules rules, GridMap map)
        {
            Rules = rules;
            Map = map;
            players = new Dictionary<int, PlayerState>();
            actors = new Dictionary<int, ActorState>();
            pathfinder = new GridPathfinder();
            nextActorId = 1;
            nextQueueItemId = 1;
        }

        public IReadOnlyDictionary<int, PlayerState> Players
        {
            get { return players; }
        }

        public IReadOnlyDictionary<int, ActorState> Actors
        {
            get { return actors; }
        }

        public PlayerState AddPlayer(int playerId, string name, int credits)
        {
            var player = new PlayerState(playerId, name, credits);
            players.Add(playerId, player);
            return player;
        }

        public ActorState CreateActor(string typeId, int ownerPlayerId, Int2 cell)
        {
            var definition = Rules.GetDefinition(typeId);
            var actor = new ActorState(new ActorId(nextActorId++), ownerPlayerId, typeId, cell, definition.MaxHealth);
            actor.AnimationStateId = definition.Animation.IdleStateId;
            if (definition is UnitDefinition)
                actor.VisualMotionProfileId = ((UnitDefinition)definition).Movement.VisualMotionProfileId;
            else
                actor.VisualMotionProfileId = "building_static";

            actors.Add(actor.Id.Value, actor);

            var building = definition as BuildingDefinition;
            if (building != null)
                Map.OccupyBuilding(cell, building.FootprintCells, actor.Id);

            UpdatePowerAndActorFlags();
            return actor;
        }

        public bool TryGetActor(ActorId actorId, out ActorState actor)
        {
            return actors.TryGetValue(actorId.Value, out actor);
        }

        public ActorState FirstActorOfType(string typeId, int ownerPlayerId)
        {
            foreach (var actor in SortedActors())
                if (actor.TypeId == typeId && actor.OwnerPlayerId == ownerPlayerId)
                    return actor;

            return null;
        }

        public void ForcePlayerPowerState(int playerId, PlayerPowerState? state)
        {
            players[playerId].ForcedPowerState = state;
            UpdatePowerAndActorFlags();
        }

        public CommandResult IssueCommand(ISimCommand command)
        {
            if (!players.ContainsKey(command.PlayerId))
                return CommandResult.Fail("UnknownPlayer", "The command references a player that does not exist.");

            if (command is SelectActorsCommand)
                return CommandResult.Ok("Selection is client-local in Stage 0.");
            if (command is IssueMoveOrderCommand)
                return IssueMoveOrder((IssueMoveOrderCommand)command);
            if (command is IssueAttackOrderCommand)
                return IssueAttackOrder((IssueAttackOrderCommand)command);
            if (command is BeginProductionCommand)
                return BeginProduction((BeginProductionCommand)command);
            if (command is CancelProductionCommand)
                return CancelProduction((CancelProductionCommand)command);
            if (command is PlaceBuildingCommand)
                return PlaceBuilding((PlaceBuildingCommand)command);
            if (command is SetRallyPointCommand)
                return SetRallyPoint((SetRallyPointCommand)command);
            if (command is StopCommand)
                return Stop((StopCommand)command);
            if (command is PowerToggleCommand)
                return PowerToggle((PowerToggleCommand)command);

            return CommandResult.Fail("UnknownCommand", "The command type is not supported by the simulation.");
        }

        public void Tick()
        {
            TickNumber++;
            UpdatePowerAndActorFlags();
            TickProduction();
            TickMovement();
            UpdatePowerAndActorFlags();
        }

        public PlacementPreviewSnapshot PreviewPlacement(int playerId, string typeId, Int2 topLeftCell)
        {
            var footprint = GetFootprintCells(typeId, topLeftCell);
            var result = ValidatePlacement(playerId, typeId, topLeftCell, false);
            return new PlacementPreviewSnapshot(typeId, topLeftCell, result.Success, result.ErrorCode, footprint);
        }

        public WorldSnapshot CreateSnapshot()
        {
            UpdatePowerAndActorFlags();

            var playerSnapshots = new List<PlayerSnapshot>();
            foreach (var player in SortedPlayers())
            {
                var production = new List<ProductionSnapshot>();
                foreach (var item in player.ProductionQueue)
                    production.Add(new ProductionSnapshot(item.QueueItemId, item.ProducerActorId.Value, item.TypeId, item.ProgressTicks, item.BuildTimeTicks, item.State.ToString()));

                playerSnapshots.Add(new PlayerSnapshot(
                    player.PlayerId,
                    player.Name,
                    player.Credits,
                    new PowerSnapshot(player.PowerGenerated, player.PowerConsumed, player.PowerState),
                    production));
            }

            var actorSnapshots = new List<ActorSnapshot>();
            foreach (var actor in SortedActors())
            {
                var definition = Rules.GetDefinition(actor.TypeId);
                var unit = definition as UnitDefinition;
                var turnRate = unit == null ? 0 : unit.Movement.TurnRateDegreesPerTick;

                actorSnapshots.Add(new ActorSnapshot(
                    actor.Id.Value,
                    actor.TypeId,
                    actor.OwnerPlayerId,
                    actor.CellPosition,
                    actor.WorldPositionFixed,
                    actor.FacingDegrees,
                    actor.Health,
                    false,
                    actor.IsPowered,
                    actor.IsLowPower,
                    actor.LightsActive,
                    actor.MachineryActive,
                    actor.IsProducing,
                    actor.ProductionProgress,
                    actor.AnimationStateId,
                    actor.VisualMotionProfileId,
                    actor.DesiredSpeed,
                    actor.NormalizedSpeed,
                    turnRate,
                    actor.MovementPhase));
            }

            return new WorldSnapshot(TickNumber, playerSnapshots, actorSnapshots);
        }

        public string GetDeterminismSummary()
        {
            UpdatePowerAndActorFlags();
            var sb = new StringBuilder();
            sb.Append("tick=").Append(TickNumber).AppendLine();

            foreach (var player in SortedPlayers())
            {
                sb.Append("player ")
                    .Append(player.PlayerId)
                    .Append(" credits=").Append(player.Credits)
                    .Append(" power=").Append(player.PowerGenerated).Append('/').Append(player.PowerConsumed)
                    .Append(" state=").Append(player.PowerState)
                    .AppendLine();

                foreach (var item in player.ProductionQueue)
                {
                    sb.Append("queue ")
                        .Append(item.QueueItemId).Append(' ')
                        .Append(item.TypeId).Append(' ')
                        .Append(item.ProgressTicks).Append('/')
                        .Append(item.BuildTimeTicks).Append(' ')
                        .Append(item.State)
                        .AppendLine();
                }
            }

            foreach (var actor in SortedActors())
            {
                sb.Append("actor ")
                    .Append(actor.Id.Value).Append(' ')
                    .Append(actor.TypeId).Append(' ')
                    .Append(actor.OwnerPlayerId).Append(' ')
                    .Append(actor.CellPosition).Append(' ')
                    .Append(actor.WorldPositionFixed).Append(' ')
                    .Append(actor.Health).Append(' ')
                    .Append(actor.FacingDegrees).Append(' ')
                    .Append(actor.CurrentOrder).Append(' ')
                    .Append(actor.IsPowered).Append(' ')
                    .Append(actor.IsLowPower)
                    .AppendLine();
            }

            return sb.ToString();
        }

        CommandResult BeginProduction(BeginProductionCommand command)
        {
            ActorState producer;
            if (!TryGetOwnedActor(command.PlayerId, command.ProducerActorId, out producer))
                return CommandResult.Fail("InvalidProducer", "The producer actor does not exist or is not owned by the command player.");

            var producerDefinition = Rules.GetDefinition(producer.TypeId) as BuildingDefinition;
            if (producerDefinition == null)
                return CommandResult.Fail("InvalidProducer", "Only buildings can produce actors in Stage 0.");

            ActorDefinition targetDefinition;
            if (!Rules.TryGetDefinition(command.TypeId, out targetDefinition))
                return CommandResult.Fail("UnknownActorType", "The requested production item is not registered in the rules.");

            if (!Contains(producerDefinition.ProducesTypeIds, command.TypeId))
                return CommandResult.Fail("ProducerCannotBuildType", "The selected producer cannot build the requested actor type.");

            if (targetDefinition.Production.Kind == ProductionKind.None)
                return CommandResult.Fail("NotBuildable", "The requested actor type is not buildable.");

            var player = players[command.PlayerId];
            if (player.Credits < targetDefinition.Production.Cost)
                return CommandResult.Fail("InsufficientCredits", "The player does not have enough credits for this production item.");

            player.Credits -= targetDefinition.Production.Cost;
            var item = new ProductionQueueItem(
                nextQueueItemId++,
                command.PlayerId,
                command.ProducerActorId,
                command.TypeId,
                targetDefinition.Production.Cost,
                targetDefinition.Production.BuildTimeTicks,
                targetDefinition.Kind == ActorKind.Building,
                targetDefinition.Production.ExemptFromLowPowerPause);

            player.MutableProductionQueue.Add(item);
            UpdatePowerAndActorFlags();
            return CommandResult.Ok("Production started.");
        }

        CommandResult CancelProduction(CancelProductionCommand command)
        {
            var player = players[command.PlayerId];
            for (var i = 0; i < player.MutableProductionQueue.Count; i++)
            {
                var item = player.MutableProductionQueue[i];
                if (item.QueueItemId == command.QueueItemId)
                {
                    player.Credits += item.TotalCost / 2;
                    player.MutableProductionQueue.RemoveAt(i);
                    UpdatePowerAndActorFlags();
                    return CommandResult.Ok("Production cancelled with a partial Stage 0 refund.");
                }
            }

            return CommandResult.Fail("QueueItemNotFound", "The production queue item was not found.");
        }

        CommandResult PlaceBuilding(PlaceBuildingCommand command)
        {
            var validation = ValidatePlacement(command.PlayerId, command.TypeId, command.TopLeftCell, true);
            if (!validation.Success)
                return validation;

            var actor = CreateActor(command.TypeId, command.PlayerId, command.TopLeftCell);
            RemoveFirstPendingPlacement(command.PlayerId, command.TypeId);
            UpdatePowerAndActorFlags();
            return CommandResult.Ok("Building placed as actor " + actor.Id.Value + ".");
        }

        CommandResult IssueMoveOrder(IssueMoveOrderCommand command)
        {
            if (!Map.Contains(command.DestinationCell))
                return CommandResult.Fail("DestinationOutsideMap", "The move destination is outside the map.");

            var planned = new Dictionary<int, List<Int2>>();
            var details = new List<string>();
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                {
                    details.Add("actor " + actorId.Value + ": not owned or missing");
                    continue;
                }

                if (!(Rules.GetDefinition(actor.TypeId) is UnitDefinition))
                {
                    details.Add("actor " + actorId.Value + ": not a unit");
                    continue;
                }

                if (actor.CellPosition.Equals(command.DestinationCell))
                {
                    planned[actorId.Value] = new List<Int2>();
                    continue;
                }

                var path = pathfinder.FindPath(Map, actor.CellPosition, command.DestinationCell);
                if (path.Count == 0)
                    details.Add("actor " + actorId.Value + ": no path");
                else
                    planned[actorId.Value] = path;
            }

            if (details.Count > 0)
                return CommandResult.Fail("MoveOrderRejected", "One or more actors could not accept the move order.", details);

            foreach (var pair in planned)
            {
                var actor = actors[pair.Key];
                actor.Path.Clear();
                actor.Path.AddRange(pair.Value);
                actor.CurrentOrder = actor.Path.Count == 0 ? ActorOrderKind.Idle : ActorOrderKind.Move;
                actor.OrderTargetCell = command.DestinationCell;
                actor.MovementPhase = actor.Path.Count == 0 ? "idle" : "moving";
            }

            return CommandResult.Ok("Move order accepted.");
        }

        CommandResult IssueAttackOrder(IssueAttackOrderCommand command)
        {
            if (!actors.ContainsKey(command.TargetActorId.Value))
                return CommandResult.Fail("AttackTargetMissing", "The attack target actor does not exist.");

            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                    return CommandResult.Fail("AttackActorInvalid", "An attack subject actor does not exist or is not owned by the command player.");

                actor.CurrentOrder = ActorOrderKind.Attack;
                actor.OrderTargetCell = actors[command.TargetActorId.Value].CellPosition;
            }

            return CommandResult.Ok("Attack order placeholder accepted.");
        }

        CommandResult SetRallyPoint(SetRallyPointCommand command)
        {
            ActorState producer;
            if (!TryGetOwnedActor(command.PlayerId, command.ProducerActorId, out producer))
                return CommandResult.Fail("InvalidProducer", "The producer actor does not exist or is not owned by the command player.");

            if (!Map.Contains(command.RallyCell))
                return CommandResult.Fail("RallyOutsideMap", "The rally point is outside the map.");

            producer.RallyPoint = command.RallyCell;
            producer.CurrentOrder = ActorOrderKind.RallyPoint;
            return CommandResult.Ok("Rally point recorded.");
        }

        CommandResult Stop(StopCommand command)
        {
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                    return CommandResult.Fail("StopActorInvalid", "A stop subject actor does not exist or is not owned by the command player.");

                actor.Path.Clear();
                actor.CurrentOrder = ActorOrderKind.Stop;
                actor.DesiredSpeed = 0;
                actor.NormalizedSpeed = 0;
                actor.MovementPhase = "idle";
            }

            return CommandResult.Ok("Stop order accepted.");
        }

        CommandResult PowerToggle(PowerToggleCommand command)
        {
            ActorState actor;
            if (!TryGetOwnedActor(command.PlayerId, command.ActorId, out actor))
                return CommandResult.Fail("PowerToggleActorInvalid", "The target actor does not exist or is not owned by the command player.");

            if (!(Rules.GetDefinition(actor.TypeId) is BuildingDefinition))
                return CommandResult.Fail("PowerToggleRequiresBuilding", "Only buildings can be power-toggled in Stage 0.");

            actor.ManuallyPoweredOff = !actor.ManuallyPoweredOff;
            actor.CurrentOrder = ActorOrderKind.PowerToggle;
            UpdatePowerAndActorFlags();
            return CommandResult.Ok("Power toggle applied.");
        }

        void TickProduction()
        {
            var completedUnits = new List<ProductionQueueItem>();
            foreach (var player in SortedPlayers())
            {
                foreach (var item in player.MutableProductionQueue)
                {
                    if (item.State == ProductionItemState.CompletedPendingPlacement)
                        continue;

                    if (player.PowerState != PlayerPowerState.Normal && !item.ExemptFromLowPowerPause)
                    {
                        item.State = ProductionItemState.Paused;
                        continue;
                    }

                    item.State = ProductionItemState.Active;
                    item.ProgressTicks++;
                    if (item.ProgressTicks < item.BuildTimeTicks)
                        continue;

                    if (item.IsBuilding)
                        item.State = ProductionItemState.CompletedPendingPlacement;
                    else
                        completedUnits.Add(item);
                }
            }

            foreach (var item in completedUnits)
            {
                SpawnProducedUnit(item);
                players[item.PlayerId].MutableProductionQueue.Remove(item);
            }
        }

        void SpawnProducedUnit(ProductionQueueItem item)
        {
            ActorState producer;
            if (!actors.TryGetValue(item.ProducerActorId.Value, out producer))
                return;

            var producerDefinition = Rules.GetDefinition(producer.TypeId) as BuildingDefinition;
            var exitOffset = producerDefinition == null ? new Int2(0, 1) : producerDefinition.UnitExitOffset;
            var spawnCell = FindNearestSpawnCell(producer.CellPosition + exitOffset);
            var unit = CreateActor(item.TypeId, item.PlayerId, spawnCell);

            if (!producer.RallyPoint.Equals(producer.CellPosition) && !producer.RallyPoint.Equals(spawnCell))
                IssueCommand(new IssueMoveOrderCommand(item.PlayerId, new[] { unit.Id }, producer.RallyPoint));
        }

        Int2 FindNearestSpawnCell(Int2 preferred)
        {
            if (Map.IsPassableForUnit(preferred))
                return preferred;

            for (var radius = 1; radius <= 8; radius++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var x = -radius; x <= radius; x++)
                    {
                        if (Math.Abs(x) != radius && Math.Abs(y) != radius)
                            continue;

                        var candidate = new Int2(preferred.X + x, preferred.Y + y);
                        if (Map.IsPassableForUnit(candidate))
                            return candidate;
                    }
                }
            }

            return preferred;
        }

        void TickMovement()
        {
            foreach (var actor in SortedActors())
            {
                var definition = Rules.GetDefinition(actor.TypeId) as UnitDefinition;
                if (definition == null || actor.Path.Count == 0)
                {
                    actor.DesiredSpeed = 0;
                    actor.NormalizedSpeed = 0;
                    if (actor.CurrentOrder != ActorOrderKind.Move)
                        actor.MovementPhase = "idle";
                    continue;
                }

                var targetCell = actor.Path[0];
                var targetWorld = FixedMath.CellCenter(targetCell);
                var dx = targetWorld.X - actor.WorldPositionFixed.X;
                var dy = targetWorld.Y - actor.WorldPositionFixed.Y;
                if (dx > 0)
                    actor.FacingDegrees = 90;
                else if (dx < 0)
                    actor.FacingDegrees = 270;
                else if (dy > 0)
                    actor.FacingDegrees = 180;
                else if (dy < 0)
                    actor.FacingDegrees = 0;

                var step = definition.Movement.SpeedPerTick;
                var nextWorld = new Int2(
                    actor.WorldPositionFixed.X + FixedMath.ClampStep(dx, step),
                    actor.WorldPositionFixed.Y + FixedMath.ClampStep(dy, step));

                actor.WorldPositionFixed = nextWorld;
                actor.DesiredSpeed = step;
                actor.NormalizedSpeed = 1000;
                actor.MovementPhase = "moving";

                if (nextWorld.Equals(targetWorld))
                {
                    actor.CellPosition = targetCell;
                    actor.Path.RemoveAt(0);
                    if (actor.Path.Count == 0)
                    {
                        actor.CurrentOrder = ActorOrderKind.Idle;
                        actor.DesiredSpeed = 0;
                        actor.NormalizedSpeed = 0;
                        actor.MovementPhase = "idle";
                    }
                }
            }
        }

        void UpdatePowerAndActorFlags()
        {
            foreach (var player in SortedPlayers())
            {
                var generated = 0;
                var consumed = 0;
                foreach (var actor in SortedActors())
                {
                    if (actor.OwnerPlayerId != player.PlayerId || actor.ManuallyPoweredOff)
                        continue;

                    var definition = Rules.GetDefinition(actor.TypeId);
                    if (definition.Kind != ActorKind.Building)
                        continue;

                    generated += definition.Power.Generated;
                    consumed += definition.Power.Consumed;
                }

                player.PowerGenerated = generated;
                player.PowerConsumed = consumed;
                if (player.ForcedPowerState.HasValue)
                    player.PowerState = player.ForcedPowerState.Value;
                else if (generated <= 0 && consumed > 0)
                    player.PowerState = PlayerPowerState.Offline;
                else if (generated < consumed)
                    player.PowerState = PlayerPowerState.LowPower;
                else
                    player.PowerState = PlayerPowerState.Normal;
            }

            foreach (var actor in SortedActors())
            {
                var definition = Rules.GetDefinition(actor.TypeId);
                var player = players[actor.OwnerPlayerId];
                var isBuilding = definition.Kind == ActorKind.Building;
                var powered = !actor.ManuallyPoweredOff && player.PowerState != PlayerPowerState.Offline;
                var lowPower = isBuilding && player.PowerState == PlayerPowerState.LowPower;

                actor.IsPowered = !isBuilding || powered;
                actor.IsLowPower = lowPower;
                actor.LightsActive = !isBuilding || (powered && !lowPower);
                actor.MachineryActive = !isBuilding || (powered && !lowPower);
                actor.IsProducing = false;
                actor.ProductionProgress = 0;

                foreach (var item in player.ProductionQueue)
                {
                    if (item.ProducerActorId.Value == actor.Id.Value && item.State != ProductionItemState.CompletedPendingPlacement)
                    {
                        actor.IsProducing = item.State == ProductionItemState.Active;
                        actor.ProductionProgress = item.ProgressTicks;
                        break;
                    }
                }

                if (player.PowerState == PlayerPowerState.Offline && isBuilding)
                    actor.AnimationStateId = definition.Animation.OfflineStateId;
                else if (lowPower)
                    actor.AnimationStateId = definition.Animation.LowPowerStateId;
                else if (actor.IsProducing)
                    actor.AnimationStateId = definition.Animation.ProducingStateId;
                else
                    actor.AnimationStateId = definition.Animation.IdleStateId;
            }
        }

        CommandResult ValidatePlacement(int playerId, string typeId, Int2 topLeftCell, bool requirePendingPlacement)
        {
            ActorDefinition actorDefinition;
            if (!Rules.TryGetDefinition(typeId, out actorDefinition))
                return CommandResult.Fail("UnknownActorType", "The building type is not registered in the rules.");

            var building = actorDefinition as BuildingDefinition;
            if (building == null)
                return CommandResult.Fail("NotABuilding", "Only building definitions can be placed.");

            var footprintCells = GetFootprintCells(typeId, topLeftCell);
            foreach (var cell in footprintCells)
            {
                if (!Map.Contains(cell))
                    return CommandResult.Fail("OutsideMap", "The building footprint extends outside the map.");
                if (Map.IsBlocked(cell))
                    return CommandResult.Fail("BlockedCell", "The building footprint includes a blocked cell.");
                if (Map.HasBuildingAt(cell))
                    return CommandResult.Fail("OccupiedCell", "The building footprint overlaps an occupied cell.");
            }

            if (!IsInsideConstructionRadius(playerId, footprintCells))
                return CommandResult.Fail("OutsideConstructionRadius", "The building is outside the owned powered construction radius.");

            if (requirePendingPlacement && !HasPendingPlacement(playerId, typeId))
                return CommandResult.Fail("NoCompletedBuildingPending", "No completed production item is pending placement for this building type.");

            return CommandResult.Ok("Placement is valid.");
        }

        List<Int2> GetFootprintCells(string typeId, Int2 topLeftCell)
        {
            var result = new List<Int2>();
            ActorDefinition actorDefinition;
            if (!Rules.TryGetDefinition(typeId, out actorDefinition))
                return result;

            var building = actorDefinition as BuildingDefinition;
            if (building == null)
                return result;

            for (var y = 0; y < building.FootprintCells.Y; y++)
                for (var x = 0; x < building.FootprintCells.X; x++)
                    result.Add(new Int2(topLeftCell.X + x, topLeftCell.Y + y));

            return result;
        }

        bool IsInsideConstructionRadius(int playerId, IReadOnlyList<Int2> footprintCells)
        {
            foreach (var provider in SortedActors())
            {
                if (provider.OwnerPlayerId != playerId || !provider.IsPowered)
                    continue;

                var providerDefinition = Rules.GetDefinition(provider.TypeId) as BuildingDefinition;
                if (providerDefinition == null || !providerDefinition.ProvidesConstructionRadius)
                    continue;

                foreach (var cell in footprintCells)
                    if (cell.ManhattanDistanceTo(provider.CellPosition) <= providerDefinition.ConstructionRadiusCells)
                        return true;
            }

            return false;
        }

        bool HasPendingPlacement(int playerId, string typeId)
        {
            foreach (var item in players[playerId].ProductionQueue)
                if (item.TypeId == typeId && item.State == ProductionItemState.CompletedPendingPlacement)
                    return true;

            return false;
        }

        void RemoveFirstPendingPlacement(int playerId, string typeId)
        {
            var queue = players[playerId].MutableProductionQueue;
            for (var i = 0; i < queue.Count; i++)
            {
                if (queue[i].TypeId == typeId && queue[i].State == ProductionItemState.CompletedPendingPlacement)
                {
                    queue.RemoveAt(i);
                    return;
                }
            }
        }

        bool TryGetOwnedActor(int playerId, ActorId actorId, out ActorState actor)
        {
            if (!actors.TryGetValue(actorId.Value, out actor))
                return false;

            return actor.OwnerPlayerId == playerId;
        }

        static bool Contains(IReadOnlyList<string> values, string value)
        {
            for (var i = 0; i < values.Count; i++)
                if (values[i] == value)
                    return true;

            return false;
        }

        List<PlayerState> SortedPlayers()
        {
            var list = new List<PlayerState>(players.Values);
            list.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));
            return list;
        }

        List<ActorState> SortedActors()
        {
            var list = new List<ActorState>(actors.Values);
            list.Sort((a, b) => a.Id.Value.CompareTo(b.Id.Value));
            return list;
        }
    }
}
