using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.Performance;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage15SceneValidator
    {
        public static void ValidateStage15SceneBatch()
        {
            try
            {
                ValidateStage15Scene();
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

        public static void ValidateStage15Scene()
        {
            if (!System.IO.File.Exists(Stage15SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 15 scene missing: " + Stage15SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage15SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 15 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            if (Camera.main == null && UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
                throw new InvalidOperationException("Stage 15 camera missing.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var pool = Require<ObjectPoolService>("ObjectPoolService");
            var budgetLibrary = Require<PerformanceBudgetLibrary>("PerformanceBudgetLibrary");
            var stats = Require<RuntimePerformanceStats>("RuntimePerformanceStats");
            var complexity = Require<SceneComplexityReporter>("SceneComplexityReporter");
            var quality = Require<QualityProfileApplier>("QualityProfileApplier");
            var quest = Require<QuestBuildReadinessReporter>("QuestBuildReadinessReporter");
            var pc = Require<PcBuildReadinessReporter>("PcBuildReadinessReporter");
            Require<RenderStatsHud>("RenderStatsHud");
            var projectileRenderer = Require<ProjectileRenderSystem>("ProjectileRenderSystem");
            var vfx = Require<VfxFeedbackController>("VfxFeedbackController");

            budgetLibrary.EnsureInitialized();
            if (budgetLibrary.ProfileCount < 2)
                throw new InvalidOperationException("Stage 15 budget library must include Quest and PC profiles.");
            if (projectileRenderer.objectPoolService != pool)
                throw new InvalidOperationException("Stage 15 projectile renderer is not wired to the object pool.");
            if (vfx.objectPoolService != pool)
                throw new InvalidOperationException("Stage 15 feedback VFX controller is not wired to the object pool.");
            if (bootstrapper.objectPoolService != pool || bootstrapper.performanceBudgetLibrary != budgetLibrary)
                throw new InvalidOperationException("Stage 15 bootstrapper performance references are incomplete.");

            stats.Initialize(bootstrapper.simulationDriver, projectileRenderer, vfx, pool);
            complexity.Refresh();
            quality.Initialize(budgetLibrary);
            quest.Initialize(budgetLibrary, stats, complexity, quality);
            pc.Initialize(budgetLibrary, stats, complexity);

            if (!quest.reportGenerated || !pc.reportGenerated)
                throw new InvalidOperationException("Stage 15 readiness reports were not generated.");
            if (!System.IO.File.Exists(Stage14SceneCreator.ScenePath))
                throw new InvalidOperationException("Previous stage scene missing: " + Stage14SceneCreator.ScenePath);

            Debug.Log("Stage 15 scene validation passed.");
        }

        static void RequireObject(string name)
        {
            if (GameObject.Find(name) == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
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
