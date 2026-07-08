using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32_5TerrainArtBatch01Importer
    {
        public const string SourceFolder = "Assets/Rts/Art/Source/Terrain/Batch01";
        public const string ManifestJsonPath = SourceFolder + "/terrain_batch01_manifest.json";
        public const string MaterialFolder = "Assets/Rts/Art/Materials/Terrain/Batch01Imported";
        public const string MeshFolder = "Assets/Rts/Art/Meshes/Terrain/Batch01Imported";
        public const string PrefabFolder = "Assets/Rts/Art/Prefabs/Terrain/Batch01Imported";
        public const string MappedPrefabFolder = PrefabFolder + "/MappedDefinitions";
        public const string ReviewScenePath = "Assets/Rts/Scenes/Stage32_5_TerrainArtBatch01Review.unity";
        public const int MinimumSourceAssetCount = 40;
        public const int MinimumPlayerFacingReplacementCount = 32;

        static readonly Dictionary<string, string> DefinitionToAssetMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "base_foundation_pad_01", "concrete_pad_small" },
            { "base_foundation_pad_02", "concrete_pad_medium" },
            { "base_foundation_pad_03", "foundation_pad_large" },
            { "base_production_apron_01", "concrete_pad_medium" },
            { "base_production_apron_02", "foundation_pad_large" },
            { "base_road_strip_01", "road_straight_01" },
            { "base_road_strip_02", "damaged_road_01" },
            { "base_rally_exit_marking_01", "road_crossing_01" },
            { "base_rally_exit_marking_02", "road_t_junction_01" },
            { "ground_compact_soil_patch_01", "dirt_ground_01" },
            { "ground_compact_soil_patch_02", "road_to_dirt_edge_01" },
            { "ground_scorched_patch_01", "scorched_ground_01" },
            { "ground_mud_patch_01", "mud_ground_01" },
            { "transition_concrete_ground_edge_01", "cracked_concrete_01" },
            { "transition_buildable_edge_01", "base_curb_straight" },
            { "transition_dirt_road_blend_01", "road_to_dirt_edge_01" },
            { "resource_cluster_01", "resource_cluster_blue_01" },
            { "resource_cluster_02", "resource_cluster_green_01" },
            { "resource_rich_cluster_01", "resource_cluster_blue_01" },
            { "resource_decal_01", "resource_depleted_01" },
            { "resource_harvest_marker_01", "resource_cluster_green_01" },
            { "transition_resource_edge_01", "resource_depleted_01" },
            { "obstacle_rock_cluster_01", "rock_blocker_01" },
            { "obstacle_rock_cluster_02", "rock_blocker_02" },
            { "obstacle_ridge_piece_01", "ridge_piece_long_01" },
            { "obstacle_cliff_blocker_chunk_01", "broken_cliff_corner_01" },
            { "obstacle_tree_bush_cluster_01", "tree_cluster_01" },
            { "obstacle_wreckage_01", "wreckage_small_01" },
            { "obstacle_debris_01", "debris_burn_patch_01" },
            { "prop_sandbag_01", "sandbags_straight_01" },
            { "prop_sandbag_02", "sandbags_straight_01" },
            { "prop_barrier_01", "barrier_concrete_01" },
            { "prop_tank_trap_01", "anti_tank_obstacle_01" },
            { "prop_tire_tracks_01", "tire_tracks_01" },
            { "prop_tire_tracks_02", "debris_small_01" },
            { "prop_shell_mark_01", "crater_01" },
            { "prop_crates_01", "crate_stack_01" },
            { "prop_antenna_beacon_01", "barrel_group_01" },
            { "prop_destroyed_vehicle_proxy_01", "wreckage_pile_01" },
            { "transition_rock_edge_01", "ridge_piece_long_01" },
            { "ground_road_path_01", "road_to_dirt_edge_01" },
            { "ground_grass_dirt_patch_01", "grass_ground_01" },
            { "ground_grass_dirt_patch_02", "grass_ground_02" },
            { "ground_rocky_blocked_01", "rock_blocker_01" }
        };

        [MenuItem("ProjectAegisRTS/Stage 32.5/Import Terrain Art Batch01")]
        public static void ImportBatch01Menu()
        {
            EnsureBatch01TerrainArt();
        }

        public static void ImportBatch01Batch()
        {
            try
            {
                var summary = EnsureBatch01TerrainArt();
                Debug.Log("Stage 32.5 terrain art Batch01 import completed. Assets: " + summary.SourceAssetCount + ", player-facing replacements: " + summary.PlayerFacingReplacementCount);
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

        public static void ValidateBatch01Batch()
        {
            try
            {
                ValidateBatch01();
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

        public static Stage32TerrainArtIngestionSummary EnsureBatch01TerrainArt()
        {
            if (!File.Exists(ToAbsoluteProjectPath(ManifestJsonPath)))
                throw new FileNotFoundException("Stage32.5 Batch01 manifest is missing.", ManifestJsonPath);

            EnsureFolders();
            ConfigureSourceTextureImports();
            Stage32TerrainPieceGenerator.EnsureStage32TerrainPieces();

            var package = LoadPackage();
            if (package.assets == null || package.assets.Count < MinimumSourceAssetCount)
                throw new InvalidOperationException("Stage32.5 expected at least " + MinimumSourceAssetCount + " source terrain assets; found " + (package.assets == null ? 0 : package.assets.Count) + ".");

            var definitions = LoadDefinitionMap();
            var manifest = LoadOrCreateUnityManifest();
            manifest.batchId = Stage32TerrainArtIngestionGenerator.BatchId;
            manifest.sourceFolder = SourceFolder;
            manifest.entries = new List<TerrainArtManifestEntry>();

            var sourceById = new Dictionary<string, Batch01Asset>(StringComparer.OrdinalIgnoreCase);
            var canonicalById = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            var materialById = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < package.assets.Count; i++)
            {
                var asset = package.assets[i];
                ValidateAssetFiles(asset);
                var texturePath = SourceFolder + "/" + NormalizeAssetRelativePath(asset.transparentPng);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                    throw new InvalidOperationException(asset.id + " did not import as a Texture2D: " + texturePath);

                var material = CreateMaterial(asset, texture);
                var prefab = CreatePrefab(asset, asset.id, asset.id, material, true, null);
                materialById[asset.id] = material;
                canonicalById[asset.id] = prefab;
                sourceById[asset.id] = asset;
                manifest.entries.Add(CreateManifestEntry(asset, asset.id, texturePath, material, prefab, true));
            }

            var replacementCount = 0;
            foreach (var pair in DefinitionToAssetMap)
            {
                Batch01Asset asset;
                GameObject canonicalPrefab;
                Material material;
                TerrainPieceDefinition definition;
                if (!sourceById.TryGetValue(pair.Value, out asset) ||
                    !canonicalById.TryGetValue(pair.Value, out canonicalPrefab) ||
                    !materialById.TryGetValue(pair.Value, out material) ||
                    !definitions.TryGetValue(pair.Key, out definition))
                    continue;

                var mappedPrefab = CreatePrefab(asset, pair.Key, pair.Value, material, false, definition);
                ApplyDefinitionReplacement(definition, asset, mappedPrefab);
                manifest.entries.Add(CreateManifestEntry(asset, pair.Key, SourceFolder + "/" + NormalizeAssetRelativePath(asset.transparentPng), material, mappedPrefab, false));
                replacementCount++;
            }

            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ApplyImportedBatch01ToTerrainDefinitionsAndProfile();
            CreateOrUpdateReviewScene(package.assets, canonicalById);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            NormalizeGeneratedTextArtifacts();

            var summary = new Stage32TerrainArtIngestionSummary();
            summary.SourceAssetCount = package.assets.Count;
            summary.PlayerFacingReplacementCount = replacementCount;
            return summary;
        }

        public static bool HasImportedBatch01()
        {
            var manifest = Stage32TerrainArtIngestionGenerator.LoadManifest();
            return manifest != null && manifest.CountPlayerFacingReplacements() >= MinimumPlayerFacingReplacementCount;
        }

        public static void ApplyImportedBatch01ToTerrainDefinitionsAndProfile()
        {
            var manifest = Stage32TerrainArtIngestionGenerator.LoadManifest();
            if (manifest == null || manifest.entries == null)
                return;

            var definitions = LoadDefinitionMap();
            var applied = 0;
            for (var i = 0; i < manifest.entries.Count; i++)
            {
                var entry = manifest.entries[i];
                TerrainPieceDefinition definition;
                if (entry == null || !entry.playerFacingReplacement || entry.generatedPrefab == null || !definitions.TryGetValue(entry.replacesPieceId, out definition))
                    continue;

                definition.prefab = entry.generatedPrefab;
                definition.supportsTint = false;
                definition.materialProfileId = "batch01_" + Slug(entry.category.ToString());
                definition.passabilityVisualHint = "Batch01 source-art visual. Rts.Core remains authoritative for passability.";
                definition.buildableVisualHint = "Batch01 source-art visual. Rts.Core remains authoritative for buildability.";
                definition.notes = "Stage32.5 Batch01 image-backed terrain replacement from " + entry.sourceAssetPath + ". Primitive proxy remains fallback/debug only.";
                EditorUtility.SetDirty(definition);
                applied++;
            }

            var profile = Stage32TerrainPieceGenerator.LoadPlayerFacingSetDressingProfile();
            if (profile != null && applied >= MinimumPlayerFacingReplacementCount)
            {
                profile.notes = "Stage32.5 player-facing terrain uses imported Batch01 source-art textured mesh planes/cards where source assets exist. Primitive Stage32 pieces are fallback/debug only.";
                profile.maxRenderedPieces = Math.Min(profile.maxRenderedPieces <= 0 ? 44 : profile.maxRenderedPieces, 44);
                EditorUtility.SetDirty(profile);
            }
        }

        public static Stage32_5ValidationSummary ValidateBatch01()
        {
            var summary = new Stage32_5ValidationSummary();
            var importSummary = EnsureBatch01TerrainArt();
            summary.SourceAssetCount = importSummary.SourceAssetCount;
            summary.PlayerFacingReplacementCount = importSummary.PlayerFacingReplacementCount;

            var package = LoadPackage();
            summary.MaterialCount = CountAssets<Material>(MaterialFolder);
            summary.PrefabCount = CountAssets<GameObject>(PrefabFolder);
            ValidateUnityManifest(summary, package);
            ValidateGeneratedPrefabs(summary, package);
            ValidateReviewScene(summary, package.assets.Count);
            ValidatePlayerFacingReplacements(summary);
            WriteQaReport(summary);

            if (summary.Errors.Count > 0)
                throw new InvalidOperationException("Stage32.5 Batch01 validation failed: " + string.Join(" | ", summary.Errors.ToArray()));

            Debug.Log("Stage 32.5 terrain art Batch01 validation passed. Assets: " + summary.SourceAssetCount + ", materials: " + summary.MaterialCount + ", prefabs: " + summary.PrefabCount);
            return summary;
        }

        static void ValidateUnityManifest(Stage32_5ValidationSummary summary, Batch01Package package)
        {
            var manifest = Stage32TerrainArtIngestionGenerator.LoadManifest();
            if (manifest == null || manifest.entries == null)
            {
                summary.Errors.Add("Unity TerrainArtManifest was not generated.");
                return;
            }

            if (package.assets == null || package.assets.Count < MinimumSourceAssetCount)
                summary.Errors.Add("JSON manifest has fewer than " + MinimumSourceAssetCount + " assets.");
            if (manifest.CountPlayerFacingReplacements() < MinimumPlayerFacingReplacementCount)
                summary.Errors.Add("Unity manifest has fewer than " + MinimumPlayerFacingReplacementCount + " player-facing source-art replacements.");
        }

        static void ValidateGeneratedPrefabs(Stage32_5ValidationSummary summary, Batch01Package package)
        {
            for (var i = 0; i < package.assets.Count; i++)
            {
                var asset = package.assets[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + asset.id + ".prefab");
                if (prefab == null)
                {
                    summary.Errors.Add(asset.id + ": canonical prefab missing.");
                    continue;
                }

                var sourceTag = prefab.GetComponent<TerrainArtSourceTag>();
                var batchTag = prefab.GetComponent<Stage32_5TerrainAssetTag>();
                if (sourceTag == null || !sourceTag.sourceImported || sourceTag.sourceKind != TerrainArtSourceKind.Texture)
                    summary.Errors.Add(asset.id + ": TerrainArtSourceTag missing or not texture-backed.");
                if (batchTag == null || !batchTag.IsComplete() || !batchTag.canonicalSourceAsset)
                    summary.Errors.Add(asset.id + ": Stage32_5TerrainAssetTag missing or incomplete.");

                var renderer = prefab.GetComponentInChildren<MeshRenderer>(true);
                var material = renderer != null ? renderer.sharedMaterial : null;
                if (renderer == null || material == null || material.mainTexture == null)
                    summary.Errors.Add(asset.id + ": prefab is not image-backed by a material texture.");
                else if (!AssetDatabase.GetAssetPath(material.mainTexture).StartsWith(SourceFolder + "/individual/", StringComparison.OrdinalIgnoreCase))
                    summary.Errors.Add(asset.id + ": prefab does not use the Batch01 individual PNG source texture.");

                if (prefab.GetComponentsInChildren<Collider>(true).Length > 0)
                    summary.Errors.Add(asset.id + ": visual terrain source prefab must not include colliders.");
            }
        }

        static void ValidateReviewScene(Stage32_5ValidationSummary summary, int expectedSourceAssets)
        {
            if (!File.Exists(ToAbsoluteProjectPath(ReviewScenePath)))
                summary.Errors.Add("Stage32.5 review scene is missing.");

            var scene = EditorSceneManager.OpenScene(ReviewScenePath);
            if (!scene.IsValid())
            {
                summary.Errors.Add("Stage32.5 review scene did not open.");
                return;
            }

            var tags = UnityEngine.Object.FindObjectsByType<TerrainArtSourceTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var canonical = 0;
            for (var i = 0; i < tags.Length; i++)
            {
                var stageTag = tags[i] != null ? tags[i].GetComponent<Stage32_5TerrainAssetTag>() : null;
                if (stageTag != null && stageTag.canonicalSourceAsset)
                    canonical++;
            }

            summary.ReviewSceneSourcePrefabCount = canonical;
            if (canonical < expectedSourceAssets)
                summary.Errors.Add("Stage32.5 review scene does not show all imported Batch01 source assets. Found " + canonical + ", expected " + expectedSourceAssets + ".");
        }

        static void ValidatePlayerFacingReplacements(Stage32_5ValidationSummary summary)
        {
            Stage16SceneCreator.CreateOrUpdateStage16Scene();

            var definitions = LoadDefinitionMap();
            var profile = Stage32TerrainPieceGenerator.LoadPlayerFacingSetDressingProfile();
            if (profile == null || profile.placements == null)
            {
                summary.Errors.Add("Stage32 player-facing set dressing profile missing.");
                return;
            }

            var sourcePlacements = 0;
            var proxyPlacements = new List<string>();
            for (var i = 0; i < profile.placements.Count; i++)
            {
                var placement = profile.placements[i];
                TerrainPieceDefinition definition;
                if (placement == null || !definitions.TryGetValue(placement.pieceId, out definition) || definition.prefab == null)
                    continue;

                var tag = definition.prefab.GetComponent<TerrainArtSourceTag>();
                if (tag != null && tag.IsPlayerFacingSourceArt())
                    sourcePlacements++;
                else
                    proxyPlacements.Add(placement.pieceId);
            }

            summary.PlayerFacingSourcePlacementCount = sourcePlacements;
            if (sourcePlacements < MinimumPlayerFacingReplacementCount)
                summary.Errors.Add("Player-facing terrain still uses too few imported Batch01 assets. Source placements: " + sourcePlacements + ".");
            if (proxyPlacements.Count > 0)
                summary.Errors.Add("Player-facing terrain still references primitive/proxy fallback pieces: " + string.Join(", ", proxyPlacements.ToArray()));
        }

        static void CreateOrUpdateReviewScene(List<Batch01Asset> assets, Dictionary<string, GameObject> canonicalById)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage32_5_TerrainArtBatch01Review";

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 14f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.043f, 1f);
            cameraObject.transform.position = new Vector3(0f, 21f, -17f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.85f;
            light.color = new Color(1f, 0.94f, 0.84f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(54f, -36f, 0f);

            var root = new GameObject("Stage32.5 Batch01 Source Art Review").transform;
            var categoryOffsets = BuildCategoryOffsets(assets);
            var categoryCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                GameObject prefab;
                if (!canonicalById.TryGetValue(asset.id, out prefab) || prefab == null)
                    continue;

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(root, false);
                instance.name = asset.id;
                var count = categoryCounts.ContainsKey(asset.category) ? categoryCounts[asset.category] : 0;
                categoryCounts[asset.category] = count + 1;
                var offset = categoryOffsets.ContainsKey(asset.category) ? categoryOffsets[asset.category] : Vector3.zero;
                instance.transform.localPosition = offset + new Vector3((count % 8) * 2.7f, 0f, (count / 8) * 2.35f);
            }

            var sampleRoot = new GameObject("Sample Battlefield Layout").transform;
            sampleRoot.SetParent(root, false);
            sampleRoot.localPosition = new Vector3(-10.5f, 0.02f, -9.2f);
            InstantiateSample(canonicalById, sampleRoot, "grass_ground_01", 0f, 0f, 0f, 1.2f);
            InstantiateSample(canonicalById, sampleRoot, "grass_ground_02", 2.2f, 0.1f, 0f, 1.05f);
            InstantiateSample(canonicalById, sampleRoot, "road_straight_01", 4.2f, 0.2f, 90f, 1f);
            InstantiateSample(canonicalById, sampleRoot, "road_crossing_01", 6.6f, 0.2f, 0f, 1f);
            InstantiateSample(canonicalById, sampleRoot, "concrete_pad_medium", 2f, 2.2f, 0f, 0.95f);
            InstantiateSample(canonicalById, sampleRoot, "resource_cluster_blue_01", 7.8f, 3.1f, 20f, 0.85f);
            InstantiateSample(canonicalById, sampleRoot, "rock_blocker_01", 9.4f, 1.3f, -18f, 0.9f);
            InstantiateSample(canonicalById, sampleRoot, "tree_cluster_01", 0.2f, 4.0f, 15f, 0.85f);
            InstantiateSample(canonicalById, sampleRoot, "wreckage_small_01", 5.4f, 4.5f, -24f, 0.85f);

            EditorSceneManager.SaveScene(scene, ReviewScenePath);
        }

        static void InstantiateSample(Dictionary<string, GameObject> canonicalById, Transform parent, string assetId, float x, float z, float yRotation, float scale)
        {
            GameObject prefab;
            if (!canonicalById.TryGetValue(assetId, out prefab) || prefab == null)
                return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = new Vector3(x, 0.02f, z);
            instance.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            instance.transform.localScale = Vector3.one * scale;
        }

        static Dictionary<string, Vector3> BuildCategoryOffsets(List<Batch01Asset> assets)
        {
            var offsets = new Dictionary<string, Vector3>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();
            for (var i = 0; i < assets.Count; i++)
                if (!order.Contains(assets[i].category))
                    order.Add(assets[i].category);

            for (var i = 0; i < order.Count; i++)
                offsets[order[i]] = new Vector3(-10.8f, 0f, -4.2f + i * 2.7f);
            return offsets;
        }

        static GameObject CreatePrefab(Batch01Asset asset, string prefabId, string assetIdForMaterial, Material material, bool canonical, TerrainPieceDefinition definition)
        {
            var root = new GameObject(prefabId);
            var mesh = CreateMesh(prefabId, definition != null ? definition.footprintFineWidth : asset.fineGridSize.x, definition != null ? definition.footprintFineHeight : asset.fineGridSize.y);
            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            var category = definition != null ? definition.category : ToTerrainCategory(asset.category);
            var sizeClass = definition != null ? definition.sizeClass : ToSizeClass(asset);
            var validation = root.AddComponent<TerrainPieceValidationTag>();
            validation.pieceId = prefabId;
            validation.displayName = ObjectNames.NicifyVariableName(prefabId);
            validation.category = category;
            validation.sizeClass = sizeClass;
            validation.footprintFineWidth = definition != null ? definition.footprintFineWidth : asset.fineGridSize.x;
            validation.footprintFineHeight = definition != null ? definition.footprintFineHeight : asset.fineGridSize.y;
            validation.materialProfileId = "batch01_" + Slug(asset.category);
            validation.passabilityVisualHint = "Batch01 source-art passable=" + asset.passable + "; Rts.Core remains authoritative.";
            validation.buildableVisualHint = "Batch01 source-art buildable=" + asset.buildable + "; Rts.Core remains authoritative.";
            validation.supportsRotation = true;
            validation.supportsTint = false;
            validation.isGameplayBlockingVisualOnly = !asset.passable;
            validation.questBudgetTag = "QuestSafeBatch01Texture";
            validation.rendererCount = 1;
            validation.primitiveCount = 1;
            validation.notes = "Stage32.5 image-backed terrain mesh plane using " + asset.transparentPng + ".";

            var sourceTag = root.AddComponent<TerrainArtSourceTag>();
            sourceTag.batchId = Stage32TerrainArtIngestionGenerator.BatchId;
            sourceTag.artId = asset.id;
            sourceTag.replacesPieceId = prefabId;
            sourceTag.sourceKind = TerrainArtSourceKind.Texture;
            sourceTag.sourceAssetPath = SourceFolder + "/" + NormalizeAssetRelativePath(asset.transparentPng);
            sourceTag.sourceImported = true;
            sourceTag.coreBatch = true;
            sourceTag.playerFacingReplacement = !canonical;
            sourceTag.uvRect = new Vector4(0f, 0f, 1f, 1f);

            var batchTag = root.AddComponent<Stage32_5TerrainAssetTag>();
            batchTag.assetId = asset.id;
            batchTag.category = asset.category;
            batchTag.fineGridWidth = asset.fineGridSize.x;
            batchTag.fineGridHeight = asset.fineGridSize.y;
            batchTag.passable = asset.passable;
            batchTag.buildable = asset.buildable;
            batchTag.transparentPngPath = SourceFolder + "/" + NormalizeAssetRelativePath(asset.transparentPng);
            batchTag.fallbackCardPngPath = SourceFolder + "/" + NormalizeAssetRelativePath(asset.cardPng);
            batchTag.canonicalSourceAsset = canonical;
            batchTag.mappedStage32Replacement = !canonical;

            var prefabPath = (canonical ? PrefabFolder : MappedPrefabFolder) + "/" + prefabId + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static void ApplyDefinitionReplacement(TerrainPieceDefinition definition, Batch01Asset asset, GameObject prefab)
        {
            definition.prefab = prefab;
            definition.supportsTint = false;
            definition.materialProfileId = "batch01_" + Slug(asset.category);
            definition.passabilityVisualHint = "Batch01 source-art passable=" + asset.passable + "; Rts.Core terrain remains authoritative.";
            definition.buildableVisualHint = "Batch01 source-art buildable=" + asset.buildable + "; Rts.Core placement remains authoritative.";
            definition.isGameplayBlockingVisualOnly = !asset.passable;
            definition.notes = "Stage32.5 source-art replacement from " + asset.transparentPng + ". Primitive Stage32 prefab remains fallback/debug only.";
            definition.questBudgetTag = "QuestSafeBatch01Texture";
            EditorUtility.SetDirty(definition);
        }

        static TerrainArtManifestEntry CreateManifestEntry(Batch01Asset asset, string replacesPieceId, string texturePath, Material material, GameObject prefab, bool canonical)
        {
            return new TerrainArtManifestEntry
            {
                artId = asset.id,
                displayName = ObjectNames.NicifyVariableName(asset.id),
                replacesPieceId = replacesPieceId,
                category = ToTerrainCategory(asset.category),
                sourceKind = TerrainArtSourceKind.Texture,
                sourceAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath),
                sourceAssetPath = texturePath,
                generatedMaterial = material,
                generatedPrefab = prefab,
                uvRect = new Vector4(0f, 0f, 1f, 1f),
                coreBatch = true,
                playerFacingReplacement = !canonical,
                notes = canonical ? "Canonical Stage32.5 Batch01 source terrain asset." : "Mapped Stage32 player-facing replacement using Stage32.5 Batch01 source art."
            };
        }

        static Material CreateMaterial(Batch01Asset asset, Texture2D texture)
        {
            var path = MaterialFolder + "/" + asset.id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = asset.id };
                AssetDatabase.CreateAsset(material, path);
            }

            material.mainTexture = texture;
            material.color = Color.white;
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_AlphaClip"))
                material.SetFloat("_AlphaClip", 0f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.12f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = 3000;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Mesh CreateMesh(string id, int fineWidth, int fineHeight)
        {
            var path = MeshFolder + "/" + id + "_mesh.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = id + "_mesh" };
                AssetDatabase.CreateAsset(mesh, path);
            }

            var width = Mathf.Max(0.75f, fineWidth * 0.42f);
            var depth = Mathf.Max(0.75f, fineHeight * 0.42f);
            var halfWidth = width * 0.5f;
            var halfDepth = depth * 0.5f;
            mesh.Clear();
            mesh.vertices = new[]
            {
                new Vector3(-halfWidth, 0f, -halfDepth),
                new Vector3(-halfWidth, 0f, halfDepth),
                new Vector3(halfWidth, 0f, halfDepth),
                new Vector3(halfWidth, 0f, -halfDepth)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f)
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        static void ValidateAssetFiles(Batch01Asset asset)
        {
            if (asset == null || string.IsNullOrEmpty(asset.id))
                throw new InvalidOperationException("Batch01 manifest contains an unnamed asset.");
            if (!File.Exists(ToAbsoluteProjectPath(SourceFolder + "/" + NormalizeAssetRelativePath(asset.transparentPng))))
                throw new FileNotFoundException(asset.id + " transparent PNG is missing.", asset.transparentPng);
            if (!string.IsNullOrEmpty(asset.cardPng) && !File.Exists(ToAbsoluteProjectPath(SourceFolder + "/" + NormalizeAssetRelativePath(asset.cardPng))))
                throw new FileNotFoundException(asset.id + " fallback card PNG is missing.", asset.cardPng);
            if (asset.fineGridSize == null || asset.fineGridSize.x <= 0 || asset.fineGridSize.y <= 0)
                throw new InvalidOperationException(asset.id + " has invalid fineGridSize metadata.");
        }

        static void ConfigureSourceTextureImports()
        {
            AssetDatabase.Refresh();
            var sourceAbsolute = ToAbsoluteProjectPath(SourceFolder);
            if (!Directory.Exists(sourceAbsolute))
                throw new DirectoryNotFoundException(sourceAbsolute);

            var pngs = Directory.GetFiles(sourceAbsolute, "*.png", SearchOption.AllDirectories);
            for (var i = 0; i < pngs.Length; i++)
            {
                var assetPath = ToAssetPath(pngs[i]);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        static Batch01Package LoadPackage()
        {
            var json = File.ReadAllText(ToAbsoluteProjectPath(ManifestJsonPath));
            var package = JsonUtility.FromJson<Batch01Package>(json);
            if (package == null)
                throw new InvalidOperationException("Unable to parse Stage32.5 Batch01 JSON manifest.");
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

        static TerrainArtManifest LoadOrCreateUnityManifest()
        {
            var manifest = AssetDatabase.LoadAssetAtPath<TerrainArtManifest>(Stage32TerrainArtIngestionGenerator.ManifestPath);
            if (manifest != null)
                return manifest;

            EnsureFolderRecursive("Assets/Rts/ScriptableObjects/Art/TerrainPieces");
            manifest = ScriptableObject.CreateInstance<TerrainArtManifest>();
            AssetDatabase.CreateAsset(manifest, Stage32TerrainArtIngestionGenerator.ManifestPath);
            return manifest;
        }

        static int CountAssets<T>(string folder) where T : UnityEngine.Object
        {
            var count = 0;
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            for (var i = 0; i < guids.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (asset != null)
                    count++;
            }

            return count;
        }

        static TerrainPieceCategory ToTerrainCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return TerrainPieceCategory.Ground;
            if (category.IndexOf("resource", StringComparison.OrdinalIgnoreCase) >= 0)
                return TerrainPieceCategory.Resource;
            if (category.IndexOf("road", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("transition", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("edge", StringComparison.OrdinalIgnoreCase) >= 0)
                return TerrainPieceCategory.Transition;
            if (category.IndexOf("foundation", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("base", StringComparison.OrdinalIgnoreCase) >= 0)
                return TerrainPieceCategory.BaseConstruction;
            if (category.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("obstacle", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("wreckage", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("cover", StringComparison.OrdinalIgnoreCase) >= 0)
                return TerrainPieceCategory.Obstacle;
            if (category.IndexOf("prop", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("vegetation", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("debris", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("damage", StringComparison.OrdinalIgnoreCase) >= 0 || category.IndexOf("decal", StringComparison.OrdinalIgnoreCase) >= 0)
                return TerrainPieceCategory.Prop;
            return TerrainPieceCategory.Ground;
        }

        static TerrainPieceSizeClass ToSizeClass(Batch01Asset asset)
        {
            if (asset.fineGridSize.x <= 2 && asset.fineGridSize.y <= 2)
                return TerrainPieceSizeClass.Small;
            if (asset.fineGridSize.x >= 6 || asset.fineGridSize.y >= 6)
                return TerrainPieceSizeClass.Large;
            if (asset.fineGridSize.x >= asset.fineGridSize.y * 2)
                return TerrainPieceSizeClass.Strip;
            if (asset.fineGridSize.y == 1)
                return TerrainPieceSizeClass.Edge;
            return TerrainPieceSizeClass.Patch;
        }

        static void EnsureFolders()
        {
            EnsureFolderRecursive(SourceFolder);
            EnsureFolderRecursive(MaterialFolder);
            EnsureFolderRecursive(MeshFolder);
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

        static string NormalizeAssetRelativePath(string relativePath)
        {
            return string.IsNullOrEmpty(relativePath) ? string.Empty : relativePath.Replace('\\', '/').TrimStart('/');
        }

        static string ToAbsoluteProjectPath(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        static string ToAssetPath(string absolutePath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/').TrimEnd('/');
            var full = Path.GetFullPath(absolutePath).Replace('\\', '/');
            if (!full.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase))
                return null;
            return full.Substring(projectRoot.Length + 1);
        }

        static string Slug(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "unnamed";

            var chars = value.ToLowerInvariant().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            return new string(chars).Trim('_');
        }

        static void WriteQaReport(Stage32_5ValidationSummary summary)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, "docs", "STAGE32_5_TERRAIN_ART_BATCH01_QA.md");
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 32.5 Terrain Art Batch01 QA");
            builder.AppendLine();
            builder.AppendLine("- Source assets: " + summary.SourceAssetCount);
            builder.AppendLine("- Materials: " + summary.MaterialCount);
            builder.AppendLine("- Prefabs: " + summary.PrefabCount);
            builder.AppendLine("- Player-facing replacements: " + summary.PlayerFacingReplacementCount);
            builder.AppendLine("- Player-facing source placements: " + summary.PlayerFacingSourcePlacementCount);
            builder.AppendLine("- Review scene source prefabs: " + summary.ReviewSceneSourcePrefabCount);
            builder.AppendLine("- Errors: " + summary.Errors.Count);
            builder.AppendLine();
            builder.AppendLine("## Result");
            if (summary.Errors.Count == 0)
                builder.AppendLine("- Stage32.5 Batch01 source-art validation passed.");
            else
                for (var i = 0; i < summary.Errors.Count; i++)
                    builder.AppendLine("- " + summary.Errors[i]);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        static void NormalizeGeneratedTextArtifacts()
        {
            var roots = new[]
            {
                SourceFolder,
                MaterialFolder,
                MeshFolder,
                PrefabFolder
            };

            for (var i = 0; i < roots.Length; i++)
            {
                var root = ToAbsoluteProjectPath(roots[i]);
                NormalizeTextFile(root + ".meta");
                if (!Directory.Exists(root))
                    continue;

                foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                    NormalizeTextFile(file);
            }

            NormalizeTextFile(ToAbsoluteProjectPath(ReviewScenePath));
            NormalizeTextFile(ToAbsoluteProjectPath(ReviewScenePath + ".meta"));
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
                string.Equals(extension, ".unity", StringComparison.OrdinalIgnoreCase);
        }

        [Serializable]
        sealed class Batch01Package
        {
            public string packageName;
            public string version;
            public string source;
            public string intendedUse;
            public string importFolder;
            public List<Batch01Asset> assets;
        }

        [Serializable]
        sealed class Batch01Asset
        {
            public string id;
            public string category;
            public string transparentPng;
            public string cardPng;
            public string sourceSheet;
            public FineGridSize fineGridSize;
            public bool passable;
            public bool buildable;
            public string notes;
        }

        [Serializable]
        sealed class FineGridSize
        {
            public int x;
            public int y;
        }
    }

    public sealed class Stage32_5ValidationSummary
    {
        public int SourceAssetCount;
        public int MaterialCount;
        public int PrefabCount;
        public int PlayerFacingReplacementCount;
        public int PlayerFacingSourcePlacementCount;
        public int ReviewSceneSourcePrefabCount;
        public readonly List<string> Errors = new List<string>();
    }
}
