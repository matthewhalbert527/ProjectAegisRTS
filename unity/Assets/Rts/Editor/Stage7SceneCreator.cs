using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage7SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity";

        [MenuItem("ProjectAegisRTS/Create Stage 7 Building Power Production Scene")]
        public static void CreateStage7SceneMenu()
        {
            CreateOrUpdateStage7Scene();
        }

        public static void CreateStage7SceneBatch()
        {
            try
            {
                CreateOrUpdateStage7Scene();
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

        public static void CreateOrUpdateStage7Scene()
        {
            EnsureFolders();
            var buildingProfiles = Stage7BuildingProfileAssetCreator.CreateOrUpdateProfileAssets();

            if (System.IO.File.Exists(Stage6SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage6SceneCreator.ScenePath, OpenSceneMode.Single);
            else
                Stage6SceneCreator.CreateOrUpdateStage6Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            RequireObject("BoardRoot");
            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var actorRenderer = UnityEngine.Object.FindFirstObjectByType<ActorRenderSystem>();
            var bootstrapper = UnityEngine.Object.FindFirstObjectByType<RtsGameBootstrapper>();
            var driver = UnityEngine.Object.FindFirstObjectByType<RtsSimulationDriver>();
            var statusLog = UnityEngine.Object.FindFirstObjectByType<RtsStatusLog>();
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();

            ConfigureCamera(camera);
            EnsureCanvasName();

            var movementLibrary = UnityEngine.Object.FindFirstObjectByType<VisualMotionProfileLibrary>();
            if (actorRenderer != null && movementLibrary != null)
                actorRenderer.motionProfileLibrary = movementLibrary;

            var buildingLibrary = GetOrAdd<BuildingVisualProfileLibrary>(game);
            buildingLibrary.includeGeneratedDefaults = true;
            buildingLibrary.profiles = buildingProfiles;
            buildingLibrary.EnsureInitialized();

            if (actorRenderer != null)
                actorRenderer.buildingProfileLibrary = buildingLibrary;

            var demo = GetOrAdd<BuildingPowerDemoController>(game);
            demo.driver = driver;
            demo.actorRenderSystem = actorRenderer;
            demo.statusLog = statusLog;

            var hud = GetOrAdd<BuildingAnimationDebugHud>(game);
            hud.actorRenderSystem = actorRenderer;
            hud.demoController = demo;
            hud.visible = true;

            CreateShowcase(game.transform, buildingLibrary);

            if (bootstrapper != null)
                bootstrapper.actorRenderSystem = actorRenderer;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 7 scene at " + ScenePath);
        }

        static void CreateShowcase(Transform parent, BuildingVisualProfileLibrary library)
        {
            var root = GameObject.Find("Stage7 Placeholder Buildings");
            if (root == null)
                root = new GameObject("Stage7 Placeholder Buildings");
            root.transform.SetParent(parent, false);

            var ids = new[] { "power_plant", "barracks", "war_factory", "refinery", "gun_tower", "comm_center", "repair_bay", "dual_helipad" };
            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var child = root.transform.Find("Stage7 Showcase " + id);
                GameObject obj;
                if (child == null)
                {
                    obj = new GameObject("Stage7 Showcase " + id);
                    obj.transform.SetParent(root.transform, false);
                }
                else
                {
                    obj = child.gameObject;
                }

                obj.transform.localPosition = new Vector3(5f + (i % 4) * 2.4f, 0f, 18f + (i / 4) * 2.4f);
                var body = obj.transform.Find("Showcase Body");
                if (body == null)
                {
                    var primitive = GameObject.CreatePrimitive(id.Contains("tower") || id.Contains("turret") ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                    primitive.name = "Showcase Body";
                    primitive.transform.SetParent(obj.transform, false);
                    primitive.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                    primitive.transform.localScale = new Vector3(1.1f, 0.7f, 1.1f);
                    var collider = primitive.GetComponent<Collider>();
                    if (collider != null)
                        UnityEngine.Object.DestroyImmediate(collider);
                }

                var controller = GetOrAdd<BuildingVisualStateController>(obj);
                controller.standaloneTickInPlayMode = true;
                controller.Initialize(8000 + i, id, library.GetProfile(id, null));
                if (id == "barracks" || id == "war_factory")
                    controller.SetDebugForcedState(BuildingAnimationVisualState.Producing);
                else if (id == "refinery")
                    controller.SetDebugForcedState(BuildingAnimationVisualState.Damaged);
                else
                    controller.SetDebugForcedState(null);
            }
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

        static void EnsureCanvasName()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
                return;

            var stageCanvas = GameObject.Find("Stage4 Canvas");
            if (stageCanvas != null)
                stageCanvas.name = "Canvas";
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

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "Scenes");
            EnsureFolder("Assets/Rts", "ScriptableObjects");
            EnsureFolder("Assets/Rts/ScriptableObjects", "BuildingProfiles");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
