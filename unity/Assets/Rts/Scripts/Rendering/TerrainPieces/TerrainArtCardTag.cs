using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [DisallowMultipleComponent]
    public sealed class TerrainArtCardTag : MonoBehaviour
    {
        public string cardId;
        public string sourceImagePath;
        public TerrainPieceCategory category;
        public Vector2 worldSizeMeters = Vector2.one;
        public Vector2Int fineGridSize = Vector2Int.one;
        public bool playerFacingReplacement;
        public bool visualOnly = true;
        public bool imageBackedCard = true;
        public bool sourceTextureAssigned;
        public int rendererCount;
        public int materialCount;
        [TextArea(2, 5)] public string notes;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(cardId) &&
                !string.IsNullOrEmpty(sourceImagePath) &&
                worldSizeMeters.x > 0f &&
                worldSizeMeters.y > 0f &&
                fineGridSize.x > 0 &&
                fineGridSize.y > 0 &&
                visualOnly &&
                imageBackedCard &&
                sourceTextureAssigned &&
                rendererCount > 0 &&
                materialCount > 0;
        }
    }
}
