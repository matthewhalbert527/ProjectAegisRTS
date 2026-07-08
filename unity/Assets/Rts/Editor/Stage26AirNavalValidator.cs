using System;
using System.IO;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Pathfinding;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.Terrain;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage26AirNavalValidator
    {
        public static void ValidateStage26AirNavalBatch()
        {
            try
            {
                ValidateStage26AirNaval();
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

        public static void ValidateStage26AirNaval()
        {
            Stage25EngineerTransportValidator.ValidateStage25EngineerTransport();
            ValidateCoreAirNavalData();
            ValidateStage16AirNavalUi();
            ValidateMediumAuditScript();
            Debug.Log("Stage 26 air/naval validation passed.");
        }

        static void ValidateCoreAirNavalData()
        {
            var rules = DemoRules.CreateDefaultRules();
            var helipad = rules.GetDefinition("dual_helipad");
            if (helipad.Airfield == null || helipad.Airfield.PadCount != 2)
                throw new InvalidOperationException("Stage 26 dual helipad must expose two airfield pads.");

            var aircraft = rules.GetDefinition("attack_aircraft");
            if (aircraft.Aircraft == null || aircraft.Production.Kind != ProductionKind.Aircraft || !aircraft.Aircraft.RequiresAirfield)
                throw new InvalidOperationException("Stage 26 attack aircraft must expose aircraft production metadata.");

            TerrainDefinition water = null;
            var terrain = TerrainCatalog.CreateDefaultDefinitions();
            for (var i = 0; i < terrain.Count; i++)
                if (terrain[i].Kind == TerrainKind.Water)
                    water = terrain[i];
            if (water == null || !water.Allows(MovementClass.Aircraft) || !water.Allows(MovementClass.Naval) || water.Allows(MovementClass.Wheeled))
                throw new InvalidOperationException("Stage 26 water terrain must allow aircraft/naval movement and block wheeled ground movement.");

            var map = new GridMap(8, 4);
            for (var x = 0; x < 8; x++)
                map.SetTerrainKind(new Int2(x, 1), TerrainKind.Water);
            var navalPath = new GridPathfinder().QueryPath(map, rules, new Int2(1, 1), new Int2(6, 1), MovementClass.Naval);
            if (!navalPath.Success)
                throw new InvalidOperationException("Stage 26 naval movement class could not path over water: " + navalPath.FailureCode);

            var world = new RtsWorld(rules, new GridMap(16, 16));
            world.AddPlayer(1, "Air Validation Player", 5000);
            world.CreateActor("power_plant", 1, new Int2(9, 1));
            world.CreateActor("tech_center", 1, new Int2(1, 1));
            var airfieldActor = world.CreateActor("dual_helipad", 1, new Int2(5, 5));
            var begin = world.IssueCommand(new BeginProductionCommand(1, airfieldActor.Id, "attack_aircraft"));
            if (!begin.Success)
                throw new InvalidOperationException("Stage 26 aircraft production failed to start: " + begin.ErrorCode);
            for (var i = 0; i < 60; i++)
                world.Tick();

            var snapshot = world.CreateSnapshot();
            if (snapshot.Airfields.Count != 1 || snapshot.Airfields[0].Pads.Count != 2)
                throw new InvalidOperationException("Stage 26 airfield snapshot did not expose the two helipad pads.");
            if (snapshot.Aircraft.Count != 1 || snapshot.Aircraft[0].DockedAirfieldActorId != airfieldActor.Id.Value)
                throw new InvalidOperationException("Stage 26 aircraft snapshot did not dock produced aircraft at the helipad.");
        }

        static void ValidateStage16AirNavalUi()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 26 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            Require<VisualMotionProfileLibrary>("VisualMotionProfileLibrary");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();

            var snapshot = RequireSnapshot(driver);
            if (!layout.AreProductionPanelsInRightSidebar())
                throw new InvalidOperationException("Stage 26 command UI must remain in the PC right-sidebar layout.");
            if (desktopHud.showDebugOverlay || !debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 26 debug panels must remain hidden by default.");

            if (FindActor(snapshot, "tech_center", 1) == null || FindActor(snapshot, "advanced_power_plant", 1) == null)
                throw new InvalidOperationException("Stage 26 vertical slice needs powered tech support for helipad/aircraft pacing.");
            if (FindActor(snapshot, "dual_helipad", 1) == null)
                throw new InvalidOperationException("Stage 26 vertical slice must include a player dual helipad.");
            var attackAircraft = FindActor(snapshot, "attack_aircraft", 1);
            if (attackAircraft == null)
                throw new InvalidOperationException("Stage 26 vertical slice must include a player attack aircraft.");
            if (snapshot.Airfields.Count == 0 || snapshot.Aircraft.Count == 0)
                throw new InvalidOperationException("Stage 26 runtime snapshot must expose airfield and aircraft snapshot data.");
            if (snapshot.Players.Count == 0 || snapshot.Players[0].Power.State != PlayerPowerState.Normal)
                throw new InvalidOperationException("Stage 26 vertical slice should remain normally powered after adding air assets.");

            var aircraftProfile = actorRenderer.motionProfileLibrary == null ? null : actorRenderer.motionProfileLibrary.GetProfile("attack_aircraft", VisualMotionCategory.Aircraft, attackAircraft.VisualMotionProfileId);
            if (aircraftProfile == null || aircraftProfile.category != VisualMotionCategory.Aircraft)
                throw new InvalidOperationException("Stage 26 attack aircraft must resolve an aircraft visual motion profile.");
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 26 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage26-medium-checks.ps1") ||
                !content.Contains("run-unity-stage26-validation.ps1"))
                throw new InvalidOperationException("Stage 26 medium recursion audit does not include Stage 26.");
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 26 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return snapshot.Actors[i];
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
