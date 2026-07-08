using System;
using UnityEngine;
namespace ProjectAegisRTS.UnityClient.MapEditor.ArtPack { [Serializable] public sealed class ProjectAegisArtPackManifest { public string packId; public string packVersion; public string targetUnityPath; public ProjectAegisArtPackAsset[] assets; } [Serializable] public sealed class ProjectAegisArtPackAsset { public string id; public string category; public string path; public string semanticRole; public Vector3 unityScale = Vector3.one; public string pivotNotes; public string recommendedPlacementRules; } }
