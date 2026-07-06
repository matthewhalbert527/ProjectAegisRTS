using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32TerrainSampleGroundTileIntegrator
    {
        public const string SampleRoot = "Assets/TerrainSampleAssets";
        public const string TerrainLayerFolder = SampleRoot + "/TerrainLayers";
        public const string SamplePrefabFolder = SampleRoot + "/Prefabs";
        public const string MaterialFolder = "Assets/Rts/Art/Materials/Terrain/TerrainSamplePack";
        public const string MeshFolder = "Assets/Rts/Art/Meshes/Terrain/TerrainSampleGroundTiles";
        public const string PrefabFolder = "Assets/Rts/Art/Prefabs/Terrain/TerrainSampleGroundTiles";
        public const string DefinitionFolder = "Assets/Rts/ScriptableObjects/Art/TerrainPieces/Definitions/Ground";
        public const string ReportPath = "docs/TERRAIN_SAMPLE_GROUND_TILES.md";

        static readonly GroundSpec[] Specs =
        {
            new GroundSpec("ground_grass_dirt_patch_01", "Grass Soil TerrainLayer", "Grass_Soil_TerrainLayer", "Grass_A", "Plant_A"),
            new GroundSpec("ground_grass_dirt_patch_02", "Grass Moss TerrainLayer", "Grass_Moss_TerrainLayer", "Grass_B", "Fern_A"),
            new GroundSpec("ground_grass_dirt_patch_03", "Grass A TerrainLayer", "Grass_A_TerrainLayer", "Grass_C", "Plant_B"),
            new GroundSpec("ground_grass_dirt_patch_04", "Grass B TerrainLayer", "Grass_B_TerrainLayer", "Grass_D", "Fern_B"),
            new GroundSpec("ground_compact_soil_patch_01", "Soil Rocks TerrainLayer", "Soil_Rocks_TerrainLayer", "GrassDry_A", "Plant_D"),
            new GroundSpec("ground_compact_soil_patch_02", "Pebbles A TerrainLayer", "Pebbles_A_TerrainLayer", "GrassDry_B", "Plant_C"),
            new GroundSpec("ground_compact_soil_patch_03", "Pebbles B TerrainLayer", "Pebbles_B_TerrainLayer", "GrassDry_C", "Plant_D"),
            new GroundSpec("ground_mud_patch_01", "Muddy TerrainLayer", "Muddy_TerrainLayer", "GrassDry_A", "BushDry_A"),
            new GroundSpec("ground_mud_patch_02", "Pebbles C TerrainLayer", "Pebbles_C_TerrainLayer", "GrassDry_B", "BushDry_B"),
            new GroundSpec("ground_resource_field_01", "Soil Rocks TerrainLayer", "Soil_Rocks_TerrainLayer", "Heather_A", "Plant_A"),
            new GroundSpec("ground_rocky_blocked_01", "Rock TerrainLayer", "Rock_TerrainLayer", "Heather_B", "BushDry_A"),
            new GroundSpec("ground_scorched_patch_01", "Black Sand TerrainLayer", "Black_Sand_TerrainLayer", "GrassDry_C", "Plant_D"),
            new GroundSpec("ground_scorched_patch_02", "Black Sand TerrainLayer", "Black_Sand_TerrainLayer", "BushDry_A", "Plant_D")
        };

        [MenuItem("ProjectAegisRTS/Stage 32/Integrate Terrain Sample Ground Tiles")]
        public static void IntegrateMenu()
        {
            IntegrateGroundTiles();
        }

        public static void IntegrateGroundTilesBatch()
        {
            try
            {
                var summary = IntegrateGroundTiles();
                Debug.Log("Terrain Sample ground tile integration completed. Replaced definitions: " + summary.ReplacedDefinitions + ".");
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

        public static void ValidateGroundTilesBatch()
        {
            try
            {
                var summary = ValidateGroundTiles();
                Debug.Log("Terrain Sample ground tile validation passed. Replaced definitions: " + summary.ReplacedDefinitions + ".");
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

        public static TerrainSampleGroundTileSummary IntegrateGroundTiles()
        {
            EnsureImportedPackage();
            EnsureFolder(MaterialFolder);
            EnsureFolder(MeshFolder);
            EnsureFolder(PrefabFolder);

            var replaced = 0;
            var materialCount = 0;
            var prefabCount = 0;
            var dependencies = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            AssetDatabase.Refresh();
            for (var i = 0; i < Specs.Length; i++)
            {
                var spec = Specs[i];
                var definition = LoadDefinition(spec.PieceId);
                if (definition == null)
                    continue;

                var layer = LoadTerrainLayer(spec.TerrainLayerName);
                var material = CreateMaterial(spec, layer);
                var mesh = CreateMesh(spec, layer);
                var prefab = CreatePrefab(spec, definition, material, mesh);

                definition.prefab = prefab;
                definition.supportsTint = false;
                definition.notes = "Uses Unity Terrain Sample Asset Pack terrain layer textures and foliage/detail prefabs for the player-facing ground tile replacement. Visual only; gameplay authority remains in Rts.Core.";
                definition.questBudgetTag = "QuestSafeTerrainSample";
                EditorUtility.SetDirty(definition);

                var prefabDependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab), true);
                for (var d = 0; d < prefabDependencies.Length; d++)
                    if (prefabDependencies[d].StartsWith(SampleRoot + "/", StringComparison.OrdinalIgnoreCase))
                        dependencies.Add(prefabDependencies[d]);

                replaced++;
                materialCount++;
                prefabCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var summary = new TerrainSampleGroundTileSummary
            {
                ReplacedDefinitions = replaced,
                MaterialCount = materialCount,
                PrefabCount = prefabCount,
                ImportedDependencyCount = dependencies.Count
            };

            WriteReport(summary, dependencies);
            return summary;
        }

        public static TerrainSampleGroundTileSummary ValidateGroundTiles()
        {
            EnsureImportedPackage();
            var errors = new List<string>();
            var replaced = 0;
            var materialCount = 0;
            var prefabCount = 0;
            var sampleDependencies = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < Specs.Length; i++)
            {
                var spec = Specs[i];
                var definition = LoadDefinition(spec.PieceId);
                if (definition == null)
                {
                    errors.Add("Missing terrain definition: " + spec.PieceId);
                    continue;
                }

                if (definition.prefab == null)
                {
                    errors.Add(spec.PieceId + " has no prefab.");
                    continue;
                }

                var prefabPath = AssetDatabase.GetAssetPath(definition.prefab);
                if (!prefabPath.StartsWith(PrefabFolder + "/", StringComparison.OrdinalIgnoreCase))
                    errors.Add(spec.PieceId + " still points at non-TerrainSample prefab: " + prefabPath);

                var renderers = definition.prefab.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length < 2)
                    errors.Add(spec.PieceId + " should include textured ground plus imported sample detail geometry.");

                var tag = definition.prefab.GetComponent<Stage32_6RuntimeTerrainTag>();
                if (tag == null || !tag.IsComplete())
                    errors.Add(spec.PieceId + " is missing a complete Stage32_6RuntimeTerrainTag.");

                var validationTag = definition.prefab.GetComponent<TerrainPieceValidationTag>();
                if (validationTag == null || !validationTag.IsComplete())
                    errors.Add(spec.PieceId + " is missing a complete TerrainPieceValidationTag.");

                var deps = AssetDatabase.GetDependencies(prefabPath, true);
                var hasSampleTexture = false;
                var hasSamplePrefab = false;
                for (var d = 0; d < deps.Length; d++)
                {
                    if (!deps[d].StartsWith(SampleRoot + "/", StringComparison.OrdinalIgnoreCase))
                        continue;
                    sampleDependencies.Add(deps[d]);
                    if (deps[d].IndexOf("/Textures/Terrain/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        deps[d].IndexOf("/TerrainLayers/", StringComparison.OrdinalIgnoreCase) >= 0)
                        hasSampleTexture = true;
                    if (deps[d].IndexOf("/Prefabs/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        deps[d].IndexOf("/Models/", StringComparison.OrdinalIgnoreCase) >= 0)
                        hasSamplePrefab = true;
                }

                if (!hasSampleTexture)
                    errors.Add(spec.PieceId + " does not reference Terrain Sample terrain textures/layers.");
                if (!hasSamplePrefab)
                    errors.Add(spec.PieceId + " does not reference Terrain Sample foliage/detail prefab or model geometry.");

                replaced++;
                prefabCount++;
                materialCount++;
            }

            if (replaced < Specs.Length)
                errors.Add("Expected " + Specs.Length + " Terrain Sample ground tile replacements, found " + replaced + ".");

            if (errors.Count > 0)
                throw new InvalidOperationException("Terrain Sample ground tile validation failed:\n" + string.Join("\n", errors.ToArray()));

            return new TerrainSampleGroundTileSummary
            {
                ReplacedDefinitions = replaced,
                MaterialCount = materialCount,
                PrefabCount = prefabCount,
                ImportedDependencyCount = sampleDependencies.Count
            };
        }

        static void EnsureImportedPackage()
        {
            if (!AssetDatabase.IsValidFolder(SampleRoot))
                throw new InvalidOperationException("Unity Terrain Sample Asset Pack is not imported. Expected folder: " + SampleRoot);
            if (!AssetDatabase.IsValidFolder(TerrainLayerFolder))
                throw new InvalidOperationException("Terrain Sample terrain layers are missing: " + TerrainLayerFolder);
            if (!AssetDatabase.IsValidFolder(SamplePrefabFolder))
                throw new InvalidOperationException("Terrain Sample foliage/detail prefabs are missing: " + SamplePrefabFolder);
        }

        static TerrainPieceDefinition LoadDefinition(string pieceId)
        {
            var guids = AssetDatabase.FindAssets(pieceId + " t:TerrainPieceDefinition", new[] { DefinitionFolder });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var definition = AssetDatabase.LoadAssetAtPath<TerrainPieceDefinition>(path);
                if (definition != null && string.Equals(definition.pieceId, pieceId, StringComparison.OrdinalIgnoreCase))
                    return definition;
            }

            return null;
        }

        static TerrainLayer LoadTerrainLayer(string terrainLayerName)
        {
            var path = TerrainLayerFolder + "/" + terrainLayerName + ".terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer == null)
                throw new InvalidOperationException("Missing Terrain Sample layer: " + path);
            return layer;
        }

        static Material CreateMaterial(GroundSpec spec, TerrainLayer layer)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");

            var material = LoadOrCreateMaterial(MaterialFolder + "/" + spec.PieceId + "_terrain_sample.mat", shader);
            material.name = spec.PieceId + "_terrain_sample";
            material.shader = shader;

            SetTexture(material, "_BaseMap", layer.diffuseTexture);
            SetTexture(material, "_MainTex", layer.diffuseTexture);
            SetTexture(material, "_BumpMap", layer.normalMapTexture);
            SetTexture(material, "_NormalMap", layer.normalMapTexture);
            SetTexture(material, "_MaskMap", layer.maskMapTexture);
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
            SetFloat(material, "_Smoothness", Mathf.Clamp01(layer.smoothness * 0.45f));
            SetFloat(material, "_Metallic", 0f);
            if (layer.normalMapTexture != null)
                material.EnableKeyword("_NORMALMAP");

            EditorUtility.SetDirty(material);
            return material;
        }

        static Material LoadOrCreateMaterial(string path, Shader shader)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static void SetTexture(Material material, string propertyName, Texture texture)
        {
            if (texture != null && material.HasProperty(propertyName))
                material.SetTexture(propertyName, texture);
        }

        static void SetColor(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
                material.SetColor(propertyName, color);
        }

        static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }

        static Mesh CreateMesh(GroundSpec spec, TerrainLayer layer)
        {
            var path = MeshFolder + "/" + spec.PieceId + "_terrain_sample_mesh.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                return existing;

            var mesh = BuildBeveledTileMesh(StableSeed(spec.PieceId));
            mesh.name = spec.PieceId + "_terrain_sample_mesh";
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        static int StableSeed(string value)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];
                return hash;
            }
        }

        static Mesh BuildBeveledTileMesh(int seed)
        {
            const int cells = 8;
            const float half = 1.45f;
            const float edgeDepth = -0.08f;
            const float topHeight = 0.02f;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (var z = 0; z <= cells; z++)
            {
                for (var x = 0; x <= cells; x++)
                {
                    var px = Mathf.Lerp(-half, half, x / (float)cells);
                    var pz = Mathf.Lerp(-half, half, z / (float)cells);
                    var edge = x == 0 || z == 0 || x == cells || z == cells;
                    var ripple = edge ? 0f : ((Mathf.Sin((x + seed * 0.013f) * 2.31f) + Mathf.Cos((z - seed * 0.007f) * 1.87f)) * 0.006f);
                    vertices.Add(new Vector3(px, topHeight + ripple, pz));
                    uvs.Add(new Vector2(x / (float)cells, z / (float)cells));
                }
            }

            for (var z = 0; z < cells; z++)
            {
                for (var x = 0; x < cells; x++)
                {
                    var a = z * (cells + 1) + x;
                    var b = a + 1;
                    var c = a + cells + 1;
                    var d = c + 1;
                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            AddSide(vertices, uvs, triangles, new Vector3(-half, topHeight, -half), new Vector3(half, topHeight, -half), new Vector3(half, edgeDepth, -half), new Vector3(-half, edgeDepth, -half));
            AddSide(vertices, uvs, triangles, new Vector3(half, topHeight, -half), new Vector3(half, topHeight, half), new Vector3(half, edgeDepth, half), new Vector3(half, edgeDepth, -half));
            AddSide(vertices, uvs, triangles, new Vector3(half, topHeight, half), new Vector3(-half, topHeight, half), new Vector3(-half, edgeDepth, half), new Vector3(half, edgeDepth, half));
            AddSide(vertices, uvs, triangles, new Vector3(-half, topHeight, half), new Vector3(-half, topHeight, -half), new Vector3(-half, edgeDepth, -half), new Vector3(-half, edgeDepth, half));

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        static void AddSide(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 1f));
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        static GameObject CreatePrefab(GroundSpec spec, TerrainPieceDefinition definition, Material material, Mesh mesh)
        {
            var root = new GameObject(spec.PieceId);
            var ground = new GameObject("Terrain Sample textured tile");
            ground.transform.SetParent(root.transform, false);
            var filter = ground.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = ground.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;

            var detailRenderers = AddDetailPrefabs(root.transform, spec);
            var allRenderers = root.GetComponentsInChildren<Renderer>(true);

            var tag = root.AddComponent<TerrainPieceValidationTag>();
            tag.pieceId = definition.pieceId;
            tag.displayName = definition.displayName;
            tag.category = definition.category;
            tag.sizeClass = definition.sizeClass;
            tag.footprintFineWidth = definition.footprintFineWidth;
            tag.footprintFineHeight = definition.footprintFineHeight;
            tag.materialProfileId = definition.materialProfileId;
            tag.passabilityVisualHint = definition.passabilityVisualHint;
            tag.buildableVisualHint = definition.buildableVisualHint;
            tag.supportsRotation = definition.supportsRotation;
            tag.supportsTint = false;
            tag.isGameplayBlockingVisualOnly = definition.isGameplayBlockingVisualOnly;
            tag.questBudgetTag = "QuestSafeTerrainSample";
            tag.rendererCount = allRenderers.Length;
            tag.primitiveCount = 1;
            tag.notes = "Ground tile replaced with Unity Terrain Sample Asset Pack terrain layer textures and imported foliage/detail geometry.";

            var runtimeTag = root.AddComponent<Stage32_6RuntimeTerrainTag>();
            runtimeTag.runtimeAssetId = "terrain_sample_" + spec.PieceId;
            runtimeTag.mappedTerrainPieceId = spec.PieceId;
            runtimeTag.category = definition.category;
            runtimeTag.referenceOnlyPolicyEnforced = true;
            runtimeTag.usesReferenceTexture = false;
            runtimeTag.flatImageCard = false;
            runtimeTag.hasBeveledMesh = true;
            runtimeTag.hasChildGeometry = detailRenderers > 0;
            runtimeTag.pivotAtOrigin = true;
            runtimeTag.rendererCount = allRenderers.Length;
            runtimeTag.materialCount = CountUniqueMaterials(allRenderers);
            runtimeTag.sourceArtPolicy = "Runtime terrain uses imported Unity Terrain Sample Asset Pack textures/prefabs, not concept sheet cards.";
            runtimeTag.notes = "Player-facing ground tile replacement generated from Unity's Terrain Sample Asset Pack.";

            var lod = root.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(0.02f, allRenderers) });
            lod.RecalculateBounds();

            RemoveColliders(root);

            var path = PrefabFolder + "/" + spec.PieceId + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static int AddDetailPrefabs(Transform parent, GroundSpec spec)
        {
            var count = 0;
            count += AddDetailPrefab(parent, spec.DetailPrefabA, new Vector3(-0.48f, 0.03f, 0.32f), Quaternion.Euler(0f, 28f, 0f), 0.22f);
            count += AddDetailPrefab(parent, spec.DetailPrefabB, new Vector3(0.52f, 0.03f, -0.38f), Quaternion.Euler(0f, 132f, 0f), 0.18f);
            count += AddDetailPrefab(parent, spec.DetailPrefabA, new Vector3(0.14f, 0.03f, 0.58f), Quaternion.Euler(0f, 247f, 0f), 0.14f);
            return count;
        }

        static int AddDetailPrefab(Transform parent, string prefabName, Vector3 position, Quaternion rotation, float scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SamplePrefabFolder + "/" + prefabName + ".prefab");
            if (prefab == null)
                return 0;

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return 0;

            instance.name = "Terrain Sample detail " + prefabName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = Vector3.one * scale;
            RemoveColliders(instance);
            return instance.GetComponentsInChildren<Renderer>(true).Length;
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

        static void RemoveColliders(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
                Object.DestroyImmediate(colliders[i]);
        }

        static void EnsureFolder(string assetFolder)
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

        static void WriteReport(TerrainSampleGroundTileSummary summary, SortedSet<string> dependencies)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Terrain Sample Ground Tiles");
            builder.AppendLine();
            builder.AppendLine("Imported Unity Asset Store package: Terrain Sample Asset Pack.");
            builder.AppendLine();
            builder.AppendLine("- Replaced ground definitions: " + summary.ReplacedDefinitions);
            builder.AppendLine("- Generated materials: " + summary.MaterialCount);
            builder.AppendLine("- Generated prefabs: " + summary.PrefabCount);
            builder.AppendLine("- Imported Terrain Sample dependencies referenced by generated prefabs: " + summary.ImportedDependencyCount);
            builder.AppendLine();
            builder.AppendLine("Generated prefabs live under `Assets/Rts/Art/Prefabs/Terrain/TerrainSampleGroundTiles/` and replace the player-facing ground-piece definitions. Rts.Core gameplay remains unchanged.");
            builder.AppendLine();
            builder.AppendLine("Representative imported dependencies:");
            var shown = 0;
            foreach (var dependency in dependencies)
            {
                builder.AppendLine("- `" + dependency + "`");
                shown++;
                if (shown >= 24)
                    break;
            }

            var absoluteReportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ReportPath));
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteReportPath));
            File.WriteAllText(absoluteReportPath, builder.ToString());
        }

        sealed class GroundSpec
        {
            public readonly string PieceId;
            public readonly string DisplayLayerName;
            public readonly string TerrainLayerName;
            public readonly string DetailPrefabA;
            public readonly string DetailPrefabB;

            public GroundSpec(string pieceId, string displayLayerName, string terrainLayerName, string detailPrefabA, string detailPrefabB)
            {
                PieceId = pieceId;
                DisplayLayerName = displayLayerName;
                TerrainLayerName = terrainLayerName;
                DetailPrefabA = detailPrefabA;
                DetailPrefabB = detailPrefabB;
            }
        }
    }

    public sealed class TerrainSampleGroundTileSummary
    {
        public int ReplacedDefinitions;
        public int MaterialCount;
        public int PrefabCount;
        public int ImportedDependencyCount;
    }
}
