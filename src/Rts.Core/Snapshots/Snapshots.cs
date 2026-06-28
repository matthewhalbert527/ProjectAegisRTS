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

        public WorldSnapshot(int tick, IReadOnlyList<PlayerSnapshot> players, IReadOnlyList<ActorSnapshot> actors)
        {
            Tick = tick;
            Players = players;
            Actors = actors;
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
