using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage10SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity";

        [MenuItem("ProjectAegisRTS/Stage 10/Create Economy Harvesting Scene")]
        public static void CreateStage10SceneMenu()
        {
            CreateOrUpdateStage10Scene();
        }

        public static void CreateStage10SceneBatch()
        {
            try
            {
                CreateOrUpdateStage10Scene();
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

        public static void CreateOrUpdateStage10Scene()
        {
            if (System.IO.File.Exists(Stage9SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage9SceneCreator.ScenePath);
            else
                Stage9SceneCreator.CreateOrUpdateStage9Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            RequireObject("BoardRoot");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseEconomyDemoWorld = true;
            driver.UseCombatDemoWorld = false;

            var resourceRenderer = GetOrAdd<ResourceFieldRenderSystem>(game);
            var cargoRenderer = GetOrAdd<HarvesterCargoVisualController>(game);
            var dockRenderer = GetOrAdd<RefineryDockVisualController>(game);
            var eventRenderer = GetOrAdd<EconomyEventRenderSystem>(game);
            var hud = GetOrAdd<EconomyDebugHud>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);
            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var actorRenderer = UnityEngine.Object.FindFirstObjectByType<ActorRenderSystem>();
            var combatProfiles = UnityEngine.Object.FindFirstObjectByType<CombatVisualProfileLibrary>();
            var projectileRenderer = UnityEngine.Object.FindFirstObjectByType<ProjectileRenderSystem>();
            var combatEventRenderer = UnityEngine.Object.FindFirstObjectByType<CombatEventRenderSystem>();

            resourceRenderer.driver = driver;
            resourceRenderer.mapper = mapper;
            cargoRenderer.driver = driver;
            cargoRenderer.mapper = mapper;
            dockRenderer.driver = driver;
            dockRenderer.mapper = mapper;
            eventRenderer.driver = driver;
            eventRenderer.mapper = mapper;
            hud.driver = driver;
            hud.resourceFieldRenderSystem = resourceRenderer;
            hud.harvesterCargoVisualController = cargoRenderer;
            hud.refineryDockVisualController = dockRenderer;
            hud.economyEventRenderSystem = eventRenderer;
            hud.visible = true;

            bootstrapper.simulationDriver = driver;
            bootstrapper.actorRenderSystem = actorRenderer;
            bootstrapper.combatVisualProfileLibrary = combatProfiles;
            bootstrapper.projectileRenderSystem = projectileRenderer;
            bootstrapper.combatEventRenderSystem = combatEventRenderer;
            bootstrapper.resourceFieldRenderSystem = resourceRenderer;
            bootstrapper.harvesterCargoVisualController = cargoRenderer;
            bootstrapper.refineryDockVisualController = dockRenderer;
            bootstrapper.economyEventRenderSystem = eventRenderer;
            bootstrapper.startPaused = false;

            var visualLibrary = UnityEngine.Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            var resolver = UnityEngine.Object.FindFirstObjectByType<ActorVisualPrefabResolver>();
            if (actorRenderer != null)
            {
                actorRenderer.actorVisualDefinitionLibrary = visualLibrary;
                actorRenderer.actorVisualPrefabResolver = resolver;
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 10 scene at " + ScenePath);
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
