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

    public sealed class IssueAttackMoveOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }
        public Int2 DestinationCell { get; private set; }

        public IssueAttackMoveOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds, Int2 destinationCell)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
            DestinationCell = destinationCell;
        }
    }

    public sealed class IssueGuardOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }

        public IssueGuardOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
        }
    }

    public sealed class IssuePatrolOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }
        public Int2 DestinationCell { get; private set; }

        public IssuePatrolOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds, Int2 destinationCell)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
            DestinationCell = destinationCell;
        }
    }

    public sealed class IssueScatterOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }

        public IssueScatterOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
        }
    }

    public sealed class IssueDeployOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }

        public IssueDeployOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
        }
    }

    public sealed class IssueForceAttackCellCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }
        public Int2 TargetCell { get; private set; }

        public IssueForceAttackCellCommand(int playerId, IReadOnlyList<ActorId> actorIds, Int2 targetCell)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
            TargetCell = targetCell;
        }
    }

    public sealed class IssueHarvestOrderCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }
        public Int2 ResourceCell { get; private set; }

        public IssueHarvestOrderCommand(int playerId, IReadOnlyList<ActorId> actorIds, Int2 resourceCell)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
            ResourceCell = resourceCell;
        }
    }

    public sealed class AssignHarvesterToResourceCellCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId HarvesterActorId { get; private set; }
        public Int2 ResourceCell { get; private set; }

        public AssignHarvesterToResourceCellCommand(int playerId, ActorId harvesterActorId, Int2 resourceCell)
        {
            PlayerId = playerId;
            HarvesterActorId = harvesterActorId;
            ResourceCell = resourceCell;
        }
    }

    public sealed class AssignHarvesterToRefineryCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId HarvesterActorId { get; private set; }
        public ActorId RefineryActorId { get; private set; }

        public AssignHarvesterToRefineryCommand(int playerId, ActorId harvesterActorId, ActorId refineryActorId)
        {
            PlayerId = playerId;
            HarvesterActorId = harvesterActorId;
            RefineryActorId = refineryActorId;
        }
    }

    public sealed class ReturnToRefineryCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public IReadOnlyList<ActorId> ActorIds { get; private set; }

        public ReturnToRefineryCommand(int playerId, IReadOnlyList<ActorId> actorIds)
        {
            PlayerId = playerId;
            ActorIds = actorIds;
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

    public sealed class ActivateSupportPowerCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public string PowerId { get; private set; }
        public Int2 TargetCell { get; private set; }

        public ActivateSupportPowerCommand(int playerId, string powerId, Int2 targetCell)
        {
            PlayerId = playerId;
            PowerId = powerId;
            TargetCell = targetCell;
        }
    }

    public sealed class BeginRepairBuildingCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId ActorId { get; private set; }

        public BeginRepairBuildingCommand(int playerId, ActorId actorId)
        {
            PlayerId = playerId;
            ActorId = actorId;
        }
    }

    public sealed class CancelRepairBuildingCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId ActorId { get; private set; }

        public CancelRepairBuildingCommand(int playerId, ActorId actorId)
        {
            PlayerId = playerId;
            ActorId = actorId;
        }
    }

    public sealed class SellBuildingCommand : ISimCommand
    {
        public int PlayerId { get; private set; }
        public ActorId ActorId { get; private set; }

        public SellBuildingCommand(int playerId, ActorId actorId)
        {
            PlayerId = playerId;
            ActorId = actorId;
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
