using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Economy
{
    public sealed class ResourceFieldRenderSystem : MonoBehaviour
    {
        readonly Dictionary<Int2, ResourceCellViewBehaviour> views = new Dictionary<Int2, ResourceCellViewBehaviour>();
        readonly List<Int2> removeBuffer = new List<Int2>();

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        Transform resourceRoot;

        public int ResourceVisualCount { get; private set; }
        public int TotalVisibleResourceAmount { get; private set; }

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
            if (snapshot == null || snapshot.Economy == null)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            EnsureRoot();
            var seen = new HashSet<Int2>();
            TotalVisibleResourceAmount = 0;
            for (var i = 0; i < snapshot.Economy.Resources.Count; i++)
            {
                var resource = snapshot.Economy.Resources[i];
                seen.Add(resource.Cell);
                TotalVisibleResourceAmount += resource.Amount;
                ResourceCellViewBehaviour view;
                if (!views.TryGetValue(resource.Cell, out view) || view == null)
                {
                    var obj = new GameObject("Resource Cell " + resource.Cell);
                    obj.transform.SetParent(resourceRoot, false);
                    view = obj.AddComponent<ResourceCellViewBehaviour>();
                    views[resource.Cell] = view;
                }

                view.Apply(resource, mapper.CellToWorldCenter(resource.Cell));
            }

            removeBuffer.Clear();
            foreach (var pair in views)
                if (!seen.Contains(pair.Key))
                    removeBuffer.Add(pair.Key);

            for (var i = 0; i < removeBuffer.Count; i++)
            {
                var key = removeBuffer[i];
                var view = views[key];
                views.Remove(key);
                if (view != null)
                    EconomyObjectUtility.DestroyObject(view.gameObject);
            }

            ResourceVisualCount = views.Count;
        }

        void EnsureRoot()
        {
            if (resourceRoot != null)
                return;
            var root = new GameObject("Resource Field Views");
            root.transform.SetParent(transform, false);
            resourceRoot = root.transform;
        }
    }
}
