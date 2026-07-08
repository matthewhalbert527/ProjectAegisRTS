using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class PlayerPromptHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public PlayerPromptSystem promptSystem;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.P;
        public Rect area = PlayerHudLayout.PromptArea;

        public void Initialize(RtsSimulationDriver simulationDriver, PlayerPromptSystem system = null)
        {
            driver = simulationDriver;
            promptSystem = system;
        }

        void Awake()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (promptSystem == null)
                promptSystem = FindAnyObjectByType<PlayerPromptSystem>();
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
            if (promptSystem != null && !promptSystem.visible)
                return;

            var previousMatrix = PlayerHudLayout.BeginArea(area);
            GUILayout.Label("Next Step");
            GUILayout.Label(BuildPrompt());
            GUILayout.Label("P prompt  |  C checklist  |  F1/H controls");
            PlayerHudLayout.EndArea(previousMatrix);
        }

        string BuildPrompt()
        {
            if (promptSystem == null)
                promptSystem = FindAnyObjectByType<PlayerPromptSystem>();
            if (promptSystem != null)
                return promptSystem.GetPrompt();

            return "Select your Fabrication Hub, build economy, train units, and destroy the enemy base.";
        }
    }
}
