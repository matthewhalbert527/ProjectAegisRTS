using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8ConceptArtImporter
    {
        [MenuItem("ProjectAegisRTS/Stage 8/Import Concept Art References")]
        public static void ImportConceptArtMenu()
        {
            ImportConceptArt();
        }

        public static void ImportConceptArtBatch()
        {
            try
            {
                ImportConceptArt();
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        public static List<ConceptArtReference> ImportConceptArt()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            var specs = Stage8ActorCatalog.LoadSpecs();
            var references = new List<ConceptArtReference>();

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                CopyConceptIfNeeded(spec);
                ImportAsSprite(Stage8ActorCatalog.ConceptAssetPath(spec));
                var reference = CreateOrUpdateReference(spec);
                references.Add(reference);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 8 concept art references imported: " + references.Count);
            return references;
        }

        static void CopyConceptIfNeeded(Stage8ActorSpec spec)
        {
            if (string.IsNullOrEmpty(spec.SourceFile) || spec.SourceFile.StartsWith("._", StringComparison.Ordinal) || spec.SourceFile.Contains("__MACOSX"))
                return;

            var sourcePath = Stage8ActorCatalog.SourceConceptAbsolutePath(spec);
            if (!File.Exists(sourcePath))
                return;

            var destinationPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Stage8ActorCatalog.ConceptAssetPath(spec)));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            File.Copy(sourcePath, destinationPath, true);
        }

        static ConceptArtReference CreateOrUpdateReference(Stage8ActorSpec spec)
        {
            var path = Stage8ActorCatalog.ConceptReferenceAssetPath(spec);
            var reference = AssetDatabase.LoadAssetAtPath<ConceptArtReference>(path);
            if (reference == null)
            {
                reference = ScriptableObject.CreateInstance<ConceptArtReference>();
                AssetDatabase.CreateAsset(reference, path);
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Stage8ActorCatalog.ConceptAssetPath(spec));
            var sprite = Stage8ActorCatalog.LoadSpriteAtPath(Stage8ActorCatalog.ConceptAssetPath(spec));
            reference.actorTypeId = spec.ActorTypeId;
            reference.originalLabel = spec.OriginalLabel;
            reference.safeDisplayName = spec.SafeDisplayName;
            reference.sourceFileRelativePath = string.IsNullOrEmpty(spec.SourceFile) ? string.Empty : "art/concepts/source/" + spec.SourceFile;
            reference.unityTexture = texture;
            reference.unitySprite = sprite;
            reference.category = spec.Category;
            reference.intendedRole = spec.IntendedRole;
            reference.artNotes = spec.ArtNotes;
            reference.animationNotes = spec.AnimationNotes;
            reference.ipReviewRequired = spec.IpReviewRequired;
            EditorUtility.SetDirty(reference);
            return reference;
        }

        static void ImportAsSprite(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
