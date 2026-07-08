using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI
{
    public sealed class RtsDebugHud : MonoBehaviour
    {
        RtsSimulationDriver driver;
        string lastCommand = "No commands yet.";
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.BackQuote;

        public void Initialize(RtsSimulationDriver simulationDriver)
        {
            driver = simulationDriver;
        }

        public void RecordCommandResult(RtsCommandResult result)
        {
            lastCommand = result == null ? "No command result." : result.ToString();
            Debug.Log(lastCommand);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            var snapshot = driver.LatestSnapshot;
            var player = driver.GetLocalPlayerSnapshot();

            GUILayout.BeginArea(new Rect(12f, 12f, 318f, Screen.height - 24f), GUI.skin.box);
            GUILayout.Label("ProjectAegisRTS Stage 1");
            GUILayout.Space(4f);
            GUILayout.Label("Tick: " + snapshot.Tick);
            GUILayout.Label("State: " + (driver.IsPaused ? "Paused" : "Running"));
            GUILayout.Label("Credits: " + (player == null ? "n/a" : player.Credits.ToString()));
            if (player != null)
                GUILayout.Label("Power: " + player.Power.Generated + "/" + player.Power.Consumed + " " + player.Power.State);
            GUILayout.Label("Actors: " + snapshot.Actors.Count);
            GUILayout.Label("Selected: " + driver.SelectedActorIdsText());
            GUILayout.Label("Hovered: " + driver.HoveredCellText());
            GUILayout.Label("Mode: " + driver.CommandMode);
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(driver.IsPaused ? "Resume" : "Pause"))
                RecordCommandResult(driver.TogglePause());
            if (GUILayout.Button("Step"))
                RecordCommandResult(driver.StepOneTick());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Power Plant"))
                RecordCommandResult(driver.TryQueueProduction("power_plant"));
            if (GUILayout.Button("Barracks"))
                RecordCommandResult(driver.TryQueueProduction("barracks"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("War Factory"))
                RecordCommandResult(driver.TryQueueProduction("war_factory"));
            if (GUILayout.Button("Refinery"))
                RecordCommandResult(driver.TryQueueProduction("refinery"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gun Tower"))
                RecordCommandResult(driver.TryQueueProduction("gun_tower"));
            if (GUILayout.Button("Infantry"))
                RecordCommandResult(driver.TryQueueProduction("rifle_infantry"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Light Tank"))
                RecordCommandResult(driver.TryQueueProduction("light_tank"));
            if (GUILayout.Button("Harvester"))
                RecordCommandResult(driver.TryQueueProduction("harvester"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Place Ready"))
                RecordCommandResult(driver.TryEnterPlacementModeForFirstPending());
            if (GUILayout.Button("Cancel Place"))
                RecordCommandResult(driver.TryCancelPlacement());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Low Power"))
                RecordCommandResult(driver.TryForceLowPowerOrCreateLowPowerDemoCondition());
            if (GUILayout.Button("Reset"))
                RecordCommandResult(driver.ResetDemoWorld());
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            GUILayout.Label("Last: " + lastCommand);
            DrawProductionQueue(player);
            GUILayout.EndArea();
        }

        void DrawProductionQueue(PlayerSnapshot player)
        {
            if (player == null || player.Production.Count == 0)
                return;

            GUILayout.Space(6f);
            GUILayout.Label("Production");
            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                GUILayout.Label(item.TypeId + " " + item.ProgressTicks + "/" + item.BuildTimeTicks + " " + item.State);
            }
        }
    }
}
