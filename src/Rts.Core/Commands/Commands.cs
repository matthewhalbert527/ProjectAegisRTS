using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Commands
{
    public interface ISimCommand
    {
        int PlayerId { get; }
    }

    public sealed class SelectActorsCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }

        public SelectActorsCommand(int playerId, IReadOnlyList<ActorId> actorIds)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
        }
    }

    public sealed class IssueMoveOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }
        public Int2 DestinationCell { get; private set; }

        public IssueMoveOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds, Int2 destinationCell)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
            DestinationCell = destinationCell;
        }
    }

    public sealed class IssueAttackOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }
        public ActorId TargetActorId { get; private set; }

        public IssueAttackOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds, ActorId targetActorId)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
            TargetActorId = targetActorId;
        }
    }

    public sealed class BeginProductionCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId ProducerActorId { get; private set; }
        public string TypeId { get; private set; }

        public BeginProductionCommand(int playerId, ActorId producerActorId, string typeId)
        {
            PlayerId = playerId;
            ProducerActorId = producerActorId;
            TypeId = typeId;
        }
    }

    public sealed class CancelProductionCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public int QueueItemId { get; private set; }

        public CancelProductionCommand(int playerId, int queueItemId)
        {
            PlayerId = playerId;
            QueueItemId = queueItemId;
        }
    }

    public sealed class PlaceBuildingCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public string TypeId { get; private set; }
        public Int2 TopLeftCell { get; private set; }

        public PlaceBuildingCommand(int playerId, string typeId, Int2 topLeftCell)
        {
            PlayerId = playerId;
            TypeId = typeId;
            TopLeftCell = topLeftCell;
        }
    }

    public sealed class SetRallyPointCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId ProducerActorId { get; private set; }
        public Int2 RallyCell { get; private set; }

        public SetRallyPointCommand(int playerId, ActorId producerActorId, Int2 rallyCell)
        {
            PlayerId = playerId;
            ProducerActorId = producerActorId;
            RallyCell = rallyCell;
        }
    }

    public sealed class StopCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }

        public StopCommand(int playerId, IReadOnlyList<ActorId> actorIds)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
        }
    }

    public sealed class PowerToggleCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId ActorId { get; private set; }

        public PowerToggleCommand(int playerId, ActorId actorId)
        {
            PlayerId = playerId;
            ActorId = actorId;
        }
    }
}
