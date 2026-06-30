using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI;
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
        Int2 hoveredCell;
        bool hasHoveredCell;

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

            UpdateHoveredCell();
            HandleKeyboard();
            HandleMouse();
        }

        void UpdateHoveredCell()
        {
            Vector3 hitPoint;
            var ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
            if (mapper.TryRayToCell(ray, out hoveredCell, out hitPoint))
            {
                hasHoveredCell = true;
                driver.SetHoveredCell(hoveredCell);
            }
            else
            {
                hasHoveredCell = false;
                driver.ClearHoveredCell();
            }
        }

        void HandleMouse()
        {
            if (!hasHoveredCell || IsPointerOverUi())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (commandRouter != null)
                {
                    if (driver.HasPlacementMode)
                        commandRouter.PlaceAtHoveredCell();
                    else if (commandRouter.CurrentMode == DesktopCommandMode.Move)
                        commandRouter.IssueMoveToCell(hoveredCell);
                    else if (commandRouter.CurrentMode == DesktopCommandMode.AttackPlaceholder)
                        commandRouter.IssueAttackToCell(hoveredCell);
                    else
                        commandRouter.SelectAtCell(hoveredCell);
                }
                else
                {
                    var result = driver.HasPlacementMode
                        ? driver.TryPlacePendingBuildingAtCell(hoveredCell)
                        : driver.TrySelectActorAtCell(hoveredCell);
                    Record(result);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (commandRouter != null)
                    commandRouter.IssueMoveToCell(hoveredCell);
                else
                    Record(driver.TryIssueMoveSelectedToCell(hoveredCell));
            }
        }

        void HandleKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                RouteOrRecord(() => driver.TogglePause(), r => r.TogglePause());
            if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.N))
                RouteOrRecord(() => driver.StepOneTick(), r => r.StepTick());
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
