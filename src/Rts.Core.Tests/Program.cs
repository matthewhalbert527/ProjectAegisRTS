using System;
using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Production;
using ProjectAegisRTS.Simulation;

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
                DeterminismSmokeTest
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

            Console.WriteLine("All Stage 0 tests passed (" + passed + "/" + tests.Count + ").");
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
