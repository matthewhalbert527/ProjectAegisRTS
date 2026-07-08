using System.Collections.Generic;
using ProjectAegisRTS.Data;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class VisualMotionProfileLibrary : MonoBehaviour
    {
        public List<VisualMotionProfile> profiles = new List<VisualMotionProfile>();
        public bool includeGeneratedDefaults = true;

        readonly Dictionary<string, VisualMotionProfile> byId = new Dictionary<string, VisualMotionProfile>();
        readonly Dictionary<VisualMotionCategory, VisualMotionProfile> byCategory = new Dictionary<VisualMotionCategory, VisualMotionProfile>();
        bool initialized;

        public int ProfileCount
        {
            get
            {
                EnsureInitialized();
                return byId.Count;
            }
        }

        public VisualMotionProfile GetProfile(string actorTypeId, VisualMotionCategory category, string snapshotProfileId)
        {
            EnsureInitialized();

            VisualMotionProfile profile;
            if (!string.IsNullOrEmpty(snapshotProfileId) && byId.TryGetValue(snapshotProfileId, out profile))
                return profile;
            if (!string.IsNullOrEmpty(actorTypeId) && byId.TryGetValue(actorTypeId, out profile))
                return profile;
            if (byCategory.TryGetValue(category, out profile))
                return profile;
            if (byCategory.TryGetValue(VisualMotionCategory.Unknown, out profile))
                return profile;

            return VisualMotionProfile.CreateRuntimeDefault("runtime_default", VisualMotionCategory.Unknown);
        }

        public VisualMotionProfile GetProfile(string actorTypeId, ActorDefinition definition, string snapshotProfileId)
        {
            return GetProfile(actorTypeId, CategoryForActor(actorTypeId, definition), snapshotProfileId);
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            byId.Clear();
            byCategory.Clear();

            if (profiles != null)
                for (var i = 0; i < profiles.Count; i++)
                    Register(profiles[i]);

            if (includeGeneratedDefaults)
                RegisterGeneratedDefaults();
        }

        public static VisualMotionCategory CategoryForActor(string typeId, ActorDefinition definition)
        {
            if (!string.IsNullOrEmpty(typeId))
            {
                if (typeId.Contains("aircraft") || typeId.Contains("sky") || typeId.Contains("lifter"))
                    return VisualMotionCategory.Aircraft;
                if (typeId.Contains("infantry") || typeId == "engineer")
                    return VisualMotionCategory.Infantry;
                if (typeId.Contains("tower") || typeId.Contains("turret"))
                    return VisualMotionCategory.Defense;
                if (typeId.Contains("harvester"))
                    return VisualMotionCategory.Harvester;
                if (typeId.Contains("apc") || typeId.Contains("tank") || typeId.Contains("rover"))
                    return VisualMotionCategory.Vehicle;
            }

            if (definition is BuildingDefinition)
                return VisualMotionCategory.Building;
            if (definition is UnitDefinition)
                return VisualMotionCategory.Vehicle;

            return VisualMotionCategory.Unknown;
        }

        void RegisterGeneratedDefaults()
        {
            RegisterRuntimeDefault("default_infantry", VisualMotionCategory.Infantry);
            RegisterRuntimeDefault("default_vehicle", VisualMotionCategory.Vehicle);
            RegisterRuntimeDefault("light_tank", VisualMotionCategory.Vehicle);
            RegisterRuntimeDefault("medium_tank", VisualMotionCategory.Vehicle);
            RegisterRuntimeDefault("heavy_tank", VisualMotionCategory.Vehicle);
            RegisterRuntimeDefault("harvester", VisualMotionCategory.Harvester);
            RegisterRuntimeDefault("scout_rover", VisualMotionCategory.Vehicle);
            RegisterRuntimeDefault("apc", VisualMotionCategory.Vehicle);
            RegisterRuntimeDefault("default_aircraft", VisualMotionCategory.Aircraft);
            RegisterRuntimeDefault("attack_aircraft", VisualMotionCategory.Aircraft);
            RegisterRuntimeDefault("heavy_lifter_aircraft", VisualMotionCategory.Aircraft);
            RegisterRuntimeDefault("default_building", VisualMotionCategory.Building);
            RegisterRuntimeDefault("default_defense", VisualMotionCategory.Defense);
            RegisterRuntimeDefault("default_unknown", VisualMotionCategory.Unknown);
        }

        void RegisterRuntimeDefault(string id, VisualMotionCategory category)
        {
            if (byId.ContainsKey(id))
                return;

            Register(VisualMotionProfile.CreateRuntimeDefault(id, category));
        }

        void Register(VisualMotionProfile profile)
        {
            if (profile == null)
                return;

            if (!string.IsNullOrEmpty(profile.profileId) && !byId.ContainsKey(profile.profileId))
                byId.Add(profile.profileId, profile);
            if (!byCategory.ContainsKey(profile.category))
                byCategory.Add(profile.category, profile);
        }
    }
}
