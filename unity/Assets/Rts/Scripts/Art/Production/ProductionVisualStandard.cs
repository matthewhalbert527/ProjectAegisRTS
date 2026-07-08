using System;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    [Serializable]
    public sealed class ProductionVisualStandard
    {
        public string actorTypeId;
        public string displayName;
        public ProductionVisualTier requiredTier = ProductionVisualTier.FirstPassProxy;
        public ProductionVisualViewCoverage requiredViewCoverage = ProductionVisualViewCoverage.AllAround;
        public bool requiresGridAccurateBase = true;
        public bool requiresTopDownSilhouette = true;
        public bool requiresSideDetail = true;
        public bool requiresRearDetail = true;
        public bool requiresRoofDetail = true;
        public bool requiresBeveledOrTieredForm = true;
        public bool requiresAnimationSockets = true;
        public bool requiresLodGroup = true;
        public int maxRecommendedMeshObjects = 36;
        public int maxRecommendedMaterials = 8;
        [TextArea(2, 5)] public string notes;

        public static ProductionVisualStandard CreateDefault(string actorTypeId, string displayName, bool isBuilding)
        {
            return new ProductionVisualStandard
            {
                actorTypeId = actorTypeId,
                displayName = displayName,
                requiredTier = ProductionVisualTier.FirstPassProxy,
                requiredViewCoverage = ProductionVisualViewCoverage.AllAround,
                requiresGridAccurateBase = true,
                requiresTopDownSilhouette = true,
                requiresSideDetail = true,
                requiresRearDetail = isBuilding,
                requiresRoofDetail = true,
                requiresBeveledOrTieredForm = isBuilding,
                requiresAnimationSockets = true,
                requiresLodGroup = true,
                maxRecommendedMeshObjects = isBuilding ? 48 : 28,
                maxRecommendedMaterials = isBuilding ? 8 : 6,
                notes = isBuilding
                    ? "Stage 20 first-pass proxy must read from top, front, sides, rear, and roof without leaving the gameplay footprint."
                    : "Stage 20 first-pass proxy must be readable at tabletop scale and keep motion/combat sockets intact."
            };
        }
    }
}
