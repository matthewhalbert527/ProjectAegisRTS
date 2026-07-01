using ProjectAegisRTS.Match;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class MatchObjectiveHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public VerticalSliceScenarioController scenarioController;
        public VerticalSliceDebugActions debugActions;
        public bool visible = true;
        public bool showDebugActions;
        public Rect area = PlayerHudLayout.MatchArea;

        public void Initialize(RtsSimulationDriver simulationDriver, VerticalSliceScenarioController controller, VerticalSliceDebugActions actions)
        {
            driver = simulationDriver;
            scenarioController = controller;
            debugActions = actions;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            var snapshot = driver.LatestSnapshot;
            var previousMatrix = PlayerHudLayout.BeginArea(area);
            GUILayout.Label("Match Controls");
            GUILayout.Label(MatchStatus(snapshot.Match.Phase, snapshot.Match.LocalPlayerOutcome));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start") && scenarioController != null)
                scenarioController.StartScenario();
            if (GUILayout.Button(driver.IsPaused ? "Resume" : "Pause") && scenarioController != null)
                scenarioController.TogglePause();
            if (GUILayout.Button("Step") && scenarioController != null)
                scenarioController.StepOneTick();
            if (GUILayout.Button("Reset") && scenarioController != null)
                scenarioController.ResetScenario();
            GUILayout.EndHorizontal();

            if (showDebugActions)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reveal") && debugActions != null)
                    debugActions.RevealMap();
                if (GUILayout.Button("+Credits") && debugActions != null)
                    debugActions.GrantCredits();
                if (GUILayout.Button("Win") && debugActions != null)
                    debugActions.DestroyEnemyBase();
                if (GUILayout.Button("Lose") && debugActions != null)
                    debugActions.DestroyPlayerBase();
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Last: " + LastStatus());
            PlayerHudLayout.EndArea(previousMatrix);
        }

        static string MatchStatus(MatchPhase phase, PlayerOutcome outcome)
        {
            if (phase == MatchPhase.Won || outcome == PlayerOutcome.Victory)
                return "Victory: enemy base destroyed.";
            if (phase == MatchPhase.Lost || outcome == PlayerOutcome.Defeat)
                return "Defeat: player base destroyed.";
            if (phase == MatchPhase.Draw || outcome == PlayerOutcome.Draw)
                return "Draw: both bases destroyed.";
            return "Phase " + phase + "  Outcome " + outcome;
        }

        string LastStatus()
        {
            if (debugActions != null && !string.IsNullOrEmpty(debugActions.LastAction))
                return debugActions.LastAction;
            if (scenarioController != null)
                return scenarioController.LastStatus;
            return MatchPhase.NotStarted.ToString();
        }
    }
}
