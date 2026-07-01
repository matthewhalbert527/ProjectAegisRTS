using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.XR.RightHand
{
    public sealed class Stage5DualHandModeCoordinator : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public BoardRenderer boardRenderer;
        public BoardPlacementController boardPlacement;
        public Stage4ModeCoordinator leftHandCoordinator;
        public DesktopRightHandInputSource desktopInput;
        public XrRightHandInputAdapter xrInput;
        public RightHandCommandRouter commandRouter;
        public RightHandCommandHud commandHud;
        public RightHandCommandReticle commandReticle;
        public RightHandStatusPanel statusPanel;

        bool initialized;
        bool hudVisible = true;

        public RightHandCommandMode CurrentMode { get; private set; }
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
                commandRouter.Initialize(driver, commandRouter.statusLog, commandRouter.previewRenderer);
            if (commandHud != null)
                commandHud.visible = hudVisible;
            CurrentMode = RightHandCommandMode.Idle;
            ActiveInputSourceName = "none";
            initialized = true;
        }

        public void SetMode(RightHandCommandMode mode)
        {
            CurrentMode = mode;
            if (commandRouter != null)
            {
                if (mode == RightHandCommandMode.Move)
                    commandRouter.EnterMoveMode();
                else if (mode == RightHandCommandMode.Attack)
                    commandRouter.EnterAttackMode();
                else if (mode == RightHandCommandMode.ForceAttack)
                    commandRouter.EnterForceAttackMode();
                else if (mode == RightHandCommandMode.BoardManipulation && commandRouter.CurrentMode != RightHandCommandMode.BoardManipulation)
                    commandRouter.ToggleBoardManipulationMode();
            }
        }

        public void CancelActiveMode()
        {
            CurrentMode = RightHandCommandMode.Idle;
            if (commandRouter != null)
                commandRouter.CancelCommandMode();
            if (commandReticle != null)
                commandReticle.Clear();
        }

        void TickInput()
        {
            var source = GetActiveInputSource();
            if (source == null)
            {
                ActiveInputSourceName = "none";
                CurrentMode = RightHandCommandMode.Disabled;
                return;
            }

            ActiveInputSourceName = object.ReferenceEquals(source, desktopInput) ? "desktop fallback" : "XR adapter";

            if (source.GetHudToggleDown() && commandHud != null)
                commandHud.Toggle();
            if (source.GetCancelDown())
            {
                CancelActiveMode();
                return;
            }

            if (IsGameplaySuppressed())
            {
                CurrentMode = RightHandCommandMode.Disabled;
                return;
            }

            if (source.GetMoveModeDown())
                SetMode(RightHandCommandMode.Move);
            if (source.GetAttackModeDown())
                SetMode(RightHandCommandMode.Attack);
            if (source.GetForceAttackModeDown())
                SetMode(RightHandCommandMode.ForceAttack);
            if (source.GetBoardManipulationToggleDown() || source.GetBoardManipulationHeld())
                CurrentMode = RightHandCommandMode.BoardManipulation;
            else if (CurrentMode == RightHandCommandMode.BoardManipulation)
                CurrentMode = RightHandCommandMode.Idle;

            if (CurrentMode == RightHandCommandMode.BoardManipulation)
            {
                TickBoardManipulation(source);
                return;
            }

            Ray ray = default(Ray);
            Int2 cell = Int2.Zero;
            Vector3 hit = Vector3.zero;
            var usePlacementGrid = driver != null && driver.HasPlacementMode;
            var hasCell = source.TryGetRay(out ray) &&
                mapper != null &&
                (usePlacementGrid ? mapper.TryRayToPlacementCell(ray, out cell, out hit) : mapper.TryRayToCell(ray, out cell, out hit));
            if (driver != null)
            {
                if (hasCell)
                    driver.SetHoveredCell(cell, usePlacementGrid);
                else
                    driver.ClearHoveredCell();
            }

            if (boardRenderer != null)
                boardRenderer.SetHoveredCell(hasCell ? (Int2?)cell : null, usePlacementGrid);
            if (commandReticle != null)
            {
                if (hasCell)
                    commandReticle.ShowHover(cell, CurrentMode);
                else
                    commandReticle.Clear();
            }

            if (source.GetPrimaryCommandDown() && hasCell && commandRouter != null)
            {
                if (CurrentMode == RightHandCommandMode.Attack)
                    commandRouter.IssueAttackPlaceholderAtCell(cell, false);
                else if (CurrentMode == RightHandCommandMode.ForceAttack)
                    commandRouter.IssueAttackPlaceholderAtCell(cell, true);
                else
                    commandRouter.IssueContextCommandAtCell(cell);
            }
        }

        public bool IsGameplaySuppressed()
        {
            if (driver != null && driver.HasPlacementMode)
                return true;
            if (boardPlacement != null && boardPlacement.IsPlacementModeActive)
                return true;
            return false;
        }

        void TickBoardManipulation(IRightHandInputSource source)
        {
            if (boardPlacement == null)
                return;

            boardPlacement.Initialize();
            var rotate = source.GetRotateAxis();
            if (Mathf.Abs(rotate) > 0.01f)
                boardPlacement.AdjustYaw(rotate * boardPlacement.yawStepDegrees * Time.deltaTime);

            var zoom = source.GetZoomAxis();
            if (Mathf.Abs(zoom) > 0.01f)
                boardPlacement.AdjustScale(zoom * boardPlacement.scaleStep);
        }

        IRightHandInputSource GetActiveInputSource()
        {
            if (xrInput != null && xrInput.IsAvailable)
                return xrInput;
            if (desktopInput != null && desktopInput.IsAvailable)
                return desktopInput;
            return null;
        }
    }
}
