using ProjectAegisRTS.Actors;

namespace ProjectAegisRTS.Production
{
    public enum ProductionItemState
    {
        Active,
        Paused,
        CompletedPendingPlacement
    }

    public sealed class ProductionQueueItem
    {
        public int QueueItemId { get; private set; }
        public int PlayerId { get; private set; }
        public ActorId ProducerActorId { get; private set; }
        public string TypeId { get; private set; }
        public int TotalCost { get; private set; }
        public int BuildTimeTicks { get; private set; }
        public int ProgressTicks { get; set; }
        public ProductionItemState State { get; set; }
        public bool IsBuilding { get; private set; }
        public bool ExemptFromLowPowerPause { get; private set; }

        public ProductionQueueItem(int queueItemId, int playerId, ActorId producerActorId, string typeId, int totalCost, int buildTimeTicks, bool isBuilding, bool exemptFromLowPowerPause)
        {
            QueueItemId = queueItemId;
            PlayerId = playerId;
            ProducerActorId = producerActorId;
            TypeId = typeId;
            TotalCost = totalCost;
            BuildTimeTicks = buildTimeTicks;
            ProgressTicks = 0;
            State = ProductionItemState.Active;
            IsBuilding = isBuilding;
            ExemptFromLowPowerPause = exemptFromLowPowerPause;
        }
    }
}
