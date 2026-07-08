using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Economy
{
    public sealed class RefineryDockVisualController : MonoBehaviour
    {
        readonly Dictionary<int, GameObject> dockViews = new Dictionary<int, GameObject>();

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public int DockVisualCount { get; private set; }
        public int UnloadingDockCount { get; private set; }

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
            if (snapshot == null || snapshot.Economy == null)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            UnloadingDockCount = 0;
            for (var i = 0; i < snapshot.Economy.Refineries.Count; i++)
            {
                var refinery = snapshot.Economy.Refineries[i];
                GameObject view;
                if (!dockViews.TryGetValue(refinery.ActorId, out view) || view == null)
                {
                    view = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    view.name = "Refinery Dock " + refinery.ActorId;
                    view.transform.SetParent(transform, false);
                    var collider = view.GetComponent<Collider>();
                    if (collider != null)
                        EconomyObjectUtility.DestroyObject(collider);
                    var renderer = view.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                    dockViews[refinery.ActorId] = view;
                }

                if (refinery.IsUnloading)
                    UnloadingDockCount++;
                view.transform.position = mapper.CellToWorldCenter(refinery.DockCell) + Vector3.up * 0.06f;
                view.transform.localScale = new Vector3(0.5f, 0.04f, 0.5f);
                var r = view.GetComponent<Renderer>();
                if (r != null)
                    r.sharedMaterial.color = refinery.IsUnloading ? new Color(0.1f, 0.95f, 0.35f, 1f) : new Color(0.2f, 0.45f, 0.75f, 0.85f);
            }

            DockVisualCount = dockViews.Count;
        }
    }
}
