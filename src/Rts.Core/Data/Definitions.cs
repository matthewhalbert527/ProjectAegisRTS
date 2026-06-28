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

    public sealed class WeaponDefinition
    {
        public string WeaponId { get; private set; }
        public int DamagePlaceholder { get; private set; }
        public int RangeCellsPlaceholder { get; private set; }

        public WeaponDefinition(string weaponId, int damagePlaceholder, int rangeCellsPlaceholder)
        {
            WeaponId = weaponId;
            DamagePlaceholder = damagePlaceholder;
            RangeCellsPlaceholder = rangeCellsPlaceholder;
        }
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

        protected ActorDefinition(string typeId, string displayName, ActorKind kind, int maxHealth, ProductionDefinition production, PowerDefinition power, AnimationStateDefinition animation)
        {
            TypeId = typeId;
            DisplayName = displayName;
            Kind = kind;
            MaxHealth = maxHealth;
            Production = production;
            Power = power;
            Animation = animation;
        }
    }

    public sealed class UnitDefinition : ActorDefinition
    {
        public MovementDefinition Movement { get; private set; }
        public WeaponDefinition Weapon { get; private set; }

        public UnitDefinition(string typeId, string displayName, int maxHealth, ProductionDefinition production, MovementDefinition movement, WeaponDefinition weapon, AnimationStateDefinition animation)
            : base(typeId, displayName, ActorKind.Unit, maxHealth, production, new PowerDefinition(0, 0, false), animation)
        {
            Movement = movement;
            Weapon = weapon;
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
            IReadOnlyList<string> producesTypeIds)
            : base(typeId, displayName, ActorKind.Building, maxHealth, production, power, animation)
        {
            FootprintCells = footprintCells;
            ProvidesConstructionRadius = providesConstructionRadius;
            ConstructionRadiusCells = constructionRadiusCells;
            UnitExitOffset = unitExitOffset;
            ProducesTypeIds = producesTypeIds;
        }
    }
}
