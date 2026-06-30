using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class IntegratedSystemsStatusHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public bool visible = true;
        public Rect area = new Rect(12f, 258f, 360f, 218f);

        public void Initialize(RtsSimulationDriver simulationDriver)
        {
            driver = simulationDriver;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            var snapshot = driver.LatestSnapshot;
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Integrated Systems");
            GUILayout.Label(Status("Board", snapshot.Map.Width > 0 && snapshot.Map.Height > 0) +
                "  " + Status("Actors", snapshot.Actors.Count > 0) +
                "  " + Status("Terrain", snapshot.Map.TerrainCells.Count > 0));
            GUILayout.Label(Status("Economy", snapshot.Economy.Resources.Count > 0 && snapshot.Economy.Harvesters.Count > 0) +
                "  " + Status("Fog", snapshot.Fog.Cells.Count > 0) +
                "  " + Status("AI", snapshot.Ai.Players.Count > 0));
            GUILayout.Label(Status("Combat", HasEnemyAndFriendly(snapshot)) +
                "  " + Status("Minimap", snapshot.Minimap.ActorDots.Count > 0) +
                "  " + Status("Match", !string.IsNullOrEmpty(snapshot.Match.ScenarioId)));
            GUILayout.Label("Actors " + snapshot.Actors.Count + "  Resources " + snapshot.Economy.Resources.Count + "  Selected " + driver.SelectedActorIds.Count);
            GUILayout.Label("Credits " + LocalCredits(snapshot) + "  Queue " + LocalQueueCount(snapshot) + "  Command " + driver.CommandMode);
            GUILayout.EndArea();
        }

        static string Status(string label, bool ok)
        {
            return (ok ? "[ok] " : "[--] ") + label;
        }

        static bool HasEnemyAndFriendly(ProjectAegisRTS.Snapshots.WorldSnapshot snapshot)
        {
            var sawFriendly = false;
            var sawEnemy = false;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                if (snapshot.Actors[i].OwnerId == 1)
                    sawFriendly = true;
                else
                    sawEnemy = true;
            }

            return sawFriendly && sawEnemy;
        }

        static int LocalCredits(ProjectAegisRTS.Snapshots.WorldSnapshot snapshot)
        {
            for (var i = 0; i < snapshot.Players.Count; i++)
                if (snapshot.Players[i].PlayerId == 1)
                    return snapshot.Players[i].Credits;
            return 0;
        }

        static int LocalQueueCount(ProjectAegisRTS.Snapshots.WorldSnapshot snapshot)
        {
            for (var i = 0; i < snapshot.Players.Count; i++)
                if (snapshot.Players[i].PlayerId == 1)
                    return snapshot.Players[i].Production.Count;
            return 0;
        }
    }
}
