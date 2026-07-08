using System;
using System.IO;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage28_1LayoutValidator
    {
        static readonly Vector2Int[] Resolutions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440)
        };

        public static void ValidateStage28_1LayoutBatch()
        {
            try
            {
                ValidateStage28_1Layout();
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        public static void ValidateStage28_1Layout()
        {
            ValidateDocsAndTools();
            ValidateBootScene();
            ValidateStage16Layout();
            Debug.Log("Stage 28.1 layout validation passed.");
        }

        static void ValidateDocsAndTools()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            RequireFile(repoRoot, "docs", "STAGE28_1_FULL_GATE_AND_LAYOUT_REPORT.md");
            RequireFile(repoRoot, "docs", "STAGE28_1_PC_LAYOUT_SAFE_AREA.md");
            RequireFile(repoRoot, "tools", "audit-full-validation-recursion.ps1");
            RequireFile(repoRoot, "tools", "run-unity-stage28-1-validation.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-1-fast-checks.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-1-medium-checks.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-1-checks.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-1-player-facing-checks.ps1");
        }

        static void ValidateBootScene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 28.1 boot scene did not open.");

            Require<ProjectAegisRTS.UnityClient.Boot.MainMenuHud>("MainMenuHud");
            Require<ProjectAegisRTS.UnityClient.Boot.GameBootController>("GameBootController");
        }

        static void ValidateStage16Layout()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 28.1 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var uiMode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var boardPlacement = Require<BoardPlacementController>("BoardPlacementController");
            var boardHud = Require<BoardPlacementHud>("BoardPlacementHud");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            uiMode.ApplyPcDesktopMode();
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            var safeArea = EnsureSafeArea(desktopHud, layout, uiMode);
            var framer = EnsureFramer(safeArea);

            if (!uiMode.IsPcSidebarVisibleForDesktop())
                throw new InvalidOperationException("Stage 28.1 expected PCDesktop sidebar to be visible.");
            if (!layout.AreProductionPanelsInRightSidebar() || !layout.IsMinimapAboveProductionGrid() || !layout.IsRightSidebarDockedToScreenEdge())
                throw new InvalidOperationException("Stage 28.1 right sidebar/minimap layout regressed.");
            if (boardPlacement.IsPlacementModeActive || boardHud.gameObject.activeInHierarchy || placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28.1 placement UI must be hidden before building placement.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 28.1 debug panels must remain hidden by default.");

            for (var i = 0; i < Resolutions.Length; i++)
                ValidateResolution(Resolutions[i], safeArea, framer);

            framer.ApplyFraming();
            if (!framer.IsBoardInsideSafeArea(3f))
                throw new InvalidOperationException("Stage 28.1 current-screen board bounds are outside the PC gameplay safe area. bounds=" + framer.GetBoardScreenBounds() + " safe=" + safeArea.GameplayViewportRect);

            uiMode.ApplyQuestXrMode();
            var questSnapshot = framer.ApplyFramingForScreen(1600, 900);
            if (questSnapshot.UsesPcSafeArea || framer.targetCamera.rect != new Rect(0f, 0f, 1f, 1f))
                throw new InvalidOperationException("Stage 28.1 QuestXR mode should not inherit the PC right-sidebar camera safe area.");
            uiMode.ApplyPcDesktopMode();
            framer.ApplyFraming();
        }

        static void ValidateResolution(Vector2Int resolution, PcGameplaySafeAreaController safeArea, PlayerFacingCameraFramer framer)
        {
            var snapshot = safeArea.CalculateForScreen(resolution.x, resolution.y);
            framer.ApplyFramingForScreen(resolution.x, resolution.y);
            var cameraRect = framer.targetCamera.rect;

            if (!snapshot.UsesPcSafeArea)
                throw new InvalidOperationException("Stage 28.1 expected PC safe area at " + resolution + ".");
            if (snapshot.RightReservedPx <= 0f || snapshot.SidebarWidthPx <= 0f)
                throw new InvalidOperationException("Stage 28.1 right sidebar width was not reserved at " + resolution + ".");
            if (snapshot.LeftReservedPx <= 0f || snapshot.ObjectiveWidthPx <= 0f)
                throw new InvalidOperationException("Stage 28.1 objective/checklist width was not reserved at " + resolution + ".");
            if (snapshot.GameplayViewportRect.width < safeArea.minimumGameplayWidthPx)
                throw new InvalidOperationException("Stage 28.1 gameplay viewport is too narrow at " + resolution + ": " + snapshot.GameplayViewportRect.width);
            if (cameraRect.xMin < snapshot.NormalizedCameraRect.xMin - 0.001f || cameraRect.xMax > snapshot.NormalizedCameraRect.xMax + 0.001f)
                throw new InvalidOperationException("Stage 28.1 camera rect does not match safe gameplay viewport at " + resolution + ".");
            if (cameraRect.xMax > 1f - (snapshot.RightReservedPx / snapshot.ScreenWidth) + 0.001f)
                throw new InvalidOperationException("Stage 28.1 camera can render under the right sidebar at " + resolution + ".");
            if (cameraRect.xMin < snapshot.LeftReservedPx / snapshot.ScreenWidth - 0.001f)
                throw new InvalidOperationException("Stage 28.1 camera can render under the left objective column at " + resolution + ".");
            if (framer.targetCamera.orthographicSize > framer.maxOrthographicSize + 0.001f)
                throw new InvalidOperationException("Stage 28.1 camera over-zoomed at " + resolution + ".");

            Debug.Log(PcGameplaySafeAreaController.Describe(snapshot));
        }

        static PcGameplaySafeAreaController EnsureSafeArea(DesktopRtsHudRoot hud, CncStyleSidebarLayout layout, PlayerFacingUiModeController mode)
        {
            var safeArea = UnityEngine.Object.FindFirstObjectByType<PcGameplaySafeAreaController>();
            if (safeArea == null)
                safeArea = hud.gameObject.AddComponent<PcGameplaySafeAreaController>();

            safeArea.desktopHud = hud;
            safeArea.sidebarLayout = layout;
            safeArea.uiModeController = mode;
            safeArea.hudCanvas = hud.canvas;
            safeArea.Refresh();
            return safeArea;
        }

        static PlayerFacingCameraFramer EnsureFramer(PcGameplaySafeAreaController safeArea)
        {
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Stage 28.1 layout validation requires a camera.");

            var framer = camera.GetComponent<PlayerFacingCameraFramer>();
            if (framer == null)
                framer = camera.gameObject.AddComponent<PlayerFacingCameraFramer>();

            framer.targetCamera = camera;
            framer.safeAreaController = safeArea;
            framer.mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            framer.uiModeController = UnityEngine.Object.FindFirstObjectByType<PlayerFacingUiModeController>();
            framer.logOnApply = true;
            framer.ApplyFraming();
            return framer;
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (ProjectAegisRTS.Core.Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
        }

        static void RequireFile(string repoRoot, string folder, string fileName)
        {
            var path = Path.Combine(repoRoot, folder, fileName);
            if (!File.Exists(path))
                throw new InvalidOperationException("Stage 28.1 expected file is missing: " + path);
        }

        static T Require<T>(string label) where T : Component
        {
            var active = UnityEngine.Object.FindFirstObjectByType<T>();
            if (active != null)
                return active;

            var all = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    return all[i];

            throw new InvalidOperationException("Missing component: " + label);
        }
    }
}
