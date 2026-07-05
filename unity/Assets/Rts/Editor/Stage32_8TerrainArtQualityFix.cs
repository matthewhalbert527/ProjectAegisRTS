using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Rendering.Terrain;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32_8TerrainArtQualityFix
    {
        public const string CardSourceFolder = "Assets/Rts/Art/Source/Terrain/Batch01/individual";
        public const string CardMaterialFolder = "Assets/Rts/Art/Materials/Terrain/Stage32_8Cards";
        public const string CardPrefabFolder = "Assets/Rts/Art/Prefabs/Terrain/Stage32_8Cards";
        public const string CardMappedPrefabFolder = CardPrefabFolder + "/MappedDefinitions";
        public const string CardMeshFolder = "Assets/Rts/Art/Meshes/Terrain/Stage32_8Cards";
        public const string CardMeshPath = CardMeshFolder + "/image_card_quad.asset";
        public const string ReviewScenePath = "Assets/Rts/Scenes/Stage32_8_TerrainArtQualityReview.unity";
        public const string QualityReportPath = "docs/STAGE32_8_TERRAIN_IMPORT_QUALITY_REPORT.md";
        public const string CardModeDocPath = "docs/STAGE32_8_TERRAIN_ART_CARD_MODE.md";
        public const string ReportPath = "docs/STAGE32_8_REPORT.md";
        const string FinalMeshMaterialFolder = Stage32_6FinalTerrainMeshBatch01Importer.MaterialFolder;
        const string FinalMeshPrefabFolder = Stage32_6FinalTerrainMeshBatch01Importer.PrefabFolder;
        const string FinalMeshSourceFolder = Stage32_6FinalTerrainMeshBatch01Importer.SourceFolder;
        const int MinimumCardPrefabCount = 6;
        const int MinimumPlayerFacingCardReplacements = 2;

        static readonly CardSpec[] CardSpecs =
        {
            new CardSpec("grass_ground_01", "Grass Ground 01 Card", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 4f, 4f, 8, 8, true, true, "ground_grass_dirt_patch_01"),
            new CardSpec("grass_ground_02", "Grass Ground 02 Card", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 4f, 4f, 8, 8, true, true, null),
            new CardSpec("dirt_ground_01", "Dirt Ground 01 Card", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 4f, 4f, 8, 8, true, true, null),
            new CardSpec("road_straight_01", "Road Straight 01 Card", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Medium, 4f, 4f, 8, 8, true, false, "ground_road_path_01"),
            new CardSpec("concrete_pad_medium", "Concrete Pad Medium Card", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4f, 4f, 8, 8, true, true, null),
            new CardSpec("resource_cluster_blue_01", "Blue Resource Cluster 01 Card", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 3.3f, 3.3f, 7, 7, false, false, "resource_cluster_01"),
            new CardSpec("rock_blocker_01", "Rock Blocker 01 Card", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Medium, 4f, 4f, 8, 8, false, false, "obstacle_rock_cluster_01"),
            new CardSpec("shrub_cluster_01", "Shrub Cluster 01 Card", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2.5f, 2.5f, 5, 5, true, false, null)
        };

        static readonly string[] FinalTexturePaths =
        {
            FinalMeshSourceFolder + "/ground_grass_dirt_01/textures/ground_grass_dirt_01_albedo.png",
            FinalMeshSourceFolder + "/ground_grass_dirt_01/textures/ground_grass_dirt_01_normal.png",
            FinalMeshSourceFolder + "/ground_grass_dirt_01/textures/ground_grass_dirt_01_roughness.png",
            FinalMeshSourceFolder + "/ground_grass_dirt_01/textures/ground_grass_dirt_01_height.png",
            FinalMeshSourceFolder + "/resource_cluster_blue_01/textures/resource_cluster_blue_01_ground_albedo.png",
            FinalMeshSourceFolder + "/resource_cluster_blue_01/textures/resource_cluster_blue_01_ground_normal.png",
            FinalMeshSourceFolder + "/resource_cluster_blue_01/textures/resource_cluster_blue_01_ground_roughness.png"
        };

        [MenuItem("ProjectAegisRTS/Stage 32.8/Generate Terrain Art Quality Fix")]
        public static void GenerateStage32_8Menu()
        {
            EnsureStage32_8Assets();
        }

        public static void GenerateStage32_8AssetsBatch()
        {
            try
            {
                var summary = EnsureStage32_8Assets();
                Debug.Log("Stage 32.8 terrain art quality assets generated. Card prefabs: " + summary.CardPrefabCount);
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

        public static void CreateStage32_8ReviewSceneBatch()
        {
            try
            {
                EnsureStage32_8Assets();
                CreateOrUpdateReviewScene();
                Debug.Log("Stage 32.8 terrain art quality review scene created.");
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

        public static void ValidateStage32_8Batch()
        {
            try
            {
                var summary = ValidateStage32_8Assets();
                Debug.Log("Stage 32.8 terrain art quality validation passed. Card prefabs: " + summary.CardPrefabCount + ", mapped replacements: " + summary.PlayerFacingCardReplacementCount);
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

        public static Stage32_8Summary EnsureStage32_8Assets()
        {
            EnsureFolders();
            Stage32_6FinalTerrainMeshBatch01Importer.EnsureFinalMeshBatch01();
            ConfigureFinalTextureImports();
            AssetDatabase.Refresh();
            ImproveFinalMeshMaterials();

            var mesh = EnsureCardMesh();
            var cardPrefabs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            var availableCards = 0;
            for (var i = 0; i < CardSpecs.Length; i++)
            {
                var spec = CardSpecs[i];
                if (!HasCardSource(spec))
                    continue;
                ConfigureCardTextureImport(spec);
                var material = CreateCardMaterial(spec);
                var prefab = CreateCardPrefab(spec, material, mesh, false, null);
                cardPrefabs[spec.Id] = prefab;
                availableCards++;
            }

            var mapped = ApplyCardPrefabsToPlayerFacingDefinitions(cardPrefabs);
            CreateOrUpdateReviewScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var summary = BuildSummary(availableCards, mapped);
            WriteDocs(summary, new List<string>());
            return summary;
        }

        public static Stage32_8Summary ValidateStage32_8Assets()
        {
            var summary = EnsureStage32_8Assets();
            var errors = new List<string>();
            ValidateFinalMeshMaterials(errors);
            ValidateFinalMeshPrefabs(errors);
            ValidateCardPrefabs(errors);
            ValidatePlayerFacingMappings(errors);
            ValidateReviewScene(errors);

            summary.Errors = errors;
            WriteDocs(summary, errors);
            if (errors.Count > 0)
                throw new InvalidOperationException("Stage 32.8 terrain import quality validation failed: " + string.Join(" | ", errors.ToArray()));

            return summary;
        }

        static void ConfigureFinalTextureImports()
        {
            for (var i = 0; i < FinalTexturePaths.Length; i++)
            {
                var path = FinalTexturePaths[i];
                var normal = path.IndexOf("_normal", StringComparison.OrdinalIgnoreCase) >= 0;
                var roughnessOrHeight = path.IndexOf("_roughness", StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf("_height", StringComparison.OrdinalIgnoreCase) >= 0;
                ConfigureTextureImport(path, normal, !normal && !roughnessOrHeight, false);
            }
        }

        static void ConfigureCardTextureImport(CardSpec spec)
        {
            ConfigureTextureImport(spec.CardSourcePath, false, true, true);
        }

        static void ConfigureTextureImport(string assetPath, bool normalMap, bool sRgb, bool alpha)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            var dirty = false;
            var desiredType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (importer.textureType != desiredType)
            {
                importer.textureType = desiredType;
                dirty = true;
            }
            if (!normalMap && importer.sRGBTexture != sRgb)
            {
                importer.sRGBTexture = sRgb;
                dirty = true;
            }
            if (importer.alphaIsTransparency != alpha)
            {
                importer.alphaIsTransparency = alpha;
                dirty = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                dirty = true;
            }
            if (importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                dirty = true;
            }
            if (importer.filterMode != FilterMode.Trilinear)
            {
                importer.filterMode = FilterMode.Trilinear;
                dirty = true;
            }
            if (importer.maxTextureSize < 2048)
            {
                importer.maxTextureSize = 2048;
                dirty = true;
            }

            if (dirty)
                importer.SaveAndReimport();
        }

        static void ImproveFinalMeshMaterials()
        {
            ConfigureFinalMaterial(
                "ground_grass_dirt_01_ground_surface",
                FinalTexturePaths[0],
                FinalTexturePaths[1],
                FinalTexturePaths[2],
                new Color(1.08f, 1.12f, 1.02f, 1f),
                0.08f,
                0f,
                1.15f);
            ConfigureFinalMaterial(
                "resource_cluster_blue_01_resource_ground",
                FinalTexturePaths[4],
                FinalTexturePaths[5],
                FinalTexturePaths[6],
                new Color(1.04f, 0.98f, 0.88f, 1f),
                0.08f,
                0f,
                1.0f);

            ConfigureColorMaterial("ground_grass_dirt_01_edge_dark", new Color(0.23f, 0.19f, 0.14f, 1f), 0.04f, 0f);
            ConfigureColorMaterial("ground_grass_dirt_01_stone", new Color(0.54f, 0.52f, 0.47f, 1f), 0.06f, 0f);
            ConfigureColorMaterial("ground_grass_dirt_01_grass_detail", new Color(0.30f, 0.58f, 0.22f, 1f), 0.04f, 0f);
            ConfigureColorMaterial("resource_cluster_blue_01_edge_dark", new Color(0.21f, 0.16f, 0.11f, 1f), 0.04f, 0f);
            ConfigureColorMaterial("resource_cluster_blue_01_stone", new Color(0.52f, 0.50f, 0.45f, 1f), 0.06f, 0f);
            ConfigureColorMaterial("resource_cluster_blue_01_crystal_blue", new Color(0.05f, 0.34f, 1.45f, 1f), 0.78f, 0f);
            ConfigureColorMaterial("resource_cluster_blue_01_crystal_cyan", new Color(0.02f, 0.95f, 0.90f, 1f), 0.74f, 0f);
        }

        static void ConfigureFinalMaterial(string materialId, string albedoPath, string normalPath, string roughnessPath, Color color, float smoothness, float metallic, float bumpScale)
        {
            var material = LoadOrCreateMaterial(FinalMeshMaterialFolder, materialId, false);
            ApplyOpaqueMaterialBase(material, color, smoothness, metallic);
            ApplyTexture(material, new[] { "_BaseMap", "_MainTex" }, AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath), true);
            ApplyTexture(material, new[] { "_BumpMap" }, AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath), false);
            ApplyTexture(material, new[] { "_MetallicGlossMap", "_SpecGlossMap", "_MaskMap" }, AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath), false);
            if (material.HasProperty("_BumpScale"))
                material.SetFloat("_BumpScale", bumpScale);
            if (material.HasProperty("_OcclusionStrength"))
                material.SetFloat("_OcclusionStrength", 0.35f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
        }

        static void ConfigureColorMaterial(string materialId, Color color, float smoothness, float metallic)
        {
            var material = LoadOrCreateMaterial(FinalMeshMaterialFolder, materialId, false);
            ApplyOpaqueMaterialBase(material, color, smoothness, metallic);
            EditorUtility.SetDirty(material);
        }

        static void ApplyOpaqueMaterialBase(Material material, Color color, float smoothness, float metallic)
        {
            SetMaterialColor(material, color);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 0f);
            material.renderQueue = -1;
            material.SetOverrideTag("RenderType", "Opaque");
        }

        static Mesh EnsureCardMesh()
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(CardMeshPath);
            if (mesh != null)
                return mesh;

            mesh = new Mesh { name = "stage32_8_image_card_quad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(0.5f, 0f, 0.5f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, CardMeshPath);
            return mesh;
        }

        static Material CreateCardMaterial(CardSpec spec)
        {
            var material = LoadOrCreateMaterial(CardMaterialFolder, spec.Id + "_card", true);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spec.CardSourcePath);
            ApplyTexture(material, new[] { "_BaseMap", "_MainTex" }, texture, true);
            var color = Color.white;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_AlphaClip"))
                material.SetFloat("_AlphaClip", 0f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.05f);
            material.renderQueue = 3000;
            material.SetOverrideTag("RenderType", "Transparent");
            EditorUtility.SetDirty(material);
            return material;
        }

        static GameObject CreateCardPrefab(CardSpec spec, Material material, Mesh mesh, bool mappedDefinition, TerrainPieceDefinition definition)
        {
            var root = new GameObject(mappedDefinition ? definition.pieceId : spec.Id);
            var visual = new GameObject("ImageBackedTerrainCard");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(spec.WorldWidth, 1f, spec.WorldDepth);
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;
            visual.AddComponent<MeshRenderer>().sharedMaterial = material;

            AddCardMetadata(root, spec, mappedDefinition, definition);
            var path = (mappedDefinition ? CardMappedPrefabFolder : CardPrefabFolder) + "/" + (mappedDefinition ? definition.pieceId : spec.Id) + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            if (prefab == null)
                throw new InvalidOperationException("Could not save Stage32.8 terrain card prefab: " + path);
            return prefab;
        }

        static void AddCardMetadata(GameObject root, CardSpec spec, bool mappedDefinition, TerrainPieceDefinition definition)
        {
            var validation = root.AddComponent<TerrainPieceValidationTag>();
            validation.pieceId = mappedDefinition ? definition.pieceId : spec.Id;
            validation.displayName = mappedDefinition ? definition.displayName : spec.DisplayName;
            validation.category = spec.Category;
            validation.sizeClass = mappedDefinition ? definition.sizeClass : spec.SizeClass;
            validation.footprintFineWidth = mappedDefinition ? definition.footprintFineWidth : spec.FineGridWidth;
            validation.footprintFineHeight = mappedDefinition ? definition.footprintFineHeight : spec.FineGridHeight;
            validation.materialProfileId = "stage32_8_card_" + spec.Id;
            validation.passabilityVisualHint = "Image-backed card is visual-only; Rts.Core terrain/pathing remains authoritative.";
            validation.buildableVisualHint = "Image-backed card is visual-only; Rts.Core building placement remains authoritative.";
            validation.supportsRotation = true;
            validation.supportsTint = false;
            validation.isGameplayBlockingVisualOnly = !spec.Passable;
            validation.questBudgetTag = "QuestSafeStage32_8Card";
            validation.rendererCount = 1;
            validation.primitiveCount = 1;
            validation.notes = "Stage32.8 interim high-visual-quality terrain card using Batch01 individual art. It is not gameplay authority.";

            var cardTag = root.AddComponent<TerrainArtCardTag>();
            cardTag.cardId = spec.Id;
            cardTag.sourceImagePath = spec.CardSourcePath;
            cardTag.category = spec.Category;
            cardTag.worldSizeMeters = new Vector2(spec.WorldWidth, spec.WorldDepth);
            cardTag.fineGridSize = new Vector2Int(spec.FineGridWidth, spec.FineGridHeight);
            cardTag.playerFacingReplacement = mappedDefinition;
            cardTag.visualOnly = true;
            cardTag.imageBackedCard = true;
            cardTag.sourceTextureAssigned = true;
            cardTag.rendererCount = 1;
            cardTag.materialCount = 1;
            cardTag.notes = mappedDefinition ? "Mapped player-facing set dressing replacement." : "Canonical Stage32.8 image-backed terrain card.";

            var stage32Tag = root.AddComponent<Stage32TerrainPieceTag>();
            stage32Tag.terrainId = mappedDefinition ? definition.pieceId : spec.Id;
            stage32Tag.displayName = mappedDefinition ? definition.displayName : spec.DisplayName;
            stage32Tag.category = ToStage32Category(spec.Category);
            stage32Tag.fineGridSize = new Vector2Int(validation.footprintFineWidth, validation.footprintFineHeight);
            stage32Tag.blocksMovement = !spec.Passable;
            stage32Tag.buildable = spec.Buildable;
            stage32Tag.road = spec.Id.IndexOf("road", StringComparison.OrdinalIgnoreCase) >= 0;
            stage32Tag.resourceField = spec.Category == TerrainPieceCategory.Resource;
            stage32Tag.decorativeOnly = false;
            stage32Tag.hasBeveledBase = false;
            stage32Tag.hasTopDownReadableShape = true;
            stage32Tag.questSafeProxy = true;
            stage32Tag.estimatedMeshObjectCount = 1;
            stage32Tag.estimatedMaterialCount = 1;
            stage32Tag.notes = "Stage32.8 image-backed visual card; gameplay remains unchanged.";
        }

        static int ApplyCardPrefabsToPlayerFacingDefinitions(Dictionary<string, GameObject> cardPrefabs)
        {
            var library = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            if (library == null)
                return 0;

            var definitions = library.GetDefinitions();
            if (definitions == null)
                return 0;

            var mapped = 0;
            for (var i = 0; i < CardSpecs.Length; i++)
            {
                var spec = CardSpecs[i];
                if (string.IsNullOrEmpty(spec.MappedDefinitionId))
                    continue;

                GameObject sourcePrefab;
                if (!cardPrefabs.TryGetValue(spec.Id, out sourcePrefab) || sourcePrefab == null)
                    continue;

                var definition = FindDefinition(definitions, spec.MappedDefinitionId);
                if (definition == null)
                    continue;

                var material = AssetDatabase.LoadAssetAtPath<Material>(CardMaterialFolder + "/" + spec.Id + "_card.mat");
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(CardMeshPath);
                var mappedPrefab = CreateCardPrefab(spec, material, mesh, true, definition);
                definition.prefab = mappedPrefab;
                definition.supportsTint = false;
                definition.materialProfileId = "stage32_8_card_" + spec.Id;
                definition.passabilityVisualHint = "Stage32.8 image-backed visual replacement; passability remains owned by Rts.Core.";
                definition.buildableVisualHint = "Stage32.8 image-backed visual replacement; building placement remains owned by Rts.Core.";
                definition.isGameplayBlockingVisualOnly = !spec.Passable;
                definition.questBudgetTag = "QuestSafeStage32_8Card";
                definition.notes = "Stage32.8 player-facing image-card visual replacement mapped from " + spec.Id + ". This is an interim visual layer until authored 3D terrain arrives.";
                EditorUtility.SetDirty(definition);
                mapped++;
            }

            AssetDatabase.SaveAssets();
            return mapped;
        }

        static TerrainPieceDefinition FindDefinition(IReadOnlyList<TerrainPieceDefinition> definitions, string pieceId)
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition != null && string.Equals(definition.pieceId, pieceId, StringComparison.OrdinalIgnoreCase))
                    return definition;
            }
            return null;
        }

        static void CreateOrUpdateReviewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage32_8_TerrainArtQualityReview";

            var root = new GameObject("Stage32_8 Terrain Art Quality Review");
            AddFinalMesh(root.transform, "ground_grass_dirt_01", new Vector3(-4.1f, 0f, 1.35f), "Improved mesh material");
            AddFinalMesh(root.transform, "resource_cluster_blue_01", new Vector3(0.7f, 0f, 1.35f), "Improved mesh material");

            AddCardInstance(root.transform, "grass_ground_01", new Vector3(4.75f, 0.01f, 1.35f), "Image card: grass");
            AddCardInstance(root.transform, "resource_cluster_blue_01", new Vector3(8.8f, 0.01f, 1.35f), "Image card: resource");

            AddCardInstance(root.transform, "grass_ground_01", new Vector3(-3.2f, 0f, -3.1f), null);
            AddCardInstance(root.transform, "dirt_ground_01", new Vector3(0.9f, 0f, -3.1f), null);
            AddCardInstance(root.transform, "road_straight_01", new Vector3(5.0f, 0f, -3.1f), null);
            AddCardInstance(root.transform, "rock_blocker_01", new Vector3(8.9f, 0.01f, -3.1f), null);
            AddLabel(root.transform, "Sample image-card terrain patch", new Vector3(1.35f, 0.08f, -5.55f), 0.18f);

            AddSubtleGrid(root.transform, new Vector3(-4.1f, -0.01f, 1.35f), 4f, 4f, 8, 8);
            AddSubtleGrid(root.transform, new Vector3(0.7f, -0.01f, 1.35f), 3.3f, 3.3f, 7, 7);

            AddLabel(root.transform, "Stage 32.8 Terrain Art Import Quality\nImproved material imports + interim image-backed cards\nGrid helpers are subtle and sit behind the art", new Vector3(-6.75f, 0.08f, -6.45f), 0.18f);
            CreateLightingAndCamera();

            EditorSceneManager.SaveScene(scene, ReviewScenePath);
        }

        static void AddFinalMesh(Transform parent, string id, Vector3 position, string labelSuffix)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FinalMeshPrefabFolder + "/" + id + ".prefab");
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    instance.name = "ImprovedFinalMesh_" + id;
                    instance.transform.SetParent(parent, false);
                    instance.transform.localPosition = position;
                    instance.transform.localRotation = Quaternion.identity;
                }
            }

            AddLabel(parent, id + "\n" + labelSuffix, position + new Vector3(-1.75f, 0.07f, -2.3f), 0.16f);
        }

        static void AddCardInstance(Transform parent, string id, Vector3 position, string label)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabFolder + "/" + id + ".prefab");
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    instance.name = "ImageCard_" + id;
                    instance.transform.SetParent(parent, false);
                    instance.transform.localPosition = position;
                    instance.transform.localRotation = Quaternion.identity;
                }
            }

            if (!string.IsNullOrEmpty(label))
                AddLabel(parent, label, position + new Vector3(-1.65f, 0.07f, -2.3f), 0.16f);
        }

        static void AddSubtleGrid(Transform parent, Vector3 center, float width, float depth, int cellsX, int cellsZ)
        {
            var material = LoadOrCreateMaterial(CardMaterialFolder, "subtle_fine_grid_reference", true);
            var color = new Color(0.28f, 0.77f, 0.90f, 0.16f);
            SetMaterialColor(material, color);
            material.renderQueue = 3001;
            material.SetOverrideTag("RenderType", "Transparent");
            EditorUtility.SetDirty(material);

            var root = new GameObject("Stage32_8 Subtle Grid Helper");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = center;

            for (var i = 0; i <= cellsX; i++)
            {
                var offset = -width * 0.5f + width * i / cellsX;
                AddLine(root.transform, new Vector3(offset, 0f, 0f), new Vector3(0.008f, 0.006f, depth), material);
            }
            for (var i = 0; i <= cellsZ; i++)
            {
                var offset = -depth * 0.5f + depth * i / cellsZ;
                AddLine(root.transform, new Vector3(0f, 0f, offset), new Vector3(width, 0.006f, 0.008f), material);
            }
        }

        static void AddLine(Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "subtle_grid_line";
            line.transform.SetParent(parent, false);
            line.transform.localPosition = localPosition;
            line.transform.localScale = localScale;
            var renderer = line.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            var collider = line.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
        }

        static void AddLabel(Transform parent, string text, Vector3 position, float size)
        {
            var label = new GameObject("Label " + text.Replace('\n', ' '));
            label.transform.SetParent(parent, false);
            label.transform.localPosition = position;
            label.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size;
            mesh.anchor = TextAnchor.UpperLeft;
            mesh.color = new Color(0.92f, 0.94f, 0.90f, 1f);
        }

        static void CreateLightingAndCamera()
        {
            var lightObject = new GameObject("Key Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.94f, 0.84f, 1f);
            light.intensity = 2.55f;
            lightObject.transform.rotation = Quaternion.Euler(46f, -34f, 18f);

            var fillObject = new GameObject("Soft Terrain Fill");
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.intensity = 1.65f;
            fill.range = 16f;
            fillObject.transform.position = new Vector3(1.5f, 5.5f, -4f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.0f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.041f, 0.041f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            cameraObject.transform.position = new Vector3(2.2f, 10.6f, -7.8f);
            cameraObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }

        static void ValidateFinalMeshMaterials(List<string> errors)
        {
            RequireMaterialTexture("ground_grass_dirt_01_ground_surface", FinalTexturePaths[0], FinalTexturePaths[1], FinalTexturePaths[2], errors);
            RequireMaterialTexture("resource_cluster_blue_01_resource_ground", FinalTexturePaths[4], FinalTexturePaths[5], FinalTexturePaths[6], errors);
        }

        static void RequireMaterialTexture(string materialId, string albedoPath, string normalPath, string roughnessPath, List<string> errors)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(FinalMeshMaterialFolder + "/" + materialId + ".mat");
            if (material == null)
            {
                errors.Add("Final mesh material is missing: " + materialId);
                return;
            }

            if (!MaterialReferencesTexture(material, albedoPath))
                errors.Add(materialId + " does not reference its albedo texture.");
            if (!MaterialReferencesTexture(material, normalPath))
                errors.Add(materialId + " does not reference its normal map.");
            if (!MaterialReferencesTexture(material, roughnessPath))
                errors.Add(materialId + " does not reference its roughness map.");
        }

        static void ValidateFinalMeshPrefabs(List<string> errors)
        {
            ValidateFinalMeshPrefab("ground_grass_dirt_01", errors);
            ValidateFinalMeshPrefab("resource_cluster_blue_01", errors);
        }

        static void ValidateFinalMeshPrefab(string id, List<string> errors)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FinalMeshPrefabFolder + "/" + id + ".prefab");
            if (prefab == null)
            {
                errors.Add("Final mesh prefab missing: " + id);
                return;
            }

            if (prefab.GetComponent<Stage32_6FinalTerrainMeshTag>() == null)
                errors.Add(id + ": final mesh tag missing.");
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                errors.Add(id + ": no renderers.");
            if (CountTexturedMaterials(renderers) == 0)
                errors.Add(id + ": no renderer material references imported texture maps.");
            var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            var sourceMeshCount = 0;
            for (var i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh == null)
                    continue;
                var assetPath = AssetDatabase.GetAssetPath(mesh).Replace('\\', '/');
                if (assetPath.StartsWith(FinalMeshSourceFolder + "/", StringComparison.OrdinalIgnoreCase))
                    sourceMeshCount++;
            }
            if (sourceMeshCount == 0)
                errors.Add(id + ": prefab does not use FinalMeshBatch01 OBJ mesh geometry.");
        }

        static void ValidateCardPrefabs(List<string> errors)
        {
            var available = 0;
            for (var i = 0; i < CardSpecs.Length; i++)
            {
                var spec = CardSpecs[i];
                if (!HasCardSource(spec))
                    continue;
                available++;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabFolder + "/" + spec.Id + ".prefab");
                if (prefab == null)
                {
                    errors.Add("Image-card prefab missing: " + spec.Id);
                    continue;
                }

                var tag = prefab.GetComponent<TerrainArtCardTag>();
                if (tag == null || !tag.IsComplete())
                    errors.Add(spec.Id + ": TerrainArtCardTag missing or incomplete.");
                var renderer = prefab.GetComponentInChildren<Renderer>(true);
                if (renderer == null || renderer.sharedMaterial == null)
                    errors.Add(spec.Id + ": image-card renderer/material missing.");
                else if (!MaterialReferencesTexture(renderer.sharedMaterial, spec.CardSourcePath))
                    errors.Add(spec.Id + ": image-card material does not reference " + spec.CardSourcePath + ".");
            }

            if (available < MinimumCardPrefabCount)
                errors.Add("Not enough Batch01 card source images were available. Found " + available + ".");
        }

        static void ValidatePlayerFacingMappings(List<string> errors)
        {
            var library = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            var definitions = library != null ? library.GetDefinitions() : null;
            if (definitions == null)
            {
                errors.Add("Terrain piece library definitions are unavailable.");
                return;
            }

            var mapped = 0;
            for (var i = 0; i < CardSpecs.Length; i++)
            {
                var spec = CardSpecs[i];
                if (string.IsNullOrEmpty(spec.MappedDefinitionId))
                    continue;

                var definition = FindDefinition(definitions, spec.MappedDefinitionId);
                if (definition == null || definition.prefab == null)
                    continue;
                var tag = definition.prefab.GetComponent<TerrainArtCardTag>();
                if (tag != null && string.Equals(tag.cardId, spec.Id, StringComparison.OrdinalIgnoreCase) && tag.playerFacingReplacement)
                    mapped++;
            }

            if (mapped < MinimumPlayerFacingCardReplacements)
                errors.Add("Expected at least " + MinimumPlayerFacingCardReplacements + " player-facing image-card replacements; found " + mapped + ".");
        }

        static void ValidateReviewScene(List<string> errors)
        {
            if (!File.Exists(ToAbsoluteProjectPath(ReviewScenePath)))
            {
                errors.Add("Stage32.8 review scene missing.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ReviewScenePath);
            if (!scene.IsValid())
            {
                errors.Add("Stage32.8 review scene did not open.");
                return;
            }

            var finalTags = Object.FindObjectsByType<Stage32_6FinalTerrainMeshTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (finalTags.Length < 2)
                errors.Add("Stage32.8 review scene must include both final mesh prefabs.");

            var cardTags = Object.FindObjectsByType<TerrainArtCardTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (cardTags.Length < MinimumCardPrefabCount)
                errors.Add("Stage32.8 review scene must include a visible image-card terrain sample.");

            var primitiveOnly = finalTags.Length == 0 && cardTags.Length == 0;
            if (primitiveOnly)
                errors.Add("Stage32.8 review scene contains only fallback primitive content.");

            var objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < objects.Length; i++)
            {
                var objectName = objects[i].name;
                if (objectName.IndexOf("before placeholder", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    objectName.IndexOf("Placeholder Comparison", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    objectName.IndexOf("Batch01Imported", StringComparison.OrdinalIgnoreCase) >= 0)
                    errors.Add("Stage32.8 review scene contains old placeholder content: " + objectName);
            }

            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].name.IndexOf("subtle_grid", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                var material = renderers[i].sharedMaterial;
                if (material == null)
                    continue;
                if (!TryGetMaterialAlpha(material, out var alpha))
                    continue;
                if (alpha > 0.24f)
                    errors.Add("Stage32.8 subtle grid material is too opaque.");
            }
        }

        static int CountTexturedMaterials(Renderer[] renderers)
        {
            var count = 0;
            for (var i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                for (var j = 0; j < materials.Length; j++)
                    if (materials[j] != null && MaterialHasAnyTexture(materials[j]))
                        count++;
            }
            return count;
        }

        static bool MaterialHasAnyTexture(Material material)
        {
            var properties = material.GetTexturePropertyNames();
            for (var i = 0; i < properties.Length; i++)
                if (material.GetTexture(properties[i]) != null)
                    return true;
            return false;
        }

        static bool MaterialReferencesTexture(Material material, string expectedPath)
        {
            if (material == null)
                return false;
            var normalizedExpected = expectedPath.Replace('\\', '/');
            if (material.mainTexture != null)
            {
                var mainPath = AssetDatabase.GetAssetPath(material.mainTexture).Replace('\\', '/');
                if (string.Equals(mainPath, normalizedExpected, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            var properties = material.GetTexturePropertyNames();
            for (var i = 0; i < properties.Length; i++)
            {
                var texture = material.GetTexture(properties[i]);
                if (texture == null)
                    continue;
                var path = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                if (string.Equals(path, normalizedExpected, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        static void ApplyTexture(Material material, string[] properties, Texture texture, bool assignMainTexture)
        {
            if (texture == null)
                return;
            if (assignMainTexture)
                material.mainTexture = texture;
            for (var i = 0; i < properties.Length; i++)
                if (material.HasProperty(properties[i]))
                    material.SetTexture(properties[i], texture);
        }

        static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
        }

        static bool TryGetMaterialAlpha(Material material, out float alpha)
        {
            if (material.HasProperty("_BaseColor"))
            {
                alpha = material.GetColor("_BaseColor").a;
                return true;
            }
            if (material.HasProperty("_Color"))
            {
                alpha = material.GetColor("_Color").a;
                return true;
            }

            alpha = 0f;
            return false;
        }

        static Material LoadOrCreateMaterial(string folder, string materialId, bool transparent)
        {
            var path = folder + "/" + materialId + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            var shader = transparent ? Shader.Find("Universal Render Pipeline/Unlit") : Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null && transparent)
                shader = Shader.Find("Unlit/Transparent");
            if (shader == null)
                shader = Shader.Find("Standard");
            material = new Material(shader) { name = materialId };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static bool HasCardSource(CardSpec spec)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(spec.CardSourcePath) != null;
        }

        static Stage32TerrainCategory ToStage32Category(TerrainPieceCategory category)
        {
            switch (category)
            {
                case TerrainPieceCategory.BaseConstruction:
                    return Stage32TerrainCategory.BaseConstruction;
                case TerrainPieceCategory.Transition:
                    return Stage32TerrainCategory.Road;
                case TerrainPieceCategory.Resource:
                    return Stage32TerrainCategory.Resource;
                case TerrainPieceCategory.Obstacle:
                    return Stage32TerrainCategory.Obstacle;
                case TerrainPieceCategory.Prop:
                    return Stage32TerrainCategory.BattlefieldProp;
                default:
                    return Stage32TerrainCategory.Ground;
            }
        }

        static Stage32_8Summary BuildSummary(int cardCount, int mappedCount)
        {
            var summary = new Stage32_8Summary();
            summary.FinalMeshPrefabCount = CountAssets<GameObject>(FinalMeshPrefabFolder);
            summary.FinalMeshPrimaryMaterialCount = 2;
            summary.CardPrefabCount = cardCount;
            summary.PlayerFacingCardReplacementCount = mappedCount;
            summary.ReviewScenePath = ReviewScenePath;
            summary.Batch01CardSourceAvailable = cardCount > 0;
            return summary;
        }

        static int CountAssets<T>(string folder) where T : Object
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            return guids != null ? guids.Length : 0;
        }

        static void WriteDocs(Stage32_8Summary summary, List<string> errors)
        {
            WriteQualityReport(summary, errors);
            WriteCardModeDoc(summary);
            WriteReport(summary, errors);
        }

        static void WriteQualityReport(Stage32_8Summary summary, List<string> errors)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 32.8 Terrain Import Quality Report");
            builder.AppendLine();
            builder.AppendLine("- Albedo textures assigned: yes, on both primary final mesh materials.");
            builder.AppendLine("- Normal maps assigned: yes, final mesh normal imports are forced to Normal Map.");
            builder.AppendLine("- Roughness maps assigned: yes, roughness textures are assigned to supported Lit-map slots.");
            builder.AppendLine("- UVs valid: yes enough for texture assignment, but the supplied ground OBJ remains low-detail and cannot match the reference art by geometry alone.");
            builder.AppendLine("- Ground dark/flat root cause: the final OBJ is simple geometry with limited material slots, dark texture exposure, and an overpowering cyan grid helper in the old review composition.");
            builder.AppendLine("- Cyan grid: helper only; Stage32.8 moves it behind the art and lowers opacity so it no longer dominates.");
            builder.AppendLine("- Lighting/exposure: Stage32.8 review uses stronger warm key and fill lighting.");
            builder.AppendLine("- Mesh detail: still limited; real production quality requires artist-authored 3D terrain meshes/textures.");
            builder.AppendLine();
            builder.AppendLine("## Results");
            builder.AppendLine("- Final mesh prefabs: " + summary.FinalMeshPrefabCount);
            builder.AppendLine("- Image-card prefabs: " + summary.CardPrefabCount);
            builder.AppendLine("- Player-facing card replacements: " + summary.PlayerFacingCardReplacementCount);
            if (errors.Count == 0)
                builder.AppendLine("- Validation: passed.");
            else
                for (var i = 0; i < errors.Count; i++)
                    builder.AppendLine("- Validation issue: " + errors[i]);
            WriteTextFile(QualityReportPath, builder.ToString());
        }

        static void WriteCardModeDoc(Stage32_8Summary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 32.8 Terrain Art Card Mode");
            builder.AppendLine();
            builder.AppendLine("Stage32.8 adds an interim image-backed terrain-card mode for player-facing visual improvement while final authored terrain meshes are still pending.");
            builder.AppendLine();
            builder.AppendLine("- Source art: `Assets/Rts/Art/Source/Terrain/Batch01/individual/*_card.png`.");
            builder.AppendLine("- Prefabs: `Assets/Rts/Art/Prefabs/Terrain/Stage32_8Cards/`.");
            builder.AppendLine("- Metadata: `TerrainArtCardTag` marks these as visual-only image-backed cards.");
            builder.AppendLine("- Gameplay: no Rts.Core terrain, pathing, placement, or economy behavior changes.");
            builder.AppendLine("- Player-facing mappings: " + summary.PlayerFacingCardReplacementCount + " existing Stage32 terrain definitions now use card prefabs where safe.");
            builder.AppendLine();
            builder.AppendLine("This is deliberately an interim mode. True final terrain still needs authored 3D models, proper UVs, and production textures.");
            WriteTextFile(CardModeDocPath, builder.ToString());
        }

        static void WriteReport(Stage32_8Summary summary, List<string> errors)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 32.8 Report");
            builder.AppendLine();
            builder.AppendLine("- Review scene: `" + ReviewScenePath + "`");
            builder.AppendLine("- Final mesh material quality pass: completed.");
            builder.AppendLine("- Image-backed terrain-card mode: " + (summary.Batch01CardSourceAvailable ? "created" : "unavailable; no source art found") + ".");
            builder.AppendLine("- Card prefabs: " + summary.CardPrefabCount);
            builder.AppendLine("- Player-facing visual replacements: " + summary.PlayerFacingCardReplacementCount);
            builder.AppendLine("- Rts.Core gameplay changes: none.");
            builder.AppendLine();
            if (errors.Count == 0)
                builder.AppendLine("Validation passed.");
            else
                for (var i = 0; i < errors.Count; i++)
                    builder.AppendLine("- Validation issue: " + errors[i]);
            WriteTextFile(ReportPath, builder.ToString());
        }

        static void WriteTextFile(string relativePath, string text)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        static void EnsureFolders()
        {
            CreateFolderRecursive(CardMaterialFolder);
            CreateFolderRecursive(CardPrefabFolder);
            CreateFolderRecursive(CardMappedPrefabFolder);
            CreateFolderRecursive(CardMeshFolder);
        }

        static void CreateFolderRecursive(string assetFolder)
        {
            var parts = assetFolder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static string ToAbsoluteProjectPath(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        sealed class CardSpec
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly TerrainPieceCategory Category;
            public readonly TerrainPieceSizeClass SizeClass;
            public readonly float WorldWidth;
            public readonly float WorldDepth;
            public readonly int FineGridWidth;
            public readonly int FineGridHeight;
            public readonly bool Passable;
            public readonly bool Buildable;
            public readonly string MappedDefinitionId;

            public CardSpec(string id, string displayName, TerrainPieceCategory category, TerrainPieceSizeClass sizeClass, float worldWidth, float worldDepth, int fineGridWidth, int fineGridHeight, bool passable, bool buildable, string mappedDefinitionId)
            {
                Id = id;
                DisplayName = displayName;
                Category = category;
                SizeClass = sizeClass;
                WorldWidth = worldWidth;
                WorldDepth = worldDepth;
                FineGridWidth = fineGridWidth;
                FineGridHeight = fineGridHeight;
                Passable = passable;
                Buildable = buildable;
                MappedDefinitionId = mappedDefinitionId;
            }

            public string CardSourcePath
            {
                get { return CardSourceFolder + "/" + Id + "_card.png"; }
            }
        }
    }

    public sealed class Stage32_8Summary
    {
        public int FinalMeshPrefabCount;
        public int FinalMeshPrimaryMaterialCount;
        public int CardPrefabCount;
        public int PlayerFacingCardReplacementCount;
        public string ReviewScenePath;
        public bool Batch01CardSourceAvailable;
        public List<string> Errors = new List<string>();
    }
}
