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
    public static class Stage24TechSupportValidator
    {
        public static void ValidateStage24TechSupportBatch()
        {
            try
            {
                ValidateStage24TechSupport();
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

        public static void ValidateStage24TechSupport()
        {
            Stage23BaseManagementValidator.ValidateStage23BaseManagement();
            ValidateCoreTechSupportData();
            ValidateStage16TechSupportUi();
            ValidateMediumAuditScript();
            Debug.Log("Stage 24 tech/support validation passed.");
        }

        static void ValidateCoreTechSupportData()
        {
            var rules = DemoRules.CreateDefaultRules();
            RequirePrerequisite(rules, "tech_center", "comm_center");
            RequirePrerequisite(rules, "heavy_tank", "tech_center");
            RequirePrerequisite(rules, "rocket_infantry", "comm_center");
            RequirePrerequisite(rules, "advanced_gun_tower", "tech_center");

            if (rules.SupportPowerDefinitions.Count < 5)
                throw new InvalidOperationException("Stage 24 expected five support-power definitions.");
            if (!rules.TryGetSupportPowerDefinition("reveal_scan", out var revealScan))
                throw new InvalidOperationException("Stage 24 reveal_scan support power is missing.");
            if (revealScan.PrerequisiteTypeIds.Count == 0 || revealScan.PrerequisiteTypeIds[0] != "comm_center")
                throw new InvalidOperationException("Stage 24 reveal_scan must require comm_center.");

            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            var locked = world.IssueCommand(new BeginProductionCommand(1, hub.Id, "tech_center"));
            if (locked.Success || locked.ErrorCode != "MissingPrerequisite")
                throw new InvalidOperationException("Stage 24 tech_center should be locked without comm_center.");

            world.CreateActor("comm_center", 1, new Int2(8, 4));
            if (world.GetMissingProductionPrerequisiteTypeId(1, "tech_center") != string.Empty)
                throw new InvalidOperationException("Stage 24 comm_center should unlock tech_center.");
            if (world.GetMissingSupportPowerPrerequisiteTypeId(1, "reveal_scan") != string.Empty)
                throw new InvalidOperationException("Stage 24 comm_center should unlock reveal_scan.");
        }

        static void ValidateStage16TechSupportUi()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 24 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var supportPanel = Require<SupportPowerPanelController>("SupportPowerPanelController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var leftRouter = Require<LeftHandCommandRouter>("LeftHandCommandRouter");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();

            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 24 needs a live runtime snapshot for tech/support validation.");
            if (!layout.AreProductionPanelsInRightSidebar())
                throw new InvalidOperationException("Stage 24 support powers must remain in the PC right-sidebar layout.");
            if (desktopHud.showDebugOverlay || !debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 24 debug panels must remain hidden by default.");
            if (supportPanel.transform.parent != layout.rightSidebarRoot)
                throw new InvalidOperationException("Stage 24 support-power panel is not parented under the right sidebar.");
            if (leftRouter == null)
                throw new InvalidOperationException("Stage 24 left-hand support-power routing placeholder is missing.");

            var buttons = supportPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length < 5)
                throw new InvalidOperationException("Stage 24 support-power panel should expose five buttons.");

            var player = driver.GetLocalPlayerSnapshot();
            var reveal = FindSupportPower(player, "reveal_scan");
            if (reveal == null || !reveal.IsUnlocked || !reveal.IsReady)
                throw new InvalidOperationException("Stage 24 vertical slice should expose ready Reveal Scan through support snapshots.");
            if (driver.GetMissingProductionPrerequisiteTypeId("tech_center") != string.Empty)
                throw new InvalidOperationException("Stage 24 vertical slice comm center should satisfy tech_center prerequisites.");
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 24 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage24-medium-checks.ps1") ||
                !content.Contains("run-unity-stage24-validation.ps1"))
                throw new InvalidOperationException("Stage 24 medium recursion audit does not include Stage 24.");
        }

        static void RequirePrerequisite(RtsRules rules, string typeId, string prerequisiteTypeId)
        {
            var definition = rules.GetDefinition(typeId);
            for (var i = 0; i < definition.Production.PrerequisiteTypeIds.Count; i++)
                if (definition.Production.PrerequisiteTypeIds[i] == prerequisiteTypeId)
                    return;

            throw new InvalidOperationException("Stage 24 " + typeId + " is missing prerequisite " + prerequisiteTypeId + ".");
        }

        static SupportPowerSnapshot FindSupportPower(PlayerSnapshot player, string powerId)
        {
            if (player == null)
                return null;

            for (var i = 0; i < player.SupportPowers.Count; i++)
                if (player.SupportPowers[i].PowerId == powerId)
                    return player.SupportPowers[i];

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
