using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class DesktopRtsHudRoot : MonoBehaviour
    {
        public float sidebarWidth = 380f;
        public float bottomCommandBarHeight = 92f;
        public float minimapSize = 158f;
        public int productionGridColumns = 2;
        public bool showDebugOverlay;

        public RtsGameBootstrapper bootstrapper;
        public RtsSimulationDriver driver;
        public Canvas canvas;
        public DesktopUiCommandRouter commandRouter;
        public DesktopSidebarController sidebarController;
        public ProductionCategoryTabs categoryTabs;
        public ProductionGridController productionGrid;
        public ProductionQueuePanel productionQueue;
        public PlacementModePanel placementPanel;
        public SelectionPanelController selectionPanel;
        public CommandBarController commandBar;
        public MinimapPlaceholderController minimap;
        public RtsStatusLog statusLog;

        bool initialized;

        void Start()
        {
            Initialize();
        }

        void Update()
        {
            if (!initialized)
                Initialize();
        }

        public void Initialize()
        {
            EnsureSceneReferences();
            EnsureUiReferences();

            if (driver == null)
                return;

            commandRouter.Initialize(driver, statusLog);
            sidebarController.Initialize(driver, commandRouter, categoryTabs, productionGrid, productionQueue, placementPanel, selectionPanel, minimap);
            categoryTabs.Initialize(productionGrid, statusLog);
            productionGrid.Initialize(driver, commandRouter, categoryTabs, productionGridColumns);
            productionQueue.Initialize(driver, commandRouter);
            placementPanel.Initialize(driver, commandRouter);
            selectionPanel.Initialize(driver, commandRouter);
            commandBar.Initialize(driver, commandRouter);
            minimap.Initialize(driver);

            var input = FindAnyObjectByType<RtsDesktopInputController>();
            if (input != null)
                input.SetCommandRouter(commandRouter);

            var debugHud = FindAnyObjectByType<ProjectAegisRTS.UnityClient.UI.RtsDebugHud>();
            if (debugHud != null)
                debugHud.visible = showDebugOverlay;

            initialized = true;
        }

        void EnsureSceneReferences()
        {
            if (bootstrapper == null)
                bootstrapper = FindAnyObjectByType<RtsGameBootstrapper>();
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
            if (EventSystem.current == null && FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }

        void EnsureUiReferences()
        {
            if (commandRouter == null)
                commandRouter = GetComponent<DesktopUiCommandRouter>();
            if (commandRouter == null)
                commandRouter = gameObject.AddComponent<DesktopUiCommandRouter>();

            if (statusLog == null)
                statusLog = GetComponentInChildren<RtsStatusLog>(true);
            if (sidebarController == null)
                sidebarController = GetComponentInChildren<DesktopSidebarController>(true);
            if (categoryTabs == null)
                categoryTabs = GetComponentInChildren<ProductionCategoryTabs>(true);
            if (productionGrid == null)
                productionGrid = GetComponentInChildren<ProductionGridController>(true);
            if (productionQueue == null)
                productionQueue = GetComponentInChildren<ProductionQueuePanel>(true);
            if (placementPanel == null)
                placementPanel = GetComponentInChildren<PlacementModePanel>(true);
            if (selectionPanel == null)
                selectionPanel = GetComponentInChildren<SelectionPanelController>(true);
            if (commandBar == null)
                commandBar = GetComponentInChildren<CommandBarController>(true);
            if (minimap == null)
                minimap = GetComponentInChildren<MinimapPlaceholderController>(true);

            if (sidebarController == null)
                sidebarController = CreatePanel("Right Sidebar", typeof(DesktopSidebarController), AnchorRight(sidebarWidth)).GetComponent<DesktopSidebarController>();
            if (categoryTabs == null)
                categoryTabs = CreatePanel("Production Tabs", typeof(ProductionCategoryTabs), InsideSidebar(220f, 34f)).GetComponent<ProductionCategoryTabs>();
            if (productionGrid == null)
                productionGrid = CreatePanel("Production Grid", typeof(ProductionGridController), InsideSidebar(260f, 255f)).GetComponent<ProductionGridController>();
            if (productionQueue == null)
                productionQueue = CreatePanel("Production Queue", typeof(ProductionQueuePanel), InsideSidebar(520f, 120f)).GetComponent<ProductionQueuePanel>();
            if (placementPanel == null)
                placementPanel = CreatePanel("Placement Panel", typeof(PlacementModePanel), InsideSidebar(650f, 84f)).GetComponent<PlacementModePanel>();
            if (selectionPanel == null)
                selectionPanel = CreatePanel("Selection Panel", typeof(SelectionPanelController), InsideSidebar(742f, 150f)).GetComponent<SelectionPanelController>();
            if (minimap == null)
                minimap = CreatePanel("Minimap", typeof(MinimapPlaceholderController), InsideSidebar(56f, minimapSize)).GetComponent<MinimapPlaceholderController>();
            if (commandBar == null)
                commandBar = CreatePanel("Command Bar", typeof(CommandBarController), AnchorBottom()).GetComponent<CommandBarController>();
            if (statusLog == null)
                statusLog = CreatePanel("Status Log", typeof(RtsStatusLog), AnchorStatus()).GetComponent<RtsStatusLog>();
        }

        GameObject CreatePanel(string objectName, System.Type componentType, Rect rect)
        {
            var obj = new GameObject(objectName);
            obj.transform.SetParent(transform, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(rect.xMin, rect.yMin);
            rt.anchorMax = new Vector2(rect.xMax, rect.yMax);
            rt.offsetMin = new Vector2(rect.width, rect.height);
            rt.offsetMax = Vector2.zero;
            obj.AddComponent(componentType);
            return obj;
        }

        Rect AnchorRight(float width)
        {
            return new Rect(1f, 0f, -width, 0f);
        }

        Rect AnchorBottom()
        {
            return new Rect(0f, 0f, -sidebarWidth, bottomCommandBarHeight);
        }

        Rect AnchorStatus()
        {
            return new Rect(0f, 0f, -sidebarWidth, bottomCommandBarHeight + 4f);
        }

        Rect InsideSidebar(float top, float height)
        {
            return new Rect(1f, 1f, -sidebarWidth + 14f, -top - height);
        }
    }
}
