using ProjectAegisRTS.Match;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class MatchResultHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public VerticalSliceScenarioController scenarioController;
        public bool visible = true;
        public string bootSceneName = "Stage16_5_Boot";
        public Rect area = new Rect(0f, 0f, 500f, 280f);

        public bool HasResultToShow
        {
            get
            {
                if (!visible || driver == null || driver.LatestSnapshot == null)
                    return false;

                var phase = driver.LatestSnapshot.Match.Phase;
                return phase == MatchPhase.Won || phase == MatchPhase.Lost || phase == MatchPhase.Draw;
            }
        }

        public void Initialize(RtsSimulationDriver simulationDriver, VerticalSliceScenarioController controller)
        {
            driver = simulationDriver;
            scenarioController = controller;
        }

        void Awake()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (scenarioController == null)
                scenarioController = FindAnyObjectByType<VerticalSliceScenarioController>();
        }

        void OnGUI()
        {
            if (!HasResultToShow)
                return;

            var centered = new Rect(
                (Screen.width - area.width) * 0.5f,
                (Screen.height - area.height) * 0.5f,
                area.width,
                area.height);

            var match = driver.LatestSnapshot.Match;
            GUILayout.BeginArea(centered, GUI.skin.box);
            GUILayout.Label(ResultTitle(match.Phase));
            GUILayout.Label(ResultReason(match.Phase));
            GUILayout.Label("Elapsed ticks: " + match.ElapsedTicks);
            GUILayout.Space(12f);
            if (GUILayout.Button("Restart Scenario", GUILayout.Height(36f)) && scenarioController != null)
                scenarioController.ResetScenario();
            if (GUILayout.Button("Return to Main Menu", GUILayout.Height(32f)))
                SceneManager.LoadScene(bootSceneName, LoadSceneMode.Single);
            if (GUILayout.Button("Quit", GUILayout.Height(32f)))
                Application.Quit();
            GUILayout.EndArea();
        }

        static string ResultTitle(MatchPhase phase)
        {
            if (phase == MatchPhase.Won)
                return "Victory";
            if (phase == MatchPhase.Lost)
                return "Defeat";
            return "Draw";
        }

        static string ResultReason(MatchPhase phase)
        {
            if (phase == MatchPhase.Won)
                return "Enemy base destroyed.";
            if (phase == MatchPhase.Lost)
                return "Player base destroyed.";
            return "Both bases were destroyed.";
        }
    }
}
