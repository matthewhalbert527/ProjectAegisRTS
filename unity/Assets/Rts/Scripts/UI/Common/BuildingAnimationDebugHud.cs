using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class BuildingAnimationDebugHud : MonoBehaviour
    {
        public ActorRenderSystem actorRenderSystem;
        public BuildingPowerDemoController demoController;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.F10;

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
            EnsureReferences();
        }

        void OnGUI()
        {
            if (!visible)
                return;

            EnsureReferences();
            GUILayout.BeginArea(new Rect(402f, 260f, 410f, 390f), GUI.skin.box);
            GUILayout.Label("BUILDING ANIMATION DEBUG HUD (F10)");

            if (actorRenderSystem == null)
            {
                GUILayout.Label("ActorRenderSystem: missing");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label("Building visuals: " + actorRenderSystem.BuildingVisualCount);
            GUILayout.Label("Powered: " + actorRenderSystem.PoweredBuildingCount + "  Low/offline: " + actorRenderSystem.LowPowerBuildingCount);
            GUILayout.Label("Producing: " + actorRenderSystem.ProducingBuildingCount + "  Damaged: " + actorRenderSystem.DamagedBuildingCount);

            ActorViewBehaviour view;
            if (actorRenderSystem.TryGetDebugBuildingView(out view) && view != null && view.BuildingVisual != null)
            {
                var building = view.BuildingVisual;
                GUILayout.Space(6f);
                GUILayout.Label("Selected/first building: " + building.ActorId + " " + building.ActorTypeId);
                GUILayout.Label("Profile: " + (building.ActiveProfile == null ? "none" : building.ActiveProfile.profileId));
                GUILayout.Label("Power state: " + building.PowerVisualState);
                GUILayout.Label("Animation state: " + building.AnimationVisualState);
                GUILayout.Label("Lights active: " + building.LightsActive + " intensity " + (building.Lights == null ? 0f : building.Lights.LightIntensity01).ToString("0.00"));
                GUILayout.Label("Machinery active: " + building.MachineryActive + " speed " + building.MachinerySpeed.ToString("0.00") + " phase " + building.MachineryPhase.ToString("0.00"));
                GUILayout.Label("Producing: " + building.IsProducing + " progress " + building.ProductionProgress01.ToString("0.00"));
                GUILayout.Label("Health: " + building.Health01.ToString("0.00"));
                GUILayout.Label("Door open: " + building.DoorOpen01.ToString("0.00"));
                GUILayout.Label("Damage: " + (building.Damage == null ? "missing" : (building.Damage.IsDamaged + " destroyed " + building.Damage.IsDestroyedPlaceholder)));
            }
            else
            {
                GUILayout.Label("No building visual controller available yet.");
            }

            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Trigger Low Power"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.TriggerLowPowerDemo(); });
            if (GUILayout.Button("Clear Low Power"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.ClearLowPowerDemo(); });
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Power Plant"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.QueuePowerPlantDemo(); });
            if (GUILayout.Button("Barracks"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.QueueBarracksDemo(); });
            if (GUILayout.Button("War Factory"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.QueueWarFactoryDemo(); });
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refinery"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.QueueRefineryDemo(); });
            if (GUILayout.Button("Gun Tower"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.QueueGunTowerDemo(); });
            if (GUILayout.Button("Force Visual Production"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.ForceProductionVisualDemo(); });
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Overrides"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.ClearVisualDemoOverrides(); });
            if (GUILayout.Button("Toggle Visual Debug"))
                WithDemoController(delegate(BuildingPowerDemoController demo) { demo.ToggleBuildingVisualDebug(); });
            GUILayout.EndHorizontal();

            if (demoController != null && !string.IsNullOrEmpty(demoController.StatusText))
                GUILayout.Label("Status: " + demoController.StatusText);

            GUILayout.EndArea();
        }

        void EnsureReferences()
        {
            if (actorRenderSystem == null)
                actorRenderSystem = Object.FindFirstObjectByType<ActorRenderSystem>();
            if (demoController == null)
                demoController = Object.FindFirstObjectByType<BuildingPowerDemoController>();
        }

        void WithDemoController(System.Action<BuildingPowerDemoController> action)
        {
            EnsureReferences();
            if (demoController != null && action != null)
                action(demoController);
        }
    }
}
