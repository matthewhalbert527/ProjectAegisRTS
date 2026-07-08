using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Ai;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class AiDebugHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public AiIntentRenderSystem aiIntentRenderSystem;
        public AiPlanTimelineView aiPlanTimelineView;
        public bool visible = true;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            var snapshot = driver.LatestSnapshot;
            GUILayout.BeginArea(new Rect(410, 600, 420, 155), GUI.skin.box);
            GUILayout.Label("Stage 12 AI Skirmish");
            GUILayout.Label("Tick: " + snapshot.Tick + "  AI players: " + snapshot.Ai.Players.Count);
            if (snapshot.Ai.Players.Count > 0)
            {
                var ai = snapshot.Ai.Players[0];
                GUILayout.Label("Player " + ai.PlayerId + " / " + ai.DifficultyId + " / seq " + ai.DecisionSequence + " / invalid " + ai.ConsecutiveInvalidCommands);
                GUILayout.Label("Plan: " + ai.CurrentPlan + " next " + ai.NextDecisionTick);
            }

            GUILayout.Label("Intent count: " + (aiIntentRenderSystem != null ? aiIntentRenderSystem.IntentCount : 0) +
                " issued: " + (aiIntentRenderSystem != null ? aiIntentRenderSystem.IssuedCommandCount : 0));
            GUILayout.Label("Latest: " + (aiIntentRenderSystem != null ? aiIntentRenderSystem.LatestIntentSummary : string.Empty));
            if (GUILayout.Button("Reset AI Demo"))
                driver.TryCreateAiSkirmishDemoWorld();
            GUILayout.EndArea();
        }
    }
}
