using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Terrain;
using ProjectAegisRTS.Visibility;

namespace ProjectAegisRTS.Demo
{
    public static class DemoRules
    {
        public static RtsRules CreateDefaultRules()
        {
            var actors = new List<ActorDefinition>();
            var buildingAnimation = new AnimationStateDefinition("building_idle_powered", "building_production_active", "building_low_power", "building_offline");
            var unitAnimation = new AnimationStateDefinition("unit_idle", "unit_active", "unit_idle", "unit_idle");

            actors.Add(Building(
                "fabrication_hub", "Fabrication Hub", 2000, ProductionKind.None, 0, 0, string.Empty,
                25, 10, new Int2(3, 3), true, 7, new Int2(1, 3),
                new[] { "power_plant", "barracks", "war_factory", "refinery", "gun_tower" },
                buildingAnimation));

            actors.Add(Building("power_plant", "Power Plant", 900, ProductionKind.Building, 300, 20, "fabrication_hub", 40, 0, new Int2(2, 2), false, 0, new Int2(0, 2), new string[0], buildingAnimation));
            actors.Add(Building("advanced_power_plant", "Advanced Power Plant", 1200, ProductionKind.Building, 650, 32, "fabrication_hub", 80, 0, new Int2(2, 2), false, 0, new Int2(0, 2), new string[0], buildingAnimation));
            actors.Add(Building("barracks", "Barracks", 1000, ProductionKind.Building, 350, 24, "fabrication_hub", 0, 8, new Int2(2, 2), false, 0, new Int2(1, 2), new[] { "rifle_infantry", "grenade_infantry", "rocket_infantry", "flame_infantry", "engineer" }, buildingAnimation));
            actors.Add(Building("war_factory", "War Factory", 1500, ProductionKind.Building, 700, 35, "fabrication_hub", 0, 15, new Int2(3, 2), false, 0, new Int2(1, 2), new[] { "light_tank", "medium_tank", "heavy_tank", "harvester", "scout_rover", "apc" }, buildingAnimation));
            actors.Add(Building("refinery", "Refinery", 1400, ProductionKind.Building, 600, 32, "fabrication_hub", 0, 12, new Int2(3, 3), false, 0, new Int2(1, 3), new string[0], buildingAnimation));
            actors.Add(Building("gun_tower", "Gun Tower", 700, ProductionKind.Building, 250, 18, "fabrication_hub", 0, 6, new Int2(1, 1), false, 0, new Int2(0, 1), new string[0], buildingAnimation, Weapon("tower_shell", "Tower Shell", 70, 6, 34, 512, ProjectileKind.Shell, true, false, true, true)));
            actors.Add(Building("field_hospital", "Field Hospital", 900, ProductionKind.Building, 450, 28, "fabrication_hub", 0, 6, new Int2(2, 2), false, 0, new Int2(1, 2), new string[0], buildingAnimation));
            actors.Add(Building("comm_center", "Comm Center", 1000, ProductionKind.Building, 800, 45, "fabrication_hub", 0, 14, new Int2(2, 2), false, 0, new Int2(1, 2), new string[0], buildingAnimation, null, 7, new RadarDefinition(true, 20)));
            actors.Add(Building("repair_bay", "Repair Bay", 1100, ProductionKind.Building, 500, 30, "fabrication_hub", 0, 10, new Int2(3, 2), false, 0, new Int2(1, 2), new string[0], buildingAnimation));
            actors.Add(Building("tech_center", "Tech Center", 1300, ProductionKind.Building, 1200, 60, "fabrication_hub", 0, 18, new Int2(2, 2), false, 0, new Int2(1, 2), new string[0], buildingAnimation));
            actors.Add(Building("cannon_turret", "Cannon Turret", 850, ProductionKind.Building, 400, 26, "fabrication_hub", 0, 8, new Int2(1, 1), false, 0, new Int2(0, 1), new string[0], buildingAnimation, Weapon("cannon_turret_shell", "Cannon Turret Shell", 110, 7, 42, 448, ProjectileKind.Shell, true, false, true, true)));
            actors.Add(Building("advanced_gun_tower", "Advanced Gun Tower", 1000, ProductionKind.Building, 550, 34, "fabrication_hub", 0, 12, new Int2(1, 1), false, 0, new Int2(0, 1), new string[0], buildingAnimation, Weapon("advanced_tower_shell", "Advanced Tower Shell", 95, 7, 28, 576, ProjectileKind.Shell, true, true, true, true)));
            actors.Add(Building("dual_helipad", "Dual Helipad", 1200, ProductionKind.Building, 700, 36, "fabrication_hub", 0, 12, new Int2(3, 2), false, 0, new Int2(1, 2), new[] { "attack_aircraft", "heavy_lifter_aircraft" }, buildingAnimation));

            actors.Add(Unit("rifle_infantry", "Rifle Infantry", 120, ProductionKind.Infantry, 100, 10, "barracks", 256, 45, "infantry_basic", unitAnimation, Weapon("rifle_bullet", "Rifle Bullet", 24, 5, 12, 768, ProjectileKind.Bullet, true, false, true, true)));
            actors.Add(Unit("grenade_infantry", "Grenade Infantry", 115, ProductionKind.Infantry, 130, 12, "barracks", 240, 45, "infantry_heavy", unitAnimation, Weapon("grenade_placeholder", "Grenade Placeholder", 45, 4, 28, 384, ProjectileKind.Shell, true, false, true, true)));
            actors.Add(Unit("rocket_infantry", "Rocket Infantry", 110, ProductionKind.Infantry, 180, 16, "barracks", 220, 45, "infantry_heavy", unitAnimation, Weapon("rocket_placeholder", "Rocket Placeholder", 80, 6, 42, 360, ProjectileKind.Rocket, true, true, true, true)));
            actors.Add(Unit("flame_infantry", "Flame Infantry", 125, ProductionKind.Infantry, 170, 16, "barracks", 230, 45, "infantry_assault", unitAnimation, Weapon("flame_burst", "Flame Burst", 34, 3, 18, 0, ProjectileKind.None, true, false, true, true)));
            actors.Add(Unit("engineer", "Engineer", 90, ProductionKind.Infantry, 250, 20, "barracks", 240, 45, "infantry_utility", unitAnimation, null));
            actors.Add(Unit("light_tank", "Light Tank", 450, ProductionKind.Vehicle, 500, 30, "war_factory", 128, 18, "tracked_light", unitAnimation, Weapon("light_tank_shell", "Light Tank Shell", 90, 6, 32, 512, ProjectileKind.Shell, true, false, true, true)));
            actors.Add(Unit("medium_tank", "Medium Tank", 600, ProductionKind.Vehicle, 700, 38, "war_factory", 112, 15, "tracked_medium", unitAnimation, Weapon("medium_tank_shell", "Medium Tank Shell", 115, 6, 38, 480, ProjectileKind.Shell, true, false, true, true)));
            actors.Add(Unit("heavy_tank", "Heavy Tank", 850, ProductionKind.Vehicle, 950, 48, "war_factory", 96, 12, "tracked_heavy", unitAnimation, Weapon("heavy_tank_shell", "Heavy Tank Shell", 155, 7, 48, 416, ProjectileKind.Shell, true, false, true, true)));
            actors.Add(Unit("harvester", "Harvester", 700, ProductionKind.Vehicle, 700, 35, "war_factory", 96, 12, "wheeled_heavy", unitAnimation, null));
            actors.Add(Unit("scout_rover", "Scout Rover", 300, ProductionKind.Vehicle, 300, 20, "war_factory", 160, 24, "wheeled_scout", unitAnimation, Weapon("scout_rover_burst", "Scout Rover Burst", 28, 4, 18, 768, ProjectileKind.Bullet, true, false, false, true), 8));
            actors.Add(Unit("apc", "APC", 550, ProductionKind.Vehicle, 600, 34, "war_factory", 128, 18, "wheeled_apc", unitAnimation, Weapon("apc_burst", "APC Burst", 36, 4, 20, 768, ProjectileKind.Bullet, true, false, false, true)));
            actors.Add(Unit("attack_aircraft", "Attack Aircraft", 450, ProductionKind.Aircraft, 800, 45, "dual_helipad", 192, 30, "aircraft_attack", unitAnimation, Weapon("aircraft_rocket", "Aircraft Rocket", 85, 6, 36, 448, ProjectileKind.Rocket, true, true, true, true)));
            actors.Add(Unit("heavy_lifter_aircraft", "Heavy Lifter Aircraft", 650, ProductionKind.Aircraft, 900, 50, "dual_helipad", 160, 20, "aircraft_lifter", unitAnimation, null));

            return new RtsRules(actors);
        }

