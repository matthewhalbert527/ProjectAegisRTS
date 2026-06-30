using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Power;

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

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors)
            : this(tick, players, actors, new ProjectileSnapshot[0], new CombatEventSnapshot[0], EconomySnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents)
            : this(tick, players, actors, projectiles, combatEvents, EconomySnapshot.Empty)
        {
        }

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors, IReadOnlyList<ProjectileSnapshot> projectiles, IReadOnlyList<CombatEventSnapshot> combatEvents, EconomySnapshot economy)
        {
            Tick = tick;
            Players = players;
            Actors = actors;
            Projectiles = projectiles;
            CombatEvents = combatEvents;
            Economy = economy ?? EconomySnapshot.Empty;
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
