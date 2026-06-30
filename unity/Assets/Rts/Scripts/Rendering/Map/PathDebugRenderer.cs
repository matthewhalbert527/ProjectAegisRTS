using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Map
{
    public sealed class PathDebugRenderer : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        Transform pathRoot;
        LineRenderer line;
        Material lineMaterial;

        public int PathCellCount { get; private set; }
        public int RecentQueryCount { get; private set; }
        public string LatestQuerySummary { get; private set; }

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
            EnsureLine();
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            PathCellCount = 0;
            RecentQueryCount = 0;
            LatestQuerySummary = string.Empty;
            if (snapshot == null || snapshot.Map == null)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            EnsureRoot();
            EnsureLine();
            RecentQueryCount = snapshot.Map.RecentPathQueries.Count;
            if (RecentQueryCount == 0)
            {
                line.positionCount = 0;
                return;
            }

            var latest = snapshot.Map.RecentPathQueries[RecentQueryCount - 1];
            LatestQuerySummary = latest.MovementClass + " " + latest.StartCell + " -> " + latest.GoalCell + " / " +
                (latest.Success ? "cost " + latest.TotalCost : latest.FailureCode);

            if (!latest.Success || latest.Path.Count == 0)
            {
                line.positionCount = 0;
                return;
            }

            PathCellCount = latest.Path.Count;
            line.positionCount = latest.Path.Count + 1;
            line.SetPosition(0, mapper.CellToWorldCenter(latest.StartCell) + Vector3.up * 0.18f);
            for (var i = 0; i < latest.Path.Count; i++)
                line.SetPosition(i + 1, mapper.CellToWorldCenter(latest.Path[i]) + Vector3.up * 0.18f);
        }

        void EnsureRoot()
        {
            if (pathRoot != null)
                return;
            var root = new GameObject("Path Debug Views");
            root.transform.SetParent(transform, false);
            pathRoot = root.transform;
        }

        void EnsureLine()
        {
            if (line != null)
                return;

            lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            lineMaterial.color = new Color(0.05f, 0.85f, 0.90f, 0.92f);
            var lineObject = new GameObject("Latest Path Debug Line");
            lineObject.transform.SetParent(pathRoot, false);
            line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.widthMultiplier = 0.08f;
            line.positionCount = 0;
            line.sharedMaterial = lineMaterial;
        }
    }
}
