using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Actors
{
    public readonly struct ActorId
    {
        public readonly int Value;

        public ActorId(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public enum ActorOrderKind
    {
        Idle,
        Move,
        Attack,
        AttackMove,
        Guard,
        Patrol,
        Scatter,
        Deploy,
        Harvest,
        Stop,
        RallyPoint,
        PowerToggle
    }

    public sealed class ActorState
    {
        public ActorId Id { get; private set; }
        public int OwnerPlayerId { get; private set; }
        public string TypeId { get; private set; }
        public Int2 CellPosition { get; set; }
        public Int2 PlacementTopLeftCell { get; private set; }
        public Int2 PlacementFootprintCells { get; private set; }
        public Int2 WorldPositionFixed { get; set; }
        public int Health { get; set; }
        public ActorOrderKind CurrentOrder { get; set; }
        public Int2 OrderTargetCell { get; set; }
        public int FacingDegrees { get; set; }
        public bool IsPowered { get; set; }
        public bool IsLowPower { get; set; }
        public bool LightsActive { get; set; }
        public bool MachineryActive { get; set; }
        public bool IsProducing { get; set; }
        public int ProductionProgress { get; set; }
        public string AnimationStateId { get; set; }
        public string VisualMotionProfileId { get; set; }
        public bool ManuallyPoweredOff { get; set; }
        public List<Int2> Path { get; private set; }
        public int DesiredSpeed { get; set; }
        public int NormalizedSpeed { get; set; }
        public string MovementPhase { get; set; }
        public Int2 RallyPoint { get; set; }
        public int AttackTargetActorId { get; set; }
        public Int2 AttackTargetCell { get; set; }
        public int WeaponCooldownRemaining { get; set; }
        public bool IsAttacking { get; set; }
        public int LastDamageTick { get; set; }
        public bool IsDying { get; set; }
        public bool IsDestroyed { get; set; }
        public int DeathTick { get; set; }
        public int DestroyedByActorId { get; set; }
        public string ActiveWeaponId { get; set; }
        public bool HasHarvestOrder { get; set; }

        public ActorState(ActorId id, int ownerPlayerId, string typeId, Int2 cellPosition, int health)
        {
            Id = id;
            OwnerPlayerId = ownerPlayerId;
            TypeId = typeId;
            CellPosition = cellPosition;
            PlacementTopLeftCell = PlacementGridMetrics.CoarseCellToPlacementCell(cellPosition);
            PlacementFootprintCells = Int2.Zero;
            WorldPositionFixed = FixedMath.CellCenter(cellPosition);
            Health = health;
            CurrentOrder = ActorOrderKind.Idle;
            OrderTargetCell = cellPosition;
            FacingDegrees = 0;
            IsPowered = true;
            IsLowPower = false;
            LightsActive = true;
            MachineryActive = true;
            IsProducing = false;
            ProductionProgress = 0;
            AnimationStateId = "idle";
            VisualMotionProfileId = "none";
            Path = new List<Int2>();
            DesiredSpeed = 0;
            NormalizedSpeed = 0;
            MovementPhase = "idle";
            RallyPoint = cellPosition;
            AttackTargetActorId = 0;
            AttackTargetCell = cellPosition;
            WeaponCooldownRemaining = 0;
            IsAttacking = false;
            LastDamageTick = -1;
            IsDying = false;
            IsDestroyed = false;
            DeathTick = -1;
            DestroyedByActorId = 0;
            ActiveWeaponId = string.Empty;
            HasHarvestOrder = false;
        }

        public void SetBuildingPlacement(Int2 topLeftPlacementCell, Int2 placementFootprintCells)
        {
            PlacementTopLeftCell = topLeftPlacementCell;
            PlacementFootprintCells = placementFootprintCells;
            WorldPositionFixed = PlacementGridMetrics.PlacementFootprintCenterFixed(topLeftPlacementCell, placementFootprintCells);
        }
    }
}
