using System.Collections.Generic;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Production;
using ProjectAegisRTS.Support;

namespace ProjectAegisRTS.Simulation
{
    public sealed class PlayerState
    {
        readonly List<ProductionQueueItem> productionQueue;
        readonly Dictionary<string, SupportPowerState> supportPowers;

        public int PlayerId { get; private set; }
        public string Name { get; private set; }
        public int Credits { get; set; }
        public int PowerGenerated { get; set; }
        public int PowerConsumed { get; set; }
        public PlayerPowerState PowerState { get; set; }
        public PlayerPowerState? ForcedPowerState { get; set; }

        public PlayerState(int playerId, string name, int credits)
        {
            PlayerId = playerId;
            Name = name;
            Credits = credits;
            PowerState = PlayerPowerState.Normal;
            productionQueue = new List<ProductionQueueItem>();
            supportPowers = new Dictionary<string, SupportPowerState>();
        }

        public List<ProductionQueueItem> MutableProductionQueue
        {
            get { return productionQueue; }
        }

        public IReadOnlyList<ProductionQueueItem> ProductionQueue
        {
            get { return productionQueue; }
        }

        public Dictionary<string, SupportPowerState> MutableSupportPowers
        {
            get { return supportPowers; }
        }

        public IReadOnlyDictionary<string, SupportPowerState> SupportPowers
        {
            get { return supportPowers; }
        }
    }
}
