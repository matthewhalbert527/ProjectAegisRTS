using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.Performance;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Ai;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using ProjectAegisRTS.UnityClient.Rendering.Map;
using ProjectAegisRTS.UnityClient.Rendering.Visibility;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI;
using ProjectAegisRTS.UnityClient.UI.Common;
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
        public TerrainDebugRenderer terrainDebugRenderer;
        public PathDebugRenderer pathDebugRenderer;
        public MapAuthoringOverlay mapAuthoringOverlay;
        public FeedbackProfileLibrary feedbackProfileLibrary;
        public FeedbackEventBus feedbackEventBus;
        public AudioFeedbackController audioFeedbackController;
        public VfxFeedbackController vfxFeedbackController;
        public UiFeedbackController uiFeedbackController;
        public HapticFeedbackAdapter hapticFeedbackAdapter;
        public FeedbackDebugHud feedbackDebugHud;
        public ObjectPoolService objectPoolService;
        public PerformanceBudgetLibrary performanceBudgetLibrary;
        public RuntimePerformanceStats runtimePerformanceStats;
        public SceneComplexityReporter sceneComplexityReporter;
        public QualityProfileApplier qualityProfileApplier;
        public QuestBuildReadinessReporter questBuildReadinessReporter;
        public PcBuildReadinessReporter pcBuildReadinessReporter;
        public RenderStatsHud renderStatsHud;
        public VerticalSliceScenarioController verticalSliceScenarioController;
        public VerticalSliceProgressTracker verticalSliceProgressTracker;
        public MatchObjectiveHud matchObjectiveHud;
        public PlayerObjectiveHud playerObjectiveHud;
        public VerticalSliceChecklistHud verticalSliceChecklistHud;
        public PlayerPromptSystem playerPromptSystem;
        public PlayerPromptHud playerPromptHud;
        public PlayerControlsOverlay playerControlsOverlay;
        public MatchResultHud matchResultHud;
        public IntegratedSystemsStatusHud integratedSystemsStatusHud;
        public VerticalSliceDebugActions verticalSliceDebugActions;
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
            if (feedbackEventBus != null)
                simulationDriver.feedbackEventBus = feedbackEventBus;
            simulationDriver.Initialize(ticksPerSecond, startPaused);
            coordinateMapper.Configure(boardWidth, boardHeight, boardCellSizeMeters, boardRoot);
            boardRenderer.Initialize(coordinateMapper);
            actorRenderSystem.Initialize(coordinateMapper, simulationDriver, enableSmoothVisualInterpolation);
            if (combatVisualProfileLibrary != null)
                combatVisualProfileLibrary.EnsureInitialized();
            if (performanceBudgetLibrary != null)
                performanceBudgetLibrary.EnsureInitialized();
            if (projectileRenderSystem != null)
                projectileRenderSystem.Initialize(simulationDriver, coordinateMapper, combatVisualProfileLibrary, objectPoolService);
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
            if (terrainDebugRenderer != null)
                terrainDebugRenderer.Initialize(simulationDriver, coordinateMapper);
            if (pathDebugRenderer != null)
                pathDebugRenderer.Initialize(simulationDriver, coordinateMapper);
            if (mapAuthoringOverlay != null)
                mapAuthoringOverlay.Initialize(simulationDriver, coordinateMapper);
            if (feedbackProfileLibrary != null)
                feedbackProfileLibrary.EnsureInitialized();
            if (feedbackEventBus != null)
                feedbackEventBus.Initialize(simulationDriver, coordinateMapper);
            if (audioFeedbackController != null)
                audioFeedbackController.Initialize(feedbackEventBus, feedbackProfileLibrary);
            if (vfxFeedbackController != null)
                vfxFeedbackController.Initialize(feedbackEventBus, feedbackProfileLibrary, objectPoolService);
            if (uiFeedbackController != null)
                uiFeedbackController.Initialize(feedbackEventBus);
            if (hapticFeedbackAdapter != null)
                hapticFeedbackAdapter.Initialize(feedbackEventBus, feedbackProfileLibrary);
            if (feedbackDebugHud != null)
            {
                feedbackDebugHud.eventBus = feedbackEventBus;
                feedbackDebugHud.profileLibrary = feedbackProfileLibrary;
                feedbackDebugHud.audioController = audioFeedbackController;
                feedbackDebugHud.vfxController = vfxFeedbackController;
                feedbackDebugHud.uiController = uiFeedbackController;
                feedbackDebugHud.hapticAdapter = hapticFeedbackAdapter;
            }
            if (sceneComplexityReporter != null)
                sceneComplexityReporter.Refresh();
            if (qualityProfileApplier != null)
            {
                qualityProfileApplier.Initialize(performanceBudgetLibrary);
                if (qualityProfileApplier.applyOnStart)
                    qualityProfileApplier.ApplySelectedProfile();
            }
            if (runtimePerformanceStats != null)
                runtimePerformanceStats.Initialize(simulationDriver, projectileRenderSystem, vfxFeedbackController, objectPoolService);
            if (questBuildReadinessReporter != null)
                questBuildReadinessReporter.Initialize(performanceBudgetLibrary, runtimePerformanceStats, sceneComplexityReporter, qualityProfileApplier);
            if (pcBuildReadinessReporter != null)
                pcBuildReadinessReporter.Initialize(performanceBudgetLibrary, runtimePerformanceStats, sceneComplexityReporter);
            if (renderStatsHud != null)
                renderStatsHud.Initialize(runtimePerformanceStats, sceneComplexityReporter, performanceBudgetLibrary, questBuildReadinessReporter, pcBuildReadinessReporter);
            if (verticalSliceDebugActions != null)
                verticalSliceDebugActions.Initialize(simulationDriver);
            if (verticalSliceProgressTracker != null)
                verticalSliceProgressTracker.Initialize(simulationDriver);
            if (matchObjectiveHud != null)
                matchObjectiveHud.Initialize(simulationDriver, verticalSliceScenarioController, verticalSliceDebugActions);
            if (playerObjectiveHud != null)
                playerObjectiveHud.Initialize(simulationDriver, verticalSliceProgressTracker);
            if (verticalSliceChecklistHud != null)
                verticalSliceChecklistHud.Initialize(simulationDriver, verticalSliceProgressTracker);
            if (playerPromptSystem != null)
                playerPromptSystem.Initialize(simulationDriver, verticalSliceProgressTracker);
            if (playerPromptHud != null)
                playerPromptHud.Initialize(simulationDriver, playerPromptSystem);
            if (matchResultHud != null)
                matchResultHud.Initialize(simulationDriver, verticalSliceScenarioController);
            if (integratedSystemsStatusHud != null)
                integratedSystemsStatusHud.Initialize(simulationDriver);
            if (verticalSliceScenarioController != null)
                verticalSliceScenarioController.Initialize(simulationDriver, matchObjectiveHud, integratedSystemsStatusHud, verticalSliceDebugActions);
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
            if (matchObjectiveHud == null)
                matchObjectiveHud = GetOrAdd<MatchObjectiveHud>(gameObject);
            if (verticalSliceProgressTracker == null)
                verticalSliceProgressTracker = GetOrAdd<VerticalSliceProgressTracker>(gameObject);
            if (playerObjectiveHud == null)
                playerObjectiveHud = GetOrAdd<PlayerObjectiveHud>(gameObject);
            if (verticalSliceChecklistHud == null)
                verticalSliceChecklistHud = GetOrAdd<VerticalSliceChecklistHud>(gameObject);
            if (playerPromptSystem == null)
                playerPromptSystem = GetOrAdd<PlayerPromptSystem>(gameObject);
            if (playerPromptHud == null)
                playerPromptHud = GetOrAdd<PlayerPromptHud>(gameObject);
            if (playerControlsOverlay == null)
                playerControlsOverlay = GetOrAdd<PlayerControlsOverlay>(gameObject);
            if (matchResultHud == null)
                matchResultHud = GetOrAdd<MatchResultHud>(gameObject);

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
