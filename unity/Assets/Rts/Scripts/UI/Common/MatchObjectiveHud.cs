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
        public Rect area = new Rect(12f, 12f, 360f, 238f);

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
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Stage 16 Vertical Slice");
            GUILayout.Label("Tick " + snapshot.Tick + "  Phase " + snapshot.Match.Phase + "  Outcome " + snapshot.Match.LocalPlayerOutcome);
            for (var i = 0; i < snapshot.Scenario.Objectives.Count; i++)
            {
                var objective = snapshot.Scenario.Objectives[i];
                GUILayout.Label(StateLabel(objective.State) + " " + objective.Description);
            }

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

            GUILayout.Label("Last: " + LastStatus());
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
