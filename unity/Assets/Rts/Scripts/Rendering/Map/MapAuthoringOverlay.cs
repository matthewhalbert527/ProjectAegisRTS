using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Map
{
    public sealed class MapAuthoringOverlay : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;

        public int AuthoringCellCount { get; private set; }
        public string Summary { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            AuthoringCellCount = 0;
            Summary = string.Empty;
            if (snapshot == null || snapshot.Map == null)
                return;

            AuthoringCellCount = snapshot.Map.TerrainCells.Count;
            Summary = snapshot.Map.Width + "x" + snapshot.Map.Height + " terrain cells / valid=" + snapshot.Map.IsValid;
        }
    }
}
