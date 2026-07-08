using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [DisallowMultipleComponent]
    public sealed class Stage32_6FinalTerrainMeshTag : MonoBehaviour
    {
        public string assetId;
        public string sourceMeshPath;
        public string manifestPath;
        public TerrainPieceCategory category;
        public Vector2 worldSizeMeters;
        public Vector2Int fineGridSize;
        public bool passable;
        public bool buildable;
        public bool harvestable;
        public bool playerFacingReplacement;
        public bool previewPngUsedAsRuntimeCard;
        public int rendererCount;
        public int materialCount;
        [TextArea(2, 5)] public string notes;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(assetId) &&
                !string.IsNullOrEmpty(sourceMeshPath) &&
                !string.IsNullOrEmpty(manifestPath) &&
                worldSizeMeters.x > 0f &&
                worldSizeMeters.y > 0f &&
                fineGridSize.x > 0 &&
                fineGridSize.y > 0 &&
                !previewPngUsedAsRuntimeCard &&
                rendererCount > 0 &&
                materialCount > 0;
        }
    }
}
