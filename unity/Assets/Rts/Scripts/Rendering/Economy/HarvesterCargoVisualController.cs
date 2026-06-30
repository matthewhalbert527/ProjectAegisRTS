using System.Collections.Generic;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Economy
{
    public sealed class HarvesterCargoVisualController : MonoBehaviour
    {
        readonly Dictionary<int, GameObject> cargoViews = new Dictionary<int, GameObject>();
        readonly List<int> removeBuffer = new List<int>();

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public int CargoVisualCount { get; private set; }
        public int LastObservedCargoAmount { get; private set; }

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

            var seen = new HashSet<int>();
            for (var i = 0; i < snapshot.Economy.Harvesters.Count; i++)
            {
                var harvester = snapshot.Economy.Harvesters[i];
                var actor = FindActor(snapshot, harvester.ActorId);
                if (actor == null)
                    continue;

                seen.Add(harvester.ActorId);
                LastObservedCargoAmount = harvester.CargoAmount;
                GameObject view;
                if (!cargoViews.TryGetValue(harvester.ActorId, out view) || view == null)
                {
                    view = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    view.name = "Harvester Cargo " + harvester.ActorId;
                    view.transform.SetParent(transform, false);
                    var collider = view.GetComponent<Collider>();
                    if (collider != null)
                        EconomyObjectUtility.DestroyObject(collider);
                    var renderer = view.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                        renderer.sharedMaterial.color = new Color(0.1f, 0.85f, 0.55f, 1f);
                    }
                    cargoViews[harvester.ActorId] = view;
                }

                var normalized = harvester.CargoCapacity <= 0 ? 0f : Mathf.Clamp01(harvester.CargoAmount / (float)harvester.CargoCapacity);
                view.transform.position = mapper.FixedWorldToBoardWorld(actor.FixedWorldPosition) + Vector3.up * (0.65f + normalized * 0.45f);
                view.transform.localScale = new Vector3(0.18f, 0.08f + normalized * 0.28f, 0.18f);
                view.SetActive(harvester.CargoAmount > 0);
            }

            removeBuffer.Clear();
            foreach (var pair in cargoViews)
                if (!seen.Contains(pair.Key))
                    removeBuffer.Add(pair.Key);
            for (var i = 0; i < removeBuffer.Count; i++)
            {
                var key = removeBuffer[i];
                var view = cargoViews[key];
                cargoViews.Remove(key);
                EconomyObjectUtility.DestroyObject(view);
            }

            CargoVisualCount = cargoViews.Count;
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].ActorId == actorId)
                    return snapshot.Actors[i];
            return null;
        }
    }
}
