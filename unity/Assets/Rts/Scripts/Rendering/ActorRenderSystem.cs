using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
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

        public VisualMotionProfileLibrary motionProfileLibrary;
        public BuildingVisualProfileLibrary buildingProfileLibrary;
        public int ActorVisualCount { get; private set; }
        public int MovingVisualCount { get; private set; }
        public int VehicleMotionControllerCount { get; private set; }
        public int InfantryMotionControllerCount { get; private set; }
        public int AircraftMotionControllerCount { get; private set; }
        public int BuildingVisualCount { get; private set; }
        public int PoweredBuildingCount { get; private set; }
        public int LowPowerBuildingCount { get; private set; }
        public int ProducingBuildingCount { get; private set; }
        public int DamagedBuildingCount { get; private set; }

        public void Initialize(BoardCoordinateMapper coordinateMapper, RtsSimulationDriver simulationDriver, bool enableSmoothInterpolation)
        {
            mapper = coordinateMapper;
            driver = simulationDriver;
            smoothInterpolation = enableSmoothInterpolation;
            materials = Stage1MaterialLibrary.Create();
            if (motionProfileLibrary == null)
                motionProfileLibrary = Object.FindFirstObjectByType<VisualMotionProfileLibrary>();
            if (motionProfileLibrary != null)
                motionProfileLibrary.EnsureInitialized();
            if (buildingProfileLibrary == null)
                buildingProfileLibrary = Object.FindFirstObjectByType<BuildingVisualProfileLibrary>();
            if (buildingProfileLibrary != null)
                buildingProfileLibrary.EnsureInitialized();

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

                var profile = motionProfileLibrary == null ? null : motionProfileLibrary.GetProfile(actor.TypeId, definition, actor.VisualMotionProfileId);
                var buildingDefinition = definition as BuildingDefinition;
                var buildingProfile = buildingProfileLibrary == null || buildingDefinition == null ? null : buildingProfileLibrary.GetProfile(actor.TypeId, buildingDefinition);
                view.ApplySnapshot(actor, definition, mapper, materials, selectedIds.Contains(actor.ActorId), smoothInterpolation, driver.TicksPerSecond, snapshot.Tick, profile, buildingProfile);
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

            UpdateDebugStats();
        }

        public bool TryGetDebugActorView(out ActorViewBehaviour view)
        {
            view = null;

            foreach (var selectedId in selectedIds)
                if (actorViews.TryGetValue(selectedId, out view) && view != null)
                    return true;

            foreach (var pair in actorViews)
            {
                view = pair.Value;
                return view != null;
            }

            return false;
        }

        public bool TryGetActorView(int actorId, out ActorViewBehaviour view)
        {
            return actorViews.TryGetValue(actorId, out view);
        }

        public bool TryGetDebugBuildingView(out ActorViewBehaviour view)
        {
            view = null;

            foreach (var selectedId in selectedIds)
                if (actorViews.TryGetValue(selectedId, out view) && view != null && view.BuildingVisual != null && view.BuildingVisual.enabled)
                    return true;

            foreach (var pair in actorViews)
            {
                view = pair.Value;
                if (view != null && view.BuildingVisual != null && view.BuildingVisual.enabled)
                    return true;
            }

            view = null;
            return false;
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

        void UpdateDebugStats()
        {
            ActorVisualCount = actorViews.Count;
            MovingVisualCount = 0;
            VehicleMotionControllerCount = 0;
            InfantryMotionControllerCount = 0;
            AircraftMotionControllerCount = 0;
            BuildingVisualCount = 0;
            PoweredBuildingCount = 0;
            LowPowerBuildingCount = 0;
            ProducingBuildingCount = 0;
            DamagedBuildingCount = 0;

            foreach (var pair in actorViews)
            {
                var view = pair.Value;
                if (view == null)
                    continue;

                if (view.ActorVisualMotion != null && view.ActorVisualMotion.IsMoving)
                    MovingVisualCount++;
                if (view.VehicleMotion != null && view.VehicleMotion.enabled)
                    VehicleMotionControllerCount++;
                if (view.InfantryMotion != null && view.InfantryMotion.enabled)
                    InfantryMotionControllerCount++;
                if (view.AircraftMotion != null && view.AircraftMotion.enabled)
                    AircraftMotionControllerCount++;
                if (view.BuildingVisual != null && view.BuildingVisual.enabled)
                {
                    BuildingVisualCount++;
                    if (view.BuildingVisual.PowerVisualState == BuildingPowerVisualState.Normal)
                        PoweredBuildingCount++;
                    if (view.BuildingVisual.PowerVisualState == BuildingPowerVisualState.LowPower || view.BuildingVisual.PowerVisualState == BuildingPowerVisualState.Offline)
                        LowPowerBuildingCount++;
                    if (view.BuildingVisual.IsProducing)
                        ProducingBuildingCount++;
                    if (view.BuildingVisual.Damage != null && view.BuildingVisual.Damage.IsDamaged)
                        DamagedBuildingCount++;
                }
            }
        }
    }
}
