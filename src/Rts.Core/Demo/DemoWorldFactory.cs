using ProjectAegisRTS.Core;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Simulation;

namespace ProjectAegisRTS.Demo
{
    public static class DemoWorldFactory
    {
        public static RtsWorld CreateMvpWorld()
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 32));
            world.AddPlayer(1, "Aegis Test Player", 5000);
            world.CreateActor("fabrication_hub", 1, new Int2(4, 4));
            world.CreateActor("scout_rover", 1, new Int2(10, 4));
            return world;
        }

        public static RtsWorld CreateCombatDemoWorld()
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 32));
            world.AddPlayer(1, "Aegis Combat Player", 5000);
            world.AddPlayer(2, "Enemy Test Player", 5000);

            world.CreateActor("fabrication_hub", 1, new Int2(3, 3));
            world.CreateActor("light_tank", 1, new Int2(8, 8));
            world.CreateActor("rifle_infantry", 1, new Int2(7, 9));
            world.CreateActor("gun_tower", 1, new Int2(10, 10));

            world.CreateActor("medium_tank", 2, new Int2(12, 8));
            world.CreateActor("rifle_infantry", 2, new Int2(13, 9));
            world.CreateActor("gun_tower", 2, new Int2(15, 10));
            world.CreateActor("power_plant", 2, new Int2(18, 10));

            return world;
        }

        public static RtsWorld CreateEconomyDemoWorld()
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 32));
            world.AddPlayer(1, "Aegis Economy Player", 1000);

            world.CreateActor("fabrication_hub", 1, new Int2(3, 3));
            world.CreateActor("refinery", 1, new Int2(6, 6));
            world.CreateActor("harvester", 1, new Int2(12, 8));
            world.CreateActor("scout_rover", 1, new Int2(10, 4));

            for (var y = 7; y <= 9; y++)
                for (var x = 15; x <= 17; x++)
                    world.AddResourceCell(new Int2(x, y), ResourceKind.Ore, 120);

            return world;
        }

        public static RtsWorld CreateFogRadarDemoWorld()
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 32));
            world.AddPlayer(1, "Aegis Fog Player", 5000);
            world.AddPlayer(2, "Hidden Test Player", 5000);

            world.CreateActor("fabrication_hub", 1, new Int2(3, 3));
            world.CreateActor("comm_center", 1, new Int2(7, 3));
            world.CreateActor("scout_rover", 1, new Int2(8, 8));
            world.CreateActor("rifle_infantry", 1, new Int2(5, 8));

            world.CreateActor("rifle_infantry", 2, new Int2(12, 8));
            world.CreateActor("medium_tank", 2, new Int2(25, 25));

            for (var y = 10; y <= 11; y++)
                for (var x = 14; x <= 16; x++)
                    world.AddResourceCell(new Int2(x, y), ResourceKind.Ore, 100);

            return world;
        }
    }
}
