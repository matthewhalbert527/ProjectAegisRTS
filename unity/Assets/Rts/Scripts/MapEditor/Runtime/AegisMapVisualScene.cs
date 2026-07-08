using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor
{
    public sealed class AegisMapVisualScene : MonoBehaviour
    {
        public string MapId;
        public string SourceAssetPath;
        public int Width;
        public int Height;
        public int Seed;
        public string Biome;
        public string VisualCompilerVersion;
        public string VisualThemeId;
        public string ArtPackRoot;
        public bool UsesArtPackTextures;
        public bool UsesArtPackMeshes;
        public int ArtPackMeshInstanceCount;
        public int ArtPackDerivedProxyInstanceCount;
        public int GenericFallbackInstanceCount;
        [TextArea(3, 12)]
        public string VisualCompilerSummary;
    }
}
