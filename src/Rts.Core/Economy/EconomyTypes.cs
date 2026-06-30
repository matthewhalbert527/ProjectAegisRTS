using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Economy
{
    public enum ResourceKind
    {
        None,
        Ore
    }

    public enum HarvesterWorkState
    {
        Idle,
        MovingToResource,
        Harvesting,
        MovingToRefinery,
        WaitingForDock,
        Unloading,
        Returning,
        Blocked
    }

    public sealed class ResourceDefinition
    {
        public ResourceKind Kind { get; private set; }
        public int CreditsPerUnit { get; private set; }
        public int HarvestRatePerTick { get; private set; }

        public ResourceDefinition(ResourceKind kind, int creditsPerUnit, int harvestRatePerTick)
        {
            Kind = kind;
            CreditsPerUnit = creditsPerUnit;
            HarvestRatePerTick = harvestRatePerTick;
        }
    }

    public sealed class ResourceCellState
    {
        public Int2 Cell { get; private set; }
        public ResourceKind Kind { get; private set; }
        public int Amount { get; set; }
        public int MaxAmount { get; private set; }

        public ResourceCellState(Int2 cell, ResourceKind kind, int amount)
        {
            Cell = cell;
            Kind = kind;
            Amount = amount;
            MaxAmount = amount;
        }

        public bool IsDepleted
        {
            get { return Amount <= 0; }
        }
    }

    public sealed class HarvesterState
    {
        public int ActorId { get; private set; }
        public int CargoAmount { get; set; }
        public int CargoCapacity { get; private set; }
        public ResourceKind CarriedResourceKind { get; set; }
        public Int2 HarvestTargetCell { get; set; }
        public int AssignedRefineryActorId { get; set; }
        public HarvesterWorkState State { get; set; }
        public int HarvestProgressTicks { get; set; }
        public int UnloadProgressTicks { get; set; }

        public HarvesterState(int actorId, int cargoCapacity)
        {
            ActorId = actorId;
            CargoCapacity = cargoCapacity;
            CargoAmount = 0;
            CarriedResourceKind = ResourceKind.None;
            HarvestTargetCell = Int2.Zero;
            AssignedRefineryActorId = 0;
            State = HarvesterWorkState.Idle;
            HarvestProgressTicks = 0;
            UnloadProgressTicks = 0;
        }
    }

    public sealed class RefineryState
    {
        public int ActorId { get; private set; }
        public Int2 DockCell { get; set; }
        public int ActiveHarvesterActorId { get; set; }
        public int UnloadRatePerTick { get; private set; }
        public bool IsUnloading { get; set; }
        public int TotalResourcesReceived { get; set; }

        public RefineryState(int actorId, Int2 dockCell, int unloadRatePerTick)
        {
            ActorId = actorId;
            DockCell = dockCell;
            ActiveHarvesterActorId = 0;
            UnloadRatePerTick = unloadRatePerTick;
            IsUnloading = false;
            TotalResourcesReceived = 0;
        }
    }
}
