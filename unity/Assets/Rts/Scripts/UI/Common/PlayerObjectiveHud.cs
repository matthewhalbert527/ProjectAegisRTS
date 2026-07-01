using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class PlayerObjectiveHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public bool visible = true;
        public Rect area = new Rect(12f, 12f, 380f, 220f);

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

            var snapshot = driver.LatestSnapshot;
            var player = driver.GetLocalPlayerSnapshot();

            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Objective");
            GUILayout.Label("Build economy. Produce combat units. Destroy enemy base.");
            GUILayout.Space(6f);
            GUILayout.Label("Match: " + snapshot.Match.Phase + "  Tick: " + snapshot.Tick);
            if (player != null)
            {
                GUILayout.Label("Credits: " + player.Credits + "  Power: " + player.Power.Generated + " / " + player.Power.Consumed + "  " + player.Power.State);
                GUILayout.Label("Production: " + ProductionSummary(player));
            }

            GUILayout.Label("Selection: " + SelectionSummary(snapshot));
            GUILayout.Label(EnemyBaseSummary(snapshot));

            if (snapshot.Scenario != null && snapshot.Scenario.Objectives.Count > 0)
            {
                GUILayout.Space(4f);
                for (var i = 0; i < snapshot.Scenario.Objectives.Count; i++)
                {
                    var objective = snapshot.Scenario.Objectives[i];
                    GUILayout.Label(StateLabel(objective.State) + " " + objective.Description);
                }
            }
            GUILayout.EndArea();
        }

        static string StateLabel(ScenarioObjectiveState state)
        {
            if (state == ScenarioObjectiveState.Completed)
                return "[done]";
            if (state == ScenarioObjectiveState.Failed)
                return "[fail]";
            if (state == ScenarioObjectiveState.Active)
                return "[open]";
            return "[idle]";
        }

        string SelectionSummary(WorldSnapshot snapshot)
        {
            if (driver.SelectedActorIds.Count == 0)
                return "none";

            if (driver.SelectedActorIds.Count == 1)
            {
                ActorSnapshot actor;
                if (driver.TryGetActorSnapshot(driver.SelectedActorIds[0], out actor))
                    return actor.TypeId + " #" + actor.ActorId;
            }

            return driver.SelectedActorIds.Count + " actors";
        }

        static string ProductionSummary(PlayerSnapshot player)
        {
            var active = 0;
            var pending = 0;
            for (var i = 0; i < player.Production.Count; i++)
            {
                if (player.Production[i].State == "CompletedPendingPlacement")
                    pending++;
                else
                    active++;
            }

            if (active == 0 && pending == 0)
                return "idle";
            return active + " active, " + pending + " ready to place";
        }

        static string EnemyBaseSummary(WorldSnapshot snapshot)
        {
            var localPlayerId = snapshot.Match.LocalPlayerId;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != localPlayerId && actor.TypeId == "fabrication_hub")
                    return actor.IsDestroyed ? "Enemy base: destroyed" : "Enemy base: " + actor.Health + " / " + actor.MaxHealth;
            }

            return "Enemy base: destroyed";
        }
    }
}
