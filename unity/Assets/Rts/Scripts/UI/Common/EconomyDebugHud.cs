using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class EconomyDebugHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public ResourceFieldRenderSystem resourceFieldRenderSystem;
        public HarvesterCargoVisualController harvesterCargoVisualController;
        public RefineryDockVisualController refineryDockVisualController;
        public EconomyEventRenderSystem economyEventRenderSystem;
        public bool visible = true;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible)
                return;
            if (driver == null)
                driver = Object.FindFirstObjectByType<RtsSimulationDriver>();

            GUILayout.BeginArea(new Rect(12f, 690f, 380f, 260f), "Stage 10 Economy", GUI.skin.window);
            if (driver == null || driver.LatestSnapshot == null)
            {
                GUILayout.Label("No economy snapshot.");
                GUILayout.EndArea();
                return;
            }

            var snapshot = driver.LatestSnapshot;
            var player = driver.GetLocalPlayerSnapshot();
            GUILayout.Label("Tick: " + snapshot.Tick + "  Credits: " + (player == null ? 0 : player.Credits));
            GUILayout.Label("Resources: " + snapshot.Economy.Resources.Count + "  Harvesters: " + snapshot.Economy.Harvesters.Count + "  Refineries: " + snapshot.Economy.Refineries.Count);
            if (resourceFieldRenderSystem != null)
                GUILayout.Label("Visible resource amount: " + resourceFieldRenderSystem.TotalVisibleResourceAmount);

            HarvesterSnapshot harvester = snapshot.Economy.Harvesters.Count > 0 ? snapshot.Economy.Harvesters[0] : null;
            if (harvester != null)
                GUILayout.Label("Harvester " + harvester.ActorId + ": " + harvester.State + " cargo " + harvester.CargoAmount + "/" + harvester.CargoCapacity);

            if (GUILayout.Button("Reset Economy Demo"))
                driver.TryCreateEconomyDemoWorld();
            if (GUILayout.Button("Select Harvester"))
                SelectFirstHarvester(snapshot);
            if (GUILayout.Button("Harvest First Resource"))
                HarvestFirstResource(snapshot);
            if (GUILayout.Button("Return To Refinery"))
                driver.TryReturnSelectedHarvesters();
            GUILayout.EndArea();
        }

        void SelectFirstHarvester(WorldSnapshot snapshot)
        {
            for (var i = 0; i < snapshot.Economy.Harvesters.Count; i++)
            {
                driver.SetSelectedActorIds(new[] { snapshot.Economy.Harvesters[i].ActorId });
                return;
            }
        }

        void HarvestFirstResource(WorldSnapshot snapshot)
        {
            if (snapshot.Economy.Resources.Count == 0)
                return;
            if (driver.SelectedActorIds.Count == 0)
                SelectFirstHarvester(snapshot);
            driver.TryIssueHarvestSelectedAtCell(snapshot.Economy.Resources[0].Cell);
        }
    }
}
