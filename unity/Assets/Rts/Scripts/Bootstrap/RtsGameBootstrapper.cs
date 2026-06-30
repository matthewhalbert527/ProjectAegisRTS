using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Ai;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using ProjectAegisRTS.UnityClient.Rendering.Visibility;
using ProjectAegisRTS.UnityClient.UI;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Bootstrap
{
    public sealed class RtsGameBootstrapper : MonoBehaviour
    {
        [Header("Simulation")]
        public int ticksPerSecond = 20;
        public int boardWidth = 32;
        public int boardHeight = 32;
        public float boardCellSizeMeters = 1f;
        public bool startPaused;
        public bool enableSmoothVisualInterpolation = true;

        [Header("Scene References")]
        public Transform boardRoot;
        public BoardCoordinateMapper coordinateMapper;
        public BoardRenderer boardRenderer;
        public ActorRenderSystem actorRenderSystem;
        public CombatVisualProfileLibrary combatVisualProfileLibrary;
        public ProjectileRenderSystem projectileRenderSystem;
        public CombatEventRenderSystem combatEventRenderSystem;
        public ResourceFieldRenderSystem resourceFieldRenderSystem;
        public HarvesterCargoVisualController harvesterCargoVisualController;
        public RefineryDockVisualController refineryDockVisualController;
        public EconomyEventRenderSystem economyEventRenderSystem;
        public FogOverlayRenderer fogOverlayRenderer;
        public VisibilityDebugRenderer visibilityDebugRenderer;
        public RadarSnapshotAdapter radarSnapshotAdapter;
        public MinimapRenderSystem minimapRenderSystem;
        public AiIntentRenderSystem aiIntentRenderSystem;
        public AiPlanTimelineView aiPlanTimelineView;
        public RtsSimulationDriver simulationDriver;
        public RtsDesktopInputController inputController;
        public RtsDebugHud debugHud;
        public RtsCameraController cameraController;
        public Camera sceneCamera;

        bool initialized;

        void Start()
        {
            InitializeScene();
        }

        void Update()
        {
            if (!initialized)
                return;

            simulationDriver.ManualUpdate(Time.deltaTime);

            var hovered = simulationDriver.HasHoveredCell ? (ProjectAegisRTS.Core.Int2?)simulationDriver.HoveredCell : null;
            boardRenderer.UpdateHover(hovered);

            PlacementPreviewSnapshot preview;
            boardRenderer.UpdatePlacementPreview(simulationDriver.TryGetPlacementPreview(out preview) ? preview : null);
            actorRenderSystem.RenderSnapshot(simulationDriver.LatestSnapshot, simulationDriver.SelectedActorIds, Time.deltaTime);
        }

        public void InitializeScene()
        {
            EnsureReferences();
            simulationDriver.Initialize(ticksPerSecond, startPaused);
            coordinateMapper.Configure(boardWidth, boardHeight, boardCellSizeMeters, boardRoot);
            boardRenderer.Initialize(coordinateMapper);
            actorRenderSystem.Initialize(coordinateMapper, simulationDriver, enableSmoothVisualInterpolation);
            if (combatVisualProfileLibrary != null)
                combatVisualProfileLibrary.EnsureInitialized();
            if (projectileRenderSystem != null)
                projectileRenderSystem.Initialize(simulationDriver, coordinateMapper, combatVisualProfileLibrary);
            if (combatEventRenderSystem != null)
                combatEventRenderSystem.Initialize(simulationDriver, coordinateMapper, combatVisualProfileLibrary);
            if (resourceFieldRenderSystem != null)
                resourceFieldRenderSystem.Initialize(simulationDriver, coordinateMapper);
            if (harvesterCargoVisualController != null)
                harvesterCargoVisualController.Initialize(simulationDriver, coordinateMapper);
            if (refineryDockVisualController != null)
                refineryDockVisualController.Initialize(simulationDriver, coordinateMapper);
            if (economyEventRenderSystem != null)
                economyEventRenderSystem.Initialize(simulationDriver, coordinateMapper);
            if (fogOverlayRenderer != null)
                fogOverlayRenderer.Initialize(simulationDriver, coordinateMapper);
            if (visibilityDebugRenderer != null)
                visibilityDebugRenderer.Initialize(simulationDriver, coordinateMapper);
            if (radarSnapshotAdapter != null)
                radarSnapshotAdapter.driver = simulationDriver;
            if (minimapRenderSystem != null)
                minimapRenderSystem.Initialize(simulationDriver, coordinateMapper);
            if (aiIntentRenderSystem != null)
                aiIntentRenderSystem.Initialize(simulationDriver, coordinateMapper);
            if (aiPlanTimelineView != null)
                aiPlanTimelineView.Initialize(simulationDriver);
            inputController.Initialize(sceneCamera, coordinateMapper, simulationDriver, debugHud);
            debugHud.Initialize(simulationDriver);
            cameraController.Configure(coordinateMapper);
            initialized = true;
        }

        void EnsureReferences()
        {
            if (simulationDriver == null)
                simulationDriver = GetOrAdd<RtsSimulationDriver>(gameObject);

            if (boardRoot == null)
            {
                var existingBoard = GameObject.Find("BoardRoot");
                boardRoot = existingBoard != null ? existingBoard.transform : new GameObject("BoardRoot").transform;
            }

            if (coordinateMapper == null)
                coordinateMapper = GetOrAdd<BoardCoordinateMapper>(boardRoot.gameObject);
            if (boardRenderer == null)
                boardRenderer = GetOrAdd<BoardRenderer>(boardRoot.gameObject);
            if (actorRenderSystem == null)
                actorRenderSystem = GetOrAdd<ActorRenderSystem>(gameObject);
            if (inputController == null)
                inputController = GetOrAdd<RtsDesktopInputController>(gameObject);
            if (debugHud == null)
                debugHud = GetOrAdd<RtsDebugHud>(gameObject);

            if (sceneCamera == null)
            {
                sceneCamera = Camera.main;
                if (sceneCamera == null)
                {
                    var cameraObject = new GameObject("Main Camera");
                    cameraObject.tag = "MainCamera";
                    sceneCamera = cameraObject.AddComponent<Camera>();
                }
            }

            if (cameraController == null)
                cameraController = GetOrAdd<RtsCameraController>(sceneCamera.gameObject);

            if (FindAnyObjectByType<Light>() == null)
            {
                var lightObject = new GameObject("Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            }
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
