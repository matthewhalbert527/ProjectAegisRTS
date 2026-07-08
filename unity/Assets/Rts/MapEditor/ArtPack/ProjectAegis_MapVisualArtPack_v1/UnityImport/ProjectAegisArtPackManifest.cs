// Project Aegis RTS - Art Pack Manifest DTOs
// Plain serializable data classes only. No UnityEditor dependency.
using System;
using System.Collections.Generic;

namespace ProjectAegis.MapEditor.ArtPack
{
    [Serializable]
    public sealed class ProjectAegisArtPackManifest
    {
        public string packId;
        public string name;
        public string version;
        public string unityTarget;
        public string worldScale;
        public string origin;
        public string runtimeFormatNote;
        public List<ProjectAegisArtPackAsset> assets = new List<ProjectAegisArtPackAsset>();
    }

    [Serializable]
    public sealed class ProjectAegisArtPackAsset
    {
        public string id;
        public string category;
        public string kind;
        public string file;
        public string intendedUnityScale;
        public string pivotNotes;
        public ProjectAegisMaterialTexturePaths materialTexturePaths;
        public string recommendedPlacementRules;
    }

    [Serializable]
    public sealed class ProjectAegisMaterialTexturePaths
    {
        public string albedo;
        public string normal;
        public string roughnessAO;
    }
}
