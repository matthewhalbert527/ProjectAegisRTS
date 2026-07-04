using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32_6TerrainArtIntegrationCorrection
    {
        public const string RuntimeMaterialFolder = "Assets/Rts/Art/Materials/Terrain/Stage32_6Runtime";
        public const string RuntimePrefabFolder = "Assets/Rts/Art/Prefabs/Terrain/Stage32_6Runtime";
        public const string RuntimeMappedPrefabFolder = RuntimePrefabFolder + "/MappedDefinitions";
        public const string RuntimeMeshFolder = "Assets/Rts/Art/Meshes/Terrain/Stage32_6Runtime";
        public const string ReferenceFolder = "Assets/Rts/Art/References/Terrain/Stage32_6ArtDirection";
        public const string ReviewScenePath = "Assets/Rts/Scenes/Stage32_6_TerrainArtIntegrationReview.unity";
        public const string ReportPath = "docs/STAGE32_6_REPORT.md";
        public const string ReviewScreenshotPath = "build/screenshots/stage32_6/terrain_review.png";
        public const string PlayerFacingScreenshotPath = "build/screenshots/stage32_6/player_facing.png";

        static readonly string[] LegacyFlatCardFolders =
        {
            Stage32TerrainArtIngestionGenerator.GeneratedPrefabFolder,
            Stage32TerrainArtIngestionGenerator.GeneratedMaterialFolder,
            Stage32TerrainArtIngestionGenerator.GeneratedMeshFolder,
            Stage32TerrainArtIngestionGenerator.GeneratedTextureFolder
        };

        [MenuItem("ProjectAegisRTS/Stage 32.6/Generate Runtime Terrain Assets")]
        public static void GenerateStage32_6AssetsMenu()
        {
            EnsureStage32_6Assets();
        }

        public static void GenerateStage32_6AssetsBatch()
        {
            try
            {
                var summary = EnsureStage32_6Assets();
                Debug.Log("Stage 32.6 terrain art integration assets generated. Runtime prefabs: " + summary.RuntimePrefabCount);
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

        public static void CreateStage32_6ReviewSceneBatch()
        {
            try
            {
                EnsureStage32_6Assets();
                CreateOrUpdateReviewScene();
                Debug.Log("Stage 32.6 terrain art integration review scene created.");
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

        public static void ValidateStage32_6Batch()
        {
            try
            {
                var summary = ValidateStage32_6Assets();
                Debug.Log("Stage 32.6 terrain art integration validation passed. Runtime prefabs: " + summary.RuntimePrefabCount);
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

        public static void CaptureStage32_6ScreenshotsBatch()
        {
            try
            {
                EnsureStage32_6Assets();
                CreateOrUpdateReviewScene();
                CaptureScreenshots();
                Debug.Log("Stage 32.6 screenshot capture completed.");
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

        public static Stage32_6Summary EnsureStage32_6Assets()
        {
            EnsureFolders();
            var movedReferences = MoveBatch01SourceImagesToReferenceFolder();
            DeleteLegacyFlatTerrainCards();
            var materials = CreateRuntimeMaterials();
            var specs = BuildRuntimeSpecs();
            var runtimePrefabs = CreateRuntimePrefabs(specs, materials);

            Stage32TerrainPieceGenerator.EnsureStage32TerrainPieces();
            var library = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            var mappedCount = 0;
            if (library != null)
                mappedCount = ApplyRuntimePrefabsToStage32Definitions(library.GetDefinitions());

            CreateOrUpdateReferenceOnlyManifest();
            Stage32SceneCreator.CreateOrUpdateStage32Scene();
            CreateOrUpdateReviewScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var summary = new Stage32_6Summary();
            summary.ReferenceImageCount = FindReferenceSheetAssetPaths().Count;
            summary.MovedReferenceImageCount = movedReferences;
            summary.RuntimePrefabCount = runtimePrefabs.Count;
            summary.MappedDefinitionPrefabCount = mappedCount;
            summary.MaterialCount = materials.Count;
            summary.ReviewScenePath = ReviewScenePath;
            WriteReport(summary, new List<string>());
            return summary;
        }

        public static int ApplyRuntimePrefabsToStage32Definitions(IReadOnlyList<TerrainPieceDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0)
                return 0;

            EnsureFolders();
            var materials = CreateRuntimeMaterials();
            var specs = BuildRuntimeSpecs();
            CreateRuntimePrefabs(specs, materials);

            var definitionMap = BuildDefinitionRuntimeMap();
            var runtimeSpecById = BuildRuntimeSpecMap(specs);
            var mappedCount = 0;

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null || string.IsNullOrEmpty(definition.pieceId))
                    continue;

                string runtimeId;
                if (!definitionMap.TryGetValue(definition.pieceId, out runtimeId))
                    continue;

                RuntimeSpec runtimeSpec;
                if (!runtimeSpecById.TryGetValue(runtimeId, out runtimeSpec))
                    continue;

                var canonicalPrefab = LoadRuntimePrefab(runtimeId);
                if (canonicalPrefab == null)
                    continue;

                var mappedPrefab = CreateMappedDefinitionPrefab(definition, runtimeSpec, canonicalPrefab);
                if (mappedPrefab == null)
                    continue;

                definition.prefab = mappedPrefab;
                definition.supportsTint = false;
                definition.notes = "Stage32.6 runtime terrain assembly mapped from " + runtimeId + ". Reference sheets are art direction only; this prefab is visual-only and never gameplay authority.";
                EditorUtility.SetDirty(definition);
                mappedCount++;
            }

            AssetDatabase.SaveAssets();
            return mappedCount;
        }

        public static int MoveBatch01SourceImagesToReferenceFolder()
        {
            EnsureFolders();
            var moved = 0;
            var absoluteSource = ToAbsoluteProjectPath(Stage32TerrainArtIngestionGenerator.SourceFolder);
            if (!Directory.Exists(absoluteSource))
                return moved;

            var files = Directory.GetFiles(absoluteSource, "*.*", SearchOption.TopDirectoryOnly);
            for (var i = 0; i < files.Length; i++)
            {
                if (!IsImagePath(files[i]))
                    continue;

                var sourceAssetPath = ToAssetPath(files[i]);
                if (string.IsNullOrEmpty(sourceAssetPath))
                    continue;

                var destinationAssetPath = ReferenceFolder + "/" + Path.GetFileName(files[i]);
                if (AssetDatabase.LoadAssetAtPath<Object>(destinationAssetPath) != null)
                {
                    AssetDatabase.DeleteAsset(sourceAssetPath);
                    moved++;
                    continue;
                }

                var error = AssetDatabase.MoveAsset(sourceAssetPath, destinationAssetPath);
                if (!string.IsNullOrEmpty(error))
                    throw new InvalidOperationException("Could not move terrain reference image from " + sourceAssetPath + " to " + destinationAssetPath + ": " + error);
                moved++;
            }

            AssetDatabase.Refresh();
            return moved;
        }

        public static void DeleteLegacyFlatTerrainCards()
        {
            for (var i = 0; i < LegacyFlatCardFolders.Length; i++)
                DeleteAssetIfExists(LegacyFlatCardFolders[i]);
            AssetDatabase.Refresh();
        }

        public static Stage32_6Summary ValidateStage32_6Assets()
        {
            var summary = EnsureStage32_6Assets();
            var errors = new List<string>();
            ValidateReferencePolicy(summary, errors);
            ValidateRuntimePrefabs(summary, errors);
            ValidatePlayerFacingMappings(summary, errors);
            ValidateStage16Loads(errors);
            ValidateSceneAssetTextPolicy(Stage16SceneCreator.ScenePath, "Stage16 player-facing scene", errors);
            ValidateSceneAssetTextPolicy(Stage32SceneCreator.ScenePath, "Stage32 terrain review scene", errors);
            ValidateSceneAssetTextPolicy(ReviewScenePath, "Stage32.6 terrain review scene", errors);

            summary.Errors = errors;
            WriteReport(summary, errors);
            if (errors.Count > 0)
                throw new InvalidOperationException("Stage 32.6 terrain art integration validation failed: " + string.Join(" | ", errors.ToArray()));

            return summary;
        }

        static Dictionary<string, Material> CreateRuntimeMaterials()
        {
            var materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            AddMaterial(materials, "realistic_grass", new Color(0.20f, 0.31f, 0.16f, 1f), 0.22f, 0f);
            AddMaterial(materials, "dirt", new Color(0.31f, 0.24f, 0.17f, 1f), 0.18f, 0f);
            AddMaterial(materials, "mud", new Color(0.14f, 0.11f, 0.085f, 1f), 0.32f, 0f);
            AddMaterial(materials, "scorched_ground", new Color(0.055f, 0.052f, 0.045f, 1f), 0.10f, 0f);
            AddMaterial(materials, "cracked_concrete", new Color(0.42f, 0.42f, 0.38f, 1f), 0.24f, 0f);
            AddMaterial(materials, "asphalt", new Color(0.11f, 0.105f, 0.095f, 1f), 0.16f, 0f);
            AddMaterial(materials, "resource_crystal_blue", new Color(0.13f, 0.40f, 0.95f, 1f), 0.62f, 0f);
            AddMaterial(materials, "resource_crystal_green", new Color(0.10f, 0.72f, 0.50f, 1f), 0.58f, 0f);
            AddMaterial(materials, "rock", new Color(0.31f, 0.30f, 0.27f, 1f), 0.14f, 0f);
            AddMaterial(materials, "debris_rust_metal", new Color(0.31f, 0.23f, 0.16f, 1f), 0.25f, 0.08f);
            AddMaterial(materials, "vegetation", new Color(0.13f, 0.28f, 0.11f, 1f), 0.12f, 0f);
            AddMaterial(materials, "sandbag", new Color(0.47f, 0.39f, 0.25f, 1f), 0.18f, 0f);
            AddMaterial(materials, "concrete_barrier", new Color(0.48f, 0.47f, 0.42f, 1f), 0.22f, 0f);
            AddMaterial(materials, "paint_marking", new Color(0.92f, 0.82f, 0.48f, 1f), 0.30f, 0f);
            return materials;
        }

        static void AddMaterial(Dictionary<string, Material> materials, string id, Color color, float smoothness, float metallic)
        {
            var path = RuntimeMaterialFolder + "/stage32_6_" + id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            BattlefieldMaterialLibrary.ApplyMaterialProperties(material, color, smoothness, metallic);
            material.mainTexture = null;
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", null);
            if (material.HasProperty("_BumpMap"))
                material.SetTexture("_BumpMap", null);
            materials[id] = material;
            EditorUtility.SetDirty(material);
        }

        static List<GameObject> CreateRuntimePrefabs(List<RuntimeSpec> specs, Dictionary<string, Material> materials)
        {
            var prefabs = new List<GameObject>();
            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                var root = new GameObject(spec.Id);
                BuildRuntimeGeometry(root.transform, spec, materials);
                AddRuntimeMetadata(root, spec, spec.Id, spec.DisplayName, spec.Category, spec.SizeClass, spec.FootprintFineWidth, spec.FootprintFineHeight, spec.BlockingVisualOnly, spec.MaterialKey);
                AddLodGroup(root);

                var path = RuntimePrefabFolder + "/" + spec.Id + ".prefab";
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                Object.DestroyImmediate(root);
                if (prefab != null)
                    prefabs.Add(prefab);
            }

            return prefabs;
        }

        static GameObject CreateMappedDefinitionPrefab(TerrainPieceDefinition definition, RuntimeSpec runtimeSpec, GameObject canonicalPrefab)
        {
            var root = new GameObject(definition.pieceId);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(canonicalPrefab);
            instance.name = "Stage32_6RuntimeAssembly_" + runtimeSpec.Id;
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            AddRuntimeMetadata(root, runtimeSpec, runtimeSpec.Id, definition.displayName, definition.category, definition.sizeClass, definition.footprintFineWidth, definition.footprintFineHeight, definition.isGameplayBlockingVisualOnly, definition.materialProfileId, definition.pieceId);
            AddLodGroup(root);

            var path = RuntimeMappedPrefabFolder + "/" + definition.pieceId + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void BuildRuntimeGeometry(Transform root, RuntimeSpec spec, Dictionary<string, Material> materials)
        {
            if (spec.Id.IndexOf("road", StringComparison.OrdinalIgnoreCase) >= 0 ||
                spec.Id.IndexOf("pad", StringComparison.OrdinalIgnoreCase) >= 0 ||
                spec.Id.IndexOf("curb", StringComparison.OrdinalIgnoreCase) >= 0 ||
                spec.Id.IndexOf("ramp", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                BuildRoadOrBase(root, spec, materials);
                return;
            }

            if (spec.Id.IndexOf("resource", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                BuildResource(root, spec, materials);
                return;
            }

            if (spec.Id.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                spec.Id.IndexOf("ridge", StringComparison.OrdinalIgnoreCase) >= 0 ||
                spec.Id.IndexOf("cliff", StringComparison.OrdinalIgnoreCase) >= 0 ||
                spec.Id.IndexOf("crater", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                BuildRockOrCrater(root, spec, materials);
                return;
            }

            if (spec.Category == TerrainPieceCategory.Prop || spec.Id.IndexOf("shrub", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                BuildProp(root, spec, materials);
                return;
            }

            BuildGround(root, spec, materials);
        }

        static void BuildGround(Transform root, RuntimeSpec spec, Dictionary<string, Material> materials)
        {
            var primary = MaterialFor(materials, spec.MaterialKey);
            var detail = MaterialFor(materials, "rock");
            var vegetation = MaterialFor(materials, "vegetation");
            AddBeveledPlate(root, spec.Id + "_base", spec.FootprintWidth, spec.FootprintHeight, 0.10f, 0.10f, primary);

            var rng = CreateRandom(spec.Id);
            for (var i = 0; i < 4; i++)
            {
                var pos = RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.09f);
                CreatePrimitive(root, "embedded_rock_" + i, PrimitiveType.Sphere, pos, new Vector3(RandomRange(rng, 0.08f, 0.16f), RandomRange(rng, 0.025f, 0.055f), RandomRange(rng, 0.08f, 0.16f)), detail, Quaternion.Euler(0f, RandomRange(rng, 0f, 180f), 0f));
            }

            for (var i = 0; i < 3; i++)
            {
                var pos = RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.12f);
                CreatePrimitive(root, "grass_clump_" + i, PrimitiveType.Cube, pos + new Vector3(0f, 0.025f, 0f), new Vector3(0.045f, RandomRange(rng, 0.10f, 0.18f), 0.045f), vegetation, Quaternion.Euler(0f, RandomRange(rng, 0f, 180f), RandomRange(rng, -8f, 8f)));
            }

            if (spec.Id.IndexOf("concrete", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("scorched", StringComparison.OrdinalIgnoreCase) >= 0)
                AddCrackStrips(root, spec, materials, 3);
        }

        static void BuildRoadOrBase(Transform root, RuntimeSpec spec, Dictionary<string, Material> materials)
        {
            var surface = MaterialFor(materials, spec.MaterialKey);
            var barrier = MaterialFor(materials, "concrete_barrier");
            var paint = MaterialFor(materials, "paint_marking");
            var metal = MaterialFor(materials, "debris_rust_metal");
            AddBeveledPlate(root, spec.Id + "_slab", spec.FootprintWidth, spec.FootprintHeight, spec.Id.IndexOf("curb", StringComparison.OrdinalIgnoreCase) >= 0 ? 0.16f : 0.12f, 0.09f, surface);

            var curbLength = Mathf.Max(spec.FootprintWidth, spec.FootprintHeight);
            if (spec.Id.IndexOf("corner", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(root, "curb_leg_a", PrimitiveType.Cube, new Vector3(-spec.FootprintWidth * 0.34f, 0.17f, 0f), new Vector3(0.16f, 0.14f, spec.FootprintHeight * 0.72f), barrier, Quaternion.identity);
                CreatePrimitive(root, "curb_leg_b", PrimitiveType.Cube, new Vector3(0f, 0.17f, -spec.FootprintHeight * 0.34f), new Vector3(spec.FootprintWidth * 0.72f, 0.14f, 0.16f), barrier, Quaternion.identity);
            }
            else
            {
                CreatePrimitive(root, "curb_left", PrimitiveType.Cube, new Vector3(-spec.FootprintWidth * 0.44f, 0.16f, 0f), new Vector3(0.12f, 0.12f, curbLength * 0.88f), barrier, Quaternion.identity);
                CreatePrimitive(root, "curb_right", PrimitiveType.Cube, new Vector3(spec.FootprintWidth * 0.44f, 0.16f, 0f), new Vector3(0.12f, 0.12f, curbLength * 0.88f), barrier, Quaternion.identity);
            }

            var lineCount = spec.Id.IndexOf("cross", StringComparison.OrdinalIgnoreCase) >= 0 ? 6 : 4;
            for (var i = 0; i < lineCount; i++)
            {
                var offset = ((float)i - (lineCount - 1) * 0.5f) * 0.38f;
                var rot = spec.Id.IndexOf("t_junction", StringComparison.OrdinalIgnoreCase) >= 0 && i % 2 == 0 ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
                CreatePrimitive(root, "paint_mark_" + i, PrimitiveType.Cube, new Vector3(offset, 0.205f, 0f), new Vector3(0.055f, 0.018f, 0.34f), paint, rot);
            }

            if (spec.Id.IndexOf("damaged", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("dirt_edge", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                AddCrackStrips(root, spec, materials, 3);
                CreatePrimitive(root, "edge_rubble_a", PrimitiveType.Sphere, new Vector3(spec.FootprintWidth * 0.30f, 0.20f, spec.FootprintHeight * 0.22f), new Vector3(0.16f, 0.06f, 0.13f), metal, Quaternion.identity);
                CreatePrimitive(root, "edge_rubble_b", PrimitiveType.Sphere, new Vector3(-spec.FootprintWidth * 0.24f, 0.20f, -spec.FootprintHeight * 0.20f), new Vector3(0.13f, 0.05f, 0.14f), metal, Quaternion.identity);
            }
        }

        static void BuildResource(Transform root, RuntimeSpec spec, Dictionary<string, Material> materials)
        {
            AddBeveledPlate(root, spec.Id + "_mineral_bed", spec.FootprintWidth, spec.FootprintHeight, 0.09f, 0.10f, MaterialFor(materials, "dirt"));
            var crystalMaterial = spec.Id.IndexOf("green", StringComparison.OrdinalIgnoreCase) >= 0 ? MaterialFor(materials, "resource_crystal_green") : MaterialFor(materials, "resource_crystal_blue");
            if (spec.Id.IndexOf("depleted", StringComparison.OrdinalIgnoreCase) >= 0)
                crystalMaterial = MaterialFor(materials, "rock");

            var rng = CreateRandom(spec.Id);
            for (var i = 0; i < 8; i++)
            {
                var pos = RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.14f);
                var height = spec.Id.IndexOf("depleted", StringComparison.OrdinalIgnoreCase) >= 0 ? RandomRange(rng, 0.08f, 0.18f) : RandomRange(rng, 0.26f, 0.55f);
                CreatePrimitive(root, "crystal_" + i, PrimitiveType.Cube, pos + new Vector3(0f, height * 0.35f, 0f), new Vector3(RandomRange(rng, 0.08f, 0.16f), height, RandomRange(rng, 0.08f, 0.16f)), crystalMaterial, Quaternion.Euler(RandomRange(rng, -8f, 8f), RandomRange(rng, 0f, 180f), RandomRange(rng, -8f, 8f)));
            }

            for (var i = 0; i < 3; i++)
                CreatePrimitive(root, "mineral_rock_" + i, PrimitiveType.Sphere, RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.10f), new Vector3(0.14f, 0.045f, 0.14f), MaterialFor(materials, "rock"), Quaternion.identity);
        }

        static void BuildRockOrCrater(Transform root, RuntimeSpec spec, Dictionary<string, Material> materials)
        {
            var baseMaterial = spec.Id.IndexOf("crater", StringComparison.OrdinalIgnoreCase) >= 0 ? MaterialFor(materials, "scorched_ground") : MaterialFor(materials, "dirt");
            AddBeveledPlate(root, spec.Id + "_ground", spec.FootprintWidth, spec.FootprintHeight, 0.08f, 0.10f, baseMaterial);
            var rock = MaterialFor(materials, "rock");
            var rng = CreateRandom(spec.Id);

            if (spec.Id.IndexOf("crater", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(root, "crater_shadow", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(spec.FootprintWidth * 0.26f, 0.025f, spec.FootprintHeight * 0.26f), MaterialFor(materials, "scorched_ground"), Quaternion.identity);
                for (var i = 0; i < 8; i++)
                {
                    var angle = (Mathf.PI * 2f * i) / 8f;
                    var pos = new Vector3(Mathf.Cos(angle) * spec.FootprintWidth * 0.24f, 0.16f, Mathf.Sin(angle) * spec.FootprintHeight * 0.24f);
                    CreatePrimitive(root, "crater_rim_" + i, PrimitiveType.Sphere, pos, new Vector3(0.15f, 0.045f, 0.13f), rock, Quaternion.identity);
                }
                return;
            }

            var count = spec.Id.IndexOf("ridge", StringComparison.OrdinalIgnoreCase) >= 0 ? 9 : 8;
            for (var i = 0; i < count; i++)
            {
                var pos = RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.15f);
                var elongation = spec.Id.IndexOf("ridge", StringComparison.OrdinalIgnoreCase) >= 0 ? 1.8f : 1f;
                CreatePrimitive(root, "rock_mass_" + i, PrimitiveType.Sphere, pos + new Vector3(0f, RandomRange(rng, 0.07f, 0.20f), 0f), new Vector3(RandomRange(rng, 0.18f, 0.42f) * elongation, RandomRange(rng, 0.12f, 0.36f), RandomRange(rng, 0.18f, 0.36f)), rock, Quaternion.Euler(RandomRange(rng, -12f, 12f), RandomRange(rng, 0f, 180f), RandomRange(rng, -12f, 12f)));
            }
        }

        static void BuildProp(Transform root, RuntimeSpec spec, Dictionary<string, Material> materials)
        {
            AddBeveledPlate(root, spec.Id + "_anchor", spec.FootprintWidth, spec.FootprintHeight, 0.045f, 0.06f, MaterialFor(materials, spec.Id.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("shrub", StringComparison.OrdinalIgnoreCase) >= 0 ? "realistic_grass" : "dirt"));
            var metal = MaterialFor(materials, "debris_rust_metal");
            var concrete = MaterialFor(materials, "concrete_barrier");
            var vegetation = MaterialFor(materials, "vegetation");
            var rng = CreateRandom(spec.Id);

            if (spec.Id.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("shrub", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var foliageCount = spec.Id.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0 ? 5 : 7;
                for (var i = 0; i < foliageCount; i++)
                {
                    var pos = RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.10f);
                    if (spec.Id.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0)
                        CreatePrimitive(root, "tree_trunk_" + i, PrimitiveType.Cylinder, pos + new Vector3(0f, 0.18f, 0f), new Vector3(0.045f, 0.22f, 0.045f), MaterialFor(materials, "debris_rust_metal"), Quaternion.identity);
                    CreatePrimitive(root, "foliage_" + i, PrimitiveType.Sphere, pos + new Vector3(0f, RandomRange(rng, 0.18f, 0.45f), 0f), new Vector3(RandomRange(rng, 0.18f, 0.35f), RandomRange(rng, 0.16f, 0.32f), RandomRange(rng, 0.18f, 0.35f)), vegetation, Quaternion.identity);
                }
                return;
            }

            if (spec.Id.IndexOf("crate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (var i = 0; i < 5; i++)
                    CreatePrimitive(root, "crate_" + i, PrimitiveType.Cube, new Vector3(((i % 3) - 1) * 0.22f, 0.18f + (i / 3) * 0.16f, (i / 3) * 0.22f), new Vector3(0.22f, 0.18f, 0.22f), metal, Quaternion.Euler(0f, i * 12f, 0f));
                return;
            }

            if (spec.Id.IndexOf("barrel", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (var i = 0; i < 5; i++)
                    CreatePrimitive(root, "barrel_" + i, PrimitiveType.Cylinder, new Vector3(((i % 3) - 1) * 0.22f, 0.24f, (i / 3) * 0.24f), new Vector3(0.095f, 0.18f, 0.095f), metal, Quaternion.identity);
                return;
            }

            if (spec.Id.IndexOf("tire_tracks", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(root, "track_left", PrimitiveType.Cube, new Vector3(-0.28f, 0.08f, 0f), new Vector3(0.12f, 0.026f, spec.FootprintHeight * 0.72f), MaterialFor(materials, "scorched_ground"), Quaternion.identity);
                CreatePrimitive(root, "track_right", PrimitiveType.Cube, new Vector3(0.28f, 0.08f, 0f), new Vector3(0.12f, 0.026f, spec.FootprintHeight * 0.72f), MaterialFor(materials, "scorched_ground"), Quaternion.identity);
                for (var i = 0; i < 8; i++)
                    CreatePrimitive(root, "track_tread_" + i, PrimitiveType.Cube, new Vector3(i % 2 == 0 ? -0.28f : 0.28f, 0.105f, -spec.FootprintHeight * 0.35f + i * spec.FootprintHeight * 0.10f), new Vector3(0.18f, 0.024f, 0.035f), MaterialFor(materials, "scorched_ground"), Quaternion.identity);
                return;
            }

            if (spec.Id.IndexOf("anti_tank", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(root, "beam_a", PrimitiveType.Cube, Vector3.up * 0.22f, new Vector3(0.12f, 0.12f, 0.90f), metal, Quaternion.Euler(0f, 45f, 25f));
                CreatePrimitive(root, "beam_b", PrimitiveType.Cube, Vector3.up * 0.22f, new Vector3(0.12f, 0.12f, 0.90f), metal, Quaternion.Euler(0f, -45f, -25f));
                CreatePrimitive(root, "beam_c", PrimitiveType.Cube, Vector3.up * 0.22f, new Vector3(0.90f, 0.12f, 0.12f), metal, Quaternion.Euler(20f, 0f, 0f));
                return;
            }

            if (spec.Id.IndexOf("fence", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("barrier", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(root, "barrier_main", PrimitiveType.Cube, new Vector3(0f, 0.26f, 0f), new Vector3(spec.FootprintWidth * 0.76f, 0.28f, 0.16f), concrete, Quaternion.identity);
                CreatePrimitive(root, "barrier_cap", PrimitiveType.Cube, new Vector3(0f, 0.43f, 0f), new Vector3(spec.FootprintWidth * 0.68f, 0.06f, 0.12f), metal, Quaternion.identity);
                CreatePrimitive(root, "barrier_stripe_a", PrimitiveType.Cube, new Vector3(-spec.FootprintWidth * 0.18f, 0.46f, -0.01f), new Vector3(0.22f, 0.025f, 0.17f), MaterialFor(materials, "paint_marking"), Quaternion.Euler(0f, 0f, 18f));
                CreatePrimitive(root, "barrier_stripe_b", PrimitiveType.Cube, new Vector3(spec.FootprintWidth * 0.18f, 0.46f, -0.01f), new Vector3(0.22f, 0.025f, 0.17f), MaterialFor(materials, "paint_marking"), Quaternion.Euler(0f, 0f, 18f));
                return;
            }

            var debrisCount = spec.Id.IndexOf("pile", StringComparison.OrdinalIgnoreCase) >= 0 ? 8 : 5;
            for (var i = 0; i < debrisCount; i++)
                CreatePrimitive(root, "debris_" + i, PrimitiveType.Cube, RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.11f) + new Vector3(0f, RandomRange(rng, 0.06f, 0.16f), 0f), new Vector3(RandomRange(rng, 0.10f, 0.28f), RandomRange(rng, 0.06f, 0.16f), RandomRange(rng, 0.10f, 0.28f)), metal, Quaternion.Euler(RandomRange(rng, -18f, 18f), RandomRange(rng, 0f, 180f), RandomRange(rng, -18f, 18f)));
        }

        static void AddCrackStrips(Transform root, RuntimeSpec spec, Dictionary<string, Material> materials, int count)
        {
            var rng = CreateRandom(spec.Id + "_cracks");
            for (var i = 0; i < count; i++)
            {
                var pos = RandomPosition(rng, spec.FootprintWidth, spec.FootprintHeight, 0.17f);
                CreatePrimitive(root, "crack_" + i, PrimitiveType.Cube, pos + new Vector3(0f, 0.13f, 0f), new Vector3(RandomRange(rng, 0.22f, 0.48f), 0.018f, 0.035f), MaterialFor(materials, "scorched_ground"), Quaternion.Euler(0f, RandomRange(rng, 0f, 180f), 0f));
            }
        }

        static void AddRuntimeMetadata(GameObject root, RuntimeSpec spec, string runtimeId, string displayName, TerrainPieceCategory category, TerrainPieceSizeClass sizeClass, int footprintWidth, int footprintHeight, bool blockingVisualOnly, string materialProfileId, string mappedPieceId = null)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var materials = CountUniqueMaterials(renderers);

            var validation = root.AddComponent<TerrainPieceValidationTag>();
            validation.pieceId = string.IsNullOrEmpty(mappedPieceId) ? runtimeId : mappedPieceId;
            validation.displayName = displayName;
            validation.category = category;
            validation.sizeClass = sizeClass;
            validation.footprintFineWidth = footprintWidth;
            validation.footprintFineHeight = footprintHeight;
            validation.materialProfileId = string.IsNullOrEmpty(materialProfileId) ? spec.MaterialKey : materialProfileId;
            validation.passabilityVisualHint = blockingVisualOnly ? "Visual blocker cue only; Rts.Core remains authoritative." : "Passable visual dressing only; Rts.Core remains authoritative.";
            validation.buildableVisualHint = blockingVisualOnly ? "No-build visual cue only; Rts.Core remains authoritative." : "Buildability visual cue only; Rts.Core remains authoritative.";
            validation.supportsRotation = true;
            validation.supportsTint = false;
            validation.isGameplayBlockingVisualOnly = blockingVisualOnly;
            validation.questBudgetTag = "QuestSafeStage32_6Runtime";
            validation.rendererCount = renderers.Length;
            validation.primitiveCount = Mathf.Max(1, renderers.Length);
            validation.notes = "Stage32.6 runtime terrain mesh/material assembly. No reference-sheet texture or flat image-card runtime usage.";

            var runtime = root.AddComponent<Stage32_6RuntimeTerrainTag>();
            runtime.runtimeAssetId = runtimeId;
            runtime.mappedTerrainPieceId = mappedPieceId;
            runtime.category = category;
            runtime.referenceOnlyPolicyEnforced = true;
            runtime.usesReferenceTexture = false;
            runtime.flatImageCard = false;
            runtime.hasBeveledMesh = true;
            runtime.hasChildGeometry = renderers.Length > 1;
            runtime.pivotAtOrigin = root.transform.localPosition == Vector3.zero;
            runtime.rendererCount = renderers.Length;
            runtime.materialCount = materials;
            runtime.notes = "Created by Stage32.6 terrain art integration correction from modular geometry and shared materials.";
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

        static void AddBeveledPlate(Transform parent, string meshId, float width, float depth, float height, float bevel, Material material)
        {
            var mesh = CreateOrUpdateBeveledMesh(meshId, width, depth, height, bevel);
            var obj = new GameObject(meshId);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Vector3.zero;
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            var renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        static Mesh CreateOrUpdateBeveledMesh(string meshId, float width, float depth, float height, float bevel)
        {
            var path = RuntimeMeshFolder + "/" + meshId + "_mesh.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = meshId + "_mesh" };
                AssetDatabase.CreateAsset(mesh, path);
            }

            var halfWidth = width * 0.5f;
            var halfDepth = depth * 0.5f;
            var topHalfWidth = Mathf.Max(0.05f, halfWidth - bevel);
            var topHalfDepth = Mathf.Max(0.05f, halfDepth - bevel);
            var vertices = new[]
            {
                new Vector3(-halfWidth, 0f, -halfDepth),
                new Vector3(halfWidth, 0f, -halfDepth),
                new Vector3(halfWidth, 0f, halfDepth),
                new Vector3(-halfWidth, 0f, halfDepth),
                new Vector3(-topHalfWidth, height, -topHalfDepth),
                new Vector3(topHalfWidth, height, -topHalfDepth),
                new Vector3(topHalfWidth, height, topHalfDepth),
                new Vector3(-topHalfWidth, height, topHalfDepth)
            };
            var triangles = new[]
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7
            };

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        static GameObject CreatePrimitive(Transform parent, string objectName, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material, Quaternion localRotation)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = objectName;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = localRotation;
            obj.transform.localScale = localScale;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
            return obj;
        }

        static void CreateOrUpdateReviewScene()
        {
            EnsureFolders();
            var specs = BuildRuntimeSpecs();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage32_6_TerrainArtIntegrationReview";

            var root = new GameObject("Stage32_6 Runtime Terrain Review");
            var byCategoryOffset = new Dictionary<TerrainPieceCategory, int>();

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                int categoryIndex;
                byCategoryOffset.TryGetValue(spec.Category, out categoryIndex);
                byCategoryOffset[spec.Category] = categoryIndex + 1;

                var prefab = LoadRuntimePrefab(spec.Id);
                if (prefab == null)
                    continue;

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(root.transform, false);
                instance.transform.localPosition = new Vector3((categoryIndex % 8) * 2.4f, 0f, CategoryRow(spec.Category) * 2.6f + (categoryIndex / 8) * 1.9f);
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }

            var composition = new GameObject("Sample Battlefield Composition");
            composition.transform.SetParent(root.transform, false);
            composition.transform.localPosition = new Vector3(22f, 0f, 3f);
            AddCompositionPrefab(composition.transform, "foundation_pad_large", 0f, 0f, 0f, 1f);
            AddCompositionPrefab(composition.transform, "road_straight_01", 0f, 3.0f, 90f, 1f);
            AddCompositionPrefab(composition.transform, "road_crossing_01", 3f, 3.0f, 0f, 1f);
            AddCompositionPrefab(composition.transform, "resource_cluster_blue_01", 5.2f, 5.4f, 12f, 1f);
            AddCompositionPrefab(composition.transform, "rock_blocker_01", 6.4f, 0.2f, -20f, 1.1f);
            AddCompositionPrefab(composition.transform, "tree_cluster_01", -3.3f, 5.0f, 0f, 1f);
            AddCompositionPrefab(composition.transform, "wreckage_pile_01", 4.1f, -2.3f, 35f, 1f);

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(48f, -35f, 20f);

            var fill = new GameObject("Soft Fill Light").AddComponent<Light>();
            fill.type = LightType.Point;
            fill.intensity = 0.6f;
            fill.range = 28f;
            fill.transform.position = new Vector3(12f, 8f, 8f);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.transform.position = new Vector3(16f, 18f, -18f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 13f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.07f, 0.075f, 1f);

            EditorSceneManager.SaveScene(scene, ReviewScenePath);
        }

        static void AddCompositionPrefab(Transform parent, string runtimeId, float x, float z, float rotationY, float scale)
        {
            var prefab = LoadRuntimePrefab(runtimeId);
            if (prefab == null)
                return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = new Vector3(x, 0f, z);
            instance.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);
            instance.transform.localScale = Vector3.one * scale;
        }

        static void CaptureScreenshots()
        {
            Directory.CreateDirectory(Path.Combine(Stage8ActorCatalog.RepoRoot, "build", "screenshots", "stage32_6"));

            var reviewScene = EditorSceneManager.OpenScene(ReviewScenePath);
            if (!reviewScene.IsValid())
                throw new InvalidOperationException("Stage32.6 review scene did not open for screenshot capture.");
            RenderMainCameraToPng(Path.Combine(Stage8ActorCatalog.RepoRoot, ReviewScreenshotPath));

            Stage16SceneCreator.CreateOrUpdateStage16Scene();
            var stage16 = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!stage16.IsValid())
                throw new InvalidOperationException("Stage16 scene did not open for Stage32.6 screenshot capture.");
            RenderMainCameraToPng(Path.Combine(Stage8ActorCatalog.RepoRoot, PlayerFacingScreenshotPath));
        }

        static void RenderMainCameraToPng(string absolutePath)
        {
            var camera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("No camera is available for Stage32.6 screenshot capture.");

            var previousTarget = camera.targetTexture;
            var texture = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            var readable = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = texture;
                camera.Render();
                RenderTexture.active = texture;
                readable.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                readable.Apply(false);
                File.WriteAllBytes(absolutePath, readable.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = null;
                Object.DestroyImmediate(readable);
                Object.DestroyImmediate(texture);
            }
        }

        static void ValidateReferencePolicy(Stage32_6Summary summary, List<string> errors)
        {
            var sourceImages = FindImageAssetPaths(Stage32TerrainArtIngestionGenerator.SourceFolder);
            if (sourceImages.Count > 0)
                errors.Add("Batch01 source folder still contains concept/reference images: " + string.Join(", ", sourceImages.ToArray()));

            var references = FindReferenceSheetAssetPaths();
            summary.ReferenceImageCount = references.Count;
            if (references.Count == 0)
                errors.Add("Stage32.6 expected reference-only terrain art images under " + ReferenceFolder + ".");

            for (var i = 0; i < LegacyFlatCardFolders.Length; i++)
                if (AssetDatabase.IsValidFolder(LegacyFlatCardFolders[i]))
                    errors.Add("Legacy flat-card runtime folder still exists: " + LegacyFlatCardFolders[i]);
        }

        static void ValidateRuntimePrefabs(Stage32_6Summary summary, List<string> errors)
        {
            var specs = BuildRuntimeSpecs();
            var materialCount = CreateRuntimeMaterials().Count;
            summary.MaterialCount = materialCount;
            if (materialCount < 13)
                errors.Add("Stage32.6 material set is underfilled. Materials: " + materialCount);

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                var prefab = LoadRuntimePrefab(spec.Id);
                if (prefab == null)
                {
                    errors.Add("Missing required Stage32.6 runtime prefab: " + spec.Id);
                    continue;
                }

                ValidateRuntimePrefab(prefab, spec.Id, errors);
            }

            summary.RuntimePrefabCount = specs.Count;
        }

        static void ValidateRuntimePrefab(GameObject prefab, string id, List<string> errors)
        {
            if (prefab.GetComponent<TerrainArtSourceTag>() != null)
                errors.Add(id + ": runtime prefab still carries TerrainArtSourceTag.");

            var runtimeTag = prefab.GetComponent<Stage32_6RuntimeTerrainTag>();
            if (runtimeTag == null || !runtimeTag.IsComplete())
                errors.Add(id + ": Stage32_6RuntimeTerrainTag missing or incomplete.");

            var colliders = prefab.GetComponentsInChildren<Collider>(true);
            if (colliders.Length > 0)
                errors.Add(id + ": runtime terrain prefab must not include colliders.");

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length < 3)
                errors.Add(id + ": runtime terrain prefab appears underbuilt; renderer count=" + renderers.Length + ".");
            if (renderers.Length > 14)
                errors.Add(id + ": runtime terrain prefab exceeds Quest-safe renderer budget; renderer count=" + renderers.Length + ".");

            var bounds = CalculateRendererBounds(renderers);
            if (bounds.size.y < 0.035f)
                errors.Add(id + ": runtime prefab has insufficient vertical depth and risks reading as a flat card.");

            if (LooksLikeFlatImageCard(prefab, renderers))
                errors.Add(id + ": runtime prefab looks like a flat image card.");

            ValidateNoReferenceTextures(id, renderers, errors);
        }

        static void ValidatePlayerFacingMappings(Stage32_6Summary summary, List<string> errors)
        {
            var profile = Stage32TerrainPieceGenerator.LoadPlayerFacingSetDressingProfile();
            var library = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            if (profile == null || library == null)
            {
                errors.Add("Stage32.6 cannot validate player-facing set dressing because the profile or library is missing.");
                return;
            }

            var mapped = 0;
            for (var i = 0; i < profile.placements.Count; i++)
            {
                var placement = profile.placements[i];
                if (placement == null || string.IsNullOrEmpty(placement.pieceId))
                    continue;

                var definition = library.GetDefinition(placement.pieceId);
                var prefab = definition != null ? definition.prefab : null;
                if (prefab == null)
                {
                    errors.Add("Stage32.6 player-facing placement has no prefab: " + placement.pieceId);
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(prefab);
                if (path.IndexOf("Batch01Imported", StringComparison.OrdinalIgnoreCase) >= 0)
                    errors.Add("Stage32.6 player-facing placement still uses Batch01Imported flat-card prefab: " + placement.pieceId);
                if (prefab.GetComponent<TerrainArtSourceTag>() != null)
                    errors.Add("Stage32.6 player-facing placement still uses TerrainArtSourceTag: " + placement.pieceId);
                if (prefab.GetComponent<Stage32_6RuntimeTerrainTag>() == null)
                    errors.Add("Stage32.6 player-facing placement is not mapped to a runtime terrain wrapper: " + placement.pieceId);
                else
                    mapped++;
            }

            summary.MappedDefinitionPrefabCount = mapped;
            if (mapped < profile.placements.Count)
                errors.Add("Stage32.6 player-facing runtime terrain mapping is incomplete. Mapped=" + mapped + " placements=" + profile.placements.Count + ".");
        }

        static void ValidateStage16Loads(List<string> errors)
        {
            Stage16SceneCreator.CreateOrUpdateStage16Scene();
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
            {
                errors.Add("Stage16 scene did not load after Stage32.6 terrain integration.");
                return;
            }

            var layer = Object.FindFirstObjectByType<TerrainSetDressingRuntimeLayer>();
            if (layer == null || layer.activeProfile == null || layer.pieceLibrary == null)
                errors.Add("Stage16 terrain set dressing runtime layer is missing after Stage32.6 integration.");
        }

        static void ValidateSceneAssetTextPolicy(string scenePath, string label, List<string> errors)
        {
            var absolutePath = ToAbsoluteProjectPath(scenePath);
            if (!File.Exists(absolutePath))
            {
                errors.Add(label + " is missing: " + scenePath);
                return;
            }

            var sceneText = File.ReadAllText(absolutePath);
            if (sceneText.IndexOf("TerrainArtSourceTag", StringComparison.OrdinalIgnoreCase) >= 0)
                errors.Add(label + " still contains TerrainArtSourceTag from the rejected flat-card source art path.");
            if (sceneText.IndexOf("Batch01Imported", StringComparison.OrdinalIgnoreCase) >= 0)
                errors.Add(label + " still references Batch01Imported flat-card terrain assets.");
            if (sceneText.IndexOf(Stage32TerrainArtIngestionGenerator.SourceFolder, StringComparison.OrdinalIgnoreCase) >= 0)
                errors.Add(label + " still references Batch01 source sheets instead of reference-only/runtime terrain assets.");
        }

        static void ValidateNoReferenceTextures(string id, Renderer[] renderers, List<string> errors)
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                var sharedMaterials = renderer.sharedMaterials;
                for (var j = 0; j < sharedMaterials.Length; j++)
                {
                    var material = sharedMaterials[j];
                    if (material == null)
                        continue;

                    var texture = material.mainTexture;
                    if (texture == null && material.HasProperty("_BaseMap"))
                        texture = material.GetTexture("_BaseMap");
                    if (texture == null)
                        continue;

                    var texturePath = AssetDatabase.GetAssetPath(texture);
                    if (texturePath.IndexOf(ReferenceFolder, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        texturePath.IndexOf("Batch01Imported", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        texturePath.IndexOf(Stage32TerrainArtIngestionGenerator.SourceFolder, StringComparison.OrdinalIgnoreCase) >= 0)
                        errors.Add(id + ": runtime material uses forbidden reference/source texture: " + texturePath);
                }
            }
        }

        static bool LooksLikeFlatImageCard(GameObject prefab, Renderer[] renderers)
        {
            if (renderers.Length != 1)
                return false;

            var filter = prefab.GetComponentInChildren<MeshFilter>(true);
            if (filter == null || filter.sharedMesh == null)
                return false;

            return filter.sharedMesh.vertexCount <= 4 && renderers[0].sharedMaterial != null && renderers[0].sharedMaterial.mainTexture != null;
        }

        static Bounds CalculateRendererBounds(Renderer[] renderers)
        {
            var hasBounds = false;
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;
                if (!hasBounds)
                {
                    bounds = renderers[i].bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return bounds;
        }

        static void CreateOrUpdateReferenceOnlyManifest()
        {
            var manifest = AssetDatabase.LoadAssetAtPath<TerrainArtManifest>(Stage32TerrainArtIngestionGenerator.ManifestPath);
            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<TerrainArtManifest>();
                AssetDatabase.CreateAsset(manifest, Stage32TerrainArtIngestionGenerator.ManifestPath);
            }

            manifest.batchId = Stage32TerrainArtIngestionGenerator.BatchId;
            manifest.sourceFolder = ReferenceFolder;
            manifest.entries = new List<TerrainArtManifestEntry>();

            var references = FindReferenceSheetAssetPaths();
            for (var i = 0; i < references.Count; i++)
            {
                var source = AssetDatabase.LoadAssetAtPath<Object>(references[i]);
                manifest.entries.Add(new TerrainArtManifestEntry
                {
                    artId = Path.GetFileNameWithoutExtension(references[i]),
                    displayName = ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(references[i])),
                    replacesPieceId = string.Empty,
                    category = TerrainPieceCategory.Ground,
                    sourceKind = TerrainArtSourceKind.Texture,
                    sourceAsset = source,
                    sourceAssetPath = references[i],
                    generatedMaterial = null,
                    generatedPrefab = null,
                    uvRect = new Vector4(0f, 0f, 1f, 1f),
                    coreBatch = true,
                    playerFacingReplacement = false,
                    notes = "Reference-only art direction. Stage32.6 forbids direct runtime use as texture cards or cropped sheet planes."
                });
            }

            EditorUtility.SetDirty(manifest);
        }

        static void WriteReport(Stage32_6Summary summary, List<string> errors)
        {
            var absolutePath = Path.Combine(Stage8ActorCatalog.RepoRoot, ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));

            var builder = new StringBuilder();
            builder.AppendLine("# Stage 32.6 Report");
            builder.AppendLine();
            builder.AppendLine("Stage 32.6 corrects the rejected terrain-card integration by treating Batch01 sheet images as reference-only art direction and using runtime mesh/material prefab assemblies for player-facing terrain dressing.");
            builder.AppendLine();
            builder.AppendLine("- Runtime terrain prefabs: " + summary.RuntimePrefabCount);
            builder.AppendLine("- Player-facing mapped wrappers: " + summary.MappedDefinitionPrefabCount);
            builder.AppendLine("- Shared runtime materials: " + summary.MaterialCount);
            builder.AppendLine("- Reference-only images: " + summary.ReferenceImageCount);
            builder.AppendLine("- Review scene: `" + ReviewScenePath + "`");
            builder.AppendLine("- Review screenshot: `" + ReviewScreenshotPath + "`");
            builder.AppendLine("- Player-facing screenshot: `" + PlayerFacingScreenshotPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Root Cause");
            builder.AppendLine("The prior Batch01 ingestion cropped concept/reference sheets into texture materials and saved one-plane prefab cards under `Batch01Imported`. Those technically validated, but they were not usable runtime terrain art.");
            builder.AppendLine();
            builder.AppendLine("## Runtime Rule");
            builder.AppendLine("Runtime terrain prefabs must use Unity meshes and shared materials. Reference sheets may remain in `Assets/Rts/Art/References/Terrain/Stage32_6ArtDirection/`, but they must not be assigned to player-facing materials, terrain prefabs, or Stage16 set dressing.");
            builder.AppendLine();
            builder.AppendLine("## Validation Errors");
            if (errors == null || errors.Count == 0)
                builder.AppendLine("- None");
            else
                for (var i = 0; i < errors.Count; i++)
                    builder.AppendLine("- " + errors[i]);

            File.WriteAllText(absolutePath, builder.ToString(), Encoding.UTF8);
        }

        static List<RuntimeSpec> BuildRuntimeSpecs()
        {
            var specs = new List<RuntimeSpec>();
            Add(specs, "grass_ground_01", "Grass Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 3, "realistic_grass", false);
            Add(specs, "grass_ground_02", "Grass Ground 02", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 3, "realistic_grass", false);
            Add(specs, "dirt_ground_01", "Dirt Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 2, "dirt", false);
            Add(specs, "mud_ground_01", "Mud Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 2, 2, "mud", false);
            Add(specs, "scorched_ground_01", "Scorched Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 2, 2, "scorched_ground", false);
            Add(specs, "cracked_concrete_01", "Cracked Concrete 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 2, "cracked_concrete", false);

            Add(specs, "road_straight_01", "Road Straight 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 2, "asphalt", false);
            Add(specs, "road_corner_01", "Road Corner 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Small, 3, 3, "asphalt", false);
            Add(specs, "road_t_junction_01", "Road T Junction 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "asphalt", false);
            Add(specs, "road_crossing_01", "Road Crossing 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "asphalt", false);
            Add(specs, "damaged_road_01", "Damaged Road 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 2, "asphalt", false);
            Add(specs, "road_to_dirt_edge_01", "Road To Dirt Edge 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 2, "asphalt", false);
            Add(specs, "concrete_pad_small", "Concrete Pad Small", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Small, 3, 3, "cracked_concrete", false);
            Add(specs, "concrete_pad_medium", "Concrete Pad Medium", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 3, "cracked_concrete", false);
            Add(specs, "foundation_pad_large", "Foundation Pad Large", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Large, 5, 4, "cracked_concrete", false);
            Add(specs, "ramp_concrete_01", "Ramp Concrete 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Small, 3, 2, "cracked_concrete", false);
            Add(specs, "base_curb_straight", "Base Curb Straight", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 1, "concrete_barrier", false);
            Add(specs, "base_curb_corner", "Base Curb Corner", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Small, 2, 2, "concrete_barrier", false);

            Add(specs, "resource_cluster_blue_01", "Resource Cluster Blue 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "resource_crystal_blue", false);
            Add(specs, "resource_cluster_green_01", "Resource Cluster Green 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "resource_crystal_green", false);
            Add(specs, "resource_depleted_01", "Resource Depleted 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "rock", false);
            Add(specs, "rock_blocker_01", "Rock Blocker 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "rock", true);
            Add(specs, "rock_blocker_02", "Rock Blocker 02", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "rock", true);
            Add(specs, "ridge_piece_long_01", "Ridge Piece Long 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Edge, 4, 1, "rock", true);
            Add(specs, "broken_cliff_corner_01", "Broken Cliff Corner 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Medium, 3, 2, "rock", true);
            Add(specs, "crater_01", "Crater 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Patch, 2, 2, "scorched_ground", false);
            Add(specs, "crater_02", "Crater 02", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Patch, 3, 3, "scorched_ground", false);
            Add(specs, "debris_burn_patch_01", "Debris Burn Patch 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "debris_rust_metal", false);
            Add(specs, "wreckage_small_01", "Wreckage Small 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 3, 2, "debris_rust_metal", true);
            Add(specs, "wreckage_pile_01", "Wreckage Pile 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 3, 2, "debris_rust_metal", true);
            Add(specs, "anti_tank_obstacle_01", "Anti Tank Obstacle 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "debris_rust_metal", true);
            Add(specs, "fence_barrier_01", "Fence Barrier 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Edge, 3, 1, "concrete_barrier", true);
            Add(specs, "tire_tracks_01", "Tire Tracks 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Strip, 4, 1, "scorched_ground", false);
            Add(specs, "debris_small_01", "Debris Small 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "debris_rust_metal", false);
            Add(specs, "shrub_cluster_01", "Shrub Cluster 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "vegetation", true);
            Add(specs, "shrub_cluster_02", "Shrub Cluster 02", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "vegetation", true);
            Add(specs, "tree_cluster_01", "Tree Cluster 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "vegetation", true);
            Add(specs, "tree_cluster_02", "Tree Cluster 02", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "vegetation", true);
            Add(specs, "crate_stack_01", "Crate Stack 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "debris_rust_metal", false);
            Add(specs, "barrel_group_01", "Barrel Group 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "debris_rust_metal", false);
            return specs;
        }

        static void Add(List<RuntimeSpec> specs, string id, string displayName, TerrainPieceCategory category, TerrainPieceSizeClass sizeClass, int width, int height, string materialKey, bool blocking)
        {
            specs.Add(new RuntimeSpec
            {
                Id = id,
                DisplayName = displayName,
                Category = category,
                SizeClass = sizeClass,
                FootprintFineWidth = width,
                FootprintFineHeight = height,
                FootprintWidth = Mathf.Max(0.8f, width * 0.55f),
                FootprintHeight = Mathf.Max(0.8f, height * 0.55f),
                MaterialKey = materialKey,
                BlockingVisualOnly = blocking
            });
        }

        static Dictionary<string, string> BuildDefinitionRuntimeMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddMap(map, "base_foundation_pad_01", "concrete_pad_small");
            AddMap(map, "base_foundation_pad_02", "concrete_pad_medium");
            AddMap(map, "base_foundation_pad_03", "foundation_pad_large");
            AddMap(map, "base_production_apron_01", "concrete_pad_medium");
            AddMap(map, "base_production_apron_02", "cracked_concrete_01");
            AddMap(map, "base_road_strip_01", "road_straight_01");
            AddMap(map, "base_road_strip_02", "damaged_road_01");
            AddMap(map, "base_rally_exit_marking_01", "road_crossing_01");
            AddMap(map, "base_rally_exit_marking_02", "road_t_junction_01");
            AddMap(map, "ground_compact_soil_patch_01", "dirt_ground_01");
            AddMap(map, "ground_compact_soil_patch_02", "road_to_dirt_edge_01");
            AddMap(map, "ground_scorched_patch_01", "scorched_ground_01");
            AddMap(map, "ground_mud_patch_01", "mud_ground_01");
            AddMap(map, "transition_concrete_ground_edge_01", "cracked_concrete_01");
            AddMap(map, "transition_buildable_edge_01", "base_curb_straight");
            AddMap(map, "transition_dirt_road_blend_01", "road_to_dirt_edge_01");
            AddMap(map, "resource_cluster_01", "resource_cluster_blue_01");
            AddMap(map, "resource_cluster_02", "resource_cluster_green_01");
            AddMap(map, "resource_rich_cluster_01", "resource_cluster_blue_01");
            AddMap(map, "resource_decal_01", "resource_depleted_01");
            AddMap(map, "resource_harvest_marker_01", "resource_cluster_green_01");
            AddMap(map, "transition_resource_edge_01", "resource_depleted_01");
            AddMap(map, "obstacle_rock_cluster_01", "rock_blocker_01");
            AddMap(map, "obstacle_rock_cluster_02", "rock_blocker_02");
            AddMap(map, "obstacle_ridge_piece_01", "ridge_piece_long_01");
            AddMap(map, "obstacle_cliff_blocker_chunk_01", "broken_cliff_corner_01");
            AddMap(map, "obstacle_tree_bush_cluster_01", "tree_cluster_01");
            AddMap(map, "obstacle_wreckage_01", "wreckage_small_01");
            AddMap(map, "obstacle_debris_01", "debris_small_01");
            AddMap(map, "prop_sandbag_01", "fence_barrier_01");
            AddMap(map, "prop_sandbag_02", "base_curb_corner");
            AddMap(map, "prop_barrier_01", "fence_barrier_01");
            AddMap(map, "prop_tank_trap_01", "anti_tank_obstacle_01");
            AddMap(map, "prop_tire_tracks_01", "tire_tracks_01");
            AddMap(map, "prop_tire_tracks_02", "tire_tracks_01");
            AddMap(map, "prop_shell_mark_01", "crater_01");
            AddMap(map, "prop_crates_01", "crate_stack_01");
            AddMap(map, "prop_antenna_beacon_01", "barrel_group_01");
            AddMap(map, "prop_destroyed_vehicle_proxy_01", "wreckage_pile_01");
            AddMap(map, "transition_rock_edge_01", "ridge_piece_long_01");
            AddMap(map, "ground_road_path_01", "road_to_dirt_edge_01");
            AddMap(map, "ground_grass_dirt_patch_01", "grass_ground_01");
            AddMap(map, "ground_grass_dirt_patch_02", "grass_ground_02");
            AddMap(map, "ground_rocky_blocked_01", "rock_blocker_01");
            return map;
        }

        static void AddMap(Dictionary<string, string> map, string pieceId, string runtimeId)
        {
            map[pieceId] = runtimeId;
        }

        static Dictionary<string, RuntimeSpec> BuildRuntimeSpecMap(List<RuntimeSpec> specs)
        {
            var map = new Dictionary<string, RuntimeSpec>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < specs.Count; i++)
                map[specs[i].Id] = specs[i];
            return map;
        }

        static GameObject LoadRuntimePrefab(string runtimeId)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(RuntimePrefabFolder + "/" + runtimeId + ".prefab");
        }

        static int CategoryRow(TerrainPieceCategory category)
        {
            if (category == TerrainPieceCategory.Ground)
                return 0;
            if (category == TerrainPieceCategory.BaseConstruction || category == TerrainPieceCategory.Transition)
                return 2;
            if (category == TerrainPieceCategory.Resource)
                return 4;
            if (category == TerrainPieceCategory.Obstacle)
                return 6;
            return 8;
        }

        static Material MaterialFor(Dictionary<string, Material> materials, string key)
        {
            Material material;
            if (!string.IsNullOrEmpty(key) && materials.TryGetValue(key, out material))
                return material;
            return materials["dirt"];
        }

        static int CountUniqueMaterials(Renderer[] renderers)
        {
            var set = new HashSet<Material>();
            for (var i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                for (var j = 0; j < materials.Length; j++)
                    if (materials[j] != null)
                        set.Add(materials[j]);
            }
            return set.Count;
        }

        static Vector3 RandomPosition(System.Random rng, float width, float depth, float y)
        {
            return new Vector3(RandomRange(rng, -width * 0.38f, width * 0.38f), y, RandomRange(rng, -depth * 0.38f, depth * 0.38f));
        }

        static float RandomRange(System.Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }

        static System.Random CreateRandom(string seed)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < seed.Length; i++)
                    hash = hash * 31 + seed[i];
                return new System.Random(hash);
            }
        }

        static bool IsImagePath(string path)
        {
            var ext = Path.GetExtension(path);
            return ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        static List<string> FindReferenceSheetAssetPaths()
        {
            return FindImageAssetPaths(ReferenceFolder);
        }

        static List<string> FindImageAssetPaths(string assetFolder)
        {
            var paths = new List<string>();
            var absolute = ToAbsoluteProjectPath(assetFolder);
            if (!Directory.Exists(absolute))
                return paths;

            var files = Directory.GetFiles(absolute, "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                if (!IsImagePath(files[i]))
                    continue;
                var assetPath = ToAssetPath(files[i]);
                if (!string.IsNullOrEmpty(assetPath))
                    paths.Add(assetPath);
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);
            return paths;
        }

        static void DeleteAssetIfExists(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath) || AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
        }

        static void EnsureFolders()
        {
            CreateFolderRecursive(RuntimeMaterialFolder);
            CreateFolderRecursive(RuntimePrefabFolder);
            CreateFolderRecursive(RuntimeMappedPrefabFolder);
            CreateFolderRecursive(RuntimeMeshFolder);
            CreateFolderRecursive(ReferenceFolder);
            CreateFolderRecursive("Assets/Rts/ScriptableObjects/Art/TerrainPieces");
        }

        static void CreateFolderRecursive(string path)
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
            if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
            return Path.Combine(projectRoot, assetPath);
        }

        static string ToAssetPath(string absolutePath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/').TrimEnd('/');
            var full = Path.GetFullPath(absolutePath).Replace('\\', '/');
            if (!full.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase))
                return null;
            return full.Substring(projectRoot.Length + 1);
        }

        sealed class RuntimeSpec
        {
            public string Id;
            public string DisplayName;
            public TerrainPieceCategory Category;
            public TerrainPieceSizeClass SizeClass;
            public int FootprintFineWidth;
            public int FootprintFineHeight;
            public float FootprintWidth;
            public float FootprintHeight;
            public string MaterialKey;
            public bool BlockingVisualOnly;
        }
    }

    public sealed class Stage32_6Summary
    {
        public int ReferenceImageCount;
        public int MovedReferenceImageCount;
        public int RuntimePrefabCount;
        public int MappedDefinitionPrefabCount;
        public int MaterialCount;
        public string ReviewScenePath;
        public List<string> Errors = new List<string>();
    }
}
