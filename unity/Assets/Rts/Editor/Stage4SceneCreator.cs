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
using ProjectAegisRTS.UnityClient.Selection;
using ProjectAegisRTS.UnityClient.UI;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.XR;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage4SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity";

        [MenuItem("ProjectAegisRTS/Create Stage 4 Left-Hand Build Selection Scene")]
        public static void CreateStage4SceneMenu()
        {
            CreateOrUpdateStage4Scene();
        }

        public static void CreateStage4SceneBatch()
        {
            try
            {
                CreateOrUpdateStage4Scene();
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

        public static void CreateOrUpdateStage4Scene()
        {
            EnsureFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage4_LeftHandBuildSelection";

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
            camera.orthographicSize = 28f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            cameraObject.transform.position = new Vector3(16f, 38f, -26f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            var cameraController = cameraObject.AddComponent<RtsCameraController>();
            cameraController.orthographicSize = 28f;

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var screenCanvasObject = new GameObject("Stage4 Canvas");
            var screenCanvas = screenCanvasObject.AddComponent<Canvas>();
            screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            screenCanvasObject.AddComponent<GraphicRaycaster>();
            var screenScaler = screenCanvasObject.AddComponent<CanvasScaler>();
            screenScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            screenScaler.referenceResolution = new Vector2(1920f, 1080f);
            screenScaler.matchWidthOrHeight = 0.5f;

            var rigObject = new GameObject("Simulated Left Hand Rig");
            var simulatedRig = rigObject.AddComponent<SimulatedLeftHandRig>();
            simulatedRig.sceneCamera = camera;
            simulatedRig.EnsureRig();

            var wristCanvasObject = new GameObject("Left Hand Wrist Canvas");
            wristCanvasObject.transform.SetParent(simulatedRig.uiAnchor, false);
            var wristRect = wristCanvasObject.AddComponent<RectTransform>();
            wristRect.sizeDelta = new Vector2(390f, 360f);
            wristCanvasObject.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
            var wristCanvas = wristCanvasObject.AddComponent<Canvas>();
            wristCanvas.renderMode = RenderMode.WorldSpace;
            wristCanvas.worldCamera = camera;
            wristCanvasObject.AddComponent<GraphicRaycaster>();

            var controllerObject = new GameObject("Stage4 Left Hand Controllers");
            var desktopInput = controllerObject.AddComponent<DesktopLeftHandInputSource>();
            desktopInput.sceneCamera = camera;
            var xrInput = controllerObject.AddComponent<XrLeftHandInputAdapter>();
            xrInput.leftRayOrigin = simulatedRig.leftRayOrigin;
            xrInput.adapterEnabled = false;

            var router = controllerObject.AddComponent<LeftHandCommandRouter>();
            var buildMenu = controllerObject.AddComponent<LeftHandBuildMenuController>();
            var selectionController = controllerObject.AddComponent<LeftHandSelectionController>();
            var lassoController = controllerObject.AddComponent<LeftHandLassoSelectionController>();
            var coordinator = controllerObject.AddComponent<Stage4ModeCoordinator>();

            var radialObject = CreateRectChild(wristCanvasObject.transform, "Left Hand Radial Menu", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var radialView = radialObject.AddComponent<LeftHandRadialMenuView>();
            radialView.followTarget = simulatedRig.uiAnchor;

            var placementObject = CreateRectChild(screenCanvasObject.transform, "Left Hand Placement Panel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(12f, 206f), new Vector2(392f, 336f));
            var placementPanel = placementObject.AddComponent<LeftHandPlacementPanel>();

            var selectionObject = CreateRectChild(screenCanvasObject.transform, "Left Hand Selection Panel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(12f, 344f), new Vector2(392f, 528f));
            var selectionPanel = selectionObject.AddComponent<LeftHandSelectionPanel>();

            var statusObject = CreateRectChild(screenCanvasObject.transform, "Stage4 Status HUD", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(12f, 12f), new Vector2(392f, 198f));
            var statusHud = statusObject.AddComponent<LeftHandStatusHud>();

            var statusLogObject = CreateRectChild(screenCanvasObject.transform, "Stage4 Status Log", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-432f, 12f), new Vector2(-12f, 150f));
            var statusLog = statusLogObject.AddComponent<RtsStatusLog>();

            var placementCompatObject = new GameObject("Stage4 Board Placement Compatibility");
            var boardPlacement = placementCompatObject.AddComponent<BoardPlacementController>();
            boardPlacement.boardRoot = boardRoot.transform;
            boardPlacement.coordinateMapper = mapper;
            var boardPlacementHudObject = CreateRectChild(screenCanvasObject.transform, "Stage4 Board Placement HUD", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -216f), new Vector2(392f, -12f));
            var boardPlacementHud = boardPlacementHudObject.AddComponent<BoardPlacementHud>();
            boardPlacementHud.controller = boardPlacement;
            boardPlacementHud.recenterReference = camera.transform;

            bootstrapper.boardRoot = boardRoot.transform;
            bootstrapper.coordinateMapper = mapper;
            bootstrapper.boardRenderer = boardRenderer;
            bootstrapper.actorRenderSystem = actorRenderer;
            bootstrapper.simulationDriver = driver;
            bootstrapper.inputController = rtsInput;
            bootstrapper.debugHud = debugHud;
            bootstrapper.sceneCamera = camera;
            bootstrapper.cameraController = cameraController;

            router.driver = driver;
            router.statusLog = statusLog;
            router.buildMenu = buildMenu;
            router.selectionController = selectionController;
            router.modeCoordinator = coordinator;

            buildMenu.driver = driver;
            buildMenu.commandRouter = router;
            buildMenu.radialView = radialView;
            radialView.menuController = buildMenu;

            selectionController.driver = driver;
            selectionController.mapper = mapper;
            selectionController.commandRouter = router;
            selectionController.statusLog = statusLog;

            lassoController.driver = driver;
            lassoController.mapper = mapper;
            lassoController.boardRenderer = boardRenderer;
            lassoController.commandRouter = router;

            placementPanel.driver = driver;
            placementPanel.commandRouter = router;
            placementPanel.boardRenderer = boardRenderer;

            selectionPanel.driver = driver;
            selectionPanel.selectionController = selectionController;

            statusHud.driver = driver;
            statusHud.modeCoordinator = coordinator;
            statusHud.buildMenu = buildMenu;
            statusHud.selectionController = selectionController;
            statusHud.commandRouter = router;

            coordinator.driver = driver;
            coordinator.mapper = mapper;
            coordinator.boardRenderer = boardRenderer;
            coordinator.desktopInput = desktopInput;
            coordinator.xrInput = xrInput;
            coordinator.buildMenu = buildMenu;
            coordinator.commandRouter = router;
            coordinator.selectionController = selectionController;
            coordinator.lassoController = lassoController;
            coordinator.placementPanel = placementPanel;
            coordinator.selectionPanel = selectionPanel;
            coordinator.statusHud = statusHud;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 4 scene at " + ScenePath);
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
                "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity",
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
            EnsureFolder("Assets/Rts/Scripts/UI", "XR");
            EnsureFolder("Assets/Rts/Scripts/UI/XR", "LeftHand");
            EnsureFolder("Assets/Rts/Scripts/Input", "Desktop");
            EnsureFolder("Assets/Rts/Scripts/Input", "XR");
            EnsureFolder("Assets/Rts/Scripts", "Selection");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
