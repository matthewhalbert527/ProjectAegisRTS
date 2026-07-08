using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class ProductionVisualStandardLibrary : MonoBehaviour
    {
        public List<ProductionVisualStandard> standards = new List<ProductionVisualStandard>();
        public ProductionVisualStandard fallbackStandard = ProductionVisualStandard.CreateDefault("fallback", "Fallback", true);

        readonly Dictionary<string, ProductionVisualStandard> byActorTypeId = new Dictionary<string, ProductionVisualStandard>();
        bool initialized;

        public int StandardCount
        {
            get
            {
                EnsureInitialized();
                return byActorTypeId.Count;
            }
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            EnsureDefaults();
            byActorTypeId.Clear();
            for (var i = 0; i < standards.Count; i++)
            {
                var standard = standards[i];
                if (standard != null && !string.IsNullOrEmpty(standard.actorTypeId) && !byActorTypeId.ContainsKey(standard.actorTypeId))
                    byActorTypeId.Add(standard.actorTypeId, standard);
            }
        }

        public void RebuildLookup()
        {
            initialized = false;
            EnsureInitialized();
        }

        public ProductionVisualStandard GetStandard(string actorTypeId)
        {
            EnsureInitialized();
            ProductionVisualStandard standard;
            return !string.IsNullOrEmpty(actorTypeId) && byActorTypeId.TryGetValue(actorTypeId, out standard) ? standard : fallbackStandard;
        }

        public List<ProductionVisualStandard> GetAllStandards()
        {
            EnsureInitialized();
            return standards;
        }

        public void EnsureDefaults()
        {
            if (standards == null)
                standards = new List<ProductionVisualStandard>();

            for (var i = 0; i < Stage20MvpVisualActorSet.ActorTypeIds.Length; i++)
            {
                var actorTypeId = Stage20MvpVisualActorSet.ActorTypeIds[i];
                if (ContainsStandard(actorTypeId))
                    continue;

                var isBuilding = IsBuildingOrDefense(actorTypeId);
                standards.Add(ProductionVisualStandard.CreateDefault(actorTypeId, Stage8DisplayName(actorTypeId), isBuilding));
            }
        }

        bool ContainsStandard(string actorTypeId)
        {
            for (var i = 0; i < standards.Count; i++)
                if (standards[i] != null && standards[i].actorTypeId == actorTypeId)
                    return true;
            return false;
        }

        static bool IsBuildingOrDefense(string actorTypeId)
        {
            return actorTypeId == "fabrication_hub" ||
                actorTypeId == "power_plant" ||
                actorTypeId == "refinery" ||
                actorTypeId == "barracks" ||
                actorTypeId == "war_factory" ||
                actorTypeId == "gun_tower";
        }

        static string Stage8DisplayName(string actorTypeId)
        {
            if (string.IsNullOrEmpty(actorTypeId))
                return string.Empty;

            var parts = actorTypeId.Split('_');
            for (var i = 0; i < parts.Length; i++)
                if (parts[i].Length > 0)
                    parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            return string.Join(" ", parts);
        }
    }
}
