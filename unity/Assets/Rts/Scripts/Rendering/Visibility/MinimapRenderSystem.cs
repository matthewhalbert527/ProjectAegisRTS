using System.Collections.Generic;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visibility
{
    public sealed class MinimapRenderSystem : MonoBehaviour
    {
        readonly Dictionary<int, MinimapActorDotView> actorDots = new Dictionary<int, MinimapActorDotView>();
        readonly List<int> removeBuffer = new List<int>();

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        Transform minimapRoot;

        public int ActorDotCount { get; private set; }
        public int ResourceDotCount { get; private set; }

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
            if (snapshot == null || snapshot.Minimap == null)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            EnsureRoot();
            var seen = new HashSet<int>();
            for (var i = 0; i < snapshot.Minimap.ActorDots.Count; i++)
            {
                var dot = snapshot.Minimap.ActorDots[i];
                seen.Add(dot.ActorId);
                MinimapActorDotView view;
                if (!actorDots.TryGetValue(dot.ActorId, out view) || view == null)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    obj.name = "Minimap Actor Dot " + dot.ActorId;
                    obj.transform.SetParent(minimapRoot, false);
                    view = obj.AddComponent<MinimapActorDotView>();
                    actorDots[dot.ActorId] = view;
                }

                var position = mapper.CellToWorldCenter(dot.Cell) + Vector3.up * 0.65f;
                view.Apply(dot, position, mapper.CellSizeMeters * 0.22f);
            }

            removeBuffer.Clear();
            foreach (var pair in actorDots)
                if (!seen.Contains(pair.Key))
                    removeBuffer.Add(pair.Key);

            for (var i = 0; i < removeBuffer.Count; i++)
            {
                var key = removeBuffer[i];
                var view = actorDots[key];
                actorDots.Remove(key);
                if (view != null)
                    VisibilityObjectUtility.DestroyObject(view.gameObject);
            }

            ActorDotCount = actorDots.Count;
            ResourceDotCount = snapshot.Minimap.ResourceDots.Count;
        }

        void EnsureRoot()
        {
            if (minimapRoot != null)
                return;
            var root = new GameObject("Minimap Dot Views");
            root.transform.SetParent(transform, false);
            minimapRoot = root.transform;
        }
    }
}
