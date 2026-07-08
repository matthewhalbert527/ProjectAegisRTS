using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI;
using ProjectAegisRTS.UnityClient.UI.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage3SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity";

        [MenuItem("ProjectAegisRTS/Create Stage 3 XR Board Placement Scene")]
        public static void CreateStage3SceneMenu()
        {
            CreateOrUpdateStage3Scene();
        }

        public static void CreateStage3SceneBatch()
        {
            try
            {
                CreateOrUpdateStage3Scene();
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

        public static void CreateOrUpdateStage3Scene()
        {
            EnsureFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage3_XRBoardPlacement";

            var boardRoot = new GameObject("BoardRoot");
            var mapper = boardRoot.AddComponent<BoardCoordinateMapper>();
            var boardRenderer = boardRoot.AddComponent<BoardRenderer>();

            var game = new GameObject("RtsGame");
            var bootstrapper = game.AddComponent<RtsGameBootstrapper>();
            var driver = game.AddComponent<RtsSimulationDriver>();
            var actorRenderer = game.AddComponent<ActorRenderSystem>();
            var rtsInput = game.AddComponent<RtsDesktopInputController>();
            var debugHud = game.AddComponent<RtsDebugHud>();
            debugHud.visible = false;

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 30f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            cameraObject.transform.position = new Vector3(16f, 40f, -30f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            var cameraController = cameraObject.AddComponent<RtsCameraController>();
            cameraController.orthographicSize = 30f;

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var canvasObject = new GameObject("Stage3 Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var placementObject = new GameObject("Stage3 Board Placement");
            var placement = placementObject.AddComponent<BoardPlacementController>();
            placement.boardRoot = boardRoot.transform;
            placement.coordinateMapper = mapper;

            var desktopPlacementInput = placementObject.AddComponent<DesktopBoardPlacementInput>();
            desktopPlacementInput.controller = placement;
            desktopPlacementInput.sceneCamera = camera;
            desktopPlacementInput.disableWhilePlacementActive = new Behaviour[] { cameraController };

            var xrAdapter = placementObject.AddComponent<XrBoardPlacementInputAdapter>();
            xrAdapter.controller = placement;

            var rigObject = new GameObject("Stage3 XR Rig Placeholder");
            var rig = rigObject.AddComponent<Stage3XrRigPlaceholder>();
            rig.fallbackCamera = camera;

            var hudObject = CreateRectChild(canvasObject.transform, "Stage3 Board Placement HUD", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -420f), new Vector2(390f, -12f));
            var hud = hudObject.AddComponent<BoardPlacementHud>();
            hud.controller = placement;
            hud.recenterReference = camera.transform;

            bootstrapper.boardRoot = boardRoot.transform;
            bootstrapper.coordinateMapper = mapper;
            bootstrapper.boardRenderer = boardRenderer;
            bootstrapper.actorRenderSystem = actorRenderer;
            bootstrapper.simulationDriver = driver;
            bootstrapper.inputController = rtsInput;
            bootstrapper.debugHud = debugHud;
            bootstrapper.sceneCamera = camera;
            bootstrapper.cameraController = cameraController;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 3 scene at " + ScenePath);
        }

        static GameObject CreateRectChild(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return obj;
        }

        static void UpdateBuildScenes()
        {
            var paths = new[]
            {
                "Assets/Rts/Scenes/Stage1_DesktopBoard.unity",
                "Assets/Rts/Scenes/Stage2_PCSidebar.unity",
                ScenePath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < paths.Length; i++)
                if (System.IO.File.Exists(paths[i]))
                    scenes.Add(new EditorBuildSettingsScene(paths[i], true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "Scenes");
            EnsureFolder("Assets/Rts/Scripts", "Board");
            EnsureFolder("Assets/Rts/Scripts/Input", "Desktop");
            EnsureFolder("Assets/Rts/Scripts/Input", "XR");
            EnsureFolder("Assets/Rts/Scripts/UI", "XR");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
