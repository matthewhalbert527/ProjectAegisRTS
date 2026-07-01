using ProjectAegisRTS.Match;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class PlayerPromptHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public bool visible = true;
        public Rect area = new Rect(12f, 240f, 380f, 112f);

        public void Initialize(RtsSimulationDriver simulationDriver)
        {
            driver = simulationDriver;
        }

        void Awake()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Next Step");
            GUILayout.Label(BuildPrompt());
            GUILayout.Label("F1 / H controls  |  O objective HUD  |  Space pause");
            GUILayout.EndArea();
        }

        string BuildPrompt()
        {
            var snapshot = driver.LatestSnapshot;
            if (snapshot.Match.Phase == MatchPhase.Won || snapshot.Match.Phase == MatchPhase.Lost || snapshot.Match.Phase == MatchPhase.Draw)
                return "Match complete. Use the result screen to restart or return to the menu.";

            if (driver.HasPlacementMode)
                return "Place " + driver.PendingPlacementTypeId + " on a clear highlighted footprint, or press Escape to cancel.";

            if (driver.SelectedActorIds.Count == 0)
                return "Left click a unit or building. Select a harvester to start economy, or combat units to attack.";

            var player = driver.GetLocalPlayerSnapshot();
            if (player != null && player.Production.Count == 0)
                return "Use the right sidebar to queue a refinery, infantry, vehicles, or defenses.";

            return "Right click the board to move. Right click an enemy to attack. Destroy the enemy fabrication hub.";
        }
    }
}
