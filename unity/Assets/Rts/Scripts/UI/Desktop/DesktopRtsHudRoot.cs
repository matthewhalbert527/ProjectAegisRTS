using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.Scenario;
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
        public VerticalSliceProgressTracker progressTracker;
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
            sidebarController.Initialize(driver, commandRouter, categoryTabs, productionGrid, productionQueue, placementPanel, selectionPanel, minimap, progressTracker);
            categoryTabs.Initialize(productionGrid, statusLog);
            productionGrid.Initialize(driver, commandRouter, categoryTabs, productionGridColumns, progressTracker);
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
            if (progressTracker == null)
                progressTracker = FindAnyObjectByType<VerticalSliceProgressTracker>();
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
                sidebarController = CreatePanel("Right Sidebar", typeof(DesktopSidebarController), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-sidebarWidth, 0f), Vector2.zero).GetComponent<DesktopSidebarController>();
            if (categoryTabs == null)
                categoryTabs = CreatePanel("Production Tabs", typeof(ProductionCategoryTabs), SidebarPanel(220f, 34f)).GetComponent<ProductionCategoryTabs>();
            if (productionGrid == null)
                productionGrid = CreatePanel("Production Grid", typeof(ProductionGridController), SidebarPanel(260f, 255f)).GetComponent<ProductionGridController>();
            if (productionQueue == null)
                productionQueue = CreatePanel("Production Queue", typeof(ProductionQueuePanel), SidebarPanel(520f, 120f)).GetComponent<ProductionQueuePanel>();
            if (placementPanel == null)
                placementPanel = CreatePanel("Placement Panel", typeof(PlacementModePanel), SidebarPanel(650f, 84f)).GetComponent<PlacementModePanel>();
            if (selectionPanel == null)
                selectionPanel = CreatePanel("Selection Panel", typeof(SelectionPanelController), SidebarPanel(742f, 150f)).GetComponent<SelectionPanelController>();
            if (minimap == null)
                minimap = CreatePanel("Minimap", typeof(MinimapPlaceholderController), SidebarPanel(56f, minimapSize)).GetComponent<MinimapPlaceholderController>();
            if (commandBar == null)
                commandBar = CreatePanel("Command Bar", typeof(CommandBarController), new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(-sidebarWidth, bottomCommandBarHeight)).GetComponent<CommandBarController>();
            if (statusLog == null)
                statusLog = CreatePanel("Status Log", typeof(RtsStatusLog), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, bottomCommandBarHeight + 8f), new Vector2(-sidebarWidth - 12f, bottomCommandBarHeight + 102f)).GetComponent<RtsStatusLog>();

            ApplyPanelLayouts();
            SetStatusLogVisible(showDebugOverlay);
        }

        GameObject CreatePanel(string objectName, System.Type componentType, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = new GameObject(objectName);
            obj.transform.SetParent(transform, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            obj.AddComponent(componentType);
            return obj;
        }

        GameObject CreatePanel(string objectName, System.Type componentType, SidebarPanelRect panel)
        {
            return CreatePanel(objectName, componentType, panel.AnchorMin, panel.AnchorMax, panel.OffsetMin, panel.OffsetMax);
        }

        SidebarPanelRect SidebarPanel(float top, float height)
        {
            return new SidebarPanelRect(
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-sidebarWidth + 14f, -top - height),
                new Vector2(-14f, -top));
        }

        void SetStatusLogVisible(bool visible)
        {
            if (statusLog == null)
                return;

            statusLog.visible = visible;
            statusLog.gameObject.SetActive(visible);
        }

        void ApplyPanelLayouts()
        {
            ApplyPanelLayout(sidebarController, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-sidebarWidth, 0f), Vector2.zero);
            ApplyPanelLayout(categoryTabs, SidebarPanel(220f, 34f));
            ApplyPanelLayout(productionGrid, SidebarPanel(260f, 255f));
            ApplyPanelLayout(productionQueue, SidebarPanel(520f, 120f));
            ApplyPanelLayout(placementPanel, SidebarPanel(650f, 84f));
            ApplyPanelLayout(selectionPanel, SidebarPanel(742f, 150f));
            ApplyPanelLayout(minimap, SidebarPanel(56f, minimapSize));
            ApplyPanelLayout(commandBar, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(-sidebarWidth, bottomCommandBarHeight));
            ApplyPanelLayout(statusLog, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, bottomCommandBarHeight + 8f), new Vector2(-sidebarWidth - 12f, bottomCommandBarHeight + 102f));
        }

        static void ApplyPanelLayout(Component component, SidebarPanelRect panel)
        {
            ApplyPanelLayout(component, panel.AnchorMin, panel.AnchorMax, panel.OffsetMin, panel.OffsetMax);
        }

        static void ApplyPanelLayout(Component component, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (component == null)
                return;

            var rect = component.GetComponent<RectTransform>();
            if (rect == null)
                rect = component.gameObject.AddComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        readonly struct SidebarPanelRect
        {
            public readonly Vector2 AnchorMin;
            public readonly Vector2 AnchorMax;
            public readonly Vector2 OffsetMin;
            public readonly Vector2 OffsetMax;

            public SidebarPanelRect(Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
            {
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                OffsetMin = offsetMin;
                OffsetMax = offsetMax;
            }
        }
    }
}
