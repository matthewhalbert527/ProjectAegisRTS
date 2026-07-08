using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage5SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage5_DualHandCommand.unity";

        [MenuItem("ProjectAegisRTS/Create Stage 5 Dual-Hand Command Scene")]
        public static void CreateStage5SceneMenu()
        {
            CreateOrUpdateStage5Scene();
        }

        public static void CreateStage5SceneBatch()
        {
            try
            {
                CreateOrUpdateStage5Scene();
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

        public static void CreateOrUpdateStage5Scene()
        {
            EnsureFolders();
            if (System.IO.File.Exists(Stage4SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage4SceneCreator.ScenePath, OpenSceneMode.Single);
            else
                Stage4SceneCreator.CreateOrUpdateStage4Scene();

            var scene = EditorSceneManager.GetActiveScene();

            var game = GameObject.Find("RtsGame");
            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var driver = UnityEngine.Object.FindFirstObjectByType<RtsSimulationDriver>();
            var boardRenderer = UnityEngine.Object.FindFirstObjectByType<BoardRenderer>();
            var boardPlacement = UnityEngine.Object.FindFirstObjectByType<BoardPlacementController>();
            var leftCoordinator = UnityEngine.Object.FindFirstObjectByType<Stage4ModeCoordinator>();
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var canvas = GameObject.Find("Stage4 Canvas");
            var statusLog = UnityEngine.Object.FindFirstObjectByType<RtsStatusLog>();

            var preview = game.AddComponent<CommandPreviewRenderer>();
            preview.mapper = mapper;

            var rigObject = new GameObject("Simulated Right Hand Rig");
            var rightRig = rigObject.AddComponent<SimulatedRightHandRig>();
            rightRig.sceneCamera = camera;
            rightRig.EnsureRig();

            var wristCanvasObject = new GameObject("Right Hand Wrist Canvas");
            wristCanvasObject.transform.SetParent(rightRig.uiAnchor, false);
            var wristRect = wristCanvasObject.AddComponent<RectTransform>();
            wristRect.sizeDelta = new Vector2(360f, 260f);
            wristCanvasObject.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
            var wristCanvas = wristCanvasObject.AddComponent<Canvas>();
            wristCanvas.renderMode = RenderMode.WorldSpace;
            wristCanvas.worldCamera = camera;
            wristCanvasObject.AddComponent<GraphicRaycaster>();

            var controllerObject = new GameObject("Stage5 Right Hand Controllers");
            var desktopInput = controllerObject.AddComponent<DesktopRightHandInputSource>();
            desktopInput.sceneCamera = camera;
            var xrInput = controllerObject.AddComponent<XrRightHandInputAdapter>();
            xrInput.rightRayOrigin = rightRig.rightRayOrigin;
            xrInput.adapterEnabled = false;

            var router = controllerObject.AddComponent<RightHandCommandRouter>();
            var coordinator = controllerObject.AddComponent<Stage5DualHandModeCoordinator>();
            var reticle = controllerObject.AddComponent<RightHandCommandReticle>();

            var hudObject = CreateRectChild(canvas.transform, "Right Hand Command HUD", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-432f, 158f), new Vector2(-12f, 338f));
            var hud = hudObject.AddComponent<RightHandCommandHud>();

            var statusObject = CreateRectChild(canvas.transform, "Right Hand Status Panel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-432f, 344f), new Vector2(-12f, 482f));
            var statusPanel = statusObject.AddComponent<RightHandStatusPanel>();

            router.driver = driver;
            router.statusLog = statusLog;
            router.previewRenderer = preview;

            reticle.previewRenderer = preview;

            hud.driver = driver;
            hud.coordinator = coordinator;
            hud.commandRouter = router;

            statusPanel.coordinator = coordinator;
            statusPanel.commandRouter = router;
            statusPanel.boardPlacement = boardPlacement;

            coordinator.driver = driver;
            coordinator.mapper = mapper;
            coordinator.boardRenderer = boardRenderer;
            coordinator.boardPlacement = boardPlacement;
            coordinator.leftHandCoordinator = leftCoordinator;
            coordinator.desktopInput = desktopInput;
            coordinator.xrInput = xrInput;
            coordinator.commandRouter = router;
            coordinator.commandHud = hud;
            coordinator.commandReticle = reticle;
            coordinator.statusPanel = statusPanel;

            var bootstrapper = UnityEngine.Object.FindFirstObjectByType<RtsGameBootstrapper>();
            if (bootstrapper != null)
                bootstrapper.boardRenderer = boardRenderer;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 5 scene at " + ScenePath);
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
                "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity",
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
            EnsureFolder("Assets/Rts/Scripts/UI/XR", "RightHand");
            EnsureFolder("Assets/Rts/Scripts/Input", "Desktop");
            EnsureFolder("Assets/Rts/Scripts/Input", "XR");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
