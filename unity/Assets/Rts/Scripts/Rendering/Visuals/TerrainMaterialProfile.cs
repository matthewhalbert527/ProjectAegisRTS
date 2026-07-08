using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Visuals/Terrain Material Profile")]
    public sealed class TerrainMaterialProfile : ScriptableObject
    {
        public string terrainKind;
        public string displayName;
        public Material material;
        public Color baseColor = Color.white;
        public Color accentColor = Color.gray;
        public bool blocksGroundMovement;
        public bool supportsResources;
        public bool supportsBasePlacement;
        public bool needsFineGridGuidance = true;
        [TextArea(2, 5)] public string readabilityNotes;

        public void Configure(string kind, string label, Material terrainMaterial, Color baseTint, Color accentTint, bool blocked, bool resource, bool placement, string notes)
        {
            terrainKind = kind;
            displayName = label;
            material = terrainMaterial;
            baseColor = baseTint;
            accentColor = accentTint;
            blocksGroundMovement = blocked;
            supportsResources = resource;
            supportsBasePlacement = placement;
            needsFineGridGuidance = kind != "Water" && kind != "FogExplored";
            readabilityNotes = notes;
        }
    }
}
