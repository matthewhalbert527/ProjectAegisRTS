using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Data
{
    public enum ActorKind
    {
        Unit,
        Building
    }

    public enum ProductionKind
    {
        None,
        Building,
        Infantry,
        Vehicle,
        Aircraft,
        Support
    }

    public enum DamageKind
    {
        None,
        Kinetic,
        Explosive,
        Fire,
        Energy
    }

    public enum TargetKind
    {
        None,
        Ground,
        Air,
        Building,
        Unit
    }

    public enum WeaponFireMode
    {
        InstantHit,
        Projectile
    }

    public enum ProjectileKind
    {
        None,
        Bullet,
        Shell,
        Rocket,
        Beam
    }

    public sealed class DamageDefinition
    {
        public int Amount { get; private set; }
        public DamageKind Kind { get; private set; }

        public DamageDefinition(int amount, DamageKind kind)
        {
            Amount = amount;
            Kind = kind;
        }
    }

    public sealed class ProjectileDefinition
    {
        public ProjectileKind Kind { get; private set; }
        public int SpeedSubCellsPerTick { get; private set; }
        public int LifetimeTicks { get; private set; }
        public bool HomesToTarget { get; private set; }

        public ProjectileDefinition(ProjectileKind kind, int speedSubCellsPerTick, int lifetimeTicks, bool homesToTarget)
        {
            Kind = kind;
            SpeedSubCellsPerTick = speedSubCellsPerTick;
            LifetimeTicks = lifetimeTicks;
            HomesToTarget = homesToTarget;
        }
    }

    public sealed class DeathDefinition
    {
        public int DestroyedVisibleTicks { get; private set; }
        public string DeathVisualId { get; private set; }

        public DeathDefinition(int destroyedVisibleTicks, string deathVisualId)
        {
            DestroyedVisibleTicks = destroyedVisibleTicks;
            DeathVisualId = deathVisualId;
        }
    }

    public sealed class WeaponDefinition
    {
        public string WeaponId { get; private set; }
        public string DisplayName { get; private set; }
        public int Damage { get; private set; }
        public int RangeCells { get; private set; }
        public int MinRangeCells { get; private set; }
        public int CooldownTicks { get; private set; }
        public int ProjectileSpeedSubCellsPerTick { get; private set; }
        public ProjectileKind ProjectileKind { get; private set; }
        public WeaponFireMode FireMode { get; private set; }
        public bool CanTargetGround { get; private set; }
        public bool CanTargetAir { get; private set; }
        public bool CanTargetBuildings { get; private set; }
        public bool CanTargetUnits { get; private set; }
        public bool RequiresLineOfSight { get; private set; }
        public int BurstCount { get; private set; }
        public int BurstDelayTicks { get; private set; }
        public int AreaRadiusCells { get; private set; }
        public string MuzzleSocketId { get; private set; }
        public string ImpactVisualId { get; private set; }
        public string ProjectileVisualId { get; private set; }
        public DamageDefinition DamageDefinition { get; private set; }
        public ProjectileDefinition ProjectileDefinition { get; private set; }

        public WeaponDefinition(string weaponId, int damagePlaceholder, int rangeCellsPlaceholder)
            : this(
                weaponId,
                weaponId,
                damagePlaceholder,
                rangeCellsPlaceholder,
                0,
                30,
                512,
                ProjectileKind.Shell,
                true,
                false,
                true,
                true,
                false,
                1,
                0,
                0,
                "MuzzlePrimary",
                "impact_placeholder",
                weaponId + "_projectile")
        {
        }

        public WeaponDefinition(
            string weaponId,
            string displayName,
            int damage,
            int rangeCells,
            int minRangeCells,
            int cooldownTicks,
            int projectileSpeedSubCellsPerTick,
            ProjectileKind projectileKind,
            bool canTargetGround,
            bool canTargetAir,
            bool canTargetBuildings,
            bool canTargetUnits,
            bool requiresLineOfSight,
            int burstCount,
            int burstDelayTicks,
            int areaRadiusCells,
            string muzzleSocketId,
            string impactVisualId,
            string projectileVisualId)
        {
            WeaponId = weaponId;
            DisplayName = displayName;
            Damage = damage;
            RangeCells = rangeCells;
            MinRangeCells = minRangeCells;
            CooldownTicks = cooldownTicks;
            ProjectileSpeedSubCellsPerTick = projectileSpeedSubCellsPerTick;
            ProjectileKind = projectileKind;
            FireMode = projectileKind == ProjectileKind.None || projectileSpeedSubCellsPerTick <= 0 ? WeaponFireMode.InstantHit : WeaponFireMode.Projectile;
            CanTargetGround = canTargetGround;
            CanTargetAir = canTargetAir;
            CanTargetBuildings = canTargetBuildings;
            CanTargetUnits = canTargetUnits;
            RequiresLineOfSight = requiresLineOfSight;
            BurstCount = burstCount <= 0 ? 1 : burstCount;
            BurstDelayTicks = burstDelayTicks;
            AreaRadiusCells = areaRadiusCells;
            MuzzleSocketId = string.IsNullOrEmpty(muzzleSocketId) ? "MuzzlePrimary" : muzzleSocketId;
            ImpactVisualId = string.IsNullOrEmpty(impactVisualId) ? "impact_placeholder" : impactVisualId;
            ProjectileVisualId = string.IsNullOrEmpty(projectileVisualId) ? weaponId + "_projectile" : projectileVisualId;
            DamageDefinition = new DamageDefinition(damage, projectileKind == ProjectileKind.Rocket ? DamageKind.Explosive : DamageKind.Kinetic);
            ProjectileDefinition = new ProjectileDefinition(projectileKind, projectileSpeedSubCellsPerTick, 240, false);
        }

        public int DamagePlaceholder { get { return Damage; } }
        public int RangeCellsPlaceholder { get { return RangeCells; } }
    }

    public sealed class ProductionDefinition
    {
        public ProductionKind Kind { get; private set; }
        public int Cost { get; private set; }
        public int BuildTimeTicks { get; private set; }
        public string FactoryTypeId { get; private set; }
        public bool ExemptFromLowPowerPause { get; private set; }

        public ProductionDefinition(ProductionKind kind, int cost, int buildTimeTicks, string factoryTypeId, bool exemptFromLowPowerPause)
        {
            Kind = kind;
            Cost = cost;
            BuildTimeTicks = buildTimeTicks;
            FactoryTypeId = factoryTypeId;
            ExemptFromLowPowerPause = exemptFromLowPowerPause;
        }
    }

    public sealed class PowerDefinition
    {
        public int Generated { get; private set; }
        public int Consumed { get; private set; }
        public bool Essential { get; private set; }

        public PowerDefinition(int generated, int consumed, bool essential)
        {
            Generated = generated;
            Consumed = consumed;
            Essential = essential;
        }
    }

    public sealed class MovementDefinition
    {
        public int SpeedPerTick { get; private set; }
        public int TurnRateDegreesPerTick { get; private set; }
        public string VisualMotionProfileId { get; private set; }

        public MovementDefinition(int speedPerTick, int turnRateDegreesPerTick, string visualMotionProfileId)
        {
            SpeedPerTick = speedPerTick;
            TurnRateDegreesPerTick = turnRateDegreesPerTick;
            VisualMotionProfileId = visualMotionProfileId;
        }
    }

    public sealed class AnimationStateDefinition
    {
        public string IdleStateId { get; private set; }
        public string ProducingStateId { get; private set; }
        public string LowPowerStateId { get; private set; }
        public string OfflineStateId { get; private set; }

        public AnimationStateDefinition(string idleStateId, string producingStateId, string lowPowerStateId, string offlineStateId)
        {
            IdleStateId = idleStateId;
            ProducingStateId = producingStateId;
            LowPowerStateId = lowPowerStateId;
            OfflineStateId = offlineStateId;
        }
    }

    public abstract class ActorDefinition
    {
        public string TypeId { get; private set; }
        public string DisplayName { get; private set; }
        public ActorKind Kind { get; private set; }
        public int MaxHealth { get; private set; }
        public ProductionDefinition Production { get; private set; }
        public PowerDefinition Power { get; private set; }
        public AnimationStateDefinition Animation { get; private set; }
        public WeaponDefinition Weapon { get; private set; }
        public DeathDefinition Death { get; private set; }

        protected ActorDefinition(string typeId, string displayName, ActorKind kind, int maxHealth, ProductionDefinition production, PowerDefinition power, AnimationStateDefinition animation, WeaponDefinition weapon, DeathDefinition death)
        {
            TypeId = typeId;
            DisplayName = displayName;
            Kind = kind;
            MaxHealth = maxHealth;
            Production = production;
            Power = power;
            Animation = animation;
            Weapon = weapon;
            Death = death ?? new DeathDefinition(120, "death_placeholder");
        }
    }

    public sealed class UnitDefinition : ActorDefinition
    {
        public MovementDefinition Movement { get; private set; }

        public UnitDefinition(string typeId, string displayName, int maxHealth, ProductionDefinition production, MovementDefinition movement, WeaponDefinition weapon, AnimationStateDefinition animation)
            : base(typeId, displayName, ActorKind.Unit, maxHealth, production, new PowerDefinition(0, 0, false), animation, weapon, new DeathDefinition(120, "unit_death_placeholder"))
        {
            Movement = movement;
        }
    }

    public sealed class BuildingDefinition : ActorDefinition
    {
        public Int2 FootprintCells { get; private set; }
        public bool ProvidesConstructionRadius { get; private set; }
        public int ConstructionRadiusCells { get; private set; }
        public Int2 UnitExitOffset { get; private set; }
        public IReadOnlyList<string> ProducesTypeIds { get; private set; }

        public BuildingDefinition(
            string typeId,
            string displayName,
            int maxHealth,
            ProductionDefinition production,
            PowerDefinition power,
            AnimationStateDefinition animation,
            Int2 footprintCells,
            bool providesConstructionRadius,
            int constructionRadiusCells,
            Int2 unitExitOffset,
            IReadOnlyList<string> producesTypeIds,
            WeaponDefinition weapon = null)
            : base(typeId, displayName, ActorKind.Building, maxHealth, production, power, animation, weapon, new DeathDefinition(180, "building_death_placeholder"))
        {
            FootprintCells = footprintCells;
            ProvidesConstructionRadius = providesConstructionRadius;
            ConstructionRadiusCells = constructionRadiusCells;
            UnitExitOffset = unitExitOffset;
            ProducesTypeIds = producesTypeIds;
        }
    }
}
