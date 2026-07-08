using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visibility
{
    public sealed class RadarSnapshotAdapter : MonoBehaviour
    {
        public RtsSimulationDriver driver;

        public bool IsRadarActive { get; private set; }
        public int ProviderActorId { get; private set; }
        public int RadiusCells { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                Apply(driver.LatestSnapshot);
        }

        public void Apply(WorldSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Radar == null)
                return;

            IsRadarActive = snapshot.Radar.IsActive;
            ProviderActorId = snapshot.Radar.ProviderActorId;
            RadiusCells = snapshot.Radar.RadiusCells;
        }
    }
}
