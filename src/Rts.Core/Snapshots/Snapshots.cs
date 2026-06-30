using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Match;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Visibility;

namespace ProjectAegisRTS.Snapshots
{
    public sealed class WorldSnapshot
    {
        public int Tick { get; private set; }
        public IReadOnlyList<PlayerSnapshot> Players { get; private set; }
        public IReadOnlyList<ActorSnapshot> Actors { get; private set; }
        public IReadOnlyList<ProjectileSnapshot> Projectiles { get; private set; }
        public IReadOnlyList<CombatEventSnapshot> CombatEvents { get; private set; }
        public EconomySnapshot Economy { get; private set; }
        public FogSnapshot Fog { get; private set; }
        public RadarSnapshot Radar { get; private set; }
        public MinimapSnapshot Minimap { get; private set; }
        public AiSnapshot Ai { get; private set; }
        public MapSnapshot Map { get; private set; }
        public MatchSnapshot Match { get; private set; }
        public ScenarioSnapshot Scenario { get; private set; }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors)
            : this(tick, players, actors, new ProjectileSnapshot[0], new CombatEventSnapshot[0], EconomySnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents)
            : this(tick, players, actors, projectiles, combatEvents, EconomySnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents, EconomySnapshot economy)
            : this(tick, players, actors, projectiles, combatEvents, economy, FogSnapshot.Empty, RadarSnapshot.Empty, MinimapSnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents, EconomySnapshot economy, FogSnapshot fog, RadarSnapshot radar, MinimapSnapshot minimap)
            : this(tick, players, actors, projectiles, combatEvents, economy, fog, radar, minimap, AiSnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents, EconomySnapshot economy, FogSnapshot fog, RadarSnapshot radar, MinimapSnapshot minimap, AiSnapshot ai)
            : this(tick, players, actors, projectiles, combatEvents, economy, fog, radar, minimap, ai, MapSnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents, EconomySnapshot economy, FogSnapshot fog, RadarSnapshot radar, MinimapSnapshot minimap, AiSnapshot ai, MapSnapshot map)
            : this(tick, players, actors, projectiles, combatEvents, economy, fog, radar, minimap, ai, map, MatchSnapshot.Empty, ScenarioSnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents, EconomySnapshot economy, FogSnapshot fog, RadarSnapshot radar, MinimapSnapshot minimap, AiSnapshot ai, MapSnapshot map, MatchSnapshot match, ScenarioSnapshot scenario)
        {
            Tick = tick;
            Players = players;
            Actors = actors;
            Projectiles = projectiles;
            CombatEvents = combatEvents;
            Economy = economy ?? EconomySnapshot.Empty;
            Fog = fog ?? FogSnapshot.Empty;
            Radar = radar ?? RadarSnapshot.Empty;
            Minimap = minimap ?? MinimapSnapshot.Empty;
            Ai = ai ?? AiSnapshot.Empty;
            Map = map ?? MapSnapshot.Empty;
            Match = match ?? MatchSnapshot.Empty;
            Scenario = scenario ?? ScenarioSnapshot.Empty;
        }
    }

    public sealed class MapSnapshot
    {
        public static readonly MapSnapshot Empty = new MapSnapshot(0, 0, new TerrainCellSnapshot[0], new PathDebugSnapshot[0], true, new string[0], new string[0]);

        public int Width { get; private set; }
        public int Height { get; private set; }
        public IReadOnlyList<TerrainCellSnapshot> TerrainCells { get; private set; }
        public IReadOnlyList<PathDebugSnapshot> RecentPathQueries { get; private set; }
        public bool IsValid { get; private set; }
        public IReadOnlyList<string> ValidationErrors { get; private set; }
        public IReadOnlyList<string> ValidationWarnings { get; private set; }

        public MapSnapshot(int width, int height, IReadOnlyList<TerrainCellSnapshot> terrainCells, IReadOnlyList<PathDebugSnapshot> recentPathQueries, bool isValid, IReadOnlyList<string> validationErrors, IReadOnlyList<string> validationWarnings)
        {
            Width = width;
            Height = height;
            TerrainCells = terrainCells ?? new TerrainCellSnapshot[0];
            RecentPathQueries = recentPathQueries ?? new PathDebugSnapshot[0];
            IsValid = isValid;
            ValidationErrors = validationErrors ?? new string[0];
            ValidationWarnings = validationWarnings ?? new string[0];
        }
    }

    public sealed class TerrainCellSnapshot
    {
        public Int2 Cell { get; private set; }
        public string Kind { get; private set; }
        public int MovementCost { get; private set; }
        public string Passability { get; private set; }
        public bool IsBlocked { get; private set; }
        public bool HasBuilding { get; private set; }

        public TerrainCellSnapshot(Int2 cell, string kind, int movementCost, string passability, bool isBlocked, bool hasBuilding)
        {
            Cell = cell;
            Kind = kind;
            MovementCost = movementCost;
            Passability = passability;
            IsBlocked = isBlocked;
            HasBuilding = hasBuilding;
        }
    }

    public sealed class PathDebugSnapshot
    {
        public int QueryId { get; private set; }
        public int Tick { get; private set; }
        public int ActorId { get; private set; }
        public Int2 StartCell { get; private set; }
        public Int2 GoalCell { get; private set; }
        public string MovementClass { get; private set; }
        public bool Success { get; private set; }
        public int TotalCost { get; private set; }
        public int VisitedCellCount { get; private set; }
        public string FailureCode { get; private set; }
        public IReadOnlyList<Int2> Path { get; private set; }

        public PathDebugSnapshot(int queryId, int tick, int actorId, Int2 startCell, Int2 goalCell, string movementClass, bool success, int totalCost, int visitedCellCount, string failureCode, IReadOnlyList<Int2> path)
        {
            QueryId = queryId;
            Tick = tick;
            ActorId = actorId;
            StartCell = startCell;
            GoalCell = goalCell;
            MovementClass = movementClass;
            Success = success;
            TotalCost = totalCost;
            VisitedCellCount = visitedCellCount;
            FailureCode = failureCode ?? string.Empty;
            Path = path ?? new Int2[0];
        }
    }

    public sealed class AiSnapshot
    {
        public static readonly AiSnapshot Empty = new AiSnapshot(new AiPlayerSnapshot[0]);

        public IReadOnlyList<AiPlayerSnapshot> Players { get; private set; }

        public AiSnapshot(IReadOnlyList<AiPlayerSnapshot> players)
        {
            Players = players;
        }
    }

    public sealed class AiPlayerSnapshot
    {
        public int PlayerId { get; private set; }
        public bool Enabled { get; private set; }
        public string DifficultyId { get; private set; }
        public int DecisionSequence { get; private set; }
        public int NextDecisionTick { get; private set; }
        public int ConsecutiveInvalidCommands { get; private set; }
        public string CurrentPlan { get; private set; }
        public IReadOnlyList<AiIntentSnapshot> RecentIntents { get; private set; }

        public AiPlayerSnapshot(int playerId, bool enabled, string difficultyId, int decisionSequence, int nextDecisionTick, int consecutiveInvalidCommands, string currentPlan, IReadOnlyList<AiIntentSnapshot> recentIntents)
        {
            PlayerId = playerId;
            Enabled = enabled;
            DifficultyId = difficultyId;
            DecisionSequence = decisionSequence;
            NextDecisionTick = nextDecisionTick;
            ConsecutiveInvalidCommands = consecutiveInvalidCommands;
            CurrentPlan = currentPlan;
            RecentIntents = recentIntents;
        }
    }

    public sealed class AiIntentSnapshot
    {
        public int SequenceId { get; private set; }
        public int Tick { get; private set; }
        public string Kind { get; private set; }
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

        public AiIntentSnapshot(int sequenceId, int tick, string kind, string intentId, string commandType, string targetTypeId, int sourceActorId, int targetActorId, Int2 targetCell, bool wasCommandIssued, bool commandSucceeded, string resultCode, string status)
        {
            SequenceId = sequenceId;
            Tick = tick;
            Kind = kind;
            IntentId = intentId;
            CommandType = commandType;
            TargetTypeId = targetTypeId;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            TargetCell = targetCell;
            WasCommandIssued = wasCommandIssued;
            CommandSucceeded = commandSucceeded;
            ResultCode = resultCode;
            Status = status;
        }
    }

    public sealed class PlayerSnapshot
    {
        public int PlayerId { get; private set; }
        public string Name { get; private set; }
        public int Credits { get; private set; }
        public PowerSnapshot Power { get; private set; }
        public IReadOnlyList<ProductionSnapshot> Production { get; private set; }

        public PlayerSnapshot(int playerId, string name, int credits, PowerSnapshot power, IReadOnlyList<ProductionSnapshot> production)
        {
            PlayerId = playerId;
            Name = name;
            Credits = credits;
            Power = power;
            Production = production;
        }
    }

    public sealed class ActorSnapshot
    {
        public int ActorId { get; private set; }
        public string TypeId { get; private set; }
        public int OwnerId { get; private set; }
        public Int2 CellPosition { get; private set; }
        public Int2 FixedWorldPosition { get; private set; }
        public int FacingDegrees { get; private set; }
        public int Health { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsPowered { get; private set; }
        public bool IsLowPower { get; private set; }
        public bool LightsActive { get; private set; }
        public bool MachineryActive { get; private set; }
        public bool IsProducing { get; private set; }
        public int ProductionProgress { get; private set; }
        public string AnimationStateId { get; private set; }
        public string VisualMotionProfileId { get; private set; }
        public int DesiredSpeed { get; private set; }
        public int NormalizedSpeed { get; private set; }
        public int TurnRateDegrees { get; private set; }
        public string MovementPhase { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsAlive { get; private set; }
        public bool IsDying { get; private set; }
        public bool IsDestroyed { get; private set; }
        public int LastDamageTick { get; private set; }
        public int DeathTick { get; private set; }
        public int DestroyedByActorId { get; private set; }
        public string ActiveWeaponId { get; private set; }
        public int WeaponCooldownRemaining { get; private set; }
        public bool IsAttacking { get; private set; }
        public int AttackTargetActorId { get; private set; }
        public Int2 AttackTargetCell { get; private set; }
        public bool HasHarvestOrder { get; private set; }

        public ActorSnapshot(
            int actorId,
            string typeId,
            int ownerId,
            Int2 cellPosition,
            Int2 fixedWorldPosition,
            int facingDegrees,
            int health,
            bool isSelected,
            bool isPowered,
            bool isLowPower,
            bool lightsActive,
            bool machineryActive,
            bool isProducing,
            int productionProgress,
            string animationStateId,
            string visualMotionProfileId,
            int desiredSpeed,
            int normalizedSpeed,
            int turnRateDegrees,
            string movementPhase)
            : this(
                actorId,
                typeId,
                ownerId,
                cellPosition,
                fixedWorldPosition,
                facingDegrees,
                health,
                health,
                isSelected,
                isPowered,
                isLowPower,
                lightsActive,
                machineryActive,
                isProducing,
                productionProgress,
                animationStateId,
                visualMotionProfileId,
                desiredSpeed,
                normalizedSpeed,
                turnRateDegrees,
                movementPhase,
                health > 0,
                false,
                false,
                -1,
                -1,
                0,
                string.Empty,
                0,
                false,
                0,
                cellPosition,
                false)
        {
        }

        public ActorSnapshot(
            int actorId,
            string typeId,
            int ownerId,
            Int2 cellPosition,
            Int2 fixedWorldPosition,
            int facingDegrees,
            int health,
            int maxHealth,
            bool isSelected,
            bool isPowered,
            bool isLowPower,
            bool lightsActive,
            bool machineryActive,
            bool isProducing,
            int productionProgress,
            string animationStateId,
            string visualMotionProfileId,
            int desiredSpeed,
            int normalizedSpeed,
            int turnRateDegrees,
            string movementPhase,
            bool isAlive,
            bool isDying,
            bool isDestroyed,
            int lastDamageTick,
            int deathTick,
            int destroyedByActorId,
            string activeWeaponId,
            int weaponCooldownRemaining,
            bool isAttacking,
            int attackTargetActorId,
            Int2 attackTargetCell,
            bool hasHarvestOrder = false)
        {
            ActorId = actorId;
            TypeId = typeId;
            OwnerId = ownerId;
            CellPosition = cellPosition;
            FixedWorldPosition = fixedWorldPosition;
            FacingDegrees = facingDegrees;
            Health = health;
            IsSelected = isSelected;
            IsPowered = isPowered;
            IsLowPower = isLowPower;
            LightsActive = lightsActive;
            MachineryActive = machineryActive;
            IsProducing = isProducing;
            ProductionProgress = productionProgress;
            AnimationStateId = animationStateId;
            VisualMotionProfileId = visualMotionProfileId;
            DesiredSpeed = desiredSpeed;
            NormalizedSpeed = normalizedSpeed;
            TurnRateDegrees = turnRateDegrees;
            MovementPhase = movementPhase;
            MaxHealth = maxHealth;
            IsAlive = isAlive;
            IsDying = isDying;
            IsDestroyed = isDestroyed;
            LastDamageTick = lastDamageTick;
            DeathTick = deathTick;
            DestroyedByActorId = destroyedByActorId;
            ActiveWeaponId = activeWeaponId;
            WeaponCooldownRemaining = weaponCooldownRemaining;
            IsAttacking = isAttacking;
            AttackTargetActorId = attackTargetActorId;
            AttackTargetCell = attackTargetCell;
            HasHarvestOrder = hasHarvestOrder;
        }
    }

    public sealed class FogSnapshot
    {
        public static readonly FogSnapshot Empty = new FogSnapshot(0, 0, 0, new CellVisibilitySnapshot[0]);

        public int PlayerId { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IReadOnlyList<CellVisibilitySnapshot> Cells { get; private set; }

        public FogSnapshot(int playerId, int width, int height, IReadOnlyList<CellVisibilitySnapshot> cells)
        {
            PlayerId = playerId;
            Width = width;
            Height = height;
            Cells = cells;
        }
    }

    public sealed class CellVisibilitySnapshot
    {
        public Int2 Cell { get; private set; }
        public CellVisibility Visibility { get; private set; }

        public CellVisibilitySnapshot(Int2 cell, CellVisibility visibility)
        {
            Cell = cell;
            Visibility = visibility;
        }
    }

    public sealed class RadarSnapshot
    {
        public static readonly RadarSnapshot Empty = new RadarSnapshot(0, false, 0, 0);

        public int PlayerId { get; private set; }
        public bool IsActive { get; private set; }
        public int ProviderActorId { get; private set; }
        public int RadiusCells { get; private set; }

        public RadarSnapshot(int playerId, bool isActive, int providerActorId, int radiusCells)
        {
            PlayerId = playerId;
            IsActive = isActive;
            ProviderActorId = providerActorId;
            RadiusCells = radiusCells;
        }
    }

    public sealed class MinimapSnapshot
    {
        public static readonly MinimapSnapshot Empty = new MinimapSnapshot(0, 0, 0, new MinimapActorDotSnapshot[0], new MinimapResourceDotSnapshot[0]);

        public int PlayerId { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IReadOnlyList<MinimapActorDotSnapshot> ActorDots { get; private set; }
        public IReadOnlyList<MinimapResourceDotSnapshot> ResourceDots { get; private set; }

        public MinimapSnapshot(int playerId, int width, int height, IReadOnlyList<MinimapActorDotSnapshot> actorDots, IReadOnlyList<MinimapResourceDotSnapshot> resourceDots)
        {
            PlayerId = playerId;
            Width = width;
            Height = height;
            ActorDots = actorDots;
            ResourceDots = resourceDots;
        }
    }

    public sealed class MinimapActorDotSnapshot
    {
        public int ActorId { get; private set; }
        public int OwnerId { get; private set; }
        public string TypeId { get; private set; }
        public Int2 Cell { get; private set; }
        public bool IsEnemy { get; private set; }
        public bool IsVisible { get; private set; }

        public MinimapActorDotSnapshot(int actorId, int ownerId, string typeId, Int2 cell, bool isEnemy, bool isVisible)
        {
            ActorId = actorId;
            OwnerId = ownerId;
            TypeId = typeId;
            Cell = cell;
            IsEnemy = isEnemy;
            IsVisible = isVisible;
        }
    }

    public sealed class MinimapResourceDotSnapshot
    {
        public Int2 Cell { get; private set; }
        public string Kind { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsDepleted { get; private set; }

        public MinimapResourceDotSnapshot(Int2 cell, string kind, bool isVisible, bool isDepleted)
        {
            Cell = cell;
            Kind = kind;
            IsVisible = isVisible;
            IsDepleted = isDepleted;
        }
    }

    public sealed class EconomySnapshot
    {
        public static readonly EconomySnapshot Empty = new EconomySnapshot(new ResourceSnapshot[0], new HarvesterSnapshot[0], new RefinerySnapshot[0], new EconomyEventSnapshot[0]);

        public IReadOnlyList<ResourceSnapshot> Resources { get; private set; }
        public IReadOnlyList<HarvesterSnapshot> Harvesters { get; private set; }
        public IReadOnlyList<RefinerySnapshot> Refineries { get; private set; }
        public IReadOnlyList<EconomyEventSnapshot> Events { get; private set; }

        public EconomySnapshot(IReadOnlyList<ResourceSnapshot> resources, IReadOnlyList<HarvesterSnapshot> harvesters, IReadOnlyList<RefinerySnapshot> refineries, IReadOnlyList<EconomyEventSnapshot> events)
        {
            Resources = resources;
            Harvesters = harvesters;
            Refineries = refineries;
            Events = events;
        }
    }

    public sealed class ResourceSnapshot
    {
        public Int2 Cell { get; private set; }
        public string Kind { get; private set; }
        public int Amount { get; private set; }
        public int MaxAmount { get; private set; }
        public bool IsDepleted { get; private set; }

        public ResourceSnapshot(Int2 cell, string kind, int amount, int maxAmount, bool isDepleted)
        {
            Cell = cell;
            Kind = kind;
            Amount = amount;
            MaxAmount = maxAmount;
            IsDepleted = isDepleted;
        }
    }

    public sealed class HarvesterSnapshot
    {
        public int ActorId { get; private set; }
        public int CargoAmount { get; private set; }
        public int CargoCapacity { get; private set; }
        public string CarriedResourceKind { get; private set; }
        public Int2 HarvestTargetCell { get; private set; }
        public int AssignedRefineryActorId { get; private set; }
        public string State { get; private set; }
        public int HarvestProgressTicks { get; private set; }
        public int UnloadProgressTicks { get; private set; }

        public HarvesterSnapshot(int actorId, int cargoAmount, int cargoCapacity, string carriedResourceKind, Int2 harvestTargetCell, int assignedRefineryActorId, string state, int harvestProgressTicks, int unloadProgressTicks)
        {
            ActorId = actorId;
            CargoAmount = cargoAmount;
            CargoCapacity = cargoCapacity;
            CarriedResourceKind = carriedResourceKind;
            HarvestTargetCell = harvestTargetCell;
            AssignedRefineryActorId = assignedRefineryActorId;
            State = state;
            HarvestProgressTicks = harvestProgressTicks;
            UnloadProgressTicks = unloadProgressTicks;
        }
    }

    public sealed class RefinerySnapshot
    {
        public int ActorId { get; private set; }
        public Int2 DockCell { get; private set; }
        public int ActiveHarvesterActorId { get; private set; }
        public int UnloadRatePerTick { get; private set; }
        public bool IsUnloading { get; private set; }
        public int TotalResourcesReceived { get; private set; }

        public RefinerySnapshot(int actorId, Int2 dockCell, int activeHarvesterActorId, int unloadRatePerTick, bool isUnloading, int totalResourcesReceived)
        {
            ActorId = actorId;
            DockCell = dockCell;
            ActiveHarvesterActorId = activeHarvesterActorId;
            UnloadRatePerTick = unloadRatePerTick;
            IsUnloading = isUnloading;
            TotalResourcesReceived = totalResourcesReceived;
        }
    }

    public sealed class EconomyEventSnapshot
    {
        public int EventId { get; private set; }
        public int Tick { get; private set; }
        public string EventType { get; private set; }
        public int HarvesterActorId { get; private set; }
        public int RefineryActorId { get; private set; }
        public Int2 Cell { get; private set; }
        public int Amount { get; private set; }
        public int CreditsAwarded { get; private set; }

        public EconomyEventSnapshot(int eventId, int tick, string eventType, int harvesterActorId, int refineryActorId, Int2 cell, int amount, int creditsAwarded)
        {
            EventId = eventId;
            Tick = tick;
            EventType = eventType;
            HarvesterActorId = harvesterActorId;
            RefineryActorId = refineryActorId;
            Cell = cell;
            Amount = amount;
            CreditsAwarded = creditsAwarded;
        }
    }

    public sealed class ProjectileSnapshot
    {
        public int ProjectileId { get; private set; }
        public int OwnerPlayerId { get; private set; }
        public int SourceActorId { get; private set; }
        public int TargetActorId { get; private set; }
        public string WeaponId { get; private set; }
        public string ProjectileKind { get; private set; }
        public Int2 CurrentPositionFixed { get; private set; }
        public Int2 TargetPositionFixed { get; private set; }
        public Int2 TargetCell { get; private set; }
        public int SpeedSubCellsPerTick { get; private set; }
        public int Damage { get; private set; }
        public bool HasImpacted { get; private set; }
        public int ImpactTick { get; private set; }

        public ProjectileSnapshot(
            int projectileId,
            int ownerPlayerId,
            int sourceActorId,
            int targetActorId,
            string weaponId,
            string projectileKind,
            Int2 currentPositionFixed,
            Int2 targetPositionFixed,
            Int2 targetCell,
            int speedSubCellsPerTick,
            int damage,
            bool hasImpacted,
            int impactTick)
        {
            ProjectileId = projectileId;
            OwnerPlayerId = ownerPlayerId;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            WeaponId = weaponId;
            ProjectileKind = projectileKind;
            CurrentPositionFixed = currentPositionFixed;
            TargetPositionFixed = targetPositionFixed;
            TargetCell = targetCell;
            SpeedSubCellsPerTick = speedSubCellsPerTick;
            Damage = damage;
            HasImpacted = hasImpacted;
            ImpactTick = impactTick;
        }
    }

    public sealed class CombatEventSnapshot
    {
        public int EventId { get; private set; }
        public int Tick { get; private set; }
        public string EventType { get; private set; }
        public int SourceActorId { get; private set; }
        public int TargetActorId { get; private set; }
        public int ProjectileId { get; private set; }
        public string WeaponId { get; private set; }
        public int Damage { get; private set; }
        public int TargetHealth { get; private set; }
        public Int2 Cell { get; private set; }
        public Int2 FixedWorldPosition { get; private set; }

        public CombatEventSnapshot(int eventId, int tick, string eventType, int sourceActorId, int targetActorId, int projectileId, string weaponId, int damage, int targetHealth, Int2 cell, Int2 fixedWorldPosition)
        {
            EventId = eventId;
            Tick = tick;
            EventType = eventType;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            ProjectileId = projectileId;
            WeaponId = weaponId;
            Damage = damage;
            TargetHealth = targetHealth;
            Cell = cell;
            FixedWorldPosition = fixedWorldPosition;
        }
    }

    public sealed class ProductionSnapshot
    {
        public int QueueItemId { get; private set; }
        public int ProducerActorId { get; private set; }
        public string TypeId { get; private set; }
        public int ProgressTicks { get; private set; }
        public int BuildTimeTicks { get; private set; }
        public string State { get; private set; }

        public ProductionSnapshot(int queueItemId, int producerActorId, string typeId, int progressTicks, int buildTimeTicks, string state)
        {
            QueueItemId = queueItemId;
            ProducerActorId = producerActorId;
            TypeId = typeId;
            ProgressTicks = progressTicks;
            BuildTimeTicks = buildTimeTicks;
            State = state;
        }
    }

    public sealed class PowerSnapshot
    {
        public int Generated { get; private set; }
        public int Consumed { get; private set; }
        public PlayerPowerState State { get; private set; }

        public PowerSnapshot(int generated, int consumed, PlayerPowerState state)
        {
            Generated = generated;
            Consumed = consumed;
            State = state;
        }
    }

    public sealed class PlacementPreviewSnapshot
    {
        public string TypeId { get; private set; }
        public Int2 TopLeftCell { get; private set; }
        public bool CanPlace { get; private set; }
        public string ErrorCode { get; private set; }
        public IReadOnlyList<Int2> FootprintCells { get; private set; }

        public PlacementPreviewSnapshot(string typeId, Int2 topLeftCell, bool canPlace, string errorCode, IReadOnlyList<Int2> footprintCells)
        {
            TypeId = typeId;
            TopLeftCell = topLeftCell;
            CanPlace = canPlace;
            ErrorCode = errorCode;
            FootprintCells = footprintCells;
        }
    }
}
