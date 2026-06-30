using ProjectAegisRTS.Ai;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Terrain;

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

        public static RtsWorld CreateAiSkirmishDemoWorld()
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 32));
            world.AddPlayer(1, "Aegis Skirmish Player", 5000);
            world.AddPlayer(2, "Deterministic AI Player", 3000);

            world.CreateActor("fabrication_hub", 1, new Int2(3, 3));
            world.CreateActor("light_tank", 1, new Int2(15, 9));
            world.CreateActor("rifle_infantry", 1, new Int2(16, 10));

            world.CreateActor("fabrication_hub", 2, new Int2(20, 20));
            world.CreateActor("power_plant", 2, new Int2(24, 20));
            world.CreateActor("refinery", 2, new Int2(20, 24));
            world.CreateActor("barracks", 2, new Int2(24, 23));
            world.CreateActor("harvester", 2, new Int2(18, 24));
            world.CreateActor("rifle_infantry", 2, new Int2(13, 9));
            world.CreateActor("scout_rover", 2, new Int2(12, 9));

            for (var y = 22; y <= 23; y++)
                for (var x = 16; x <= 18; x++)
                    world.AddResourceCell(new Int2(x, y), ResourceKind.Ore, 150);

            world.ConfigureAiPlayer(new AiPlayerDefinition(2, new AiDifficultyDefinition("stage12-standard", 16, 3, 2, 12012)));
            return world;
        }

        public static RtsWorld CreateMapTerrainDemoWorld()
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 32));
            world.AddPlayer(1, "Aegis Terrain Player", 5000);
            world.AddPlayer(2, "Terrain Test Enemy", 5000);

            for (var x = 3; x <= 24; x++)
                world.SetTerrainCell(new Int2(x, 6), TerrainKind.Road);

            for (var y = 8; y <= 13; y++)
                for (var x = 12; x <= 15; x++)
                    world.SetTerrainCell(new Int2(x, y), TerrainKind.Rough);

            for (var y = 12; y <= 18; y++)
                for (var x = 7; x <= 10; x++)
                    world.SetTerrainCell(new Int2(x, y), TerrainKind.Forest);

            for (var y = 3; y <= 18; y++)
                if (y != 6)
                    world.SetTerrainCell(new Int2(20, y), TerrainKind.Water);

            for (var y = 20; y <= 26; y++)
                world.SetTerrainCell(new Int2(24, y), TerrainKind.Cliff);

            world.CreateActor("fabrication_hub", 1, new Int2(3, 3));
            world.CreateActor("refinery", 1, new Int2(6, 6));
            world.CreateActor("harvester", 1, new Int2(10, 6));
            world.CreateActor("scout_rover", 1, new Int2(4, 10));
            world.CreateActor("rifle_infantry", 1, new Int2(6, 10));
            world.CreateActor("medium_tank", 1, new Int2(5, 12));

            world.CreateActor("rifle_infantry", 2, new Int2(23, 6));
            world.CreateActor("gun_tower", 2, new Int2(26, 7));

            for (var y = 8; y <= 9; y++)
                for (var x = 16; x <= 18; x++)
                    world.AddResourceCell(new Int2(x, y), ResourceKind.Ore, 140);

            return world;
        }
    }
}
