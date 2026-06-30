using System;
using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Production;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;

namespace ProjectAegisRTS.Ai
{
    public enum AiIntentKind
    {
        Economy,
        Production,
        Attack,
        Scouting,
        Defense
    }

    public sealed class AiDifficultyDefinition
    {
        public string DifficultyId { get; private set; }
        public int DecisionIntervalTicks { get; private set; }
        public int MaxQueuedProductionItems { get; private set; }
        public int AttackSquadSize { get; private set; }
        public int DeterministicSeed { get; private set; }

        public AiDifficultyDefinition(string difficultyId, int decisionIntervalTicks, int maxQueuedProductionItems, int attackSquadSize, int deterministicSeed)
        {
            DifficultyId = string.IsNullOrEmpty(difficultyId) ? "standard" : difficultyId;
            DecisionIntervalTicks = decisionIntervalTicks <= 0 ? 16 : decisionIntervalTicks;
            MaxQueuedProductionItems = maxQueuedProductionItems <= 0 ? 3 : maxQueuedProductionItems;
            AttackSquadSize = attackSquadSize <= 0 ? 2 : attackSquadSize;
            DeterministicSeed = deterministicSeed;
        }

        public static AiDifficultyDefinition CreateStandard()
        {
            return new AiDifficultyDefinition("standard", 16, 3, 2, 1200);
        }
    }

    public sealed class AiPlayerDefinition
    {
        public int PlayerId { get; private set; }
        public bool Enabled { get; private set; }
        public AiDifficultyDefinition Difficulty { get; private set; }

        public AiPlayerDefinition(int playerId, AiDifficultyDefinition difficulty)
            : this(playerId, true, difficulty)
        {
        }

        public AiPlayerDefinition(int playerId, bool enabled, AiDifficultyDefinition difficulty)
        {
            PlayerId = playerId;
            Enabled = enabled;
            Difficulty = difficulty ?? AiDifficultyDefinition.CreateStandard();
        }
    }

    public sealed class AiIntent
    {
        public int SequenceId { get; private set; }
        public int Tick { get; private set; }
        public int PlayerId { get; private set; }
        public AiIntentKind Kind { get; private set; }
        public string IntentId { get; private set; }
        public string CommandType { get; private set; }
        public string TargetTypeId { get; private set; }
        public int SourceActorId { get; private set; }
        public int TargetActorId { get; private set; }
        public Int2 TargetCell { get; private set; }
        public bool WasCommandIssued { get; private set; }
        public bool CommandSucceeded { get; private set; }
        public string ResultCode { get; private set; }
        public string Status { get; private set; }

        public AiIntent(int sequenceId, int tick, int playerId, AiIntentKind kind, string intentId, string commandType, string targetTypeId, int sourceActorId, int targetActorId, Int2 targetCell, bool wasCommandIssued, bool commandSucceeded, string resultCode, string status)
        {
            SequenceId = sequenceId;
            Tick = tick;
            PlayerId = playerId;
            Kind = kind;
            IntentId = string.IsNullOrEmpty(intentId) ? kind.ToString() : intentId;
            CommandType = commandType ?? string.Empty;
            TargetTypeId = targetTypeId ?? string.Empty;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            TargetCell = targetCell;
            WasCommandIssued = wasCommandIssued;
            CommandSucceeded = commandSucceeded;
            ResultCode = resultCode ?? string.Empty;
            Status = status ?? string.Empty;
        }
    }

    public sealed class AiPlanState
    {
        const int MaxRecentIntents = 24;
        readonly List<AiIntent> recentIntents;

        public int PlayerId { get; private set; }
        public bool Enabled { get; set; }
        public AiDifficultyDefinition Difficulty { get; private set; }
        public int DecisionSequence { get; set; }
        public int NextDecisionTick { get; set; }
        public int ConsecutiveInvalidCommands { get; set; }
        public string CurrentPlan { get; set; }

        public AiPlanState(AiPlayerDefinition definition, int currentTick)
        {
            PlayerId = definition.PlayerId;
            Enabled = definition.Enabled;
            Difficulty = definition.Difficulty;
            DecisionSequence = 0;
            NextDecisionTick = currentTick;
            ConsecutiveInvalidCommands = 0;
            CurrentPlan = "initializing";
            recentIntents = new List<AiIntent>();
        }

