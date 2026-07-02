using System;
using System.IO;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage25EngineerTransportValidator
    {
        public static void ValidateStage25EngineerTransportBatch()
        {
            try
            {
                ValidateStage25EngineerTransport();
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

        public static void ValidateStage25EngineerTransport()
        {
            Stage24TechSupportValidator.ValidateStage24TechSupport();
            ValidateCoreEngineerTransportData();
            ValidateStage16EngineerTransportUi();
            ValidateMediumAuditScript();
            Debug.Log("Stage 25 engineer/transport validation passed.");
        }

        static void ValidateCoreEngineerTransportData()
        {
            var rules = DemoRules.CreateDefaultRules();
            var engineer = rules.GetDefinition("engineer");
            if (engineer.Capture == null || !engineer.Capture.CanCaptureBuildings || !engineer.Capture.CanRepairBuildings)
                throw new InvalidOperationException("Stage 25 engineer must expose capture and building repair capabilities.");

            var apc = rules.GetDefinition("apc");
            if (apc.Transport == null || apc.Transport.Capacity < 5 || !apc.Transport.AllowsInfantry)
                throw new InvalidOperationException("Stage 25 APC must expose infantry transport capacity.");

            var refinery = rules.GetDefinition("refinery");
            if (refinery.Captureable == null || !refinery.Captureable.CanBeCaptured)
                throw new InvalidOperationException("Stage 25 buildings must expose captureable metadata.");

            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var playerEngineer = world.CreateActor("engineer", 1, new Int2(17, 10));
            var enemyPower = world.FirstActorOfType("power_plant", 2);
            var capture = world.IssueCommand(new CaptureBuildingCommand(1, playerEngineer.Id, enemyPower.Id));
            if (!capture.Success || enemyPower.OwnerPlayerId != 1)
                throw new InvalidOperationException("Stage 25 core capture command failed: " + capture.ErrorCode);

            var transportWorld = DemoWorldFactory.CreateCombatDemoWorld();
            var transport = transportWorld.CreateActor("apc", 1, new Int2(7, 8));
            var passenger = transportWorld.FirstActorOfType("rifle_infantry", 1);
            var load = transportWorld.IssueCommand(new LoadTransportCommand(1, transport.Id, new[] { passenger.Id }));
            if (!load.Success || passenger.LoadedIntoTransportActorId != transport.Id.Value)
                throw new InvalidOperationException("Stage 25 core transport load failed: " + load.ErrorCode);

            var snapshot = transportWorld.CreateSnapshot();
            if (FindTransport(snapshot, transport.Id.Value) == null)
                throw new InvalidOperationException("Stage 25 transport snapshot is missing.");
        }

        static void ValidateStage16EngineerTransportUi()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 25 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var commandBar = Require<CommandBarController>("CommandBarController");
            var selectionPanel = Require<SelectionPanelController>("SelectionPanelController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var leftRouter = Require<LeftHandCommandRouter>("LeftHandCommandRouter");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();

            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 25 needs a live runtime snapshot.");
            if (!layout.AreProductionPanelsInRightSidebar())
                throw new InvalidOperationException("Stage 25 command UI must remain in the PC right-sidebar layout.");
            if (desktopHud.showDebugOverlay || !debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 25 debug panels must remain hidden by default.");
            if (selectionPanel == null || leftRouter == null)
                throw new InvalidOperationException("Stage 25 selection/pass-through routers are missing.");

            RequireCommandButton(commandBar, "Capture");
            RequireCommandButton(commandBar, "Eng Repair");
            RequireCommandButton(commandBar, "Load");
            RequireCommandButton(commandBar, "Unload");

            if (FindActor(driver.LatestSnapshot, "engineer", 1) == null)
                throw new InvalidOperationException("Stage 25 vertical slice should include a player engineer for validation.");
            var apc = FindActor(driver.LatestSnapshot, "apc", 1);
            if (apc == null || apc.TransportCapacity <= 0)
                throw new InvalidOperationException("Stage 25 vertical slice should include a transport with snapshot capacity.");
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 25 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage25-medium-checks.ps1") ||
                !content.Contains("run-unity-stage25-validation.ps1"))
                throw new InvalidOperationException("Stage 25 medium recursion audit does not include Stage 25.");
        }

        static void RequireCommandButton(CommandBarController commandBar, string label)
        {
            var buttons = commandBar.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
            {
                var text = buttons[i].GetComponentInChildren<Text>(true);
                if (text != null && text.text == label)
                    return;
            }

            throw new InvalidOperationException("Stage 25 command button is missing: " + label);
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return snapshot.Actors[i];
            return null;
        }

        static TransportSnapshot FindTransport(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Transports.Count; i++)
                if (snapshot.Transports[i].ActorId == actorId)
                    return snapshot.Transports[i];
            return null;
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
