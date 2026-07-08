using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Ai;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Match;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage27SkirmishPlayabilityValidator
    {
        public static void ValidateStage27SkirmishPlayabilityBatch()
        {
            try
            {
                ValidateStage27SkirmishPlayability();
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

        public static void ValidateStage27SkirmishPlayability()
        {
            Stage26AirNavalValidator.ValidateStage26AirNaval();
            ValidateCoreSkirmishAi();
            ValidateBootDifficultyControls();
            ValidateStage16SkirmishUi();
            ValidateMediumAuditScript();
            Debug.Log("Stage 27 skirmish playability validation passed.");
        }

        static void ValidateCoreSkirmishAi()
        {
            var easy = AiDifficultyDefinition.CreateEasy();
            var normal = AiDifficultyDefinition.CreateNormal();
            var hard = AiDifficultyDefinition.CreateHard();
            if (easy.DecisionIntervalTicks <= normal.DecisionIntervalTicks || hard.DecisionIntervalTicks >= normal.DecisionIntervalTicks)
                throw new InvalidOperationException("Stage 27 AI difficulty profiles must scale decision timing.");
            if (easy.InitialAttackDelayTicks <= normal.InitialAttackDelayTicks || hard.InitialAttackDelayTicks >= normal.InitialAttackDelayTicks)
                throw new InvalidOperationException("Stage 27 AI difficulty profiles must scale first-wave timing.");
            if (!hard.EnableBuildingRepair || hard.DesiredVehicleCount <= normal.DesiredVehicleCount)
                throw new InvalidOperationException("Stage 27 Hard profile must enable repair and target more production.");

            var world = DemoWorldFactory.CreateVerticalSliceWorld(normal);
            world.StartMatch();
            RunTicks(world, normal.InitialAttackDelayTicks + normal.DecisionIntervalTicks + 20);
            var snapshot = world.CreateSnapshot();
            var ai = FirstAi(snapshot);
            if (ai == null || ai.DifficultyId != "normal" || ai.AttackWaveSequence < 1)
                throw new InvalidOperationException("Stage 27 Normal skirmish AI did not issue a timed attack wave.");
            if (!HasSuccessfulAiIntent(snapshot, "Attack", "BasicAttackWave"))
                throw new InvalidOperationException("Stage 27 timed attack wave intent did not succeed.");
            if (!AnyOwnedCombatActorHasOrder(world, 2, ActorOrderKind.AttackMove) && !AnyOwnedCombatActorHasOrder(world, 2, ActorOrderKind.Attack))
                throw new InvalidOperationException("Stage 27 attack wave did not assign pressure orders to enemy combat units.");

            var hardWorld = DemoWorldFactory.CreateVerticalSliceWorld(hard);
            hardWorld.StartMatch();
            var enemyHub = hardWorld.FirstActorOfType("fabrication_hub", 2);
            enemyHub.Health -= 120;
            hardWorld.Tick();
            if (!enemyHub.IsRepairing || !HasSuccessfulAiIntent(hardWorld.CreateSnapshot(), "Defense", "DefenseRepairDamagedBuilding"))
                throw new InvalidOperationException("Stage 27 Hard AI did not start deterministic building repair.");

            ValidateNormalVictoryPath();
        }

        static void ValidateNormalVictoryPath()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld(AiDifficultyDefinition.CreateNormal());
            world.StartMatch();
            var enemyHub = world.FirstActorOfType("fabrication_hub", 2);
            var combatUnits = CollectOwnedCombatUnitIds(world, 1);
            if (combatUnits.Count < 3)
                throw new InvalidOperationException("Stage 27 vertical slice needs at least three player combat units for non-debug victory validation.");

            for (var i = 0; i < combatUnits.Count; i++)
            {
                var move = world.IssueCommand(new IssueMoveOrderCommand(1, new[] { combatUnits[i] }, new Int2(20, 53)));
                if (!move.Success)
                    throw new InvalidOperationException("Stage 27 staging move failed: " + move.ErrorCode);
            }

            RunTicks(world, 680);

            var attackUnits = new List<ActorId>();
            for (var i = 0; i < combatUnits.Count; i++)
            {
                ActorState actor;
                if (!world.TryGetActor(combatUnits[i], out actor) || actor.IsDestroyed)
                    continue;

                var definition = world.Rules.GetDefinition(actor.TypeId);
                if (definition.Weapon != null &&
                    definition.Weapon.CanTargetBuildings &&
                    actor.CellPosition.ManhattanDistanceTo(enemyHub.CellPosition) <= definition.Weapon.RangeCells)
                    attackUnits.Add(actor.Id);
            }

            if (attackUnits.Count < 3)
                throw new InvalidOperationException("Stage 27 player combat group did not reach enemy hub attack range.");

            var attack = world.IssueCommand(new IssueAttackOrderCommand(1, attackUnits, enemyHub.Id));
            if (!attack.Success)
                throw new InvalidOperationException("Stage 27 player attack order failed: " + attack.ErrorCode);

            RunTicks(world, 900);
            var snapshot = world.CreateSnapshot(1);
            if (!enemyHub.IsDestroyed || snapshot.Match.Phase != MatchPhase.Won || snapshot.Match.LocalPlayerOutcome != PlayerOutcome.Victory)
                throw new InvalidOperationException("Stage 27 normal combat path did not reach player victory.");
            if (!HasObjectiveState(snapshot.Scenario, "destroy_enemy_base", ScenarioObjectiveState.Completed))
                throw new InvalidOperationException("Stage 27 normal combat path did not complete the destroy-base objective.");
        }

        static void ValidateBootDifficultyControls()
        {
            var previousDifficulty = PlayerPrefs.GetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, "normal");
            try
            {
                var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 27 boot scene did not open.");

                var controller = Require<GameBootController>("GameBootController");
                Require<MainMenuHud>("MainMenuHud");
                Require<OptionsMenuHud>("OptionsMenuHud");

                controller.SetSkirmishDifficulty("hard");
                if (PlayerPrefs.GetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, string.Empty) != "hard" || controller.SelectedSkirmishDifficultyLabel != "Hard")
                    throw new InvalidOperationException("Stage 27 boot menu did not save Hard skirmish difficulty.");

                controller.SetSkirmishDifficulty("normal");
                if (PlayerPrefs.GetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, string.Empty) != "normal" || controller.SelectedSkirmishDifficultyLabel != "Normal")
                    throw new InvalidOperationException("Stage 27 boot menu did not restore Normal skirmish difficulty.");
            }
            finally
            {
                PlayerPrefs.SetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, previousDifficulty);
                PlayerPrefs.Save();
            }
        }

        static void ValidateStage16SkirmishUi()
        {
            var previousDifficulty = PlayerPrefs.GetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, "normal");
            try
            {
                PlayerPrefs.SetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, "normal");
                PlayerPrefs.Save();

                var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 27 Stage16 scene did not open.");

                var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
                var objectiveHud = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
                var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
                var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
                var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

                bootstrapper.InitializeScene();
                desktopHud.Initialize();
                layout.ApplyLayout();

                var snapshot = RequireSnapshot(driver);
                var ai = FirstAi(snapshot);
                if (ai == null || ai.DifficultyId != "normal")
                    throw new InvalidOperationException("Stage 27 Stage16 did not load the saved Normal skirmish difficulty.");
                if (!objectiveHud.visible)
                    throw new InvalidOperationException("Stage 27 player objective HUD must remain visible.");
                if (!layout.AreProductionPanelsInRightSidebar())
                    throw new InvalidOperationException("Stage 27 PC right sidebar layout was not preserved.");
                if (desktopHud.showDebugOverlay || !debugVisibility.AreDebugPanelsHiddenByDefault())
                    throw new InvalidOperationException("Stage 27 debug panels must remain hidden by default.");

                var hardResult = driver.SetSkirmishDifficulty("hard", true);
                if (!hardResult.Success || FirstAi(RequireSnapshot(driver)).DifficultyId != "hard")
                    throw new InvalidOperationException("Stage 27 in-scene difficulty restart did not apply Hard.");

                var normalResult = driver.SetSkirmishDifficulty("normal", true);
                if (!normalResult.Success || FirstAi(RequireSnapshot(driver)).DifficultyId != "normal")
                    throw new InvalidOperationException("Stage 27 in-scene difficulty restart did not restore Normal.");
            }
            finally
            {
                PlayerPrefs.SetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, previousDifficulty);
                PlayerPrefs.Save();
            }
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 27 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage27-medium-checks.ps1") ||
                !content.Contains("run-unity-stage27-validation.ps1") ||
                !content.Contains("run-stage27-player-facing-checks.ps1"))
                throw new InvalidOperationException("Stage 27 medium recursion audit does not include Stage 27.");
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 27 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static AiPlayerSnapshot FirstAi(WorldSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Ai == null || snapshot.Ai.Players.Count == 0)
                return null;
            return snapshot.Ai.Players[0];
        }

        static bool HasSuccessfulAiIntent(WorldSnapshot snapshot, string kind, string intentId)
        {
            var ai = FirstAi(snapshot);
            if (ai == null)
                return false;

            for (var i = 0; i < ai.RecentIntents.Count; i++)
            {
                var intent = ai.RecentIntents[i];
                if (intent.Kind == kind && intent.IntentId == intentId && intent.WasCommandIssued && intent.CommandSucceeded)
                    return true;
            }

            return false;
        }

        static bool AnyOwnedCombatActorHasOrder(RtsWorld world, int playerId, ActorOrderKind order)
        {
            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                var definition = world.Rules.GetDefinition(actor.TypeId);
                if (actor.OwnerPlayerId == playerId && !actor.IsDestroyed && definition is UnitDefinition && definition.Weapon != null && actor.CurrentOrder == order)
                    return true;
            }

            return false;
        }

        static List<ActorId> CollectOwnedCombatUnitIds(RtsWorld world, int playerId)
        {
            var ids = new List<ActorId>();
            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                var definition = world.Rules.GetDefinition(actor.TypeId);
                if (actor.OwnerPlayerId == playerId && !actor.IsDestroyed && definition is UnitDefinition && definition.Weapon != null)
                    ids.Add(actor.Id);
            }

            return ids;
        }

        static bool HasObjectiveState(ScenarioSnapshot snapshot, string objectiveId, ScenarioObjectiveState state)
        {
            for (var i = 0; i < snapshot.Objectives.Count; i++)
                if (snapshot.Objectives[i].ObjectiveId == objectiveId && snapshot.Objectives[i].State == state)
                    return true;
            return false;
        }

        static void RunTicks(RtsWorld world, int ticks)
        {
            for (var i = 0; i < ticks; i++)
                world.Tick();
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
