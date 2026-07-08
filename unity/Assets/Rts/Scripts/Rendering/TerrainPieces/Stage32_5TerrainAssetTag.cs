using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [DisallowMultipleComponent]
    public sealed class Stage32_5TerrainAssetTag : MonoBehaviour
    {
        public string assetId;
        public string category;
        public int fineGridWidth;
        public int fineGridHeight;
        public bool passable;
        public bool buildable;
        public string transparentPngPath;
        public string fallbackCardPngPath;
        public bool canonicalSourceAsset;
        public bool mappedStage32Replacement;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(assetId) &&
                !string.IsNullOrEmpty(category) &&
                fineGridWidth > 0 &&
                fineGridHeight > 0 &&
                !string.IsNullOrEmpty(transparentPngPath);
        }
    }
}