        public IReadOnlyList<AiIntent> RecentIntents
        {
            get { return recentIntents; }
        }

        public void RecordIntent(AiIntent intent)
        {
            recentIntents.Add(intent);
            while (recentIntents.Count > MaxRecentIntents)
                recentIntents.RemoveAt(0);

            if (intent.WasCommandIssued && !intent.CommandSucceeded)
                ConsecutiveInvalidCommands++;
            else if (intent.WasCommandIssued && intent.CommandSucceeded)
                ConsecutiveInvalidCommands = 0;
        }
    }

    public sealed class AiSystem
    {
        readonly Dictionary<int, AiPlanState> plans;
        readonly AiCommandPlanner planner;

        public AiSystem()
        {
            plans = new Dictionary<int, AiPlanState>();
            planner = new AiCommandPlanner();
        }

        public IReadOnlyDictionary<int, AiPlanState> Plans
        {
            get { return plans; }
        }

        public AiPlanState RegisterPlayer(AiPlayerDefinition definition, int currentTick)
        {
            var state = new AiPlanState(definition, currentTick);
            plans[definition.PlayerId] = state;
            return state;
        }

        public void Tick(RtsWorld world)
        {
            foreach (var state in SortedPlans())
            {
                if (!state.Enabled || world.TickNumber < state.NextDecisionTick || !world.Players.ContainsKey(state.PlayerId))
                    continue;

                state.DecisionSequence++;
                state.CurrentPlan = "evaluating";
                planner.Plan(world, state);
                state.NextDecisionTick = world.TickNumber + state.Difficulty.DecisionIntervalTicks;
            }
        }

        public AiSnapshot CreateSnapshot()
        {
            var players = new List<AiPlayerSnapshot>();
            foreach (var plan in SortedPlans())
            {
                var intents = new List<AiIntentSnapshot>();
                foreach (var intent in plan.RecentIntents)
                {
                    intents.Add(new AiIntentSnapshot(
                        intent.SequenceId,
                        intent.Tick,
                        intent.Kind.ToString(),
                        intent.IntentId,
                        intent.CommandType,
                        intent.TargetTypeId,
                        intent.SourceActorId,
                        intent.TargetActorId,
                        intent.TargetCell,
                        intent.WasCommandIssued,
                        intent.CommandSucceeded,
                        intent.ResultCode,
                        intent.Status));
                }

                players.Add(new AiPlayerSnapshot(
                    plan.PlayerId,
                    plan.Enabled,
                    plan.Difficulty.DifficultyId,
                    plan.DecisionSequence,
                    plan.NextDecisionTick,
                    plan.ConsecutiveInvalidCommands,
                    plan.CurrentPlan,
                    intents));
            }

            return new AiSnapshot(players);
        }

