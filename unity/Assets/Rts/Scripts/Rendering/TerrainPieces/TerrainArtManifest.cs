using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    public enum TerrainArtSourceKind
    {
        Texture,
        Model,
        Prefab
    }

    [CreateAssetMenu(menuName = "ProjectAegisRTS/Stage32/Terrain Art Manifest")]
    public sealed class TerrainArtManifest : ScriptableObject
    {
        public string batchId = "Batch01";
        public string sourceFolder = "Assets/Rts/Art/Source/Terrain/Batch01";
        public List<TerrainArtManifestEntry> entries = new List<TerrainArtManifestEntry>();

        public int CountPlayerFacingReplacements()
        {
            var count = 0;
            if (entries == null)
                return count;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.playerFacingReplacement && entry.generatedPrefab != null)
                    count++;
            }

            return count;
        }

        public TerrainArtManifestEntry FindReplacement(string pieceId)
        {
            if (entries == null || string.IsNullOrEmpty(pieceId))
                return null;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.playerFacingReplacement && entry.replacesPieceId == pieceId)
                    return entry;
            }

            return null;
        }
    }

    [Serializable]
    public sealed class TerrainArtManifestEntry
    {
        public string artId;
        public string displayName;
        public string replacesPieceId;
        public TerrainPieceCategory category;
        public TerrainArtSourceKind sourceKind;
        public UnityEngine.Object sourceAsset;
        public string sourceAssetPath;
        public Material generatedMaterial;
        public GameObject generatedPrefab;
        public Vector4 uvRect;
        public bool coreBatch;
        public bool playerFacingReplacement;
        [TextArea(2, 5)] public string notes;
    }
}
