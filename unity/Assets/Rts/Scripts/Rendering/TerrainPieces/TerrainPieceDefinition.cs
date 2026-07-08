using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Stage32/Terrain Piece Definition")]
    public sealed class TerrainPieceDefinition : ScriptableObject
    {
        public string pieceId;
        public string displayName;
        public TerrainPieceCategory category;
        public TerrainPieceSizeClass sizeClass;
        public int footprintFineWidth = 1;
        public int footprintFineHeight = 1;
        public GameObject prefab;
        public string materialProfileId;
        public string passabilityVisualHint;
        public string buildableVisualHint;
        public bool supportsRotation = true;
        public bool supportsTint = true;
        public bool isGameplayBlockingVisualOnly;
        [TextArea(2, 5)] public string notes;
        public string questBudgetTag = "QuestSafeLow";

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(pieceId) &&
                !string.IsNullOrEmpty(displayName) &&
                footprintFineWidth > 0 &&
                footprintFineHeight > 0 &&
                prefab != null &&
                !string.IsNullOrEmpty(materialProfileId) &&
                !string.IsNullOrEmpty(passabilityVisualHint) &&
                !string.IsNullOrEmpty(buildableVisualHint) &&
                !string.IsNullOrEmpty(questBudgetTag);
        }
    }
}
