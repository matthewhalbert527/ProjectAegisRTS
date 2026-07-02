using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectAegisRTS.UnityClient.InputControls
{
    public sealed class RtsDesktopInputController : MonoBehaviour
    {
        Camera sceneCamera;
        BoardCoordinateMapper mapper;
        RtsSimulationDriver driver;
        RtsDebugHud debugHud;
        DesktopUiCommandRouter commandRouter;
        PauseMenuController pauseMenu;
        Int2 hoveredCell;
        bool hasHoveredCell;
        readonly Dictionary<int, List<int>> controlGroups = new Dictionary<int, List<int>>();
        Vector3 leftMouseDownPosition;
        Int2 leftMouseDownCell;
        bool leftMouseDownValid;
        float lastLeftClickTime = -1f;
        Int2 lastLeftClickCell;

        const float DoubleClickSeconds = 0.32f;
        const float MarqueePixelThreshold = 12f;

        public void Initialize(Camera cameraReference, BoardCoordinateMapper coordinateMapper, RtsSimulationDriver simulationDriver, RtsDebugHud hud)
        {
            sceneCamera = cameraReference;
            mapper = coordinateMapper;
            driver = simulationDriver;
            debugHud = hud;
        }

        public void SetCommandRouter(DesktopUiCommandRouter router)
        {
            commandRouter = router;
        }

        void Update()
        {
            if (sceneCamera == null || mapper == null || driver == null)
                return;

            if (pauseMenu == null)
                pauseMenu = FindAnyObjectByType<PauseMenuController>();
            if (pauseMenu != null && (pauseMenu.BlocksGameplayInput() || Input.GetKeyDown(KeyCode.Escape)))
            {
                driver.ClearHoveredCell();
                return;
            }

            UpdateHoveredCell();
            HandleKeyboard();
            HandleMouse();
        }

        void UpdateHoveredCell()
        {
            Vector3 hitPoint;
            var ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
            var usePlacementGrid = driver.HasPlacementMode;
            var hasCell = usePlacementGrid
                ? mapper.TryRayToPlacementCell(ray, out hoveredCell, out hitPoint)
                : mapper.TryRayToCell(ray, out hoveredCell, out hitPoint);
            if (hasCell)
            {
                hasHoveredCell = true;
                driver.SetHoveredCell(hoveredCell, usePlacementGrid);
            }
            else
            {
                hasHoveredCell = false;
                driver.ClearHoveredCell();
            }
        }

        void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                leftMouseDownPosition = Input.mousePosition;
                leftMouseDownCell = hasHoveredCell ? CommandCell() : Int2.Zero;
                leftMouseDownValid = hasHoveredCell && !IsPointerOverUi();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!leftMouseDownValid)
                    return;

                var dragDistance = Vector2.Distance(Input.mousePosition, leftMouseDownPosition);
                if (dragDistance >= MarqueePixelThreshold && CanUseMarqueeSelection())
                {
                    var rect = ScreenRect(leftMouseDownPosition, Input.mousePosition);
                    Record(driver.TrySelectActorsInScreenRect(rect, sceneCamera, mapper, IsAdditiveSelection()));
                    leftMouseDownValid = false;
                    return;
                }

                if (!hasHoveredCell || IsPointerOverUi())
                {
                    leftMouseDownValid = false;
                    return;
                }

                HandleLeftClick(CommandCell());
                leftMouseDownValid = false;
            }

            if (Input.GetMouseButtonDown(1) && hasHoveredCell && !IsPointerOverUi())
            {
                if (commandRouter != null)
                    commandRouter.IssueMoveToCell(CommandCell());
                else
                    Record(driver.TryIssueMoveSelectedToCell(CommandCell()));
            }
        }

        void HandleLeftClick(Int2 cell)
        {
            if (commandRouter != null)
            {
                if (driver.HasPlacementMode)
                    commandRouter.PlaceAtHoveredCell();
                else if (commandRouter.CurrentMode == DesktopCommandMode.Move)
                    commandRouter.IssueMoveToCell(cell);
                else if (commandRouter.CurrentMode == DesktopCommandMode.AttackPlaceholder)
                    commandRouter.IssueAttackToCell(cell);
                else if (commandRouter.CurrentMode == DesktopCommandMode.AttackMove)
                    commandRouter.IssueAttackMoveToCell(cell);
                else if (commandRouter.CurrentMode == DesktopCommandMode.Patrol)
                    commandRouter.IssuePatrolToCell(cell);
                else if (IsDoubleClick(cell))
                    commandRouter.SelectSameTypeAtCell(cell);
                else
                    commandRouter.SelectAtCell(cell);
            }
            else
            {
                var result = driver.HasPlacementMode
                    ? driver.TryPlacePendingBuildingAtCell(hoveredCell)
                    : (IsDoubleClick(cell) ? driver.TrySelectOwnedActorsOfSameTypeAtCell(cell) : driver.TrySelectActorAtCell(cell));
                Record(result);
            }

            lastLeftClickTime = Time.unscaledTime;
            lastLeftClickCell = cell;
        }

        bool IsDoubleClick(Int2 cell)
        {
            return Time.unscaledTime - lastLeftClickTime <= DoubleClickSeconds && lastLeftClickCell.Equals(cell);
        }

        bool CanUseMarqueeSelection()
        {
            if (driver == null || driver.HasPlacementMode)
                return false;
            return commandRouter == null || commandRouter.CurrentMode == DesktopCommandMode.Normal;
        }

        static Rect ScreenRect(Vector3 start, Vector3 end)
        {
            var minX = Mathf.Min(start.x, end.x);
            var minY = Mathf.Min(start.y, end.y);
            var maxX = Mathf.Max(start.x, end.x);
            var maxY = Mathf.Max(start.y, end.y);
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        static bool IsAdditiveSelection()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        Int2 CommandCell()
        {
            return driver != null && driver.HoveredCellIsPlacementCell ? driver.HoveredCoarseCell : hoveredCell;
        }

        void HandleKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                RouteOrRecord(() => driver.TogglePause(), r => r.TogglePause());
            if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.N))
                RouteOrRecord(() => driver.StepOneTick(), r => r.StepTick());
            HandleControlGroups();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (commandRouter != null)
                    commandRouter.CancelActiveMode();
                else
                    Record(driver.HasPlacementMode ? driver.TryCancelPlacement() : driver.ClearSelection());
            }
            if (Input.GetKeyDown(KeyCode.P))
                Queue("power_plant");
            if (Input.GetKeyDown(KeyCode.B))
                Queue("barracks");
            if (Input.GetKeyDown(KeyCode.W))
                Queue("war_factory");
            if (Input.GetKeyDown(KeyCode.R))
                Queue("refinery");
            if (Input.GetKeyDown(KeyCode.G))
                Queue("gun_tower");
            if (Input.GetKeyDown(KeyCode.I))
                Queue("rifle_infantry");
            if (Input.GetKeyDown(KeyCode.T))
                Queue("light_tank");
            if (Input.GetKeyDown(KeyCode.H))
                Queue("harvester");
            if (Input.GetKeyDown(KeyCode.L))
                RouteOrRecord(() => driver.TryForceLowPowerOrCreateLowPowerDemoCondition(), r => r.TriggerLowPowerDemo());
        }

        void HandleControlGroups()
        {
            for (var i = 1; i <= 9; i++)
            {
                var key = (KeyCode)((int)KeyCode.Alpha0 + i);
                if (!Input.GetKeyDown(key))
                    continue;

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    AssignControlGroup(i);
                else
                    RecallControlGroup(i);
            }
        }

        void AssignControlGroup(int groupIndex)
        {
            var actorIds = new List<int>(driver.SelectedActorIds);
            controlGroups[groupIndex] = actorIds;
            if (commandRouter != null)
                commandRouter.NoteControlGroupAssigned(groupIndex, actorIds.Count);
            else
                Debug.Log("Control group " + groupIndex + " assigned (" + actorIds.Count + " actors).");
        }

        void RecallControlGroup(int groupIndex)
        {
            List<int> actorIds;
            if (!controlGroups.TryGetValue(groupIndex, out actorIds))
                actorIds = new List<int>();

            Record(driver.SetSelectedActorIds(actorIds));
            if (commandRouter != null)
                commandRouter.NoteControlGroupRecalled(groupIndex, actorIds.Count);
        }

        void Queue(string typeId)
        {
            if (commandRouter != null)
                commandRouter.QueueProduction(typeId);
            else
                Record(driver.TryQueueProduction(typeId));
        }

        void RouteOrRecord(System.Func<RtsCommandResult> fallback, System.Action<DesktopUiCommandRouter> routed)
        {
            if (commandRouter != null)
                routed(commandRouter);
            else
                Record(fallback());
        }

        void Record(RtsCommandResult result)
        {
            if (debugHud != null)
                debugHud.RecordCommandResult(result);
            else
                Debug.Log(result.ToString());
        }

        static bool IsPointerOverUi()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;

            return false;
        }
    }
}
