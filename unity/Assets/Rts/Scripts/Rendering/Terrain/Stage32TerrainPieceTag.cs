using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Terrain
{
    public enum Stage32TerrainCategory
    {
        Ground,
        Transition,
        BaseConstruction,
        Road,
        Resource,
        Obstacle,
        BattlefieldProp,
        Vegetation,
        Water,
        ReviewOnly
    }

    [DisallowMultipleComponent]
    public sealed class Stage32TerrainPieceTag : MonoBehaviour
    {
        [Header("Identity")]
        public string terrainId = string.Empty;
        public string displayName = string.Empty;
        public Stage32TerrainCategory category = Stage32TerrainCategory.Ground;

        [Header("Grid / Gameplay Metadata")]
        public Vector2Int fineGridSize = new Vector2Int(2, 2);
        public bool blocksMovement;
        public bool buildable = true;
        public bool road;
        public bool water;
        public bool resourceField;
        public bool decorativeOnly;

        [Header("Visual QA")]
        public bool hasBeveledBase = true;
        public bool hasTopDownReadableShape = true;
        public bool questSafeProxy = true;
        public int estimatedMeshObjectCount;
        public int estimatedMaterialCount;
        [TextArea(2, 6)] public string notes = string.Empty;
    }
}
