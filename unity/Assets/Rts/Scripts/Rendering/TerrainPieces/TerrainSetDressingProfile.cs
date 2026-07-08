using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Stage32/Terrain Set Dressing Profile")]
    public sealed class TerrainSetDressingProfile : ScriptableObject
    {
        public string profileId;
        public string displayName;
        public int deterministicSeed = 3201;
        public int maxRenderedPieces = 48;
        public bool preserveFineGridReadability = true;
        public bool visualOnlyNeverGameplayAuthority = true;
        [TextArea(2, 6)] public string notes;
        public List<TerrainSetDressingPlacement> placements = new List<TerrainSetDressingPlacement>();

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(profileId) &&
                !string.IsNullOrEmpty(displayName) &&
                maxRenderedPieces > 0 &&
                visualOnlyNeverGameplayAuthority &&
                placements != null &&
                placements.Count > 0;
        }
    }

    [Serializable]
    public sealed class TerrainSetDressingPlacement
    {
        public string pieceId;
        public Vector3 localPosition;
        public float rotationY;
        public float uniformScale = 1f;
        public Color tint = Color.white;
        public string placementRole;
    }
}
