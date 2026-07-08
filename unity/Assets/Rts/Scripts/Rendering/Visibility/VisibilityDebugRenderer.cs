using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visibility
{
    public sealed class VisibilityDebugRenderer : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        Transform markerRoot;

        public int VisibleCellSampleCount { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
            EnsureRoot();
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Fog == null || snapshot.Fog.Cells.Count == 0)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            EnsureRoot();
            VisibleCellSampleCount = 0;
            for (var i = 0; i < snapshot.Fog.Cells.Count; i++)
                if (snapshot.Fog.Cells[i].Visibility == ProjectAegisRTS.Visibility.CellVisibility.Visible)
                    VisibleCellSampleCount++;
        }

        void EnsureRoot()
        {
            if (markerRoot != null)
                return;
            var root = new GameObject("Visibility Debug Views");
            root.transform.SetParent(transform, false);
            markerRoot = root.transform;
        }
    }
}
