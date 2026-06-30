using System;
using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Production;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;
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
                BuildingPlacementSucceedsWithinConstructionRadius,
                ProductionDeductsCredits,
                ProductionCompletesAfterExpectedTicks,
                UnitSpawnsFromBarracks,
                MovementPathReachesDestination,
                LowPowerTogglesLightsAndMachineryFalse,
                DeterminismSmokeTest,
                WeaponDefinitionsExistForMvpActors,
                AttackOrderFailsForMissingTarget,
                AttackOrderFailsForDestroyedAttacker,
                AttackOrderStartsCombatForValidTarget,
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
                VisibilityDeterminismSmokeTest
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
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", new Int2(31, 31)));
            Assert(!result.Success && result.ErrorCode == "OutsideMap", "Expected OutsideMap, got " + result.ErrorCode);
        }

        static void BuildingPlacementFailsOnOccupiedFootprint()
        {
            var world = WorldWithCompletedPowerPlant();
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", new Int2(4, 4)));
            Assert(!result.Success && result.ErrorCode == "OccupiedCell", "Expected OccupiedCell, got " + result.ErrorCode);
        }

        static void BuildingPlacementSucceedsWithinConstructionRadius()
        {
            var world = WorldWithCompletedPowerPlant();
            var result = world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", new Int2(8, 4)));
            Assert(result.Success, "Expected placement success: " + result.ErrorCode);
            Assert(world.FirstActorOfType("power_plant", 1) != null, "Expected placed power plant.");
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

        static string RunDeterministicSequence()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            var scout = world.FirstActorOfType("scout_rover", 1);
            world.IssueCommand(new BeginProductionCommand(1, hub.Id, "power_plant"));
            RunTicks(world, 20);
            world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", new Int2(8, 4)));
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

        static RtsWorld EconomyWorldWithHarvestOrder()
        {
            var world = DemoWorldFactory.CreateEconomyDemoWorld();
            var harvester = world.FirstActorOfType("harvester", 1);
            var result = world.IssueCommand(new IssueHarvestOrderCommand(1, new[] { harvester.Id }, new Int2(15, 8)));
            Assert(result.Success, "Expected harvest order success: " + result.ErrorCode);
            return world;
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

        static bool HasCellVisibility(FogSnapshot snapshot, Int2 cell, CellVisibility visibility)
        {
            for (var i = 0; i < snapshot.Cells.Count; i++)
                if (snapshot.Cells[i].Cell.Equals(cell) && snapshot.Cells[i].Visibility == visibility)
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
            var place = world.IssueCommand(new PlaceBuildingCommand(1, "barracks", new Int2(4, 8)));
            Assert(place.Success, "Expected barracks placement success: " + place.ErrorCode);
            return world;
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
