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

        public ActorState(ActorId id, int ownerPlayerId, string typeId, Int2 cellPosition, int health)
        {
            Id = id;
            OwnerPlayerId = ownerPlayerId;
            TypeId = typeId;
            CellPosition = cellPosition;
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
        }
    }
}
