using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Utilities;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering
{
    public sealed class ActorRenderSystem : MonoBehaviour
    {
        readonly Dictionary<int, ActorViewBehaviour> actorViews = new Dictionary<int, ActorViewBehaviour>();
        readonly List<int> removeBuffer = new List<int>();
        readonly HashSet<int> selectedIds = new HashSet<int>();
        BoardCoordinateMapper mapper;
        RtsSimulationDriver driver;
        Stage1MaterialLibrary materials;
        Transform actorRoot;
        bool smoothInterpolation;

        public void Initialize(BoardCoordinateMapper coordinateMapper, RtsSimulationDriver simulationDriver, bool enableSmoothInterpolation)
        {
            mapper = coordinateMapper;
            driver = simulationDriver;
            smoothInterpolation = enableSmoothInterpolation;
            materials = Stage1MaterialLibrary.Create();

            if (actorRoot == null)
            {
                var root = new GameObject("Actor Views");
                root.transform.SetParent(transform, false);
                actorRoot = root.transform;
            }
        }

        public void RenderSnapshot(WorldSnapshot snapshot, IReadOnlyList<int> selectedActorIds, float deltaTime)
        {
            if (snapshot == null || mapper == null || driver == null)
                return;

            selectedIds.Clear();
            for (var i = 0; i < selectedActorIds.Count; i++)
                selectedIds.Add(selectedActorIds[i]);

            var seen = new HashSet<int>();
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                seen.Add(actor.ActorId);

                ActorDefinition definition;
                if (!driver.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                ActorViewBehaviour view;
                if (!actorViews.TryGetValue(actor.ActorId, out view))
                    view = CreateActorView(actor);

                view.ApplySnapshot(actor, definition, mapper, materials, selectedIds.Contains(actor.ActorId), smoothInterpolation, driver.TicksPerSecond, snapshot.Tick);
                view.TickVisual(deltaTime);
            }

            removeBuffer.Clear();
            foreach (var pair in actorViews)
                if (!seen.Contains(pair.Key))
                    removeBuffer.Add(pair.Key);

            for (var i = 0; i < removeBuffer.Count; i++)
            {
                var id = removeBuffer[i];
                var view = actorViews[id];
                actorViews.Remove(id);
                if (view != null)
                    Destroy(view.gameObject);
            }
        }

        ActorViewBehaviour CreateActorView(ActorSnapshot actor)
        {
            var actorObject = new GameObject("Actor " + actor.ActorId + " " + actor.TypeId);
            actorObject.transform.SetParent(actorRoot, false);
            var view = actorObject.AddComponent<ActorViewBehaviour>();
            view.Initialize(actor.ActorId);
            actorViews.Add(actor.ActorId, view);
            return view;
        }
    }
}
