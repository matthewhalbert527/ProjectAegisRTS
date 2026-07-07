using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.Ai;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Maps;
using ProjectAegisRTS.Maps.Generation;
using ProjectAegisRTS.Maps.Tiled;
using ProjectAegisRTS.MapGeneration;
using ProjectAegisRTS.Match;
using ProjectAegisRTS.Pathfinding;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Production;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.Terrain;
using ProjectAegisRTS.Visibility;

namespace ProjectAegisRTS.Tests
{
    static class Program
    {
        static int Main()
        {
            var tests = new List<Action>
            {
                WorldCreationSucceeds,
                BuildingPlacementFailsOutsideMap,
                BuildingPlacementFailsOnOccupiedFootprint,
                BuildingPlacementFailsOnActiveResourceCell,
                BuildingPlacementSucceedsOnDepletedResourceCell,
                BuildingPlacementSucceedsWithinConstructionRadius,
                BuildingDefinitionsExposeFinePlacementFootprints,
                PlacementPreviewUsesFineGrid,
                BuildingPlacementSucceedsAtFineOffset,
                FinePlacementRejectsPartialOverlap,
                PlacementSnapshotsExposeFineGridMetadata,
                ProductionDeductsCredits,
                ProductionCompletesAfterExpectedTicks,
                UnitSpawnsFromBarracks,
                MovementPathReachesDestination,
                MovementTickAdvancesDiagonally,
                LowPowerTogglesLightsAndMachineryFalse,
                DeterminismSmokeTest,
                WeaponDefinitionsExistForMvpActors,
                AttackOrderFailsForMissingTarget,
                AttackOrderFailsForDestroyedAttacker,
                AttackOrderStartsCombatForValidTarget,
                AttackMoveOrderStartsMoveAndAutoAttackFoundation,
                GuardPatrolScatterAndDeployOrdersAreAccepted,
                StopClearsStage22CommandOrders,
                SellBuildingRemovesBuildingAndRefundsCredits,
                PowerToggleChangesConsumptionAndPausesProduction,
                RepairConsumesCreditsAndRestoresHealth,
                RallyPointAffectsSpawnedUnitOrder,
                BaseManagementDeterminismSmokeTest,
                MissingPrerequisiteDisablesProductionItem,
                BarracksUnlocksInfantryProduction,
                WarFactoryUnlocksVehicleProduction,
                TechCenterUnlocksAdvancedProduction,
                RevealScanChangesVisibility,
                SupportPowerCooldownWorks,
                SupportPowerSnapshotsExposeAvailability,
                SupportPowerDeterminismSmokeTest,
                EngineerCapturesEnemyBuilding,
                EngineerCannotCaptureFriendlyBuilding,
                EngineerRepairsOwnedBuilding,
                TransportLoadsInfantry,
                TransportUnloadRestoresPassengerToBoard,
                TransportCapacityIsEnforced,
                TransportDeathDestroysPassengers,
                EngineerTransportDeterminismSmokeTest,
                WeaponCooldownPreventsContinuousFire,
                ProjectileSpawnsForProjectileWeapon,
                ProjectileImpactsAndDealsDamage,
                DamageReducesHealth,
                ActorDiesWhenHealthReachesZero,
                DestroyedActorStopsAcceptingMoveOrAttackOrders,
                GunTowerCanAttackEnemyInRange,
                CombatDeterminismSmokeTest,
                ProjectileSnapshotAppearsInWorldSnapshot,
                DeathOrDamageEventAppearsInSnapshot,
                ResourceCellsExistInEconomyDemo,
                HarvestOrderStartsHarvesterState,
                HarvesterCargoIncreases,
                ResourceAmountDecreases,
                RefineryUnloadingAddsCredits,
                HarvesterCargoDecreasesAfterUnload,
                DepletedResourceStopsHarvesting,
                StopClearsHarvestOrder,
                EconomySnapshotContainsResourceHarvesterRefinery,
                EconomyDeterminismSmokeTest,
                VisibilityInitializes,
                ActorRevealsCellsWithinSightRadius,
                ExploredCellsRemainAfterActorMovesAway,
                EnemyHiddenWhenNotVisible,
                EnemyVisibleWhenInSight,
                FogSnapshotContainsVisibilityData,
                RadarSnapshotExists,
                VisibilityDeterminismSmokeTest,
                AiInitializes,
                AiProducesDeterministicIntents,
                AiQueuesProductionWhenResourcesAllow,
                AiDoesNotIssueInvalidCommandsRepeatedly,
                AiCanIssueAttackIntentIfEnemyExists,
                AiDifficultyProfilesScaleAttackTiming,
                AiAttackWaveUsesTimedPressure,
                AiHardDifficultyAttacksEarlierThanEasy,
                AiHardDifficultyRepairsDamagedBuilding,
                AiDeterminismSmokeTest,
                TerrainDefinitionsExist,
                ImpassableTerrainBlocksPath,
                MovementClassesCanDiffer,
                PathQueryReturnsStructuredResult,
                DiagonalMovementUsesEightWayPathing,
                DiagonalMovementDoesNotCutBlockedCorners,
                MapValidationCatchesInvalidLayout,
                MapSnapshotContainsTerrainAndPathDebug,
                GeneratedMapIsDeterministic,
                GeneratedMapChangesWithSeed,
                GeneratedMapIsLeftRightSymmetric,
                GeneratedSkirmishWorldCreatesValidMap,
                GeneratedSkirmishWorldHasReachableResourcesAndEnemy,
                AegisMapAccepts100x100Maps,
                AegisMapAccepts400x400Maps,
                AegisMapRejects99x100Maps,
                AegisMapRejects401x400Maps,
                AegisMapCreatesRuntimeWorldFrom400x400,
                TiledImporterImportsSmallSample,
                TiledImporterRejectsInfiniteMaps,
                TiledImporterRejectsUnsupportedOrientation,
                TiledImporterImportsBlockers,
                TiledImporterImportsPlayerStarts,
                TiledImporterImportsActorPlacements,
                TiledExporterRoundTripsAegisMap,
                AegisMapPromptParserParsesSizePresets,
                AegisMapPromptParserParsesResourceDensity,
                AegisMapPromptParserParsesCliffDensity,
                AegisMapPromptParserParsesRockiness,
                AegisMapPromptParserHandlesUnknownWordsWithWarnings,
                AegisProceduralGeneratorCreatesSmall100,
                AegisProceduralGeneratorCreatesMedium200,
                AegisProceduralGeneratorCreatesLarge400,
                AegisProceduralGeneratorRejectsBelowMinimumSize,
                AegisProceduralGeneratorRejectsAboveMaximumSize,
                AegisProceduralGeneratorIsDeterministicForSameSeed,
                AegisProceduralGeneratorChangesWithDifferentSeeds,
                AegisProceduralGeneratedMapsPassValidator,
                AegisProceduralLowResourcesCreatesFewerFieldsThanHigh,
                AegisProceduralResourcesAvoidPlayerStartPads,
                AegisProceduralHighCliffsCreatesMoreBlockersThanLow,
                AegisProceduralRockyMapsCreateMoreRockBlockers,
                AegisProceduralEachStartHasCleanBuildPads,
                AegisProceduralPreservesPathabilityBetweenStarts,
                AegisProceduralBuildabilityAcceptsBasePads,
                AegisProceduralBuildabilityRejectsBlockersCliffsResources,
                AegisProceduralLargeMapBuildabilityCompletes,
                AegisResourceSimulationHarvestReducesAmount,
                AegisResourceSimulationHarvestReachesZeroAndDepletes,
                AegisResourceSimulationRejectsDepletedHarvest,
                AegisResourceSimulationRegenerationDelayWorks,
                AegisResourceSimulationRegeneratesAndCapsAtMax,
                AegisResourceSimulationIsDeterministic,
                RtsCoreDoesNotReferenceUnityEngine,
                PathingDeterminismSmokeTest,
                AircraftAndAirfieldDefinitionsExist,
                HelipadCreatesAirfieldSnapshot,
                HelipadSupportsAircraftProductionDockingPlaceholder,
                AircraftCanPathOverWater,
                WaterTerrainBlocksGroundUnits,
                NavalMovementClassCanPathOverWater,
                AirNavalDeterminismSmokeTest,
                VerticalSliceWorldCreates,
                VerticalSliceWorldHasPlayerBase,
                VerticalSliceWorldHasEnemyBase,
                VerticalSliceWorldHasResources,
                VerticalSliceWorldHasFog,
                VerticalSliceWorldHasAiPlayer,
                MatchStartsRunning,
                VerticalSliceCanReachVictoryWithNormalCombatPath,
                DestroyEnemyBaseTriggersVictory,
                DestroyPlayerBaseTriggersDefeat,
                ObjectiveSnapshotMatchesMatchOutcome,
                VerticalSliceDeterminismSmokeTest
            };

            var passed = 0;
            foreach (var test in tests)
            {
                try
                {
                    test();
                    Console.WriteLine("PASS " + test.Method.Name);
                    passed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FAIL " + test.Method.Name + ": " + ex.Message);
                    return 1;
                }
            }

            Console.WriteLine("All Rts.Core tests passed (" + passed + "/" + tests.Count + ").");
            return 0;
        }

        static void WorldCreationSucceeds()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            Assert(world.Map.Width == 32 && world.Map.Height == 32, "Expected 32x32 map.");
            Assert(world.FirstActorOfType("fabrication_hub", 1) != null, "Expected fabrication hub.");
            Assert(world.FirstActorOfType("scout_rover", 1) != null, "Expected scout rover.");
        }

