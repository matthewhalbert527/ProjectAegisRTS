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
        public const string GeneratedTextureFolder = "Assets/Rts/Art/Textures/Terrain/Batch01Imported";
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
            Stage32_6TerrainArtIntegrationCorrection.MoveBatch01SourceImagesToReferenceFolder();
            Stage32_6TerrainArtIntegrationCorrection.DeleteLegacyFlatTerrainCards();
            AssetDatabase.Refresh();

            var sourcePaths = FindReferenceOnlyImagePaths();
            var summary = new Stage32TerrainArtIngestionSummary { SourceAssetCount = sourcePaths.Count };
            var manifest = LoadOrCreateManifest();
            manifest.batchId = BatchId;
            manifest.sourceFolder = Stage32_6TerrainArtIntegrationCorrection.ReferenceFolder;
            manifest.entries = new List<TerrainArtManifestEntry>();

            for (var i = 0; i < sourcePaths.Count; i++)
            {
                var sourceAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePaths[i]);
                manifest.entries.Add(new TerrainArtManifestEntry
                {
                    artId = Path.GetFileNameWithoutExtension(sourcePaths[i]),
                    displayName = ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(sourcePaths[i])),
                    replacesPieceId = string.Empty,
                    category = TerrainPieceCategory.Ground,
                    sourceKind = TerrainArtSourceKind.Texture,
                    sourceAsset = sourceAsset,
                    sourceAssetPath = sourcePaths[i],
                    generatedMaterial = null,
                    generatedPrefab = null,
                    uvRect = new Vector4(0f, 0f, 1f, 1f),
                    coreBatch = true,
                    playerFacingReplacement = false,
                    notes = "Reference-only terrain art direction. Stage32.6 forbids direct runtime use as flat cards or cropped sheet textures."
                });
            }

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
            var croppedTexture = CreateOrUpdateCroppedTexture(spec, texture);
            var material = CreateTextureMaterial(spec, croppedTexture != null ? croppedTexture : texture, croppedTexture == null ? spec.UvRect : new Vector4(0f, 0f, 1f, 1f));
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

        static Texture2D CreateOrUpdateCroppedTexture(TerrainArtReplacementSpec spec, Texture2D sourceTexture)
        {
            if (sourceTexture == null)
                return null;

            var sourceWidth = sourceTexture.width;
            var sourceHeight = sourceTexture.height;
            var x = Mathf.Clamp(Mathf.RoundToInt(spec.UvRect.x * sourceWidth), 0, sourceWidth - 1);
            var y = Mathf.Clamp(Mathf.RoundToInt(spec.UvRect.y * sourceHeight), 0, sourceHeight - 1);
            var width = Mathf.Clamp(Mathf.RoundToInt(spec.UvRect.z * sourceWidth), 1, sourceWidth - x);
            var height = Mathf.Clamp(Mathf.RoundToInt(spec.UvRect.w * sourceHeight), 1, sourceHeight - y);

            var pixels = sourceTexture.GetPixels(x, y, width, height);
            var applyCutout = ShouldApplySourceBackgroundCutout(spec);
            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                pixel.a = applyCutout && IsLikelySourceSheetBackground(pixel) ? 0f : 1f;
                pixels[i] = pixel;
            }

            var cropped = new Texture2D(width, height, TextureFormat.RGBA32, true) { name = spec.ArtId + "_crop" };
            cropped.SetPixels(pixels);
            cropped.Apply(true, false);

            var texturePath = GeneratedTextureFolder + "/" + spec.ArtId + ".png";
            var absolutePath = ToAbsoluteProjectPath(texturePath);
            File.WriteAllBytes(absolutePath, cropped.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(cropped);

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        }

        static bool ShouldApplySourceBackgroundCutout(TerrainArtReplacementSpec spec)
        {
            return spec.Category == TerrainPieceCategory.Prop ||
                spec.Category == TerrainPieceCategory.Obstacle ||
                spec.Category == TerrainPieceCategory.Resource;
        }

        static bool IsLikelySourceSheetBackground(Color pixel)
        {
            var max = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
            var min = Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
            return max < 0.10f || (max < 0.16f && max - min < 0.045f);
        }

        static Material CreateTextureMaterial(TerrainArtReplacementSpec spec, Texture2D texture, Vector4 textureUvRect)
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
            material.mainTextureScale = new Vector2(textureUvRect.z, textureUvRect.w);
            material.mainTextureOffset = new Vector2(textureUvRect.x, textureUvRect.y);
            material.color = Color.white;
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", new Vector2(textureUvRect.z, textureUvRect.w));
                material.SetTextureOffset("_BaseMap", new Vector2(textureUvRect.x, textureUvRect.y));
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
                material.SetTextureScale("_MainTex", new Vector2(textureUvRect.z, textureUvRect.w));
                material.SetTextureOffset("_MainTex", new Vector2(textureUvRect.x, textureUvRect.y));
            }
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.18f);
            ConfigureMaterialTransparency(material, ShouldApplySourceBackgroundCutout(spec));

            spec.GeneratedMaterial = material;
            EditorUtility.SetDirty(material);
            return material;
        }

        static void ConfigureMaterialTransparency(Material material, bool transparent)
        {
            if (transparent)
            {
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                if (material.HasProperty("_Surface"))
                    material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_AlphaClip"))
                    material.SetFloat("_AlphaClip", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = 3000;
                return;
            }

            material.SetOverrideTag("RenderType", "Opaque");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 0f);
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = 2450;
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

            var footprintScale = GetVisualFootprintScale(spec);
            var width = Mathf.Max(0.6f, spec.FootprintFineWidth * footprintScale);
            var depth = Mathf.Max(0.6f, spec.FootprintFineHeight * footprintScale);
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

        static float GetVisualFootprintScale(TerrainArtReplacementSpec spec)
        {
            if (spec.Category == TerrainPieceCategory.Prop ||
                spec.Category == TerrainPieceCategory.Obstacle ||
                spec.Category == TerrainPieceCategory.Resource)
                return 1.05f;

            if (spec.Category == TerrainPieceCategory.Transition)
                return 0.92f;

            return 0.86f;
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

            var sheetA = FindSource(textures, "sheet_a_ground_foundations") ?? FindSource(textures, "full_kit") ?? textures[0];
            var sheetB = FindSource(textures, "sheet_b_roads_edges") ?? FindSource(textures, "road_base_edges") ?? sheetA;
            var sheetC = FindSource(textures, "sheet_c_resources_obstacles") ?? FindSource(textures, "cliffs_resources_props") ?? sheetA;
            var sheetD = FindSource(textures, "sheet_d_props_vegetation") ?? sheetC;

            Add(specs, "base_foundation_pad_01", "Concrete Pad Small", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "foundation", sheetA, PixelUv(335, 405, 235, 180), "Concrete Pad Small crop from Batch01 Sheet A.");
            Add(specs, "base_foundation_pad_02", "Concrete Pad Medium", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "foundation", sheetA, PixelUv(585, 400, 310, 180), "Concrete Pad Medium crop from Batch01 Sheet A.");
            Add(specs, "base_foundation_pad_03", "Foundation Pad Large", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Medium, 4, 4, "foundation", sheetA, PixelUv(890, 410, 350, 370), "Foundation Pad Large crop from Batch01 Sheet A.");
            Add(specs, "base_production_apron_01", "Concrete Production Apron", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 2, "foundation", sheetA, PixelUv(585, 400, 310, 180), "Concrete pad apron crop from Batch01 Sheet A.");
            Add(specs, "base_production_apron_02", "Cracked Concrete Apron", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 2, "foundation", sheetA, PixelUv(35, 420, 240, 160), "Cracked concrete crop from Batch01 Sheet A.");
            Add(specs, "base_road_strip_01", "Road Straight 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 1, "road", sheetB, PixelUv(45, 100, 240, 255), "Road Straight 01 crop from Batch01 Sheet B.");
            Add(specs, "base_road_strip_02", "Damaged Road 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Strip, 5, 1, "road", sheetB, PixelUv(35, 455, 325, 185), "Damaged Road 01 crop from Batch01 Sheet B.");
            Add(specs, "base_rally_exit_marking_01", "Road Crossing 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Small, 3, 1, "caution", sheetB, PixelUv(960, 105, 245, 240), "Road Crossing 01 crop from Batch01 Sheet B.");
            Add(specs, "base_rally_exit_marking_02", "Road T Junction 01", TerrainPieceCategory.BaseConstruction, TerrainPieceSizeClass.Small, 3, 1, "caution", sheetB, PixelUv(600, 100, 315, 235), "Road T Junction 01 crop from Batch01 Sheet B.");

            Add(specs, "ground_compact_soil_patch_01", "Dirt Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 2, "compact_soil", sheetA, PixelUv(565, 58, 225, 235), "Dirt Ground 01 crop from Batch01 Sheet A.");
            Add(specs, "ground_compact_soil_patch_02", "Road To Dirt Edge 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 2, "compact_soil", sheetB, PixelUv(460, 455, 320, 175), "Road To Dirt Edge 01 crop from Batch01 Sheet B.");
            Add(specs, "ground_scorched_patch_01", "Scorched Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 2, 2, "scorch", sheetA, PixelUv(1070, 55, 190, 235), "Scorched Ground 01 crop from Batch01 Sheet A.");
            Add(specs, "ground_mud_patch_01", "Mud Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 2, 2, "mud", sheetA, PixelUv(820, 55, 225, 235), "Mud Ground 01 crop from Batch01 Sheet A.");
            Add(specs, "ground_road_path_01", "Road To Dirt Path 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Strip, 5, 2, "road", sheetB, PixelUv(460, 455, 320, 175), "Road To Dirt Edge 01 crop from Batch01 Sheet B.");
            Add(specs, "ground_grass_dirt_patch_01", "Grass Ground 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 3, "grass_dirt", sheetA, PixelUv(20, 55, 260, 245), "Grass Ground 01 crop from Batch01 Sheet A.");
            Add(specs, "ground_grass_dirt_patch_02", "Grass Ground 02", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Patch, 3, 3, "grass_dirt", sheetA, PixelUv(295, 60, 240, 235), "Grass Ground 02 crop from Batch01 Sheet A.");
            Add(specs, "ground_rocky_blocked_01", "Rock Blocker 01", TerrainPieceCategory.Ground, TerrainPieceSizeClass.Medium, 3, 3, "rock", sheetC, PixelUv(40, 330, 290, 190), "Rock Blocker 01 crop from Batch01 Sheet C.");

            Add(specs, "transition_concrete_ground_edge_01", "Cracked Concrete Edge", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 1, "concrete", sheetA, PixelUv(35, 420, 240, 160), "Cracked Concrete 01 crop from Batch01 Sheet A.");
            Add(specs, "transition_buildable_edge_01", "Base Curb Straight", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 1, "foundation", sheetB, PixelUv(925, 755, 300, 95), "Base Curb Straight crop from Batch01 Sheet B.");
            Add(specs, "transition_dirt_road_blend_01", "Road To Dirt Edge 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 4, 1, "road", sheetB, PixelUv(460, 455, 320, 175), "Road To Dirt Edge 01 crop from Batch01 Sheet B.");
            Add(specs, "transition_resource_edge_01", "Resource Depleted 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 3, 1, "resource_ground", sheetC, PixelUv(860, 70, 240, 165), "Resource Depleted 01 crop from Batch01 Sheet C.");
            Add(specs, "transition_rock_edge_01", "Ridge Piece Long 01", TerrainPieceCategory.Transition, TerrainPieceSizeClass.Edge, 3, 1, "rock", sheetC, PixelUv(720, 335, 455, 190), "Ridge Piece Long 01 crop from Batch01 Sheet C.");

            Add(specs, "resource_cluster_01", "Resource Cluster Blue 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "mineral", sheetC, PixelUv(70, 65, 230, 170), "Resource Cluster Blue 01 crop from Batch01 Sheet C.");
            Add(specs, "resource_cluster_02", "Resource Cluster Green 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "mineral", sheetC, PixelUv(430, 70, 230, 165), "Resource Cluster Green 01 crop from Batch01 Sheet C.");
            Add(specs, "resource_rich_cluster_01", "Resource Cluster Blue Rich", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Small, 2, 2, "rich_mineral", sheetC, PixelUv(70, 65, 230, 170), "Resource Cluster Blue 01 crop from Batch01 Sheet C.");
            Add(specs, "resource_decal_01", "Resource Depleted 01", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Patch, 3, 3, "resource_ground", sheetC, PixelUv(860, 70, 240, 165), "Resource Depleted 01 crop from Batch01 Sheet C.");
            Add(specs, "resource_harvest_marker_01", "Resource Cluster Green Marker", TerrainPieceCategory.Resource, TerrainPieceSizeClass.Tiny, 2, 1, "caution", sheetC, PixelUv(430, 70, 230, 165), "Resource Cluster Green 01 crop from Batch01 Sheet C.");

            Add(specs, "obstacle_rock_cluster_01", "Rock Blocker 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "rock", sheetC, PixelUv(40, 330, 290, 190), "Rock Blocker 01 crop from Batch01 Sheet C.");
            Add(specs, "obstacle_rock_cluster_02", "Rock Blocker 02", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "rock", sheetC, PixelUv(420, 325, 260, 195), "Rock Blocker 02 crop from Batch01 Sheet C.");
            Add(specs, "obstacle_ridge_piece_01", "Ridge Piece Long 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Edge, 4, 1, "rock", sheetC, PixelUv(720, 335, 455, 190), "Ridge Piece Long 01 crop from Batch01 Sheet C.");
            Add(specs, "obstacle_cliff_blocker_chunk_01", "Broken Cliff Corner 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Medium, 3, 2, "rock", sheetC, PixelUv(65, 650, 260, 185), "Broken Cliff Corner 01 crop from Batch01 Sheet C.");
            Add(specs, "obstacle_tree_bush_cluster_01", "Tree Cluster 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "foliage", sheetD, PixelUv(80, 650, 250, 190), "Tree Cluster 01 crop from Batch01 Sheet D.");
            Add(specs, "obstacle_wreckage_01", "Wreckage Small 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 3, 2, "dark_metal", sheetD, PixelUv(70, 85, 230, 150), "Wreckage Small 01 crop from Batch01 Sheet D.");
            Add(specs, "obstacle_debris_01", "Debris Burn Patch 01", TerrainPieceCategory.Obstacle, TerrainPieceSizeClass.Small, 2, 2, "metal", sheetC, PixelUv(1035, 645, 225, 190), "Debris Burn Patch 01 crop from Batch01 Sheet C.");

            Add(specs, "prop_sandbag_01", "Sandbags Straight 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 1, "sandbag", sheetB, PixelUv(25, 750, 315, 85), "Sandbags Straight 01 crop from Batch01 Sheet B.");
            Add(specs, "prop_sandbag_02", "Sandbags Straight 01 Alternate", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 1, "sandbag", sheetB, PixelUv(25, 750, 315, 85), "Sandbags Straight 01 crop from Batch01 Sheet B.");
            Add(specs, "prop_barrier_01", "Barrier Concrete 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 3, 1, "metal", sheetB, PixelUv(455, 745, 330, 100), "Barrier Concrete 01 crop from Batch01 Sheet B.");
            Add(specs, "prop_tank_trap_01", "Anti Tank Obstacle 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "dark_metal", sheetD, PixelUv(735, 90, 170, 145), "Anti Tank Obstacle 01 crop from Batch01 Sheet D.");
            Add(specs, "prop_tire_tracks_01", "Tire Tracks 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Strip, 4, 1, "tire", sheetD, PixelUv(50, 390, 250, 140), "Tire Tracks 01 crop from Batch01 Sheet D.");
            Add(specs, "prop_tire_tracks_02", "Debris Small 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Strip, 4, 1, "tire", sheetD, PixelUv(420, 390, 260, 140), "Debris Small 01 crop from Batch01 Sheet D.");
            Add(specs, "prop_shell_mark_01", "Crater 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Tiny, 2, 2, "scorch", sheetC, PixelUv(430, 650, 230, 180), "Crater 01 crop from Batch01 Sheet C.");
            Add(specs, "prop_crates_01", "Crate Stack 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 2, 2, "metal", sheetD, PixelUv(705, 670, 245, 175), "Crate Stack 01 crop from Batch01 Sheet D.");
            Add(specs, "prop_antenna_beacon_01", "Barrel Group 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Tiny, 2, 2, "beacon", sheetD, PixelUv(990, 665, 235, 180), "Barrel Group 01 crop from Batch01 Sheet D.");
            Add(specs, "prop_destroyed_vehicle_proxy_01", "Wreckage Pile 01", TerrainPieceCategory.Prop, TerrainPieceSizeClass.Small, 3, 2, "dark_metal", sheetD, PixelUv(390, 75, 265, 170), "Wreckage Pile 01 crop from Batch01 Sheet D.");

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

        static Vector4 PixelUv(float x, float y, float width, float height)
        {
            const float sourceWidth = 1280f;
            const float sourceHeight = 960f;
            return TopUv(x / sourceWidth, y / sourceHeight, width / sourceWidth, height / sourceHeight);
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

        static List<string> FindReferenceOnlyImagePaths()
        {
            var paths = new List<string>();
            var absoluteFolder = ToAbsoluteProjectPath(Stage32_6TerrainArtIntegrationCorrection.ReferenceFolder);
            if (!Directory.Exists(absoluteFolder))
                return paths;

            var files = Directory.GetFiles(absoluteFolder, "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var ext = Path.GetExtension(files[i]);
                if (!ext.Equals(".png", StringComparison.OrdinalIgnoreCase) &&
                    !ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
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
                importer.isReadable = true;
                importer.alphaIsTransparency = true;
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
            CreateFolderRecursive(Stage32_6TerrainArtIntegrationCorrection.ReferenceFolder);
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
            public Texture2D GeneratedTexture;
            public string Notes;
        }
    }

    public sealed class Stage32TerrainArtIngestionSummary
    {
        public int SourceAssetCount;
        public int PlayerFacingReplacementCount;
    }
}
