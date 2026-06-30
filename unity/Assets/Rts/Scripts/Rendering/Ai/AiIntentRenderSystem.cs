using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Ai
{
    public sealed class AiIntentRenderSystem : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        Transform markerRoot;

        public int IntentCount { get; private set; }
        public int IssuedCommandCount { get; private set; }
        public string LatestIntentSummary { get; private set; }

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
            IntentCount = 0;
            IssuedCommandCount = 0;
            LatestIntentSummary = string.Empty;
            if (snapshot == null || snapshot.Ai == null || snapshot.Ai.Players.Count == 0)
                return;

            EnsureRoot();
            for (var playerIndex = 0; playerIndex < snapshot.Ai.Players.Count; playerIndex++)
            {
                var player = snapshot.Ai.Players[playerIndex];
                for (var intentIndex = 0; intentIndex < player.RecentIntents.Count; intentIndex++)
                {
                    var intent = player.RecentIntents[intentIndex];
                    IntentCount++;
                    if (intent.WasCommandIssued)
                        IssuedCommandCount++;
                    LatestIntentSummary = intent.Kind + " / " + intent.IntentId + " / " + intent.ResultCode;
                }
            }
        }

        public Vector3 CellToWorld(Int2 cell)
        {
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            return mapper == null ? Vector3.zero : mapper.CellToWorldCenter(cell);
        }

        void EnsureRoot()
        {
            if (markerRoot != null)
                return;
            var root = new GameObject("AI Intent Views");
            root.transform.SetParent(transform, false);
            markerRoot = root.transform;
        }
    }
}