        static UnitDefinition Unit(string typeId, string displayName, int health, ProductionKind productionKind, int cost, int buildTimeTicks, string factoryTypeId, int speedPerTick, int turnRate, string visualProfile, AnimationStateDefinition animation, WeaponDefinition weapon, int sightRadius = 4)
        {
            return new UnitDefinition(
                typeId,
                displayName,
                health,
                new ProductionDefinition(productionKind, cost, buildTimeTicks, factoryTypeId, false),
                new MovementDefinition(speedPerTick, turnRate, visualProfile, MovementClassFor(productionKind, visualProfile, typeId)),
                weapon,
                animation,
                new SightDefinition(sightRadius));
        }

        static MovementClass MovementClassFor(ProductionKind productionKind, string visualProfile, string typeId)
        {
            if (productionKind == ProductionKind.Infantry)
                return MovementClass.Infantry;
            if (productionKind == ProductionKind.Aircraft)
                return MovementClass.Aircraft;
            if (typeId == "harvester")
                return MovementClass.Harvester;
            if (visualProfile != null && visualProfile.StartsWith("tracked", System.StringComparison.Ordinal))
                return MovementClass.Tracked;
            return MovementClass.Wheeled;
        }

        static BuildingDefinition Building(string typeId, string displayName, int health, ProductionKind productionKind, int cost, int buildTimeTicks, string factoryTypeId, int powerGenerated, int powerConsumed, Int2 footprint, bool providesConstructionRadius, int constructionRadius, Int2 unitExitOffset, IReadOnlyList<string> produces, AnimationStateDefinition animation, WeaponDefinition weapon = null, int sightRadius = 5, RadarDefinition radar = null)
        {
            return new BuildingDefinition(
                typeId,
                displayName,
                health,
                new ProductionDefinition(productionKind, cost, buildTimeTicks, factoryTypeId, false),
                new PowerDefinition(powerGenerated, powerConsumed, providesConstructionRadius),
                animation,
                footprint,
                providesConstructionRadius,
                constructionRadius,
                unitExitOffset,
                produces,
                weapon,
                new SightDefinition(sightRadius),
                radar);
        }

        static WeaponDefinition Weapon(string weaponId, string displayName, int damage, int rangeCells, int cooldownTicks, int projectileSpeedSubCellsPerTick, ProjectileKind projectileKind, bool canTargetGround, bool canTargetAir, bool canTargetBuildings, bool canTargetUnits)
        {
            return new WeaponDefinition(
                weaponId,
                displayName,
                damage,
                rangeCells,
                0,
                cooldownTicks,
                projectileSpeedSubCellsPerTick,
                projectileKind,
                canTargetGround,
                canTargetAir,
                canTargetBuildings,
                canTargetUnits,
                false,
                1,
                0,
                0,
                "MuzzlePrimary",
                projectileKind == ProjectileKind.Rocket ? "explosion_placeholder" : "impact_placeholder",
                weaponId);
        }
    }
}
