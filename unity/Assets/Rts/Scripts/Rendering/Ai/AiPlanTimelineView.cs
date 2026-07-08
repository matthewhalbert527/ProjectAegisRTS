using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Ai
{
    public sealed class AiPlanTimelineView : MonoBehaviour
    {
        public RtsSimulationDriver driver;

        public int PlayerCount { get; private set; }
        public int DecisionSequence { get; private set; }
        public int RecentIntentCount { get; private set; }
        public string CurrentPlan { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver)
        {
            driver = simulationDriver;
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            PlayerCount = 0;
            DecisionSequence = 0;
            RecentIntentCount = 0;
            CurrentPlan = string.Empty;
            if (snapshot == null || snapshot.Ai == null)
                return;

            PlayerCount = snapshot.Ai.Players.Count;
            for (var i = 0; i < snapshot.Ai.Players.Count; i++)
            {
                var player = snapshot.Ai.Players[i];
                if (player.DecisionSequence >= DecisionSequence)
                {
                    DecisionSequence = player.DecisionSequence;
                    CurrentPlan = player.CurrentPlan;
                }

                RecentIntentCount += player.RecentIntents.Count;
            }
        }
    }
}
