using System.Collections.Generic;
using ProjectAegisRTS.Data;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingVisualProfileLibrary : MonoBehaviour
    {
        public List<BuildingVisualProfile> profiles = new List<BuildingVisualProfile>();
        public bool includeGeneratedDefaults = true;

        readonly Dictionary<string, BuildingVisualProfile> byTypeId = new Dictionary<string, BuildingVisualProfile>();
        readonly Dictionary<string, BuildingVisualProfile> byProfileId = new Dictionary<string, BuildingVisualProfile>();
        readonly Dictionary<BuildingVisualCategory, BuildingVisualProfile> byCategory = new Dictionary<BuildingVisualCategory, BuildingVisualProfile>();
        bool initialized;

        public int ProfileCount
        {
            get
            {
                EnsureInitialized();
                return byProfileId.Count;
            }
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            byTypeId.Clear();
            byProfileId.Clear();
            byCategory.Clear();

            if (profiles != null)
                for (var i = 0; i < profiles.Count; i++)
                    Register(profiles[i]);

            if (includeGeneratedDefaults)
                RegisterGeneratedDefaults();
        }

        public BuildingVisualProfile GetProfile(string actorTypeId, BuildingDefinition definition)
        {
            EnsureInitialized();

            BuildingVisualProfile profile;
            if (!string.IsNullOrEmpty(actorTypeId) && byTypeId.TryGetValue(actorTypeId, out profile))
                return profile;

            var category = CategoryForActor(actorTypeId);
            if (byCategory.TryGetValue(category, out profile))
                return profile;
            if (byTypeId.TryGetValue("default_building", out profile))
                return profile;

            var footprint = definition == null ? new ProjectAegisRTS.Core.Int2(2, 2) : definition.FootprintCells;
            return BuildingVisualProfile.CreateRuntimeDefault(string.IsNullOrEmpty(actorTypeId) ? "default_building" : actorTypeId, category);
        }

        public BuildingVisualProfile GetProfileById(string profileId)
        {
            EnsureInitialized();

            BuildingVisualProfile profile;
            return !string.IsNullOrEmpty(profileId) && byProfileId.TryGetValue(profileId, out profile) ? profile : null;
        }

        public static BuildingVisualCategory CategoryForActor(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                return BuildingVisualCategory.Unknown;
            if (typeId == "fabrication_hub")
                return BuildingVisualCategory.Construction;
            if (typeId.Contains("power_plant"))
                return BuildingVisualCategory.Power;
            if (typeId == "barracks" || typeId == "war_factory")
                return BuildingVisualCategory.Production;
            if (typeId == "refinery")
                return BuildingVisualCategory.Refinery;
            if (typeId.Contains("tower") || typeId.Contains("turret"))
                return BuildingVisualCategory.Defense;
            if (typeId == "comm_center")
                return BuildingVisualCategory.Tech;
            if (typeId == "repair_bay")
                return BuildingVisualCategory.Repair;
            if (typeId == "dual_helipad")
                return BuildingVisualCategory.Airfield;
            if (typeId == "field_hospital")
                return BuildingVisualCategory.Medical;
            if (typeId == "tech_center")
                return BuildingVisualCategory.Tech;
            return BuildingVisualCategory.Support;
        }

        void RegisterGeneratedDefaults()
        {
            RegisterRuntime("fabrication_hub", 3, 3);
            RegisterRuntime("power_plant", 2, 2);
            RegisterRuntime("advanced_power_plant", 2, 2);
            RegisterRuntime("barracks", 2, 2);
            RegisterRuntime("war_factory", 3, 2);
            RegisterRuntime("refinery", 3, 3);
            RegisterRuntime("gun_tower", 1, 1);
            RegisterRuntime("cannon_turret", 1, 1);
            RegisterRuntime("advanced_gun_tower", 1, 1);
            RegisterRuntime("comm_center", 2, 2);
            RegisterRuntime("repair_bay", 3, 2);
            RegisterRuntime("tech_center", 2, 2);
            RegisterRuntime("field_hospital", 2, 2);
            RegisterRuntime("dual_helipad", 3, 2);
            RegisterRuntime("default_building", 2, 2);
            RegisterRuntime("default_defense", 1, 1);
        }

        void RegisterRuntime(string actorTypeId, int width, int height)
        {
            if (byTypeId.ContainsKey(actorTypeId))
                return;

            var profile = BuildingVisualProfile.CreateRuntimeDefault(actorTypeId, CategoryForActor(actorTypeId));
            profile.ConfigureDefault(actorTypeId + "_building_visual", actorTypeId, CategoryForActor(actorTypeId), width, height);
            Register(profile);
        }

        void Register(BuildingVisualProfile profile)
        {
            if (profile == null)
                return;

            if (!string.IsNullOrEmpty(profile.profileId) && !byProfileId.ContainsKey(profile.profileId))
                byProfileId.Add(profile.profileId, profile);
            if (!string.IsNullOrEmpty(profile.actorTypeId) && !byTypeId.ContainsKey(profile.actorTypeId))
                byTypeId.Add(profile.actorTypeId, profile);
            if (!byCategory.ContainsKey(profile.category))
                byCategory.Add(profile.category, profile);
        }
    }
}
