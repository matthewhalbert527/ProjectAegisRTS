using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage2SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage2_PCSidebar.unity";
        const float SidebarWidth = 380f;

        [MenuItem("ProjectAegisRTS/Create Stage 2 PC Sidebar Scene")]
        public static void CreateStage2SceneMenu()
        {
            CreateOrUpdateStage2Scene();
        }

        public static void CreateStage2SceneBatch()
        {
            try
            {
                CreateOrUpdateStage2Scene();
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

        public static void CreateOrUpdateStage2Scene()
        {
            EnsureFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage2_PCSidebar";

            var boardRoot = new GameObject("BoardRoot");
            var mapper = boardRoot.AddComponent<BoardCoordinateMapper>();
            var boardRenderer = boardRoot.AddComponent<BoardRenderer>();

            var game = new GameObject("RtsGame");
            var bootstrapper = game.AddComponent<RtsGameBootstrapper>();
            var driver = game.AddComponent<RtsSimulationDriver>();
            var actorRenderer = game.AddComponent<ActorRenderSystem>();
            var input = game.AddComponent<RtsDesktopInputController>();
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

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var canvasObject = new GameObject("Stage2 Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var hudObject = CreateRectChild(canvasObject.transform, "DesktopRtsHudRoot", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var hudRoot = hudObject.AddComponent<DesktopRtsHudRoot>();
            var router = hudObject.AddComponent<DesktopUiCommandRouter>();

            var sidebarObject = CreateRightSidebar(hudObject.transform);
            var sidebar = sidebarObject.AddComponent<DesktopSidebarController>();
            var minimap = CreateSidebarSection(sidebarObject.transform, "Minimap", 56f, 158f).AddComponent<MinimapPlaceholderController>();
            var tabs = CreateSidebarSection(sidebarObject.transform, "Production Category Tabs", 220f, 64f).AddComponent<ProductionCategoryTabs>();
            var grid = CreateSidebarSection(sidebarObject.transform, "Production Grid", 292f, 220f).AddComponent<ProductionGridController>();
            var queue = CreateSidebarSection(sidebarObject.transform, "Production Queue", 522f, 130f).AddComponent<ProductionQueuePanel>();
            var placement = CreateSidebarSection(sidebarObject.transform, "Placement Mode Panel", 662f, 92f).AddComponent<PlacementModePanel>();
            var selection = CreateSidebarSection(sidebarObject.transform, "Selection Panel", 764f, 190f).AddComponent<SelectionPanelController>();

            var commandBarObject = CreateRectChild(hudObject.transform, "Command Bar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(-SidebarWidth, 92f));
            var commandBar = commandBarObject.AddComponent<CommandBarController>();

            var statusObject = CreateRectChild(hudObject.transform, "Status Log", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(10f, 98f), new Vector2(-SidebarWidth - 10f, 190f));
            var statusLog = statusObject.AddComponent<RtsStatusLog>();

            bootstrapper.boardRoot = boardRoot.transform;
            bootstrapper.coordinateMapper = mapper;
            bootstrapper.boardRenderer = boardRenderer;
            bootstrapper.actorRenderSystem = actorRenderer;
            bootstrapper.simulationDriver = driver;
            bootstrapper.inputController = input;
            bootstrapper.debugHud = debugHud;
            bootstrapper.sceneCamera = camera;
            bootstrapper.cameraController = cameraController;

            hudRoot.bootstrapper = bootstrapper;
            hudRoot.driver = driver;
            hudRoot.canvas = canvas;
            hudRoot.commandRouter = router;
            hudRoot.sidebarController = sidebar;
            hudRoot.categoryTabs = tabs;
            hudRoot.productionGrid = grid;
            hudRoot.productionQueue = queue;
            hudRoot.placementPanel = placement;
            hudRoot.selectionPanel = selection;
            hudRoot.commandBar = commandBar;
            hudRoot.minimap = minimap;
            hudRoot.statusLog = statusLog;
            hudRoot.showDebugOverlay = false;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 2 scene at " + ScenePath);
        }

        static GameObject CreateRightSidebar(Transform parent)
        {
            return CreateRectChild(parent, "Right Sidebar", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-SidebarWidth, 0f), Vector2.zero);
        }

        static GameObject CreateSidebarSection(Transform parent, string name, float top, float height)
        {
            return CreateRectChild(parent, name, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -top - height), new Vector2(-14f, -top));
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

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "Scenes");
            EnsureFolder("Assets/Rts/Scripts/UI", "Desktop");
            EnsureFolder("Assets/Rts/Scripts/UI", "Common");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
