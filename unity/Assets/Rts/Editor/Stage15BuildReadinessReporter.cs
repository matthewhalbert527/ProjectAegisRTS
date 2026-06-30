using System;
using ProjectAegisRTS.UnityClient.Performance;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage15BuildReadinessReporter
    {
        public static void RunStage15BuildReadinessReportBatch()
        {
            try
            {
                RunStage15BuildReadinessReport();
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        [MenuItem("ProjectAegisRTS/Stage 15/Run Build Readiness Report")]
        public static void RunStage15BuildReadinessReport()
        {
            Stage15SceneValidator.ValidateStage15Scene();
            var scene = EditorSceneManager.OpenScene(Stage15SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 15 scene did not open for build readiness.");

            var budgets = Require<PerformanceBudgetLibrary>("PerformanceBudgetLibrary");
            var stats = Require<RuntimePerformanceStats>("RuntimePerformanceStats");
            var complexity = Require<SceneComplexityReporter>("SceneComplexityReporter");
            var quality = Require<QualityProfileApplier>("QualityProfileApplier");
            var quest = Require<QuestBuildReadinessReporter>("QuestBuildReadinessReporter");
            var pc = Require<PcBuildReadinessReporter>("PcBuildReadinessReporter");

            budgets.EnsureInitialized();
            stats.RefreshSnapshotCounts();
            complexity.Refresh();
            quality.Initialize(budgets);
            quest.Initialize(budgets, stats, complexity, quality);
            pc.Initialize(budgets, stats, complexity);

            var stageSceneIncluded = false;
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var buildScene = EditorBuildSettings.scenes[i];
                if (buildScene.enabled && buildScene.path == Stage15SceneCreator.ScenePath)
                    stageSceneIncluded = true;
            }

            if (!stageSceneIncluded)
                throw new InvalidOperationException("Stage 15 scene is not enabled in EditorBuildSettings.");
            if (!quest.reportGenerated || !pc.reportGenerated)
                throw new InvalidOperationException("Stage 15 readiness reporters did not generate reports.");

            Debug.Log("Stage 15 build readiness report passed. Active build target: " + EditorUserBuildSettings.activeBuildTarget + ". Android/Quest module availability is advisory; no APK or PC player was produced.");
        }

        static T Require<T>(string label) where T : Component
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            return component;
        }
    }
}
