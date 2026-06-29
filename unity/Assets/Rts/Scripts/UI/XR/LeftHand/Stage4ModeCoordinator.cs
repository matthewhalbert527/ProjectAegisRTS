using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Selection;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.XR.LeftHand
{
    public sealed class Stage4ModeCoordinator : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public BoardRenderer boardRenderer;
        public DesktopLeftHandInputSource desktopInput;
        public XrLeftHandInputAdapter xrInput;
        public LeftHandBuildMenuController buildMenu;
        public LeftHandCommandRouter commandRouter;
        public LeftHandSelectionController selectionController;
        public LeftHandLassoSelectionController lassoController;
        public LeftHandPlacementPanel placementPanel;
        public LeftHandSelectionPanel selectionPanel;
        public LeftHandStatusHud statusHud;

        bool initialized;

        public LeftHandCommandMode CurrentMode { get; private set; }
        public string ActiveInputSourceName { get; private set; }

        void Start()
        {
            InitializeIfNeeded();
        }

        void Update()
        {
            InitializeIfNeeded();
            TickInput();
        }

        public void InitializeIfNeeded()
        {
            if (initialized)
                return;

            if (commandRouter != null)
                commandRouter.modeCoordinator = this;
            if (buildMenu != null)
                buildMenu.Initialize(driver, commandRouter, buildMenu.radialView);
            if (selectionController != null)
                selectionController.Initialize(driver, mapper, commandRouter);
            if (lassoController != null)
                lassoController.Initialize(driver, mapper, boardRenderer, commandRouter);

            CurrentMode = buildMenu != null && buildMenu.IsOpen ? LeftHandCommandMode.Idle : LeftHandCommandMode.Hidden;
            ActiveInputSourceName = "none";
            initialized = true;
        }

        public void SetMode(LeftHandCommandMode mode)
        {
            CurrentMode = mode;
        }

        public void CancelActiveMode()
        {
            if (lassoController != null && lassoController.IsActive)
                lassoController.CancelLasso();
            if (driver != null && driver.HasPlacementMode && commandRouter != null)
                commandRouter.CancelPlacement();
            else if (buildMenu != null && buildMenu.IsOpen)
                buildMenu.CloseMenu();
            else if (commandRouter != null)
                commandRouter.ClearSelection();

            if (boardRenderer != null)
                boardRenderer.ClearPlacementPreview();
            CurrentMode = buildMenu != null && buildMenu.IsOpen ? LeftHandCommandMode.Idle : LeftHandCommandMode.Hidden;
        }

        void TickInput()
        {
            var source = GetActiveInputSource();
            if (source == null)
            {
                ActiveInputSourceName = "none";
                CurrentMode = LeftHandCommandMode.Disabled;
                return;
            }

            ActiveInputSourceName = object.ReferenceEquals(source, desktopInput) ? "desktop fallback" : "XR adapter";

            var desktop = source as DesktopLeftHandInputSource;
            if (desktop != null && desktop.GetStatusHudToggleDown() && statusHud != null)
                statusHud.Toggle();

            if (source.GetMenuToggleDown() && buildMenu != null)
            {
                buildMenu.ToggleMenu();
                CurrentMode = buildMenu.IsOpen ? LeftHandCommandMode.CategorySelect : LeftHandCommandMode.Hidden;
            }

            if (source.GetCancelDown())
            {
                CancelActiveMode();
                return;
            }

            HandleCategoryAndItemInput(source, desktop);

            Ray ray = default(Ray);
            Int2 cell = Int2.Zero;
            Vector3 hit = Vector3.zero;
            var hasCell = source.TryGetRay(out ray) && mapper != null && mapper.TryRayToCell(ray, out cell, out hit);
            if (driver != null)
            {
                if (hasCell)
                    driver.SetHoveredCell(cell);
                else
                    driver.ClearHoveredCell();
            }

            if (boardRenderer != null)
                boardRenderer.SetHoveredCell(hasCell ? (Int2?)cell : null);

            if (driver != null && driver.HasPlacementMode)
            {
                CurrentMode = LeftHandCommandMode.Placement;
                PlacementPreviewSnapshot preview;
                if (placementPanel != null)
                    placementPanel.RefreshPreview(hasCell ? (Int2?)cell : null, driver.TryGetPlacementPreview(out preview) ? preview : null);
                if (source.GetPrimarySelectDown() && commandRouter != null)
                    commandRouter.ConfirmPlacementAtHoveredCell();
                return;
            }

            if (lassoController != null && hasCell && source.GetLassoModifierHeld())
            {
                CurrentMode = LeftHandCommandMode.SelectionLasso;
                if (source.GetPrimarySelectDown())
                    lassoController.StartLasso(cell);
                else if (source.GetPrimarySelectHeld() && lassoController.IsActive)
                    lassoController.UpdateLasso(cell);
                else if (source.GetPrimarySelectUp() && lassoController.IsActive)
                    lassoController.CompleteLasso(source.GetSecondaryModifierHeld());
                return;
            }

            if (boardRenderer != null)
                boardRenderer.ClearPlacementPreview();

            if (selectionController != null)
            {
                selectionController.RefreshCandidates(ray, hasCell ? (Int2?)cell : null);
                if (source.GetCycleCandidateNextDown())
                    selectionController.CycleCandidate(1);
                if (source.GetCycleCandidatePreviousDown())
                    selectionController.CycleCandidate(-1);
                if (source.GetPrimarySelectDown())
                    selectionController.SelectCurrentCandidate(source.GetSecondaryModifierHeld());

                if (selectionController.Candidates.Count > 1)
                    CurrentMode = LeftHandCommandMode.AmbiguousSelection;
                else if (selectionController.Candidates.Count == 1)
                    CurrentMode = LeftHandCommandMode.SelectionRay;
                else if (buildMenu != null && buildMenu.IsOpen)
                    CurrentMode = LeftHandCommandMode.BuildItemSelect;
                else
                    CurrentMode = LeftHandCommandMode.Idle;
            }
        }

        void HandleCategoryAndItemInput(ILeftHandInputSource source, DesktopLeftHandInputSource desktop)
        {
            if (buildMenu == null)
                return;

            if (desktop != null)
            {
                var categoryHotkey = desktop.GetCategoryHotkeyDown();
                if (categoryHotkey > 0)
                    buildMenu.SetCategory((LeftHandBuildCategory)(categoryHotkey - 1));

                if (desktop.LastNumberKeyDown > 0)
                    buildMenu.QueueItemByIndex(desktop.LastNumberKeyDown);
            }

            if (source.GetCategoryNextDown() || source.GetCategoryAxis() > 0.5f)
                buildMenu.SelectNextCategory();
            if (source.GetCategoryPreviousDown() || source.GetCategoryAxis() < -0.5f)
                buildMenu.SelectPreviousCategory();
        }

        ILeftHandInputSource GetActiveInputSource()
        {
            if (xrInput != null && xrInput.IsAvailable)
                return xrInput;
            if (desktopInput != null && desktopInput.IsAvailable)
                return desktopInput;
            return null;
        }
    }
}
