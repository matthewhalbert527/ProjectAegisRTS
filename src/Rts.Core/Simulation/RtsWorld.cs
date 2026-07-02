using System;
using System.Collections.Generic;
using System.Text;
using ProjectAegisRTS.Ai;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Match;
using ProjectAegisRTS.Pathfinding;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Production;
using ProjectAegisRTS.Projectiles;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.Terrain;
using ProjectAegisRTS.Visibility;

namespace ProjectAegisRTS.Simulation
{
    public sealed class RtsWorld
    {
        readonly Dictionary<int, PlayerState> players;
        readonly Dictionary<int, ActorState> actors;
        readonly Dictionary<int, ProjectileState> projectiles;
        readonly Dictionary<Int2, ResourceCellState> resourceCells;
        readonly Dictionary<int, HarvesterState> harvesters;
        readonly Dictionary<int, RefineryState> refineries;
        readonly Dictionary<int, PlayerVisibilityState> visibilityStates;
        readonly List<CombatEventSnapshot> recentCombatEvents;
        readonly List<EconomyEventSnapshot> recentEconomyEvents;
        readonly List<PathDebugSnapshot> recentPathQueries;
        readonly MatchState matchState;
        readonly AiSystem aiSystem;
        readonly GridPathfinder pathfinder;
        int nextActorId;
        int nextQueueItemId;
        int nextProjectileId;
        int nextCombatEventId;
        int nextEconomyEventId;
        int nextPathQueryId;
        const int MaxRecentCombatEvents = 64;
        const int MaxRecentEconomyEvents = 64;
        const int MaxRecentPathQueries = 32;
        const int HarvesterCargoCapacity = 200;
        const int RefineryUnloadRatePerTick = 25;

        public RtsRules Rules { get; private set; }
        public GridMap Map { get; private set; }
        public int TickNumber { get; private set; }

        public RtsWorld(RtsRules rules, GridMap map)
        {
            Rules = rules;
            Map = map;
            players = new Dictionary<int, PlayerState>();
            actors = new Dictionary<int, ActorState>();
            projectiles = new Dictionary<int, ProjectileState>();
            resourceCells = new Dictionary<Int2, ResourceCellState>();
            harvesters = new Dictionary<int, HarvesterState>();
            refineries = new Dictionary<int, RefineryState>();
            visibilityStates = new Dictionary<int, PlayerVisibilityState>();
            recentCombatEvents = new List<CombatEventSnapshot>();
            recentEconomyEvents = new List<EconomyEventSnapshot>();
            recentPathQueries = new List<PathDebugSnapshot>();
            matchState = new MatchState();
            aiSystem = new AiSystem();
            pathfinder = new GridPathfinder();
            nextActorId = 1;
            nextQueueItemId = 1;
            nextProjectileId = 1;
            nextCombatEventId = 1;
            nextEconomyEventId = 1;
            nextPathQueryId = 1;
        }

        public IReadOnlyDictionary<int, PlayerState> Players
        {
            get { return players; }
        }

        public IReadOnlyDictionary<int, ActorState> Actors
        {
            get { return actors; }
        }

        public IReadOnlyDictionary<int, ProjectileState> Projectiles
        {
            get { return projectiles; }
        }

        public IReadOnlyDictionary<Int2, ResourceCellState> ResourceCells
        {
            get { return resourceCells; }
        }

        public IReadOnlyDictionary<int, HarvesterState> Harvesters
        {
            get { return harvesters; }
        }

        public IReadOnlyDictionary<int, RefineryState> Refineries
        {
            get { return refineries; }
        }

        public IReadOnlyDictionary<int, PlayerVisibilityState> VisibilityStates
        {
            get { return visibilityStates; }
        }

        public AiSystem AiSystem
        {
            get { return aiSystem; }
        }

        public MatchState MatchState
        {
            get { return matchState; }
        }

        public PlayerState AddPlayer(int playerId, string name, int credits)
        {
            var player = new PlayerState(playerId, name, credits);
            players.Add(playerId, player);
            visibilityStates[playerId] = new PlayerVisibilityState(playerId, Map.Width, Map.Height);
            return player;
        }

        public AiPlanState ConfigureAiPlayer(AiPlayerDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");
            if (!players.ContainsKey(definition.PlayerId))
                throw new InvalidOperationException("AI player must be registered before it can be configured: " + definition.PlayerId);

            return aiSystem.RegisterPlayer(definition, TickNumber);
        }

        public ActorState CreateActor(string typeId, int ownerPlayerId, Int2 cell)
        {
            return CreateActorInternal(typeId, ownerPlayerId, cell, PlacementGridMetrics.CoarseCellToPlacementCell(cell));
        }

        ActorState CreateActorAtPlacementCell(string typeId, int ownerPlayerId, Int2 topLeftPlacementCell)
        {
            return CreateActorInternal(typeId, ownerPlayerId, PlacementGridMetrics.PlacementCellToCoarseCell(topLeftPlacementCell), topLeftPlacementCell);
        }

        ActorState CreateActorInternal(string typeId, int ownerPlayerId, Int2 cell, Int2 topLeftPlacementCell)
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
            {
                actor.SetBuildingPlacement(topLeftPlacementCell, building.PlacementFootprintCells);
                Map.OccupyBuildingAtPlacement(topLeftPlacementCell, building.PlacementFootprintCells, actor.Id);
                if (typeId == "refinery")
                    refineries[actor.Id.Value] = new RefineryState(actor.Id.Value, GetRefineryDockCell(actor, building), RefineryUnloadRatePerTick);
            }
            else if (typeId == "harvester")
            {
                harvesters[actor.Id.Value] = new HarvesterState(actor.Id.Value, HarvesterCargoCapacity);
            }

            UpdatePowerAndActorFlags();
            return actor;
        }

        public void AddResourceCell(Int2 cell, ResourceKind kind, int amount)
        {
            if (!Map.Contains(cell))
                throw new InvalidOperationException("Resource cell outside map: " + cell);

            resourceCells[cell] = new ResourceCellState(cell, kind, amount);
            if (Map.GetTerrainKind(cell) == TerrainKind.Clear)
                Map.SetTerrainKind(cell, TerrainKind.OreField);
        }

        public void SetTerrainCell(Int2 cell, TerrainKind kind)
        {
            if (!Map.Contains(cell))
                throw new InvalidOperationException("Terrain cell outside map: " + cell);

            Map.SetTerrainKind(cell, kind);
        }

        public PathQueryResult QueryPath(ActorId actorId, Int2 destinationCell)
        {
            ActorState actor;
            if (!actors.TryGetValue(actorId.Value, out actor))
            {
                var missing = new PathQueryResult(false, destinationCell, destinationCell, MovementClass.Wheeled, 0, 0, "ActorMissing", new Int2[0]);
                RecordPathQuery(0, missing);
                return missing;
            }

            var result = QueryPathForActor(actor, destinationCell);
            RecordPathQuery(actor.Id.Value, result);
            return result;
        }

        public MapValidationResult ValidateMapForPlayer(int playerId)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (!players.ContainsKey(playerId))
                errors.Add("UnknownPlayer:" + playerId);

            foreach (var actor in SortedActors())
            {
                if (!Map.Contains(actor.CellPosition))
                    errors.Add("ActorOutsideMap:" + actor.Id.Value + ":" + actor.CellPosition);

                var definition = Rules.GetDefinition(actor.TypeId);
                var unit = definition as UnitDefinition;
                if (unit != null && !Map.IsPassableForUnit(actor.CellPosition, unit.Movement.MovementClass, Rules))
                    errors.Add("ActorOnImpassableTerrain:" + actor.Id.Value + ":" + actor.CellPosition);
            }

            foreach (var resource in SortedResources())
            {
                if (!Map.Contains(resource.Cell))
                    errors.Add("ResourceOutsideMap:" + resource.Cell);
                else if (!Map.IsPassableForUnit(resource.Cell, MovementClass.Harvester, Rules))
                    errors.Add("ResourceOnImpassableTerrain:" + resource.Cell);
            }

            foreach (var refinery in SortedRefineries())
            {
                ActorState refineryActor;
                if (!actors.TryGetValue(refinery.ActorId, out refineryActor) || refineryActor.OwnerPlayerId != playerId)
                    continue;
                if (!Map.IsPassableForUnit(refinery.DockCell, MovementClass.Harvester, Rules))
                    errors.Add("RefineryDockBlocked:" + refinery.ActorId + ":" + refinery.DockCell);
            }

            if (resourceCells.Count == 0)
                warnings.Add("NoResourceCells");

            return new MapValidationResult(errors, warnings);
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

        public CommandResult ConfigureScenario(ScenarioDefinition scenarioDefinition)
        {
            return matchState.Configure(scenarioDefinition);
        }

        public CommandResult StartMatch()
        {
            var result = matchState.StartMatch(TickNumber);
            matchState.Update(this);
            return result;
        }

        public CommandResult ResetMatch()
        {
            return matchState.ResetMatch();
        }

        public CommandResult ApplyScenarioDamage(int playerId, ActorId targetActorId, int damage, string reason)
        {
            if (!players.ContainsKey(playerId))
                return CommandResult.Fail("UnknownPlayer", "The scenario damage request references a player that does not exist.");
            if (damage <= 0)
                return CommandResult.Fail("InvalidDamage", "Scenario damage must be greater than zero.");

            ActorState target;
            if (!actors.TryGetValue(targetActorId.Value, out target))
                return CommandResult.Fail("ActorMissing", "The scenario damage target does not exist.");
            if (target.IsDestroyed)
                return CommandResult.Fail("ActorDestroyed", "The scenario damage target is already destroyed.");

            ApplyDamage(target, damage, 0, 0, string.IsNullOrEmpty(reason) ? "scenario_debug" : reason);
            UpdatePowerAndActorFlags();
            UpdateVisibility();
            matchState.Update(this);
            return CommandResult.Ok("Scenario damage applied.");
        }

