using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class QuestBuildReadinessReporter : MonoBehaviour
    {
        public PerformanceBudgetLibrary budgetLibrary;
        public RuntimePerformanceStats runtimeStats;
        public SceneComplexityReporter complexityReporter;
        public QualityProfileApplier qualityProfileApplier;

        public bool reportGenerated;
        public bool withinRuntimeBudget;
        public bool withinSceneBudget;
        public string summary;

        public void Initialize(PerformanceBudgetLibrary library, RuntimePerformanceStats stats, SceneComplexityReporter complexity, QualityProfileApplier quality)
        {
            budgetLibrary = library;
            runtimeStats = stats;
            complexityReporter = complexity;
            qualityProfileApplier = quality;
            RefreshReport();
        }

        public void RefreshReport()
        {
            var profile = budgetLibrary == null ? null : budgetLibrary.GetProfile("quest");
            if (runtimeStats != null)
                runtimeStats.RefreshSnapshotCounts();
            if (complexityReporter != null)
                complexityReporter.Refresh();

            withinRuntimeBudget = runtimeStats == null || runtimeStats.IsWithinBudget(profile);
            withinSceneBudget = complexityReporter == null || complexityReporter.IsWithinBudget(profile);
            reportGenerated = true;
            summary = "Quest readiness: " + (withinRuntimeBudget && withinSceneBudget ? "within placeholder budgets" : "budget warning") +
                ". Android module is audited by editor validation when available; no physical Quest is required.";
        }
    }
}
