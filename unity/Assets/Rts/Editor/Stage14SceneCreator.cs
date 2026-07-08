using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage14SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage14_FeedbackPolish.unity";

        [MenuItem("ProjectAegisRTS/Stage 14/Create Feedback Polish Scene")]
        public static void CreateStage14SceneMenu()
        {
            CreateOrUpdateStage14Scene();
        }

        public static void CreateStage14SceneBatch()
        {
            try
            {
                CreateOrUpdateStage14Scene();
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

        public static void CreateOrUpdateStage14Scene()
        {
            if (System.IO.File.Exists(Stage13SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage13SceneCreator.ScenePath);
            else
                Stage13SceneCreator.CreateOrUpdateStage13Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var profiles = Stage14FeedbackProfileAssetCreator.CreateOrUpdateFeedbackProfiles();
            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseCombatDemoWorld = true;
            driver.UseEconomyDemoWorld = false;
            driver.UseFogRadarDemoWorld = false;
            driver.UseAiSkirmishDemoWorld = false;
            driver.UseMapTerrainDemoWorld = false;
            driver.UsePlayerPerspectiveSnapshot = false;

            var profileLibrary = GetOrAdd<FeedbackProfileLibrary>(game);
            var bus = GetOrAdd<FeedbackEventBus>(game);
            var audio = GetOrAdd<AudioFeedbackController>(game);
            var vfx = GetOrAdd<VfxFeedbackController>(game);
            var ui = GetOrAdd<UiFeedbackController>(game);
            var haptic = GetOrAdd<HapticFeedbackAdapter>(game);
            var hud = GetOrAdd<FeedbackDebugHud>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);
            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();

            profileLibrary.profiles = profiles;
            profileLibrary.defaultProfile = profiles.Length > 0 ? profiles[0] : null;
            profileLibrary.EnsureInitialized();

            bus.driver = driver;
            bus.mapper = mapper;
            audio.eventBus = bus;
            audio.profileLibrary = profileLibrary;
            vfx.eventBus = bus;
            vfx.profileLibrary = profileLibrary;
            ui.eventBus = bus;
            haptic.eventBus = bus;
            haptic.profileLibrary = profileLibrary;
            hud.eventBus = bus;
            hud.profileLibrary = profileLibrary;
            hud.audioController = audio;
            hud.vfxController = vfx;
            hud.uiController = ui;
            hud.hapticAdapter = haptic;
            hud.visible = true;

            bootstrapper.simulationDriver = driver;
            bootstrapper.feedbackProfileLibrary = profileLibrary;
            bootstrapper.feedbackEventBus = bus;
            bootstrapper.audioFeedbackController = audio;
            bootstrapper.vfxFeedbackController = vfx;
            bootstrapper.uiFeedbackController = ui;
            bootstrapper.hapticFeedbackAdapter = haptic;
            bootstrapper.feedbackDebugHud = hud;
            bootstrapper.startPaused = false;

            driver.feedbackEventBus = bus;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 14 scene at " + ScenePath);
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
