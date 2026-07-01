using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class ProductionVisualValidationTag : MonoBehaviour
    {
        public string actorTypeId;
        public ProductionVisualTier visualTier = ProductionVisualTier.FirstPassProxy;
        public bool hasTopDetail;
        public bool hasFrontDetail;
        public bool hasBackDetail;
        public bool hasLeftDetail;
        public bool hasRightDetail;
        public bool hasRoofDetail;
        public bool hasBeveledOrTieredForm;
        public bool hasGridAccurateBase;
        public bool hasLodGroup;
        [TextArea(2, 5)] public string notes;

        public ProductionVisualViewCoverage ViewCoverage
        {
            get
            {
                var coverage = ProductionVisualViewCoverage.None;
                if (hasTopDetail)
                    coverage |= ProductionVisualViewCoverage.Top;
                if (hasFrontDetail)
                    coverage |= ProductionVisualViewCoverage.Front;
                if (hasBackDetail)
                    coverage |= ProductionVisualViewCoverage.Back;
                if (hasLeftDetail)
                    coverage |= ProductionVisualViewCoverage.Left;
                if (hasRightDetail)
                    coverage |= ProductionVisualViewCoverage.Right;
                if (hasRoofDetail)
                    coverage |= ProductionVisualViewCoverage.Roof;
                return coverage;
            }
        }
    }
}
