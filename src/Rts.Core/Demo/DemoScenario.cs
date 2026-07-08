using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Power;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Snapshots;

namespace ProjectAegisRTS.Demo
{
    public static class DemoScenario
    {
        public static WorldSnapshot RunStage0Script()
        {
            var world = DemoWorldFactory.CreateMvpWorld();
            var hub = world.FirstActorOfType("fabrication_hub", 1);
            world.IssueCommand(new BeginProductionCommand(1, hub.Id, "power_plant"));
            RunTicks(world, 20);
            world.IssueCommand(new PlaceBuildingCommand(1, "power_plant", PlacementGridMetrics.CoarseCellToPlacementCell(new Int2(8, 4))));

            world.IssueCommand(new BeginProductionCommand(1, hub.Id, "barracks"));
            RunTicks(world, 24);
            world.IssueCommand(new PlaceBuildingCommand(1, "barracks", PlacementGridMetrics.CoarseCellToPlacementCell(new Int2(4, 8))));

            var barracks = world.FirstActorOfType("barracks", 1);
            world.IssueCommand(new BeginProductionCommand(1, barracks.Id, "rifle_infantry"));
            RunTicks(world, 10);

            var scout = world.FirstActorOfType("scout_rover", 1);
            world.IssueCommand(new IssueMoveOrderCommand(1, new[] { scout.Id }, new Int2(16, 10)));
            RunTicks(world, 96);

            world.ForcePlayerPowerState(1, PlayerPowerState.LowPower);
            world.Tick();
            return world.CreateSnapshot();
        }

        static void RunTicks(RtsWorld world, int ticks)
        {
            for (var i = 0; i < ticks; i++)
                world.Tick();
        }
    }
}
