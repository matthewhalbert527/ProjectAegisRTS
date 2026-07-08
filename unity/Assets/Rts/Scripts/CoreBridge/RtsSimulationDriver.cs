using System;
using System.Collections.Generic;
using ProjectAegisRTS.Ai;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.Support;
using ProjectAegisRTS.UnityClient.Feedback;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.CoreBridge
{
    public sealed class RtsSimulationDriver : MonoBehaviour
    {
        public const string SkirmishDifficultyPlayerPrefsKey = "ProjectAegisRTS.SkirmishDifficulty";
        public const string GeneratedSkirmishEnabledPlayerPrefsKey = "ProjectAegisRTS.GeneratedSkirmishEnabled";
        public const string GeneratedSkirmishSeedPlayerPrefsKey = "ProjectAegisRTS.GeneratedSkirmishSeed";

        [SerializeField] int playerId = 1;
        [SerializeField] bool useCombatDemoWorld;
        [SerializeField] bool useEconomyDemoWorld;
        [SerializeField] bool useFogRadarDemoWorld;
        [SerializeField] bool useAiSkirmishDemoWorld;
        [SerializeField] bool useMapTerrainDemoWorld;
        [SerializeField] bool useVerticalSliceDemoWorld;
        [SerializeField] bool useGeneratedSkirmishWorld;
        [SerializeField] bool usePlayerPerspectiveSnapshot;
        [SerializeField] string skirmishDifficultyId = "normal";
        [SerializeField] int generatedSkirmishSeed = 34034;
        public FeedbackEventBus feedbackEventBus;

        readonly List<int> selectedActorIds = new List<int>();
        RtsWorld world;
        WorldSnapshot latestSnapshot;
        Int2 hoveredCell;
        bool hasHoveredCell;
        bool hoveredCellIsPlacementCell;
        string pendingPlacementTypeId = string.Empty;
        bool forceLowPower;
        float tickAccumulator;

        public int TicksPerSecond { get; private set; }
        public bool IsPaused { get; private set; }
        public WorldSnapshot LatestSnapshot { get { return latestSnapshot; } }
        public RtsRules Rules { get { return world == null ? null : world.Rules; } }
        public IReadOnlyList<int> SelectedActorIds { get { return selectedActorIds; } }
        public bool HasHoveredCell { get { return hasHoveredCell; } }
        public Int2 HoveredCell { get { return hoveredCell; } }
        public bool HoveredCellIsPlacementCell { get { return hoveredCellIsPlacementCell; } }
        public Int2 HoveredCoarseCell { get { return hoveredCellIsPlacementCell ? PlacementGridMetrics.PlacementCellToCoarseCell(hoveredCell) : hoveredCell; } }
        public Int2 HoveredPlacementCell { get { return hoveredCellIsPlacementCell ? hoveredCell : PlacementGridMetrics.CoarseCellToPlacementCell(hoveredCell); } }
        public bool HasPlacementMode { get { return !string.IsNullOrEmpty(pendingPlacementTypeId); } }
        public string PendingPlacementTypeId { get { return pendingPlacementTypeId; } }
        public int PlayerId { get { return playerId; } }
        public bool UseCombatDemoWorld { get { return useCombatDemoWorld; } set { useCombatDemoWorld = value; } }
        public bool UseEconomyDemoWorld { get { return useEconomyDemoWorld; } set { useEconomyDemoWorld = value; } }
        public bool UseFogRadarDemoWorld { get { return useFogRadarDemoWorld; } set { useFogRadarDemoWorld = value; } }
        public bool UseAiSkirmishDemoWorld { get { return useAiSkirmishDemoWorld; } set { useAiSkirmishDemoWorld = value; } }
        public bool UseMapTerrainDemoWorld { get { return useMapTerrainDemoWorld; } set { useMapTerrainDemoWorld = value; } }
        public bool UseVerticalSliceDemoWorld { get { return useVerticalSliceDemoWorld; } set { useVerticalSliceDemoWorld = value; } }
        public bool UseGeneratedSkirmishWorld { get { return useGeneratedSkirmishWorld; } set { useGeneratedSkirmishWorld = value; } }
        public bool UsePlayerPerspectiveSnapshot { get { return usePlayerPerspectiveSnapshot; } set { usePlayerPerspectiveSnapshot = value; } }
        public string SkirmishDifficultyId { get { return skirmishDifficultyId; } }
        public string SkirmishDifficultyLabel { get { return GetSkirmishDifficultyLabel(skirmishDifficultyId); } }
        public int GeneratedSkirmishSeed { get { return generatedSkirmishSeed; } }

        public string CommandMode
        {
            get
            {
                if (HasPlacementMode)
                    return "Placement: " + pendingPlacementTypeId;

                return IsPaused ? "Paused" : "Command";
            }
        }

        public void Initialize(int ticksPerSecond, bool startPaused)
        {
            TicksPerSecond = Math.Max(1, ticksPerSecond);
            IsPaused = startPaused;
            ResetDemoWorld();
        }

        public void ManualUpdate(float deltaTime)
        {
            if (world == null || IsPaused)
                return;

            tickAccumulator += Mathf.Max(0f, deltaTime);
            var tickLength = 1f / Math.Max(1, TicksPerSecond);
            var ticksThisFrame = 0;
            while (tickAccumulator >= tickLength && ticksThisFrame < 8)
            {
                world.Tick();
                tickAccumulator -= tickLength;
                ticksThisFrame++;
            }

            if (ticksThisFrame > 0)
                RefreshSnapshot();
        }

        public RtsCommandResult TogglePause()
        {
            IsPaused = !IsPaused;
            return RtsCommandResult.Ok(IsPaused ? "Simulation paused." : "Simulation running.");
        }

        public RtsCommandResult StepOneTick()
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            world.Tick();
            RefreshSnapshot();
            return RtsCommandResult.Ok("Advanced one deterministic tick.");
        }

        public RtsCommandResult ResetDemoWorld()
        {
            ResolveGeneratedSkirmishPreferences();

            if (useGeneratedSkirmishWorld)
            {
                usePlayerPerspectiveSnapshot = true;
                world = DemoWorldFactory.CreateGeneratedSkirmishWorld(generatedSkirmishSeed, ResolveSkirmishDifficulty());
            }
            else if (useVerticalSliceDemoWorld)
                world = DemoWorldFactory.CreateVerticalSliceWorld(ResolveSkirmishDifficulty());
            else if (useMapTerrainDemoWorld)
                world = DemoWorldFactory.CreateMapTerrainDemoWorld();
            else if (useAiSkirmishDemoWorld)
                world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            else if (useFogRadarDemoWorld)
                world = DemoWorldFactory.CreateFogRadarDemoWorld();
            else if (useEconomyDemoWorld)
                world = DemoWorldFactory.CreateEconomyDemoWorld();
            else
                world = useCombatDemoWorld ? DemoWorldFactory.CreateCombatDemoWorld() : DemoWorldFactory.CreateMvpWorld();
            selectedActorIds.Clear();
            pendingPlacementTypeId = string.Empty;
            forceLowPower = false;
            tickAccumulator = 0f;
            RefreshSnapshot();
            if (feedbackEventBus != null)
                feedbackEventBus.ResetSnapshotTracking();
            return RtsCommandResult.Ok(ResetWorldMessage());
        }

        public RtsCommandResult SetSkirmishDifficulty(string difficultyId, bool resetWorld)
        {
            skirmishDifficultyId = NormalizeSkirmishDifficultyId(difficultyId);
            PlayerPrefs.SetString(SkirmishDifficultyPlayerPrefsKey, skirmishDifficultyId);
            PlayerPrefs.Save();

            if (resetWorld && (useVerticalSliceDemoWorld || useAiSkirmishDemoWorld))
                return ResetDemoWorld();

            return RtsCommandResult.Ok("Skirmish difficulty set to " + SkirmishDifficultyLabel + ".");
        }

        public RtsCommandResult SetGeneratedSkirmishEnabled(bool enabled, bool resetWorld)
        {
            useGeneratedSkirmishWorld = enabled;
            PlayerPrefs.SetInt(GeneratedSkirmishEnabledPlayerPrefsKey, useGeneratedSkirmishWorld ? 1 : 0);
            PlayerPrefs.Save();

            if (resetWorld)
                return ResetDemoWorld();

            return RtsCommandResult.Ok(useGeneratedSkirmishWorld ? "Generated skirmish maps enabled." : "Generated skirmish maps disabled.");
        }

        public RtsCommandResult SetGeneratedSkirmishSeed(int seed, bool resetWorld)
        {
            generatedSkirmishSeed = NormalizeGeneratedSkirmishSeed(seed);
            PlayerPrefs.SetInt(GeneratedSkirmishSeedPlayerPrefsKey, generatedSkirmishSeed);
            PlayerPrefs.Save();

            if (resetWorld && useGeneratedSkirmishWorld)
                return ResetDemoWorld();

            return RtsCommandResult.Ok("Generated skirmish seed set to " + generatedSkirmishSeed + ".");
        }

        public RtsCommandResult RandomizeGeneratedSkirmishSeed(bool resetWorld)
        {
            var seed = unchecked(Environment.TickCount ^ (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF));
            return SetGeneratedSkirmishSeed(seed, resetWorld);
        }

        public RtsCommandResult TryCreateCombatDemoWorld()
        {
            useCombatDemoWorld = true;
            useEconomyDemoWorld = false;
            useFogRadarDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            useMapTerrainDemoWorld = false;
            useVerticalSliceDemoWorld = false;
            useGeneratedSkirmishWorld = false;
            SaveGeneratedSkirmishEnabledPreference();
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateEconomyDemoWorld()
        {
            useEconomyDemoWorld = true;
            useCombatDemoWorld = false;
            useFogRadarDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            useMapTerrainDemoWorld = false;
            useVerticalSliceDemoWorld = false;
            useGeneratedSkirmishWorld = false;
            SaveGeneratedSkirmishEnabledPreference();
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateFogRadarDemoWorld()
        {
            useFogRadarDemoWorld = true;
            useEconomyDemoWorld = false;
            useCombatDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            useMapTerrainDemoWorld = false;
            useVerticalSliceDemoWorld = false;
            useGeneratedSkirmishWorld = false;
            usePlayerPerspectiveSnapshot = true;
            SaveGeneratedSkirmishEnabledPreference();
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateAiSkirmishDemoWorld()
        {
            useAiSkirmishDemoWorld = true;
            useFogRadarDemoWorld = false;
            useEconomyDemoWorld = false;
            useCombatDemoWorld = false;
            useMapTerrainDemoWorld = false;
            useVerticalSliceDemoWorld = false;
            useGeneratedSkirmishWorld = false;
            usePlayerPerspectiveSnapshot = false;
            SaveGeneratedSkirmishEnabledPreference();
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateMapTerrainDemoWorld()
        {
            useMapTerrainDemoWorld = true;
            useAiSkirmishDemoWorld = false;
            useFogRadarDemoWorld = false;
            useEconomyDemoWorld = false;
            useCombatDemoWorld = false;
            useVerticalSliceDemoWorld = false;
            useGeneratedSkirmishWorld = false;
            usePlayerPerspectiveSnapshot = false;
            SaveGeneratedSkirmishEnabledPreference();
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateVerticalSliceWorld()
        {
            useVerticalSliceDemoWorld = true;
            useMapTerrainDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            useFogRadarDemoWorld = false;
            useEconomyDemoWorld = false;
            useCombatDemoWorld = false;
            useGeneratedSkirmishWorld = false;
            usePlayerPerspectiveSnapshot = true;
            SaveGeneratedSkirmishEnabledPreference();
            return ResetDemoWorld();
        }

        public RtsCommandResult TryCreateGeneratedSkirmishWorld()
        {
            useGeneratedSkirmishWorld = true;
            useVerticalSliceDemoWorld = false;
            useMapTerrainDemoWorld = false;
            useAiSkirmishDemoWorld = false;
            useFogRadarDemoWorld = false;
            useEconomyDemoWorld = false;
            useCombatDemoWorld = false;
            usePlayerPerspectiveSnapshot = true;
            SaveGeneratedSkirmishEnabledPreference();
            return ResetDemoWorld();
        }

        public RtsCommandResult TryStartMatch()
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var result = RtsCommandResult.FromCore("Start match", world.StartMatch());
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryResetMatch()
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var result = RtsCommandResult.FromCore("Reset match", world.ResetMatch());
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryApplyScenarioDamage(int actorId, int damage, string reason)
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var result = RtsCommandResult.FromCore("Scenario damage", world.ApplyScenarioDamage(playerId, new ActorId(actorId), damage, reason));
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryGrantScenarioCredits(int credits)
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var result = RtsCommandResult.FromCore("Grant credits", world.GrantScenarioCredits(playerId, credits, "unity_debug"));
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryRevealScenarioMap()
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var result = RtsCommandResult.FromCore("Reveal map", world.RevealScenarioMap(playerId));
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryDestroyEnemyBaseForScenario()
        {
            int actorId;
            if (!TryFindAliveActorOfType("fabrication_hub", 2, out actorId))
                return RtsCommandResult.Fail("EnemyBaseMissing", "No enemy fabrication hub is available.");

            return TryApplyScenarioDamage(actorId, 9999, "scenario_destroy_enemy_base");
        }

        public RtsCommandResult TryDestroyPlayerBaseForScenario()
        {
            int actorId;
            if (!TryFindAliveActorOfType("fabrication_hub", playerId, out actorId))
                return RtsCommandResult.Fail("PlayerBaseMissing", "No player fabrication hub is available.");

            return TryApplyScenarioDamage(actorId, 9999, "scenario_destroy_player_base");
        }

        public bool TryFindAliveActorOfType(string typeId, int ownerId, out int actorId)
        {
            actorId = 0;
            if (world == null)
                return false;

            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                if (actor.TypeId == typeId && actor.OwnerPlayerId == ownerId && !actor.IsDestroyed)
                {
                    actorId = actor.Id.Value;
                    return true;
                }
            }

            return false;
        }

        public bool TryFindFirstEnemyCombatActor(out int actorId)
        {
            actorId = 0;
            if (world == null || Rules == null)
                return false;

            if (latestSnapshot != null)
            {
                for (var i = 0; i < latestSnapshot.Actors.Count; i++)
                {
                    var snapshot = latestSnapshot.Actors[i];
                    ActorDefinition snapshotDefinition;
                    if (snapshot.OwnerId != playerId && !snapshot.IsDestroyed && Rules.TryGetDefinition(snapshot.TypeId, out snapshotDefinition) && snapshotDefinition is UnitDefinition && snapshotDefinition.Weapon != null)
                    {
                        actorId = snapshot.ActorId;
                        return true;
                    }
                }

                for (var i = 0; i < latestSnapshot.Actors.Count; i++)
                {
                    var snapshot = latestSnapshot.Actors[i];
                    ActorDefinition snapshotDefinition;
                    if (snapshot.OwnerId != playerId && !snapshot.IsDestroyed && Rules.TryGetDefinition(snapshot.TypeId, out snapshotDefinition) && snapshotDefinition.Weapon != null)
                    {
                        actorId = snapshot.ActorId;
                        return true;
                    }
                }
            }

            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                ActorDefinition definition;
                if (actor.OwnerPlayerId != playerId && !actor.IsDestroyed && Rules.TryGetDefinition(actor.TypeId, out definition) && definition is UnitDefinition && definition.Weapon != null)
                {
                    actorId = actor.Id.Value;
                    return true;
                }
            }

            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                ActorDefinition definition;
                if (actor.OwnerPlayerId != playerId && !actor.IsDestroyed && Rules.TryGetDefinition(actor.TypeId, out definition) && definition.Weapon != null)
                {
                    actorId = actor.Id.Value;
                    return true;
                }
            }

            return false;
        }

        public RtsCommandResult TrySelectFirstOwnedActorOfType(string typeId)
        {
            int actorId;
            if (!TryFindAliveActorOfType(typeId, playerId, out actorId))
                return RtsCommandResult.Fail("ActorMissing", "No owned actor of type " + typeId + " is available.");

            return SetSelectedActorIds(new[] { actorId });
        }

        public RtsCommandResult TrySelectFirstOwnedCombatActor()
        {
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                ActorDefinition definition;
                if (actor.OwnerPlayerId == playerId && !actor.IsDestroyed && Rules.TryGetDefinition(actor.TypeId, out definition) && definition.Weapon != null)
                    return SetSelectedActorIds(new[] { actor.Id.Value });
            }

            return RtsCommandResult.Fail("CombatActorMissing", "No owned combat actor is available.");
        }

        public RtsCommandResult TrySelectOwnedCombatGroup()
        {
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var actorIds = new List<int>();
            foreach (var pair in world.Actors)
            {
                var actor = pair.Value;
                ActorDefinition definition;
                if (actor.OwnerPlayerId == playerId &&
                    !actor.IsDestroyed &&
                    Rules.TryGetDefinition(actor.TypeId, out definition) &&
                    definition is UnitDefinition &&
                    definition.Weapon != null)
                    actorIds.Add(actor.Id.Value);
            }

            if (actorIds.Count == 0)
                return RtsCommandResult.Fail("CombatActorMissing", "No owned combat units are available.");

            actorIds.Sort();
            return SetSelectedActorIds(actorIds);
        }

        public void SetHoveredCell(Int2 cell)
        {
            SetHoveredCell(cell, false);
        }

        public void SetHoveredCell(Int2 cell, bool isPlacementCell)
        {
            hoveredCell = cell;
            hoveredCellIsPlacementCell = isPlacementCell;
            hasHoveredCell = true;
        }

        public void ClearHoveredCell()
        {
            hasHoveredCell = false;
            hoveredCellIsPlacementCell = false;
        }

        public RtsCommandResult TrySelectActorAtCell(Int2 cell)
        {
            if (latestSnapshot == null)
                return RtsCommandResult.Fail("SnapshotMissing", "No world snapshot is available.");

            int actorId;
            if (!TryFindActorAtCell(cell, out actorId))
            {
                selectedActorIds.Clear();
                var cleared = RtsCommandResult.Ok("Selection cleared.");
                EmitCommandFeedback(FeedbackEventType.SelectionChanged, cleared, cell, 0, "Selection cleared");
                return cleared;
            }

            selectedActorIds.Clear();
            selectedActorIds.Add(actorId);
            var result = RtsCommandAdapter.SelectActors(world, playerId, selectedActorIds);
            EmitCommandFeedback(FeedbackEventType.SelectionChanged, result, cell, actorId, "Selection");
            return result;
        }

        public RtsCommandResult ClearSelection()
        {
            selectedActorIds.Clear();
            var result = RtsCommandResult.Ok("Selection cleared.");
            EmitCommandFeedback(FeedbackEventType.SelectionChanged, result, hasHoveredCell ? hoveredCell : Int2.Zero, 0, "Selection cleared");
            return result;
        }

        public RtsCommandResult SetSelectedActorIds(IReadOnlyList<int> actorIds)
        {
            selectedActorIds.Clear();
            if (actorIds != null)
            {
                for (var i = 0; i < actorIds.Count; i++)
                {
                    ActorSnapshot actor;
                    if (TryGetActorSnapshot(actorIds[i], out actor) && actor.OwnerId == playerId && !selectedActorIds.Contains(actor.ActorId))
                        selectedActorIds.Add(actor.ActorId);
                }
            }

            if (world == null)
            {
                var localResult = RtsCommandResult.Ok("Selected actors: " + SelectedActorIdsText() + ".");
                EmitCommandFeedback(FeedbackEventType.SelectionChanged, localResult, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Selection");
                return localResult;
            }

            var result = RtsCommandAdapter.SelectActors(world, playerId, selectedActorIds);
            EmitCommandFeedback(FeedbackEventType.SelectionChanged, result, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Selection");
            return result;
        }

        public RtsCommandResult AddOrRemoveSelectedActor(int actorId)
        {
            ActorSnapshot actor;
            if (!TryGetActorSnapshot(actorId, out actor) || actor.OwnerId != playerId)
                return RtsCommandResult.Fail("ActorUnavailable", "Selectable actor is no longer available.");

            if (selectedActorIds.Contains(actorId))
                selectedActorIds.Remove(actorId);
            else
                selectedActorIds.Add(actorId);

            if (world == null)
            {
                var localResult = RtsCommandResult.Ok("Selected actors: " + SelectedActorIdsText() + ".");
                EmitCommandFeedback(FeedbackEventType.SelectionChanged, localResult, actor.CellPosition, actorId, "Selection");
                return localResult;
            }

            var result = RtsCommandAdapter.SelectActors(world, playerId, selectedActorIds);
            EmitCommandFeedback(FeedbackEventType.SelectionChanged, result, actor.CellPosition, actorId, "Selection");
            return result;
        }

        public RtsCommandResult TryIssueMoveSelectedToCell(Int2 cell)
        {
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select a mobile unit before issuing a move command.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, cell, 0, "Move");
                return fail;
            }

            var mobileActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (definition is UnitDefinition)
                    mobileActorIds.Add(actor.ActorId);
            }

            if (mobileActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoMobileSelection", "The current selection has no mobile units.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, cell, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Move");
                return fail;
            }

            var result = RtsCommandAdapter.IssueMoveOrder(world, playerId, mobileActorIds, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.MoveCommand, result, cell, mobileActorIds.Count > 0 ? mobileActorIds[0] : 0, "Move");
            return result;
        }

        public RtsCommandResult TryIssueAttackMoveSelectedToCell(Int2 cell)
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, cell, 0, "Attack move");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select armed mobile units before issuing attack-move.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, cell, 0, "Attack move");
                return fail;
            }

            var actorIds = SelectedMobileArmedActorIds();
            if (actorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoMobileArmedSelection", "The current selection has no armed mobile units.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, cell, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Attack move");
                return fail;
            }

            var result = RtsCommandAdapter.IssueAttackMoveOrder(world, playerId, actorIds, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Attack, result, cell, actorIds.Count > 0 ? actorIds[0] : 0, "Attack move");
            return result;
        }

        public RtsCommandResult TryIssueGuardSelected()
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, 0, "Guard");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select armed actors before issuing guard.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, 0, "Guard");
                return fail;
            }

            var actorIds = SelectedArmedActorIds();
            if (actorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoArmedSelection", "The current selection has no armed actors.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Guard");
                return fail;
            }

            var result = RtsCommandAdapter.IssueGuardOrder(world, playerId, actorIds);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Attack, result, Int2.Zero, actorIds.Count > 0 ? actorIds[0] : 0, "Guard");
            return result;
        }

        public RtsCommandResult TryIssuePatrolSelectedToCell(Int2 cell)
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, cell, 0, "Patrol");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select armed mobile units before issuing patrol.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, cell, 0, "Patrol");
                return fail;
            }

            var actorIds = SelectedMobileArmedActorIds();
            if (actorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoMobileArmedSelection", "The current selection has no armed mobile units.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, cell, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Patrol");
                return fail;
            }

            var result = RtsCommandAdapter.IssuePatrolOrder(world, playerId, actorIds, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.MoveCommand, result, cell, actorIds.Count > 0 ? actorIds[0] : 0, "Patrol");
            return result;
        }

        public RtsCommandResult TryIssueScatterSelected()
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, Int2.Zero, 0, "Scatter");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select mobile units before issuing scatter.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, Int2.Zero, 0, "Scatter");
                return fail;
            }

            var actorIds = SelectedMobileActorIds();
            if (actorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoMobileSelection", "The current selection has no mobile units.");
                EmitCommandFeedback(FeedbackEventType.MoveCommand, fail, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Scatter");
                return fail;
            }

            var result = RtsCommandAdapter.IssueScatterOrder(world, playerId, actorIds);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.MoveCommand, result, Int2.Zero, actorIds.Count > 0 ? actorIds[0] : 0, "Scatter");
            return result;
        }

        public RtsCommandResult TryIssueDeploySelected()
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, Int2.Zero, 0, "Deploy");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select actors before issuing deploy.");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, Int2.Zero, 0, "Deploy");
                return fail;
            }

            var result = RtsCommandAdapter.IssueDeployOrder(world, playerId, selectedActorIds);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Deploy");
            return result;
        }

        List<int> SelectedMobileActorIds()
        {
            var actorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (definition is UnitDefinition)
                    actorIds.Add(actor.ActorId);
            }

            return actorIds;
        }

        List<int> SelectedArmedActorIds()
        {
            var actorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (definition.Weapon != null)
                    actorIds.Add(actor.ActorId);
            }

            return actorIds;
        }

        List<int> SelectedMobileArmedActorIds()
        {
            var actorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (definition is UnitDefinition && definition.Weapon != null)
                    actorIds.Add(actor.ActorId);
            }

            return actorIds;
        }

        public RtsCommandResult TryIssueAttackSelectedToActor(int targetActorId)
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, 0, "Attack");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select an armed actor before issuing an attack command.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, 0, "Attack");
                return fail;
            }

            ActorSnapshot target;
            if (!TryGetActorSnapshot(targetActorId, out target))
            {
                var fail = RtsCommandResult.Fail("TargetMissing", "No target actor exists for the attack command.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Attack");
                return fail;
            }
            if (target.OwnerId == playerId)
            {
                var fail = RtsCommandResult.Fail("TargetFriendly", "Select an enemy actor as the attack target.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, target.CellPosition, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Attack");
                return fail;
            }

            var armedActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;
                if (definition.Weapon != null)
                    armedActorIds.Add(actor.ActorId);
            }

            if (armedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoArmedSelection", "The current selection has no armed actors.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, target.CellPosition, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Attack");
                return fail;
            }

            var result = RtsCommandAdapter.IssueAttackOrder(world, playerId, armedActorIds, targetActorId);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Attack, result, target.CellPosition, armedActorIds.Count > 0 ? armedActorIds[0] : 0, "Attack");
            return result;
        }

        public RtsCommandResult TryIssueDebugAttackSelectedToKnownActor(int targetActorId)
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, 0, "Debug attack");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select an armed actor before issuing a debug attack command.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, 0, "Debug attack");
                return fail;
            }

            ActorState target;
            if (!world.Actors.TryGetValue(targetActorId, out target) || target.IsDestroyed)
            {
                var fail = RtsCommandResult.Fail("TargetMissing", "No live target actor exists for the debug attack command.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Debug attack");
                return fail;
            }
            if (target.OwnerPlayerId == playerId)
            {
                var fail = RtsCommandResult.Fail("TargetFriendly", "Select an enemy actor as the debug attack target.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, target.CellPosition, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Debug attack");
                return fail;
            }
            ActorDefinition targetDefinition;
            if (!Rules.TryGetDefinition(target.TypeId, out targetDefinition))
            {
                var fail = RtsCommandResult.Fail("TargetDefinitionMissing", "No target definition exists for the debug attack command.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, target.CellPosition, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Debug attack");
                return fail;
            }

            var armedActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorState actor;
                ActorDefinition definition;
                if (!world.Actors.TryGetValue(selectedActorIds[i], out actor) || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;
                if (actor.OwnerPlayerId != playerId || definition.Weapon == null)
                    continue;
                if (targetDefinition.Kind == ActorKind.Building && !definition.Weapon.CanTargetBuildings)
                    continue;
                if (targetDefinition.Kind == ActorKind.Unit && !definition.Weapon.CanTargetUnits)
                    continue;

                var distance = actor.CellPosition.ManhattanDistanceTo(target.CellPosition);
                if (distance < definition.Weapon.MinRangeCells || distance > definition.Weapon.RangeCells)
                    continue;

                if (definition.Weapon != null)
                    armedActorIds.Add(actor.Id.Value);
            }

            if (armedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoArmedSelection", "The current selection has no armed actors.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, target.CellPosition, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Debug attack");
                return fail;
            }

            var result = RtsCommandAdapter.IssueAttackOrder(world, playerId, armedActorIds, targetActorId);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Attack, result, target.CellPosition, armedActorIds.Count > 0 ? armedActorIds[0] : 0, "Debug attack");
            return result;
        }

        public RtsCommandResult TryIssueAttackSelectedAtCell(Int2 cell)
        {
            int targetActorId;
            if (!TryFindTargetActorAtCell(cell, out targetActorId))
            {
                var fail = RtsCommandResult.Fail("NoAttackTarget", "No enemy actor is present at " + cell + ".");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, cell, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Attack");
                return fail;
            }

            return TryIssueAttackSelectedToActor(targetActorId);
        }

        public RtsCommandResult TryIssueForceAttackSelectedAtCell(Int2 cell)
        {
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select an actor before issuing force-attack.");
                EmitCommandFeedback(FeedbackEventType.Attack, fail, cell, 0, "Force attack");
                return fail;
            }

            var result = RtsCommandAdapter.IssueForceAttackCell(world, playerId, selectedActorIds, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Attack, result, cell, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Force attack");
            return result;
        }

        public RtsCommandResult TryIssueHarvestSelectedAtCell(Int2 cell)
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Harvest, fail, cell, 0, "Harvest");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select a harvester before issuing a harvest command.");
                EmitCommandFeedback(FeedbackEventType.Harvest, fail, cell, 0, "Harvest");
                return fail;
            }

            var harvesterActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                if (TryGetActorSnapshot(selectedActorIds[i], out actor) && actor.OwnerId == playerId && actor.TypeId == "harvester" && !actor.IsDestroyed)
                    harvesterActorIds.Add(actor.ActorId);
            }

            if (harvesterActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoHarvesterSelection", "The current selection has no harvesters.");
                EmitCommandFeedback(FeedbackEventType.Harvest, fail, cell, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Harvest");
                return fail;
            }

            var result = RtsCommandAdapter.IssueHarvestOrder(world, playerId, harvesterActorIds, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Harvest, result, cell, harvesterActorIds.Count > 0 ? harvesterActorIds[0] : 0, "Harvest");
            return result;
        }

        public RtsCommandResult TryReturnSelectedHarvesters()
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Unload, fail, Int2.Zero, 0, "Return to refinery");
                return fail;
            }
            if (selectedActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoSelection", "Select a harvester before returning to refinery.");
                EmitCommandFeedback(FeedbackEventType.Unload, fail, Int2.Zero, 0, "Return to refinery");
                return fail;
            }

            var harvesterActorIds = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                ActorSnapshot actor;
                if (TryGetActorSnapshot(selectedActorIds[i], out actor) && actor.OwnerId == playerId && actor.TypeId == "harvester" && !actor.IsDestroyed)
                    harvesterActorIds.Add(actor.ActorId);
            }

            if (harvesterActorIds.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoHarvesterSelection", "The current selection has no harvesters.");
                EmitCommandFeedback(FeedbackEventType.Unload, fail, Int2.Zero, selectedActorIds.Count > 0 ? selectedActorIds[0] : 0, "Return to refinery");
                return fail;
            }

            var result = RtsCommandAdapter.ReturnToRefinery(world, playerId, harvesterActorIds);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Unload, result, Int2.Zero, harvesterActorIds.Count > 0 ? harvesterActorIds[0] : 0, "Return to refinery");
            return result;
        }

        public RtsCommandResult TryStopSelectedCombat()
        {
            return TryStopSelected();
        }

        public RtsCommandResult TryQueueProduction(string typeId)
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.ProductionStarted, fail, Int2.Zero, 0, "Production");
                return fail;
            }

            ActorDefinition targetDefinition;
            if (!Rules.TryGetDefinition(typeId, out targetDefinition))
            {
                var fail = RtsCommandResult.Fail("UnknownActorType", "Unknown production type: " + typeId);
                EmitCommandFeedback(FeedbackEventType.ProductionStarted, fail, Int2.Zero, 0, "Production");
                return fail;
            }

            if (targetDefinition is BuildingDefinition && HasCompletedPendingPlacement(typeId))
            {
                pendingPlacementTypeId = typeId;
                var placementResult = RtsCommandResult.Ok("Placement mode entered for " + typeId + ".");
                EmitCommandFeedback(FeedbackEventType.ProductionCompleted, placementResult, Int2.Zero, 0, "Placement mode");
                return placementResult;
            }

            var factoryTypeId = targetDefinition.Production.FactoryTypeId;
            if (string.IsNullOrEmpty(factoryTypeId))
            {
                var fail = RtsCommandResult.Fail("NoFactory", typeId + " is not buildable in the Stage 0 rules.");
                EmitCommandFeedback(FeedbackEventType.ProductionStarted, fail, Int2.Zero, 0, "Production");
                return fail;
            }

            int producerActorId;
            if (!TryFindOwnedActorOfType(factoryTypeId, out producerActorId))
            {
                var fail = RtsCommandResult.Fail("ProducerMissing", "Build " + factoryTypeId + " before producing " + typeId + ".");
                EmitCommandFeedback(FeedbackEventType.ProductionStarted, fail, Int2.Zero, 0, "Production");
                return fail;
            }

            var missingPrerequisite = GetMissingProductionPrerequisiteTypeId(typeId);
            if (!string.IsNullOrEmpty(missingPrerequisite))
            {
                var fail = RtsCommandResult.Fail("MissingPrerequisite", "Build " + missingPrerequisite + " before producing " + typeId + ".");
                EmitCommandFeedback(FeedbackEventType.ProductionStarted, fail, Int2.Zero, producerActorId, "Production");
                return fail;
            }

            var result = RtsCommandAdapter.BeginProduction(world, playerId, producerActorId, typeId);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.ProductionStarted, result, Int2.Zero, producerActorId, "Production");
            return result;
        }

        public RtsCommandResult TryEnterPlacementModeForFirstPending()
        {
            var player = GetLocalPlayerSnapshot();
            if (player == null)
                return RtsCommandResult.Fail("PlayerMissing", "No local player snapshot is available.");

            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                ActorDefinition definition;
                if (item.State == "CompletedPendingPlacement" &&
                    Rules.TryGetDefinition(item.TypeId, out definition) &&
                    definition is BuildingDefinition)
                {
                    pendingPlacementTypeId = item.TypeId;
                    return RtsCommandResult.Ok("Placement mode entered for " + item.TypeId + ".");
                }
            }

            return RtsCommandResult.Fail("NoPendingPlacement", "No completed building is waiting for placement.");
        }

        public RtsCommandResult TryPlacePendingBuildingAtCell(Int2 cell)
        {
            if (!HasPlacementMode)
            {
                var fail = RtsCommandResult.Fail("PlacementInactive", "No pending building placement is active.");
                EmitCommandFeedback(FeedbackEventType.BuildingPlaced, fail, cell, 0, "Building placement");
                return fail;
            }

            var typeId = pendingPlacementTypeId;
            var result = RtsCommandAdapter.PlaceBuilding(world, playerId, typeId, cell);
            if (result.Success)
                pendingPlacementTypeId = string.Empty;

            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.BuildingPlaced, result, cell, 0, "Building placement");
            return result;
        }

        public RtsCommandResult TryPlacePendingBuildingNearHoveredCell(int searchRadius)
        {
            if (!HasPlacementMode)
                return RtsCommandResult.Fail("PlacementInactive", "No pending building placement is active.");
            if (!hasHoveredCell)
                return RtsCommandResult.Fail("NoHoveredCell", "Hover a board cell before placing.");

            Int2 placementCell;
            if (!TryFindNearestValidPlacementCell(HoveredPlacementCell, Mathf.Max(1, searchRadius), out placementCell))
                return RtsCommandResult.Fail("NoNearbyPlacement", "No legal placement slot was found near the cursor. Use Place Suggested or move closer to your base.");

            return TryPlacePendingBuildingAtCell(placementCell);
        }

        public RtsCommandResult TryPlacePendingBuildingAtSuggestedCell()
        {
            if (!HasPlacementMode)
                return RtsCommandResult.Fail("PlacementInactive", "No pending building placement is active.");

            Int2 placementCell;
            if (!TryFindSuggestedPlacementCell(out placementCell))
                return RtsCommandResult.Fail("NoSuggestedPlacement", "No legal placement slot is available near your base.");

            return TryPlacePendingBuildingAtCell(placementCell);
        }

        public bool TryFindSuggestedPlacementCell(out Int2 placementCell)
        {
            placementCell = Int2.Zero;
            if (!HasPlacementMode || world == null || latestSnapshot == null || latestSnapshot.Map == null)
                return false;

            var origin = SuggestedPlacementOrigin();
            var bestScore = int.MaxValue;
            var found = false;
            for (var y = 0; y < latestSnapshot.Map.PlacementHeight; y++)
            {
                for (var x = 0; x < latestSnapshot.Map.PlacementWidth; x++)
                {
                    var candidate = new Int2(x, y);
                    var preview = world.PreviewPlacement(playerId, pendingPlacementTypeId, candidate);
                    if (preview == null || !preview.CanPlace)
                        continue;

                    var score = candidate.ManhattanDistanceTo(origin) * 1000 + y * latestSnapshot.Map.PlacementWidth + x;
                    if (score >= bestScore)
                        continue;

                    bestScore = score;
                    placementCell = candidate;
                    found = true;
                }
            }

            return found;
        }

        public bool TryFindNearestValidPlacementCell(Int2 origin, int searchRadius, out Int2 placementCell)
        {
            placementCell = Int2.Zero;
            if (!HasPlacementMode || world == null || latestSnapshot == null || latestSnapshot.Map == null)
                return false;

            var radius = Mathf.Max(0, searchRadius);
            var bestScore = int.MaxValue;
            var found = false;
            for (var y = Mathf.Max(0, origin.Y - radius); y <= Mathf.Min(latestSnapshot.Map.PlacementHeight - 1, origin.Y + radius); y++)
            {
                for (var x = Mathf.Max(0, origin.X - radius); x <= Mathf.Min(latestSnapshot.Map.PlacementWidth - 1, origin.X + radius); x++)
                {
                    var candidate = new Int2(x, y);
                    var distance = candidate.ManhattanDistanceTo(origin);
                    if (distance > radius)
                        continue;

                    var preview = world.PreviewPlacement(playerId, pendingPlacementTypeId, candidate);
                    if (preview == null || !preview.CanPlace)
                        continue;

                    var score = distance * 1000 + y * latestSnapshot.Map.PlacementWidth + x;
                    if (score >= bestScore)
                        continue;

                    bestScore = score;
                    placementCell = candidate;
                    found = true;
                }
            }

            return found;
        }

        public RtsCommandResult TryCancelPlacement()
        {
            if (!HasPlacementMode)
                return RtsCommandResult.Ok("No placement mode was active.");

            pendingPlacementTypeId = string.Empty;
            return RtsCommandResult.Ok("Placement mode cancelled.");
        }

        public bool IsPlacementValidAtCell(Int2 cell, out PlacementPreviewSnapshot preview)
        {
            preview = null;
            if (!HasPlacementMode || world == null)
                return false;

            var placementCell = hoveredCellIsPlacementCell && cell.Equals(hoveredCell) ? cell : PlacementGridMetrics.CoarseCellToPlacementCell(cell);
            preview = world.PreviewPlacement(playerId, pendingPlacementTypeId, placementCell);
            return preview != null && preview.CanPlace;
        }

        public RtsCommandResult TryCancelProduction(int queueItemId)
        {
            if (world == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            var result = RtsCommandAdapter.CancelProduction(world, playerId, queueItemId);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryStopSelected()
        {
            if (selectedActorIds.Count == 0)
                return RtsCommandResult.Fail("NoSelection", "Select actors before issuing Stop.");

            var result = RtsCommandAdapter.StopActors(world, playerId, selectedActorIds);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryTogglePowerSelected()
        {
            if (selectedActorIds.Count != 1)
                return RtsCommandResult.Fail("SelectionRequiresOne", "Select one building before toggling power.");

            var result = RtsCommandAdapter.TogglePower(world, playerId, selectedActorIds[0]);
            RefreshSnapshot();
            return result;
        }

        public RtsCommandResult TryBeginRepairSelectedBuilding()
        {
            int actorId;
            var validation = TryGetSingleSelectedOwnedBuilding("repairing", out actorId);
            if (!validation.Success)
                return validation;

            var result = RtsCommandAdapter.BeginRepairBuilding(world, playerId, actorId);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, Int2.Zero, actorId, "Repair");
            return result;
        }

        public RtsCommandResult TryCancelRepairSelectedBuilding()
        {
            int actorId;
            var validation = TryGetSingleSelectedOwnedBuilding("cancelling repair", out actorId);
            if (!validation.Success)
                return validation;

            var result = RtsCommandAdapter.CancelRepairBuilding(world, playerId, actorId);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, Int2.Zero, actorId, "Cancel repair");
            return result;
        }

        public RtsCommandResult TrySellSelectedBuilding()
        {
            int actorId;
            var validation = TryGetSingleSelectedOwnedBuilding("selling", out actorId);
            if (!validation.Success)
                return validation;

            var result = RtsCommandAdapter.SellBuilding(world, playerId, actorId);
            if (result.Success)
                selectedActorIds.Clear();
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, Int2.Zero, actorId, "Sell");
            return result;
        }

        public RtsCommandResult TrySetRallyPointForSelectedProducer(Int2 cell)
        {
            int actorId;
            var validation = TryGetSingleSelectedOwnedBuilding("setting a rally point", out actorId);
            if (!validation.Success)
                return validation;

            var result = RtsCommandAdapter.SetRallyPoint(world, playerId, actorId, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.MoveCommand, result, cell, actorId, "Rally point");
            return result;
        }

        public RtsCommandResult TryActivateSupportPowerAtCell(string powerId, Int2 cell)
        {
            if (world == null || Rules == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, 0, "Support power");
                return fail;
            }

            SupportPowerDefinition definition;
            if (!Rules.TryGetSupportPowerDefinition(powerId, out definition))
            {
                var fail = RtsCommandResult.Fail("UnknownSupportPower", "Unknown support power: " + powerId);
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, 0, "Support power");
                return fail;
            }

            if (definition.TargetKind == SupportPowerTargetKind.Cell && !world.Map.Contains(cell))
            {
                var fail = RtsCommandResult.Fail("SupportTargetOutsideMap", "Hover a board cell before using " + definition.DisplayName + ".");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, 0, "Support power");
                return fail;
            }

            var result = RtsCommandAdapter.ActivateSupportPower(world, playerId, powerId, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, cell, 0, definition.DisplayName);
            return result;
        }

        public RtsCommandResult TryCaptureSelectedAtCell(Int2 cell)
        {
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            int engineerActorId;
            var validation = TryGetSingleSelectedEngineer("capturing", true, out engineerActorId);
            if (!validation.Success)
                return validation;

            int targetActorId;
            if (!TryFindTargetActorAtCell(cell, out targetActorId))
            {
                var fail = RtsCommandResult.Fail("NoCaptureTarget", "No enemy building is present at " + cell + ".");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, engineerActorId, "Capture");
                return fail;
            }

            var result = RtsCommandAdapter.CaptureBuilding(world, playerId, engineerActorId, targetActorId);
            RefreshSnapshot();
            ActorSnapshot engineerAfterCapture;
            if (result.Success && !TryGetActorSnapshot(engineerActorId, out engineerAfterCapture))
                selectedActorIds.Remove(engineerActorId);
            EmitCommandFeedback(FeedbackEventType.Generic, result, cell, engineerActorId, "Capture");
            return result;
        }

        public RtsCommandResult TryEngineerRepairSelectedAtCell(Int2 cell)
        {
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            int engineerActorId;
            var validation = TryGetSingleSelectedEngineer("field-repairing", false, out engineerActorId);
            if (!validation.Success)
                return validation;

            int targetActorId;
            if (!TryFindActorAtCell(cell, out targetActorId))
            {
                var fail = RtsCommandResult.Fail("NoRepairTarget", "No owned building is present at " + cell + ".");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, engineerActorId, "Engineer repair");
                return fail;
            }

            var result = RtsCommandAdapter.EngineerRepairBuilding(world, playerId, engineerActorId, targetActorId);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, cell, engineerActorId, "Engineer repair");
            return result;
        }

        public RtsCommandResult TryLoadSelectedIntoTransportAtCell(Int2 cell)
        {
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            int transportActorId;
            if (!TryFindActorAtCell(cell, out transportActorId))
            {
                var fail = RtsCommandResult.Fail("NoTransportTarget", "No owned transport is present at " + cell + ".");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, 0, "Load");
                return fail;
            }

            ActorSnapshot transport;
            ActorDefinition transportDefinition;
            if (!TryGetActorSnapshot(transportActorId, out transport) || !Rules.TryGetDefinition(transport.TypeId, out transportDefinition) || transportDefinition.Transport == null)
            {
                var fail = RtsCommandResult.Fail("TargetNotTransport", "Click an owned transport to load passengers.");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, transportActorId, "Load");
                return fail;
            }

            var passengers = SelectedPassengerActorIds(transportActorId);
            if (passengers.Count == 0)
            {
                var fail = RtsCommandResult.Fail("NoPassengerSelection", "Select infantry passengers before loading a transport.");
                EmitCommandFeedback(FeedbackEventType.Generic, fail, cell, transportActorId, "Load");
                return fail;
            }

            var result = RtsCommandAdapter.LoadTransport(world, playerId, transportActorId, passengers);
            if (result.Success)
            {
                selectedActorIds.Clear();
                selectedActorIds.Add(transportActorId);
            }

            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, cell, transportActorId, "Load");
            return result;
        }

        public RtsCommandResult TryUnloadSelectedTransportAtCell(Int2 cell)
        {
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");

            int transportActorId;
            var validation = TryGetSingleSelectedTransport("unloading", out transportActorId);
            if (!validation.Success)
                return validation;

            var result = RtsCommandAdapter.UnloadTransport(world, playerId, transportActorId, cell);
            RefreshSnapshot();
            EmitCommandFeedback(FeedbackEventType.Generic, result, cell, transportActorId, "Unload");
            return result;
        }

        public RtsCommandResult TryForceLowPowerOrCreateLowPowerDemoCondition()
        {
            if (world == null)
            {
                var fail = RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
                EmitCommandFeedback(FeedbackEventType.LowPower, fail, Int2.Zero, 0, "Low power");
                return fail;
            }

            forceLowPower = !forceLowPower;
            world.ForcePlayerPowerState(playerId, forceLowPower ? PlayerPowerState.LowPower : (PlayerPowerState?)null);
            RefreshSnapshot();
            var result = RtsCommandResult.Ok(forceLowPower ? "Forced low-power demo state." : "Returned power state to simulation rules.");
            EmitCommandFeedback(FeedbackEventType.LowPower, result, Int2.Zero, 0, "Low power");
            return result;
        }

        Int2 SuggestedPlacementOrigin()
        {
            if (hasHoveredCell)
                return HoveredPlacementCell;

            if (latestSnapshot != null && latestSnapshot.Actors != null && Rules != null)
            {
                for (var i = 0; i < latestSnapshot.Actors.Count; i++)
                {
                    var actor = latestSnapshot.Actors[i];
                    ActorDefinition definition;
                    var building = Rules.TryGetDefinition(actor.TypeId, out definition) ? definition as BuildingDefinition : null;
                    if (actor.OwnerId == playerId && !actor.IsDestroyed && building != null && building.ProvidesConstructionRadius)
                    {
                        var footprint = actor.PlacementFootprintCells.Equals(Int2.Zero) ? building.PlacementFootprintCells : actor.PlacementFootprintCells;
                        return new Int2(actor.PlacementTopLeftCell.X + footprint.X / 2, actor.PlacementTopLeftCell.Y + footprint.Y / 2);
                    }
                }
            }

            return latestSnapshot != null && latestSnapshot.Map != null
                ? new Int2(latestSnapshot.Map.PlacementWidth / 2, latestSnapshot.Map.PlacementHeight / 2)
                : Int2.Zero;
        }

        public bool TryGetPlacementPreview(out PlacementPreviewSnapshot preview)
        {
            preview = null;
            if (!HasPlacementMode || !hasHoveredCell || world == null)
                return false;

            var placementCell = HoveredPlacementCell;
            preview = world.PreviewPlacement(playerId, pendingPlacementTypeId, placementCell);
            return true;
        }

        public bool IsActorSelected(int actorId)
        {
            return selectedActorIds.Contains(actorId);
        }

        public PlayerSnapshot GetLocalPlayerSnapshot()
        {
            if (latestSnapshot == null)
                return null;

            for (var i = 0; i < latestSnapshot.Players.Count; i++)
                if (latestSnapshot.Players[i].PlayerId == playerId)
                    return latestSnapshot.Players[i];

            return null;
        }

        public string SelectedActorIdsText()
        {
            return selectedActorIds.Count == 0 ? "none" : string.Join(", ", selectedActorIds);
        }

        public string HoveredCellText()
        {
            if (!hasHoveredCell)
                return "none";
            return hoveredCellIsPlacementCell ? hoveredCell + " fine (" + HoveredCoarseCell + " coarse)" : hoveredCell.ToString();
        }

        public bool TryGetDefinition(string typeId, out ActorDefinition definition)
        {
            definition = null;
            return Rules != null && Rules.TryGetDefinition(typeId, out definition);
        }

        public bool TryGetActorSnapshot(int actorId, out ActorSnapshot snapshot)
        {
            snapshot = null;
            if (latestSnapshot == null)
                return false;

            for (var i = 0; i < latestSnapshot.Actors.Count; i++)
            {
                if (latestSnapshot.Actors[i].ActorId == actorId)
                {
                    snapshot = latestSnapshot.Actors[i];
                    return true;
                }
            }

            return false;
        }

        public bool HasOwnedActorOfType(string typeId)
        {
            int actorId;
            return TryFindOwnedActorOfType(typeId, out actorId);
        }

        public string GetMissingProductionPrerequisiteTypeId(string typeId)
        {
            return world == null ? string.Empty : world.GetMissingProductionPrerequisiteTypeId(playerId, typeId);
        }

        public string GetMissingSupportPowerPrerequisiteTypeId(string powerId)
        {
            return world == null ? string.Empty : world.GetMissingSupportPowerPrerequisiteTypeId(playerId, powerId);
        }

        public RtsCommandResult TrySelectOwnedActorsOfSameTypeAtCell(Int2 cell)
        {
            int actorId;
            if (!TryFindActorAtCell(cell, out actorId))
                return ClearSelection();

            ActorSnapshot selected;
            if (!TryGetActorSnapshot(actorId, out selected))
                return ClearSelection();

            var actorIds = new List<int>();
            for (var i = 0; i < latestSnapshot.Actors.Count; i++)
            {
                var actor = latestSnapshot.Actors[i];
                if (actor.OwnerId == playerId && !actor.IsDestroyed && actor.TypeId == selected.TypeId)
                    actorIds.Add(actor.ActorId);
            }

            actorIds.Sort();
            return SetSelectedActorIds(actorIds);
        }

        public RtsCommandResult TrySelectActorsInScreenRect(Rect screenRect, Camera camera, BoardCoordinateMapper coordinateMapper, bool additive)
        {
            if (latestSnapshot == null || camera == null || coordinateMapper == null || Rules == null)
                return RtsCommandResult.Fail("SelectionUnavailable", "Selection marquee needs a world snapshot, camera, and board mapper.");

            var actorIds = additive ? new List<int>(selectedActorIds) : new List<int>();
            for (var i = 0; i < latestSnapshot.Actors.Count; i++)
            {
                var actor = latestSnapshot.Actors[i];
                ActorDefinition definition;
                if (actor.OwnerId != playerId || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                var worldPosition = coordinateMapper.ActorToWorldPosition(actor, definition);
                var screenPosition = camera.WorldToScreenPoint(worldPosition);
                if (screenPosition.z < 0f)
                    continue;

                if (screenRect.Contains(new Vector2(screenPosition.x, screenPosition.y)) && !actorIds.Contains(actor.ActorId))
                    actorIds.Add(actor.ActorId);
            }

            actorIds.Sort();
            if (actorIds.Count == 0)
                return additive ? RtsCommandResult.Ok("No actors added to selection.") : ClearSelection();

            return SetSelectedActorIds(actorIds);
        }

        void RefreshSnapshot()
        {
            latestSnapshot = world == null ? null : (usePlayerPerspectiveSnapshot ? world.CreateSnapshot(playerId) : world.CreateSnapshot());
        }

        AiDifficultyDefinition ResolveSkirmishDifficulty()
        {
            var saved = PlayerPrefs.GetString(SkirmishDifficultyPlayerPrefsKey, skirmishDifficultyId);
            skirmishDifficultyId = NormalizeSkirmishDifficultyId(saved);
            return AiDifficultyDefinition.CreateForId(skirmishDifficultyId);
        }

        void ResolveGeneratedSkirmishPreferences()
        {
            useGeneratedSkirmishWorld = PlayerPrefs.GetInt(GeneratedSkirmishEnabledPlayerPrefsKey, useGeneratedSkirmishWorld ? 1 : 0) != 0;
            generatedSkirmishSeed = NormalizeGeneratedSkirmishSeed(PlayerPrefs.GetInt(GeneratedSkirmishSeedPlayerPrefsKey, generatedSkirmishSeed));
        }

        void SaveGeneratedSkirmishEnabledPreference()
        {
            PlayerPrefs.SetInt(GeneratedSkirmishEnabledPlayerPrefsKey, useGeneratedSkirmishWorld ? 1 : 0);
            PlayerPrefs.Save();
        }

        string ResetWorldMessage()
        {
            if (useGeneratedSkirmishWorld)
                return "Generated skirmish world reset on " + SkirmishDifficultyLabel + " with seed " + generatedSkirmishSeed + ".";
            if (useVerticalSliceDemoWorld)
                return "Vertical slice demo world reset on " + SkirmishDifficultyLabel + ".";
            if (useMapTerrainDemoWorld)
                return "Map terrain demo world reset.";
            if (useAiSkirmishDemoWorld)
                return "AI skirmish demo world reset.";
            if (useFogRadarDemoWorld)
                return "Fog/radar demo world reset.";
            if (useEconomyDemoWorld)
                return "Economy demo world reset.";
            if (useCombatDemoWorld)
                return "Combat demo world reset.";

            return "Demo world reset.";
        }

        public static string NormalizeSkirmishDifficultyId(string difficultyId)
        {
            if (string.Equals(difficultyId, "easy", StringComparison.OrdinalIgnoreCase))
                return "easy";
            if (string.Equals(difficultyId, "hard", StringComparison.OrdinalIgnoreCase))
                return "hard";
            return "normal";
        }

        public static int NormalizeGeneratedSkirmishSeed(int seed)
        {
            return seed == 0 ? 34034 : seed;
        }

        public static string GetSkirmishDifficultyLabel(string difficultyId)
        {
            var normalized = NormalizeSkirmishDifficultyId(difficultyId);
            if (normalized == "easy")
                return "Easy";
            if (normalized == "hard")
                return "Hard";
            return "Normal";
        }

        void EmitCommandFeedback(FeedbackEventType eventType, RtsCommandResult result, Int2 cell, int sourceActorId, string label)
        {
            if (feedbackEventBus != null)
                feedbackEventBus.EmitCommandFeedback(eventType, result, cell, sourceActorId, label);
        }

        RtsCommandResult TryGetSingleSelectedOwnedBuilding(string actionLabel, out int actorId)
        {
            actorId = 0;
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
            if (selectedActorIds.Count != 1)
                return RtsCommandResult.Fail("SelectionRequiresOne", "Select one building before " + actionLabel + ".");

            ActorSnapshot actor;
            ActorDefinition definition;
            if (!TryGetActorSnapshot(selectedActorIds[0], out actor) || actor.OwnerId != playerId || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                return RtsCommandResult.Fail("SelectionInvalid", "The selected actor is no longer available.");
            if (!(definition is BuildingDefinition))
                return RtsCommandResult.Fail("SelectionRequiresBuilding", "Select a building before " + actionLabel + ".");

            actorId = actor.ActorId;
            return RtsCommandResult.Ok("Selected building accepted.");
        }

        RtsCommandResult TryGetSingleSelectedEngineer(string actionLabel, bool requireCapture, out int actorId)
        {
            actorId = 0;
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
            if (selectedActorIds.Count != 1)
                return RtsCommandResult.Fail("SelectionRequiresOne", "Select one engineer before " + actionLabel + ".");

            ActorSnapshot actor;
            ActorDefinition definition;
            if (!TryGetActorSnapshot(selectedActorIds[0], out actor) || actor.OwnerId != playerId || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                return RtsCommandResult.Fail("SelectionInvalid", "The selected actor is no longer available.");
            if (definition.Capture == null || (requireCapture && !definition.Capture.CanCaptureBuildings) || (!requireCapture && !definition.Capture.CanRepairBuildings))
                return RtsCommandResult.Fail("SelectionRequiresEngineer", "Select an engineer before " + actionLabel + ".");

            actorId = actor.ActorId;
            return RtsCommandResult.Ok("Selected engineer accepted.");
        }

        RtsCommandResult TryGetSingleSelectedTransport(string actionLabel, out int actorId)
        {
            actorId = 0;
            if (world == null || Rules == null)
                return RtsCommandResult.Fail("WorldMissing", "Simulation world has not been initialized.");
            if (selectedActorIds.Count != 1)
                return RtsCommandResult.Fail("SelectionRequiresOne", "Select one transport before " + actionLabel + ".");

            ActorSnapshot actor;
            ActorDefinition definition;
            if (!TryGetActorSnapshot(selectedActorIds[0], out actor) || actor.OwnerId != playerId || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                return RtsCommandResult.Fail("SelectionInvalid", "The selected actor is no longer available.");
            if (definition.Transport == null)
                return RtsCommandResult.Fail("SelectionRequiresTransport", "Select a transport before " + actionLabel + ".");

            actorId = actor.ActorId;
            return RtsCommandResult.Ok("Selected transport accepted.");
        }

        List<int> SelectedPassengerActorIds(int transportActorId)
        {
            var passengers = new List<int>();
            for (var i = 0; i < selectedActorIds.Count; i++)
            {
                if (selectedActorIds[i] == transportActorId)
                    continue;

                ActorSnapshot actor;
                ActorDefinition definition;
                if (!TryGetActorSnapshot(selectedActorIds[i], out actor) || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (definition is UnitDefinition && definition.Production.Kind == ProductionKind.Infantry)
                    passengers.Add(actor.ActorId);
            }

            return passengers;
        }

        bool TryFindActorAtCell(Int2 cell, out int actorId)
        {
            actorId = 0;
            if (latestSnapshot == null)
                return false;

            for (var i = latestSnapshot.Actors.Count - 1; i >= 0; i--)
            {
                var actor = latestSnapshot.Actors[i];
                ActorDefinition definition;
                if (actor.OwnerId != playerId || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (ActorCoversCell(actor, definition, cell))
                {
                    actorId = actor.ActorId;
                    return true;
                }
            }

            return false;
        }

        bool TryFindTargetActorAtCell(Int2 cell, out int actorId)
        {
            actorId = 0;
            if (latestSnapshot == null)
                return false;

            for (var i = latestSnapshot.Actors.Count - 1; i >= 0; i--)
            {
                var actor = latestSnapshot.Actors[i];
                ActorDefinition definition;
                if (actor.OwnerId == playerId || actor.IsDestroyed || !Rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (ActorCoversCell(actor, definition, cell))
                {
                    actorId = actor.ActorId;
                    return true;
                }
            }

            return false;
        }

        bool ActorCoversCell(ActorSnapshot actor, ActorDefinition definition, Int2 cell)
        {
            var building = definition as BuildingDefinition;
            if (building == null)
                return actor.CellPosition.Equals(cell);

            var placementFootprint = actor.PlacementFootprintCells.Equals(Int2.Zero) ? building.PlacementFootprintCells : actor.PlacementFootprintCells;
            var min = PlacementGridMetrics.PlacementCellToCoarseCell(actor.PlacementTopLeftCell);
            var max = PlacementGridMetrics.PlacementCellToCoarseCell(new Int2(
                actor.PlacementTopLeftCell.X + placementFootprint.X - 1,
                actor.PlacementTopLeftCell.Y + placementFootprint.Y - 1));

            return cell.X >= min.X &&
                   cell.Y >= min.Y &&
                   cell.X <= max.X &&
                   cell.Y <= max.Y;
        }

        bool TryFindOwnedActorOfType(string typeId, out int actorId)
        {
            actorId = 0;
            if (latestSnapshot == null)
                return false;

            for (var i = 0; i < latestSnapshot.Actors.Count; i++)
            {
                var actor = latestSnapshot.Actors[i];
                if (actor.OwnerId == playerId && actor.TypeId == typeId)
                {
                    actorId = actor.ActorId;
                    return true;
                }
            }

            return false;
        }

        bool HasCompletedPendingPlacement(string typeId)
        {
            var player = GetLocalPlayerSnapshot();
            if (player == null)
                return false;

            for (var i = 0; i < player.Production.Count; i++)
            {
                var item = player.Production[i];
                if (item.TypeId == typeId && item.State == "CompletedPendingPlacement")
                    return true;
            }

            return false;
        }
    }
}
