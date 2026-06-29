using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8IconGenerator
    {
        [MenuItem("ProjectAegisRTS/Stage 8/Generate Actor Icons")]
        public static void GenerateIconsMenu()
        {
            GenerateIcons();
        }

        public static void GenerateIconsBatch()
        {
            try
            {
                GenerateIcons();
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

        public static List<Sprite> GenerateIcons()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            var specs = Stage8ActorCatalog.LoadSpecs();
            var sprites = new List<Sprite>();

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                var icon = CreateIconTexture(spec);
                var absolutePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Stage8ActorCatalog.IconAssetPath(spec)));
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
                File.WriteAllBytes(absolutePath, icon.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(icon);
                AssetDatabase.ImportAsset(Stage8ActorCatalog.IconAssetPath(spec), ImportAssetOptions.ForceUpdate);
                ImportAsSprite(Stage8ActorCatalog.IconAssetPath(spec));
                sprites.Add(Stage8ActorCatalog.LoadSpriteAtPath(Stage8ActorCatalog.IconAssetPath(spec)));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Stage8ActorVisualDefinitionGenerator.CreateOrUpdateDefinitions();
            Debug.Log("Stage 8 actor icons updated: " + sprites.Count);
            return sprites;
        }

        static Texture2D CreateIconTexture(Stage8ActorSpec spec)
        {
            var sourcePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Stage8ActorCatalog.ConceptAssetPath(spec)));
            Texture2D source = null;
            if (File.Exists(sourcePath))
            {
                source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                source.LoadImage(File.ReadAllBytes(sourcePath));
            }

            var icon = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            var tint = ColorForCategory(spec.Category);
            for (var y = 0; y < 128; y++)
                for (var x = 0; x < 128; x++)
                {
                    Color color;
                    if (source != null)
                    {
                        color = source.GetPixelBilinear((x + 0.5f) / 128f, (y + 0.5f) / 128f);
                        color = Color.Lerp(color, tint, 0.18f);
                    }
                    else
                    {
                        var checker = ((x / 16) + (y / 16)) % 2 == 0 ? 0.85f : 0.65f;
                        color = tint * checker;
                        color.a = 1f;
                    }

                    icon.SetPixel(x, y, color);
                }

            icon.Apply();
            if (source != null)
                UnityEngine.Object.DestroyImmediate(source);
            return icon;
        }

        static Color ColorForCategory(ActorArtCategory category)
        {
            switch (category)
            {
                case ActorArtCategory.Infantry: return new Color(0.35f, 0.72f, 0.46f);
                case ActorArtCategory.Vehicle: return new Color(0.72f, 0.58f, 0.34f);
                case ActorArtCategory.Aircraft: return new Color(0.42f, 0.65f, 0.84f);
                case ActorArtCategory.Defense: return new Color(0.78f, 0.42f, 0.36f);
                case ActorArtCategory.Support: return new Color(0.56f, 0.48f, 0.78f);
                default: return new Color(0.54f, 0.60f, 0.66f);
            }
        }

        static void ImportAsSprite(string assetPath)
        {
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
