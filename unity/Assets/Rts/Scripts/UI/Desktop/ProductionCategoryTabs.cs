using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class ProductionCategoryTabs : MonoBehaviour
    {
        readonly DesktopProductionCategory[] categories =
        {
            DesktopProductionCategory.Buildings,
            DesktopProductionCategory.Defenses,
            DesktopProductionCategory.Infantry,
            DesktopProductionCategory.Vehicles,
            DesktopProductionCategory.Aircraft,
            DesktopProductionCategory.Support
        };

        Button[] buttons;
        ProductionGridController productionGrid;
        RtsStatusLog statusLog;

        public DesktopProductionCategory ActiveCategory { get; private set; }

        public void Initialize(ProductionGridController grid, RtsStatusLog log)
        {
            productionGrid = grid;
            statusLog = log;
            BuildIfNeeded();
            SetActiveCategory(DesktopProductionCategory.Buildings);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                SetActiveCategory(DesktopProductionCategory.Buildings);
            if (Input.GetKeyDown(KeyCode.F2))
                SetActiveCategory(DesktopProductionCategory.Defenses);
            if (Input.GetKeyDown(KeyCode.F3))
                SetActiveCategory(DesktopProductionCategory.Infantry);
            if (Input.GetKeyDown(KeyCode.F4))
                SetActiveCategory(DesktopProductionCategory.Vehicles);
            if (Input.GetKeyDown(KeyCode.F5))
                SetActiveCategory(DesktopProductionCategory.Aircraft);
            if (Input.GetKeyDown(KeyCode.F6))
                SetActiveCategory(DesktopProductionCategory.Support);
        }

        public void SetActiveCategory(DesktopProductionCategory category)
        {
            ActiveCategory = category;
            RefreshVisuals();
            if (productionGrid != null)
                productionGrid.SetCategory(category);
            if (statusLog != null)
                statusLog.AddInfo("Production tab: " + category);
        }

        void BuildIfNeeded()
        {
            if (buttons != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            var layout = EnsureSingleGridLayout();
            if (layout != null)
            {
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = 3;
                layout.spacing = new Vector2(4f, 4f);
                layout.cellSize = new Vector2(112f, 30f);
                layout.padding = new RectOffset(0, 0, 0, 0);
            }

            buttons = new Button[categories.Length];
            for (var i = 0; i < categories.Length; i++)
            {
                var category = categories[i];
                var button = GetOrCreateCategoryButton(category);
                var captured = category;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SetActiveCategory(captured));
                buttons[i] = button;
            }
        }

        GridLayoutGroup EnsureSingleGridLayout()
        {
            var layouts = GetComponents<LayoutGroup>();
            GridLayoutGroup grid = null;
            for (var i = 0; i < layouts.Length; i++)
            {
                var candidate = layouts[i] as GridLayoutGroup;
                if (candidate != null && grid == null)
                {
                    grid = candidate;
                    continue;
                }

                DestroyComponent(layouts[i]);
            }

            return grid != null ? grid : gameObject.AddComponent<GridLayoutGroup>();
        }

        static void DestroyComponent(Component component)
        {
            if (component == null)
                return;
            if (Application.isPlaying)
                Destroy(component);
            else
                DestroyImmediate(component);
        }

        Button GetOrCreateCategoryButton(DesktopProductionCategory category)
        {
            var child = transform.Find(category + " Tab");
            var button = child != null ? child.GetComponent<Button>() : null;
            return button != null ? button : RtsUiFactory.CreateButton(transform, category + " Tab", category.ToString());
        }

        void RefreshVisuals()
        {
            if (buttons == null)
                return;

            for (var i = 0; i < buttons.Length; i++)
            {
                var image = buttons[i].GetComponent<Image>();
                if (image != null)
                    image.color = categories[i] == ActiveCategory ? new Color(0.24f, 0.47f, 0.62f, 1f) : new Color(0.18f, 0.22f, 0.27f, 0.95f);
            }
        }
    }
}
