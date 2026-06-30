using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class RenderStatsHud : MonoBehaviour
    {
        public RuntimePerformanceStats runtimeStats;
        public SceneComplexityReporter complexityReporter;
        public PerformanceBudgetLibrary budgetLibrary;
        public QuestBuildReadinessReporter questReporter;
        public PcBuildReadinessReporter pcReporter;
        public bool visible = true;

        GUIStyle style;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                visible = !visible;
        }

        public void Initialize(RuntimePerformanceStats stats, SceneComplexityReporter complexity, PerformanceBudgetLibrary budgets, QuestBuildReadinessReporter quest, PcBuildReadinessReporter pc)
        {
            runtimeStats = stats;
            complexityReporter = complexity;
            budgetLibrary = budgets;
            questReporter = quest;
            pcReporter = pc;
        }

        void OnGUI()
        {
            if (!visible)
                return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box);
                style.alignment = TextAnchor.UpperLeft;
                style.fontSize = 13;
                style.normal.textColor = Color.white;
            }

            var profile = budgetLibrary == null ? null : budgetLibrary.GetProfile("quest");
            var fps = runtimeStats == null ? 0f : runtimeStats.SmoothedFps;
            var frameMs = runtimeStats == null ? 0f : runtimeStats.AverageFrameMs;
            var actors = runtimeStats == null ? 0 : runtimeStats.ActorSnapshotCount;
            var projectiles = runtimeStats == null ? 0 : runtimeStats.ProjectileVisualCount;
            var markers = runtimeStats == null ? 0 : runtimeStats.FeedbackMarkerCount;
            var pool = runtimeStats == null ? 0 : runtimeStats.PoolInactiveCount;
            var objects = complexityReporter == null ? 0 : complexityReporter.gameObjectCount;
            var renderers = complexityReporter == null ? 0 : complexityReporter.activeRendererCount;
            var budget = runtimeStats == null || runtimeStats.IsWithinBudget(profile) ? "OK" : "WARN";

            GUI.Box(new Rect(12f, 190f, 330f, 150f),
                "Stage 15 Performance\n" +
                "FPS " + fps.ToString("0.0") + "  Avg " + frameMs.ToString("0.00") + "ms  Budget " + budget + "\n" +
                "Actors " + actors + "  Projectiles " + projectiles + "  Feedback " + markers + "\n" +
                "Pool inactive " + pool + "\n" +
                "Scene objects " + objects + "  Renderers " + renderers + "\n" +
                "Quest " + (questReporter != null && questReporter.reportGenerated ? questReporter.summary : "pending") + "\n" +
                "PC " + (pcReporter != null && pcReporter.reportGenerated ? pcReporter.summary : "pending"),
                style);
        }
    }
}
