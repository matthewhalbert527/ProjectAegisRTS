using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [DisallowMultipleComponent]
    public sealed class TerrainArtSourceTag : MonoBehaviour
    {
        public string batchId;
        public string artId;
        public string replacesPieceId;
        public TerrainArtSourceKind sourceKind;
        public string sourceAssetPath;
        public bool sourceImported;
        public bool coreBatch;
        public bool playerFacingReplacement;
        public Vector4 uvRect;

        public bool IsPlayerFacingSourceArt()
        {
            return sourceImported &&
                playerFacingReplacement &&
                !string.IsNullOrEmpty(batchId) &&
                !string.IsNullOrEmpty(artId) &&
                !string.IsNullOrEmpty(replacesPieceId) &&
                !string.IsNullOrEmpty(sourceAssetPath);
        }
    }
}
