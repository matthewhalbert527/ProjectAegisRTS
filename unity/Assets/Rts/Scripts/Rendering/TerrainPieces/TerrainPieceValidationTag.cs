using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    public sealed class TerrainPieceValidationTag : MonoBehaviour
    {
        public string pieceId;
        public string displayName;
        public TerrainPieceCategory category;
        public TerrainPieceSizeClass sizeClass;
        public int footprintFineWidth;
        public int footprintFineHeight;
        public string materialProfileId;
        public string passabilityVisualHint;
        public string buildableVisualHint;
        public bool supportsRotation;
        public bool supportsTint;
        public bool isGameplayBlockingVisualOnly;
        public string questBudgetTag;
        public int rendererCount;
        public int primitiveCount;
        [TextArea(2, 5)] public string notes;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(pieceId) &&
                !string.IsNullOrEmpty(displayName) &&
                footprintFineWidth > 0 &&
                footprintFineHeight > 0 &&
                !string.IsNullOrEmpty(materialProfileId) &&
                !string.IsNullOrEmpty(passabilityVisualHint) &&
                !string.IsNullOrEmpty(buildableVisualHint) &&
                !string.IsNullOrEmpty(questBudgetTag) &&
                rendererCount > 0 &&
                primitiveCount > 0;
        }
    }
}