        public CommandResult GrantScenarioCredits(int playerId, int credits, string reason)
        {
            if (!players.ContainsKey(playerId))
                return CommandResult.Fail("UnknownPlayer", "The credit grant references a player that does not exist.");
            if (credits <= 0)
                return CommandResult.Fail("InvalidCredits", "Scenario credit grant must be greater than zero.");

            players[playerId].Credits += credits;
            AddEconomyEvent("ScenarioCreditsGranted", 0, 0, Int2.Zero, credits, credits);
            return CommandResult.Ok("Scenario credits granted.");
        }

        public CommandResult RevealScenarioMap(int playerId)
        {
            PlayerVisibilityState visibility;
            if (!visibilityStates.TryGetValue(playerId, out visibility))
                return CommandResult.Fail("UnknownPlayer", "The reveal request references a player that does not exist.");

            visibility.RevealAll();
            return CommandResult.Ok("Scenario map revealed.");
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
            if (command is IssueAttackMoveOrderCommand)
                return IssueAttackMoveOrder((IssueAttackMoveOrderCommand)command);
            if (command is IssueGuardOrderCommand)
                return IssueGuardOrder((IssueGuardOrderCommand)command);
            if (command is IssuePatrolOrderCommand)
                return IssuePatrolOrder((IssuePatrolOrderCommand)command);
            if (command is IssueScatterOrderCommand)
                return IssueScatterOrder((IssueScatterOrderCommand)command);
            if (command is IssueDeployOrderCommand)
                return IssueDeployOrder((IssueDeployOrderCommand)command);
            if (command is IssueForceAttackCellCommand)
                return IssueForceAttackCell((IssueForceAttackCellCommand)command);
            if (command is IssueHarvestOrderCommand)
                return IssueHarvestOrder((IssueHarvestOrderCommand)command);
            if (command is AssignHarvesterToResourceCellCommand)
                return AssignHarvesterToResourceCell((AssignHarvesterToResourceCellCommand)command);
            if (command is AssignHarvesterToRefineryCommand)
                return AssignHarvesterToRefinery((AssignHarvesterToRefineryCommand)command);
            if (command is ReturnToRefineryCommand)
                return ReturnToRefinery((ReturnToRefineryCommand)command);
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
            aiSystem.Tick(this);
            TickProduction();
            TickMovement();
            TickHarvesting();
            TickCombat();
            TickProjectiles();
            UpdatePowerAndActorFlags();
            UpdateVisibility();
            matchState.Update(this);
        }

        public PlacementPreviewSnapshot PreviewPlacement(int playerId, string typeId, Int2 topLeftCell)
        {
            var footprint = GetPlacementFootprintCells(typeId, topLeftCell);
            var result = ValidatePlacement(playerId, typeId, topLeftCell, false);
            var footprintSize = GetPlacementFootprintSize(typeId);
            return new PlacementPreviewSnapshot(typeId, topLeftCell, result.Success, result.ErrorCode, footprint, PlacementGridMetrics.PlacementGridScale, footprintSize);
        }

        public WorldSnapshot CreateSnapshot()
        {
            return CreateSnapshotForPlayer(0);
        }

        public WorldSnapshot CreateSnapshot(int perspectivePlayerId)
        {
            return CreateSnapshotForPlayer(perspectivePlayerId);
        }

        WorldSnapshot CreateSnapshotForPlayer(int perspectivePlayerId)
        {
            UpdatePowerAndActorFlags();
            UpdateVisibility();

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
                if (!ShouldIncludeActorInSnapshot(actor, perspectivePlayerId))
                    continue;

                var definition = Rules.GetDefinition(actor.TypeId);
                var unit = definition as UnitDefinition;
                var building = definition as BuildingDefinition;
                var turnRate = unit == null ? 0 : unit.Movement.TurnRateDegreesPerTick;

                actorSnapshots.Add(new ActorSnapshot(
                    actor.Id.Value,
                    actor.TypeId,
                    actor.OwnerPlayerId,
                    actor.CellPosition,
                    actor.WorldPositionFixed,
                    actor.FacingDegrees,
                    actor.Health,
                    definition.MaxHealth,
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
                    actor.MovementPhase,
                    !actor.IsDestroyed,
                    actor.IsDying,
                    actor.IsDestroyed,
                    actor.LastDamageTick,
                    actor.DeathTick,
                    actor.DestroyedByActorId,
                    actor.ActiveWeaponId,
                    actor.WeaponCooldownRemaining,
                    actor.IsAttacking,
                    actor.AttackTargetActorId,
                    actor.AttackTargetCell,
                    actor.HasHarvestOrder,
                    actor.PlacementTopLeftCell,
                    building == null ? Int2.Zero : building.PlacementFootprintCells,
                    PlacementGridMetrics.PlacementGridScale));
            }

            var projectileSnapshots = new List<ProjectileSnapshot>();
            foreach (var projectile in SortedProjectiles())
            {
                projectileSnapshots.Add(new ProjectileSnapshot(
                    projectile.ProjectileId,
                    projectile.OwnerPlayerId,
                    projectile.SourceActorId,
                    projectile.TargetActorId,
                    projectile.WeaponId,
                    projectile.ProjectileKind.ToString(),
                    projectile.CurrentPositionFixed,
                    projectile.TargetPositionFixed,
                    projectile.TargetCell,
                    projectile.SpeedSubCellsPerTick,
                    projectile.Damage,
                    projectile.HasImpacted,
                    projectile.ImpactTick));
            }

            var resourceSnapshots = new List<ResourceSnapshot>();
            foreach (var resource in SortedResources())
            {
                if (perspectivePlayerId > 0 && !IsCellExploredOrVisible(perspectivePlayerId, resource.Cell))
                    continue;

                resourceSnapshots.Add(new ResourceSnapshot(resource.Cell, resource.Kind.ToString(), resource.Amount, resource.MaxAmount, resource.IsDepleted));
            }

            var harvesterSnapshots = new List<HarvesterSnapshot>();
            foreach (var harvester in SortedHarvesters())
                harvesterSnapshots.Add(new HarvesterSnapshot(
                    harvester.ActorId,
                    harvester.CargoAmount,
                    harvester.CargoCapacity,
                    harvester.CarriedResourceKind.ToString(),
                    harvester.HarvestTargetCell,
                    harvester.AssignedRefineryActorId,
                    harvester.State.ToString(),
                    harvester.HarvestProgressTicks,
                    harvester.UnloadProgressTicks));

            var refinerySnapshots = new List<RefinerySnapshot>();
            foreach (var refinery in SortedRefineries())
                refinerySnapshots.Add(new RefinerySnapshot(
                    refinery.ActorId,
                    refinery.DockCell,
                    refinery.ActiveHarvesterActorId,
                    refinery.UnloadRatePerTick,
                    refinery.IsUnloading,
                    refinery.TotalResourcesReceived));

            var economy = new EconomySnapshot(resourceSnapshots, harvesterSnapshots, refinerySnapshots, new List<EconomyEventSnapshot>(recentEconomyEvents));
            var fog = perspectivePlayerId > 0 ? CreateFogSnapshot(perspectivePlayerId) : FogSnapshot.Empty;
            var radar = perspectivePlayerId > 0 ? CreateRadarSnapshot(perspectivePlayerId) : RadarSnapshot.Empty;
            var minimap = perspectivePlayerId > 0 ? CreateMinimapSnapshot(perspectivePlayerId) : MinimapSnapshot.Empty;
            var map = CreateMapSnapshot(perspectivePlayerId);

