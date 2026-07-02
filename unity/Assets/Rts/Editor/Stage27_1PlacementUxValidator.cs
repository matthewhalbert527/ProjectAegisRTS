using System;
using System.IO;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
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
    public static class Stage27_1PlacementUxValidator
    {
        public static void ValidateStage27_1PlacementUxBatch()
        {
            try
            {
                ValidateStage27_1PlacementUx();
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

        public static void ValidateStage27_1PlacementUx()
        {
            ValidateBootSceneFirst();
            ValidatePcDesktopPlacementSeparation();
            ValidateExplicitBoardSetupAvailability();
            ValidateMediumAuditScript();
            Debug.Log("Stage 27.1 placement UX validation passed.");
        }

        static void ValidateBootSceneFirst()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 27.1 boot scene did not open.");

            var mainMenu = RequireComponent<MainMenuHud>("MainMenuHud");
            RequireComponent<GameBootController>("GameBootController");
            if (!mainMenu.visible)
                throw new InvalidOperationException("Stage 27.1 boot menu must be visible before loading the vertical slice.");
            if (mainMenu.area.width < 520f || mainMenu.area.height < 410f)
                throw new InvalidOperationException("Stage 27.1 boot menu must use a player-facing PC menu footprint.");
            if (BootHudLayout.ScaleForScreen(1600, 900) < 1.2f)
                throw new InvalidOperationException("Stage 27.1 boot menu must scale up at the default PC window size.");
            if (BootHudLayout.ScaleForScreen(3840, 2160) < 1.5f)
                throw new InvalidOperationException("Stage 27.1 boot menu must scale up on high-resolution PC displays.");
        }

        static void ValidatePcDesktopPlacementSeparation()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 27.1 Stage16 scene did not open.");

            var bootstrapper = RequireComponent<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = RequireComponent<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = RequireComponent<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireComponent<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = RequireComponent<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = RequireComponent<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var mode = RequireComponent<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = RequireComponent<DebugHudVisibilityController>("DebugHudVisibilityController");
            var placementPanel = RequireComponent<PlacementModePanel>("PlacementModePanel");
            var boardPlacement = RequireComponent<BoardPlacementController>("BoardPlacementController");
            var boardHud = RequireComponent<BoardPlacementHud>("BoardPlacementHud");
            var router = RequireComponent<DesktopUiCommandRouter>("DesktopUiCommandRouter");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            mode.ApplyPcDesktopMode();
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 6, 0.05f);

            ValidateDesktopHudDocking(desktopHud, layout);
            if (!mode.WindowsPlayerDefaultsToPcDesktop() || !mode.IsPcSidebarVisibleForDesktop())
                throw new InvalidOperationException("Stage 27.1 PCDesktop sidebar defaults are not active.");
            if (!layout.AreProductionPanelsInRightSidebar() || !layout.IsMinimapAboveProductionGrid())
                throw new InvalidOperationException("Stage 27.1 minimap/sidebar layout was not preserved.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 27.1 Quest/XR build menus must stay hidden in PCDesktop mode.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 27.1 debug panels must remain hidden by default.");
            if (!debugVisibility.IsBoardPlacementHudHiddenInPcDesktop() || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 BoardPlacementHud must be hidden by default in PCDesktop mode.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 right-sidebar placement panel must be hidden before building placement.");
            if (boardPlacement.IsPlacementModeActive)
                throw new InvalidOperationException("Stage 27.1 board setup placement must not start active.");

            var powerPlantsBefore = CountOwnedActors(driver, "power_plant", 1);
            RequireSuccess(router.QueueProduction("power_plant"), "queue Power Plant from right sidebar route");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(router.QueueProduction("power_plant"), "click ready Power Plant production card");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);

            if (!driver.HasPlacementMode || driver.PendingPlacementTypeId != "power_plant")
                throw new InvalidOperationException("Stage 27.1 ready Power Plant card did not enter building placement mode.");
            if (boardPlacement.IsPlacementModeActive)
                throw new InvalidOperationException("Stage 27.1 ready Power Plant card incorrectly entered board setup placement.");
            if (!placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 right-sidebar PlacementModePanel must be visible during building placement.");
            if (boardHud.gameObject.activeInHierarchy || !debugVisibility.IsBoardPlacementHudHiddenInPcDesktop())
                throw new InvalidOperationException("Stage 27.1 BoardPlacementHud became visible during PC building placement.");

            var validCell = FindValidPlacementCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(validCell, true);
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            PlacementPreviewSnapshot preview;
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace)
                throw new InvalidOperationException("Stage 27.1 expected a valid fine-grid building placement preview.");
            if (preview.FootprintCells.Count == 0 || preview.PlacementGridScale <= 1)
                throw new InvalidOperationException("Stage 27.1 placement preview must expose fine-grid footprint cells.");

            var coarseHoverCell = FindValidCoarsePlacementHoverCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(coarseHoverCell, false);
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace)
                throw new InvalidOperationException("Stage 27.1 desktop coarse hover did not resolve to a valid fine-grid placement preview.");
            if (!driver.HoveredPlacementCell.Equals(PlacementGridMetrics.CoarseCellToPlacementCell(coarseHoverCell)))
                throw new InvalidOperationException("Stage 27.1 desktop hover must normalize to the matching fine-grid placement cell.");
            Int2 suggestedCell;
            if (!driver.TryFindSuggestedPlacementCell(out suggestedCell))
                throw new InvalidOperationException("Stage 27.1 expected a suggested legal placement cell for the completed Power Plant.");

            RequireSuccess(router.CancelPlacement(), "right-sidebar Cancel");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            if (driver.HasPlacementMode || placementPanel.gameObject.activeInHierarchy || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 right-sidebar Cancel did not restore the normal PC command UI.");

            RequireSuccess(router.QueueProduction("power_plant"), "reactivate ready Power Plant for suggested placement");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            RequireSuccess(router.PlaceAtSuggestedCell(), "right-sidebar Place Suggested");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);
            if (driver.HasPlacementMode || placementPanel.gameObject.activeInHierarchy || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 Place Suggested did not restore normal PC UI after placement.");
            if (CountOwnedActors(driver, "power_plant", 1) <= powerPlantsBefore)
                throw new InvalidOperationException("Stage 27.1 Place Suggested did not create a new Power Plant.");
        }

        static void ValidateExplicitBoardSetupAvailability()
        {
            var mode = RequireComponent<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = RequireComponent<DebugHudVisibilityController>("DebugHudVisibilityController");
            var boardPlacement = RequireComponent<BoardPlacementController>("BoardPlacementController");
            var boardHud = RequireComponent<BoardPlacementHud>("BoardPlacementHud");

            mode.ApplyQuestXrMode();
            boardPlacement.SetPlacementMode(true);
            debugVisibility.ApplyPlayerFacingDefaults();

            if (!boardPlacement.IsPlacementModeActive)
                throw new InvalidOperationException("Stage 27.1 explicit board setup placement did not activate.");
            if (!boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 BoardPlacementHud must remain available for explicit board setup.");
            if (!mode.AreQuestLeftHandControlsAvailable() || !mode.AreQuestRightHandControlsAvailable())
                throw new InvalidOperationException("Stage 27.1 QuestXR hand-control components must remain available.");

            boardPlacement.CancelPlacement();
            mode.ApplyPcDesktopMode();
            debugVisibility.ApplyPlayerFacingDefaults();
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 27.1 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage27-1-medium-checks.ps1") ||
                !content.Contains("run-unity-stage27-1-validation.ps1") ||
                !content.Contains("run-stage27-1-player-facing-checks.ps1"))
                throw new InvalidOperationException("Stage 27.1 medium recursion audit does not include Stage 27.1.");
        }

        static void StepUntilPendingPlacement(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, string typeId)
        {
            for (var i = 0; i < 600; i++)
            {
                if (HasPendingPlacement(driver, typeId))
                    return;
                StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.1f);
            }

            throw new InvalidOperationException("Timed out waiting for pending placement: " + typeId);
        }

        static Int2 FindValidPlacementCell(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            var snapshot = RequireSnapshot(driver);
            for (var y = 0; y < snapshot.Map.PlacementHeight; y++)
            {
                for (var x = 0; x < snapshot.Map.PlacementWidth; x++)
                {
                    var candidate = new Int2(x, y);
                    driver.SetHoveredCell(candidate, true);
                    StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
                    PlacementPreviewSnapshot preview;
                    if (driver.TryGetPlacementPreview(out preview) && preview.CanPlace)
                        return candidate;
                }
            }

            throw new InvalidOperationException("No valid Stage 27.1 placement cell was found.");
        }

        static Int2 FindValidCoarsePlacementHoverCell(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            var snapshot = RequireSnapshot(driver);
            for (var y = 0; y < snapshot.Map.Height; y++)
            {
                for (var x = 0; x < snapshot.Map.Width; x++)
                {
                    var candidate = new Int2(x, y);
                    driver.SetHoveredCell(candidate, false);
                    StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
                    PlacementPreviewSnapshot preview;
                    if (driver.TryGetPlacementPreview(out preview) && preview.CanPlace)
                        return candidate;
                }
            }

            throw new InvalidOperationException("No valid Stage 27.1 coarse desktop hover placement cell was found.");
        }

        static void ValidateDesktopHudDocking(DesktopRtsHudRoot desktopHud, CncStyleSidebarLayout layout)
        {
            if (desktopHud == null || layout == null)
                throw new InvalidOperationException("Stage 27.1 desktop HUD docking check needs a HUD root and layout.");

            var canvas = desktopHud.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                throw new InvalidOperationException("Stage 27.1 desktop HUD must run on a Screen Space Overlay canvas.");

            var rootRect = desktopHud.GetComponent<RectTransform>();
            if (rootRect == null ||
                !Approximately(rootRect.anchorMin.x, 0f) ||
                !Approximately(rootRect.anchorMin.y, 0f) ||
                !Approximately(rootRect.anchorMax.x, 1f) ||
                !Approximately(rootRect.anchorMax.y, 1f) ||
                !Approximately(rootRect.offsetMin.x, 0f) ||
                !Approximately(rootRect.offsetMin.y, 0f) ||
                !Approximately(rootRect.offsetMax.x, 0f) ||
                !Approximately(rootRect.offsetMax.y, 0f))
                throw new InvalidOperationException("Stage 27.1 desktop HUD root must stretch to the full player canvas.");

            if (!layout.IsRightSidebarDockedToScreenEdge())
                throw new InvalidOperationException("Stage 27.1 PC right sidebar must be docked to the screen's right edge.");
        }

        static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.01f;
        }

        static bool HasPendingPlacement(RtsSimulationDriver driver, string typeId)
        {
            var player = driver.GetLocalPlayerSnapshot();
            if (player == null)
                return false;

            for (var i = 0; i < player.Production.Count; i++)
                if (player.Production[i].TypeId == typeId && player.Production[i].State == "CompletedPendingPlacement")
                    return true;
            return false;
        }

        static int CountOwnedActors(RtsSimulationDriver driver, string typeId, int ownerId)
        {
            var snapshot = RequireSnapshot(driver);
            var count = 0;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId == ownerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    count++;
            }

            return count;
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 27.1 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 27.1 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
        }

        static T RequireComponent<T>(string label) where T : Component
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
