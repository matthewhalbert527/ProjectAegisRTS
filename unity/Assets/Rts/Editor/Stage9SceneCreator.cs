using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage9SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity";

        [MenuItem("ProjectAegisRTS/Stage 9/Create Combat Weapons Damage Scene")]
        public static void CreateStage9SceneMenu()
        {
            CreateOrUpdateStage9Scene();
        }

        public static void CreateStage9SceneBatch()
        {
            try
            {
                CreateOrUpdateStage9Scene();
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

        public static void CreateOrUpdateStage9Scene()
        {
            var profiles = Stage9CombatProfileAssetCreator.CreateOrUpdateProfiles();
            if (System.IO.File.Exists(Stage8SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage8SceneCreator.ScenePath);
            else
                Stage8SceneCreator.CreateOrUpdateStage8Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            RequireObject("BoardRoot");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseCombatDemoWorld = true;

            var profileLibrary = GetOrAdd<CombatVisualProfileLibrary>(game);
            profileLibrary.profiles = profiles;
            profileLibrary.RebuildLookup();

            var projectileRenderer = GetOrAdd<ProjectileRenderSystem>(game);
            var eventRenderer = GetOrAdd<CombatEventRenderSystem>(game);
            var hud = GetOrAdd<CombatDebugHud>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);
            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var actorRenderer = UnityEngine.Object.FindFirstObjectByType<ActorRenderSystem>();
            var statusLog = UnityEngine.Object.FindFirstObjectByType<RtsStatusLog>();
            var preview = UnityEngine.Object.FindFirstObjectByType<CommandPreviewRenderer>();

            projectileRenderer.driver = driver;
            projectileRenderer.mapper = mapper;
            projectileRenderer.profileLibrary = profileLibrary;
            eventRenderer.driver = driver;
            eventRenderer.mapper = mapper;
            eventRenderer.profileLibrary = profileLibrary;
            hud.driver = driver;
            hud.projectileRenderSystem = projectileRenderer;
            hud.combatEventRenderSystem = eventRenderer;
            hud.visible = true;

            bootstrapper.simulationDriver = driver;
            bootstrapper.actorRenderSystem = actorRenderer;
            bootstrapper.combatVisualProfileLibrary = profileLibrary;
            bootstrapper.projectileRenderSystem = projectileRenderer;
            bootstrapper.combatEventRenderSystem = eventRenderer;
            bootstrapper.startPaused = false;

            var visualLibrary = UnityEngine.Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            var resolver = UnityEngine.Object.FindFirstObjectByType<ActorVisualPrefabResolver>();
            if (actorRenderer != null)
            {
                actorRenderer.actorVisualDefinitionLibrary = visualLibrary;
                actorRenderer.actorVisualPrefabResolver = resolver;
            }

            if (statusLog == null)
                game.AddComponent<RtsStatusLog>();
            if (preview == null)
                game.AddComponent<CommandPreviewRenderer>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 9 scene at " + ScenePath);
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
