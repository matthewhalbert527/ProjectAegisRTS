using ProjectAegisRTS.Ai;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.MapGeneration;
using ProjectAegisRTS.Scenarios;
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
            return CreateAiSkirmishDemoWorld(new AiDifficultyDefinition("stage12-standard", 16, 3, 2, 12012));
        }

        public static RtsWorld CreateAiSkirmishDemoWorld(AiDifficultyDefinition difficulty)
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

            world.ConfigureAiPlayer(new AiPlayerDefinition(2, difficulty ?? AiDifficultyDefinition.CreateStandard()));
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

        public static RtsWorld CreateGeneratedSkirmishWorld(int seed, AiDifficultyDefinition aiDifficulty)
        {
            return CreateGeneratedSkirmishWorld(MapGenerationSettings.CreateDefaultSkirmish(seed), aiDifficulty);
        }

        public static RtsWorld CreateGeneratedSkirmishWorld(MapGenerationSettings settings, AiDifficultyDefinition aiDifficulty)
        {
            var rules = DemoRules.CreateDefaultRules();
            var generated = new AegisMapGenerator().Generate(settings ?? MapGenerationSettings.CreateDefaultSkirmish(34034));
            var world = new RtsWorld(rules, new GridMap(generated.Width, generated.Height));

            for (var i = 0; i < generated.TerrainCells.Count; i++)
                world.SetTerrainCell(generated.TerrainCells[i].Cell, generated.TerrainCells[i].Kind);

            world.AddPlayer(1, "Aegis Commander", 5000);
            world.AddPlayer(2, "Generated Rival AI", 3500);

            for (var i = 0; i < generated.Resources.Count; i++)
                world.AddResourceCell(generated.Resources[i].Cell, generated.Resources[i].Kind, generated.Resources[i].Amount);

            var playerSpawn = FindSpawn(generated, 1);
            var enemySpawn = FindSpawn(generated, 2);
            StampGeneratedBase(world, 1, playerSpawn, generated.Width);
            StampGeneratedBase(world, 2, enemySpawn, generated.Width);

            world.ConfigureAiPlayer(new AiPlayerDefinition(2, aiDifficulty ?? AiDifficultyDefinition.CreateNormal()));
            world.ConfigureScenario(ScenarioDefinition.CreateVerticalSlice(1, 2));
            return world;
        }

        public static RtsWorld CreateVerticalSliceWorld()
        {
            return CreateVerticalSliceWorld(AiDifficultyDefinition.CreateNormal());
        }

        public static RtsWorld CreateVerticalSliceWorld(AiDifficultyDefinition aiDifficulty)
        {
            var rules = DemoRules.CreateDefaultRules();
            var world = new RtsWorld(rules, new GridMap(32, 64));
            world.AddPlayer(1, "Aegis Commander", 5000);
            world.AddPlayer(2, "Deterministic Rival AI", 3500);

            for (var x = 3; x <= 28; x++)
            {
                world.SetTerrainCell(new Int2(x, 13), TerrainKind.Road);
                world.SetTerrainCell(new Int2(x, 45), TerrainKind.Road);
            }

            for (var y = 13; y <= 45; y++)
                world.SetTerrainCell(new Int2(18, y), TerrainKind.Road);

            for (var y = 15; y <= 17; y++)
                for (var x = 12; x <= 16; x++)
                    world.SetTerrainCell(new Int2(x, y), TerrainKind.Rough);

            for (var y = 47; y <= 49; y++)
                for (var x = 12; x <= 16; x++)
                    world.SetTerrainCell(new Int2(x, y), TerrainKind.Rough);

            for (var y = 14; y <= 18; y++)
                for (var x = 6; x <= 9; x++)
                    world.SetTerrainCell(new Int2(x, y), TerrainKind.Forest);

            for (var y = 46; y <= 50; y++)
                for (var x = 6; x <= 9; x++)
                    world.SetTerrainCell(new Int2(x, y), TerrainKind.Forest);

            for (var y = 4; y <= 10; y++)
                if (y != 7)
                    world.SetTerrainCell(new Int2(20, y), TerrainKind.Water);

            for (var x = 2; x <= 8; x++)
                world.SetTerrainCell(new Int2(x, 22), TerrainKind.Cliff);

            for (var x = 2; x <= 8; x++)
                world.SetTerrainCell(new Int2(x, 54), TerrainKind.Cliff);

            for (var x = 22; x <= 28; x++)
                world.SetTerrainCell(new Int2(x, 35), TerrainKind.Rough);

            world.CreateActor("fabrication_hub", 1, new Int2(3, 3));
            world.CreateActor("power_plant", 1, new Int2(7, 3));
            world.CreateActor("barracks", 1, new Int2(10, 3));
            world.CreateActor("war_factory", 1, new Int2(7, 7));
            world.CreateActor("refinery", 1, new Int2(3, 8));
            world.CreateActor("comm_center", 1, new Int2(11, 7));
            world.CreateActor("tech_center", 1, new Int2(14, 3));
            world.CreateActor("advanced_power_plant", 1, new Int2(16, 3));
            world.CreateActor("dual_helipad", 1, new Int2(14, 6));
            world.CreateActor("attack_aircraft", 1, new Int2(14, 6));
            world.CreateActor("gun_tower", 1, new Int2(13, 10));
            world.CreateActor("harvester", 1, new Int2(8, 12));
            world.CreateActor("scout_rover", 1, new Int2(10, 11));
            world.CreateActor("light_tank", 1, new Int2(14, 10));
            world.CreateActor("light_tank", 1, new Int2(13, 12));
            world.CreateActor("engineer", 1, new Int2(9, 10));
            world.CreateActor("apc", 1, new Int2(15, 10));
            world.CreateActor("rifle_infantry", 1, new Int2(12, 11));
            world.CreateActor("rocket_infantry", 1, new Int2(11, 12));

            world.CreateActor("fabrication_hub", 2, new Int2(23, 54));
            world.CreateActor("power_plant", 2, new Int2(20, 54));
            world.CreateActor("barracks", 2, new Int2(23, 50));
            world.CreateActor("war_factory", 2, new Int2(18, 56));
            world.CreateActor("refinery", 2, new Int2(27, 49));
            world.CreateActor("gun_tower", 2, new Int2(22, 52));
            world.CreateActor("medium_tank", 2, new Int2(20, 46));
            world.CreateActor("rifle_infantry", 2, new Int2(21, 47));
            world.CreateActor("scout_rover", 2, new Int2(21, 52));
            world.CreateActor("harvester", 2, new Int2(26, 53));

            for (var y = 11; y <= 13; y++)
                for (var x = 14; x <= 17; x++)
                    world.AddResourceCell(new Int2(x, y), ResourceKind.Ore, 220);

            for (var y = 29; y <= 31; y++)
                for (var x = 11; x <= 14; x++)
                    world.AddResourceCell(new Int2(x, y), ResourceKind.Ore, 180);

            for (var y = 46; y <= 48; y++)
                for (var x = 25; x <= 28; x++)
                    world.AddResourceCell(new Int2(x, y), ResourceKind.Ore, 160);

            world.ConfigureAiPlayer(new AiPlayerDefinition(2, aiDifficulty ?? AiDifficultyDefinition.CreateNormal()));
            world.ConfigureScenario(ScenarioDefinition.CreateVerticalSlice(1, 2));
            return world;
        }

        static Int2 FindSpawn(GeneratedMapResult generated, int playerId)
        {
            for (var i = 0; i < generated.Spawns.Count; i++)
                if (generated.Spawns[i].PlayerId == playerId)
                    return generated.Spawns[i].Cell;

            return playerId == 1
                ? new Int2(6, generated.Height / 2)
                : new Int2(generated.Width - 7, generated.Height / 2);
        }

        static void StampGeneratedBase(RtsWorld world, int playerId, Int2 spawn, int mapWidth)
        {
            CreateGeneratedBuilding(world, playerId, "fabrication_hub", BaseTopLeft(world, "fabrication_hub", spawn, mapWidth, -2, -1));
            CreateGeneratedBuilding(world, playerId, "power_plant", BaseTopLeft(world, "power_plant", spawn, mapWidth, -2, 3));
            CreateGeneratedBuilding(world, playerId, "barracks", BaseTopLeft(world, "barracks", spawn, mapWidth, 2, -1));
            CreateGeneratedBuilding(world, playerId, "war_factory", BaseTopLeft(world, "war_factory", spawn, mapWidth, 6, -1));
            CreateGeneratedBuilding(world, playerId, "refinery", BaseTopLeft(world, "refinery", spawn, mapWidth, 2, 3));
            CreateGeneratedBuilding(world, playerId, "gun_tower", BaseTopLeft(world, "gun_tower", spawn, mapWidth, 7, 3));

            CreateGeneratedUnit(world, playerId, "harvester", BaseUnitCell(spawn, mapWidth, 7, 6));
            CreateGeneratedUnit(world, playerId, "light_tank", BaseUnitCell(spawn, mapWidth, 10, -3));
            CreateGeneratedUnit(world, playerId, "rifle_infantry", BaseUnitCell(spawn, mapWidth, 10, -1));
            CreateGeneratedUnit(world, playerId, "scout_rover", BaseUnitCell(spawn, mapWidth, 10, 1));
        }

        static Int2 BaseTopLeft(RtsWorld world, string typeId, Int2 spawn, int mapWidth, int localX, int localY)
        {
            var footprint = BuildingFootprint(world, typeId);
            if (spawn.X < mapWidth / 2)
                return new Int2(spawn.X + localX, spawn.Y + localY);

            return new Int2(spawn.X + 1 - localX - footprint.X, spawn.Y + localY);
        }

        static Int2 BaseUnitCell(Int2 spawn, int mapWidth, int localX, int localY)
        {
            if (spawn.X < mapWidth / 2)
                return new Int2(spawn.X + localX, spawn.Y + localY);

            return new Int2(spawn.X - localX, spawn.Y + localY);
        }

        static void CreateGeneratedBuilding(RtsWorld world, int playerId, string typeId, Int2 topLeft)
        {
            var footprint = BuildingFootprint(world, typeId);
            ClearRect(world, topLeft, footprint);
            world.CreateActor(typeId, playerId, topLeft);
        }

        static void CreateGeneratedUnit(RtsWorld world, int playerId, string typeId, Int2 cell)
        {
            ClearRect(world, cell, new Int2(1, 1));
            world.CreateActor(typeId, playerId, cell);
        }

        static Int2 BuildingFootprint(RtsWorld world, string typeId)
        {
            var building = world.Rules.GetDefinition(typeId) as BuildingDefinition;
            return building == null ? new Int2(1, 1) : building.FootprintCells;
        }

        static void ClearRect(RtsWorld world, Int2 topLeft, Int2 size)
        {
            for (var y = -1; y <= size.Y; y++)
                for (var x = -1; x <= size.X; x++)
                {
                    var cell = new Int2(topLeft.X + x, topLeft.Y + y);
                    if (world.Map.Contains(cell))
                        world.SetTerrainCell(cell, TerrainKind.Clear);
                }
        }
    }
}
