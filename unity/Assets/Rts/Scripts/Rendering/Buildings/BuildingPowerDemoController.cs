using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingPowerDemoController : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public ActorRenderSystem actorRenderSystem;
        public RtsStatusLog statusLog;

        bool lowPowerDemoActive;
        bool visualProductionOverrideActive;

        public string StatusText { get; private set; }

        void Awake()
        {
            EnsureReferences();
        }

        public void TriggerLowPowerDemo()
        {
            EnsureReferences();
            if (driver == null)
            {
                SetStatus("Driver missing; low-power demo unavailable.");
                return;
            }

            if (!lowPowerDemoActive)
            {
                var result = driver.TryForceLowPowerOrCreateLowPowerDemoCondition();
                lowPowerDemoActive = result.Success;
                SetStatus(result.ToString());
            }
            else
            {
                SetStatus("Low-power demo is already active.");
            }
        }

        public void ClearLowPowerDemo()
        {
            EnsureReferences();
            if (driver == null)
            {
                SetStatus("Driver missing; low-power demo unavailable.");
                return;
            }

            if (lowPowerDemoActive)
            {
                var result = driver.TryForceLowPowerOrCreateLowPowerDemoCondition();
                lowPowerDemoActive = false;
                SetStatus(result.ToString());
            }
            else
            {
                SetStatus("Low-power demo is already clear.");
            }
        }

        public void QueuePowerPlantDemo()
        {
            QueueProduction("power_plant");
        }

        public void QueueBarracksDemo()
        {
            QueueProduction("barracks");
        }

        public void QueueWarFactoryDemo()
        {
            QueueProduction("war_factory");
        }

        public void QueueRefineryDemo()
        {
            QueueProduction("refinery");
        }

        public void QueueGunTowerDemo()
        {
            QueueProduction("gun_tower");
        }

        public void ForceProductionVisualDemo()
        {
            EnsureReferences();
            ActorViewBehaviour view;
            if (actorRenderSystem != null && actorRenderSystem.TryGetDebugBuildingView(out view) && view != null && view.BuildingVisual != null)
            {
                visualProductionOverrideActive = true;
                view.BuildingVisual.SetDebugForcedState(BuildingAnimationVisualState.Producing);
                SetStatus("Visual-only production override applied to actor " + view.ActorId + ".");
                return;
            }

            SetStatus("No building visual available for production override.");
        }

        public void ClearVisualDemoOverrides()
        {
            EnsureReferences();
            ActorViewBehaviour view;
            if (actorRenderSystem != null && actorRenderSystem.TryGetDebugBuildingView(out view) && view != null && view.BuildingVisual != null)
                view.BuildingVisual.SetDebugForcedState(null);

            visualProductionOverrideActive = false;
            SetStatus("Visual demo overrides cleared.");
        }

        public void ToggleBuildingVisualDebug()
        {
            EnsureReferences();
            ActorViewBehaviour view;
            if (actorRenderSystem != null && actorRenderSystem.TryGetDebugBuildingView(out view) && view != null && view.BuildingVisual != null)
            {
                view.BuildingVisual.VisualDebugEnabled = !view.BuildingVisual.VisualDebugEnabled;
                SetStatus("Building visual debug " + (view.BuildingVisual.VisualDebugEnabled ? "enabled." : "disabled."));
                return;
            }

            SetStatus("No building visual available to toggle.");
        }

        void QueueProduction(string typeId)
        {
            EnsureReferences();
            if (driver == null)
            {
                SetStatus("Driver missing; production demo unavailable.");
                return;
            }

            var result = driver.TryQueueProduction(typeId);
            SetStatus(result.ToString());
        }

        void EnsureReferences()
        {
            if (driver == null)
                driver = Object.FindFirstObjectByType<RtsSimulationDriver>();
            if (actorRenderSystem == null)
                actorRenderSystem = Object.FindFirstObjectByType<ActorRenderSystem>();
            if (statusLog == null)
                statusLog = Object.FindFirstObjectByType<RtsStatusLog>();
        }

        void SetStatus(string status)
        {
            StatusText = status;
            if (statusLog != null)
                statusLog.AddInfo(status);
        }
    }
}