        List<AiPlanState> SortedPlans()
        {
            var result = new List<AiPlanState>(plans.Values);
            result.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));
            return result;
        }
    }

    public sealed class AiCommandPlanner
    {
        readonly AiEconomyPlanner economyPlanner;
        readonly AiProductionPlanner productionPlanner;
        readonly AiAttackPlanner attackPlanner;
        readonly AiScoutingPlanner scoutingPlanner;
        readonly AiDefensePlanner defensePlanner;

        public AiCommandPlanner()
        {
            economyPlanner = new AiEconomyPlanner();
            productionPlanner = new AiProductionPlanner();
            attackPlanner = new AiAttackPlanner();
            scoutingPlanner = new AiScoutingPlanner();
            defensePlanner = new AiDefensePlanner();
        }

        public void Plan(RtsWorld world, AiPlanState state)
        {
            var placementIntent = productionPlanner.PlaceCompletedBuilding(world, state);
            if (placementIntent != null)
                state.RecordIntent(placementIntent);

            state.RecordIntent(economyPlanner.Plan(world, state));
            state.RecordIntent(productionPlanner.Plan(world, state));
            state.RecordIntent(attackPlanner.Plan(world, state));
            state.RecordIntent(scoutingPlanner.Plan(world, state));
            state.RecordIntent(defensePlanner.Plan(world, state));
            state.CurrentPlan = "complete";
        }
    }

    public sealed class AiEconomyPlanner
    {
        public AiIntent Plan(RtsWorld world, AiPlanState state)
        {
            var harvester = FirstOwnedHarvesterNeedingOrder(world, state.PlayerId);
            if (harvester == null)
                return Hold(world, state, "EconomyStable", "No idle harvester requires assignment.");

            var resource = NearestResource(world, harvester.CellPosition);
            if (resource == null)
                return Hold(world, state, "EconomyNoResource", "No resource cell is available.");

            var result = world.IssueCommand(new IssueHarvestOrderCommand(state.PlayerId, new[] { harvester.Id }, resource.Cell));
            return new AiIntent(
                state.DecisionSequence,
                world.TickNumber,
                state.PlayerId,
                AiIntentKind.Economy,
                "AssignHarvester",
                "IssueHarvestOrder",
                "resource",
                harvester.Id.Value,
                0,
                resource.Cell,
                true,
                result.Success,
                result.Success ? "Ok" : result.ErrorCode,
                result.Success ? "Assigned idle harvester to resource cell." : result.Message);
        }

        static AiIntent Hold(RtsWorld world, AiPlanState state, string intentId, string status)
        {
            return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Economy, intentId, string.Empty, string.Empty, 0, 0, Int2.Zero, false, false, string.Empty, status);
        }

        static ActorState FirstOwnedHarvesterNeedingOrder(RtsWorld world, int playerId)
        {
            foreach (var actor in SortedActors(world))
            {
                if (actor.OwnerPlayerId != playerId || actor.TypeId != "harvester" || actor.IsDestroyed || actor.HasHarvestOrder)
                    continue;

                HarvesterState harvester;
                if (world.Harvesters.TryGetValue(actor.Id.Value, out harvester) && harvester.State == HarvesterWorkState.Idle)
                    return actor;
            }

            return null;
        }

        static ResourceCellState NearestResource(RtsWorld world, Int2 fromCell)
        {
            ResourceCellState best = null;
            var bestDistance = int.MaxValue;
            foreach (var resource in SortedResources(world))
            {
                if (resource.IsDepleted)
                    continue;

                var distance = fromCell.ManhattanDistanceTo(resource.Cell);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = resource;
                }
            }

            return best;
        }

        static List<ActorState> SortedActors(RtsWorld world)
        {
            var result = new List<ActorState>(world.Actors.Values);
            result.Sort((a, b) => a.Id.Value.CompareTo(b.Id.Value));
            return result;
        }

        static List<ResourceCellState> SortedResources(RtsWorld world)
        {
            var result = new List<ResourceCellState>(world.ResourceCells.Values);
            result.Sort((a, b) =>
            {
                var y = a.Cell.Y.CompareTo(b.Cell.Y);
                return y != 0 ? y : a.Cell.X.CompareTo(b.Cell.X);
            });
            return result;
        }
    }

    public sealed class AiProductionPlanner
    {
        public AiIntent PlaceCompletedBuilding(RtsWorld world, AiPlanState state)
        {
            foreach (var item in world.Players[state.PlayerId].ProductionQueue)
            {
                if (item.State != ProductionItemState.CompletedPendingPlacement)
                    continue;

                var cell = FindPlacementCell(world, state.PlayerId, item.TypeId);
                if (cell == null)
                {
                    return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Production, "PlaceCompletedBuildingBlocked", "PlaceBuilding", item.TypeId, item.ProducerActorId.Value, 0, Int2.Zero, false, false, "NoPlacementCell", "No deterministic placement cell is currently available.");
                }

                var result = world.IssueCommand(new PlaceBuildingCommand(state.PlayerId, item.TypeId, cell.Value));
                return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Production, "PlaceCompletedBuilding", "PlaceBuilding", item.TypeId, item.ProducerActorId.Value, 0, cell.Value, true, result.Success, result.Success ? "Ok" : result.ErrorCode, result.Success ? "Placed completed AI building." : result.Message);
            }

            return null;
        }

        public AiIntent Plan(RtsWorld world, AiPlanState state)
        {
            if (world.Players[state.PlayerId].ProductionQueue.Count >= state.Difficulty.MaxQueuedProductionItems)
                return Hold(world, state, "ProductionQueueFull", "AI production queue is at the configured cap.");

            var typeId = ChooseProductionType(world, state.PlayerId);
            if (string.IsNullOrEmpty(typeId))
                return Hold(world, state, "ProductionStable", "No production need selected this interval.");

            if (HasQueuedType(world, state.PlayerId, typeId))
                return Hold(world, state, "ProductionAlreadyQueued", typeId + " is already queued or pending placement.");

            var producer = FindProducer(world, state.PlayerId, typeId);
            if (producer == null)
                return Hold(world, state, "ProductionNoProducer", "No producer can build " + typeId + ".");

            var result = world.IssueCommand(new BeginProductionCommand(state.PlayerId, producer.Id, typeId));
            return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Production, "QueueProduction", "BeginProduction", typeId, producer.Id.Value, 0, producer.CellPosition, true, result.Success, result.Success ? "Ok" : result.ErrorCode, result.Success ? "Queued " + typeId + "." : result.Message);
        }

        static AiIntent Hold(RtsWorld world, AiPlanState state, string intentId, string status)
        {
            return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Production, intentId, string.Empty, string.Empty, 0, 0, Int2.Zero, false, false, string.Empty, status);
        }

        static string ChooseProductionType(RtsWorld world, int playerId)
        {
            var player = world.Players[playerId];
            var powerMargin = player.PowerGenerated - player.PowerConsumed;
            if (powerMargin < 10)
                return "power_plant";
            if (CountOwnedActors(world, playerId, "refinery") == 0)
                return "refinery";
            if (CountOwnedActors(world, playerId, "barracks") == 0)
                return "barracks";
            if (CountOwnedActors(world, playerId, "war_factory") == 0)
                return "war_factory";
            if (CountOwnedActors(world, playerId, "rifle_infantry") < 4)
                return "rifle_infantry";
            if (CountOwnedActors(world, playerId, "light_tank") < 3)
                return "light_tank";

            return string.Empty;
        }

        static ActorState FindProducer(RtsWorld world, int playerId, string typeId)
        {
            foreach (var actor in SortedActors(world))
            {
                if (actor.OwnerPlayerId != playerId || actor.IsDestroyed)
                    continue;

                var building = world.Rules.GetDefinition(actor.TypeId) as BuildingDefinition;
                if (building == null || !actor.IsPowered)
                    continue;

                for (var i = 0; i < building.ProducesTypeIds.Count; i++)
                    if (building.ProducesTypeIds[i] == typeId)
                        return actor;
            }

            return null;
        }

        static bool HasQueuedType(RtsWorld world, int playerId, string typeId)
        {
            foreach (var item in world.Players[playerId].ProductionQueue)
                if (item.TypeId == typeId)
                    return true;

            return false;
        }

        static int CountOwnedActors(RtsWorld world, int playerId, string typeId)
        {
            var count = 0;
            foreach (var actor in world.Actors.Values)
                if (actor.OwnerPlayerId == playerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    count++;
            return count;
        }

        static Int2? FindPlacementCell(RtsWorld world, int playerId, string typeId)
        {
            var hub = FirstOwnedActor(world, playerId, "fabrication_hub");
            if (hub == null)
                return null;

            var offsets = new[]
            {
                new Int2(4, 0),
                new Int2(0, 4),
                new Int2(5, 4),
                new Int2(4, 7),
                new Int2(8, 0),
                new Int2(0, 8),
                new Int2(8, 5),
                new Int2(5, 8),
                new Int2(-3, 0),
                new Int2(0, -3),
            };

            for (var i = 0; i < offsets.Length; i++)
            {
                var candidate = hub.CellPosition + offsets[i];
                var preview = world.PreviewPlacement(playerId, typeId, candidate);
                if (preview.CanPlace)
                    return candidate;
            }

            return null;
        }

        static ActorState FirstOwnedActor(RtsWorld world, int playerId, string typeId)
        {
            foreach (var actor in SortedActors(world))
                if (actor.OwnerPlayerId == playerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    return actor;
            return null;
        }

        static List<ActorState> SortedActors(RtsWorld world)
        {
            var result = new List<ActorState>(world.Actors.Values);
            result.Sort((a, b) => a.Id.Value.CompareTo(b.Id.Value));
            return result;
        }
    }

    public sealed class AiAttackPlanner
    {
        public AiIntent Plan(RtsWorld world, AiPlanState state)
        {
            var squad = AttackSquad(world, state.PlayerId);
            if (squad.Count < state.Difficulty.AttackSquadSize)
                return Hold(world, state, "AttackSquadBuilding", "AI squad is below the attack threshold.");

            var target = FirstAttackableEnemy(world, state.PlayerId, squad);
            if (target == null)
                return Hold(world, state, "AttackNoTargetInRange", "No enemy target is currently in range for the basic attack-wave placeholder.");

            var ids = new List<ActorId>();
            foreach (var actor in squad)
                ids.Add(actor.Id);

            var result = world.IssueCommand(new IssueAttackOrderCommand(state.PlayerId, ids, target.Id));
            return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Attack, "BasicAttackWave", "IssueAttackOrder", target.TypeId, ids[0].Value, target.Id.Value, target.CellPosition, true, result.Success, result.Success ? "Ok" : result.ErrorCode, result.Success ? "Issued basic attack-wave order." : result.Message);
        }

        static AiIntent Hold(RtsWorld world, AiPlanState state, string intentId, string status)
        {
            return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Attack, intentId, string.Empty, string.Empty, 0, 0, Int2.Zero, false, false, string.Empty, status);
        }

        static List<ActorState> AttackSquad(RtsWorld world, int playerId)
        {
            var result = new List<ActorState>();
            foreach (var actor in SortedActors(world))
            {
                if (actor.OwnerPlayerId != playerId || actor.IsDestroyed)
                    continue;
                if (actor.TypeId == "harvester")
                    continue;

                var definition = world.Rules.GetDefinition(actor.TypeId);
                if (definition.Kind == ActorKind.Unit && definition.Weapon != null && actor.CurrentOrder != ActorOrderKind.Attack)
                    result.Add(actor);
            }

            return result;
        }

        static ActorState FirstAttackableEnemy(RtsWorld world, int playerId, IReadOnlyList<ActorState> squad)
        {
            foreach (var enemy in SortedActors(world))
            {
                if (enemy.OwnerPlayerId == playerId || enemy.IsDestroyed)
                    continue;

                var allInRange = true;
                for (var i = 0; i < squad.Count; i++)
                {
                    var weapon = world.Rules.GetDefinition(squad[i].TypeId).Weapon;
                    if (weapon == null || squad[i].CellPosition.ManhattanDistanceTo(enemy.CellPosition) > weapon.RangeCells)
                    {
                        allInRange = false;
                        break;
                    }
                }

                if (allInRange)
                    return enemy;
            }

            return null;
        }

        static List<ActorState> SortedActors(RtsWorld world)
        {
            var result = new List<ActorState>(world.Actors.Values);
            result.Sort((a, b) => a.Id.Value.CompareTo(b.Id.Value));
            return result;
        }
    }

    public sealed class AiScoutingPlanner
    {
        public AiIntent Plan(RtsWorld world, AiPlanState state)
        {
            var scout = FirstOwnedScout(world, state.PlayerId);
            var target = scout == null ? Int2.Zero : new Int2(Math.Min(world.Map.Width - 2, scout.CellPosition.X + 3), Math.Max(1, scout.CellPosition.Y - 2));
            return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Scouting, scout == null ? "ScoutingNoScout" : "ScoutingPlaceholder", string.Empty, "scout_rover", scout == null ? 0 : scout.Id.Value, 0, target, false, false, string.Empty, scout == null ? "No scout unit is available yet." : "Reserved deterministic scout target for a later movement planner.");
        }

        static ActorState FirstOwnedScout(RtsWorld world, int playerId)
        {
            foreach (var actor in world.Actors.Values)
                if (actor.OwnerPlayerId == playerId && actor.TypeId == "scout_rover" && !actor.IsDestroyed)
                    return actor;
            return null;
        }
    }

    public sealed class AiDefensePlanner
    {
        public AiIntent Plan(RtsWorld world, AiPlanState state)
        {
            var hub = FirstOwnedHub(world, state.PlayerId);
            return new AiIntent(state.DecisionSequence, world.TickNumber, state.PlayerId, AiIntentKind.Defense, hub == null ? "DefenseNoHub" : "DefenseHold", string.Empty, "fabrication_hub", hub == null ? 0 : hub.Id.Value, 0, hub == null ? Int2.Zero : hub.CellPosition, false, false, string.Empty, hub == null ? "No base hub is available for defensive planning." : "Reserved hub defense anchor for later threat scoring.");
        }

        static ActorState FirstOwnedHub(RtsWorld world, int playerId)
        {
            foreach (var actor in world.Actors.Values)
                if (actor.OwnerPlayerId == playerId && actor.TypeId == "fabrication_hub" && !actor.IsDestroyed)
                    return actor;
            return null;
        }
    }
}
