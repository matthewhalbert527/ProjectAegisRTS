using System.Collections.Generic;
using ProjectAegisRTS.Data;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public enum DesktopProductionCategory
    {
        Buildings,
        Defenses,
        Infantry,
        Vehicles,
        Aircraft,
        Support
    }

    public enum DesktopCommandMode
    {
        Normal,
        Move,
        AttackPlaceholder,
        AttackMove,
        Patrol,
        Rally,
        Capture,
        EngineerRepair,
        LoadTransport,
        UnloadTransport
    }

    public static class DesktopProductionCatalog
    {
        public static readonly string[] ActiveMvpTypeIds =
        {
            "power_plant",
            "barracks",
            "war_factory",
            "refinery",
            "gun_tower",
            "rifle_infantry",
            "light_tank",
            "harvester"
        };

        public static readonly string[] FutureTypeIds =
        {
            "medium_tank",
            "heavy_tank",
            "rocket_infantry",
            "engineer",
            "repair_bay",
            "tech_center",
            "comm_center",
            "advanced_power_plant",
            "advanced_gun_tower",
            "apc",
            "scout_rover",
            "attack_aircraft",
            "heavy_lifter_aircraft",
            "dual_helipad"
        };

        public static bool IsActiveMvp(string typeId)
        {
            return Contains(ActiveMvpTypeIds, typeId);
        }

        public static bool IsFuturePlaceholder(string typeId)
        {
            return Contains(FutureTypeIds, typeId);
        }

        public static DesktopProductionCategory GetCategory(ActorDefinition definition)
        {
            if (definition.TypeId.Contains("tower") || definition.TypeId.Contains("turret"))
                return DesktopProductionCategory.Defenses;

            if (definition.TypeId.Contains("repair") || definition.TypeId.Contains("comm") || definition.TypeId.Contains("tech"))
                return DesktopProductionCategory.Support;

            switch (definition.Production.Kind)
            {
                case ProductionKind.Infantry:
                    return DesktopProductionCategory.Infantry;
                case ProductionKind.Vehicle:
                    return DesktopProductionCategory.Vehicles;
                case ProductionKind.Aircraft:
                    return DesktopProductionCategory.Aircraft;
                case ProductionKind.Support:
                    return DesktopProductionCategory.Support;
                default:
                    return DesktopProductionCategory.Buildings;
            }
        }

        public static List<string> GetOrderedTypeIds(DesktopProductionCategory category, IReadOnlyDictionary<string, ActorDefinition> definitions)
        {
            var result = new List<string>();
            AddMatching(result, ActiveMvpTypeIds, category, definitions);
            AddMatching(result, FutureTypeIds, category, definitions);
            return result;
        }

        static void AddMatching(List<string> result, string[] typeIds, DesktopProductionCategory category, IReadOnlyDictionary<string, ActorDefinition> definitions)
        {
            for (var i = 0; i < typeIds.Length; i++)
            {
                ActorDefinition definition;
                if (definitions.TryGetValue(typeIds[i], out definition) && GetCategory(definition) == category && !result.Contains(typeIds[i]))
                    result.Add(typeIds[i]);
            }
        }

        static bool Contains(string[] values, string value)
        {
            for (var i = 0; i < values.Length; i++)
                if (values[i] == value)
                    return true;

            return false;
        }
    }
}
