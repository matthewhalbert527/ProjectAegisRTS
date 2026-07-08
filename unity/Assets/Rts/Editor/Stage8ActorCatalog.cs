using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8ActorCatalog
    {
        public const int ExpectedActorCount = 27;
        public const string ConceptFolder = "Assets/Rts/Art/Concepts";
        public const string IconFolder = "Assets/Rts/Art/Icons";
        public const string MaterialFolder = "Assets/Rts/Art/Materials";
        public const string GeneratedBlockoutFolder = "Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts";
        public const string DefinitionFolder = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions";
        public const string ConceptReferenceFolder = "Assets/Rts/ScriptableObjects/Art/ConceptReferences";
        public const string ValidationFolder = "Assets/Rts/Art/Validation";
        public const string ValidationJsonPath = ValidationFolder + "/stage8_prefab_validation.json";
        public const string ValidationMarkdownPath = "docs/STAGE8_PREFAB_VALIDATION.md";

        static readonly ActorSeed[] Seeds =
        {
            new ActorSeed("light_tank", ActorArtCategory.Vehicle, 1, 1),
            new ActorSeed("medium_tank", ActorArtCategory.Vehicle, 1, 1),
            new ActorSeed("heavy_tank", ActorArtCategory.Vehicle, 1, 1),
            new ActorSeed("rifle_infantry", ActorArtCategory.Infantry, 1, 1),
            new ActorSeed("grenade_infantry", ActorArtCategory.Infantry, 1, 1),
            new ActorSeed("rocket_infantry", ActorArtCategory.Infantry, 1, 1),
            new ActorSeed("flame_infantry", ActorArtCategory.Infantry, 1, 1),
            new ActorSeed("engineer", ActorArtCategory.Infantry, 1, 1),
            new ActorSeed("fabrication_hub", ActorArtCategory.Building, 3, 3),
            new ActorSeed("field_hospital", ActorArtCategory.Support, 2, 2),
            new ActorSeed("barracks", ActorArtCategory.Building, 2, 2),
            new ActorSeed("war_factory", ActorArtCategory.Building, 3, 2),
            new ActorSeed("refinery", ActorArtCategory.Building, 3, 3),
            new ActorSeed("harvester", ActorArtCategory.Vehicle, 1, 1),
            new ActorSeed("power_plant", ActorArtCategory.Building, 2, 2),
            new ActorSeed("advanced_power_plant", ActorArtCategory.Building, 2, 2),
            new ActorSeed("comm_center", ActorArtCategory.Support, 2, 2),
            new ActorSeed("repair_bay", ActorArtCategory.Support, 3, 2),
            new ActorSeed("tech_center", ActorArtCategory.Support, 2, 2),
            new ActorSeed("cannon_turret", ActorArtCategory.Defense, 1, 1),
            new ActorSeed("gun_tower", ActorArtCategory.Defense, 1, 1),
            new ActorSeed("advanced_gun_tower", ActorArtCategory.Defense, 1, 1),
            new ActorSeed("scout_rover", ActorArtCategory.Vehicle, 1, 1),
            new ActorSeed("apc", ActorArtCategory.Vehicle, 1, 1),
            new ActorSeed("attack_aircraft", ActorArtCategory.Aircraft, 1, 1),
            new ActorSeed("heavy_lifter_aircraft", ActorArtCategory.Aircraft, 1, 1),
            new ActorSeed("dual_helipad", ActorArtCategory.Support, 3, 2)
        };

        public static string RepoRoot
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..")); }
        }

        public static List<Stage8ActorSpec> LoadSpecs()
        {
            var specs = new List<Stage8ActorSpec>();
            var byId = new Dictionary<string, Stage8ActorSpec>();
            for (var i = 0; i < Seeds.Length; i++)
            {
                var seed = Seeds[i];
                var spec = new Stage8ActorSpec(seed.ActorTypeId, seed.Category, seed.Width, seed.Height);
                specs.Add(spec);
                byId.Add(spec.ActorTypeId, spec);
            }

            var registryPath = Path.Combine(RepoRoot, "art", "concepts", "registry", "assets.json");
            if (File.Exists(registryPath))
            {
                var json = File.ReadAllText(registryPath);
                var wrapper = JsonUtility.FromJson<RegistryWrapper>("{\"items\":" + json + "}");
                if (wrapper != null && wrapper.items != null)
                    for (var i = 0; i < wrapper.items.Length; i++)
                        ApplyRegistryEntry(byId, wrapper.items[i]);
            }

            return specs;
        }

        public static void EnsureStage8Folders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "Art");
            EnsureFolder("Assets/Rts/Art", "Concepts");
            EnsureFolder("Assets/Rts/Art", "Icons");
            EnsureFolder("Assets/Rts/Art", "Materials");
            EnsureFolder("Assets/Rts/Art", "Models");
            EnsureFolder("Assets/Rts/Art/Models", "Source");
            EnsureFolder("Assets/Rts/Art/Models", "Imported");
            EnsureFolder("Assets/Rts/Art", "Textures");
            EnsureFolder("Assets/Rts/Art", "VFX");
            EnsureFolder("Assets/Rts/Art", "Audio");
            EnsureFolder("Assets/Rts/Art", "Prefabs");
            EnsureFolder("Assets/Rts/Art/Prefabs", "Actors");
            EnsureFolder("Assets/Rts/Art/Prefabs/Actors", "GeneratedBlockouts");
            EnsureFolder("Assets/Rts/Art/Prefabs/Actors", "Production");
            EnsureFolder("Assets/Rts/Art/Prefabs", "Buildings");
            EnsureFolder("Assets/Rts/Art/Prefabs", "Vehicles");
            EnsureFolder("Assets/Rts/Art/Prefabs", "Infantry");
            EnsureFolder("Assets/Rts/Art/Prefabs", "Aircraft");
            EnsureFolder("Assets/Rts/Art/Prefabs", "Defenses");
            EnsureFolder("Assets/Rts/Art/Prefabs", "Support");
            EnsureFolder("Assets/Rts/Art", "Validation");
            EnsureFolder("Assets/Rts", "ScriptableObjects");
            EnsureFolder("Assets/Rts/ScriptableObjects", "Art");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "ActorVisualDefinitions");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "ConceptReferences");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "PrefabValidation");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "ProductionSpecs");
        }

        public static string ConceptAssetPath(Stage8ActorSpec spec)
        {
            return ConceptFolder + "/" + spec.ActorTypeId + ".png";
        }

        public static string ConceptReferenceAssetPath(Stage8ActorSpec spec)
        {
            return ConceptReferenceFolder + "/" + spec.ActorTypeId + "_concept.asset";
        }

        public static string DefinitionAssetPath(Stage8ActorSpec spec)
        {
            return DefinitionFolder + "/" + spec.ActorTypeId + "_visual.asset";
        }

        public static string BlockoutPrefabPath(Stage8ActorSpec spec)
        {
            return GeneratedBlockoutFolder + "/" + spec.ActorTypeId + "_blockout.prefab";
        }

        public static string IconAssetPath(Stage8ActorSpec spec)
        {
            return IconFolder + "/" + spec.ActorTypeId + "_icon.png";
        }

        public static string SourceConceptAbsolutePath(Stage8ActorSpec spec)
        {
            return Path.Combine(RepoRoot, "art", "concepts", "source", spec.SourceFile ?? string.Empty);
        }

        public static Sprite LoadSpriteAtPath(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                return sprite;

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (var i = 0; i < assets.Length; i++)
            {
                sprite = assets[i] as Sprite;
                if (sprite != null)
                    return sprite;
            }

            return null;
        }

        public static List<ActorPrefabSocketKind> RequiredSocketsFor(Stage8ActorSpec spec)
        {
            var set = new HashSet<ActorPrefabSocketKind>();
            Add(set, ActorPrefabSocketKind.Root);

            if (spec.Category == ActorArtCategory.Infantry)
            {
                Add(set, ActorPrefabSocketKind.BodyRoot, ActorPrefabSocketKind.VisualRoot, ActorPrefabSocketKind.Head, ActorPrefabSocketKind.WeaponSocket, ActorPrefabSocketKind.AimPivot, ActorPrefabSocketKind.SelectionAnchor, ActorPrefabSocketKind.HealthBarAnchor);
                return SortedSockets(set);
            }

            if (spec.Category == ActorArtCategory.Aircraft)
            {
                Add(set, ActorPrefabSocketKind.BodyRoot, ActorPrefabSocketKind.VisualRoot, ActorPrefabSocketKind.AircraftRotor, ActorPrefabSocketKind.SelectionAnchor, ActorPrefabSocketKind.HealthBarAnchor, ActorPrefabSocketKind.LandingPadAnchor, ActorPrefabSocketKind.VfxSmoke, ActorPrefabSocketKind.VfxExplosion);
                if (spec.ActorTypeId == "attack_aircraft")
                    Add(set, ActorPrefabSocketKind.MuzzlePrimary);
                return SortedSockets(set);
            }

            if (spec.Category == ActorArtCategory.Vehicle || spec.Category == ActorArtCategory.Resource)
            {
                Add(set, ActorPrefabSocketKind.BodyRoot, ActorPrefabSocketKind.VisualRoot, ActorPrefabSocketKind.SelectionAnchor, ActorPrefabSocketKind.HealthBarAnchor, ActorPrefabSocketKind.VfxSmoke, ActorPrefabSocketKind.VfxExplosion);
                if (spec.ActorTypeId == "scout_rover" || spec.ActorTypeId == "apc")
                    Add(set, ActorPrefabSocketKind.WheelLeft, ActorPrefabSocketKind.WheelRight);
                else
                    Add(set, ActorPrefabSocketKind.TrackLeft, ActorPrefabSocketKind.TrackRight);
                if (spec.ActorTypeId.Contains("tank") || spec.ActorTypeId == "apc")
                    Add(set, ActorPrefabSocketKind.TurretRoot, ActorPrefabSocketKind.BarrelRoot, ActorPrefabSocketKind.MuzzlePrimary);
                if (spec.ActorTypeId == "harvester")
                    Add(set, ActorPrefabSocketKind.HarvesterDock);
                return SortedSockets(set);
            }

            if (spec.Category == ActorArtCategory.Defense)
            {
                Add(set, ActorPrefabSocketKind.BodyRoot, ActorPrefabSocketKind.VisualRoot, ActorPrefabSocketKind.TurretRoot, ActorPrefabSocketKind.BarrelRoot, ActorPrefabSocketKind.MuzzlePrimary, ActorPrefabSocketKind.SelectionAnchor, ActorPrefabSocketKind.HealthBarAnchor, ActorPrefabSocketKind.LightRoot, ActorPrefabSocketKind.VfxSmoke, ActorPrefabSocketKind.VfxExplosion);
                return SortedSockets(set);
            }

            Add(set, ActorPrefabSocketKind.BodyRoot, ActorPrefabSocketKind.VisualRoot, ActorPrefabSocketKind.SelectionAnchor, ActorPrefabSocketKind.HealthBarAnchor, ActorPrefabSocketKind.UiAnchor, ActorPrefabSocketKind.LightRoot, ActorPrefabSocketKind.VfxSmoke, ActorPrefabSocketKind.VfxExplosion);
            if (IsProductionBuilding(spec))
                Add(set, ActorPrefabSocketKind.DoorRoot, ActorPrefabSocketKind.ProductionExit, ActorPrefabSocketKind.RallyExit, ActorPrefabSocketKind.VfxProduction);
            if (spec.ActorTypeId.Contains("power_plant"))
                Add(set, ActorPrefabSocketKind.TurbineRoot, ActorPrefabSocketKind.LightRoot);
            if (spec.ActorTypeId == "refinery")
                Add(set, ActorPrefabSocketKind.HarvesterDock, ActorPrefabSocketKind.DoorRoot, ActorPrefabSocketKind.ProductionExit);
            if (spec.ActorTypeId == "repair_bay")
                Add(set, ActorPrefabSocketKind.RepairArmRoot, ActorPrefabSocketKind.DoorRoot);
            if (spec.ActorTypeId == "fabrication_hub")
                Add(set, ActorPrefabSocketKind.CraneRoot, ActorPrefabSocketKind.ProductionExit, ActorPrefabSocketKind.RallyExit);
            if (spec.ActorTypeId == "comm_center" || spec.ActorTypeId == "tech_center")
                Add(set, ActorPrefabSocketKind.RadarDishRoot, ActorPrefabSocketKind.LightRoot);
            return SortedSockets(set);
        }

        public static bool IsProductionBuilding(Stage8ActorSpec spec)
        {
            return spec.ActorTypeId == "fabrication_hub" || spec.ActorTypeId == "barracks" || spec.ActorTypeId == "war_factory" || spec.ActorTypeId == "dual_helipad";
        }

        public static string MotionProfileIdFor(Stage8ActorSpec spec)
        {
            switch (spec.ActorTypeId)
            {
                case "light_tank": return "tracked_light";
                case "medium_tank": return "tracked_medium";
                case "heavy_tank": return "tracked_heavy";
                case "grenade_infantry":
                case "flame_infantry": return "infantry_assault";
                case "rocket_infantry": return "infantry_heavy";
                case "engineer": return "infantry_utility";
                case "rifle_infantry": return "infantry_basic";
                case "scout_rover": return "wheeled_scout";
                case "apc": return "wheeled_apc";
                case "harvester": return "wheeled_heavy";
                case "attack_aircraft": return "aircraft_attack";
                case "heavy_lifter_aircraft": return "aircraft_lifter";
                default:
                    if (spec.Category == ActorArtCategory.Defense)
                        return "default_defense";
                    if (spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support)
                        return "building_static";
                    return "default_unknown";
            }
        }

        public static string BuildingProfileIdFor(Stage8ActorSpec spec)
        {
            return spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support || spec.Category == ActorArtCategory.Defense ? spec.ActorTypeId + "_building_visual" : string.Empty;
        }

        public static string ToDisplayName(string actorTypeId)
        {
            if (string.IsNullOrEmpty(actorTypeId))
                return string.Empty;

            var parts = actorTypeId.Split('_');
            for (var i = 0; i < parts.Length; i++)
                if (parts[i].Length > 0)
                    parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            return string.Join(" ", parts);
        }

        static void ApplyRegistryEntry(Dictionary<string, Stage8ActorSpec> byId, RegistryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.safe_working_id))
                return;

            Stage8ActorSpec spec;
            if (!byId.TryGetValue(entry.safe_working_id, out spec))
                return;

            spec.OriginalLabel = entry.original_label;
            spec.SafeDisplayName = ToDisplayName(spec.ActorTypeId);
            spec.SourceFile = entry.source_file;
            spec.IntendedRole = entry.intended_role;
            spec.ArtNotes = entry.art_notes;
            spec.AnimationNotes = entry.animation_notes;
            spec.IpReviewRequired = entry.ip_review_required;
            spec.ProductionCost = entry.production_cost_placeholder;
            spec.BuildTimeTicks = entry.build_time_ticks_placeholder;
            spec.PowerGenerated = entry.power_generated;
            spec.PowerConsumed = entry.power_consumed;
            spec.Category = ParseCategory(entry.category, spec.Category);
            if (entry.footprint_cells != null && entry.footprint_cells.x > 0 && entry.footprint_cells.y > 0)
            {
                spec.FootprintWidth = entry.footprint_cells.x;
                spec.FootprintHeight = entry.footprint_cells.y;
            }
        }

        static ActorArtCategory ParseCategory(string category, ActorArtCategory fallback)
        {
            switch (category)
            {
                case "building": return ActorArtCategory.Building;
                case "defense": return ActorArtCategory.Defense;
                case "infantry": return ActorArtCategory.Infantry;
                case "vehicle": return ActorArtCategory.Vehicle;
                case "aircraft": return ActorArtCategory.Aircraft;
                case "support": return ActorArtCategory.Support;
                case "resource": return ActorArtCategory.Resource;
                default: return fallback;
            }
        }

        static void Add(HashSet<ActorPrefabSocketKind> set, params ActorPrefabSocketKind[] sockets)
        {
            for (var i = 0; i < sockets.Length; i++)
                set.Add(sockets[i]);
        }

        static List<ActorPrefabSocketKind> SortedSockets(HashSet<ActorPrefabSocketKind> set)
        {
            var list = new List<ActorPrefabSocketKind>(set);
            list.Sort();
            return list;
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        struct ActorSeed
        {
            public readonly string ActorTypeId;
            public readonly ActorArtCategory Category;
            public readonly int Width;
            public readonly int Height;

            public ActorSeed(string actorTypeId, ActorArtCategory category, int width, int height)
            {
                ActorTypeId = actorTypeId;
                Category = category;
                Width = width;
                Height = height;
            }
        }

        [Serializable]
        sealed class RegistryWrapper
        {
            public RegistryEntry[] items;
        }

        [Serializable]
        sealed class RegistryEntry
        {
            public string source_file;
            public string art_notes;
            public int production_cost_placeholder;
            public Footprint footprint_cells;
            public string category;
            public int power_generated;
            public int power_consumed;
            public string animation_notes;
            public string intended_role;
            public string safe_working_id;
            public bool ip_review_required;
            public string original_label;
            public int build_time_ticks_placeholder;
        }

        [Serializable]
        sealed class Footprint
        {
            public int x;
            public int y;
        }
    }

    public sealed class Stage8ActorSpec
    {
        public string ActorTypeId;
        public string OriginalLabel;
        public string SafeDisplayName;
        public ActorArtCategory Category;
        public string SourceFile;
        public string IntendedRole;
        public string ArtNotes;
        public string AnimationNotes;
        public bool IpReviewRequired;
        public int FootprintWidth;
        public int FootprintHeight;
        public int ProductionCost;
        public int BuildTimeTicks;
        public int PowerGenerated;
        public int PowerConsumed;

        public Stage8ActorSpec(string actorTypeId, ActorArtCategory category, int width, int height)
        {
            ActorTypeId = actorTypeId;
            OriginalLabel = Stage8ActorCatalog.ToDisplayName(actorTypeId);
            SafeDisplayName = Stage8ActorCatalog.ToDisplayName(actorTypeId);
            Category = category;
            FootprintWidth = width;
            FootprintHeight = height;
        }
    }
}
