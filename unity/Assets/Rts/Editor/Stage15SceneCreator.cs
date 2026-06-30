using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.Performance;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage15SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage15_PerformanceBuildReadiness.unity";

        [MenuItem("ProjectAegisRTS/Stage 15/Create Performance Build Readiness Scene")]
        public static void CreateStage15SceneMenu()
        {
            CreateOrUpdateStage15Scene();
        }

        public static void CreateStage15SceneBatch()
        {
            try
            {
                CreateOrUpdateStage15Scene();
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

        public static void CreateOrUpdateStage15Scene()
        {
            if (System.IO.File.Exists(Stage14SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage14SceneCreator.ScenePath);
            else
                Stage14SceneCreator.CreateOrUpdateStage14Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var profiles = Stage15PerformanceProfileAssetCreator.CreateOrUpdatePerformanceProfiles();
            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseCombatDemoWorld = true;
            driver.UseEconomyDemoWorld = false;
            driver.UseFogRadarDemoWorld = false;
            driver.UseAiSkirmishDemoWorld = false;
            driver.UseMapTerrainDemoWorld = false;
            driver.UsePlayerPerspectiveSnapshot = false;

            var pool = GetOrAdd<ObjectPoolService>(game);
            var budgetLibrary = GetOrAdd<PerformanceBudgetLibrary>(game);
            var stats = GetOrAdd<RuntimePerformanceStats>(game);
            var complexity = GetOrAdd<SceneComplexityReporter>(game);
            var quality = GetOrAdd<QualityProfileApplier>(game);
            var quest = GetOrAdd<QuestBuildReadinessReporter>(game);
            var pc = GetOrAdd<PcBuildReadinessReporter>(game);
            var hud = GetOrAdd<RenderStatsHud>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);
            var projectileRenderer = UnityEngine.Object.FindFirstObjectByType<ProjectileRenderSystem>();
            var vfx = UnityEngine.Object.FindFirstObjectByType<VfxFeedbackController>();

            budgetLibrary.profiles = profiles;
            budgetLibrary.defaultProfile = profiles.Length > 0 ? profiles[0] : null;
            budgetLibrary.EnsureInitialized();

            quality.budgetLibrary = budgetLibrary;
            quality.selectedProfileId = "quest";
            quality.applyOnStart = true;
            quality.applyQualitySettingsInEditMode = false;

            if (projectileRenderer != null)
                projectileRenderer.objectPoolService = pool;
            if (vfx != null)
                vfx.objectPoolService = pool;

            stats.driver = driver;
            stats.projectileRenderSystem = projectileRenderer;
            stats.vfxFeedbackController = vfx;
            stats.objectPoolService = pool;
            quest.budgetLibrary = budgetLibrary;
            quest.runtimeStats = stats;
            quest.complexityReporter = complexity;
            quest.qualityProfileApplier = quality;
            pc.budgetLibrary = budgetLibrary;
            pc.runtimeStats = stats;
            pc.complexityReporter = complexity;
            hud.runtimeStats = stats;
            hud.complexityReporter = complexity;
            hud.budgetLibrary = budgetLibrary;
            hud.questReporter = quest;
            hud.pcReporter = pc;
            hud.visible = true;

            bootstrapper.simulationDriver = driver;
            bootstrapper.objectPoolService = pool;
            bootstrapper.performanceBudgetLibrary = budgetLibrary;
            bootstrapper.runtimePerformanceStats = stats;
            bootstrapper.sceneComplexityReporter = complexity;
            bootstrapper.qualityProfileApplier = quality;
            bootstrapper.questBuildReadinessReporter = quest;
            bootstrapper.pcBuildReadinessReporter = pc;
            bootstrapper.renderStatsHud = hud;
            bootstrapper.projectileRenderSystem = projectileRenderer;
            bootstrapper.vfxFeedbackController = vfx;
            bootstrapper.startPaused = false;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 15 scene at " + ScenePath);
        }

        static void ConfigureCamera(Camera camera)
        {
            if (camera == null)
                return;
            camera.orthographic = true;
            camera.orthographicSize = 28f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = new Vector3(16f, 38f, -26f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        }

        static void UpdateBuildScenes()
        {
            var paths = new[]
            {
                "Assets/Rts/Scenes/Stage1_DesktopBoard.unity",
                "Assets/Rts/Scenes/Stage2_PCSidebar.unity",
                "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity",
                "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity",
                "Assets/Rts/Scenes/Stage5_DualHandCommand.unity",
                "Assets/Rts/Scenes/Stage6_MovementVisualization.unity",
                Stage7SceneCreator.ScenePath,
                Stage8SceneCreator.ScenePath,
                Stage9SceneCreator.ScenePath,
                Stage10SceneCreator.ScenePath,
                Stage11SceneCreator.ScenePath,
                Stage12SceneCreator.ScenePath,
                Stage13SceneCreator.ScenePath,
                Stage14SceneCreator.ScenePath,
                ScenePath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < paths.Length; i++)
                if (System.IO.File.Exists(paths[i]))
                    scenes.Add(new EditorBuildSettingsScene(paths[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static GameObject RequireObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
            return obj;
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
