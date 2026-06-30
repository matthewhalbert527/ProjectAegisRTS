using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class PcBuildReadinessReporter : MonoBehaviour
    {
        public PerformanceBudgetLibrary budgetLibrary;
        public RuntimePerformanceStats runtimeStats;
        public SceneComplexityReporter complexityReporter;

        public bool reportGenerated;
        public bool withinRuntimeBudget;
        public bool withinSceneBudget;
        public string summary;

        public void Initialize(PerformanceBudgetLibrary library, RuntimePerformanceStats stats, SceneComplexityReporter complexity)
        {
            budgetLibrary = library;
            runtimeStats = stats;
            complexityReporter = complexity;
            RefreshReport();
        }

        public void RefreshReport()
        {
            var profile = budgetLibrary == null ? null : budgetLibrary.GetProfile("pc");
            if (runtimeStats != null)
                runtimeStats.RefreshSnapshotCounts();
            if (complexityReporter != null)
                complexityReporter.Refresh();

            withinRuntimeBudget = runtimeStats == null || runtimeStats.IsWithinBudget(profile);
            withinSceneBudget = complexityReporter == null || complexityReporter.IsWithinBudget(profile);
            reportGenerated = true;
            summary = "PC readiness: " + (withinRuntimeBudget && withinSceneBudget ? "within placeholder budgets" : "budget warning") +
                ". Standalone build configuration is audited by editor validation.";
        }
    }
}