        static void BuildingPlacementFailsOutsideMap()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", PlacementCell(31, 31)));
            Assert(!result.Success && result.ErrorCode == "OutsideMap", "Expected OutsideMap, got " + result.ErrorCode);
        }

        static void BuildingPlacementFailsOnOccupiedFootprint()
        {
            var world = WorldWithCompletedPowerPlant();
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", PlacementCell(4, 4)));
            Assert(!result.Success && result.ErrorCode == "OccupiedCell", "Expected OccupiedCell, got " + result.ErrorCode);
        }

        static void BuildingPlacementFailsOnActiveResourceCell()
        {
            var world = WorldWithCompletedPowerPlant();
            world.AddResourceCell(new Int2(8, 4), ResourceKind.Ore, 120);
            var preview = world.PreviewPlacement(1, "power_plant", PlacementCell(8, 4));
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", PlacementCell(8, 4)));
            Assert(!preview.CanPlace && preview.ErrorCode == "ResourceOccupied", "Expected preview ResourceOccupied, got " + preview.ErrorCode);
            Assert(!result.Success && result.ErrorCode == "ResourceOccupied", "Expected ResourceOccupied, got " + result.ErrorCode);
        }

        static void BuildingPlacementSucceedsOnDepletedResourceCell()
        {
            var world = WorldWithCompletedPowerPlant();
            var resourceCell = new Int2(8, 4);
            world.AddResourceCell(resourceCell, ResourceKind.Ore, 120);
            world.ResourceCells[resourceCell].Amount = 0;
            var preview = world.PreviewPlacement(1, "power_plant", PlacementCell(8, 4));
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", PlacementCell(8, 4)));
            Assert(preview.CanPlace, "Expected preview success after ore depletion: " + preview.ErrorCode);
            Assert(result.Success, "Expected placement success after ore depletion: " + result.ErrorCode);
        }

        static void BuildingPlacementSucceedsWithinConstructionRadius()
        {
            var world = WorldWithCompletedPowerPlant();
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", PlacementCell(8, 4)));
            Assert(result.Success, "Expected placement success: " + result.ErrorCode);
            Assert(world.FirstActorOfType("power_plant", 1) != null, "Expected placed power plant.");
        }

        static void BuildingDefinitionsExposeFinePlacementFootprints()
        {
            var rules = DemoRules.CreateDefaultRules();
            var powerPlant = (BuildingDefinition)rules.GetDefinition("power_plant");
            var warFactory = (BuildingDefinition)rules.GetDefinition("war_factory");
            Assert(powerPlant.FootprintCells.Equals(new Int2(2, 2)), "Expected legacy 2x2 power plant footprint.");
            Assert(powerPlant.PlacementFootprintCells.Equals(new Int2(4, 4)), "Expected fine 4x4 power plant footprint.");
            Assert(warFactory.FootprintCells.Equals(new Int2(3, 2)), "Expected legacy 3x2 war factory footprint.");
            Assert(warFactory.PlacementFootprintCells.Equals(new Int2(6, 4)), "Expected fine 6x4 war factory footprint.");
        }

        static void PlacementPreviewUsesFineGrid()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var preview = world.PreviewPlacement(1, "power_plant", PlacementCell(8, 4));
            Assert(preview.PlacementGridScale == 2, "Expected 2x placement grid scale.");
            Assert(preview.PlacementFootprintCells.Equals(new Int2(4, 4)), "Expected 4x4 fine footprint.");
            Assert(preview.FootprintCells.Count == 16, "Expected 16 fine footprint cells.");
            Assert(preview.FootprintCells[0].Equals(new Int2(16, 8)), "Expected fine top-left footprint cell.");
        }

        static void BuildingPlacementSucceedsAtFineOffset()
        {
            var world = WorldWithCompletedPowerPlant();
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", new Int2(15, 8)));
            Assert(result.Success, "Expected half-cell fine placement success: " + result.ErrorCode);
            var actor = world.FirstActorOfType("power_plant", 1);
            Assert(actor != null, "Expected placed power plant.");
            Assert(actor.PlacementTopLeftCell.Equals(new Int2(15, 8)), "Expected fine placement top-left to be retained.");
            Assert(actor.CellPosition.Equals(new Int2(7, 4)), "Expected coarse compatibility cell from fine placement.");
        }

        static void FinePlacementRejectsPartialOverlap()
        {
            var world = WorldWithCompletedPowerPlant();
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", new Int2(11, 8)));
            Assert(!result.Success && result.ErrorCode == "OccupiedCell", "Expected fine overlap rejection, got " + result.ErrorCode);
        }

        static void PlacementSnapshotsExposeFineGridMetadata()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Map.PlacementGridScale == 2, "Expected map placement grid scale in snapshot.");
            Assert(snapshot.Map.PlacementWidth == snapshot.Map.Width * 2, "Expected doubled placement map width.");
            Assert(snapshot.Map.PlacementHeight == snapshot.Map.Height * 2, "Expected doubled placement map height.");
            var hub = FindActor(snapshot, "fabrication_hub", 1);
            Assert(hub != null, "Expected fabrication hub snapshot.");
            Assert(hub.PlacementTopLeftCell.Equals(PlacementCell(4, 4)), "Expected hub fine placement top-left.");
            Assert(hub.PlacementFootprintCells.Equals(new Int2(6, 6)), "Expected hub fine footprint.");
        }

        static void ProductionDeductsCredits()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var startCredits = world.Players[1].Credits;
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            var result = world.IssueCommand(new BeginProductionCommand(1, hub.Id, "power_plant"));
            Assert(result.Success, "Expected production success.");
            Assert(world.Players[1].Credits == startCredits - 300, "Expected credits to be deducted.");
        }

        static void ProductionCompletesAfterExpectedTicks()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            world.IssueCommand(new BeginProductionCommand(1, hub.Id, "power_plant"));
            RunTicks(world, 19);
            Assert(world.Players[1].ProductionQueue[0].State == ProductionItemState.Active, "Expected production still active at tick 19.");
            world.Tick();
            Assert(world.Players[1].ProductionQueue[0].State == ProductionItemState.CompletedPendingPlacement, "Expected completed pending placement at tick 20.");
        }

        static void UnitSpawnsFromBarracks()
        {
            var world = WorldWithPlacedBarracks();
            var barracks = world.FirstActorOfType("barracks", 1);
            var result = world.IssueCommand(new BeginProductionCommand(1, barracks.Id, "rifle_infantry"));
            Assert(result.Success, "Expected infantry production start.");
            RunTicks(world, 10);
            Assert(world.FirstActorOfType("rifle_infantry", 1) != null, "Expected rifle infantry to spawn.");
        }

        static void MovementPathReachesDestination()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var scout = world.FirstActorOfType("scout_rover", 1);
            var destination = new Int2(16, 10);
            var result = world.IssueCommand(new IssueMoveOrderCommand(1, new[] { scout.Id }, destination));
            Assert(result.Success, "Expected move order success: " + result.ErrorCode);
            RunTicks(world, 96);
            Assert(scout.CellPosition.Equals(destination), "Expected scout rover to reach destination, got " + scout.CellPosition);
        }

        static void MovementTickAdvancesDiagonally()
        {
            var world = new RtsWorld(DemoRules.CreateDefaultRules(), new GridMap(8, 8));
            world.AddPlayer(1, "Diagonal Test Player", 5000);
            var scout = world.CreateActor("scout_rover", 1, new Int2(1, 1));
            var start = scout.WorldPositionFixed;

            var result = world.IssueCommand(new IssueMoveOrderCommand(1, new[] { scout.Id }, new Int2(2, 2)));
            Assert(result.Success, "Expected diagonal move order success: " + result.ErrorCode);

            world.Tick();

            Assert(scout.WorldPositionFixed.X > start.X, "Expected diagonal movement to advance on X.");
            Assert(scout.WorldPositionFixed.Y > start.Y, "Expected diagonal movement to advance on Y.");
            Assert(scout.CellPosition.Equals(new Int2(1, 1)), "Expected actor to still be between cells after one diagonal tick.");
            Assert(scout.FacingDegrees == 135, "Expected southeast diagonal facing, got " + scout.FacingDegrees);
        }

        static void LowPowerTogglesLightsAndMachineryFalse()
        {
            var world = WorldWithPlacedBarracks();
            world.ForcePlayerPowerState(1, PlayerPowerState.LowPower);
            var snapshot = world.CreateSnapshot();
            var sawBuilding = false;
            foreach (var actor in snapshot.Actors)
            {
                if (actor.TypeId == "fabrication_hub")
                {
                    sawBuilding = true;
                    Assert(!actor.LightsActive, "Expected lights inactive in low power.");
                    Assert(!actor.MachineryActive, "Expected machinery inactive in low power.");
                    Assert(actor.IsLowPower, "Expected low power flag.");
                }
            }

            Assert(sawBuilding, "Expected to inspect a building.");
        }

        static void DeterminismSmokeTest()
        {
            var a = RunDeterministicSequence();
            var b = RunDeterministicSequence();
            Assert(a == b, "Expected deterministic summaries to match.");
        }

        static void WeaponDefinitionsExistForMvpActors()
        {
            var rules = DemoRules.CreateDefaultRules();
            Assert(rules.GetDefinition("rifle_infantry").Weapon != null, "Expected rifle infantry weapon.");
            Assert(rules.GetDefinition("light_tank").Weapon != null, "Expected light tank weapon.");
            Assert(rules.GetDefinition("gun_tower").Weapon != null, "Expected gun tower weapon.");
            Assert(rules.GetDefinition("harvester").Weapon == null, "Expected harvester to be unarmed.");
            Assert(rules.GetDefinition("light_tank").Weapon.ProjectileKind == ProjectileKind.Shell, "Expected tank shell projectile.");
        }

        static void AttackOrderFailsForMissingTarget()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var attacker = world.FirstActorOfType("light_tank", 1);
            var result = world.IssueCommand(new IssueAttackOrderCommand(1, new[] { attacker.Id }, new ActorId(9999)));
            Assert(!result.Success && result.ErrorCode == "AttackTargetMissing", "Expected AttackTargetMissing, got " + result.ErrorCode);
        }

        static void AttackOrderFailsForDestroyedAttacker()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var attacker = world.FirstActorOfType("light_tank", 1);
            var target = world.FirstActorOfType("medium_tank", 2);
            attacker.IsDestroyed = true;
            attacker.Health = 0;

            var result = world.IssueCommand(new IssueAttackOrderCommand(1, new[] { attacker.Id }, target.Id));
            Assert(!result.Success && result.ErrorCode == "AttackOrderRejected", "Expected AttackOrderRejected, got " + result.ErrorCode);
        }

        static void AttackOrderStartsCombatForValidTarget()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var attacker = world.FirstActorOfType("light_tank", 1);
            var target = world.FirstActorOfType("medium_tank", 2);

            var result = world.IssueCommand(new IssueAttackOrderCommand(1, new[] { attacker.Id }, target.Id));
            Assert(result.Success, "Expected attack order success: " + result.ErrorCode);
            Assert(attacker.CurrentOrder == ActorOrderKind.Attack, "Expected attack order kind.");
            Assert(attacker.AttackTargetActorId == target.Id.Value, "Expected attack target actor id.");
            Assert(attacker.IsAttacking, "Expected attacker to be attacking.");
        }

        static void AttackMoveOrderStartsMoveAndAutoAttackFoundation()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var attacker = world.FirstActorOfType("light_tank", 1);
            var target = world.FirstActorOfType("medium_tank", 2);

            var result = world.IssueCommand(new IssueAttackMoveOrderCommand(1, new[] { attacker.Id }, attacker.CellPosition));
            Assert(result.Success, "Expected attack-move success: " + result.ErrorCode);
            Assert(attacker.CurrentOrder == ActorOrderKind.AttackMove, "Expected attack-move order kind.");
            Assert(attacker.OrderTargetCell.Equals(attacker.CellPosition), "Expected attack-move destination to be retained.");

            world.Tick();
            Assert(attacker.CurrentOrder == ActorOrderKind.AttackMove, "Expected attack-move to remain distinct from attack order.");
            Assert(attacker.AttackTargetActorId == target.Id.Value, "Expected attack-move to acquire the nearest valid in-range target.");
            Assert(attacker.IsAttacking, "Expected attack-move actor to fire opportunistically.");
        }

        static void GuardPatrolScatterAndDeployOrdersAreAccepted()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var tank = world.FirstActorOfType("light_tank", 1);
            var infantry = world.FirstActorOfType("rifle_infantry", 1);

            var guard = world.IssueCommand(new IssueGuardOrderCommand(1, new[] { tank.Id }));
            Assert(guard.Success, "Expected guard success: " + guard.ErrorCode);
            Assert(tank.CurrentOrder == ActorOrderKind.Guard, "Expected guard order kind.");
            Assert(tank.Path.Count == 0, "Expected guard to clear movement path.");

            var patrol = world.IssueCommand(new IssuePatrolOrderCommand(1, new[] { tank.Id }, new Int2(9, 8)));
            Assert(patrol.Success, "Expected patrol success: " + patrol.ErrorCode);
            Assert(tank.CurrentOrder == ActorOrderKind.Patrol, "Expected patrol order kind.");
            Assert(tank.Path.Count > 0, "Expected patrol to create a deterministic path.");

            var scatter = world.IssueCommand(new IssueScatterOrderCommand(1, new[] { infantry.Id }));
            Assert(scatter.Success, "Expected scatter success: " + scatter.ErrorCode);
            Assert(infantry.CurrentOrder == ActorOrderKind.Scatter, "Expected scatter order kind.");

            var deploy = world.IssueCommand(new IssueDeployOrderCommand(1, new[] { infantry.Id }));
            Assert(deploy.Success, "Expected deploy success: " + deploy.ErrorCode);
            Assert(infantry.CurrentOrder == ActorOrderKind.Deploy, "Expected deploy placeholder order kind.");
            Assert(infantry.Path.Count == 0, "Expected deploy placeholder to avoid stale paths.");
        }

        static void StopClearsStage22CommandOrders()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var tank = world.FirstActorOfType("light_tank", 1);

            var attackMove = world.IssueCommand(new IssueAttackMoveOrderCommand(1, new[] { tank.Id }, new Int2(9, 8)));
            Assert(attackMove.Success, "Expected attack-move setup success: " + attackMove.ErrorCode);
            var stop = world.IssueCommand(new StopCommand(1, new[] { tank.Id }));
            Assert(stop.Success, "Expected stop success: " + stop.ErrorCode);
            Assert(tank.CurrentOrder == ActorOrderKind.Stop, "Expected Stop order kind after stopping Stage22 order.");
            Assert(tank.Path.Count == 0, "Expected Stop to clear attack-move path.");
            Assert(tank.AttackTargetActorId == 0, "Expected Stop to clear attack target.");
            Assert(!tank.IsAttacking, "Expected Stop to clear attacking flag.");
        }

        static void SellBuildingRemovesBuildingAndRefundsCredits()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var tower = world.FirstActorOfType("gun_tower", 1);
            var startCredits = world.Players[1].Credits;
            var towerId = tower.Id.Value;

            var result = world.IssueCommand(new SellBuildingCommand(1, tower.Id));
            Assert(result.Success, "Expected sell success: " + result.ErrorCode);
            Assert(!world.Actors.ContainsKey(towerId), "Expected sold building to be removed from the actor table.");
            Assert(world.Players[1].Credits == startCredits + 125, "Expected 50 percent refund for gun tower.");

            var snapshot = world.CreateSnapshot();
            for (var i = 0; i < snapshot.Actors.Count; i++)
                Assert(snapshot.Actors[i].ActorId != towerId, "Expected sold building to be absent from snapshots.");
        }

        static void PowerToggleChangesConsumptionAndPausesProduction()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            var barracks = world.FirstActorOfType("barracks", 1);
            world.CreateSnapshot();
            var consumedBefore = world.Players[1].PowerConsumed;

            var production = world.IssueCommand(new BeginProductionCommand(1, barracks.Id, "rifle_infantry"));
            Assert(production.Success, "Expected infantry production setup success: " + production.ErrorCode);

            var toggle = world.IssueCommand(new PowerToggleCommand(1, barracks.Id));
            Assert(toggle.Success, "Expected power toggle success: " + toggle.ErrorCode);
            world.CreateSnapshot();
            Assert(barracks.ManuallyPoweredOff, "Expected barracks manual power-off flag.");
            Assert(world.Players[1].PowerConsumed == consumedBefore - 8, "Expected barracks power consumption to be removed.");

            world.Tick();
            var item = world.Players[1].ProductionQueue[0];
            Assert(item.State == ProductionItemState.Paused, "Expected production to pause while producer is powered off.");
            Assert(item.ProgressTicks == 0, "Expected powered-off production not to advance.");

            var snapshot = world.CreateSnapshot();
            var barracksSnapshot = FindActor(snapshot, "barracks", 1);
            Assert(barracksSnapshot.IsManuallyPoweredOff, "Expected snapshot manual power-off flag.");
            Assert(!barracksSnapshot.IsPowered, "Expected powered-off building snapshot.");
        }

        static void RepairConsumesCreditsAndRestoresHealth()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var tower = world.FirstActorOfType("gun_tower", 1);
            tower.Health -= 50;
            var startCredits = world.Players[1].Credits;

            var result = world.IssueCommand(new BeginRepairBuildingCommand(1, tower.Id));
            Assert(result.Success, "Expected repair success: " + result.ErrorCode);
            world.Tick();

            Assert(tower.Health == 660, "Expected one repair tick to restore 10 HP, got " + tower.Health);
            Assert(world.Players[1].Credits == startCredits - 5, "Expected one repair tick to spend 5 credits.");
            Assert(tower.IsRepairing, "Expected repair to continue until max health.");
            Assert(tower.RepairProgressTicks == 1, "Expected repair progress tick accounting.");

            RunTicks(world, 4);
            Assert(tower.Health == 700, "Expected repair to stop at max health.");
            Assert(!tower.IsRepairing, "Expected repair to clear at max health.");
            Assert(tower.RepairSpentCredits == 25, "Expected total repair spend for 50 HP.");

            var snapshot = world.CreateSnapshot();
            var towerSnapshot = FindActor(snapshot, "gun_tower", 1);
            Assert(!towerSnapshot.IsRepairing, "Expected repair snapshot to clear at max health.");
            Assert(towerSnapshot.RepairSpentCredits == 25, "Expected repair spend in snapshot.");
        }

        static void RallyPointAffectsSpawnedUnitOrder()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            var factory = world.FirstActorOfType("war_factory", 1);
            var rally = new Int2(16, 12);
            var existing = new HashSet<int>();
            foreach (var pair in world.Actors)
                if (pair.Value.TypeId == "light_tank" && pair.Value.OwnerPlayerId == 1)
                    existing.Add(pair.Key);

            var rallyResult = world.IssueCommand(new SetRallyPointCommand(1, factory.Id, rally));
            Assert(rallyResult.Success, "Expected rally point success: " + rallyResult.ErrorCode);
            var production = world.IssueCommand(new BeginProductionCommand(1, factory.Id, "light_tank"));
            Assert(production.Success, "Expected tank production success: " + production.ErrorCode);
            RunTicks(world, 30);

            ActorState spawned = null;
            foreach (var pair in world.Actors)
                if (pair.Value.TypeId == "light_tank" && pair.Value.OwnerPlayerId == 1 && !existing.Contains(pair.Key))
                    spawned = pair.Value;

            Assert(spawned != null, "Expected a newly spawned light tank.");
            Assert(spawned.CurrentOrder == ActorOrderKind.Move, "Expected spawned unit to receive a rally move order.");
            Assert(spawned.OrderTargetCell.Equals(rally), "Expected spawned unit order target to match rally point.");

            var snapshot = world.CreateSnapshot();
            var factorySnapshot = FindActor(snapshot, "war_factory", 1);
            Assert(factorySnapshot.RallyPoint.Equals(rally), "Expected rally point to be exposed in snapshot.");
        }

        static void BaseManagementDeterminismSmokeTest()
        {
            var a = RunBaseManagementDeterministicSequence();
            var b = RunBaseManagementDeterministicSequence();
            Assert(a == b, "Expected base-management deterministic summaries to match.");
        }

        static void MissingPrerequisiteDisablesProductionItem()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);

            Assert(world.GetMissingProductionPrerequisiteTypeId(1, "tech_center") == "comm_center", "Expected tech center to require comm center.");
            var result = world.IssueCommand(new BeginProductionCommand(1, hub.Id, "tech_center"));
            Assert(!result.Success && result.ErrorCode == "MissingPrerequisite", "Expected MissingPrerequisite for tech center, got " + result.ErrorCode);
        }

        static void BarracksUnlocksInfantryProduction()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var barracks = world.CreateActor("barracks", 1, new Int2(8, 4));

            var result = world.IssueCommand(new BeginProductionCommand(1, barracks.Id, "rifle_infantry"));
            Assert(result.Success, "Expected barracks to unlock rifle infantry production: " + result.ErrorCode);
        }

        static void WarFactoryUnlocksVehicleProduction()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var factory = world.CreateActor("war_factory", 1, new Int2(8, 6));

            var result = world.IssueCommand(new BeginProductionCommand(1, factory.Id, "light_tank"));
            Assert(result.Success, "Expected war factory to unlock light tank production: " + result.ErrorCode);
        }

        static void TechCenterUnlocksAdvancedProduction()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var factory = world.CreateActor("war_factory", 1, new Int2(8, 6));

            var locked = world.IssueCommand(new BeginProductionCommand(1, factory.Id, "heavy_tank"));
            Assert(!locked.Success && locked.ErrorCode == "MissingPrerequisite", "Expected heavy tank to require tech center, got " + locked.ErrorCode);

            world.CreateActor("tech_center", 1, new Int2(11, 6));
            Assert(world.GetMissingProductionPrerequisiteTypeId(1, "heavy_tank") == string.Empty, "Expected tech center to unlock heavy tank.");
            var unlocked = world.IssueCommand(new BeginProductionCommand(1, factory.Id, "heavy_tank"));
            Assert(unlocked.Success, "Expected heavy tank production with tech center: " + unlocked.ErrorCode);
        }

        static void RevealScanChangesVisibility()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var target = new Int2(25, 25);
            world.CreateSnapshot(1);
            Assert(!world.IsCellVisible(1, target), "Expected target cell to begin outside visible fog.");

            var result = world.IssueCommand(new ActivateSupportPowerCommand(1, "reveal_scan", target));
            Assert(result.Success, "Expected reveal scan success: " + result.ErrorCode);
            Assert(world.IsCellVisible(1, target), "Expected reveal scan to make target visible.");

            var snapshot = world.CreateSnapshot(1);
            Assert(HasCellVisibility(snapshot.Fog, target, CellVisibility.Visible), "Expected reveal scan visibility in fog snapshot.");
        }

        static void SupportPowerCooldownWorks()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var first = world.IssueCommand(new ActivateSupportPowerCommand(1, "reveal_scan", new Int2(25, 25)));
            Assert(first.Success, "Expected first reveal scan to succeed: " + first.ErrorCode);

            var second = world.IssueCommand(new ActivateSupportPowerCommand(1, "reveal_scan", new Int2(20, 20)));
            Assert(!second.Success && second.ErrorCode == "SupportPowerCooldown", "Expected support power cooldown, got " + second.ErrorCode);

            world.Tick();
            var snapshot = world.CreateSnapshot(1);
            var scan = FindSupportPower(snapshot.Players[0], "reveal_scan");
            Assert(scan != null && scan.CooldownRemainingTicks == 179, "Expected reveal scan cooldown to tick down to 179.");
        }

        static void SupportPowerSnapshotsExposeAvailability()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var lockedSnapshot = world.CreateSnapshot(1);
            var locked = FindSupportPower(lockedSnapshot.Players[0], "reveal_scan");
            Assert(locked != null, "Expected reveal scan support snapshot.");
            Assert(!locked.IsUnlocked && locked.MissingPrerequisiteTypeId == "comm_center", "Expected reveal scan to require comm center.");

            world.CreateActor("comm_center", 1, new Int2(8, 4));
            var unlockedSnapshot = world.CreateSnapshot(1);
            var unlocked = FindSupportPower(unlockedSnapshot.Players[0], "reveal_scan");
            Assert(unlocked != null && unlocked.IsUnlocked && unlocked.IsReady, "Expected reveal scan to become ready after comm center.");
        }

        static void SupportPowerDeterminismSmokeTest()
        {
            var a = RunSupportPowerDeterministicSequence();
            var b = RunSupportPowerDeterministicSequence();
            Assert(a == b, "Expected support-power deterministic summaries to match.");
        }

        static void EngineerCapturesEnemyBuilding()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var engineer = world.CreateActor("engineer", 1, new Int2(17, 10));
            var target = world.FirstActorOfType("power_plant", 2);

            var result = world.IssueCommand(new CaptureBuildingCommand(1, engineer.Id, target.Id));
            Assert(result.Success, "Expected engineer capture success: " + result.ErrorCode);
            Assert(target.OwnerPlayerId == 1, "Expected captured building owner to change.");
            Assert(!world.Actors.ContainsKey(engineer.Id.Value), "Expected engineer to be consumed on capture.");
            Assert(HasEvent(world.CreateSnapshot(), "BuildingCaptured"), "Expected capture combat event.");
        }

        static void EngineerCannotCaptureFriendlyBuilding()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var engineer = world.CreateActor("engineer", 1, new Int2(4, 3));
            var friendlyHub = world.FirstActorOfType("fabrication_hub", 1);

            var result = world.IssueCommand(new CaptureBuildingCommand(1, engineer.Id, friendlyHub.Id));
            Assert(!result.Success && result.ErrorCode == "CaptureTargetFriendly", "Expected friendly capture rejection, got " + result.ErrorCode);
        }

        static void EngineerRepairsOwnedBuilding()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var engineer = world.CreateActor("engineer", 1, new Int2(9, 10));
            var tower = world.FirstActorOfType("gun_tower", 1);
            tower.Health -= 300;

            var result = world.IssueCommand(new EngineerRepairBuildingCommand(1, engineer.Id, tower.Id));
            Assert(result.Success, "Expected engineer repair success: " + result.ErrorCode);
            Assert(tower.Health == 700, "Expected engineer repair to restore building to max health, got " + tower.Health);
            Assert(engineer.RepairProgressTicks == 1, "Expected engineer repair progress accounting.");
        }

        static void TransportLoadsInfantry()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var apc = world.CreateActor("apc", 1, new Int2(7, 8));
            var infantry = world.FirstActorOfType("rifle_infantry", 1);

            var result = world.IssueCommand(new LoadTransportCommand(1, apc.Id, new[] { infantry.Id }));
            Assert(result.Success, "Expected transport load success: " + result.ErrorCode);
            Assert(infantry.LoadedIntoTransportActorId == apc.Id.Value, "Expected infantry loaded into APC.");
            Assert(apc.TransportPassengerActorIds.Count == 1, "Expected APC passenger count 1.");

            var snapshot = world.CreateSnapshot();
            Assert(!SnapshotContainsActor(snapshot, infantry.Id.Value), "Expected loaded infantry hidden from actor snapshot.");
            var transport = FindTransport(snapshot, apc.Id.Value);
            Assert(transport != null && transport.PassengerActorIds.Count == 1, "Expected transport snapshot passenger.");
        }

        static void TransportUnloadRestoresPassengerToBoard()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var apc = world.CreateActor("apc", 1, new Int2(7, 8));
            var infantry = world.FirstActorOfType("rifle_infantry", 1);
            world.IssueCommand(new LoadTransportCommand(1, apc.Id, new[] { infantry.Id }));

            var result = world.IssueCommand(new UnloadTransportCommand(1, apc.Id, new Int2(9, 9)));
            Assert(result.Success, "Expected transport unload success: " + result.ErrorCode);
            Assert(infantry.LoadedIntoTransportActorId == 0, "Expected infantry unloaded.");
            Assert(apc.TransportPassengerActorIds.Count == 0, "Expected APC passenger list cleared.");
            Assert(SnapshotContainsActor(world.CreateSnapshot(), infantry.Id.Value), "Expected unloaded infantry in actor snapshot.");
        }

        static void TransportCapacityIsEnforced()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var apc = world.CreateActor("apc", 1, new Int2(7, 8));
            var passengers = new List<ActorId>();
            for (var i = 0; i < 6; i++)
                passengers.Add(world.CreateActor("rifle_infantry", 1, new Int2(6 + i, 9)).Id);

            var result = world.IssueCommand(new LoadTransportCommand(1, apc.Id, passengers));
            Assert(!result.Success && result.ErrorCode == "TransportFull", "Expected transport capacity rejection, got " + result.ErrorCode);
        }

        static void TransportDeathDestroysPassengers()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var apc = world.CreateActor("apc", 1, new Int2(7, 8));
            var infantry = world.FirstActorOfType("rifle_infantry", 1);
            var load = world.IssueCommand(new LoadTransportCommand(1, apc.Id, new[] { infantry.Id }));
            Assert(load.Success, "Expected setup load success: " + load.ErrorCode);

            var damage = world.ApplyScenarioDamage(2, apc.Id, 9999, "transport_death_test");
            Assert(damage.Success, "Expected scenario damage success: " + damage.ErrorCode);
            Assert(apc.IsDestroyed, "Expected APC destroyed.");
            Assert(infantry.IsDestroyed, "Expected passenger destroyed with transport.");
            Assert(HasEvent(world.CreateSnapshot(), "PassengerDestroyedWithTransport"), "Expected passenger destruction event.");
        }

        static void EngineerTransportDeterminismSmokeTest()
        {
            var a = RunEngineerTransportDeterministicSequence();
            var b = RunEngineerTransportDeterministicSequence();
            Assert(a == b, "Expected engineer/transport deterministic summaries to match.");
        }

        static void WeaponCooldownPreventsContinuousFire()
        {
            var world = CombatWorldWithTankAttack();
            world.Tick();
            Assert(world.Projectiles.Count == 1, "Expected first projectile.");
            var attacker = world.FirstActorOfType("light_tank", 1);
            Assert(attacker.WeaponCooldownRemaining > 0, "Expected weapon cooldown.");
            world.Tick();
            Assert(world.Projectiles.Count == 1, "Expected no second projectile while cooldown is active.");
        }

        static void ProjectileSpawnsForProjectileWeapon()
        {
            var world = CombatWorldWithTankAttack();
            world.Tick();
            Assert(world.Projectiles.Count == 1, "Expected one projectile to spawn.");
        }

        static void ProjectileImpactsAndDealsDamage()
        {
            var world = CombatWorldWithTankAttack();
            var target = world.FirstActorOfType("medium_tank", 2);
            var before = target.Health;
            RunTicks(world, 20);
            Assert(target.Health < before, "Expected projectile impact to damage target.");
            Assert(HasEvent(world.CreateSnapshot(), "DamageApplied"), "Expected damage event after projectile impact.");
        }

        static void DamageReducesHealth()
        {
            var world = CombatWorldWithTankAttack();
            var target = world.FirstActorOfType("medium_tank", 2);
            RunTicks(world, 20);
            Assert(target.Health == 510, "Expected medium tank health 510 after one light tank shell, got " + target.Health);
        }

        static void ActorDiesWhenHealthReachesZero()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var heavy = world.CreateActor("heavy_tank", 1, new Int2(9, 8));
            var target = world.FirstActorOfType("rifle_infantry", 2);
            var result = world.IssueCommand(new IssueAttackOrderCommand(1, new[] { heavy.Id }, target.Id));
            Assert(result.Success, "Expected heavy tank attack success: " + result.ErrorCode);
            RunTicks(world, 20);
            Assert(target.IsDestroyed, "Expected target destroyed.");
            Assert(target.DeathTick >= 0, "Expected death tick.");
            Assert(target.DestroyedByActorId == heavy.Id.Value, "Expected killer actor id.");
        }

        static void DestroyedActorStopsAcceptingMoveOrAttackOrders()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var heavy = world.CreateActor("heavy_tank", 1, new Int2(9, 8));
            var target = world.FirstActorOfType("rifle_infantry", 2);
            world.IssueCommand(new IssueAttackOrderCommand(1, new[] { heavy.Id }, target.Id));
            RunTicks(world, 20);
            Assert(target.IsDestroyed, "Expected target destroyed before command rejection checks.");

            var move = world.IssueCommand(new IssueMoveOrderCommand(2, new[] { target.Id }, new Int2(18, 18)));
            Assert(!move.Success, "Expected destroyed actor move command to fail.");
            var attack = world.IssueCommand(new IssueAttackOrderCommand(2, new[] { target.Id }, heavy.Id));
            Assert(!attack.Success, "Expected destroyed actor attack command to fail.");
        }

        static void GunTowerCanAttackEnemyInRange()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var tower = world.FirstActorOfType("gun_tower", 1);
            var target = world.FirstActorOfType("medium_tank", 2);
            var before = target.Health;

            var result = world.IssueCommand(new IssueAttackOrderCommand(1, new[] { tower.Id }, target.Id));
            Assert(result.Success, "Expected gun tower attack success: " + result.ErrorCode);
            RunTicks(world, 20);
            Assert(target.Health < before, "Expected gun tower to damage enemy in range.");
        }

        static void CombatDeterminismSmokeTest()
        {
            var a = RunCombatDeterministicSequence();
            var b = RunCombatDeterministicSequence();
            Assert(a == b, "Expected combat deterministic summaries to match.");
        }

        static void ProjectileSnapshotAppearsInWorldSnapshot()
        {
            var world = CombatWorldWithTankAttack();
            world.Tick();
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Projectiles.Count == 1, "Expected projectile snapshot.");
            Assert(snapshot.Projectiles[0].WeaponId == "light_tank_shell", "Expected light tank shell snapshot.");
        }

        static void DeathOrDamageEventAppearsInSnapshot()
        {
            var world = CombatWorldWithTankAttack();
            RunTicks(world, 20);
            var snapshot = world.CreateSnapshot();
            Assert(HasEvent(snapshot, "DamageApplied"), "Expected damage event in snapshot.");
        }

        static void ResourceCellsExistInEconomyDemo()
        {
            var world = DemoWorldFactory.CreateEconomyDemoWorld();
            Assert(world.ResourceCells.Count == 9, "Expected 9 economy demo resource cells.");
            Assert(world.FirstActorOfType("harvester", 1) != null, "Expected economy demo harvester.");
            Assert(world.FirstActorOfType("refinery", 1) != null, "Expected economy demo refinery.");
        }

        static void HarvestOrderStartsHarvesterState()
        {
            var world = EconomyWorldWithHarvestOrder();
            var harvester = world.FirstActorOfType("harvester", 1);
            Assert(harvester.CurrentOrder == ActorOrderKind.Harvest, "Expected harvest order kind.");
            Assert(world.Harvesters[harvester.Id.Value].State == HarvesterWorkState.MovingToResource, "Expected moving to resource.");
        }

        static void HarvesterCargoIncreases()
        {
            var world = EconomyWorldWithHarvestOrder();
            var harvester = world.FirstActorOfType("harvester", 1);
            RunTicks(world, 80);
            Assert(world.Harvesters[harvester.Id.Value].CargoAmount > 0, "Expected harvester cargo to increase.");
        }

        static void ResourceAmountDecreases()
        {
            var world = EconomyWorldWithHarvestOrder();
            var resourceCell = new Int2(15, 8);
            var before = world.ResourceCells[resourceCell].Amount;
            RunTicks(world, 80);
            Assert(world.ResourceCells[resourceCell].Amount < before, "Expected resource amount to decrease.");
        }

        static void RefineryUnloadingAddsCredits()
        {
            var world = EconomyWorldWithHarvestOrder();
            var startCredits = world.Players[1].Credits;
            RunTicks(world, 260);
            Assert(world.Players[1].Credits > startCredits, "Expected refinery unload to add credits.");
            Assert(HasEconomyEvent(world.CreateSnapshot(), "HarvesterUnloaded"), "Expected unload economy event.");
        }

        static void HarvesterCargoDecreasesAfterUnload()
        {
            var world = EconomyWorldWithHarvestOrder();
            var harvester = world.FirstActorOfType("harvester", 1);
            RunTicks(world, 120);
            Assert(world.Harvesters[harvester.Id.Value].CargoAmount > 0, "Expected cargo before unload.");
            RunTicks(world, 160);
            Assert(world.Harvesters[harvester.Id.Value].CargoAmount < world.Harvesters[harvester.Id.Value].CargoCapacity, "Expected cargo below capacity after unload.");
        }

        static void DepletedResourceStopsHarvesting()
        {
            var world = DemoWorldFactory.CreateEconomyDemoWorld();
            var harvester = world.FirstActorOfType("harvester", 1);
            var resourceCell = new Int2(15, 8);
            world.ResourceCells[resourceCell].Amount = 20;
            var result = world.IssueCommand(new IssueHarvestOrderCommand(1, new[] { harvester.Id }, resourceCell));
            Assert(result.Success, "Expected harvest order success: " + result.ErrorCode);
            RunTicks(world, 320);
            Assert(world.ResourceCells[resourceCell].IsDepleted, "Expected resource depleted.");
            Assert(world.Harvesters[harvester.Id.Value].State == HarvesterWorkState.Idle, "Expected harvester to stop after depleted cell and unload.");
        }

        static void StopClearsHarvestOrder()
        {
            var world = EconomyWorldWithHarvestOrder();
            var harvester = world.FirstActorOfType("harvester", 1);
            var result = world.IssueCommand(new StopCommand(1, new[] { harvester.Id }));
            Assert(result.Success, "Expected stop success.");
            Assert(!harvester.HasHarvestOrder, "Expected harvest flag cleared.");
            Assert(world.Harvesters[harvester.Id.Value].State == HarvesterWorkState.Idle, "Expected idle harvester state.");
        }

        static void EconomySnapshotContainsResourceHarvesterRefinery()
        {
            var world = EconomyWorldWithHarvestOrder();
            RunTicks(world, 80);
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Economy.Resources.Count > 0, "Expected resource snapshots.");
            Assert(snapshot.Economy.Harvesters.Count == 1, "Expected one harvester snapshot.");
            Assert(snapshot.Economy.Refineries.Count == 1, "Expected one refinery snapshot.");
            Assert(snapshot.Economy.Events.Count > 0, "Expected economy events.");
        }

        static void EconomyDeterminismSmokeTest()
        {
            var a = RunEconomyDeterministicSequence();
            var b = RunEconomyDeterministicSequence();
            Assert(a == b, "Expected economy deterministic summaries to match.");
        }

        static void VisibilityInitializes()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var snapshot = world.CreateSnapshot(1);
            Assert(snapshot.Fog.Width == 32 && snapshot.Fog.Height == 32, "Expected fog dimensions.");
            Assert(snapshot.Fog.Cells.Count == 1024, "Expected one fog cell per map cell.");
            Assert(world.VisibilityStates.ContainsKey(1), "Expected player 1 visibility state.");
        }

        static void ActorRevealsCellsWithinSightRadius()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            world.CreateSnapshot(1);
            Assert(world.IsCellVisible(1, new Int2(16, 8)), "Expected scout rover to reveal cell at sight edge.");
        }

        static void ExploredCellsRemainAfterActorMovesAway()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var scout = world.FirstActorOfType("scout_rover", 1);
            var watchedCell = new Int2(16, 8);
            world.CreateSnapshot(1);
            Assert(world.IsCellVisible(1, watchedCell), "Expected watched cell visible before movement.");

            var move = world.IssueCommand(new IssueMoveOrderCommand(1, new[] { scout.Id }, new Int2(20, 20)));
            Assert(move.Success, "Expected scout movement success: " + move.ErrorCode);
            RunTicks(world, 220);
            var snapshot = world.CreateSnapshot(1);

            Assert(!world.IsCellVisible(1, watchedCell), "Expected watched cell no longer visible.");
            Assert(world.IsCellExploredOrVisible(1, watchedCell), "Expected watched cell to remain explored.");
            Assert(HasCellVisibility(snapshot.Fog, watchedCell, CellVisibility.Explored), "Expected fog snapshot to preserve explored cell.");
        }

        static void EnemyHiddenWhenNotVisible()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var hiddenEnemy = world.FirstActorOfType("medium_tank", 2);
            var snapshot = world.CreateSnapshot(1);
            Assert(!SnapshotContainsActor(snapshot, hiddenEnemy.Id.Value), "Expected distant enemy hidden from player perspective.");
        }

        static void EnemyVisibleWhenInSight()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var visibleEnemy = world.FirstActorOfType("rifle_infantry", 2);
            var snapshot = world.CreateSnapshot(1);
            Assert(SnapshotContainsActor(snapshot, visibleEnemy.Id.Value), "Expected enemy in scout sight to be visible.");
        }

        static void FogSnapshotContainsVisibilityData()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var snapshot = world.CreateSnapshot(1);
            Assert(snapshot.Fog.PlayerId == 1, "Expected player 1 fog snapshot.");
            Assert(HasCellVisibility(snapshot.Fog, new Int2(8, 8), CellVisibility.Visible), "Expected scout cell visible in fog snapshot.");
        }

        static void RadarSnapshotExists()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var snapshot = world.CreateSnapshot(1);
            Assert(snapshot.Radar.PlayerId == 1, "Expected player 1 radar snapshot.");
            Assert(snapshot.Radar.IsActive, "Expected powered comm center radar active.");
            Assert(snapshot.Radar.ProviderActorId == world.FirstActorOfType("comm_center", 1).Id.Value, "Expected comm center radar provider.");
            Assert(snapshot.Minimap.ActorDots.Count > 0, "Expected minimap actor dots.");
        }

        static void VisibilityDeterminismSmokeTest()
        {
            var a = RunVisibilityDeterministicSequence();
            var b = RunVisibilityDeterministicSequence();
            Assert(a == b, "Expected visibility deterministic summaries to match.");
        }

        static void AiInitializes()
        {
            var world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            Assert(world.AiSystem.Plans.ContainsKey(2), "Expected player 2 AI plan state.");
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Ai.Players.Count == 1, "Expected one AI snapshot.");
            Assert(snapshot.Ai.Players[0].PlayerId == 2, "Expected player 2 AI snapshot.");
        }

        static void AiProducesDeterministicIntents()
        {
            var world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            world.Tick();
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Ai.Players[0].RecentIntents.Count >= 5, "Expected Stage 12 AI intents.");
            Assert(HasAiIntent(snapshot, "Economy"), "Expected economy intent.");
            Assert(HasAiIntent(snapshot, "Production"), "Expected production intent.");
            Assert(HasAiIntent(snapshot, "Attack"), "Expected attack intent.");
            Assert(HasAiIntent(snapshot, "Scouting"), "Expected scouting placeholder intent.");
            Assert(HasAiIntent(snapshot, "Defense"), "Expected defense placeholder intent.");
        }

        static void AiQueuesProductionWhenResourcesAllow()
        {
            var world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            world.Tick();
            Assert(world.Players[2].ProductionQueue.Count > 0, "Expected AI production queue item.");
            Assert(HasAiIntent(world.CreateSnapshot(), "Production", "QueueProduction"), "Expected queue production intent.");
        }

        static void AiDoesNotIssueInvalidCommandsRepeatedly()
        {
            var world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            RunTicks(world, 96);
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Ai.Players[0].ConsecutiveInvalidCommands == 0, "Expected no repeated invalid AI commands.");
        }

        static void AiCanIssueAttackIntentIfEnemyExists()
        {
            var world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            world.Tick();
            var scout = world.FirstActorOfType("scout_rover", 2);
            Assert(scout.CurrentOrder == ActorOrderKind.Attack || scout.CurrentOrder == ActorOrderKind.AttackMove, "Expected AI scout to receive attack pressure order.");
            Assert(HasSuccessfulAiIntent(world.CreateSnapshot(), "Attack", "BasicAttackWave"), "Expected successful attack-wave intent.");
        }

        static void AiDifficultyProfilesScaleAttackTiming()
        {
            var easy = AiDifficultyDefinition.CreateEasy();
            var normal = AiDifficultyDefinition.CreateNormal();
            var hard = AiDifficultyDefinition.CreateHard();
            Assert(easy.DecisionIntervalTicks > normal.DecisionIntervalTicks, "Expected Easy AI to think less often than Normal.");
            Assert(hard.DecisionIntervalTicks < normal.DecisionIntervalTicks, "Expected Hard AI to think more often than Normal.");
            Assert(easy.InitialAttackDelayTicks > normal.InitialAttackDelayTicks, "Expected Easy first attack wave to arrive later than Normal.");
            Assert(hard.InitialAttackDelayTicks < normal.InitialAttackDelayTicks, "Expected Hard first attack wave to arrive earlier than Normal.");
            Assert(hard.DesiredVehicleCount > normal.DesiredVehicleCount, "Expected Hard AI to target more vehicles.");
            Assert(hard.EnableBuildingRepair, "Expected Hard AI to enable deterministic building repair.");
        }

        static void AiAttackWaveUsesTimedPressure()
        {
            var difficulty = AiDifficultyDefinition.CreateNormal();
            var world = DemoWorldFactory.CreateVerticalSliceWorld(difficulty);
            world.StartMatch();
            RunTicks(world, difficulty.InitialAttackDelayTicks + difficulty.DecisionIntervalTicks + 20);

            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Ai.Players.Count == 1 && snapshot.Ai.Players[0].DifficultyId == "normal", "Expected vertical slice AI to use Normal difficulty.");
            Assert(snapshot.Ai.Players[0].AttackWaveSequence >= 1, "Expected timed AI attack wave sequence to advance.");
            Assert(snapshot.Ai.Players[0].NextAttackWaveTick > snapshot.Tick, "Expected next attack wave tick to move forward after pressure is issued.");
            Assert(HasSuccessfulAiIntent(snapshot, "Attack", "BasicAttackWave"), "Expected successful timed attack-wave intent.");
            Assert(AnyOwnedCombatActorHasOrder(world, 2, ActorOrderKind.AttackMove) || AnyOwnedCombatActorHasOrder(world, 2, ActorOrderKind.Attack), "Expected enemy combat units to receive attack pressure orders.");
        }

        static void AiHardDifficultyAttacksEarlierThanEasy()
        {
            var easyTick = FirstSuccessfulAttackWaveTick(AiDifficultyDefinition.CreateEasy(), 540);
            var hardTick = FirstSuccessfulAttackWaveTick(AiDifficultyDefinition.CreateHard(), 160);
            Assert(easyTick > 0, "Expected Easy AI to eventually issue an attack wave.");
            Assert(hardTick > 0, "Expected Hard AI to issue an attack wave.");
            Assert(hardTick < easyTick, "Expected Hard AI pressure to start earlier than Easy.");
        }

        static void AiHardDifficultyRepairsDamagedBuilding()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld(AiDifficultyDefinition.CreateHard());
            world.StartMatch();
            var enemyHub = world.FirstActorOfType("fabrication_hub", 2);
            enemyHub.Health -= 120;
            world.Tick();
            Assert(enemyHub.IsRepairing, "Expected Hard AI to start repairing a damaged owned building.");
            Assert(HasSuccessfulAiIntent(world.CreateSnapshot(), "Defense", "DefenseRepairDamagedBuilding"), "Expected successful AI repair intent.");
        }

        static void AiDeterminismSmokeTest()
        {
            var a = RunAiDeterministicSequence();
            var b = RunAiDeterministicSequence();
            Assert(a == b, "Expected AI deterministic summaries to match.");
        }

        static void TerrainDefinitionsExist()
        {
            var rules = DemoRules.CreateDefaultRules();
            Assert(rules.TerrainDefinitions.ContainsKey(TerrainKind.Clear), "Expected clear terrain definition.");
            Assert(rules.TerrainDefinitions.ContainsKey(TerrainKind.Water), "Expected water terrain definition.");
            Assert(!rules.GetTerrainDefinition(TerrainKind.Water).Allows(MovementClass.Wheeled), "Expected wheeled units blocked by water.");
            Assert(rules.GetTerrainDefinition(TerrainKind.Water).Allows(MovementClass.Aircraft), "Expected aircraft to pass water.");
        }

        static void ImpassableTerrainBlocksPath()
        {
            var world = DemoWorldFactory.CreateMapTerrainDemoWorld();
            var scout = world.FirstActorOfType("scout_rover", 1);
            for (var y = 0; y < world.Map.Height; y++)
                world.SetTerrainCell(new Int2(12, y), TerrainKind.Water);

            var result = world.QueryPath(scout.Id, new Int2(18, 10));
            Assert(!result.Success, "Expected water wall to block wheeled path.");
            Assert(result.FailureCode == "Unreachable" || result.FailureCode == "GoalImpassable", "Expected structured failure code, got " + result.FailureCode);
        }

        static void MovementClassesCanDiffer()
        {
            var rules = DemoRules.CreateDefaultRules();
            var infantry = (UnitDefinition)rules.GetDefinition("rifle_infantry");
            var scout = (UnitDefinition)rules.GetDefinition("scout_rover");
            var tank = (UnitDefinition)rules.GetDefinition("medium_tank");
            Assert(infantry.Movement.MovementClass == MovementClass.Infantry, "Expected infantry movement class.");
            Assert(scout.Movement.MovementClass == MovementClass.Wheeled, "Expected wheeled scout movement class.");
            Assert(tank.Movement.MovementClass == MovementClass.Tracked, "Expected tracked tank movement class.");
        }

        static void PathQueryReturnsStructuredResult()
        {
            var world = DemoWorldFactory.CreateMapTerrainDemoWorld();
            var tank = world.FirstActorOfType("medium_tank", 1);
            var result = world.QueryPath(tank.Id, new Int2(18, 12));
            Assert(result.Success, "Expected tracked tank to path through rough terrain: " + result.FailureCode);
            Assert(result.Path.Count > 0, "Expected path steps.");
            Assert(result.TotalCost > 0, "Expected path cost.");
            Assert(result.VisitedCellCount > 0, "Expected visited-cell count.");
            Assert(result.MovementClass == MovementClass.Tracked, "Expected tracked query.");
        }

        static void DiagonalMovementUsesEightWayPathing()
        {
            var rules = DemoRules.CreateDefaultRules();
            var map = new GridMap(8, 8);
            var result = new GridPathfinder().QueryPath(map, rules, new Int2(1, 1), new Int2(4, 4), MovementClass.Wheeled);
            Assert(result.Success, "Expected open diagonal path: " + result.FailureCode);
            Assert(result.Path.Count == 3, "Expected three diagonal steps, got " + result.Path.Count);
            Assert(result.Path[0].Equals(new Int2(2, 2)), "Expected first diagonal step to 2,2, got " + result.Path[0]);
            Assert(result.Path[1].Equals(new Int2(3, 3)), "Expected second diagonal step to 3,3, got " + result.Path[1]);
            Assert(result.Path[2].Equals(new Int2(4, 4)), "Expected final diagonal step to 4,4, got " + result.Path[2]);
            Assert(result.TotalCost == 42, "Expected three diagonal clear-terrain costs, got " + result.TotalCost);
        }

        static void DiagonalMovementDoesNotCutBlockedCorners()
        {
            var rules = DemoRules.CreateDefaultRules();
            var map = new GridMap(5, 5);
            map.SetBlocked(new Int2(2, 1), true);
            map.SetBlocked(new Int2(1, 2), true);

            var result = new GridPathfinder().QueryPath(map, rules, new Int2(1, 1), new Int2(2, 2), MovementClass.Wheeled);
            Assert(result.Success, "Expected route around blocked corner: " + result.FailureCode);
            Assert(result.Path.Count > 1, "Expected blocked corner to prevent direct diagonal step.");
            Assert(!result.Path[0].Equals(new Int2(2, 2)), "Expected first step not to cut through the blocked corner.");
        }

        static void MapValidationCatchesInvalidLayout()
        {
            var world = DemoWorldFactory.CreateMapTerrainDemoWorld();
            var resourceCell = new Int2(16, 8);
            world.SetTerrainCell(resourceCell, TerrainKind.Water);
            var result = world.ValidateMapForPlayer(1);
            Assert(!result.Success, "Expected invalid resource terrain.");
            Assert(ContainsText(result.Errors, "ResourceOnImpassableTerrain"), "Expected resource validation error.");
        }

        static void MapSnapshotContainsTerrainAndPathDebug()
        {
            var world = DemoWorldFactory.CreateMapTerrainDemoWorld();
            var scout = world.FirstActorOfType("scout_rover", 1);
            world.QueryPath(scout.Id, new Int2(18, 6));
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Map.Width == 32 && snapshot.Map.Height == 32, "Expected map snapshot dimensions.");
            Assert(snapshot.Map.TerrainCells.Count == 1024, "Expected one terrain snapshot per cell.");
            Assert(snapshot.Map.RecentPathQueries.Count == 1, "Expected path debug snapshot.");
            Assert(HasTerrainKind(snapshot.Map, "Road"), "Expected road terrain in snapshot.");
        }

        static void GeneratedMapIsDeterministic()
        {
            var settings = MapGenerationSettings.CreateDefaultSkirmish(44031);
            var generator = new ProjectAegisRTS.MapGeneration.AegisMapGenerator();
            var a = GeneratedMapSignature(generator.Generate(settings));
            var b = GeneratedMapSignature(generator.Generate(settings));
            Assert(a == b, "Expected generated map signatures to match for the same seed.");
        }

        static void GeneratedMapChangesWithSeed()
        {
            var generator = new ProjectAegisRTS.MapGeneration.AegisMapGenerator();
            var a = GeneratedMapSignature(generator.Generate(MapGenerationSettings.CreateDefaultSkirmish(1001)));
            var b = GeneratedMapSignature(generator.Generate(MapGenerationSettings.CreateDefaultSkirmish(1002)));
            Assert(a != b, "Expected generated map signatures to differ for different seeds.");
        }

        static void GeneratedMapIsLeftRightSymmetric()
        {
            var generated = new ProjectAegisRTS.MapGeneration.AegisMapGenerator().Generate(MapGenerationSettings.CreateDefaultSkirmish(7219));
            for (var y = 0; y < generated.Height; y++)
                for (var x = 0; x < generated.Width; x++)
                {
                    var left = generated.TerrainAt(new Int2(x, y));
                    var right = generated.TerrainAt(new Int2(generated.Width - 1 - x, y));
                    Assert(left == right, "Expected mirrored terrain at " + x + "," + y + ".");
                }

            Assert(generated.Spawns.Count == 2, "Expected two generated spawn points.");
            Assert(generated.Spawns[0].Cell.X == generated.Width - 1 - generated.Spawns[1].Cell.X, "Expected mirrored spawn X positions.");
            Assert(generated.Spawns[0].Cell.Y == generated.Spawns[1].Cell.Y, "Expected mirrored spawn Y positions.");
        }

        static void GeneratedSkirmishWorldCreatesValidMap()
        {
            var world = DemoWorldFactory.CreateGeneratedSkirmishWorld(99881, AiDifficultyDefinition.CreateNormal());
            var snapshot = world.CreateSnapshot(1);
            Assert(snapshot.Map.Width == 40 && snapshot.Map.Height == 40, "Expected default generated skirmish dimensions.");
            Assert(snapshot.Map.IsValid, "Expected generated skirmish map to validate: " + string.Join(", ", snapshot.Map.ValidationErrors));
            Assert(world.FirstActorOfType("fabrication_hub", 1) != null, "Expected player generated base.");
            Assert(world.FirstActorOfType("fabrication_hub", 2) != null, "Expected enemy generated base.");
            Assert(snapshot.Economy.Resources.Count > 0, "Expected generated resources.");
            Assert(snapshot.Ai.Players.Count == 1, "Expected generated AI player.");
        }

        static void GeneratedSkirmishWorldHasReachableResourcesAndEnemy()
        {
            var world = DemoWorldFactory.CreateGeneratedSkirmishWorld(31827, AiDifficultyDefinition.CreateNormal());
            var harvester = world.FirstActorOfType("harvester", 1);
            var scout = world.FirstActorOfType("scout_rover", 1);
            var enemyScout = world.FirstActorOfType("scout_rover", 2);

            Assert(harvester != null && scout != null && enemyScout != null, "Expected generated starter units.");
            Assert(AnyReachableResource(world, harvester), "Expected at least one reachable generated resource.");

            var enemyPath = world.QueryPath(scout.Id, enemyScout.CellPosition);
            Assert(enemyPath.Success, "Expected generated road/playable region to connect player and enemy scouts: " + enemyPath.FailureCode);
        }

        static void AegisMapAccepts100x100Maps()
        {
            var document = ValidAegisMap(100, 100);
            var result = new AegisMapDocumentValidator().Validate(document, DemoRules.CreateDefaultRules());
            Assert(result.Success, "Expected 100x100 Aegis map to validate: " + string.Join(", ", result.Errors));
        }

        static void AegisMapAccepts400x400Maps()
        {
            var document = ValidAegisMap(400, 400);
            var result = new AegisMapDocumentValidator().Validate(document, DemoRules.CreateDefaultRules());
            Assert(result.Success, "Expected 400x400 Aegis map to validate: " + string.Join(", ", result.Errors));
        }

        static void AegisMapRejects99x100Maps()
        {
            var document = ValidAegisMap(99, 100);
            var result = new AegisMapDocumentValidator().Validate(document);
            Assert(!result.Success && ContainsText(result.Errors, "MapWidthTooSmall"), "Expected 99x100 map rejection.");
        }

        static void AegisMapRejects401x400Maps()
        {
            var document = ValidAegisMap(401, 400);
            var result = new AegisMapDocumentValidator().Validate(document);
            Assert(!result.Success && ContainsText(result.Errors, "MapWidthTooLarge"), "Expected 401x400 map rejection.");
        }

        static void AegisMapCreatesRuntimeWorldFrom400x400()
        {
            var document = ValidAegisMap(400, 400);
            document.ActorPlacements.Add(new AegisActorPlacement("fabrication_hub", 1, 6, 6, 0, "player_hub"));
            document.ActorPlacements.Add(new AegisActorPlacement("scout_rover", 1, 12, 12, 90, "player_scout"));
            var world = new AegisMapDocumentWorldFactory().CreateWorld(document, DemoRules.CreateDefaultRules());
            Assert(world.Map.Width == 400 && world.Map.Height == 400, "Expected 400x400 runtime world.");
            Assert(world.FirstActorOfType("fabrication_hub", 1) != null, "Expected imported fabrication hub.");
            Assert(world.FirstActorOfType("scout_rover", 1) != null, "Expected imported scout rover.");
        }

        static void TiledImporterImportsSmallSample()
        {
            var path = Path.Combine(RepoRoot(), "unity", "Assets", "Rts", "Maps", "Generated", "sample_small_100.tiled.json");
            Assert(File.Exists(path), "Expected checked-in Tiled sample at " + path);
            var result = new AegisTiledMapImporter().ImportFile(path, new AegisTiledImportOptions { DefaultMapId = "sample_small_100" });
            Assert(result.Success, "Expected Tiled sample import success: " + string.Join(", ", result.Errors));
            Assert(result.Document.Width == 100 && result.Document.Height == 100, "Expected 100x100 imported sample.");
            Assert(result.Document.PlayerStarts.Count >= 2, "Expected sample player starts.");
            Assert(result.Document.ActorPlacements.Count >= 2, "Expected sample actor placements.");
            Assert(result.Document.Resources.Count > 0, "Expected sample resources.");
        }

        static void TiledImporterRejectsInfiniteMaps()
        {
            var result = new AegisTiledMapImporter().ImportJson(TiledMapJson("orthogonal", true, string.Empty), new AegisTiledImportOptions());
            Assert(!result.Success && ContainsText(result.Errors, "TiledInfiniteMapUnsupported"), "Expected infinite Tiled map rejection.");
        }

        static void TiledImporterRejectsUnsupportedOrientation()
        {
            var result = new AegisTiledMapImporter().ImportJson(TiledMapJson("isometric", false, string.Empty), new AegisTiledImportOptions());
            Assert(!result.Success && ContainsText(result.Errors, "TiledOrientationUnsupported"), "Expected unsupported orientation rejection.");
        }

        static void TiledImporterImportsBlockers()
        {
            var layer = "\"layers\":[{\"id\":1,\"name\":\"blockers\",\"type\":\"tilelayer\",\"width\":100,\"height\":100,\"encoding\":\"csv\",\"data\":\"" + CsvLayerData(100, 100, 505, 1) + "\"}]";
            var result = new AegisTiledMapImporter().ImportJson(TiledMapJson("orthogonal", false, layer), new AegisTiledImportOptions());
            Assert(result.Success, "Expected blocker import success: " + string.Join(", ", result.Errors));
            Assert(result.Document.Blockers.Count == 1, "Expected one blocker.");
            Assert(result.Document.Blockers[0].X == 5 && result.Document.Blockers[0].Y == 5, "Expected blocker at 5,5.");
        }

        static void TiledImporterImportsPlayerStarts()
        {
            var layer = "\"layers\":[{\"id\":1,\"name\":\"player_starts\",\"type\":\"objectgroup\",\"objects\":[{\"id\":1,\"name\":\"player_2\",\"type\":\"player_start\",\"x\":320,\"y\":352,\"point\":true,\"properties\":[{\"name\":\"playerId\",\"type\":\"int\",\"value\":2}]}]}]";
            var result = new AegisTiledMapImporter().ImportJson(TiledMapJson("orthogonal", false, layer), new AegisTiledImportOptions());
            Assert(result.Success, "Expected player start import success: " + string.Join(", ", result.Errors));
            Assert(result.Document.PlayerStarts.Count == 1, "Expected one player start.");
            Assert(result.Document.PlayerStarts[0].PlayerId == 2, "Expected player 2 start.");
            Assert(result.Document.PlayerStarts[0].X == 10 && result.Document.PlayerStarts[0].Y == 11, "Expected player start cell 10,11.");
        }

        static void TiledImporterImportsActorPlacements()
        {
            var layer = "\"layers\":[{\"id\":1,\"name\":\"actor_placements\",\"type\":\"objectgroup\",\"objects\":[{\"id\":1,\"name\":\"scout_alpha\",\"type\":\"scout_rover\",\"x\":384,\"y\":416,\"point\":true,\"properties\":[{\"name\":\"ownerPlayerId\",\"type\":\"int\",\"value\":1},{\"name\":\"facingDegrees\",\"type\":\"int\",\"value\":90}]}]}]";
            var result = new AegisTiledMapImporter().ImportJson(TiledMapJson("orthogonal", false, layer), new AegisTiledImportOptions());
            Assert(result.Success, "Expected actor placement import success: " + string.Join(", ", result.Errors));
            Assert(result.Document.ActorPlacements.Count == 1, "Expected one actor placement.");
            Assert(result.Document.ActorPlacements[0].TypeId == "scout_rover", "Expected scout_rover type.");
            Assert(result.Document.ActorPlacements[0].OwnerPlayerId == 1, "Expected owner player 1.");
            Assert(result.Document.ActorPlacements[0].X == 12 && result.Document.ActorPlacements[0].Y == 13, "Expected actor cell 12,13.");
        }

        static void TiledExporterRoundTripsAegisMap()
        {
            var document = ValidAegisMap(100, 100);
            document.ActorPlacements.Add(new AegisActorPlacement("scout_rover", 1, 16, 16, 90, "round_trip_scout"));
            var json = new AegisTiledMapExporter().ExportToJson(document);
            var result = new AegisTiledMapImporter().ImportJson(json, new AegisTiledImportOptions());
            Assert(result.Success, "Expected exported Tiled JSON to import: " + string.Join(", ", result.Errors));
            Assert(result.Document.Width == 100 && result.Document.Height == 100, "Expected round-trip dimensions.");
            Assert(result.Document.PlayerStarts.Count == 2, "Expected round-trip player starts.");
            Assert(result.Document.ActorPlacements.Count == 1, "Expected round-trip actor placement.");
            Assert(result.Document.Blockers.Count == 1, "Expected round-trip blocker.");
        }

        static void AegisMapPromptParserParsesSizePresets()
        {
            var parser = new AegisMapNaturalLanguageRequestParser();
            Assert(parser.Parse("small rocky map").Request.SizePreset == AegisMapGenerationPreset.Small, "Expected small preset.");
            Assert(parser.Parse("medium forest map").Request.SizePreset == AegisMapGenerationPreset.Medium, "Expected medium preset.");
            Assert(parser.Parse("large desert map").Request.SizePreset == AegisMapGenerationPreset.Large, "Expected large preset.");
            var custom = parser.Parse("200 by 200 forest map");
            Assert(custom.Request.ResolveWidth() == 200 && custom.Request.ResolveHeight() == 200, "Expected custom 200x200 parse.");
        }

        static void AegisMapPromptParserParsesResourceDensity()
        {
            var request = new AegisMapNaturalLanguageRequestParser().Parse("large map with lots of ore and high resources").Request;
            Assert(request.ResourceDensity == AegisMapIntensity.High || request.ResourceDensity == AegisMapIntensity.VeryHigh, "Expected high resource density.");
        }

        static void AegisMapPromptParserParsesCliffDensity()
        {
            var request = new AegisMapNaturalLanguageRequestParser().Parse("small desert map with high cliffs").Request;
            Assert(request.CliffDensity == AegisMapIntensity.High, "Expected high cliff density.");
        }

        static void AegisMapPromptParserParsesRockiness()
        {
            var request = new AegisMapNaturalLanguageRequestParser().Parse("200 by 200 forest map with high rockiness and regenerating ore").Request;
            Assert(request.Rockiness == AegisMapIntensity.High, "Expected high rockiness.");
            Assert(request.OreRegenerationEnabled, "Expected regenerating ore flag.");
        }

        static void AegisMapPromptParserHandlesUnknownWordsWithWarnings()
        {
            var result = new AegisMapNaturalLanguageRequestParser().Parse("make it mysterious and jazzy");
            Assert(result.Request.PlayerCount == 2, "Expected default player count.");
            Assert(result.Warnings.Count > 0, "Expected unknown prompt warning.");
        }

        static void AegisProceduralGeneratorCreatesSmall100()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Small, 123);
            Assert(result.Success, "Expected small generation success: " + string.Join(", ", result.Errors));
            Assert(result.Document.Width == 100 && result.Document.Height == 100, "Expected 100x100 small generated map.");
        }

        static void AegisProceduralGeneratorCreatesMedium200()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Medium, 123);
            Assert(result.Success, "Expected medium generation success: " + string.Join(", ", result.Errors));
            Assert(result.Document.Width == 200 && result.Document.Height == 200, "Expected 200x200 medium generated map.");
        }

        static void AegisProceduralGeneratorCreatesLarge400()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Large, 123);
            Assert(result.Success, "Expected large generation success: " + string.Join(", ", result.Errors));
            Assert(result.Document.Width == 400 && result.Document.Height == 400, "Expected 400x400 large generated map.");
        }

        static void AegisProceduralGeneratorRejectsBelowMinimumSize()
        {
            var request = AegisMapGenerationRequest.CreateDefault();
            request.SizePreset = AegisMapGenerationPreset.Custom;
            request.CustomWidth = 99;
            request.CustomHeight = 100;
            var result = new ProjectAegisRTS.Maps.Generation.AegisMapGenerator().Generate(request);
            Assert(!result.Success && ContainsText(result.Errors, "GeneratedMapTooSmall"), "Expected below-min generated map rejection.");
        }

        static void AegisProceduralGeneratorRejectsAboveMaximumSize()
        {
            var request = AegisMapGenerationRequest.CreateDefault();
            request.SizePreset = AegisMapGenerationPreset.Custom;
            request.CustomWidth = 401;
            request.CustomHeight = 400;
            var result = new ProjectAegisRTS.Maps.Generation.AegisMapGenerator().Generate(request);
            Assert(!result.Success && ContainsText(result.Errors, "GeneratedMapTooLarge"), "Expected above-max generated map rejection.");
        }

        static void AegisProceduralGeneratorIsDeterministicForSameSeed()
        {
            var a = GenerateAegisMap(AegisMapGenerationPreset.Small, 4321);
            var b = GenerateAegisMap(AegisMapGenerationPreset.Small, 4321);
            Assert(a.Success && b.Success, "Expected generation success.");
            Assert(AegisMapDocumentSignature(a.Document) == AegisMapDocumentSignature(b.Document), "Expected same seed generated map signature.");
        }

        static void AegisProceduralGeneratorChangesWithDifferentSeeds()
        {
            var a = GenerateAegisMap(AegisMapGenerationPreset.Small, 4321);
            var b = GenerateAegisMap(AegisMapGenerationPreset.Small, 4322);
            Assert(a.Success && b.Success, "Expected generation success.");
            Assert(AegisMapDocumentSignature(a.Document) != AegisMapDocumentSignature(b.Document), "Expected different seed generated map signature.");
        }

        static void AegisProceduralGeneratedMapsPassValidator()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Medium, 8811);
            var validation = new AegisMapDocumentValidator().Validate(result.Document, DemoRules.CreateDefaultRules());
            Assert(result.Success, "Expected generated map success: " + string.Join(", ", result.Errors));
            Assert(validation.Success, "Expected generated map validation: " + string.Join(", ", validation.Errors));
        }

        static void AegisProceduralLowResourcesCreatesFewerFieldsThanHigh()
        {
            var low = GenerateAegisMap(AegisMapGenerationPreset.Small, 55, AegisMapIntensity.Low, AegisMapIntensity.Low, AegisMapIntensity.Low);
            var high = GenerateAegisMap(AegisMapGenerationPreset.Small, 55, AegisMapIntensity.VeryHigh, AegisMapIntensity.Low, AegisMapIntensity.Low);
            Assert(low.Document.Resources.Count < high.Document.Resources.Count, "Expected high resource density to create more fields.");
        }

        static void AegisProceduralResourcesAvoidPlayerStartPads()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Small, 91, AegisMapIntensity.High, AegisMapIntensity.Low, AegisMapIntensity.Low);
            for (var i = 0; i < result.Document.Resources.Count; i++)
                for (var s = 0; s < result.Document.PlayerStarts.Count; s++)
                {
                    var resource = result.Document.Resources[i];
                    var start = result.Document.PlayerStarts[s];
                    var distance = Math.Abs(resource.X - start.X) + Math.Abs(resource.Y - start.Y);
                    Assert(distance > 10, "Expected resource outside initial build pad.");
                }
        }

        static void AegisProceduralHighCliffsCreatesMoreBlockersThanLow()
        {
            var low = GenerateAegisMap(AegisMapGenerationPreset.Small, 77, AegisMapIntensity.Medium, AegisMapIntensity.Low, AegisMapIntensity.None);
            var high = GenerateAegisMap(AegisMapGenerationPreset.Small, 77, AegisMapIntensity.Medium, AegisMapIntensity.High, AegisMapIntensity.None);
            Assert(CountBlockersWithReason(high.Document, "generated_cliff") > CountBlockersWithReason(low.Document, "generated_cliff"), "Expected high cliffs to create more cliff blockers.");
        }

        static void AegisProceduralRockyMapsCreateMoreRockBlockers()
        {
            var low = GenerateAegisMap(AegisMapGenerationPreset.Small, 78, AegisMapIntensity.Medium, AegisMapIntensity.None, AegisMapIntensity.None);
            var high = GenerateAegisMap(AegisMapGenerationPreset.Small, 78, AegisMapIntensity.Medium, AegisMapIntensity.None, AegisMapIntensity.Extreme);
            Assert(CountBlockersWithReason(high.Document, "generated_rock") > CountBlockersWithReason(low.Document, "generated_rock"), "Expected rocky map to create more rock blockers.");
        }

        static void AegisProceduralEachStartHasCleanBuildPads()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Medium, 901);
            Assert(result.Success, "Expected generated map success.");
            for (var i = 0; i < result.Document.PlayerStarts.Count; i++)
            {
                var playerId = result.Document.PlayerStarts[i].PlayerId;
                Assert(result.Buildability.CountForPlayer(playerId) >= 6, "Expected clean build pads for player " + playerId + ".");
            }
        }

        static void AegisProceduralPreservesPathabilityBetweenStarts()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Medium, 902, AegisMapIntensity.Medium, AegisMapIntensity.High, AegisMapIntensity.High);
            Assert(result.Success, "Expected generated map success.");
            Assert(result.Balance.StartsConnected, "Expected generated starts to remain connected.");
        }

        static void AegisProceduralBuildabilityAcceptsBasePads()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Small, 903);
            var analyzer = new AegisMapBuildabilityAnalyzer();
            var spot = result.Buildability.BuildSpots[0];
            Assert(analyzer.CanPlace(result.Document, spot.Footprint, spot.TopLeft.X, spot.TopLeft.Y), "Expected generated base build pad to accept placement.");
        }

        static void AegisProceduralBuildabilityRejectsBlockersCliffsResources()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Small, 904, AegisMapIntensity.Medium, AegisMapIntensity.High, AegisMapIntensity.High);
            var analyzer = new AegisMapBuildabilityAnalyzer();
            var footprint = AegisBuildingFootprint.Square(1);
            var blocker = result.Document.Blockers[0];
            var resource = result.Document.Resources[0];
            Assert(!analyzer.CanPlace(result.Document, footprint, blocker.X, blocker.Y), "Expected placement rejection on blocker/cliff.");
            Assert(!analyzer.CanPlace(result.Document, footprint, resource.X, resource.Y), "Expected placement rejection on resource.");
        }

        static void AegisProceduralLargeMapBuildabilityCompletes()
        {
            var result = GenerateAegisMap(AegisMapGenerationPreset.Large, 905);
            Assert(result.Success, "Expected large generated map success.");
            Assert(result.Buildability.BuildSpots.Count >= result.Document.PlayerStarts.Count * 6, "Expected large map buildability pads.");
        }

        static void AegisResourceSimulationHarvestReducesAmount()
        {
            var sim = OreSimulation(20, 100, 5, 2);
            var result = sim.Harvest("ore_1", 7);
            Assert(result.Success && result.HarvestedAmount == 7, "Expected harvest success.");
            Assert(sim.GetField("ore_1").Amount == 13, "Expected amount reduced.");
        }

        static void AegisResourceSimulationHarvestReachesZeroAndDepletes()
        {
            var sim = OreSimulation(10, 100, 5, 2);
            sim.Harvest("ore_1", 20);
            Assert(sim.GetField("ore_1").Amount == 0, "Expected zero amount.");
            Assert(sim.GetField("ore_1").IsDepleted, "Expected depleted field.");
            Assert(!sim.GetField("ore_1").Visible, "Expected depleted field hidden.");
        }

        static void AegisResourceSimulationRejectsDepletedHarvest()
        {
            var sim = OreSimulation(5, 100, 5, 2);
            sim.Harvest("ore_1", 5);
            var result = sim.Harvest("ore_1", 1);
            Assert(!result.Success && result.ErrorCode == "ResourceDepleted", "Expected depleted harvest rejection.");
        }

        static void AegisResourceSimulationRegenerationDelayWorks()
        {
            var sim = OreSimulation(10, 20, 3, 5);
            sim.Harvest("ore_1", 10);
            for (var i = 0; i < 4; i++)
                sim.Tick();
            Assert(sim.GetField("ore_1").Amount == 0, "Expected no regeneration before delay.");
            sim.Tick();
            Assert(sim.GetField("ore_1").Amount == 3, "Expected regeneration at delay boundary.");
        }

        static void AegisResourceSimulationRegeneratesAndCapsAtMax()
        {
            var sim = OreSimulation(5, 12, 4, 1);
            sim.Harvest("ore_1", 5);
            for (var i = 0; i < 10; i++)
                sim.Tick();
            Assert(sim.GetField("ore_1").Amount == 12, "Expected regeneration to cap at max.");
        }

        static void AegisResourceSimulationIsDeterministic()
        {
            var a = RunResourceSimulationSignature();
            var b = RunResourceSimulationSignature();
            Assert(a == b, "Expected repeated resource simulation to be deterministic.");
        }

        static void RtsCoreDoesNotReferenceUnityEngine()
        {
            var coreRoot = Path.Combine(RepoRoot(), "src", "Rts.Core");
            foreach (var file in Directory.GetFiles(coreRoot, "*.cs", SearchOption.AllDirectories))
            {
                var normalized = file.Replace('\\', '/');
                if (normalized.Contains("/obj/") || normalized.Contains("/bin/"))
                    continue;

                var text = File.ReadAllText(file);
                Assert(!text.Contains("UnityEngine"), "Rts.Core must remain UnityEngine-free. Found reference in " + file);
                Assert(!text.Contains("UnityEditor"), "Rts.Core must remain UnityEditor-free. Found reference in " + file);
            }
        }

        static void PathingDeterminismSmokeTest()
        {
            var a = RunPathingDeterministicSequence();
            var b = RunPathingDeterministicSequence();
            Assert(a == b, "Expected pathing deterministic summaries to match.");
        }

        static void AircraftAndAirfieldDefinitionsExist()
        {
            var rules = DemoRules.CreateDefaultRules();
            var aircraft = rules.GetDefinition("attack_aircraft");
            var lifter = rules.GetDefinition("heavy_lifter_aircraft");
            var helipad = rules.GetDefinition("dual_helipad");
            Assert(aircraft.Production.Kind == ProductionKind.Aircraft, "Expected attack aircraft production kind.");
            Assert(aircraft.Aircraft != null && aircraft.Aircraft.RequiresAirfield, "Expected attack aircraft airfield metadata.");
            Assert(lifter.Aircraft != null, "Expected lifter aircraft metadata.");
            Assert(helipad.Airfield != null && helipad.Airfield.PadCount == 2, "Expected dual helipad to expose two pads.");
        }

        static void HelipadCreatesAirfieldSnapshot()
        {
            var world = new RtsWorld(DemoRules.CreateDefaultRules(), new GridMap(16, 16));
            world.AddPlayer(1, "Air Player", 5000);
            var helipad = world.CreateActor("dual_helipad", 1, new Int2(5, 5));
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Airfields.Count == 1, "Expected one airfield snapshot.");
            Assert(snapshot.Airfields[0].ActorId == helipad.Id.Value, "Expected helipad airfield snapshot.");
            Assert(snapshot.Airfields[0].Pads.Count == 2, "Expected dual pad snapshot.");
        }

        static void HelipadSupportsAircraftProductionDockingPlaceholder()
        {
            var world = new RtsWorld(DemoRules.CreateDefaultRules(), new GridMap(16, 16));
            world.AddPlayer(1, "Air Player", 5000);
            world.CreateActor("power_plant", 1, new Int2(9, 1));
            world.CreateActor("tech_center", 1, new Int2(1, 1));
            var helipad = world.CreateActor("dual_helipad", 1, new Int2(5, 5));
            var begin = world.IssueCommand(new BeginProductionCommand(1, helipad.Id, "attack_aircraft"));
            Assert(begin.Success, "Expected aircraft production to start: " + begin.ErrorCode);
            RunTicks(world, 60);

            var snapshot = world.CreateSnapshot();
            var aircraft = FindActor(snapshot, "attack_aircraft", 1);
            Assert(aircraft != null, "Expected produced aircraft actor.");
            Assert(snapshot.Aircraft.Count == 1, "Expected aircraft snapshot.");
            Assert(snapshot.Aircraft[0].DockedAirfieldActorId == helipad.Id.Value, "Expected aircraft docked at producing helipad.");
            Assert(snapshot.Airfields[0].Pads[0].OccupiedAircraftActorId == aircraft.ActorId || snapshot.Airfields[0].Pads[1].OccupiedAircraftActorId == aircraft.ActorId, "Expected a helipad pad occupied by produced aircraft.");
        }

        static void AircraftCanPathOverWater()
        {
            var world = new RtsWorld(DemoRules.CreateDefaultRules(), new GridMap(16, 16));
            world.AddPlayer(1, "Air Player", 5000);
            for (var y = 0; y < 16; y++)
                world.SetTerrainCell(new Int2(7, y), TerrainKind.Water);

            var aircraft = world.CreateActor("attack_aircraft", 1, new Int2(3, 8));
            var result = world.QueryPath(aircraft.Id, new Int2(12, 8));
            Assert(result.Success, "Expected aircraft to path over water: " + result.FailureCode);
            Assert(result.MovementClass == MovementClass.Aircraft, "Expected aircraft movement class path.");
        }

        static void WaterTerrainBlocksGroundUnits()
        {
            var rules = DemoRules.CreateDefaultRules();
            var map = new GridMap(8, 8);
            map.SetTerrainKind(new Int2(3, 3), TerrainKind.Water);
            Assert(!map.IsPassableForUnit(new Int2(3, 3), MovementClass.Wheeled, rules), "Expected water to block wheeled units.");
            Assert(!map.IsPassableForUnit(new Int2(3, 3), MovementClass.Tracked, rules), "Expected water to block tracked units.");
            Assert(map.IsPassableForUnit(new Int2(3, 3), MovementClass.Aircraft, rules), "Expected aircraft to pass water.");
        }

        static void NavalMovementClassCanPathOverWater()
        {
            var rules = DemoRules.CreateDefaultRules();
            var map = new GridMap(8, 4);
            for (var x = 0; x < 8; x++)
                map.SetTerrainKind(new Int2(x, 1), TerrainKind.Water);

            var path = new GridPathfinder().QueryPath(map, rules, new Int2(1, 1), new Int2(6, 1), MovementClass.Naval);
            Assert(path.Success, "Expected naval path over water: " + path.FailureCode);
            Assert(!map.IsPassableForUnit(new Int2(1, 0), MovementClass.Naval, rules), "Expected naval movement blocked on clear land.");
        }

        static void AirNavalDeterminismSmokeTest()
        {
            var a = RunAirNavalDeterministicSequence();
            var b = RunAirNavalDeterministicSequence();
            Assert(a == b, "Expected air/naval deterministic summaries to match.");
        }

        static void VerticalSliceWorldCreates()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            Assert(world.Map.Width == 32 && world.Map.Height == 64, "Expected 32x64 vertical slice map.");
            Assert(world.MatchState.IsConfigured, "Expected vertical slice scenario configuration.");
            Assert(world.ValidateMapForPlayer(1).Success, "Expected valid vertical slice map.");
        }

        static void VerticalSliceWorldHasPlayerBase()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            Assert(world.FirstActorOfType("fabrication_hub", 1) != null, "Expected player fabrication hub.");
            Assert(world.FirstActorOfType("refinery", 1) != null, "Expected player refinery.");
            Assert(world.FirstActorOfType("war_factory", 1) != null, "Expected player war factory.");
            Assert(world.FirstActorOfType("comm_center", 1) != null, "Expected player comm center.");
        }

        static void VerticalSliceWorldHasEnemyBase()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            Assert(world.FirstActorOfType("fabrication_hub", 2) != null, "Expected enemy fabrication hub.");
            Assert(world.FirstActorOfType("refinery", 2) != null, "Expected enemy refinery.");
            Assert(world.FirstActorOfType("war_factory", 2) != null, "Expected enemy war factory.");
            Assert(world.FirstActorOfType("gun_tower", 2) != null, "Expected enemy gun tower.");
        }

        static void VerticalSliceWorldHasResources()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            var snapshot = world.CreateSnapshot();
            Assert(world.ResourceCells.Count >= 20, "Expected multiple resource cells in the vertical slice.");
            Assert(snapshot.Economy.Resources.Count >= 20, "Expected resource cells in the economy snapshot.");
        }

        static void VerticalSliceWorldHasFog()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            var snapshot = world.CreateSnapshot(1);
            Assert(snapshot.Fog.Cells.Count == 2048, "Expected one fog cell per map cell.");
            Assert(HasCellVisibility(snapshot.Fog, new Int2(31, 63), CellVisibility.Unexplored), "Expected far cells to remain unexplored.");
            Assert(snapshot.Minimap.ActorDots.Count > 0, "Expected minimap actor data.");
        }

        static void VerticalSliceWorldHasAiPlayer()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            var snapshot = world.CreateSnapshot();
            Assert(snapshot.Ai.Players.Count == 1, "Expected one configured AI player.");
            Assert(snapshot.Ai.Players[0].PlayerId == 2, "Expected enemy player AI.");
        }

        static void MatchStartsRunning()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            var result = world.StartMatch();
            var snapshot = world.CreateSnapshot();
            Assert(result.Success, "Expected match start success: " + result.ErrorCode);
            Assert(snapshot.Match.Phase == MatchPhase.Running, "Expected running match phase.");
            Assert(HasObjectiveState(snapshot.Scenario, "destroy_enemy_base", ScenarioObjectiveState.Active), "Expected active destroy objective.");
            Assert(HasObjectiveState(snapshot.Scenario, "protect_player_base", ScenarioObjectiveState.Active), "Expected active protect objective.");
        }

        static void DestroyEnemyBaseTriggersVictory()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            world.StartMatch();
            var enemyHub = world.FirstActorOfType("fabrication_hub", 2);
            var result = world.ApplyScenarioDamage(1, enemyHub.Id, 9999, "test_victory");
            var snapshot = world.CreateSnapshot();
            Assert(result.Success, "Expected scenario damage success: " + result.ErrorCode);
            Assert(snapshot.Match.Phase == MatchPhase.Won, "Expected enemy base destruction to win the match.");
            Assert(snapshot.Match.LocalPlayerOutcome == PlayerOutcome.Victory, "Expected local victory outcome.");
            Assert(HasObjectiveState(snapshot.Scenario, "destroy_enemy_base", ScenarioObjectiveState.Completed), "Expected completed destroy objective.");
        }

        static void VerticalSliceCanReachVictoryWithNormalCombatPath()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            world.StartMatch();
            var enemyHub = world.FirstActorOfType("fabrication_hub", 2);
            Assert(world.CreateSnapshot().Ai.Players[0].DifficultyId == "normal", "Expected default vertical slice skirmish difficulty to be Normal.");
            var combatUnits = CollectOwnedCombatUnitIds(world, 1);
            Assert(combatUnits.Count >= 3, "Expected a Stage 19 starter combat group.");

            for (var i = 0; i < combatUnits.Count; i++)
            {
                var move = world.IssueCommand(new IssueMoveOrderCommand(1, new[] { combatUnits[i] }, new Int2(20, 53)));
                Assert(move.Success, "Expected combat unit move to staging cell: " + move.ErrorCode);
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

            Assert(attackUnits.Count >= 3, "Expected combat units to reach enemy hub attack range.");
            var attack = world.IssueCommand(new IssueAttackOrderCommand(1, attackUnits, enemyHub.Id));
            Assert(attack.Success, "Expected normal attack order against enemy hub: " + attack.ErrorCode);

            RunTicks(world, 900);
            var snapshot = world.CreateSnapshot(1);
            Assert(enemyHub.IsDestroyed, "Expected normal combat to destroy the enemy hub.");
            Assert(snapshot.Match.Phase == MatchPhase.Won, "Expected normal combat victory phase.");
            Assert(snapshot.Match.LocalPlayerOutcome == PlayerOutcome.Victory, "Expected normal combat victory outcome.");
            Assert(HasObjectiveState(snapshot.Scenario, "destroy_enemy_base", ScenarioObjectiveState.Completed), "Expected normal combat to complete destroy objective.");
        }

        static void DestroyPlayerBaseTriggersDefeat()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            world.StartMatch();
            var playerHub = world.FirstActorOfType("fabrication_hub", 1);
            var result = world.ApplyScenarioDamage(2, playerHub.Id, 9999, "test_defeat");
            var snapshot = world.CreateSnapshot();
            Assert(result.Success, "Expected scenario damage success: " + result.ErrorCode);
            Assert(snapshot.Match.Phase == MatchPhase.Lost, "Expected player base destruction to lose the match.");
            Assert(snapshot.Match.LocalPlayerOutcome == PlayerOutcome.Defeat, "Expected local defeat outcome.");
            Assert(HasObjectiveState(snapshot.Scenario, "protect_player_base", ScenarioObjectiveState.Failed), "Expected failed protect objective.");
        }

        static void ObjectiveSnapshotMatchesMatchOutcome()
        {
            var victoryWorld = DemoWorldFactory.CreateVerticalSliceWorld();
            victoryWorld.StartMatch();
            var enemyHub = victoryWorld.FirstActorOfType("fabrication_hub", 2);
            victoryWorld.ApplyScenarioDamage(1, enemyHub.Id, 9999, "test_objective_victory");
            var victory = victoryWorld.CreateSnapshot(1);
            Assert(victory.Match.Phase == MatchPhase.Won, "Expected victory phase.");
            Assert(victory.Match.LocalPlayerOutcome == PlayerOutcome.Victory, "Expected victory outcome.");
            Assert(HasObjectiveState(victory.Scenario, "destroy_enemy_base", ScenarioObjectiveState.Completed), "Expected completed destroy objective after victory.");
            Assert(HasObjectiveState(victory.Scenario, "protect_player_base", ScenarioObjectiveState.Completed), "Expected protected-base objective completed after victory.");

            var defeatWorld = DemoWorldFactory.CreateVerticalSliceWorld();
            defeatWorld.StartMatch();
            var playerHub = defeatWorld.FirstActorOfType("fabrication_hub", 1);
            defeatWorld.ApplyScenarioDamage(2, playerHub.Id, 9999, "test_objective_defeat");
            var defeat = defeatWorld.CreateSnapshot(1);
            Assert(defeat.Match.Phase == MatchPhase.Lost, "Expected defeat phase.");
            Assert(defeat.Match.LocalPlayerOutcome == PlayerOutcome.Defeat, "Expected defeat outcome.");
            Assert(HasObjectiveState(defeat.Scenario, "destroy_enemy_base", ScenarioObjectiveState.Failed), "Expected destroy objective failed after defeat.");
            Assert(HasObjectiveState(defeat.Scenario, "protect_player_base", ScenarioObjectiveState.Failed), "Expected protect objective failed after defeat.");
        }

        static void VerticalSliceDeterminismSmokeTest()
        {
            var a = RunVerticalSliceDeterministicSequence();
            var b = RunVerticalSliceDeterministicSequence();
            Assert(a == b, "Expected vertical slice deterministic summaries to match.");
        }

        static AegisMapDocument ValidAegisMap(int width, int height)
        {
            var document = AegisMapDocument.CreateEmpty(width, height, "test_map_" + width + "x" + height);
            document.DisplayName = "Test Map " + width + "x" + height;
            document.PlayerStarts.Add(new AegisPlayerStart(1, 8, 8, "Player 1"));
            document.PlayerStarts.Add(new AegisPlayerStart(2, Math.Max(10, width - 9), Math.Max(10, height - 9), "Player 2"));
            document.Resources.Add(new AegisResourcePlacement(12, 12, "ore", 500));
            document.Resources.Add(new AegisResourcePlacement(Math.Max(20, width - 13), Math.Max(20, height - 13), "ore", 500));
            document.TerrainBase.Add(new AegisTerrainCell(4, 4, AegisMapTerrainIds.Road));
            document.Blockers.Add(new AegisBlockerCell(20, 20, true, "test_blocker"));
            return document;
        }

        static string TiledMapJson(string orientation, bool infinite, string layersJson)
        {
            var layers = string.IsNullOrEmpty(layersJson) ? "\"layers\":[]" : layersJson;
            return "{" +
                "\"type\":\"map\"," +
                "\"orientation\":\"" + orientation + "\"," +
                "\"renderorder\":\"right-down\"," +
                "\"width\":100," +
                "\"height\":100," +
                "\"tilewidth\":32," +
                "\"tileheight\":32," +
                "\"infinite\":" + (infinite ? "true" : "false") + "," +
                layers + "," +
                "\"tilesets\":[]" +
                "}";
        }

        static string CsvLayerData(int width, int height, int markedIndex, int markedGid)
        {
            var builder = new StringBuilder();
            var length = width * height;
            for (var i = 0; i < length; i++)
            {
                if (i != 0)
                    builder.Append(',');
                builder.Append(i == markedIndex ? markedGid : 0);
            }
            return builder.ToString();
        }

        static string RepoRoot()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "src", "Rts.Core", "Rts.Core.csproj")))
                    return directory.FullName;
                directory = directory.Parent;
            }

            return Directory.GetCurrentDirectory();
        }

        static string RunDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            var scout = world.FirstActorOfType("scout_rover", 1);
            world.IssueCommand(new BeginProductionCommand(1, hub.Id, "power_plant"));
            RunTicks(world, 20);
            world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", PlacementCell(8, 4)));
            world.IssueCommand(new IssueMoveOrderCommand(1, new[] { scout.Id }, new Int2(16, 10)));
            RunTicks(world, 96);
            return world.GetDeterminismSummary();
        }

        static string RunCombatDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var tank = world.FirstActorOfType("light_tank", 1);
            var tower = world.FirstActorOfType("gun_tower", 1);
            var target = world.FirstActorOfType("medium_tank", 2);
            world.IssueCommand(new IssueAttackOrderCommand(1, new[] { tank.Id }, target.Id));
            world.IssueCommand(new IssueAttackOrderCommand(1, new[] { tower.Id }, target.Id));
            RunTicks(world, 72);
            return world.GetDeterminismSummary();
        }

        static string RunBaseManagementDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            var barracks = world.FirstActorOfType("barracks", 1);
            var factory = world.FirstActorOfType("war_factory", 1);
            var tower = world.FirstActorOfType("gun_tower", 1);

            tower.Health -= 60;
            world.IssueCommand(new BeginRepairBuildingCommand(1, tower.Id));
            world.IssueCommand(new PowerToggleCommand(1, barracks.Id));
            world.IssueCommand(new SetRallyPointCommand(1, factory.Id, new Int2(16, 12)));
            world.IssueCommand(new BeginProductionCommand(1, factory.Id, "light_tank"));
            RunTicks(world, 8);
            world.IssueCommand(new SellBuildingCommand(1, tower.Id));
            RunTicks(world, 32);
            return world.GetDeterminismSummary();
        }

        static string RunSupportPowerDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            world.IssueCommand(new ActivateSupportPowerCommand(1, "reveal_scan", new Int2(25, 25)));
            RunTicks(world, 12);
            world.CreateActor("repair_bay", 1, new Int2(10, 6));
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            hub.Health -= 100;
            world.IssueCommand(new ActivateSupportPowerCommand(1, "emergency_repair_pulse", hub.CellPosition));
            RunTicks(world, 24);
            return world.GetDeterminismSummary();
        }

        static string RunEngineerTransportDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var engineer = world.CreateActor("engineer", 1, new Int2(17, 10));
            var target = world.FirstActorOfType("power_plant", 2);
            world.IssueCommand(new CaptureBuildingCommand(1, engineer.Id, target.Id));

            var apc = world.CreateActor("apc", 1, new Int2(7, 8));
            var infantry = world.FirstActorOfType("rifle_infantry", 1);
            world.IssueCommand(new LoadTransportCommand(1, apc.Id, new[] { infantry.Id }));
            world.IssueCommand(new IssueMoveOrderCommand(1, new[] { apc.Id }, new Int2(9, 9)));
            RunTicks(world, 24);
            world.IssueCommand(new UnloadTransportCommand(1, apc.Id, new Int2(10, 9)));
            RunTicks(world, 8);
            return world.GetDeterminismSummary();
        }

        static string RunEconomyDeterministicSequence()
        {
            var world = EconomyWorldWithHarvestOrder();
            RunTicks(world, 320);
            return world.GetDeterminismSummary();
        }

        static string RunVisibilityDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateFogRadarDemoWorld();
            var scout = world.FirstActorOfType("scout_rover", 1);
            world.CreateSnapshot(1);
            world.IssueCommand(new IssueMoveOrderCommand(1, new[] { scout.Id }, new Int2(20, 20)));
            RunTicks(world, 220);
            world.CreateSnapshot(1);
            return world.GetDeterminismSummary();
        }

        static string RunAiDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateAiSkirmishDemoWorld();
            RunTicks(world, 128);
            return world.GetDeterminismSummary();
        }

        static string RunPathingDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateMapTerrainDemoWorld();
            var scout = world.FirstActorOfType("scout_rover", 1);
            var infantry = world.FirstActorOfType("rifle_infantry", 1);
            world.QueryPath(scout.Id, new Int2(18, 6));
            world.QueryPath(infantry.Id, new Int2(9, 15));
            world.IssueCommand(new IssueMoveOrderCommand(1, new[] { scout.Id }, new Int2(18, 6)));
            RunTicks(world, 96);
            return world.GetDeterminismSummary();
        }

        static string RunAirNavalDeterministicSequence()
        {
            var world = new RtsWorld(DemoRules.CreateDefaultRules(), new GridMap(16, 16));
            world.AddPlayer(1, "Air Player", 5000);
            for (var y = 0; y < 16; y++)
                world.SetTerrainCell(new Int2(7, y), TerrainKind.Water);

            world.CreateActor("power_plant", 1, new Int2(9, 1));
            world.CreateActor("tech_center", 1, new Int2(1, 1));
            var helipad = world.CreateActor("dual_helipad", 1, new Int2(5, 5));
            var begin = world.IssueCommand(new BeginProductionCommand(1, helipad.Id, "attack_aircraft"));
            Assert(begin.Success, "Expected deterministic aircraft production start.");
            RunTicks(world, 60);

            var aircraft = world.FirstActorOfType("attack_aircraft", 1);
            Assert(aircraft != null, "Expected deterministic aircraft.");
            var move = world.IssueCommand(new IssueMoveOrderCommand(1, new[] { aircraft.Id }, new Int2(12, 8)));
            Assert(move.Success, "Expected deterministic aircraft move: " + move.ErrorCode);
            RunTicks(world, 20);
            return world.GetDeterminismSummary();
        }

        static string RunVerticalSliceDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld();
            world.StartMatch();
            var harvester = world.FirstActorOfType("harvester", 1);
            var tank = world.FirstActorOfType("light_tank", 1);
            var target = world.FirstActorOfType("rifle_infantry", 2);
            world.IssueCommand(new IssueHarvestOrderCommand(1, new[] { harvester.Id }, new Int2(15, 8)));
            world.IssueCommand(new IssueAttackOrderCommand(1, new[] { tank.Id }, target.Id));
            RunTicks(world, 80);
            world.CreateSnapshot(1);
            return world.GetDeterminismSummary();
        }

        static RtsWorld EconomyWorldWithHarvestOrder()
        {
            var world = DemoWorldFactory.CreateEconomyDemoWorld();
            var harvester = world.FirstActorOfType("harvester", 1);
            var result = world.IssueCommand(new IssueHarvestOrderCommand(1, new[] { harvester.Id }, new Int2(15, 8)));
            Assert(result.Success, "Expected harvest order success: " + result.ErrorCode);
            return world;
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

        static RtsWorld CombatWorldWithTankAttack()
        {
            var world = DemoWorldFactory.CreateCombatDemoWorld();
            var attacker = world.FirstActorOfType("light_tank", 1);
            var target = world.FirstActorOfType("medium_tank", 2);
            var result = world.IssueCommand(new IssueAttackOrderCommand(1, new[] { attacker.Id }, target.Id));
            Assert(result.Success, "Expected tank attack order success: " + result.ErrorCode);
            return world;
        }

        static bool HasEvent(WorldSnapshot snapshot, string eventType)
        {
            for (var i = 0; i < snapshot.CombatEvents.Count; i++)
                if (snapshot.CombatEvents[i].EventType == eventType)
                    return true;
            return false;
        }

        static bool HasEconomyEvent(WorldSnapshot snapshot, string eventType)
        {
            for (var i = 0; i < snapshot.Economy.Events.Count; i++)
                if (snapshot.Economy.Events[i].EventType == eventType)
                    return true;
            return false;
        }

        static bool SnapshotContainsActor(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].ActorId == actorId)
                    return true;
            return false;
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return snapshot.Actors[i];
            return null;
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

        static TransportSnapshot FindTransport(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Transports.Count; i++)
                if (snapshot.Transports[i].ActorId == actorId)
                    return snapshot.Transports[i];

            return null;
        }

        static bool HasCellVisibility(FogSnapshot snapshot, Int2 cell, CellVisibility visibility)
        {
            for (var i = 0; i < snapshot.Cells.Count; i++)
                if (snapshot.Cells[i].Cell.Equals(cell) && snapshot.Cells[i].Visibility == visibility)
                    return true;
            return false;
        }

        static bool HasAiIntent(WorldSnapshot snapshot, string kind)
        {
            return HasAiIntent(snapshot, kind, null);
        }

        static bool HasAiIntent(WorldSnapshot snapshot, string kind, string intentId)
        {
            for (var playerIndex = 0; playerIndex < snapshot.Ai.Players.Count; playerIndex++)
            {
                var player = snapshot.Ai.Players[playerIndex];
                for (var intentIndex = 0; intentIndex < player.RecentIntents.Count; intentIndex++)
                {
                    var intent = player.RecentIntents[intentIndex];
                    if (intent.Kind == kind && (intentId == null || intent.IntentId == intentId))
                        return true;
                }
            }

            return false;
        }

        static bool HasSuccessfulAiIntent(WorldSnapshot snapshot, string kind, string intentId)
        {
            for (var playerIndex = 0; playerIndex < snapshot.Ai.Players.Count; playerIndex++)
            {
                var player = snapshot.Ai.Players[playerIndex];
                for (var intentIndex = 0; intentIndex < player.RecentIntents.Count; intentIndex++)
                {
                    var intent = player.RecentIntents[intentIndex];
                    if (intent.Kind == kind && intent.IntentId == intentId && intent.WasCommandIssued && intent.CommandSucceeded)
                        return true;
                }
            }

            return false;
        }

        static int FirstSuccessfulAttackWaveTick(AiDifficultyDefinition difficulty, int maxTicks)
        {
            var world = DemoWorldFactory.CreateVerticalSliceWorld(difficulty);
            world.StartMatch();
            for (var i = 0; i < maxTicks; i++)
            {
                world.Tick();
                var snapshot = world.CreateSnapshot();
                if (HasSuccessfulAiIntent(snapshot, "Attack", "BasicAttackWave"))
                    return snapshot.Tick;
            }

            return -1;
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

        static bool ContainsText(IReadOnlyList<string> values, string text)
        {
            for (var i = 0; i < values.Count; i++)
                if (values[i].Contains(text))
                    return true;
            return false;
        }

        static bool HasTerrainKind(MapSnapshot snapshot, string kind)
        {
            for (var i = 0; i < snapshot.TerrainCells.Count; i++)
                if (snapshot.TerrainCells[i].Kind == kind)
                    return true;
            return false;
        }

        static AegisMapGenerationResult GenerateAegisMap(AegisMapGenerationPreset preset, int seed)
        {
            return GenerateAegisMap(preset, seed, AegisMapIntensity.Medium, AegisMapIntensity.Low, AegisMapIntensity.Low);
        }

        static AegisMapGenerationResult GenerateAegisMap(AegisMapGenerationPreset preset, int seed, AegisMapIntensity resourceDensity, AegisMapIntensity cliffDensity, AegisMapIntensity rockiness)
        {
            var request = AegisMapGenerationRequest.CreateDefault();
            request.SizePreset = preset;
            request.Seed = seed;
            request.HasExplicitSeed = true;
            request.PlayerCount = preset == AegisMapGenerationPreset.Large ? 4 : 2;
            request.ResourceDensity = resourceDensity;
            request.CliffDensity = cliffDensity;
            request.Rockiness = rockiness;
            request.WaterAmount = AegisMapWaterAmount.Low;
            request.Symmetry = AegisMapSymmetryMode.Horizontal;
            request.GameplayProfile = AegisMapGameplayProfile.Balanced;
            request.OreRegenerationEnabled = true;
            request.OreRegenerationRatePerTick = 2;
            request.OreRegenerationDelayTicks = 30;
            return new ProjectAegisRTS.Maps.Generation.AegisMapGenerator().Generate(request);
        }

        static string AegisMapDocumentSignature(AegisMapDocument document)
        {
            var parts = new List<string>();
            parts.Add(document.Width + "x" + document.Height);
            for (var i = 0; i < document.TerrainBase.Count; i++)
                parts.Add("T:" + document.TerrainBase[i].X + "," + document.TerrainBase[i].Y + ":" + document.TerrainBase[i].TerrainId);
            for (var i = 0; i < document.Blockers.Count; i++)
                parts.Add("B:" + document.Blockers[i].X + "," + document.Blockers[i].Y + ":" + document.Blockers[i].Reason);
            for (var i = 0; i < document.Resources.Count; i++)
                parts.Add("R:" + document.Resources[i].X + "," + document.Resources[i].Y + ":" + document.Resources[i].Amount + ":" + document.Resources[i].RegenerationRatePerTick);
            for (var i = 0; i < document.PlayerStarts.Count; i++)
                parts.Add("S:" + document.PlayerStarts[i].PlayerId + ":" + document.PlayerStarts[i].X + "," + document.PlayerStarts[i].Y);
            return string.Join("|", parts);
        }

        static int CountBlockersWithReason(AegisMapDocument document, string reason)
        {
            var count = 0;
            for (var i = 0; i < document.Blockers.Count; i++)
                if (document.Blockers[i].Reason == reason)
                    count++;
            return count;
        }

        static AegisResourceSimulation OreSimulation(int amount, int maxAmount, int rate, int delay)
        {
            var sim = new AegisResourceSimulation(new ResourceRegenerationRules(delay, rate));
            sim.AddField(new ResourceFieldState("ore_1", new Int2(4, 4), "ore", amount, maxAmount));
            return sim;
        }

        static string RunResourceSimulationSignature()
        {
            var sim = OreSimulation(18, 30, 3, 2);
            var builder = new StringBuilder();
            for (var i = 0; i < 4; i++)
            {
                sim.Harvest("ore_1", 4);
                sim.Tick();
                builder.Append(sim.TickNumber).Append(':').Append(sim.GetField("ore_1").Amount).Append('|');
            }

            for (var i = 0; i < 8; i++)
            {
                sim.Tick();
                builder.Append(sim.TickNumber).Append(':').Append(sim.GetField("ore_1").Amount).Append('|');
            }

            return builder.ToString();
        }

        static string GeneratedMapSignature(GeneratedMapResult generated)
        {
            var parts = new List<string>();
            for (var i = 0; i < generated.TerrainCells.Count; i++)
                parts.Add(generated.TerrainCells[i].Kind.ToString());
            for (var i = 0; i < generated.Resources.Count; i++)
                parts.Add("R:" + generated.Resources[i].Cell + ":" + generated.Resources[i].Amount);
            for (var i = 0; i < generated.Spawns.Count; i++)
                parts.Add("S:" + generated.Spawns[i].PlayerId + ":" + generated.Spawns[i].Cell);
            return string.Join("|", parts);
        }

        static bool AnyReachableResource(RtsWorld world, ActorState harvester)
        {
            foreach (var pair in world.ResourceCells)
            {
                var result = world.QueryPath(harvester.Id, pair.Value.Cell);
                if (result.Success)
                    return true;
            }

            return false;
        }

        static bool HasObjectiveState(ScenarioSnapshot snapshot, string objectiveId, ScenarioObjectiveState state)
        {
            for (var i = 0; i < snapshot.Objectives.Count; i++)
                if (snapshot.Objectives[i].ObjectiveId == objectiveId && snapshot.Objectives[i].State == state)
                    return true;
            return false;
        }

        static RtsWorld WorldWithCompletedPowerPlant()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            var result = world.IssueCommand(new BeginProductionCommand(1, hub.Id, "power_plant"));
            Assert(result.Success, "Expected power plant production start.");
            RunTicks(world, 20);
            return world;
        }

        static RtsWorld WorldWithPlacedBarracks()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            world.IssueCommand(new BeginProductionCommand(1, hub.Id, "barracks"));
            RunTicks(world, 24);
            var place = world.IssueCommand(new PlaceBuildingCommand(1, "barracks", PlacementCell(4, 8)));
            Assert(place.Success, "Expected barracks placement success: " + place.ErrorCode);
            return world;
        }

        static Int2 PlacementCell(int coarseX, int coarseY)
        {
            return PlacementGridMetrics.CoarseCellToPlacementCell(new Int2(coarseX, coarseY));
        }

        static void RunTicks(RtsWorld world, int ticks)
        {
            for (var i = 0; i < ticks; i++)
                world.Tick();
        }

        static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
