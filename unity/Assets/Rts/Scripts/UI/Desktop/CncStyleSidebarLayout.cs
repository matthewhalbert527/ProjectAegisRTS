using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class CncStyleSidebarLayout : MonoBehaviour
    {
        public float width = 392f;
        public float innerPadding = 12f;
        public float minimapSize = 216f;

        public RectTransform rightSidebarRoot;
        public DesktopSidebarController resourcePowerPanel;
        public ProductionCategoryTabs categoryTabs;
        public ProductionGridController productionGrid;
        public ProductionQueuePanel productionQueue;
        public SupportPowerPanelController supportPowerPanel;
        public PlacementModePanel placementPanel;
        public SelectionPanelController selectionPanel;
        public CommandBarController commandBar;
        public MinimapPlaceholderController minimap;

        public bool IsConfigured { get; private set; }

        public void Initialize(
            DesktopRtsHudRoot hudRoot,
            DesktopSidebarController sidebar,
            ProductionCategoryTabs tabs,
            ProductionGridController grid,
            ProductionQueuePanel queue,
            PlacementModePanel placement,
            SelectionPanelController selection,
            CommandBarController commands,
            MinimapPlaceholderController map)
        {
            Initialize(hudRoot, sidebar, tabs, grid, queue, null, placement, selection, commands, map);
        }

        public void Initialize(
            DesktopRtsHudRoot hudRoot,
            DesktopSidebarController sidebar,
            ProductionCategoryTabs tabs,
            ProductionGridController grid,
            ProductionQueuePanel queue,
            SupportPowerPanelController support,
            PlacementModePanel placement,
            SelectionPanelController selection,
            CommandBarController commands,
            MinimapPlaceholderController map)
        {
            if (hudRoot != null)
            {
                width = Mathf.Clamp(hudRoot.sidebarWidth, 360f, 420f);
                minimapSize = Mathf.Clamp(hudRoot.minimapSize < 190f ? 216f : hudRoot.minimapSize, 216f, 300f);
            }

            resourcePowerPanel = sidebar;
            categoryTabs = tabs;
            productionGrid = grid;
            productionQueue = queue;
            supportPowerPanel = support;
            placementPanel = placement;
            selectionPanel = selection;
            commandBar = commands;
            minimap = map;

            if (resourcePowerPanel != null)
                rightSidebarRoot = resourcePowerPanel.GetComponent<RectTransform>();

            ApplyLayout();
        }

        public void ApplyLayout()
        {
            if (rightSidebarRoot == null)
                return;

            rightSidebarRoot.gameObject.SetActive(true);
            rightSidebarRoot.SetParent(transform, false);
            rightSidebarRoot.anchorMin = new Vector2(1f, 0f);
            rightSidebarRoot.anchorMax = new Vector2(1f, 1f);
            rightSidebarRoot.pivot = new Vector2(1f, 0.5f);
            rightSidebarRoot.offsetMin = new Vector2(-width, 0f);
            rightSidebarRoot.offsetMax = Vector2.zero;
            RtsUiFactory.AddPanel(rightSidebarRoot.gameObject, new Color(0.035f, 0.043f, 0.052f, 0.96f));

            ParentToSidebar(minimap);
            ParentToSidebar(categoryTabs);
            ParentToSidebar(productionGrid);
            ParentToSidebar(productionQueue);
            ParentToSidebar(supportPowerPanel);
            ParentToSidebar(placementPanel);
            ParentToSidebar(selectionPanel);
            ParentToSidebar(commandBar);

            if (resourcePowerPanel != null)
                resourcePowerPanel.ApplyCncReadoutLayout(266f, 74f);

            ApplyTopPanel(minimap, 42f, minimapSize);
            ApplyTopPanel(supportPowerPanel, 342f, 40f);
            ApplyTopPanel(categoryTabs, 388f, 62f);
            ApplyTopPanel(productionGrid, 458f, 236f);
            ApplyTopPanel(productionQueue, 702f, 88f);
            ApplyTopPanel(placementPanel, 798f, 80f);
            ApplyTopPanel(selectionPanel, 886f, 72f);
            ApplyTopPanel(commandBar, 966f, 104f);

            ConfigureGrid(productionGrid);
            ConfigureTabs(categoryTabs);
            IsConfigured = true;
        }

        public bool IsMinimapAboveProductionGrid()
        {
            var minimapRect = minimap == null ? null : minimap.GetComponent<RectTransform>();
            var gridRect = productionGrid == null ? null : productionGrid.GetComponent<RectTransform>();
            if (minimapRect == null || gridRect == null)
                return false;

            return minimapRect.offsetMax.y > gridRect.offsetMax.y;
        }

        public bool AreProductionPanelsInRightSidebar()
        {
            if (rightSidebarRoot == null)
                return false;

            return IsChildOfSidebar(minimap) &&
                IsChildOfSidebar(categoryTabs) &&
                IsChildOfSidebar(productionGrid) &&
                IsChildOfSidebar(productionQueue) &&
                IsChildOfSidebar(supportPowerPanel) &&
                IsChildOfSidebar(selectionPanel) &&
                IsChildOfSidebar(commandBar);
        }

        void ParentToSidebar(Component component)
        {
            if (component == null || rightSidebarRoot == null)
                return;

            var componentTransform = component.transform;
            if (componentTransform.parent != rightSidebarRoot)
                componentTransform.SetParent(rightSidebarRoot, false);
        }

        bool IsChildOfSidebar(Component component)
        {
            return component != null && rightSidebarRoot != null && component.transform.parent == rightSidebarRoot;
        }

        void ApplyTopPanel(Component component, float top, float height)
        {
            if (component == null)
                return;

            var rect = component.GetComponent<RectTransform>();
            if (rect == null)
                rect = component.gameObject.AddComponent<RectTransform>();

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(innerPadding, -top - height);
            rect.offsetMax = new Vector2(-innerPadding, -top);
        }

        static void ConfigureGrid(ProductionGridController grid)
        {
            if (grid == null)
                return;

            var layout = grid.GetComponent<GridLayoutGroup>();
            if (layout == null)
                return;

            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 2;
            layout.cellSize = new Vector2(178f, 74f);
            layout.spacing = new Vector2(8f, 7f);
            layout.padding = new RectOffset(0, 0, 0, 0);
        }

        static void ConfigureTabs(ProductionCategoryTabs tabs)
        {
            if (tabs == null)
                return;

            var layout = tabs.GetComponent<GridLayoutGroup>();
            if (layout == null)
                return;

            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            layout.cellSize = new Vector2(116f, 28f);
            layout.spacing = new Vector2(5f, 5f);
            layout.padding = new RectOffset(0, 0, 0, 0);
        }
    }
}
