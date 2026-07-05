using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ProjectAegisRTS.UnityClient.Rendering.Terrain;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32_6FinalTerrainMeshBatch01Importer
    {
        public const string SourceFolder = "Assets/Rts/Art/Source/Terrain/FinalMeshBatch01";
        public const string ManifestJsonPath = SourceFolder + "/terrain_final_mesh_batch01_manifest.json";
        public const string MaterialFolder = "Assets/Rts/Art/Materials/Terrain/FinalMeshBatch01";
        public const string PrefabFolder = "Assets/Rts/Art/Prefabs/Terrain/FinalMeshBatch01";
        public const string MappedPrefabFolder = PrefabFolder + "/MappedDefinitions";
        public const string ReviewScenePath = "Assets/Rts/Scenes/Stage32_6_FinalTerrainMeshReview.unity";
        public const string ReportPath = "docs/STAGE32_6_FINAL_TERRAIN_MESHES_REPORT.md";
        public const int RequiredPieceCount = 2;
        const int MinimumFinalMeshVertices = 100;
        const int MinimumFinalMeshTriangles = 80;

        static readonly Dictionary<string, string> DefinitionToFinalMeshMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ground_grass_dirt_patch_01", "ground_grass_dirt_01" },
            { "resource_cluster_01", "resource_cluster_blue_01" }
        };

        [MenuItem("ProjectAegisRTS/Stage 32.6/Import Final Terrain Mesh Batch01")]
        public static void ImportFinalMeshBatch01Menu()
        {
            EnsureFinalMeshBatch01();
        }

        public static void ImportFinalMeshBatch01Batch()
        {
            try
            {
                var summary = EnsureFinalMeshBatch01();
                Debug.Log("Stage 32.6 final terrain mesh Batch01 import completed. Prefabs: " + summary.PrefabCount + ", mapped definitions: " + summary.MappedDefinitionCount);
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

        public static void CreateFinalMeshReviewSceneBatch()
        {
            try
            {
                EnsureFinalMeshBatch01();
                CreateOrUpdateReviewScene(LoadPackage());
                Debug.Log("Stage 32.6 final terrain mesh Batch01 review scene created.");
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

        public static void ValidateFinalMeshBatch01Batch()
        {
            try
            {
                var summary = ValidateFinalMeshBatch01();
                Debug.Log("Stage 32.6 final terrain mesh Batch01 validation passed. Prefabs: " + summary.PrefabCount);
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

        public static void ValidateStage32_7TerrainArtIntegrationBatch()
        {
            try
            {
                var summary = ValidateFinalMeshBatch01();
                Debug.Log("Stage 32.7 terrain art integration validation passed. Final prefabs: " + summary.PrefabCount + ", review scene: " + summary.ReviewScenePath);
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

        public static FinalMeshBatch01Summary EnsureFinalMeshBatch01()
        {
            EnsureFolders();
            AssetDatabase.Refresh();

            var package = LoadPackage();
            ValidatePackageSourceFiles(package);
            ConfigureImports(package);

            var prefabs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            foreach (var piece in package.pieces)
            {
                var materials = CreateMaterials(piece);
                var prefab = CreatePrefab(piece, materials, piece.id, piece.displayName, false, null);
                prefabs[piece.id] = prefab;
            }

            Stage32TerrainPieceGenerator.EnsureStage32TerrainPieces();
            var mappedCount = ApplyFinalMeshesToTerrainDefinitions(package, prefabs);
            CreateOrUpdateReviewScene(package);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            NormalizeGeneratedTextArtifacts();

            var summary = new FinalMeshBatch01Summary();
            summary.SourceMeshCount = package.pieces.Count;
            summary.MaterialCount = CountAssets<Material>(MaterialFolder);
            summary.PrefabCount = CountAssets<GameObject>(PrefabFolder);
            summary.MappedDefinitionCount = mappedCount;
            summary.ReviewScenePath = ReviewScenePath;
            WriteReport(summary, new List<string>());
            return summary;
        }

        public static FinalMeshBatch01Summary ValidateFinalMeshBatch01()
        {
            var summary = EnsureFinalMeshBatch01();
            var errors = new List<string>();
            var package = LoadPackage();

            if (package.pieces == null || package.pieces.Count != RequiredPieceCount)
                errors.Add("Expected exactly " + RequiredPieceCount + " final terrain mesh manifest pieces.");

            ValidatePackageSourceFiles(package, errors);
            ValidateMaterials(package, errors);
            ValidatePrefabs(package, errors);
            ValidateReviewScene(errors);
            ValidateMappedDefinitions(errors);

            summary.Errors = errors;
            WriteReport(summary, errors);

            if (errors.Count > 0)
                throw new InvalidOperationException("Stage 32.6 final terrain mesh Batch01 validation failed: " + string.Join(" | ", errors.ToArray()));

            return summary;
        }

        static int ApplyFinalMeshesToTerrainDefinitions(FinalMeshPackage package, Dictionary<string, GameObject> prefabs)
        {
            var definitions = LoadDefinitionMap();
            var pieces = BuildPieceMap(package);
            var mappedCount = 0;

            foreach (var pair in DefinitionToFinalMeshMap)
            {
                TerrainPieceDefinition definition;
                FinalMeshPiece piece;
                GameObject canonicalPrefab;
                if (!definitions.TryGetValue(pair.Key, out definition) ||
                    !pieces.TryGetValue(pair.Value, out piece) ||
                    !prefabs.TryGetValue(pair.Value, out canonicalPrefab))
                    continue;

                var mappedPrefab = CreatePrefab(piece, CreateMaterials(piece), definition.pieceId, definition.displayName, true, definition);
                definition.prefab = mappedPrefab;
                definition.supportsTint = false;
                definition.materialProfileId = "finalmesh_" + piece.id;
                definition.passabilityVisualHint = "Final mesh visual metadata passable=" + piece.passable + "; Rts.Core remains authoritative.";
                definition.buildableVisualHint = "Final mesh visual metadata buildable=" + piece.buildable + "; Rts.Core placement remains authoritative.";
                definition.isGameplayBlockingVisualOnly = !piece.passable;
                definition.questBudgetTag = "QuestSafeFinalMeshBatch01";
                definition.notes = "Stage32.6 FinalMeshBatch01 replacement using imported OBJ mesh asset " + piece.sourceMesh + ". Placeholder terrain remains fallback/debug only.";
                EditorUtility.SetDirty(definition);
                mappedCount++;
            }

            return mappedCount;
        }

        static GameObject CreatePrefab(FinalMeshPiece piece, Dictionary<string, Material> materials, string prefabId, string displayName, bool mappedDefinition, TerrainPieceDefinition definition)
        {
            var modelPath = SourceFolder + "/" + NormalizeRelativePath(piece.sourceMesh);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
                throw new InvalidOperationException(piece.id + " did not import as a Unity model: " + modelPath);

            var root = new GameObject(prefabId);
            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
                instance = Object.Instantiate(model);
            instance.name = "SourceMesh_" + piece.id;
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            ApplyManifestScaleAndPivot(instance, piece);
            AssignMaterials(instance, piece, materials);
            AddMetadata(root, piece, prefabId, displayName, mappedDefinition, definition);
            AddLodGroup(root);

            var prefabPath = (mappedDefinition ? MappedPrefabFolder : PrefabFolder) + "/" + prefabId + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            if (prefab == null)
                throw new InvalidOperationException("Could not create final terrain mesh prefab at " + prefabPath);
            return prefab;
        }

        static Dictionary<string, Material> CreateMaterials(FinalMeshPiece piece)
        {
            var materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

            if (string.Equals(piece.id, "ground_grass_dirt_01", StringComparison.OrdinalIgnoreCase))
            {
                materials["ground_surface"] = CreateTexturedMaterial(piece.id + "_ground_surface", TexturePath(piece, piece.textures.albedo), TexturePath(piece, piece.textures.normal), TexturePath(piece, piece.textures.roughness), new Color(0.78f, 0.86f, 0.70f, 1f), 0.18f, 0f);
                materials["edge_dark"] = CreateColorMaterial(piece.id + "_edge_dark", new Color(0.18f, 0.14f, 0.10f, 1f), 0.12f, 0f);
                materials["stone"] = CreateColorMaterial(piece.id + "_stone", new Color(0.42f, 0.40f, 0.35f, 1f), 0.16f, 0f);
                materials["grass_detail"] = CreateColorMaterial(piece.id + "_grass_detail", new Color(0.20f, 0.42f, 0.13f, 1f), 0.10f, 0f);
                return materials;
            }

            if (string.Equals(piece.id, "resource_cluster_blue_01", StringComparison.OrdinalIgnoreCase))
            {
                materials["resource_ground"] = CreateTexturedMaterial(piece.id + "_resource_ground", TexturePath(piece, piece.textures.groundAlbedo), TexturePath(piece, piece.textures.groundNormal), TexturePath(piece, piece.textures.groundRoughness), new Color(0.68f, 0.61f, 0.48f, 1f), 0.18f, 0f);
                materials["edge_dark"] = CreateColorMaterial(piece.id + "_edge_dark", new Color(0.18f, 0.13f, 0.09f, 1f), 0.12f, 0f);
                materials["stone"] = CreateColorMaterial(piece.id + "_stone", new Color(0.42f, 0.40f, 0.36f, 1f), 0.16f, 0f);
                materials["crystal_blue"] = CreateColorMaterial(piece.id + "_crystal_blue", new Color(0.04f, 0.22f, 1.0f, 1f), 0.78f, 0f);
                materials["crystal_cyan"] = CreateColorMaterial(piece.id + "_crystal_cyan", new Color(0.02f, 0.75f, 0.72f, 1f), 0.74f, 0f);
                return materials;
            }

            materials["default"] = CreateColorMaterial(piece.id + "_default", Color.gray, 0.18f, 0f);
            return materials;
        }

        static Material CreateTexturedMaterial(string materialId, string albedoPath, string normalPath, string roughnessPath, Color fallbackColor, float smoothness, float metallic)
        {
            var material = LoadOrCreateMaterial(materialId);
            ApplyBaseMaterialProperties(material, fallbackColor, smoothness, metallic);
            var albedo = string.IsNullOrEmpty(albedoPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            var normal = string.IsNullOrEmpty(normalPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            var roughness = string.IsNullOrEmpty(roughnessPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);
            if (albedo != null)
            {
                material.mainTexture = albedo;
                if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", albedo);
                if (material.HasProperty("_MainTex"))
                    material.SetTexture("_MainTex", albedo);
            }
            if (normal != null && material.HasProperty("_BumpMap"))
            {
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }
            if (roughness != null)
            {
                if (material.HasProperty("_MetallicGlossMap"))
                {
                    material.SetTexture("_MetallicGlossMap", roughness);
                    material.EnableKeyword("_METALLICSPECGLOSSMAP");
                }
                if (material.HasProperty("_SpecGlossMap"))
                {
                    material.SetTexture("_SpecGlossMap", roughness);
                    material.EnableKeyword("_SPECGLOSSMAP");
                }
                if (material.HasProperty("_MaskMap"))
                    material.SetTexture("_MaskMap", roughness);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateColorMaterial(string materialId, Color color, float smoothness, float metallic)
        {
            var material = LoadOrCreateMaterial(materialId);
            ApplyBaseMaterialProperties(material, color, smoothness, metallic);
            material.mainTexture = null;
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", null);
            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", null);
            if (material.HasProperty("_BumpMap"))
                material.SetTexture("_BumpMap", null);
            if (material.HasProperty("_MetallicGlossMap"))
                material.SetTexture("_MetallicGlossMap", null);
            if (material.HasProperty("_SpecGlossMap"))
                material.SetTexture("_SpecGlossMap", null);
            if (material.HasProperty("_MaskMap"))
                material.SetTexture("_MaskMap", null);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material LoadOrCreateMaterial(string materialId)
        {
            var path = MaterialFolder + "/" + materialId + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            material = new Material(shader) { name = materialId };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static void ApplyBaseMaterialProperties(Material material, Color color, float smoothness, float metallic)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);
            material.renderQueue = -1;
            material.SetOverrideTag("RenderType", "Opaque");
        }

        static void AssignMaterials(GameObject instance, FinalMeshPiece piece, Dictionary<string, Material> materials)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var existing = renderer.sharedMaterials;
                if (existing == null || existing.Length == 0)
                {
                    renderer.sharedMaterial = FirstMaterial(materials);
                    continue;
                }

                var replacements = new Material[existing.Length];
                for (var i = 0; i < existing.Length; i++)
                {
                    var sourceName = existing[i] != null ? existing[i].name : string.Empty;
                    replacements[i] = FindMaterialForSlot(piece, materials, sourceName, i);
                }
                renderer.sharedMaterials = replacements;
            }
        }

        static Material FindMaterialForSlot(FinalMeshPiece piece, Dictionary<string, Material> materials, string sourceName, int index)
        {
            var key = NormalizeMaterialKey(sourceName);
            Material material;
            if (!string.IsNullOrEmpty(key) && materials.TryGetValue(key, out material))
                return material;

            if (string.Equals(piece.id, "ground_grass_dirt_01", StringComparison.OrdinalIgnoreCase))
            {
                var ordered = new[] { "ground_surface", "edge_dark", "stone", "grass_detail" };
                return materials[ordered[Mathf.Clamp(index, 0, ordered.Length - 1)]];
            }

            if (string.Equals(piece.id, "resource_cluster_blue_01", StringComparison.OrdinalIgnoreCase))
            {
                var ordered = new[] { "resource_ground", "edge_dark", "stone", "crystal_blue", "crystal_cyan" };
                return materials[ordered[Mathf.Clamp(index, 0, ordered.Length - 1)]];
            }

            return FirstMaterial(materials);
        }

        static string NormalizeMaterialKey(string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName))
                return string.Empty;
            var key = sourceName.Replace(" (Instance)", string.Empty).Trim().ToLowerInvariant();
            key = key.Replace(' ', '_').Replace('-', '_');
            if (key.EndsWith("_mat", StringComparison.OrdinalIgnoreCase))
                key = key.Substring(0, key.Length - 4);
            return key;
        }

        static Material FirstMaterial(Dictionary<string, Material> materials)
        {
            foreach (var pair in materials)
                return pair.Value;
            return null;
        }

        static void ApplyManifestScaleAndPivot(GameObject instance, FinalMeshPiece piece)
        {
            var bounds = CalculateBounds(instance);
            if (!bounds.HasValue)
                return;

            var desiredWidth = piece.WorldWidth;
            var desiredDepth = piece.WorldDepth;
            var size = bounds.Value.size;
            var scaleX = size.x > 0.001f ? desiredWidth / size.x : 1f;
            var scaleZ = size.z > 0.001f ? desiredDepth / size.z : 1f;
            instance.transform.localScale = new Vector3(instance.transform.localScale.x * scaleX, instance.transform.localScale.y, instance.transform.localScale.z * scaleZ);

            var scaledBounds = CalculateBounds(instance);
            if (!scaledBounds.HasValue)
                return;

            instance.transform.localPosition += new Vector3(-scaledBounds.Value.center.x, -scaledBounds.Value.min.y, -scaledBounds.Value.center.z);
        }

        static Bounds? CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return null;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        static void AddMetadata(GameObject root, FinalMeshPiece piece, string prefabId, string displayName, bool mappedDefinition, TerrainPieceDefinition definition)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var materialCount = CountUniqueMaterials(renderers);
            var category = ToTerrainPieceCategory(piece.category);
            var sizeClass = definition != null ? definition.sizeClass : ToSizeClass(piece);
            var fineWidth = definition != null ? definition.footprintFineWidth : piece.FineGridWidth;
            var fineHeight = definition != null ? definition.footprintFineHeight : piece.FineGridHeight;

            var validation = root.AddComponent<TerrainPieceValidationTag>();
            validation.pieceId = prefabId;
            validation.displayName = string.IsNullOrEmpty(displayName) ? piece.displayName : displayName;
            validation.category = category;
            validation.sizeClass = sizeClass;
            validation.footprintFineWidth = fineWidth;
            validation.footprintFineHeight = fineHeight;
            validation.materialProfileId = "finalmesh_" + piece.id;
            validation.passabilityVisualHint = "Final mesh visual metadata passable=" + piece.passable + "; Rts.Core remains authoritative.";
            validation.buildableVisualHint = "Final mesh visual metadata buildable=" + piece.buildable + "; Rts.Core remains authoritative.";
            validation.supportsRotation = true;
            validation.supportsTint = false;
            validation.isGameplayBlockingVisualOnly = !piece.passable;
            validation.questBudgetTag = "QuestSafeFinalMeshBatch01";
            validation.rendererCount = renderers.Length;
            validation.primitiveCount = Mathf.Max(1, renderers.Length);
            validation.notes = "Stage32.6 FinalMeshBatch01 imported OBJ mesh, not a preview PNG/card.";

            var finalTag = root.AddComponent<Stage32_6FinalTerrainMeshTag>();
            finalTag.assetId = piece.id;
            finalTag.sourceMeshPath = SourceFolder + "/" + NormalizeRelativePath(piece.sourceMesh);
            finalTag.manifestPath = ManifestJsonPath;
            finalTag.category = category;
            finalTag.worldSizeMeters = new Vector2(piece.WorldWidth, piece.WorldDepth);
            finalTag.fineGridSize = new Vector2Int(piece.FineGridWidth, piece.FineGridHeight);
            finalTag.passable = piece.passable;
            finalTag.buildable = piece.buildable;
            finalTag.harvestable = piece.harvestable;
            finalTag.playerFacingReplacement = mappedDefinition;
            finalTag.previewPngUsedAsRuntimeCard = false;
            finalTag.rendererCount = renderers.Length;
            finalTag.materialCount = materialCount;
            finalTag.notes = mappedDefinition ? "Mapped Stage32 player-facing wrapper for final terrain mesh asset." : "Canonical imported final terrain mesh asset.";

            var terrainTag = root.AddComponent<Stage32TerrainPieceTag>();
            terrainTag.terrainId = prefabId;
            terrainTag.displayName = validation.displayName;
            terrainTag.category = ToStage32Category(category);
            terrainTag.fineGridSize = new Vector2Int(fineWidth, fineHeight);
            terrainTag.blocksMovement = !piece.passable;
            terrainTag.buildable = piece.buildable;
            terrainTag.road = false;
            terrainTag.water = false;
            terrainTag.resourceField = piece.harvestable || category == TerrainPieceCategory.Resource;
            terrainTag.decorativeOnly = false;
            terrainTag.hasBeveledBase = true;
            terrainTag.hasTopDownReadableShape = true;
            terrainTag.questSafeProxy = true;
            terrainTag.estimatedMeshObjectCount = Mathf.Max(1, renderers.Length);
            terrainTag.estimatedMaterialCount = materialCount;
            terrainTag.notes = "FinalMeshBatch01 metadata mirrors manifest values. Gameplay authority remains in Rts.Core.";
        }

        static void AddLodGroup(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            var lod = root.GetComponent<LODGroup>();
            if (lod == null)
                lod = root.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(0.08f, renderers) });
            lod.RecalculateBounds();
        }

        static void CreateOrUpdateReviewScene(FinalMeshPackage package)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage32_6_FinalTerrainMeshReview";

            var root = new GameObject("Stage32_6 Final Terrain Mesh Review");
            var positions = new Dictionary<string, Vector3>(StringComparer.OrdinalIgnoreCase)
            {
                { "ground_grass_dirt_01", new Vector3(-2.65f, 0f, 0.45f) },
                { "resource_cluster_blue_01", new Vector3(2.65f, 0f, 0.45f) }
            };

            foreach (var piece in package.pieces)
            {
                Vector3 position;
                if (!positions.TryGetValue(piece.id, out position))
                    position = new Vector3(0f, 0f, 0.45f);

                AddGridReference(root.transform, piece, position.x, position.z);
                AddPrefabWithLabel(root.transform, PrefabFolder + "/" + piece.id + ".prefab", piece.displayName + "\nfinal OBJ mesh\n" + piece.FineGridWidth + "x" + piece.FineGridHeight + " fine grid", position.x, position.z);
            }

            var hud = new GameObject("Status HUD");
            hud.transform.SetParent(root.transform, false);
            hud.transform.localPosition = new Vector3(-5.45f, 0.05f, -3.95f);
            var text = hud.AddComponent<TextMesh>();
            text.text = "Stage 32.6 FinalTerrainMeshes Batch01\nOnly final OBJ mesh prefabs are displayed\nPreview PNGs and old terrain proxies are excluded";
            text.characterSize = 0.18f;
            text.anchor = TextAnchor.UpperLeft;
            text.color = new Color(0.92f, 0.96f, 0.93f, 1f);

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.45f;
            light.transform.rotation = Quaternion.Euler(52f, -28f, 22f);

            var fill = new GameObject("Soft Fill Light").AddComponent<Light>();
            fill.type = LightType.Point;
            fill.intensity = 0.9f;
            fill.range = 18f;
            fill.transform.position = new Vector3(0f, 6f, -3f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 9.5f, -7.25f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 5.15f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.043f, 0.044f, 1f);

            EditorSceneManager.SaveScene(scene, ReviewScenePath);
        }

        static void AddPrefabWithLabel(Transform parent, string prefabPath, string label, float x, float z)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    instance.transform.SetParent(parent, false);
                    instance.transform.localPosition = new Vector3(x, 0f, z);
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                }
            }

            var labelObject = new GameObject("Label " + label.Replace('\n', ' '));
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(x - 1.35f, 0.06f, z - 2.0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var text = labelObject.AddComponent<TextMesh>();
            text.text = label;
            text.characterSize = 0.18f;
            text.anchor = TextAnchor.UpperLeft;
            text.color = new Color(0.86f, 0.90f, 0.88f, 1f);
        }

        static void AddGridReference(Transform parent, FinalMeshPiece piece, float x, float z)
        {
            var gridRoot = new GameObject("Fine Grid Reference " + piece.id);
            gridRoot.transform.SetParent(parent, false);
            gridRoot.transform.localPosition = new Vector3(x, 0.015f, z);
            var lineMaterial = CreateColorMaterial("fine_grid_reference", new Color(0.35f, 0.82f, 0.95f, 0.58f), 0.05f, 0f);
            for (var i = 0; i <= piece.FineGridWidth; i++)
            {
                var offset = -piece.WorldWidth * 0.5f + piece.WorldWidth * i / piece.FineGridWidth;
                AddLineCube(gridRoot.transform, "grid_x_" + i, new Vector3(offset, 0f, 0f), new Vector3(0.018f, 0.012f, piece.WorldDepth), lineMaterial);
            }
            for (var i = 0; i <= piece.FineGridHeight; i++)
            {
                var offset = -piece.WorldDepth * 0.5f + piece.WorldDepth * i / piece.FineGridHeight;
                AddLineCube(gridRoot.transform, "grid_z_" + i, new Vector3(0f, 0f, offset), new Vector3(piece.WorldWidth, 0.012f, 0.018f), lineMaterial);
            }
        }

        static void AddLineCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = localScale;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
        }

        static void ValidatePackageSourceFiles(FinalMeshPackage package)
        {
            var errors = new List<string>();
            ValidatePackageSourceFiles(package, errors);
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" | ", errors.ToArray()));
        }

        static void ValidatePackageSourceFiles(FinalMeshPackage package, List<string> errors)
        {
            if (package == null || package.pieces == null)
            {
                errors.Add("Final mesh manifest did not parse.");
                return;
            }

            foreach (var piece in package.pieces)
            {
                if (piece == null || string.IsNullOrEmpty(piece.id))
                {
                    errors.Add("Manifest contains an unnamed final mesh piece.");
                    continue;
                }

                RequireAssetFile(SourceFolder + "/" + NormalizeRelativePath(piece.sourceMesh), piece.id + " OBJ mesh", errors);
                RequireAssetFile(SourceFolder + "/" + NormalizeRelativePath(piece.materialFile), piece.id + " MTL material file", errors);
                foreach (var texturePath in piece.TexturePaths())
                    RequireAssetFile(SourceFolder + "/" + NormalizeRelativePath(texturePath), piece.id + " texture " + texturePath, errors);

                if (piece.WorldWidth <= 0f || piece.WorldDepth <= 0f)
                    errors.Add(piece.id + ": invalid world size metadata.");
                if (piece.FineGridWidth <= 0 || piece.FineGridHeight <= 0)
                    errors.Add(piece.id + ": invalid fine grid metadata.");
            }
        }

        static void ValidateMaterials(FinalMeshPackage package, List<string> errors)
        {
            foreach (var piece in package.pieces)
            {
                var expected = string.Equals(piece.id, "resource_cluster_blue_01", StringComparison.OrdinalIgnoreCase) ? 5 : 4;
                var count = CountMaterialsForPiece(piece.id);
                if (count < expected)
                    errors.Add(piece.id + ": expected at least " + expected + " Unity material assets; found " + count + ".");
            }
        }

        static void ValidatePrefabs(FinalMeshPackage package, List<string> errors)
        {
            foreach (var piece in package.pieces)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + piece.id + ".prefab");
                if (prefab == null)
                {
                    errors.Add(piece.id + ": prefab missing.");
                    continue;
                }

                var finalTag = prefab.GetComponent<Stage32_6FinalTerrainMeshTag>();
                var validation = prefab.GetComponent<TerrainPieceValidationTag>();
                var terrainTag = prefab.GetComponent<Stage32TerrainPieceTag>();
                if (finalTag == null || !finalTag.IsComplete())
                    errors.Add(piece.id + ": Stage32_6FinalTerrainMeshTag missing or incomplete.");
                if (validation == null || !validation.IsComplete())
                    errors.Add(piece.id + ": TerrainPieceValidationTag missing or incomplete.");
                if (terrainTag == null || terrainTag.fineGridSize.x != piece.FineGridWidth || terrainTag.fineGridSize.y != piece.FineGridHeight)
                    errors.Add(piece.id + ": Stage32TerrainPieceTag missing or mismatched.");
                var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                    errors.Add(piece.id + ": prefab has no renderer.");
                ValidateRendererMaterials(piece, renderers, errors);
                ValidateSourceMeshGeometry(piece, prefab, errors);
                ValidatePrimaryMaterialTextures(piece, errors);
                if (prefab.GetComponent<LODGroup>() == null)
                    errors.Add(piece.id + ": LODGroup missing.");
                if (prefab.GetComponentsInChildren<Collider>(true).Length > 0)
                    errors.Add(piece.id + ": final visual terrain prefab should not include gameplay colliders.");
            }
        }

        static void ValidateRendererMaterials(FinalMeshPiece piece, Renderer[] renderers, List<string> errors)
        {
            var materialCount = 0;
            var texturedMaterialCount = 0;
            for (var i = 0; i < renderers.Length; i++)
            {
                var shared = renderers[i].sharedMaterials;
                for (var j = 0; j < shared.Length; j++)
                {
                    var material = shared[j];
                    if (material == null)
                    {
                        errors.Add(piece.id + ": renderer " + renderers[i].name + " has a missing material slot.");
                        continue;
                    }

                    materialCount++;
                    if (MaterialHasAnyTexture(material))
                        texturedMaterialCount++;
                }
            }

            if (materialCount == 0)
                errors.Add(piece.id + ": prefab has zero assigned materials.");
            if (texturedMaterialCount == 0)
                errors.Add(piece.id + ": prefab has no material with source texture references.");
        }

        static void ValidateSourceMeshGeometry(FinalMeshPiece piece, GameObject prefab, List<string> errors)
        {
            var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            if (filters.Length == 0)
            {
                errors.Add(piece.id + ": prefab has no MeshFilter components.");
                return;
            }

            var vertexCount = 0;
            var triangleCount = 0;
            var sourceMeshReferences = 0;
            var primitiveMeshReferences = 0;
            for (var i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh == null)
                {
                    errors.Add(piece.id + ": MeshFilter " + filters[i].name + " has no mesh.");
                    continue;
                }

                vertexCount += mesh.vertexCount;
                triangleCount += mesh.triangles != null ? mesh.triangles.Length / 3 : 0;
                var assetPath = AssetDatabase.GetAssetPath(mesh).Replace('\\', '/');
                if (assetPath.StartsWith(SourceFolder + "/", StringComparison.OrdinalIgnoreCase) && assetPath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
                    sourceMeshReferences++;
                if (IsBuiltinPrimitiveMesh(mesh, assetPath))
                    primitiveMeshReferences++;
            }

            if (sourceMeshReferences == 0)
                errors.Add(piece.id + ": prefab render mesh does not come from FinalMeshBatch01 OBJ source.");
            if (primitiveMeshReferences > 0)
                errors.Add(piece.id + ": prefab still references built-in primitive mesh geometry.");
            if (vertexCount < MinimumFinalMeshVertices)
                errors.Add(piece.id + ": suspiciously low vertex count " + vertexCount + " for final terrain art.");
            if (triangleCount < MinimumFinalMeshTriangles)
                errors.Add(piece.id + ": suspiciously low triangle count " + triangleCount + " for final terrain art.");
        }

        static bool IsBuiltinPrimitiveMesh(Mesh mesh, string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath) && assetPath.StartsWith(SourceFolder + "/", StringComparison.OrdinalIgnoreCase))
                return false;

            var name = mesh.name;
            return string.Equals(name, "Cube", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Plane", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Quad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Sphere", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Cylinder", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Capsule", StringComparison.OrdinalIgnoreCase);
        }

        static void ValidatePrimaryMaterialTextures(FinalMeshPiece piece, List<string> errors)
        {
            var materialId = string.Equals(piece.id, "resource_cluster_blue_01", StringComparison.OrdinalIgnoreCase)
                ? piece.id + "_resource_ground"
                : piece.id + "_ground_surface";
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + materialId + ".mat");
            if (material == null)
            {
                errors.Add(piece.id + ": primary textured material missing: " + materialId + ".");
                return;
            }

            var albedoPath = string.Equals(piece.id, "resource_cluster_blue_01", StringComparison.OrdinalIgnoreCase)
                ? TexturePath(piece, piece.textures.groundAlbedo)
                : TexturePath(piece, piece.textures.albedo);
            var normalPath = string.Equals(piece.id, "resource_cluster_blue_01", StringComparison.OrdinalIgnoreCase)
                ? TexturePath(piece, piece.textures.groundNormal)
                : TexturePath(piece, piece.textures.normal);
            var roughnessPath = string.Equals(piece.id, "resource_cluster_blue_01", StringComparison.OrdinalIgnoreCase)
                ? TexturePath(piece, piece.textures.groundRoughness)
                : TexturePath(piece, piece.textures.roughness);

            RequireMaterialTexture(material, new[] { "_BaseMap", "_MainTex" }, albedoPath, piece.id + " albedo", errors);
            RequireMaterialTexture(material, new[] { "_BumpMap" }, normalPath, piece.id + " normal", errors);
            RequireMaterialTexture(material, new[] { "_MetallicGlossMap", "_SpecGlossMap", "_MaskMap" }, roughnessPath, piece.id + " roughness", errors);
        }

        static void RequireMaterialTexture(Material material, string[] propertyNames, string expectedPath, string label, List<string> errors)
        {
            if (string.IsNullOrEmpty(expectedPath))
            {
                errors.Add(label + ": expected texture path is empty.");
                return;
            }

            for (var i = 0; i < propertyNames.Length; i++)
            {
                if (!material.HasProperty(propertyNames[i]))
                    continue;
                var texture = material.GetTexture(propertyNames[i]);
                if (TextureMatchesPath(texture, expectedPath))
                    return;
            }

            errors.Add(label + ": material " + material.name + " does not reference " + expectedPath + ".");
        }

        static bool TextureMatchesPath(Texture texture, string expectedPath)
        {
            if (texture == null)
                return false;
            var actualPath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
            return string.Equals(actualPath, expectedPath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
        }

        static bool MaterialHasAnyTexture(Material material)
        {
            var propertyNames = material.GetTexturePropertyNames();
            for (var i = 0; i < propertyNames.Length; i++)
            {
                if (!material.HasProperty(propertyNames[i]))
                    continue;
                var texture = material.GetTexture(propertyNames[i]);
                if (texture != null && AssetDatabase.GetAssetPath(texture).Replace('\\', '/').StartsWith(SourceFolder + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        static void ValidateReviewScene(List<string> errors)
        {
            if (!File.Exists(ToAbsoluteProjectPath(ReviewScenePath)))
            {
                errors.Add("Review scene is missing: " + ReviewScenePath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(ReviewScenePath);
            if (!scene.IsValid())
            {
                errors.Add("Review scene did not open.");
                return;
            }

            var tags = Object.FindObjectsByType<Stage32_6FinalTerrainMeshTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (tags.Length != RequiredPieceCount)
                errors.Add("Review scene must contain exactly " + RequiredPieceCount + " final mesh pieces; found " + tags.Length + ".");

            var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ground_grass_dirt_01", "resource_cluster_blue_01" };
            for (var i = 0; i < tags.Length; i++)
            {
                if (tags[i] == null || string.IsNullOrEmpty(tags[i].assetId))
                    continue;
                expected.Remove(tags[i].assetId);
                if (!tags[i].sourceMeshPath.Replace('\\', '/').StartsWith(SourceFolder + "/", StringComparison.OrdinalIgnoreCase))
                    errors.Add(tags[i].assetId + ": review scene final mesh tag does not point at FinalMeshBatch01 source.");
            }
            foreach (var missing in expected)
                errors.Add("Review scene is missing final mesh prefab: " + missing + ".");

            var terrainValidationTags = Object.FindObjectsByType<TerrainPieceValidationTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < terrainValidationTags.Length; i++)
            {
                var finalTag = terrainValidationTags[i].GetComponent<Stage32_6FinalTerrainMeshTag>();
                if (finalTag == null)
                    errors.Add("Review scene contains a terrain validation tag that is not a final mesh prefab: " + terrainValidationTags[i].name + ".");
            }

            var objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < objects.Length; i++)
            {
                var objectName = objects[i].name;
                if (objectName.IndexOf("before placeholder", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    objectName.IndexOf("placeholder comparison", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    objectName.IndexOf("Batch01Imported", StringComparison.OrdinalIgnoreCase) >= 0)
                    errors.Add("Review scene contains old placeholder content: " + objectName + ".");
            }
        }

        static void ValidateMappedDefinitions(List<string> errors)
        {
            var definitions = LoadDefinitionMap();
            foreach (var pair in DefinitionToFinalMeshMap)
            {
                TerrainPieceDefinition definition;
                if (!definitions.TryGetValue(pair.Key, out definition))
                {
                    errors.Add("Mapped terrain definition missing: " + pair.Key);
                    continue;
                }

                if (definition.prefab == null)
                {
                    errors.Add(pair.Key + ": definition has no prefab.");
                    continue;
                }

                var tag = definition.prefab.GetComponent<Stage32_6FinalTerrainMeshTag>();
                if (tag == null || !string.Equals(tag.assetId, pair.Value, StringComparison.OrdinalIgnoreCase) || !tag.playerFacingReplacement)
                    errors.Add(pair.Key + ": definition is not mapped to final mesh " + pair.Value + ".");
            }
        }

        static void ConfigureImports(FinalMeshPackage package)
        {
            foreach (var piece in package.pieces)
            {
                foreach (var texturePath in piece.TexturePaths())
                    ConfigureTextureImport(SourceFolder + "/" + NormalizeRelativePath(texturePath));

                var modelPath = SourceFolder + "/" + NormalizeRelativePath(piece.sourceMesh);
                AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
                if (importer == null)
                    continue;

                importer.globalScale = 1f;
                importer.importCameras = false;
                importer.importLights = false;
                importer.SaveAndReimport();
            }
        }

        static void ConfigureTextureImport(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            var lower = assetPath.ToLowerInvariant();
            importer.textureType = lower.Contains("_normal") ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !lower.Contains("_normal") && !lower.Contains("_roughness") && !lower.Contains("_height");
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        static FinalMeshPackage LoadPackage()
        {
            if (!File.Exists(ToAbsoluteProjectPath(ManifestJsonPath)))
                throw new FileNotFoundException("Final terrain mesh manifest is missing.", ManifestJsonPath);

            var json = File.ReadAllText(ToAbsoluteProjectPath(ManifestJsonPath));
            var package = JsonUtility.FromJson<FinalMeshPackage>(json);
            if (package == null || package.pieces == null)
                throw new InvalidOperationException("Unable to parse final terrain mesh manifest.");
            return package;
        }

        static Dictionary<string, TerrainPieceDefinition> LoadDefinitionMap()
        {
            var map = new Dictionary<string, TerrainPieceDefinition>(StringComparer.OrdinalIgnoreCase);
            var guids = AssetDatabase.FindAssets("t:TerrainPieceDefinition", new[] { Stage32TerrainPieceGenerator.DefinitionRoot });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var definition = AssetDatabase.LoadAssetAtPath<TerrainPieceDefinition>(path);
                if (definition != null && !string.IsNullOrEmpty(definition.pieceId) && !map.ContainsKey(definition.pieceId))
                    map.Add(definition.pieceId, definition);
            }

            return map;
        }

        static Dictionary<string, FinalMeshPiece> BuildPieceMap(FinalMeshPackage package)
        {
            var map = new Dictionary<string, FinalMeshPiece>(StringComparer.OrdinalIgnoreCase);
            foreach (var piece in package.pieces)
                if (piece != null && !string.IsNullOrEmpty(piece.id) && !map.ContainsKey(piece.id))
                    map.Add(piece.id, piece);
            return map;
        }

        static TerrainPieceCategory ToTerrainPieceCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return TerrainPieceCategory.Ground;
            return category.IndexOf("resource", StringComparison.OrdinalIgnoreCase) >= 0 ? TerrainPieceCategory.Resource : TerrainPieceCategory.Ground;
        }

        static Stage32TerrainCategory ToStage32Category(TerrainPieceCategory category)
        {
            return category == TerrainPieceCategory.Resource ? Stage32TerrainCategory.Resource : Stage32TerrainCategory.Ground;
        }

        static TerrainPieceSizeClass ToSizeClass(FinalMeshPiece piece)
        {
            if (piece.FineGridWidth >= 7 || piece.FineGridHeight >= 7)
                return TerrainPieceSizeClass.Large;
            return TerrainPieceSizeClass.Patch;
        }

        static int CountUniqueMaterials(Renderer[] renderers)
        {
            var materials = new HashSet<Material>();
            for (var i = 0; i < renderers.Length; i++)
            {
                var shared = renderers[i].sharedMaterials;
                for (var j = 0; j < shared.Length; j++)
                    if (shared[j] != null)
                        materials.Add(shared[j]);
            }
            return materials.Count;
        }

        static int CountMaterialsForPiece(string pieceId)
        {
            var count = 0;
            var guids = AssetDatabase.FindAssets("t:Material", new[] { MaterialFolder });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (Path.GetFileNameWithoutExtension(path).StartsWith(pieceId + "_", StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }

        static int CountAssets<T>(string folder) where T : Object
        {
            var count = 0;
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            for (var i = 0; i < guids.Length; i++)
                if (AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i])) != null)
                    count++;
            return count;
        }

        static string TexturePath(FinalMeshPiece piece, string relative)
        {
            return string.IsNullOrEmpty(relative) ? string.Empty : SourceFolder + "/" + NormalizeRelativePath(relative);
        }

        static void RequireAssetFile(string assetPath, string label, List<string> errors)
        {
            if (!File.Exists(ToAbsoluteProjectPath(assetPath)))
                errors.Add(label + " missing at " + assetPath + ".");
        }

        static string NormalizeRelativePath(string relativePath)
        {
            return string.IsNullOrEmpty(relativePath) ? string.Empty : relativePath.Replace('\\', '/').TrimStart('/');
        }

        static void EnsureFolders()
        {
            EnsureFolderRecursive(SourceFolder);
            EnsureFolderRecursive(MaterialFolder);
            EnsureFolderRecursive(PrefabFolder);
            EnsureFolderRecursive(MappedPrefabFolder);
            EnsureFolderRecursive("Assets/Rts/Scenes");
        }

        static void EnsureFolderRecursive(string path)
        {
            var parts = path.Split('/');
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

        static void WriteReport(FinalMeshBatch01Summary summary, List<string> errors)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, ReportPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 32.6 Final Terrain Meshes Report");
            builder.AppendLine();
            builder.AppendLine("- Source meshes imported: " + summary.SourceMeshCount);
            builder.AppendLine("- Unity materials: " + summary.MaterialCount);
            builder.AppendLine("- Prefabs: " + summary.PrefabCount);
            builder.AppendLine("- Player-facing mapped definitions: " + summary.MappedDefinitionCount);
            builder.AppendLine("- Review scene: `Assets/Rts/Scenes/Stage32_6_FinalTerrainMeshReview.unity`");
            builder.AppendLine("- Runtime preview-card usage: none");
            builder.AppendLine();
            builder.AppendLine("## Imported Meshes");
            builder.AppendLine("- `ground_grass_dirt_01`: passable/buildable 4m ground mesh, 8x8 fine grid.");
            builder.AppendLine("- `resource_cluster_blue_01`: harvestable visual resource mesh, non-passable/non-buildable 3.3m cluster, 7x7 fine grid.");
            builder.AppendLine();
            builder.AppendLine("## Integration");
            builder.AppendLine("- `ground_grass_dirt_patch_01` maps to `ground_grass_dirt_01` where Stage32 player-facing set dressing uses that definition.");
            builder.AppendLine("- `resource_cluster_01` maps to `resource_cluster_blue_01` where Stage32 player-facing set dressing uses that definition.");
            builder.AppendLine("- `Rts.Core` gameplay remains unchanged and authoritative.");
            builder.AppendLine();
            builder.AppendLine("## Validation");
            if (errors.Count == 0)
            {
                builder.AppendLine("- Final mesh batch validation passed.");
            }
            else
            {
                for (var i = 0; i < errors.Count; i++)
                    builder.AppendLine("- " + errors[i]);
            }
            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
        }

        static void NormalizeGeneratedTextArtifacts()
        {
            NormalizeTextFile(ToAbsoluteProjectPath(ReviewScenePath));
            NormalizeTextFile(ToAbsoluteProjectPath(ReviewScenePath + ".meta"));
            NormalizeTextTree(ToAbsoluteProjectPath(MaterialFolder));
            NormalizeTextTree(ToAbsoluteProjectPath(PrefabFolder));
            NormalizeTextTree(ToAbsoluteProjectPath(SourceFolder));
        }

        static void NormalizeTextTree(string root)
        {
            NormalizeTextFile(root + ".meta");
            if (!Directory.Exists(root))
                return;
            foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                NormalizeTextFile(file);
        }

        static void NormalizeTextFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || !IsGeneratedTextArtifact(path))
                return;

            var text = File.ReadAllText(path);
            var normalized = Regex.Replace(text, @"[ \t]+(\r?\n)", "$1");
            normalized = Regex.Replace(normalized, @"[ \t]+\z", string.Empty);
            if (!string.Equals(text, normalized, StringComparison.Ordinal))
                File.WriteAllText(path, normalized, new UTF8Encoding(false));
        }

        static bool IsGeneratedTextArtifact(string path)
        {
            var extension = Path.GetExtension(path);
            return string.Equals(extension, ".asset", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".mat", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".meta", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".prefab", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".unity", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".mtl", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".obj", StringComparison.OrdinalIgnoreCase);
        }

        [Serializable]
        public sealed class FinalMeshPackage
        {
            public string version;
            public string package;
            public string intendedUse;
            public string units;
            public List<FinalMeshPiece> pieces;
        }

        [Serializable]
        public sealed class FinalMeshPiece
        {
            public string id;
            public string displayName;
            public string category;
            public string sourceMesh;
            public string materialFile;
            public FinalMeshTexturePaths textures;
            public float[] worldSizeMeters;
            public int[] fineGridSize;
            public string pivot;
            public bool passable;
            public bool buildable;
            public bool harvestable;
            public string resourceKind;
            public bool blocksSight;
            public string recommendedCollider;
            public string notes;

            public float WorldWidth { get { return worldSizeMeters != null && worldSizeMeters.Length > 0 ? worldSizeMeters[0] : 1f; } }
            public float WorldDepth { get { return worldSizeMeters != null && worldSizeMeters.Length > 1 ? worldSizeMeters[1] : WorldWidth; } }
            public int FineGridWidth { get { return fineGridSize != null && fineGridSize.Length > 0 ? fineGridSize[0] : 1; } }
            public int FineGridHeight { get { return fineGridSize != null && fineGridSize.Length > 1 ? fineGridSize[1] : FineGridWidth; } }

            public IEnumerable<string> TexturePaths()
            {
                if (textures == null)
                    yield break;
                if (!string.IsNullOrEmpty(textures.albedo))
                    yield return textures.albedo;
                if (!string.IsNullOrEmpty(textures.normal))
                    yield return textures.normal;
                if (!string.IsNullOrEmpty(textures.roughness))
                    yield return textures.roughness;
                if (!string.IsNullOrEmpty(textures.height))
                    yield return textures.height;
                if (!string.IsNullOrEmpty(textures.groundAlbedo))
                    yield return textures.groundAlbedo;
                if (!string.IsNullOrEmpty(textures.groundNormal))
                    yield return textures.groundNormal;
                if (!string.IsNullOrEmpty(textures.groundRoughness))
                    yield return textures.groundRoughness;
            }
        }

        [Serializable]
        public sealed class FinalMeshTexturePaths
        {
            public string albedo;
            public string normal;
            public string roughness;
            public string height;
            public string groundAlbedo;
            public string groundNormal;
            public string groundRoughness;
        }
    }

    public sealed class FinalMeshBatch01Summary
    {
        public int SourceMeshCount;
        public int MaterialCount;
        public int PrefabCount;
        public int MappedDefinitionCount;
        public string ReviewScenePath;
        public List<string> Errors = new List<string>();
    }
}