            return new WorldSnapshot(TickNumber, playerSnapshots, actorSnapshots, projectileSnapshots, new List<CombatEventSnapshot>(recentCombatEvents), economy, fog, radar, minimap, aiSystem.CreateSnapshot(), map, matchState.CreateSnapshot(), matchState.CreateScenarioSnapshot());
        }

        public bool IsCellVisible(int playerId, Int2 cell)
        {
            PlayerVisibilityState visibility;
            return visibilityStates.TryGetValue(playerId, out visibility) && visibility.IsVisible(cell);
        }

        public bool IsCellExploredOrVisible(int playerId, Int2 cell)
        {
            PlayerVisibilityState visibility;
            return visibilityStates.TryGetValue(playerId, out visibility) && visibility.IsExploredOrVisible(cell);
        }

        public string GetDeterminismSummary()
        {
            UpdatePowerAndActorFlags();
            UpdateVisibility();
            var sb = new StringBuilder();
            sb.Append("tick=").Append(TickNumber).AppendLine();
            var match = matchState.CreateSnapshot();
            sb.Append("match ")
                .Append(match.ScenarioId).Append(' ')
                .Append(match.Phase).Append(' ')
                .Append(match.LocalPlayerOutcome).Append(' ')
                .Append(match.WinningPlayerId).Append(' ')
                .Append(match.StartedTick).Append(' ')
                .Append(match.ElapsedTicks)
                .AppendLine();
            var scenario = matchState.CreateScenarioSnapshot();
            foreach (var objective in scenario.Objectives)
            {
                sb.Append("objective ")
                    .Append(objective.ObjectiveId).Append(' ')
                    .Append(objective.State)
                    .AppendLine();
            }

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

            foreach (var player in SortedPlayers())
            {
                PlayerVisibilityState visibility;
                if (!visibilityStates.TryGetValue(player.PlayerId, out visibility))
                    continue;

                var visible = 0;
                var explored = 0;
                foreach (var cellVisibility in visibility.CopyCells())
                {
                    if (cellVisibility == CellVisibility.Visible)
                        visible++;
                    else if (cellVisibility == CellVisibility.Explored)
                        explored++;
                }

                var radar = CreateRadarSnapshot(player.PlayerId);
                sb.Append("visibility ")
                    .Append(player.PlayerId)
                    .Append(" visible=").Append(visible)
                    .Append(" explored=").Append(explored)
                    .Append(" radar=").Append(radar.IsActive)
                    .Append('/').Append(radar.ProviderActorId)
                    .AppendLine();
            }

            foreach (var ai in aiSystem.CreateSnapshot().Players)
            {
                sb.Append("ai ")
                    .Append(ai.PlayerId)
                    .Append(" enabled=").Append(ai.Enabled)
                    .Append(" difficulty=").Append(ai.DifficultyId)
                    .Append(" seq=").Append(ai.DecisionSequence)
                    .Append(" next=").Append(ai.NextDecisionTick)
                    .Append(" invalid=").Append(ai.ConsecutiveInvalidCommands)
                    .Append(" plan=").Append(ai.CurrentPlan)
                    .AppendLine();

                foreach (var intent in ai.RecentIntents)
                {
                    sb.Append("ai-intent ")
                        .Append(intent.SequenceId).Append(' ')
                        .Append(intent.Tick).Append(' ')
                        .Append(intent.Kind).Append(' ')
                        .Append(intent.IntentId).Append(' ')
                        .Append(intent.CommandType).Append(' ')
                        .Append(intent.TargetTypeId).Append(' ')
                        .Append(intent.SourceActorId).Append(' ')
                        .Append(intent.TargetActorId).Append(' ')
                        .Append(intent.TargetCell).Append(' ')
                        .Append(intent.WasCommandIssued).Append(' ')
                        .Append(intent.CommandSucceeded).Append(' ')
                        .Append(intent.ResultCode)
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
                    .Append(actor.PlacementTopLeftCell).Append(' ')
                    .Append(actor.WorldPositionFixed).Append(' ')
                    .Append(actor.Health).Append(' ')
                    .Append(actor.FacingDegrees).Append(' ')
                    .Append(actor.CurrentOrder).Append(' ')
                    .Append(actor.IsPowered).Append(' ')
                    .Append(actor.IsLowPower).Append(' ')
                    .Append(actor.AttackTargetActorId).Append(' ')
                    .Append(actor.WeaponCooldownRemaining).Append(' ')
                    .Append(actor.IsAttacking).Append(' ')
                    .Append(actor.IsDestroyed).Append(' ')
                    .Append(actor.DeathTick).Append(' ')
                    .Append(actor.HasHarvestOrder)
                    .AppendLine();
            }

            foreach (var resource in SortedResources())
            {
                sb.Append("resource ")
                    .Append(resource.Cell).Append(' ')
                    .Append(resource.Kind).Append(' ')
                    .Append(resource.Amount).Append('/')
                    .Append(resource.MaxAmount)
                    .AppendLine();
            }

            for (var y = 0; y < Map.Height; y++)
                for (var x = 0; x < Map.Width; x++)
                {
                    var cell = new Int2(x, y);
                    var kind = Map.GetTerrainKind(cell);
                    if (kind == TerrainKind.Clear && !Map.IsBlocked(cell))
                        continue;

                    sb.Append("terrain ")
                        .Append(cell).Append(' ')
                        .Append(kind).Append(' ')
                        .Append(Map.IsBlocked(cell))
                        .AppendLine();
                }

            foreach (var query in recentPathQueries)
            {
                sb.Append("path ")
                    .Append(query.QueryId).Append(' ')
                    .Append(query.Tick).Append(' ')
                    .Append(query.ActorId).Append(' ')
                    .Append(query.StartCell).Append(' ')
                    .Append(query.GoalCell).Append(' ')
                    .Append(query.MovementClass).Append(' ')
                    .Append(query.Success).Append(' ')
                    .Append(query.TotalCost).Append(' ')
                    .Append(query.VisitedCellCount).Append(' ')
                    .Append(query.FailureCode)
                    .AppendLine();
            }

            foreach (var harvester in SortedHarvesters())
            {
                sb.Append("harvester ")
                    .Append(harvester.ActorId).Append(' ')
                    .Append(harvester.State).Append(' ')
                    .Append(harvester.CargoAmount).Append('/')
                    .Append(harvester.CargoCapacity).Append(' ')
                    .Append(harvester.HarvestTargetCell).Append(' ')
                    .Append(harvester.AssignedRefineryActorId).Append(' ')
                    .Append(harvester.HarvestProgressTicks).Append(' ')
                    .Append(harvester.UnloadProgressTicks)
                    .AppendLine();
            }

            foreach (var refinery in SortedRefineries())
            {
                sb.Append("refinery ")
                    .Append(refinery.ActorId).Append(' ')
                    .Append(refinery.DockCell).Append(' ')
                    .Append(refinery.ActiveHarvesterActorId).Append(' ')
                    .Append(refinery.IsUnloading).Append(' ')
                    .Append(refinery.TotalResourcesReceived)
                    .AppendLine();
            }

            foreach (var projectile in SortedProjectiles())
            {
                sb.Append("projectile ")
                    .Append(projectile.ProjectileId).Append(' ')
                    .Append(projectile.OwnerPlayerId).Append(' ')
                    .Append(projectile.SourceActorId).Append(' ')
                    .Append(projectile.TargetActorId).Append(' ')
                    .Append(projectile.WeaponId).Append(' ')
                    .Append(projectile.CurrentPositionFixed).Append(' ')
                    .Append(projectile.TargetPositionFixed).Append(' ')
                    .Append(projectile.RemainingLifetimeTicks)
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

            var actor = CreateActorAtPlacementCell(command.TypeId, command.PlayerId, command.TopLeftCell);
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

                if (actor.IsDestroyed)
                {
                    details.Add("actor " + actorId.Value + ": destroyed");
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

                var path = QueryPathForActor(actor, command.DestinationCell);
                RecordPathQuery(actor.Id.Value, path);
                if (!path.Success)
                    details.Add("actor " + actorId.Value + ": no path (" + path.FailureCode + ")");
                else
                    planned[actorId.Value] = new List<Int2>(path.Path);
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
                ClearAttackState(actor);
                ClearHarvestState(actor);
                actor.MovementPhase = actor.Path.Count == 0 ? "idle" : "moving";
            }

            return CommandResult.Ok("Move order accepted.");
        }

        CommandResult IssueAttackOrder(IssueAttackOrderCommand command)
        {
            ActorState target;
            if (!actors.TryGetValue(command.TargetActorId.Value, out target))
                return CommandResult.Fail("AttackTargetMissing", "The attack target actor does not exist.");

            if (target.IsDestroyed)
                return CommandResult.Fail("AttackTargetDestroyed", "The attack target has already been destroyed.");

            var prepared = new List<ActorState>();
            var details = new List<string>();
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                {
                    details.Add("actor " + actorId.Value + ": not owned or missing");
                    continue;
                }

                var validation = ValidateAttackTarget(actor, target);
                if (!validation.Success)
                {
                    details.Add("actor " + actorId.Value + ": " + validation.ErrorCode);
                    continue;
                }

                prepared.Add(actor);
            }

            if (details.Count > 0)
                return CommandResult.Fail("AttackOrderRejected", "One or more actors could not accept the attack order.", details);

            foreach (var actor in prepared)
            {
                actor.Path.Clear();
                actor.CurrentOrder = ActorOrderKind.Attack;
                actor.AttackTargetActorId = target.Id.Value;
                actor.AttackTargetCell = target.CellPosition;
                actor.OrderTargetCell = target.CellPosition;
                actor.ActiveWeaponId = Rules.GetDefinition(actor.TypeId).Weapon.WeaponId;
                actor.IsAttacking = true;
                ClearHarvestState(actor);
                actor.DesiredSpeed = 0;
                actor.NormalizedSpeed = 0;
                actor.MovementPhase = "attacking";
            }

            return CommandResult.Ok("Attack order accepted.");
        }

        CommandResult IssueAttackMoveOrder(IssueAttackMoveOrderCommand command)
        {
            if (!Map.Contains(command.DestinationCell))
                return CommandResult.Fail("AttackMoveOutsideMap", "The attack-move destination is outside the map.");

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

                if (!ValidateMobileArmedActor(actor, "attack-move", details))
                    continue;

                if (actor.CellPosition.Equals(command.DestinationCell))
                {
                    planned[actorId.Value] = new List<Int2>();
                    continue;
                }

                var path = QueryPathForActor(actor, command.DestinationCell);
                RecordPathQuery(actor.Id.Value, path);
                if (!path.Success)
                    details.Add("actor " + actorId.Value + ": no path (" + path.FailureCode + ")");
                else
                    planned[actorId.Value] = new List<Int2>(path.Path);
            }

            if (details.Count > 0)
                return CommandResult.Fail("AttackMoveOrderRejected", "One or more actors could not accept the attack-move order.", details);

            foreach (var pair in planned)
            {
                var actor = actors[pair.Key];
                actor.Path.Clear();
                actor.Path.AddRange(pair.Value);
                actor.CurrentOrder = ActorOrderKind.AttackMove;
                actor.OrderTargetCell = command.DestinationCell;
                ClearAttackState(actor);
                ClearHarvestState(actor);
                actor.MovementPhase = actor.Path.Count == 0 ? "attack_move_hold" : "attack_moving";
            }

            return CommandResult.Ok("Attack-move order accepted.");
        }

        CommandResult IssueGuardOrder(IssueGuardOrderCommand command)
        {
            var prepared = new List<ActorState>();
            var details = new List<string>();
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                {
                    details.Add("actor " + actorId.Value + ": not owned or missing");
                    continue;
                }

                if (!ValidateArmedActor(actor, "guard", details))
                    continue;

                prepared.Add(actor);
            }

            if (details.Count > 0)
                return CommandResult.Fail("GuardOrderRejected", "One or more actors could not accept the guard order.", details);

            foreach (var actor in prepared)
            {
                actor.Path.Clear();
                actor.CurrentOrder = ActorOrderKind.Guard;
                actor.OrderTargetCell = actor.CellPosition;
                ClearAttackState(actor);
                ClearHarvestState(actor);
                actor.DesiredSpeed = 0;
                actor.NormalizedSpeed = 0;
                actor.MovementPhase = "guarding";
            }

            return CommandResult.Ok("Guard order accepted.");
        }

        CommandResult IssuePatrolOrder(IssuePatrolOrderCommand command)
        {
            if (!Map.Contains(command.DestinationCell))
                return CommandResult.Fail("PatrolOutsideMap", "The patrol destination is outside the map.");

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

                if (!ValidateMobileArmedActor(actor, "patrol", details))
                    continue;

                if (actor.CellPosition.Equals(command.DestinationCell))
                {
                    planned[actorId.Value] = new List<Int2>();
                    continue;
                }

                var path = QueryPathForActor(actor, command.DestinationCell);
                RecordPathQuery(actor.Id.Value, path);
                if (!path.Success)
                    details.Add("actor " + actorId.Value + ": no path (" + path.FailureCode + ")");
                else
                    planned[actorId.Value] = new List<Int2>(path.Path);
            }

            if (details.Count > 0)
                return CommandResult.Fail("PatrolOrderRejected", "One or more actors could not accept the patrol order.", details);

            foreach (var pair in planned)
            {
                var actor = actors[pair.Key];
                actor.Path.Clear();
                actor.Path.AddRange(pair.Value);
                actor.CurrentOrder = ActorOrderKind.Patrol;
                actor.OrderTargetCell = command.DestinationCell;
                ClearAttackState(actor);
                ClearHarvestState(actor);
                actor.MovementPhase = actor.Path.Count == 0 ? "patrol_hold" : "patrolling";
            }

            return CommandResult.Ok("Patrol order accepted.");
        }

        CommandResult IssueScatterOrder(IssueScatterOrderCommand command)
        {
            var planned = new Dictionary<int, List<Int2>>();
            var destinations = new Dictionary<int, Int2>();
            var details = new List<string>();
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                {
                    details.Add("actor " + actorId.Value + ": not owned or missing");
                    continue;
                }

                if (!ValidateMobileActor(actor, "scatter", details))
                    continue;

                Int2 destination;
                var path = FindScatterPath(actor, out destination);
                planned[actorId.Value] = new List<Int2>(path);
                destinations[actorId.Value] = destination;
            }

            if (details.Count > 0)
                return CommandResult.Fail("ScatterOrderRejected", "One or more actors could not accept the scatter order.", details);

            foreach (var pair in planned)
            {
                var actor = actors[pair.Key];
                actor.Path.Clear();
                actor.Path.AddRange(pair.Value);
                actor.CurrentOrder = ActorOrderKind.Scatter;
                actor.OrderTargetCell = destinations[pair.Key];
                ClearAttackState(actor);
                ClearHarvestState(actor);
                actor.MovementPhase = actor.Path.Count == 0 ? "scattered" : "scattering";
            }

            return CommandResult.Ok("Scatter order accepted.");
        }

        CommandResult IssueDeployOrder(IssueDeployOrderCommand command)
        {
            var prepared = new List<ActorState>();
            var details = new List<string>();
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                {
                    details.Add("actor " + actorId.Value + ": not owned or missing");
                    continue;
                }

                if (actor.IsDestroyed)
                {
                    details.Add("actor " + actorId.Value + ": destroyed");
                    continue;
                }

                prepared.Add(actor);
            }

            if (details.Count > 0)
                return CommandResult.Fail("DeployOrderRejected", "One or more actors could not accept the deploy order.", details);

            foreach (var actor in prepared)
            {
                actor.Path.Clear();
                actor.CurrentOrder = ActorOrderKind.Deploy;
                actor.OrderTargetCell = actor.CellPosition;
                ClearAttackState(actor);
                ClearHarvestState(actor);
                actor.DesiredSpeed = 0;
                actor.NormalizedSpeed = 0;
                actor.MovementPhase = "deploy_placeholder";
            }

            return CommandResult.Ok("Deploy placeholder accepted.");
        }

        CommandResult IssueForceAttackCell(IssueForceAttackCellCommand command)
        {
            if (!Map.Contains(command.TargetCell))
                return CommandResult.Fail("ForceAttackOutsideMap", "The force-attack target cell is outside the map.");

            return CommandResult.Fail("ForceAttackCellUnsupported", "Stage 9 supports actor-target attack orders; cell force-attack is reserved for a later pass.");
        }

        CommandResult IssueHarvestOrder(IssueHarvestOrderCommand command)
        {
            if (!Map.Contains(command.ResourceCell))
                return CommandResult.Fail("HarvestCellOutsideMap", "The harvest target cell is outside the map.");

            ResourceCellState resource;
            if (!resourceCells.TryGetValue(command.ResourceCell, out resource) || resource.IsDepleted)
                return CommandResult.Fail("HarvestResourceMissing", "The target cell has no available resource.");

            var prepared = new List<ActorState>();
            var details = new List<string>();
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                {
                    details.Add("actor " + actorId.Value + ": not owned or missing");
                    continue;
                }

                HarvesterState harvester;
                if (!harvesters.TryGetValue(actor.Id.Value, out harvester))
                {
                    details.Add("actor " + actorId.Value + ": not a harvester");
                    continue;
                }

                if (FindNearestOwnedRefinery(command.PlayerId, actor.CellPosition) == null)
                {
                    details.Add("actor " + actorId.Value + ": no owned refinery");
                    continue;
                }

                var path = QueryPathForActor(actor, command.ResourceCell);
                RecordPathQuery(actor.Id.Value, path);
                if (!path.Success)
                {
                    details.Add("actor " + actorId.Value + ": no path to resource (" + path.FailureCode + ")");
                    continue;
                }

                prepared.Add(actor);
            }

            if (details.Count > 0)
                return CommandResult.Fail("HarvestOrderRejected", "One or more harvesters could not accept the harvest order.", details);

            foreach (var actor in prepared)
                StartHarvestOrder(actor, command.ResourceCell);

            return CommandResult.Ok("Harvest order accepted.");
        }

        CommandResult AssignHarvesterToResourceCell(AssignHarvesterToResourceCellCommand command)
        {
            return IssueHarvestOrder(new IssueHarvestOrderCommand(command.PlayerId, new[] { command.HarvesterActorId }, command.ResourceCell));
        }

        CommandResult AssignHarvesterToRefinery(AssignHarvesterToRefineryCommand command)
        {
            ActorState harvesterActor;
            if (!TryGetOwnedActor(command.PlayerId, command.HarvesterActorId, out harvesterActor))
                return CommandResult.Fail("HarvesterInvalid", "The harvester actor does not exist or is not owned by the command player.");

            HarvesterState harvester;
            if (!harvesters.TryGetValue(harvesterActor.Id.Value, out harvester))
                return CommandResult.Fail("HarvesterInvalid", "The selected actor is not a harvester.");

            ActorState refineryActor;
            if (!TryGetOwnedActor(command.PlayerId, command.RefineryActorId, out refineryActor))
                return CommandResult.Fail("RefineryInvalid", "The refinery actor does not exist or is not owned by the command player.");

            RefineryState refinery;
            if (!refineries.TryGetValue(refineryActor.Id.Value, out refinery))
                return CommandResult.Fail("RefineryInvalid", "The selected actor is not a refinery.");

            harvester.AssignedRefineryActorId = refinery.ActorId;
            if (harvester.CargoAmount > 0)
                SendHarvesterToRefinery(harvesterActor, harvester, refinery);

            return CommandResult.Ok("Harvester assigned to refinery.");
        }

        CommandResult ReturnToRefinery(ReturnToRefineryCommand command)
        {
            var details = new List<string>();
            var prepared = new List<ActorState>();
            foreach (var actorId in command.ActorIds)
            {
                ActorState actor;
                if (!TryGetOwnedActor(command.PlayerId, actorId, out actor))
                {
                    details.Add("actor " + actorId.Value + ": not owned or missing");
                    continue;
                }

                HarvesterState harvester;
                if (!harvesters.TryGetValue(actor.Id.Value, out harvester))
                {
                    details.Add("actor " + actorId.Value + ": not a harvester");
                    continue;
                }

                var refinery = GetAssignedOrNearestRefinery(command.PlayerId, actor, harvester);
                if (refinery == null)
                {
                    details.Add("actor " + actorId.Value + ": no owned refinery");
                    continue;
                }

                prepared.Add(actor);
            }

            if (details.Count > 0)
                return CommandResult.Fail("ReturnToRefineryRejected", "One or more harvesters could not return to refinery.", details);

            foreach (var actor in prepared)
            {
                var harvester = harvesters[actor.Id.Value];
                var refinery = GetAssignedOrNearestRefinery(command.PlayerId, actor, harvester);
                SendHarvesterToRefinery(actor, harvester, refinery);
            }

            return CommandResult.Ok("Return-to-refinery order accepted.");
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
                ClearAttackState(actor);
                ClearHarvestState(actor);
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

                    ActorState producerActor;
                    if (actors.TryGetValue(item.ProducerActorId.Value, out producerActor) && producerActor.IsDestroyed)
                    {
                        item.State = ProductionItemState.Paused;
                        continue;
                    }

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
            if (producer.IsDestroyed)
                return;

            var producerDefinition = Rules.GetDefinition(producer.TypeId) as BuildingDefinition;
            var exitOffset = producerDefinition == null ? new Int2(0, 1) : producerDefinition.UnitExitOffset;
            var producedDefinition = Rules.GetDefinition(item.TypeId) as UnitDefinition;
            var movementClass = producedDefinition == null ? MovementClass.Wheeled : producedDefinition.Movement.MovementClass;
            var spawnCell = FindNearestSpawnCell(producer.CellPosition + exitOffset, movementClass);
            var unit = CreateActor(item.TypeId, item.PlayerId, spawnCell);

            if (!producer.RallyPoint.Equals(producer.CellPosition) && !producer.RallyPoint.Equals(spawnCell))
                IssueCommand(new IssueMoveOrderCommand(item.PlayerId, new[] { unit.Id }, producer.RallyPoint));
        }

        Int2 FindNearestSpawnCell(Int2 preferred)
        {
            return FindNearestSpawnCell(preferred, MovementClass.Wheeled);
        }

        Int2 FindNearestSpawnCell(Int2 preferred, MovementClass movementClass)
        {
            if (Map.IsPassableForUnit(preferred, movementClass, Rules))
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
                        if (Map.IsPassableForUnit(candidate, movementClass, Rules))
                            return candidate;
                    }
                }
            }

            return preferred;
        }

        PathQueryResult QueryPathForActor(ActorState actor, Int2 destinationCell)
        {
            return pathfinder.QueryPath(Map, Rules, actor.CellPosition, destinationCell, GetMovementClass(actor));
        }

        MovementClass GetMovementClass(ActorState actor)
        {
            var unit = Rules.GetDefinition(actor.TypeId) as UnitDefinition;
            return unit == null ? MovementClass.Building : unit.Movement.MovementClass;
        }

        void TickMovement()
        {
            foreach (var actor in SortedActors())
            {
                if (actor.IsDestroyed)
                {
                    actor.Path.Clear();
                    actor.DesiredSpeed = 0;
                    actor.NormalizedSpeed = 0;
                    actor.MovementPhase = "destroyed";
                    continue;
                }

                var definition = Rules.GetDefinition(actor.TypeId) as UnitDefinition;
                if (definition == null || actor.Path.Count == 0)
                {
                    actor.DesiredSpeed = 0;
                    actor.NormalizedSpeed = 0;
                    actor.MovementPhase = MovementPhaseForWaitingOrder(actor);
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
                actor.MovementPhase = MovementPhaseForPathOrder(actor.CurrentOrder);

                if (nextWorld.Equals(targetWorld))
                {
                    actor.CellPosition = targetCell;
                    actor.Path.RemoveAt(0);
                    if (actor.Path.Count == 0)
                    {
                        if (IsPathFollowingOrder(actor.CurrentOrder))
                            actor.CurrentOrder = ActorOrderKind.Idle;
                        actor.DesiredSpeed = 0;
                        actor.NormalizedSpeed = 0;
                        actor.MovementPhase = "idle";
                    }
                }
            }
        }

        static bool IsPathFollowingOrder(ActorOrderKind order)
        {
            return order == ActorOrderKind.Move ||
                order == ActorOrderKind.AttackMove ||
                order == ActorOrderKind.Patrol ||
                order == ActorOrderKind.Scatter;
        }

        static string MovementPhaseForPathOrder(ActorOrderKind order)
        {
            if (order == ActorOrderKind.AttackMove)
                return "attack_moving";
            if (order == ActorOrderKind.Patrol)
                return "patrolling";
            if (order == ActorOrderKind.Scatter)
                return "scattering";
            return "moving";
        }

        static string MovementPhaseForWaitingOrder(ActorState actor)
        {
            if (actor.CurrentOrder == ActorOrderKind.Attack)
                return "attacking";
            if (actor.CurrentOrder == ActorOrderKind.AttackMove)
                return "attack_move_hold";
            if (actor.CurrentOrder == ActorOrderKind.Guard)
                return "guarding";
            if (actor.CurrentOrder == ActorOrderKind.Patrol)
                return "patrol_hold";
            if (actor.CurrentOrder == ActorOrderKind.Scatter)
                return "scattered";
            if (actor.CurrentOrder == ActorOrderKind.Deploy)
                return "deploy_placeholder";
            if (actor.CurrentOrder == ActorOrderKind.Harvest)
                return actor.MovementPhase;
            return "idle";
        }

        void UpdatePowerAndActorFlags()
        {
            foreach (var player in SortedPlayers())
            {
                var generated = 0;
                var consumed = 0;
                foreach (var actor in SortedActors())
                {
                    if (actor.OwnerPlayerId != player.PlayerId || actor.ManuallyPoweredOff || actor.IsDestroyed)
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

                if (actor.IsDestroyed)
                {
                    actor.IsPowered = false;
                    actor.IsLowPower = false;
                    actor.LightsActive = false;
                    actor.MachineryActive = false;
                    actor.IsProducing = false;
                    actor.ProductionProgress = 0;
                    actor.AnimationStateId = definition.Death.DeathVisualId;
                    actor.MovementPhase = "destroyed";
                    continue;
                }

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

        void UpdateVisibility()
        {
            foreach (var pair in visibilityStates)
                pair.Value.BeginUpdate();

            foreach (var actor in SortedActors())
            {
                if (actor.IsDestroyed)
                    continue;

                PlayerVisibilityState visibility;
                if (!visibilityStates.TryGetValue(actor.OwnerPlayerId, out visibility))
                    continue;

                var definition = Rules.GetDefinition(actor.TypeId);
                RevealCircle(visibility, actor.CellPosition, definition.Sight.RadiusCells);
            }
        }

        void RevealCircle(PlayerVisibilityState visibility, Int2 center, int radius)
        {
            var radiusSquared = radius * radius;
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                for (var x = center.X - radius; x <= center.X + radius; x++)
                {
                    var cell = new Int2(x, y);
                    if (!Map.Contains(cell))
                        continue;

                    var dx = x - center.X;
                    var dy = y - center.Y;
                    if (dx * dx + dy * dy <= radiusSquared)
                        visibility.RevealCell(cell);
                }
            }
        }

        bool ShouldIncludeActorInSnapshot(ActorState actor, int perspectivePlayerId)
        {
            if (perspectivePlayerId <= 0)
                return true;
            if (actor.OwnerPlayerId == perspectivePlayerId)
                return true;

            return IsCellVisible(perspectivePlayerId, actor.CellPosition);
        }

        FogSnapshot CreateFogSnapshot(int playerId)
        {
            PlayerVisibilityState visibility;
            if (!visibilityStates.TryGetValue(playerId, out visibility))
                return FogSnapshot.Empty;

            var cells = new List<CellVisibilitySnapshot>();
            var rawCells = visibility.CopyCells();
            for (var y = 0; y < visibility.Height; y++)
                for (var x = 0; x < visibility.Width; x++)
                    cells.Add(new CellVisibilitySnapshot(new Int2(x, y), rawCells[y * visibility.Width + x]));

            return new FogSnapshot(playerId, visibility.Width, visibility.Height, cells);
        }

        RadarSnapshot CreateRadarSnapshot(int playerId)
        {
            foreach (var actor in SortedActors())
            {
                if (actor.OwnerPlayerId != playerId || actor.IsDestroyed || !actor.IsPowered || actor.IsLowPower)
                    continue;

                var definition = Rules.GetDefinition(actor.TypeId);
                if (definition.Radar.ProvidesRadar)
                    return new RadarSnapshot(playerId, true, actor.Id.Value, definition.Radar.RadiusCells);
            }

            return new RadarSnapshot(playerId, false, 0, 0);
        }

        MinimapSnapshot CreateMinimapSnapshot(int playerId)
        {
            PlayerVisibilityState visibility;
            if (!visibilityStates.TryGetValue(playerId, out visibility))
                return MinimapSnapshot.Empty;

            var actorDots = new List<MinimapActorDotSnapshot>();
            foreach (var actor in SortedActors())
            {
                var isEnemy = actor.OwnerPlayerId != playerId;
                var isVisible = visibility.IsVisible(actor.CellPosition);
                if (isEnemy && !isVisible)
                    continue;

                actorDots.Add(new MinimapActorDotSnapshot(actor.Id.Value, actor.OwnerPlayerId, actor.TypeId, actor.CellPosition, isEnemy, isVisible));
            }

            var resourceDots = new List<MinimapResourceDotSnapshot>();
            foreach (var resource in SortedResources())
            {
                if (!visibility.IsExploredOrVisible(resource.Cell))
                    continue;

                resourceDots.Add(new MinimapResourceDotSnapshot(resource.Cell, resource.Kind.ToString(), visibility.IsVisible(resource.Cell), resource.IsDepleted));
            }

            return new MinimapSnapshot(playerId, Map.Width, Map.Height, actorDots, resourceDots);
        }

        MapSnapshot CreateMapSnapshot(int perspectivePlayerId)
        {
            var terrainCells = new List<TerrainCellSnapshot>();
            for (var y = 0; y < Map.Height; y++)
                for (var x = 0; x < Map.Width; x++)
                {
                    var cell = new Int2(x, y);
                    var kind = Map.GetTerrainKind(cell);
                    var definition = Rules.GetTerrainDefinition(kind);
                    terrainCells.Add(new TerrainCellSnapshot(
                        cell,
                        kind.ToString(),
                        definition.MovementCost,
                        definition.Passability.ToString(),
                        Map.IsBlocked(cell),
                        Map.HasBuildingAt(cell)));
                }

            MapValidationResult validation;
            if (players.Count == 0)
                validation = new MapValidationResult(new string[0], new[] { "NoPlayers" });
            else
                validation = ValidateMapForPlayer(perspectivePlayerId > 0 ? perspectivePlayerId : SortedPlayers()[0].PlayerId);

            return new MapSnapshot(
                Map.Width,
                Map.Height,
                Map.PlacementGridScale,
                Map.PlacementWidth,
                Map.PlacementHeight,
                terrainCells,
                new List<PathDebugSnapshot>(recentPathQueries),
                validation.Success,
                validation.Errors,
                validation.Warnings);
        }

        CommandResult ValidatePlacement(int playerId, string typeId, Int2 topLeftCell, bool requirePendingPlacement)
        {
            ActorDefinition actorDefinition;
            if (!Rules.TryGetDefinition(typeId, out actorDefinition))
                return CommandResult.Fail("UnknownActorType", "The building type is not registered in the rules.");

            var building = actorDefinition as BuildingDefinition;
            if (building == null)
                return CommandResult.Fail("NotABuilding", "Only building definitions can be placed.");

            var footprintCells = GetPlacementFootprintCells(typeId, topLeftCell);
            foreach (var cell in footprintCells)
            {
                if (!Map.ContainsPlacementCell(cell))
                    return CommandResult.Fail("OutsideMap", "The building footprint extends outside the map.");
                if (Map.HasBuildingAtPlacementCell(cell))
                    return CommandResult.Fail("OccupiedCell", "The building footprint overlaps an occupied cell.");
                if (!Map.IsBuildablePlacementCell(cell, Rules))
                    return CommandResult.Fail("BlockedCell", "The building footprint includes a blocked cell.");
            }

            if (!IsInsideConstructionRadius(playerId, footprintCells))
                return CommandResult.Fail("OutsideConstructionRadius", "The building is outside the owned powered construction radius.");

            if (requirePendingPlacement && !HasPendingPlacement(playerId, typeId))
                return CommandResult.Fail("NoCompletedBuildingPending", "No completed production item is pending placement for this building type.");

            return CommandResult.Ok("Placement is valid.");
        }

        List<Int2> GetPlacementFootprintCells(string typeId, Int2 topLeftCell)
        {
            var result = new List<Int2>();
            ActorDefinition actorDefinition;
            if (!Rules.TryGetDefinition(typeId, out actorDefinition))
                return result;

            var building = actorDefinition as BuildingDefinition;
            if (building == null)
                return result;

            for (var y = 0; y < building.PlacementFootprintCells.Y; y++)
                for (var x = 0; x < building.PlacementFootprintCells.X; x++)
                    result.Add(new Int2(topLeftCell.X + x, topLeftCell.Y + y));

            return result;
        }

        Int2 GetPlacementFootprintSize(string typeId)
        {
            ActorDefinition actorDefinition;
            if (!Rules.TryGetDefinition(typeId, out actorDefinition))
                return Int2.Zero;

            var building = actorDefinition as BuildingDefinition;
            return building == null ? Int2.Zero : building.PlacementFootprintCells;
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
                    if (cell.ManhattanDistanceTo(provider.PlacementTopLeftCell) <= providerDefinition.ConstructionRadiusCells * PlacementGridMetrics.PlacementGridScale)
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

            return actor.OwnerPlayerId == playerId && !actor.IsDestroyed;
        }

        bool ValidateMobileActor(ActorState actor, string orderName, List<string> details)
        {
            if (actor.IsDestroyed)
            {
                details.Add("actor " + actor.Id.Value + ": destroyed");
                return false;
            }

            if (!(Rules.GetDefinition(actor.TypeId) is UnitDefinition))
            {
                details.Add("actor " + actor.Id.Value + ": " + orderName + " requires a mobile unit");
                return false;
            }

            return true;
        }

        bool ValidateArmedActor(ActorState actor, string orderName, List<string> details)
        {
            if (actor.IsDestroyed)
            {
                details.Add("actor " + actor.Id.Value + ": destroyed");
                return false;
            }

            if (Rules.GetDefinition(actor.TypeId).Weapon == null)
            {
                details.Add("actor " + actor.Id.Value + ": " + orderName + " requires an armed actor");
                return false;
            }

            return true;
        }

        bool ValidateMobileArmedActor(ActorState actor, string orderName, List<string> details)
        {
            return ValidateMobileActor(actor, orderName, details) && ValidateArmedActor(actor, orderName, details);
        }

        List<Int2> FindScatterPath(ActorState actor, out Int2 destination)
        {
            var offsets = new[]
            {
                new Int2(1, 0),
                new Int2(-1, 0),
                new Int2(0, 1),
                new Int2(0, -1),
                new Int2(1, 1),
                new Int2(-1, 1),
                new Int2(1, -1),
                new Int2(-1, -1)
            };

            var start = actor.Id.Value % offsets.Length;
            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[(start + i) % offsets.Length];
                var candidate = new Int2(actor.CellPosition.X + offset.X, actor.CellPosition.Y + offset.Y);
                if (!Map.Contains(candidate))
                    continue;

                var path = QueryPathForActor(actor, candidate);
                RecordPathQuery(actor.Id.Value, path);
                if (path.Success)
                {
                    destination = candidate;
                    return new List<Int2>(path.Path);
                }
            }

            destination = actor.CellPosition;
            return new List<Int2>();
        }

        CommandResult ValidateAttackTarget(ActorState attacker, ActorState target)
        {
            if (attacker.IsDestroyed)
                return CommandResult.Fail("AttackActorDestroyed", "The attacker has been destroyed.");
            if (target.IsDestroyed)
                return CommandResult.Fail("AttackTargetDestroyed", "The attack target has been destroyed.");
            if (attacker.OwnerPlayerId == target.OwnerPlayerId)
                return CommandResult.Fail("AttackTargetFriendly", "Stage 9 attack orders require an enemy target.");

            var attackerDefinition = Rules.GetDefinition(attacker.TypeId);
            var targetDefinition = Rules.GetDefinition(target.TypeId);
            var weapon = attackerDefinition.Weapon;
            if (weapon == null)
                return CommandResult.Fail("AttackActorUnarmed", "The attacker has no weapon definition.");

            if (targetDefinition.Kind == ActorKind.Building && !weapon.CanTargetBuildings)
                return CommandResult.Fail("AttackCannotTargetBuilding", "The weapon cannot target buildings.");
            if (targetDefinition.Kind == ActorKind.Unit && !weapon.CanTargetUnits)
                return CommandResult.Fail("AttackCannotTargetUnit", "The weapon cannot target units.");
            if (targetDefinition.Production.Kind == ProductionKind.Aircraft && !weapon.CanTargetAir)
                return CommandResult.Fail("AttackCannotTargetAir", "The weapon cannot target aircraft.");
            if (targetDefinition.Production.Kind != ProductionKind.Aircraft && !weapon.CanTargetGround)
                return CommandResult.Fail("AttackCannotTargetGround", "The weapon cannot target ground actors.");

            var distance = attacker.CellPosition.ManhattanDistanceTo(target.CellPosition);
            if (distance < weapon.MinRangeCells)
                return CommandResult.Fail("AttackTargetTooClose", "The target is inside the weapon minimum range.");
            if (distance > weapon.RangeCells)
                return CommandResult.Fail("AttackTargetOutOfRange", "The target is outside the Stage 9 weapon range.");

            return CommandResult.Ok("Attack target is valid.");
        }

        void TickCombat()
        {
            foreach (var actor in SortedActors())
            {
                if (actor.WeaponCooldownRemaining > 0)
                    actor.WeaponCooldownRemaining--;

                if (actor.IsDestroyed)
                    continue;

                if (actor.CurrentOrder == ActorOrderKind.Attack)
                {
                    if (actor.AttackTargetActorId <= 0)
                        continue;

                    ActorState target;
                    if (!actors.TryGetValue(actor.AttackTargetActorId, out target))
                    {
                        ClearAttackState(actor);
                        continue;
                    }

                    var validation = ValidateAttackTarget(actor, target);
                    if (!validation.Success)
                    {
                        ClearAttackState(actor);
                        continue;
                    }

                    actor.AttackTargetCell = target.CellPosition;
                    actor.OrderTargetCell = target.CellPosition;
                    actor.MovementPhase = "attacking";
                    if (actor.WeaponCooldownRemaining == 0)
                        FireWeapon(actor, target, Rules.GetDefinition(actor.TypeId).Weapon);
                    continue;
                }

                if (!IsAutoAttackOrder(actor.CurrentOrder))
                    continue;

                ActorState autoTarget = null;
                if (actor.AttackTargetActorId > 0)
                {
                    ActorState candidate;
                    if (actors.TryGetValue(actor.AttackTargetActorId, out candidate) &&
                        ValidateAttackTarget(actor, candidate).Success)
                    {
                        autoTarget = candidate;
                    }
                    else
                    {
                        ClearAttackState(actor);
                    }
                }

                if (autoTarget == null)
                    autoTarget = FindBestAutoAttackTarget(actor);

                if (autoTarget == null)
                {
                    actor.IsAttacking = false;
                    actor.ActiveWeaponId = string.Empty;
                    continue;
                }

                actor.AttackTargetActorId = autoTarget.Id.Value;
                actor.AttackTargetCell = autoTarget.CellPosition;
                actor.IsAttacking = true;
                actor.ActiveWeaponId = Rules.GetDefinition(actor.TypeId).Weapon.WeaponId;
                if (actor.CurrentOrder == ActorOrderKind.Guard)
                    actor.MovementPhase = "guarding";
                else if (actor.CurrentOrder == ActorOrderKind.Patrol && actor.Path.Count == 0)
                    actor.MovementPhase = "patrol_hold";
                else if (actor.CurrentOrder == ActorOrderKind.AttackMove && actor.Path.Count == 0)
                    actor.MovementPhase = "attack_move_hold";
                if (actor.WeaponCooldownRemaining == 0)
                    FireWeapon(actor, autoTarget, Rules.GetDefinition(actor.TypeId).Weapon);
            }
        }

        static bool IsAutoAttackOrder(ActorOrderKind order)
        {
            return order == ActorOrderKind.AttackMove ||
                order == ActorOrderKind.Guard ||
                order == ActorOrderKind.Patrol;
        }

        ActorState FindBestAutoAttackTarget(ActorState attacker)
        {
            if (Rules.GetDefinition(attacker.TypeId).Weapon == null)
                return null;

            ActorState best = null;
            var bestDistance = int.MaxValue;
            foreach (var target in SortedActors())
            {
                if (target.Id.Value == attacker.Id.Value || target.IsDestroyed)
                    continue;

                if (!ValidateAttackTarget(attacker, target).Success)
                    continue;

                var distance = attacker.CellPosition.ManhattanDistanceTo(target.CellPosition);
                if (best == null || distance < bestDistance || (distance == bestDistance && target.Id.Value < best.Id.Value))
                {
                    best = target;
                    bestDistance = distance;
                }
            }

            return best;
        }

        void FireWeapon(ActorState attacker, ActorState target, WeaponDefinition weapon)
        {
            attacker.WeaponCooldownRemaining = weapon.CooldownTicks;
            attacker.IsAttacking = true;
            attacker.ActiveWeaponId = weapon.WeaponId;

            if (target.WorldPositionFixed.X > attacker.WorldPositionFixed.X)
                attacker.FacingDegrees = 90;
            else if (target.WorldPositionFixed.X < attacker.WorldPositionFixed.X)
                attacker.FacingDegrees = 270;
            else if (target.WorldPositionFixed.Y > attacker.WorldPositionFixed.Y)
                attacker.FacingDegrees = 180;
            else if (target.WorldPositionFixed.Y < attacker.WorldPositionFixed.Y)
                attacker.FacingDegrees = 0;

            if (weapon.FireMode == WeaponFireMode.InstantHit)
            {
                AddCombatEvent("WeaponFired", attacker.Id.Value, target.Id.Value, 0, weapon.WeaponId, 0, target.Health, target.CellPosition, target.WorldPositionFixed);
                ApplyDamage(target, weapon.Damage, attacker.Id.Value, 0, weapon.WeaponId);
                return;
            }

            var projectile = new ProjectileState(
                nextProjectileId++,
                attacker.OwnerPlayerId,
                attacker.Id.Value,
                target.Id.Value,
                weapon.WeaponId,
                weapon.ProjectileKind,
                attacker.WorldPositionFixed,
                target.WorldPositionFixed,
                target.CellPosition,
                weapon.ProjectileSpeedSubCellsPerTick,
                weapon.Damage,
                TickNumber,
                weapon.ProjectileDefinition.LifetimeTicks);

            projectiles.Add(projectile.ProjectileId, projectile);
            AddCombatEvent("WeaponFired", attacker.Id.Value, target.Id.Value, projectile.ProjectileId, weapon.WeaponId, 0, target.Health, target.CellPosition, target.WorldPositionFixed);
        }

        void TickProjectiles()
        {
            var impacted = new List<int>();
            foreach (var projectile in SortedProjectiles())
            {
                if (projectile.CreatedTick == TickNumber)
                    continue;

                projectile.RemainingLifetimeTicks--;
                var dx = projectile.TargetPositionFixed.X - projectile.CurrentPositionFixed.X;
                var dy = projectile.TargetPositionFixed.Y - projectile.CurrentPositionFixed.Y;
                var next = new Int2(
                    projectile.CurrentPositionFixed.X + FixedMath.ClampStep(dx, projectile.SpeedSubCellsPerTick),
                    projectile.CurrentPositionFixed.Y + FixedMath.ClampStep(dy, projectile.SpeedSubCellsPerTick));
                projectile.CurrentPositionFixed = next;

                if (next.Equals(projectile.TargetPositionFixed) || projectile.RemainingLifetimeTicks <= 0)
                {
                    projectile.HasImpacted = true;
                    projectile.ImpactTick = TickNumber;
                    ActorState target;
                    if (actors.TryGetValue(projectile.TargetActorId, out target) && !target.IsDestroyed)
                        ApplyDamage(target, projectile.Damage, projectile.SourceActorId, projectile.ProjectileId, projectile.WeaponId);
                    else
                        AddCombatEvent("ProjectileExpired", projectile.SourceActorId, projectile.TargetActorId, projectile.ProjectileId, projectile.WeaponId, 0, 0, projectile.TargetCell, projectile.TargetPositionFixed);

                    impacted.Add(projectile.ProjectileId);
                }
            }

            foreach (var projectileId in impacted)
                projectiles.Remove(projectileId);
        }

        void ApplyDamage(ActorState target, int damage, int sourceActorId, int projectileId, string weaponId)
        {
            if (target.IsDestroyed)
                return;

            target.Health -= damage;
            if (target.Health < 0)
                target.Health = 0;
            target.LastDamageTick = TickNumber;
            AddCombatEvent("DamageApplied", sourceActorId, target.Id.Value, projectileId, weaponId, damage, target.Health, target.CellPosition, target.WorldPositionFixed);

            if (target.Health > 0)
                return;

            target.IsDying = true;
            target.IsDestroyed = true;
            target.DeathTick = TickNumber;
            target.DestroyedByActorId = sourceActorId;
            target.Path.Clear();
            ClearAttackState(target);
            ClearHarvestState(target);
            target.CurrentOrder = ActorOrderKind.Stop;
            target.DesiredSpeed = 0;
            target.NormalizedSpeed = 0;
            target.MovementPhase = "destroyed";
            AddCombatEvent("ActorDestroyed", sourceActorId, target.Id.Value, projectileId, weaponId, damage, target.Health, target.CellPosition, target.WorldPositionFixed);
        }

        void ClearAttackState(ActorState actor)
        {
            actor.AttackTargetActorId = 0;
            actor.AttackTargetCell = actor.CellPosition;
            actor.IsAttacking = false;
            actor.ActiveWeaponId = string.Empty;
            if (actor.CurrentOrder == ActorOrderKind.Attack)
                actor.CurrentOrder = ActorOrderKind.Idle;
        }

        void TickHarvesting()
        {
            foreach (var refinery in SortedRefineries())
            {
                refinery.IsUnloading = false;
                if (refinery.ActiveHarvesterActorId > 0)
                {
                    HarvesterState active;
                    if (!harvesters.TryGetValue(refinery.ActiveHarvesterActorId, out active) || active.State != HarvesterWorkState.Unloading)
                        refinery.ActiveHarvesterActorId = 0;
                }
            }

            foreach (var harvester in SortedHarvesters())
            {
                ActorState actor;
                if (!actors.TryGetValue(harvester.ActorId, out actor) || actor.IsDestroyed)
                    continue;

                if (!actor.HasHarvestOrder && harvester.State != HarvesterWorkState.Unloading)
                    continue;

                if (harvester.State == HarvesterWorkState.MovingToResource || harvester.State == HarvesterWorkState.Returning)
                {
                    if (actor.CellPosition.Equals(harvester.HarvestTargetCell))
                    {
                        harvester.State = HarvesterWorkState.Harvesting;
                        actor.MovementPhase = "harvesting";
                    }
                }

                if (harvester.State == HarvesterWorkState.Harvesting)
                {
                    ResourceCellState resource;
                    if (!resourceCells.TryGetValue(harvester.HarvestTargetCell, out resource) || resource.IsDepleted)
                    {
                        if (harvester.CargoAmount > 0)
                        {
                            var refinery = GetAssignedOrNearestRefinery(actor.OwnerPlayerId, actor, harvester);
                            if (refinery != null)
                                SendHarvesterToRefinery(actor, harvester, refinery);
                            else
                                harvester.State = HarvesterWorkState.Blocked;
                        }
                        else
                        {
                            ClearHarvestState(actor);
                        }

                        continue;
                    }

                    var capacityRemaining = harvester.CargoCapacity - harvester.CargoAmount;
                    if (capacityRemaining <= 0)
                    {
                        var refinery = GetAssignedOrNearestRefinery(actor.OwnerPlayerId, actor, harvester);
                        if (refinery != null)
                            SendHarvesterToRefinery(actor, harvester, refinery);
                        else
                            harvester.State = HarvesterWorkState.Blocked;
                        continue;
                    }

                    var harvestAmount = Min(10, Min(capacityRemaining, resource.Amount));
                    resource.Amount -= harvestAmount;
                    harvester.CargoAmount += harvestAmount;
                    harvester.CarriedResourceKind = resource.Kind;
                    harvester.HarvestProgressTicks++;
                    actor.MovementPhase = "harvesting";
                    AddEconomyEvent("ResourceHarvested", actor.Id.Value, harvester.AssignedRefineryActorId, resource.Cell, harvestAmount, 0);

                    if (harvester.CargoAmount >= harvester.CargoCapacity || resource.IsDepleted)
                    {
                        var refinery = GetAssignedOrNearestRefinery(actor.OwnerPlayerId, actor, harvester);
                        if (refinery != null && harvester.CargoAmount > 0)
                            SendHarvesterToRefinery(actor, harvester, refinery);
                        else if (resource.IsDepleted && harvester.CargoAmount == 0)
                            ClearHarvestState(actor);
                    }
                }

                if (harvester.State == HarvesterWorkState.MovingToRefinery || harvester.State == HarvesterWorkState.WaitingForDock)
                {
                    RefineryState refinery;
                    if (!refineries.TryGetValue(harvester.AssignedRefineryActorId, out refinery))
                    {
                        harvester.State = HarvesterWorkState.Blocked;
                        continue;
                    }

                    if (actor.CellPosition.Equals(refinery.DockCell))
                    {
                        if (refinery.ActiveHarvesterActorId == 0 || refinery.ActiveHarvesterActorId == actor.Id.Value)
                        {
                            refinery.ActiveHarvesterActorId = actor.Id.Value;
                            harvester.State = HarvesterWorkState.Unloading;
                            harvester.UnloadProgressTicks = 0;
                            actor.MovementPhase = "unloading";
                        }
                        else
                        {
                            harvester.State = HarvesterWorkState.WaitingForDock;
                            actor.MovementPhase = "waiting_for_dock";
                        }
                    }
                }

                if (harvester.State == HarvesterWorkState.Unloading)
                {
                    RefineryState refinery;
                    if (!refineries.TryGetValue(harvester.AssignedRefineryActorId, out refinery))
                    {
                        harvester.State = HarvesterWorkState.Blocked;
                        continue;
                    }

                    if (refinery.ActiveHarvesterActorId != 0 && refinery.ActiveHarvesterActorId != actor.Id.Value)
                    {
                        harvester.State = HarvesterWorkState.WaitingForDock;
                        continue;
                    }

                    refinery.ActiveHarvesterActorId = actor.Id.Value;
                    refinery.IsUnloading = true;
                    actor.MovementPhase = "unloading";

                    var unloadAmount = Min(refinery.UnloadRatePerTick, harvester.CargoAmount);
                    if (unloadAmount > 0)
                    {
                        harvester.CargoAmount -= unloadAmount;
                        harvester.UnloadProgressTicks++;
                        refinery.TotalResourcesReceived += unloadAmount;
                        players[actor.OwnerPlayerId].Credits += unloadAmount;
                        AddEconomyEvent("HarvesterUnloaded", actor.Id.Value, refinery.ActorId, refinery.DockCell, unloadAmount, unloadAmount);
                    }

                    if (harvester.CargoAmount <= 0)
                    {
                        harvester.CargoAmount = 0;
                        harvester.CarriedResourceKind = ResourceKind.None;
                        refinery.ActiveHarvesterActorId = 0;
                        refinery.IsUnloading = false;

                        ResourceCellState resource;
                        if (actor.HasHarvestOrder && resourceCells.TryGetValue(harvester.HarvestTargetCell, out resource) && !resource.IsDepleted)
                            SendHarvesterToResource(actor, harvester, harvester.HarvestTargetCell);
                        else
                            ClearHarvestState(actor);
                    }
                }
            }
        }

        void StartHarvestOrder(ActorState actor, Int2 resourceCell)
        {
            var harvester = harvesters[actor.Id.Value];
            var refinery = GetAssignedOrNearestRefinery(actor.OwnerPlayerId, actor, harvester);
            harvester.AssignedRefineryActorId = refinery == null ? 0 : refinery.ActorId;
            harvester.HarvestTargetCell = resourceCell;
            actor.HasHarvestOrder = true;
            actor.CurrentOrder = ActorOrderKind.Harvest;
            actor.OrderTargetCell = resourceCell;
            ClearAttackState(actor);
            SendHarvesterToResource(actor, harvester, resourceCell);
        }

        void SendHarvesterToResource(ActorState actor, HarvesterState harvester, Int2 resourceCell)
        {
            actor.Path.Clear();
            if (!actor.CellPosition.Equals(resourceCell))
            {
                var path = QueryPathForActor(actor, resourceCell);
                RecordPathQuery(actor.Id.Value, path);
                if (!path.Success)
                {
                    harvester.State = HarvesterWorkState.Blocked;
                    actor.MovementPhase = "blocked";
                    return;
                }

                actor.Path.AddRange(path.Path);
            }
            actor.CurrentOrder = ActorOrderKind.Harvest;
            actor.OrderTargetCell = resourceCell;
            actor.HasHarvestOrder = true;
            harvester.HarvestTargetCell = resourceCell;
            harvester.State = actor.CellPosition.Equals(resourceCell) ? HarvesterWorkState.Harvesting : HarvesterWorkState.MovingToResource;
            actor.MovementPhase = actor.CellPosition.Equals(resourceCell) ? "harvesting" : "moving_to_resource";
        }

        void SendHarvesterToRefinery(ActorState actor, HarvesterState harvester, RefineryState refinery)
        {
            actor.Path.Clear();
            if (!actor.CellPosition.Equals(refinery.DockCell))
            {
                var path = QueryPathForActor(actor, refinery.DockCell);
                RecordPathQuery(actor.Id.Value, path);
                if (!path.Success)
                {
                    harvester.State = HarvesterWorkState.Blocked;
                    actor.MovementPhase = "blocked";
                    return;
                }

                actor.Path.AddRange(path.Path);
            }
            actor.CurrentOrder = ActorOrderKind.Harvest;
            actor.OrderTargetCell = refinery.DockCell;
            actor.HasHarvestOrder = true;
            harvester.AssignedRefineryActorId = refinery.ActorId;
            harvester.State = actor.CellPosition.Equals(refinery.DockCell) ? HarvesterWorkState.Unloading : HarvesterWorkState.MovingToRefinery;
            actor.MovementPhase = actor.CellPosition.Equals(refinery.DockCell) ? "unloading" : "moving_to_refinery";
        }

        RefineryState GetAssignedOrNearestRefinery(int playerId, ActorState actor, HarvesterState harvester)
        {
            RefineryState assigned;
            ActorState assignedActor;
            if (harvester.AssignedRefineryActorId > 0 &&
                refineries.TryGetValue(harvester.AssignedRefineryActorId, out assigned) &&
                actors.TryGetValue(assigned.ActorId, out assignedActor) &&
                assignedActor.OwnerPlayerId == playerId &&
                !assignedActor.IsDestroyed)
                return assigned;

            return FindNearestOwnedRefinery(playerId, actor.CellPosition);
        }

        RefineryState FindNearestOwnedRefinery(int playerId, Int2 fromCell)
        {
            RefineryState best = null;
            var bestDistance = int.MaxValue;
            foreach (var refinery in SortedRefineries())
            {
                ActorState actor;
                if (!actors.TryGetValue(refinery.ActorId, out actor) || actor.OwnerPlayerId != playerId || actor.IsDestroyed)
                    continue;

                var distance = fromCell.ManhattanDistanceTo(refinery.DockCell);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = refinery;
                }
            }

            return best;
        }

        Int2 GetRefineryDockCell(ActorState actor, BuildingDefinition definition)
        {
            var preferred = actor.CellPosition + definition.UnitExitOffset;
            return FindNearestSpawnCell(preferred, MovementClass.Harvester);
        }

        void ClearHarvestState(ActorState actor)
        {
            actor.HasHarvestOrder = false;
            HarvesterState harvester;
            if (harvesters.TryGetValue(actor.Id.Value, out harvester))
            {
                harvester.State = HarvesterWorkState.Idle;
                harvester.HarvestProgressTicks = 0;
                harvester.UnloadProgressTicks = 0;
            }

            if (actor.CurrentOrder == ActorOrderKind.Harvest)
                actor.CurrentOrder = ActorOrderKind.Idle;
        }

        void AddCombatEvent(string eventType, int sourceActorId, int targetActorId, int projectileId, string weaponId, int damage, int targetHealth, Int2 cell, Int2 fixedWorldPosition)
        {
            recentCombatEvents.Add(new CombatEventSnapshot(nextCombatEventId++, TickNumber, eventType, sourceActorId, targetActorId, projectileId, weaponId, damage, targetHealth, cell, fixedWorldPosition));
            while (recentCombatEvents.Count > MaxRecentCombatEvents)
                recentCombatEvents.RemoveAt(0);
        }

        void AddEconomyEvent(string eventType, int harvesterActorId, int refineryActorId, Int2 cell, int amount, int creditsAwarded)
        {
            recentEconomyEvents.Add(new EconomyEventSnapshot(nextEconomyEventId++, TickNumber, eventType, harvesterActorId, refineryActorId, cell, amount, creditsAwarded));
            while (recentEconomyEvents.Count > MaxRecentEconomyEvents)
                recentEconomyEvents.RemoveAt(0);
        }

        void RecordPathQuery(int actorId, PathQueryResult result)
        {
            recentPathQueries.Add(new PathDebugSnapshot(
                nextPathQueryId++,
                TickNumber,
                actorId,
                result.StartCell,
                result.GoalCell,
                result.MovementClass.ToString(),
                result.Success,
                result.TotalCost,
                result.VisitedCellCount,
                result.FailureCode,
                result.Path));

            while (recentPathQueries.Count > MaxRecentPathQueries)
                recentPathQueries.RemoveAt(0);
        }

        static int Min(int a, int b)
        {
            return a < b ? a : b;
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

        List<ProjectileState> SortedProjectiles()
        {
            var list = new List<ProjectileState>(projectiles.Values);
            list.Sort((a, b) => a.ProjectileId.CompareTo(b.ProjectileId));
            return list;
        }

        List<ResourceCellState> SortedResources()
        {
            var list = new List<ResourceCellState>(resourceCells.Values);
            list.Sort((a, b) =>
            {
                var y = a.Cell.Y.CompareTo(b.Cell.Y);
                return y != 0 ? y : a.Cell.X.CompareTo(b.Cell.X);
            });
            return list;
        }

        List<HarvesterState> SortedHarvesters()
        {
            var list = new List<HarvesterState>(harvesters.Values);
            list.Sort((a, b) => a.ActorId.CompareTo(b.ActorId));
            return list;
        }

        List<RefineryState> SortedRefineries()
        {
            var list = new List<RefineryState>(refineries.Values);
            list.Sort((a, b) => a.ActorId.CompareTo(b.ActorId));
            return list;
        }
    }
}
