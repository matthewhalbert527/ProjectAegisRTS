using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [DisallowMultipleComponent]
    public sealed class Stage32_6RuntimeTerrainTag : MonoBehaviour
    {
        public string runtimeAssetId;
        public string mappedTerrainPieceId;
        public TerrainPieceCategory category;
        public bool referenceOnlyPolicyEnforced = true;
        public bool usesReferenceTexture;
        public bool flatImageCard;
        public bool hasBeveledMesh;
        public bool hasChildGeometry;
        public bool pivotAtOrigin;
        public int rendererCount;
        public int materialCount;
        public string sourceArtPolicy = "Batch01 terrain images are art-direction reference only.";
        [TextArea(2, 5)] public string notes;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(runtimeAssetId) &&
                referenceOnlyPolicyEnforced &&
                !usesReferenceTexture &&
                !flatImageCard &&
                hasBeveledMesh &&
                hasChildGeometry &&
                pivotAtOrigin &&
                rendererCount >= 3 &&
                materialCount > 0;
        }
    }
}
