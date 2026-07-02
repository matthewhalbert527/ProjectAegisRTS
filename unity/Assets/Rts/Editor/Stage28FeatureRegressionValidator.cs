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
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage28FeatureRegressionValidator
    {
        public static void ValidateStage28FeatureRegressionBatch()
        {
            try
            {
                ValidateStage28FeatureRegression();
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

        public static void ValidateStage28FeatureRegression()
        {
            ValidateDocsAndTools();
            ValidateBootFirst();
            ValidateStage16FeatureSurface();
            Stage27_1PlacementUxValidator.ValidateStage27_1PlacementUx();
            Debug.Log("Stage 28 feature regression validation passed.");
        }

        static void ValidateDocsAndTools()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            RequireFile(repoRoot, "docs", "STAGE28_FEATURE_REGRESSION_MATRIX.md");
            RequireFile(repoRoot, "docs", "STAGE28_REPORT.md");
            RequireFile(repoRoot, "docs", "STAGE28_PLAYTEST_STABILIZATION.md");
            RequireFile(repoRoot, "docs", "STAGE28_KNOWN_ISSUES.md");
            RequireFile(repoRoot, "tools", "run-unity-stage28-validation.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-fast-checks.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-medium-checks.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-checks.ps1");
            RequireFile(repoRoot, "tools", "run-stage28-player-facing-checks.ps1");

            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            var audit = File.ReadAllText(auditScript);
            if (!audit.Contains("run-stage28-medium-checks.ps1") ||
                !audit.Contains("run-unity-stage28-validation.ps1") ||
                !audit.Contains("run-stage28-player-facing-checks.ps1"))
                throw new InvalidOperationException("Stage 28 medium recursion audit does not include Stage 28 direct dependencies.");
        }

        static void ValidateBootFirst()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 28 boot scene did not open.");

            var mainMenu = Require<MainMenuHud>("MainMenuHud");
            Require<GameBootController>("GameBootController");
            if (!mainMenu.visible)
                throw new InvalidOperationException("Stage 28 boot menu must be visible before loading the vertical slice.");
            if (BootHudLayout.ScaleForScreen(1600, 900) < 1.2f)
                throw new InvalidOperationException("Stage 28 boot menu scale regressed at the default PC window size.");
        }

        static void ValidateStage16FeatureSurface()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 28 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var uiMode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var commandBar = Require<CommandBarController>("CommandBarController");
            var supportPanel = Require<SupportPowerPanelController>("SupportPowerPanelController");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");
            var boardPlacement = Require<BoardPlacementController>("BoardPlacementController");
            var boardHud = Require<BoardPlacementHud>("BoardPlacementHud");
            var router = Require<DesktopUiCommandRouter>("DesktopUiCommandRouter");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            uiMode.ApplyPcDesktopMode();
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            var featureHud = Require<FeatureRegressionHud>("FeatureRegressionHud");
            featureHud.Initialize(driver, router, uiMode, bootstrapper.verticalSliceProgressTracker, bootstrapper.verticalSliceMissionFlowController);
            if (!featureHud.IsHiddenByDefault())
                throw new InvalidOperationException("Stage 28 feature regression HUD must be hidden by default.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 28 debug panels must remain hidden by default.");
            if (!uiMode.IsPcSidebarVisibleForDesktop() || !layout.AreProductionPanelsInRightSidebar() || !layout.IsMinimapAboveProductionGrid())
                throw new InvalidOperationException("Stage 28 PCDesktop right sidebar/minimap layout regressed.");
            if (!layout.IsRightSidebarDockedToScreenEdge())
                throw new InvalidOperationException("Stage 28 PCDesktop sidebar must stay docked to the right screen edge.");
            if (!uiMode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 28 QuestXR menus must stay hidden in PCDesktop mode.");
            if (!uiMode.AreQuestLeftHandControlsAvailable() || !uiMode.AreQuestRightHandControlsAvailable())
                throw new InvalidOperationException("Stage 28 QuestXR hand-control components must remain present for validation.");
            if (boardPlacement.IsPlacementModeActive || boardHud.gameObject.activeInHierarchy || placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28 placement UI must not start active.");

            RequireButtons(commandBar, new[]
            {
                "Stop",
                "Move",
                "Attack",
                "Attack Move",
                "Guard",
                "Patrol",
                "Scatter",
                "Deploy",
                "Repair",
                "Sell",
                "Power",
                "Rally",
                "Capture",
                "Eng Repair",
                "Load",
                "Unload"
            });

            if (supportPanel.GetComponentsInChildren<Button>(true).Length < 5)
                throw new InvalidOperationException("Stage 28 support-power panel lost its player-facing buttons.");

            ValidateWorldSnapshot(driver);
            ValidateFeatureHud(featureHud);
        }

        static void ValidateWorldSnapshot(RtsSimulationDriver driver)
        {
            var snapshot = RequireSnapshot(driver);
            if (snapshot.Map == null || snapshot.Map.PlacementGridScale < 2)
                throw new InvalidOperationException("Stage 28 fine placement-grid metadata is missing.");
            if (snapshot.Economy == null || snapshot.Economy.Harvesters.Count == 0 || snapshot.Economy.Refineries.Count == 0)
                throw new InvalidOperationException("Stage 28 economy/harvester/refinery snapshots regressed.");
            if (snapshot.Fog == null || snapshot.Radar == null || snapshot.Minimap == null)
                throw new InvalidOperationException("Stage 28 visibility/radar/minimap snapshots regressed.");
            if (snapshot.Ai == null || snapshot.Ai.Players.Count == 0)
                throw new InvalidOperationException("Stage 28 AI pressure snapshot regressed.");
            if (snapshot.Airfields.Count == 0 || snapshot.Aircraft.Count == 0)
                throw new InvalidOperationException("Stage 28 airfield/aircraft snapshots regressed.");
            if (FindActor(snapshot, "engineer", 1) == null || FindActor(snapshot, "apc", 1) == null)
                throw new InvalidOperationException("Stage 28 engineer/transport actors must remain in the vertical slice.");
            if (FindActor(snapshot, "dual_helipad", 1) == null || FindActor(snapshot, "attack_aircraft", 1) == null)
                throw new InvalidOperationException("Stage 28 air foundation actors must remain in the vertical slice.");
        }

        static void ValidateFeatureHud(FeatureRegressionHud featureHud)
        {
            var statuses = featureHud.BuildAuditSnapshot();
            if (statuses.Length < 25)
                throw new InvalidOperationException("Stage 28 feature regression HUD did not report enough feature rows.");

            for (var i = 0; i < statuses.Length; i++)
            {
                if (statuses[i].Status != "Ready")
                    throw new InvalidOperationException("Stage 28 feature regression HUD reported a blocked route: " + statuses[i].Group + " / " + statuses[i].Label + " -> " + statuses[i].Detail);
            }

            if (!featureHud.ToggleVisibleForValidation())
                throw new InvalidOperationException("Stage 28 feature regression HUD did not toggle on for validation.");
            if (featureHud.ToggleVisibleForValidation())
                throw new InvalidOperationException("Stage 28 feature regression HUD did not toggle back off for validation.");
        }

        static void RequireButtons(CommandBarController commandBar, string[] expectedLabels)
        {
            var buttons = commandBar.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < expectedLabels.Length; i++)
            {
                var expected = expectedLabels[i];
                var found = false;
                for (var j = 0; j < buttons.Length; j++)
                {
                    var text = buttons[j].GetComponentInChildren<Text>(true);
                    if (text != null && string.Equals(text.text, expected, StringComparison.Ordinal))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    throw new InvalidOperationException("Stage 28 command bar is missing button: " + expected);
            }
        }

        static void RequireFile(string repoRoot, string folder, string fileName)
        {
            var path = Path.Combine(repoRoot, folder, fileName);
            if (!File.Exists(path))
                throw new InvalidOperationException("Stage 28 expected file is missing: " + path);
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.TypeId == typeId && actor.OwnerId == ownerId && !actor.IsDestroyed)
                    return actor;
            }

            return null;
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 28 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
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
