using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32TerrainArtIngestionGenerator
    {
        public const string BatchId = "Batch01";
        public const string SourceFolder = "Assets/Rts/Art/Source/Terrain/Batch01";
        public const string ManifestPath = "Assets/Rts/ScriptableObjects/Art/TerrainPieces/stage32_terrain_art_manifest.asset";
        public const string GeneratedMaterialFolder = "Assets/Rts/Art/Materials/Terrain/Batch01Imported";
        public const string GeneratedPrefabFolder = "Assets/Rts/Art/Prefabs/Terrain/Batch01Imported";
        public const string GeneratedMeshFolder = "Assets/Rts/Art/Meshes/Terrain/Batch01Imported";
        public const int MinimumPlayerFacingSourceReplacements = 32;

        public static readonly string[] RequiredPlayerFacingReplacementPieceIds =
        {
            "base_foundation_pad_01",
            "base_foundation_pad_02",
            "base_foundation_pad_03",
            "base_production_apron_01",
            "base_production_apron_02",
            "base_road_strip_01",
            "base_road_strip_02",
            "base_rally_exit_marking_01",
            "base_rally_exit_marking_02",
            "ground_compact_soil_patch_01",
            "ground_compact_soil_patch_02",
            "ground_scorched_patch_01",
            "ground_mud_patch_01",
            "transition_concrete_ground_edge_01",
            "transition_buildable_edge_01",
            "transition_dirt_road_blend_01",
            "resource_cluster_01",
            "resource_cluster_02",
            "resource_rich_cluster_01",
            "resource_decal_01",
            "resource_harvest_marker_01",
            "transition_resource_edge_01",
            "obstacle_rock_cluster_01",
            "obstacle_rock_cluster_02",
            "obstacle_ridge_piece_01",
            "obstacle_cliff_blocker_chunk_01",
            "obstacle_tree_bush_cluster_01",
            "obstacle_wreckage_01",
            "obstacle_debris_01",
            "prop_sandbag_01",
            "prop_sandbag_02",
            "prop_barrier_01",
            "prop_tank_trap_01",
            "prop_tire_tracks_01",
            "prop_tire_tracks_02",
            "prop_shell_mark_01",
            "prop_crates_01",
            "prop_antenna_beacon_01",
            "prop_destroyed_vehicle_proxy_01",
            "transition_rock_edge_01",
            "ground_road_path_01",
            "ground_grass_dirt_patch_01",
            "ground_grass_dirt_patch_02",
            "ground_rocky_blocked_01"
        };

        [MenuItem("ProjectAegisRTS/Stage 32/Ingest Terrain Art Batch01")]
        public static void IngestBatch01TerrainArtMenu()
        {
            EnsureBatch01TerrainArt();
        }

        public static void IngestBatch01TerrainArtBatch()
        {
            try
            {
                var summary = EnsureBatch01TerrainArt();
                Debug.Log("Stage 32 terrain art ingestion completed. Source assets: " + summary.SourceAssetCount + ", player-facing replacements: " + summary.PlayerFacingReplacementCount);
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
            EnsureFolders();
            SeedBatch01FromStage31ReferenceSheets();
            AssetDatabase.Refresh();

            ConfigureTextureImports();
            var sourcePaths = FindSupportedSourceAssetPaths();
            var summary = new Stage32TerrainArtIngestionSummary { SourceAssetCount = sourcePaths.Count };
            var manifest = LoadOrCreateManifest();
            manifest.batchId = BatchId;
            manifest.sourceFolder = SourceFolder;
            manifest.entries = new List<TerrainArtManifestEntry>();

            var definitionById = LoadDefinitionMap();
            var replacementSpecs = BuildReplacementSpecs(sourcePaths);
            for (var i = 0; i < replacementSpecs.Count; i++)
            {
                var spec = replacementSpecs[i];
                TerrainPieceDefinition definition;
                definitionById.TryGetValue(spec.PieceId, out definition);
                var entry = CreateReplacementEntry(spec, definition);
                manifest.entries.Add(entry);
                if (entry.playerFacingReplacement && entry.generatedPrefab != null)
                    summary.PlayerFacingReplacementCount++;
            }

            AddGenericSourceEntries(sourcePaths, manifest);
            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return summary;
        }

        public static TerrainArtManifest LoadManifest()
        {
            return AssetDatabase.LoadAssetAtPath<TerrainArtManifest>(ManifestPath);
        }

        public static bool HasPlayerFacingSourceArt()
        {
            var manifest = LoadManifest();
            return manifest != null && manifest.CountPlayerFacingReplacements() >= MinimumPlayerFacingSourceReplacements;
        }

        public static bool IsSourceBackedPlayerFacingPiece(string pieceId)
        {
            var manifest = LoadManifest();
            return manifest != null && manifest.FindReplacement(pieceId) != null;
        }

        static TerrainArtManifestEntry CreateReplacementEntry(TerrainArtReplacementSpec spec, TerrainPieceDefinition definition)
        {
            var sourceAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(spec.SourceAssetPath);
            var texture = sourceAsset as Texture2D;
            var prefab = texture != null
                ? CreateTexturePrefab(spec, texture, definition)
                : CreateModelOrPrefabWrapper(spec, sourceAsset, definition);

            if (definition != null && prefab != null)
            {
                definition.prefab = prefab;
                definition.notes = "Player-facing source art replacement from " + spec.SourceAssetPath + ". Old generated proxy prefab remains available for fallback/debug. Rts.Core terrain remains authoritative.";
                EditorUtility.SetDirty(definition);
            }

            return new TerrainArtManifestEntry
            {
                artId = spec.ArtId,
                displayName = spec.DisplayName,
                replacesPieceId = spec.PieceId,
                category = spec.Category,
                sourceKind = texture != null ? TerrainArtSourceKind.Texture : spec.SourceKind,
                sourceAsset = sourceAsset,
                sourceAssetPath = spec.SourceAssetPath,
                generatedMaterial = spec.GeneratedMaterial,
                generatedPrefab = prefab,
                uvRect = spec.UvRect,
                coreBatch = true,
                playerFacingReplacement = true,
                notes = spec.Notes
            };
        }

        static GameObject CreateTexturePrefab(TerrainArtReplacementSpec spec, Texture2D texture, TerrainPieceDefinition definition)
        {
            var material = CreateTextureMaterial(spec, texture);
            var root = new GameObject(spec.PieceId);
            var mesh = CreateOrUpdatePlaneMesh(spec);
            var meshFilter = root.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            var meshRenderer = root.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            AddValidationTags(root, spec, definition, TerrainArtSourceKind.Texture);
            AddLodGroup(root, meshRenderer);

            var path = GeneratedPrefabFolder + "/" + spec.ArtId + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject CreateModelOrPrefabWrapper(TerrainArtReplacementSpec spec, UnityEngine.Object sourceAsset, TerrainPieceDefinition definition)
        {
            if (sourceAsset == null)
                return null;

            var sourceGameObject = sourceAsset as GameObject;
            if (sourceGameObject == null)
                return null;

            var root = new GameObject(spec.PieceId);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(sourceGameObject);
            instance.transform.SetParent(root.transform, false);
            instance.name = "SourceModel";
            AddValidationTags(root, spec, definition, TerrainArtSourceKind.Model);
            AddLodGroup(root, root.GetComponentsInChildren<Renderer>(true));

            var path = GeneratedPrefabFolder + "/" + spec.ArtId + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static void AddValidationTags(GameObject root, TerrainArtReplacementSpec spec, TerrainPieceDefinition definition, TerrainArtSourceKind sourceKind)
        {
            var category = definition != null ? definition.category : spec.Category;
            var sizeClass = definition != null ? definition.sizeClass : spec.SizeClass;
            var width = definition != null ? definition.footprintFineWidth : spec.FootprintFineWidth;
            var height = definition != null ? definition.footprintFineHeight : spec.FootprintFineHeight;
            var materialId = definition != null ? definition.materialProfileId : spec.MaterialProfileId;
            var passability = definition != null ? definition.passabilityVisualHint : spec.PassabilityHint;
            var buildable = definition != null ? definition.buildableVisualHint : spec.BuildableHint;
            var blocking = definition != null ? definition.isGameplayBlockingVisualOnly : spec.BlockingVisualOnly;
            var questBudget = definition != null ? definition.questBudgetTag : "QuestSafeSourceArt";

            var validationTag = root.AddComponent<TerrainPieceValidationTag>();
            validationTag.pieceId = spec.PieceId;
            validationTag.displayName = spec.DisplayName;
            validationTag.category = category;
            validationTag.sizeClass = sizeClass;
            validationTag.footprintFineWidth = width;
            validationTag.footprintFineHeight = height;
            validationTag.materialProfileId = string.IsNullOrEmpty(materialId) ? "source_art" : materialId;
            validationTag.passabilityVisualHint = string.IsNullOrEmpty(passability) ? "Visual source art; core terrain remains authoritative." : passability;
            validationTag.buildableVisualHint = string.IsNullOrEmpty(buildable) ? "Visual source art; buildability remains authoritative in Rts.Core." : buildable;
            validationTag.supportsRotation = true;
            validationTag.supportsTint = false;
            validationTag.isGameplayBlockingVisualOnly = blocking;
            validationTag.questBudgetTag = string.IsNullOrEmpty(questBudget) ? "QuestSafeSourceArt" : questBudget;
            validationTag.rendererCount = Math.Max(1, root.GetComponentsInChildren<Renderer>(true).Length);
            validationTag.primitiveCount = 1;
            validationTag.notes = "Imported terrain source art from " + spec.SourceAssetPath + ". This replaces the player-facing Stage32 proxy for this piece.";

            var sourceTag = root.AddComponent<TerrainArtSourceTag>();
            sourceTag.batchId = BatchId;
            sourceTag.artId = spec.ArtId;
            sourceTag.replacesPieceId = spec.PieceId;
            sourceTag.sourceKind = sourceKind;
            sourceTag.sourceAssetPath = spec.SourceAssetPath;
            sourceTag.sourceImported = true;
            sourceTag.coreBatch = true;
            sourceTag.playerFacingReplacement = true;
            sourceTag.uvRect = spec.UvRect;
        }

        static Material CreateTextureMaterial(TerrainArtReplacementSpec spec, Texture2D texture)
        {
            var path = GeneratedMaterialFolder + "/" + spec.ArtId + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = spec.ArtId };
                AssetDatabase.CreateAsset(material, path);
            }

            material.mainTexture = texture;
            material.mainTextureScale = new Vector2(spec.UvRect.z, spec.UvRect.w);
            material.mainTextureOffset = new Vector2(spec.UvRect.x, spec.UvRect.y);
            material.color = Color.white;
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", new Vector2(spec.UvRect.z, spec.UvRect.w));
                material.SetTextureOffset("_BaseMap", new Vector2(spec.UvRect.x, spec.UvRect.y));
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
                material.SetTextureScale("_MainTex", new Vector2(spec.UvRect.z, spec.UvRect.w));
                material.SetTextureOffset("_MainTex", new Vector2(spec.UvRect.x, spec.UvRect.y));
            }
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.18f);
            material.renderQueue = 2450;

            spec.GeneratedMaterial = material;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Mesh CreateOrUpdatePlaneMesh(TerrainArtReplacementSpec spec)
        {
            var path = GeneratedMeshFolder + "/" + spec.ArtId + "_mesh.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = spec.ArtId + "_mesh" };
                AssetDatabase.CreateAsset(mesh, path);
            }

            var width = Mathf.Max(0.45f, spec.FootprintFineWidth * 0.62f);
            var depth = Mathf.Max(0.45f, spec.FootprintFineHeight * 0.62f);
            mesh.Clear();
            mesh.vertices = new[]
            {
                new Vector3(-width * 0.5f, 0.075f, -depth * 0.5f),
                new Vector3(-width * 0.5f, 0.075f, depth * 0.5f),
                new Vector3(width * 0.5f, 0.075f, depth * 0.5f),
                new Vector3(width * 0.5f, 0.075f, -depth * 0.5f)
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

        static void AddLodGroup(GameObject root, params Renderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0)
                renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            var lodGroup = root.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[] { new LOD(0.50f, renderers), new LOD(0.14f, renderers) });
            lodGroup.RecalculateBounds();
        }

        static void AddGenericSourceEntries(List<string> sourcePaths, TerrainArtManifest manifest)
        {
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < manifest.entries.Count; i++)
                if (manifest.entries[i] != null && !string.IsNullOrEmpty(manifest.entries[i].sourceAssetPath))
                    used.Add(manifest.entries[i].sourceAssetPath);

            for (var i = 0; i < sourcePaths.Count; i++)
            {
                if (used.Contains(sourcePaths[i]))
                    continue;

                var source = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePaths[i]);
                manifest.entries.Add(new TerrainArtManifestEntry
                {
                    artId = "source_" + Slug(Path.GetFileNameWithoutExtension(sourcePaths[i])),
                    displayName = ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(sourcePaths[i])),
                    sourceAsset = source,
                    sourceAssetPath = sourcePaths[i],
                    sourceKind = source is Texture2D ? TerrainArtSourceKind.Texture : TerrainArtSourceKind.Model,
                    coreBatch = false,
                    playerFacingReplacement = false,
                    notes = "Imported source asset is registered for artist review but is not mapped to a player-facing terrain piece yet."
                });
            }
        }

        static List<TerrainArtReplacementSpec> BuildReplacementSpecs(List<string> sourcePaths)
        {
            var specs = new List<TerrainArtReplacementSpec>();
            var textures = FilterTexturePaths(sourcePaths);
            if (textures.Count == 0)
                return specs;

            var fullKit = FindSource(textures, "full_kit") ?? textures[0];
            var boardLayout = FindSource(textures, "board_layout") ?? fullKit;
            var roads = FindSource(textures, "road_base_edges") ?? fullKit;
            var cliffs = FindSource(textures, "cliffs_resources_props") ?? fullKit;

            Add(specs, "base_foundation_pad_01", "Source Base Foundation Pad 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "foundation", boardLayout, TopUv(.06f, .60f, .18f, .20f), "Base pad sourced from Batch01 board-layout/base art.");
            Add(specs, "base_foundation_pad_02", "Source Base Foundation Pad 02", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "foundation", boardLayout, TopUv(.24f, .60f, .18f, .20f), "Base pad sourced from Batch01 board-layout/base art.");
            Add(specs, "base_foundation_pad_03", "Source Base Foundation Pad 03", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "foundation", roads, TopUv(.04f, .37f, .14f, .13f), "Base pad crop from Batch01 road/base source sheet.");
            Add(specs, "base_production_apron_01", "Source Production Apron 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 2, "foundation", roads, TopUv(.30f, .37f, .22f, .13f), "Industrial apron crop from Batch01 road/base source sheet.");
            Add(specs, "base_production_apron_02", "Source Production Apron 02", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 2, "foundation", roads, TopUv(.53f, .37f, .22f, .13f), "Industrial apron crop from Batch01 road/base source sheet.");
            Add(specs, "base_road_strip_01", "Source Base Road Strip 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 1, "road", roads, TopUv(.04f, .05f, .14f, .12f), "Road strip crop from Batch01 road/base source sheet.");
            Add(specs, "base_road_strip_02", "Source Base Road Strip 02", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 1, "road", roads, TopUv(.20f, .05f, .14f, .12f), "Road strip crop from Batch01 road/base source sheet.");
            Add(specs, "base_rally_exit_marking_01", "Source Rally Exit Marking 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Small, 3, 1, "caution", roads, TopUv(.48f, .06f, .16f, .12f), "Intersection marking crop from Batch01 road/base source sheet.");
            Add(specs, "base_rally_exit_marking_02", "Source Rally Exit Marking 02", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Small, 3, 1, "caution", roads, TopUv(.50f, .20f, .16f, .12f), "Lane marking crop from Batch01 road/base source sheet.");

            Add(specs, "ground_compact_soil_patch_01", "Source Compact Soil Patch 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 2, "compact_soil", fullKit, TopUv(.27f, .06f, .10f, .10f), "Compacted dirt crop from Batch01 full-kit source sheet.");
            Add(specs, "ground_compact_soil_patch_02", "Source Compact Soil Patch 02", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 2, "compact_soil", fullKit, TopUv(.39f, .06f, .10f, .10f), "Damaged dirt crop from Batch01 full-kit source sheet.");
            Add(specs, "ground_scorched_patch_01", "Source Scorched Patch 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 2, 2, "scorch", cliffs, TopUv(.34f, .31f, .10f, .11f), "Scorch/crater crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "ground_mud_patch_01", "Source Mud Patch 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 2, 2, "mud", fullKit, TopUv(.39f, .17f, .10f, .10f), "Mud/damaged dirt crop from Batch01 full-kit source sheet.");
            Add(specs, "ground_road_path_01", "Source Road Path 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Strip, 5, 2, "road", roads, TopUv(.04f, .05f, .14f, .12f), "Road surface crop from Batch01 road/base source sheet.");
            Add(specs, "ground_grass_dirt_patch_01", "Source Grass Dirt Patch 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 3, "grass_dirt", fullKit, TopUv(.03f, .06f, .10f, .10f), "Grass/dirt crop from Batch01 full-kit source sheet.");
            Add(specs, "ground_grass_dirt_patch_02", "Source Grass Dirt Patch 02", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 3, "grass_dirt", fullKit, TopUv(.15f, .06f, .10f, .10f), "Alternate grass/dirt crop from Batch01 full-kit source sheet.");
            Add(specs, "ground_rocky_blocked_01", "Source Rocky Blocked 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Medium, 3, 3, "rock", cliffs, TopUv(.72f, .04f, .10f, .11f), "Rock blocker crop from Batch01 cliffs/resources/props sheet.");

            Add(specs, "transition_concrete_ground_edge_01", "Source Concrete Ground Edge 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 1, "concrete", roads, TopUv(.03f, .51f, .16f, .11f), "Concrete edge crop from Batch01 road/base source sheet.");
            Add(specs, "transition_buildable_edge_01", "Source Buildable Edge 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 1, "foundation", roads, TopUv(.72f, .52f, .16f, .08f), "Foundation/border crop from Batch01 road/base source sheet.");
            Add(specs, "transition_dirt_road_blend_01", "Source Dirt Road Blend 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 1, "road", fullKit, TopUv(.51f, .17f, .10f, .10f), "Road/dirt transition crop from Batch01 full-kit source sheet.");
            Add(specs, "transition_resource_edge_01", "Source Resource Edge 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 3, 1, "resource_ground", cliffs, TopUv(.16f, .24f, .10f, .10f), "Resource ground edge crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "transition_rock_edge_01", "Source Rock Edge 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 3, 1, "rock", cliffs, TopUv(.04f, .04f, .16f, .11f), "Rock edge crop from Batch01 cliffs/resources/props sheet.");

            Add(specs, "resource_cluster_01", "Source Resource Cluster 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "mineral", cliffs, TopUv(.03f, .24f, .10f, .10f), "Blue resource crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "resource_cluster_02", "Source Resource Cluster 02", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "mineral", cliffs, TopUv(.13f, .24f, .10f, .10f), "Green resource crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "resource_rich_cluster_01", "Source Rich Resource Cluster 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "rich_mineral", cliffs, TopUv(.23f, .24f, .10f, .10f), "Rich resource crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "resource_decal_01", "Source Resource Decal 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Patch, 3, 3, "resource_ground", fullKit, TopUv(.15f, .37f, .10f, .10f), "Resource-field ground crop from Batch01 full-kit source sheet.");
            Add(specs, "resource_harvest_marker_01", "Source Resource Harvest Marker 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Tiny, 2, 1, "caution", roads, TopUv(.70f, .08f, .08f, .08f), "Small harvest-route marker crop from Batch01 road/base source sheet.");

            Add(specs, "obstacle_rock_cluster_01", "Source Rock Cluster 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "rock", cliffs, TopUv(.72f, .05f, .10f, .10f), "Rock cluster crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "obstacle_rock_cluster_02", "Source Rock Cluster 02", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "rock", cliffs, TopUv(.84f, .05f, .09f, .10f), "Alternate rock cluster crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "obstacle_ridge_piece_01", "Source Ridge Piece 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Edge, 4, 1, "rock", cliffs, TopUv(.04f, .04f, .20f, .10f), "Ridge crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "obstacle_cliff_blocker_chunk_01", "Source Cliff Blocker Chunk 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Medium, 3, 2, "rock", cliffs, TopUv(.32f, .04f, .18f, .11f), "Cliff blocker crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "obstacle_tree_bush_cluster_01", "Source Tree Bush Cluster 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "foliage", cliffs, TopUv(.10f, .76f, .10f, .10f), "Foliage crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "obstacle_wreckage_01", "Source Wreckage 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 3, 2, "dark_metal", cliffs, TopUv(.03f, .41f, .12f, .11f), "Wreckage crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "obstacle_debris_01", "Source Debris 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "metal", cliffs, TopUv(.38f, .42f, .10f, .10f), "Debris crop from Batch01 cliffs/resources/props sheet.");

            Add(specs, "prop_sandbag_01", "Source Sandbag 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 1, "sandbag", cliffs, TopUv(.04f, .52f, .13f, .08f), "Sandbag crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "prop_sandbag_02", "Source Sandbag 02", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 1, "sandbag", cliffs, TopUv(.17f, .52f, .13f, .08f), "Curved sandbag crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "prop_barrier_01", "Source Barrier 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 3, 1, "metal", cliffs, TopUv(.47f, .52f, .10f, .08f), "Barrier crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "prop_tank_trap_01", "Source Tank Trap 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "dark_metal", cliffs, TopUv(.04f, .62f, .10f, .08f), "Tank-trap crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "prop_tire_tracks_01", "Source Tire Tracks 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Strip, 4, 1, "tire", cliffs, TopUv(.40f, .31f, .10f, .10f), "Track/scar crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "prop_tire_tracks_02", "Source Tire Tracks 02", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Strip, 4, 1, "tire", fullKit, TopUv(.50f, .37f, .10f, .10f), "Alternate track/scar crop from Batch01 full-kit source sheet.");
            Add(specs, "prop_shell_mark_01", "Source Shell Mark 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Tiny, 2, 2, "scorch", cliffs, TopUv(.31f, .31f, .10f, .10f), "Shell mark crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "prop_crates_01", "Source Crates 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "metal", cliffs, TopUv(.73f, .62f, .08f, .08f), "Crate crop from Batch01 cliffs/resources/props sheet.");
            Add(specs, "prop_antenna_beacon_01", "Source Antenna Beacon 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Tiny, 2, 2, "beacon", roads, TopUv(.83f, .66f, .08f, .08f), "Small beacon/sign crop from Batch01 road/base source sheet.");
            Add(specs, "prop_destroyed_vehicle_proxy_01", "Source Destroyed Vehicle 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 3, 2, "dark_metal", cliffs, TopUv(.18f, .41f, .12f, .11f), "Destroyed vehicle crop from Batch01 cliffs/resources/props sheet.");

            return specs;
        }

        static void Add(List<TerrainArtReplacementSpec> specs, string pieceId, string displayName, TerrainPieceCategory category, TerrainPieceSizeClass sizeClass, int width, int height, string materialProfileId, string sourcePath, Vector4 uvRect, string notes)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return;

            specs.Add(new TerrainArtReplacementSpec
            {
                PieceId = pieceId,
                ArtId = "batch01_" + Slug(pieceId),
                DisplayName = displayName,
                Category = category,
                SizeClass = sizeClass,
                FootprintFineWidth = width,
                FootprintFineHeight = height,
                MaterialProfileId = materialProfileId,
                PassabilityHint = "Imported visual terrain art; Rts.Core passability remains authoritative.",
                BuildableHint = "Imported visual terrain art; Rts.Core buildability remains authoritative.",
                BlockingVisualOnly = category == TerrainPieceCategory.Obstacle,
                SourceAssetPath = sourcePath,
                UvRect = uvRect,
                SourceKind = TerrainArtSourceKind.Texture,
                Notes = notes
            });
        }

        static Vector4 TopUv(float x, float y, float width, float height)
        {
            return new Vector4(x, 1f - y - height, width, height);
        }

        static List<string> FilterTexturePaths(List<string> sourcePaths)
        {
            var textures = new List<string>();
            for (var i = 0; i < sourcePaths.Count; i++)
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePaths[i]) != null)
                    textures.Add(sourcePaths[i]);
            return textures;
        }

        static string FindSource(List<string> sourcePaths, string needle)
        {
            for (var i = 0; i < sourcePaths.Count; i++)
            {
                var file = Path.GetFileNameWithoutExtension(sourcePaths[i]);
                if (!string.IsNullOrEmpty(file) && file.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return sourcePaths[i];
            }

            return null;
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

        static TerrainArtManifest LoadOrCreateManifest()
        {
            var manifest = AssetDatabase.LoadAssetAtPath<TerrainArtManifest>(ManifestPath);
            if (manifest != null)
                return manifest;

            manifest = ScriptableObject.CreateInstance<TerrainArtManifest>();
            AssetDatabase.CreateAsset(manifest, ManifestPath);
            return manifest;
        }

        static List<string> FindSupportedSourceAssetPaths()
        {
            var paths = new List<string>();
            var absoluteFolder = ToAbsoluteProjectPath(SourceFolder);
            if (!Directory.Exists(absoluteFolder))
                return paths;

            var files = Directory.GetFiles(absoluteFolder, "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                if (!IsSupportedSource(files[i]))
                    continue;

                var assetPath = ToAssetPath(files[i]);
                if (!string.IsNullOrEmpty(assetPath))
                    paths.Add(assetPath);
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);
            return paths;
        }

        static bool IsSupportedSource(string path)
        {
            var ext = Path.GetExtension(path);
            return ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".fbx", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".obj", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".prefab", StringComparison.OrdinalIgnoreCase);
        }

        static void ConfigureTextureImports()
        {
            var sourcePaths = FindSupportedSourceAssetPaths();
            for (var i = 0; i < sourcePaths.Count; i++)
            {
                var importer = AssetImporter.GetAtPath(sourcePaths[i]) as TextureImporter;
                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        static void SeedBatch01FromStage31ReferenceSheets()
        {
            var absoluteSourceFolder = ToAbsoluteProjectPath(SourceFolder);
            Directory.CreateDirectory(absoluteSourceFolder);
            if (Directory.GetFiles(absoluteSourceFolder, "*.*", SearchOption.AllDirectories).Length > 0)
                return;

            var references = new[]
            {
                "Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_01_full_kit.jpg",
                "Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_02_board_layout.jpg",
                "Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_03_road_base_edges.jpg",
                "Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_04_cliffs_resources_props.jpg"
            };

            for (var i = 0; i < references.Length; i++)
            {
                var source = ToAbsoluteProjectPath(references[i]);
                if (!File.Exists(source))
                    continue;

                var destination = Path.Combine(absoluteSourceFolder, Path.GetFileName(source));
                if (!File.Exists(destination))
                    File.Copy(source, destination, false);
            }
        }

        static void EnsureFolders()
        {
            CreateFolderRecursive(SourceFolder);
            CreateFolderRecursive(GeneratedMaterialFolder);
            CreateFolderRecursive(GeneratedPrefabFolder);
            CreateFolderRecursive(GeneratedMeshFolder);
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

        static string Slug(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "unnamed";

            var chars = value.ToLowerInvariant().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (!char.IsLetterOrDigit(c))
                    chars[i] = '_';
            }

            return new string(chars).Trim('_');
        }

        sealed class TerrainArtReplacementSpec
        {
            public string PieceId;
            public string ArtId;
            public string DisplayName;
            public TerrainPieceCategory Category;
            public TerrainPieceSizeClass SizeClass;
            public int FootprintFineWidth;
            public int FootprintFineHeight;
            public string MaterialProfileId;
            public string PassabilityHint;
            public string BuildableHint;
            public bool BlockingVisualOnly;
            public string SourceAssetPath;
            public TerrainArtSourceKind SourceKind;
            public Vector4 UvRect;
            public Material GeneratedMaterial;
            public string Notes;
        }
    }

    public sealed class Stage32TerrainArtIngestionSummary
    {
        public int SourceAssetCount;
        public int PlayerFacingReplacementCount;
    }
}
