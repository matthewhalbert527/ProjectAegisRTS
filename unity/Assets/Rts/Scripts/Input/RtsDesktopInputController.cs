using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.InputControls
{
    public sealed class RtsDesktopInputController : MonoBehaviour
    {
        Camera sceneCamera;
        BoardCoordinateMapper mapper;
        RtsSimulationDriver driver;
        RtsDebugHud debugHud;
        Int2 hoveredCell;
        bool hasHoveredCell;

        public void Initialize(Camera cameraReference, BoardCoordinateMapper coordinateMapper, RtsSimulationDriver simulationDriver, RtsDebugHud hud)
        {
            sceneCamera = cameraReference;
            mapper = coordinateMapper;
            driver = simulationDriver;
            debugHud = hud;
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
            if (!hasHoveredCell || IsPointerLikelyOverHud())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                var result = driver.HasPlacementMode
                    ? driver.TryPlacePendingBuildingAtCell(hoveredCell)
                    : driver.TrySelectActorAtCell(hoveredCell);
                Record(result);
            }

            if (Input.GetMouseButtonDown(1))
                Record(driver.TryIssueMoveSelectedToCell(hoveredCell));
        }

        void HandleKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Record(driver.TogglePause());
            if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.N))
                Record(driver.StepOneTick());
            if (Input.GetKeyDown(KeyCode.Escape))
                Record(driver.HasPlacementMode ? driver.TryCancelPlacement() : driver.ClearSelection());
            if (Input.GetKeyDown(KeyCode.P))
                Record(driver.TryQueueProduction("power_plant"));
            if (Input.GetKeyDown(KeyCode.B))
                Record(driver.TryQueueProduction("barracks"));
            if (Input.GetKeyDown(KeyCode.W))
                Record(driver.TryQueueProduction("war_factory"));
            if (Input.GetKeyDown(KeyCode.R))
                Record(driver.TryQueueProduction("refinery"));
            if (Input.GetKeyDown(KeyCode.G))
                Record(driver.TryQueueProduction("gun_tower"));
            if (Input.GetKeyDown(KeyCode.I))
                Record(driver.TryQueueProduction("rifle_infantry"));
            if (Input.GetKeyDown(KeyCode.T))
                Record(driver.TryQueueProduction("light_tank"));
            if (Input.GetKeyDown(KeyCode.H))
                Record(driver.TryQueueProduction("harvester"));
            if (Input.GetKeyDown(KeyCode.L))
                Record(driver.TryForceLowPowerOrCreateLowPowerDemoCondition());
        }

        void Record(RtsCommandResult result)
        {
            if (debugHud != null)
                debugHud.RecordCommandResult(result);
            else
                Debug.Log(result.ToString());
        }

        static bool IsPointerLikelyOverHud()
        {
            return Input.mousePosition.x < 340f && Input.mousePosition.y > Screen.height - 560f;
        }
    }
}
