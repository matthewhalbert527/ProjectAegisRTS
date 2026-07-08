#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.MapEditor;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    static class AegisMapArtPack
    {
        public const string Root = "Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1";
        public const string GeneratedProxyRoot = "Assets/Rts/MapEditor/ArtPack/GeneratedProxies";
        public const string ResolutionImportedModel = "ImportedModel";
        public const string ResolutionArtPackDerivedProxy = "ArtPackDerivedProxy";

        public static readonly string[] CliffMeshes =
        {
            "Meshes/Cliffs/cliff_straight_01.glb",
            "Meshes/Cliffs/cliff_straight_02.glb",
            "Meshes/Cliffs/cliff_wall_tall_01.glb",
            "Meshes/Cliffs/cliff_wall_low_01.glb",
            "Meshes/Cliffs/cliff_endcap_01.glb",
            "Meshes/Cliffs/cliff_spire_cluster_01.glb",
            "Meshes/Cliffs/cliff_spire_cluster_02.glb",
            "Meshes/Cliffs/cliff_corner_inner_01.glb",
            "Meshes/Cliffs/cliff_corner_outer_01.glb"
        };

        public static readonly string[] BoulderMeshes =
        {
            "Meshes/Rocks/boulder_large_01.glb",
            "Meshes/Rocks/boulder_large_02.glb",
            "Meshes/Rocks/boulder_medium_01.glb",
            "Meshes/Rocks/boulder_medium_02.glb"
        };

        public static readonly string[] PebbleMeshes =
        {
            "Meshes/Rocks/pebble_cluster_01.glb",
            "Meshes/Rocks/pebble_cluster_02.glb",
            "Meshes/Rocks/scatter_stones_01.glb",
            "Meshes/Rocks/scatter_stones_02.glb"
        };

        public static readonly string[] OreMeshes =
        {
            "Meshes/Resources/ore_nugget_gold_01.glb",
            "Meshes/Resources/ore_nugget_gold_02.glb",
            "Meshes/Resources/ore_nugget_gold_03.glb",
            "Meshes/Resources/ore_cluster_gold_01.glb",
            "Meshes/Resources/ore_cluster_gold_02.glb"
        };

        public static readonly string[] CrystalMeshes =
        {
            "Meshes/Resources/crystal_cluster_blue_01.glb"
        };

        public static readonly string[] SalvageMeshes =
        {
            "Meshes/Resources/salvage_scrap_01.glb",
            "Meshes/Resources/salvage_scrap_02.glb"
        };

        public static readonly string[] EnergyMeshes =
        {
            "Meshes/Resources/energy_node_01.glb"
        };

        public const string BasePadMesh = "Meshes/BasePads/base_pad_14x14.glb";
        public const string BasePadTrimStraightMesh = "Meshes/BasePads/base_pad_trim_straight.glb";
        public const string BasePadTrimCornerMesh = "Meshes/BasePads/base_pad_trim_corner.glb";

        public static readonly string[] VegetationMeshes =
        {
            "Meshes/Vegetation/grass_tuft_01.glb",
            "Meshes/Vegetation/grass_tuft_02.glb",
            "Meshes/Vegetation/bush_low_01.glb",
            "Meshes/Vegetation/bush_low_02.glb",
            "Meshes/Vegetation/tree_small_01.glb",
            "Meshes/Vegetation/tree_small_02.glb",
            "Meshes/Vegetation/stump_01.glb"
        };

        public static readonly string[] RiverMeshes =
        {
            "Meshes/River/river_rock_cluster_01.glb",
            "Meshes/River/river_rock_cluster_02.glb",
            "Meshes/River/reed_clump_01.glb",
            "Meshes/River/reed_clump_02.glb"
        };

        public static readonly string[] CraterMeshes =
        {
            "Meshes/Craters/crater_small_mesh_01.glb",
            "Meshes/Craters/crater_medium_mesh_01.glb",
            "Meshes/Craters/crater_large_mesh_01.glb"
        };

        static readonly Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, GameObject> ProxyCache = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        static readonly HashSet<string> MissingPrefabPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static bool IsAvailable
        {
            get { return AssetDatabase.IsValidFolder(Root); }
        }

        public static string Pick(string[] candidates, int seed, int x, int y)
        {
            if (candidates == null || candidates.Length == 0)
                return null;

            unchecked
            {
                var h = (uint)seed;
                h ^= (uint)(x * 374761393);
                h = (h << 13) | (h >> 19);
                h ^= (uint)(y * 668265263);
                h *= 1274126177u;
                h ^= h >> 16;
                return candidates[(int)(h % (uint)candidates.Length)];
            }
        }

        public static Material Material(string fileName, Color color, bool textured, bool persistAssets, bool transparent = false, string albedoPath = null, string normalPath = null, string maskPath = null)
        {
            var albedo = LoadTexture(albedoPath, false, transparent);
            var normal = LoadTexture(normalPath, true, false);
            var mask = LoadTexture(maskPath, false, false);
            var wantsTexture = textured || albedo != null;

            Material material;
            if (persistAssets)
            {
                EnsureFolder("Assets/Rts", "MapEditor");
                EnsureFolder(AegisMapEditorPaths.MapEditorRoot, "VisualAssets");
                var path = AegisMapEditorPaths.VisualAssetsFolder + "/" + fileName;
                material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(FindShader(wantsTexture, transparent));
                    AssetDatabase.CreateAsset(material, path);
                }
            }
            else
            {
                material = new Material(FindShader(wantsTexture, transparent));
                material.name = Path.GetFileNameWithoutExtension(fileName) + "_transient";
            }

            ConfigureMaterial(material, color, wantsTexture, transparent, albedo, normal, mask);
            if (persistAssets)
                EditorUtility.SetDirty(material);
            return material;
        }

        public static bool TryInstantiatePrefab(Transform parent, string name, string relativePath, Vector3 position, Quaternion rotation, Vector3 localScale, Material fallbackMaterial)
        {
            if (string.IsNullOrEmpty(relativePath) || !IsAvailable)
                return false;

            var normalizedPath = NormalizeRelativePath(relativePath);
            var prefab = LoadPrefab(relativePath);
            var resolutionKind = ResolutionImportedModel;
            var sourceAssetPath = AssetPath(normalizedPath);
            if (prefab == null)
            {
                prefab = EnsureGeneratedProxy(normalizedPath, fallbackMaterial);
                resolutionKind = ResolutionArtPackDerivedProxy;
                sourceAssetPath = GeneratedProxyAssetPath(normalizedPath);
            }

            if (prefab == null)
                return false;

            var instanceObject = PrefabUtility.InstantiatePrefab(prefab);
            var instance = instanceObject as GameObject;
            if (instance == null)
                instance = UnityEngine.Object.Instantiate(prefab);
            if (instance == null)
                return false;

            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = localScale;
            if (string.Equals(resolutionKind, ResolutionArtPackDerivedProxy, StringComparison.OrdinalIgnoreCase))
                OverrideMaterials(instance, fallbackMaterial);
            else
                AssignFallbackMaterial(instance, fallbackMaterial);
            StripColliders(instance);
            MarkArtPackInstance(instance, normalizedPath, sourceAssetPath, resolutionKind);
            return true;
        }

        public static GameObject EnsureGeneratedProxy(string relativePath, Material fallbackMaterial)
        {
            if (string.IsNullOrEmpty(relativePath) || !IsAvailable)
                return null;

            var normalizedPath = NormalizeRelativePath(relativePath);
            var sourceAssetPath = AssetPath(normalizedPath);
            var sourceDiskPath = ToDiskPath(sourceAssetPath);
            if (!File.Exists(sourceDiskPath))
                return null;

            EnsureGeneratedProxyFolders();

            var proxyPath = GeneratedProxyAssetPath(normalizedPath);
            GameObject proxy;
            if (ProxyCache.TryGetValue(proxyPath, out proxy) && proxy != null)
                return proxy;

            proxy = AssetDatabase.LoadAssetAtPath<GameObject>(proxyPath);
            if (proxy != null)
            {
                ProxyCache[proxyPath] = proxy;
                return proxy;
            }

            var temp = CreateGeneratedProxyRoot(normalizedPath, fallbackMaterial);
            if (temp == null)
                return null;

            MarkArtPackInstance(temp, normalizedPath, proxyPath, ResolutionArtPackDerivedProxy);
            var saved = PrefabUtility.SaveAsPrefabAsset(temp, proxyPath);
            UnityEngine.Object.DestroyImmediate(temp);
            AssetDatabase.ImportAsset(proxyPath);
            AssetDatabase.SaveAssets();

            proxy = saved == null ? AssetDatabase.LoadAssetAtPath<GameObject>(proxyPath) : saved;
            if (proxy != null)
                ProxyCache[proxyPath] = proxy;
            return proxy;
        }

        public static void CountInstances(GameObject root, out int importedModels, out int artPackDerivedProxies)
        {
            importedModels = 0;
            artPackDerivedProxies = 0;
            if (root == null)
                return;

            var markers = root.GetComponentsInChildren<AegisArtPackVisualInstance>(true);
            for (var i = 0; i < markers.Length; i++)
            {
                if (markers[i] == null)
                    continue;

                if (string.Equals(markers[i].ResolutionKind, ResolutionArtPackDerivedProxy, StringComparison.OrdinalIgnoreCase))
                    artPackDerivedProxies++;
                else if (string.Equals(markers[i].ResolutionKind, ResolutionImportedModel, StringComparison.OrdinalIgnoreCase))
                    importedModels++;
            }
        }

        public static int CountArtPackTexturedMaterials(GameObject root)
        {
            if (root == null)
                return 0;

            var count = 0;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                if (materials == null)
                    continue;

                for (var j = 0; j < materials.Length; j++)
                    if (MaterialUsesArtPackTexture(materials[j]))
                        count++;
            }

            return count;
        }

        public static Texture2D LoadTexture(string relativePath, bool normalMap, bool transparent)
        {
            if (string.IsNullOrEmpty(relativePath) || !IsAvailable)
                return null;

            var path = AssetPath(relativePath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static string AssetPath(string relativePath)
        {
            return Root + "/" + NormalizeRelativePath(relativePath);
        }

        public static string GeneratedProxyAssetPath(string relativePath)
        {
            return GeneratedProxyRoot + "/" + "artpack_proxy_" + AegisVisualCompilerPrimitives.Sanitize(NormalizeRelativePath(relativePath)) + ".prefab";
        }

        static GameObject LoadPrefab(string relativePath)
        {
            var path = AssetPath(relativePath);
            GameObject prefab;
            if (PrefabCache.TryGetValue(path, out prefab))
                return prefab;
            if (MissingPrefabPaths.Contains(path))
                return null;

            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                PrefabCache[path] = prefab;
                return prefab;
            }

            MissingPrefabPaths.Add(path);
            return null;
        }

        static string NormalizeRelativePath(string relativePath)
        {
            return string.IsNullOrEmpty(relativePath) ? string.Empty : relativePath.Replace('\\', '/').TrimStart('/');
        }

        static string ToDiskPath(string assetPath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        static void EnsureGeneratedProxyFolders()
        {
            EnsureFolder("Assets/Rts", "MapEditor");
            EnsureFolder(AegisMapEditorPaths.MapEditorRoot, "ArtPack");
            EnsureFolder("Assets/Rts/MapEditor/ArtPack", "GeneratedProxies");
        }

        static void MarkArtPackInstance(GameObject instance, string relativePath, string sourceAssetPath, string resolutionKind)
        {
            if (instance == null)
                return;

            var marker = instance.GetComponent<AegisArtPackVisualInstance>();
            if (marker == null)
                marker = instance.AddComponent<AegisArtPackVisualInstance>();

            marker.SourceRelativePath = NormalizeRelativePath(relativePath);
            marker.SourceAssetPath = sourceAssetPath;
            marker.ResolutionKind = resolutionKind;
        }

        static GameObject CreateGeneratedProxyRoot(string relativePath, Material fallbackMaterial)
        {
            var normalized = NormalizeRelativePath(relativePath);
            var lower = normalized.ToLowerInvariant();
            var root = new GameObject("artpack_proxy_" + AegisVisualCompilerPrimitives.Sanitize(normalized));

            if (lower.Contains("/basepads/"))
            {
                AddProxyPart(root, PrimitiveType.Cube, "proxy_base_pad_panel", Vector3.zero, new Vector3(14f, 0.12f, 14f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cube, "proxy_base_pad_trim_north", new Vector3(0f, 0.08f, 6.8f), new Vector3(14.2f, 0.08f, 0.42f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cube, "proxy_base_pad_trim_south", new Vector3(0f, 0.08f, -6.8f), new Vector3(14.2f, 0.08f, 0.42f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cube, "proxy_base_pad_trim_east", new Vector3(6.8f, 0.08f, 0f), new Vector3(0.42f, 0.08f, 14.2f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cube, "proxy_base_pad_trim_west", new Vector3(-6.8f, 0.08f, 0f), new Vector3(0.42f, 0.08f, 14.2f), Quaternion.identity, fallbackMaterial);
            }
            else if (lower.Contains("/cliffs/"))
            {
                AddProxyPart(root, PrimitiveType.Cube, "proxy_cliff_body", new Vector3(0f, 0.35f, 0f), new Vector3(0.95f, 0.9f, 0.42f), Quaternion.Euler(0f, 0f, 0f), fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cube, "proxy_cliff_cap", new Vector3(0.08f, 0.85f, -0.06f), new Vector3(0.72f, 0.25f, 0.55f), Quaternion.Euler(0f, 12f, 0f), fallbackMaterial);
            }
            else if (lower.Contains("/resources/"))
            {
                AddProxyPart(root, PrimitiveType.Sphere, "proxy_resource_core_a", new Vector3(-0.18f, 0.16f, -0.05f), new Vector3(0.52f, 0.34f, 0.45f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Sphere, "proxy_resource_core_b", new Vector3(0.18f, 0.12f, 0.14f), new Vector3(0.38f, 0.28f, 0.42f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cube, "proxy_resource_shard", new Vector3(0.04f, 0.36f, -0.08f), new Vector3(0.20f, 0.46f, 0.20f), Quaternion.Euler(0f, 28f, 9f), fallbackMaterial);
            }
            else if (lower.Contains("/vegetation/"))
            {
                AddProxyPart(root, PrimitiveType.Cylinder, "proxy_vegetation_stem", new Vector3(0f, 0.32f, 0f), new Vector3(0.14f, 0.46f, 0.14f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Sphere, "proxy_vegetation_canopy", new Vector3(0f, 0.78f, 0f), new Vector3(0.62f, 0.44f, 0.62f), Quaternion.identity, fallbackMaterial);
            }
            else if (lower.Contains("/river/"))
            {
                AddProxyPart(root, PrimitiveType.Sphere, "proxy_river_rock_a", new Vector3(-0.22f, 0.08f, -0.04f), new Vector3(0.48f, 0.18f, 0.38f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Sphere, "proxy_river_rock_b", new Vector3(0.24f, 0.07f, 0.08f), new Vector3(0.36f, 0.14f, 0.32f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cylinder, "proxy_river_reed", new Vector3(0.02f, 0.34f, 0.2f), new Vector3(0.06f, 0.52f, 0.06f), Quaternion.Euler(5f, 0f, 7f), fallbackMaterial);
            }
            else if (lower.Contains("/craters/"))
            {
                AddProxyPart(root, PrimitiveType.Cylinder, "proxy_crater_rim", new Vector3(0f, 0.035f, 0f), new Vector3(0.9f, 0.05f, 0.9f), Quaternion.identity, fallbackMaterial);
                AddProxyPart(root, PrimitiveType.Cylinder, "proxy_crater_center", new Vector3(0f, 0.055f, 0f), new Vector3(0.48f, 0.035f, 0.48f), Quaternion.identity, fallbackMaterial);
            }
            else
            {
                AddProxyPart(root, PrimitiveType.Cube, "proxy_artpack_mesh", new Vector3(0f, 0.2f, 0f), new Vector3(0.65f, 0.42f, 0.65f), Quaternion.identity, fallbackMaterial);
            }

            return root;
        }

        static GameObject AddProxyPart(GameObject root, PrimitiveType primitiveType, string name, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            var part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(root.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            AssignFallbackMaterial(part, material);
            if (material != null)
            {
                var renderer = part.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = material;
            }
            StripColliders(part);
            return part;
        }

        static void AssignFallbackMaterial(GameObject root, Material fallbackMaterial)
        {
            if (fallbackMaterial == null)
                return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderers[i].sharedMaterial = fallbackMaterial;
                    continue;
                }

                for (var j = 0; j < materials.Length; j++)
                    if (materials[j] == null)
                        materials[j] = fallbackMaterial;
                renderers[i].sharedMaterials = materials;
            }
        }

        static void OverrideMaterials(GameObject root, Material material)
        {
            if (root == null || material == null)
                return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].sharedMaterial = material;
        }

        static void StripColliders(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
                UnityEngine.Object.DestroyImmediate(colliders[i]);
        }

        static bool MaterialUsesArtPackTexture(Material material)
        {
            if (material == null)
                return false;

            if (TextureComesFromArtPack(material.mainTexture))
                return true;

            if (material.HasProperty("_BaseMap") && TextureComesFromArtPack(material.GetTexture("_BaseMap")))
                return true;

            if (material.HasProperty("_MainTex") && TextureComesFromArtPack(material.GetTexture("_MainTex")))
                return true;

            if (material.HasProperty("_BumpMap") && TextureComesFromArtPack(material.GetTexture("_BumpMap")))
                return true;

            if (material.HasProperty("_OcclusionMap") && TextureComesFromArtPack(material.GetTexture("_OcclusionMap")))
                return true;

            return false;
        }

        static bool TextureComesFromArtPack(Texture texture)
        {
            if (texture == null)
                return false;

            var assetPath = AssetDatabase.GetAssetPath(texture);
            return !string.IsNullOrEmpty(assetPath) && assetPath.StartsWith(Root + "/", StringComparison.OrdinalIgnoreCase);
        }

        static void ConfigureMaterial(Material material, Color color, bool textured, bool transparent, Texture2D albedo, Texture2D normal, Texture2D mask)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (albedo != null)
            {
                material.mainTexture = albedo;
                if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", albedo);
                if (material.HasProperty("_MainTex"))
                    material.SetTexture("_MainTex", albedo);
            }
            if (normal != null)
            {
                if (material.HasProperty("_BumpMap"))
                    material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }
            if (mask != null)
            {
                if (material.HasProperty("_OcclusionMap"))
                    material.SetTexture("_OcclusionMap", mask);
                if (material.HasProperty("_MetallicGlossMap"))
                    material.SetTexture("_MetallicGlossMap", mask);
            }

            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", textured ? 0.08f : 0.12f);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", textured ? 0.06f : 0.08f);
            if (material.HasProperty("_SpecColor"))
                material.SetColor("_SpecColor", new Color(0.025f, 0.025f, 0.022f, 1f));

            if (transparent)
            {
                if (material.HasProperty("_Surface"))
                    material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_Mode"))
                    material.SetFloat("_Mode", 3f);
                if (material.HasProperty("_SrcBlend"))
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (material.HasProperty("_DstBlend"))
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (material.HasProperty("_ZWrite"))
                    material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;
            }
            else
            {
                if (material.HasProperty("_Surface"))
                    material.SetFloat("_Surface", 0f);
                if (material.HasProperty("_ZWrite"))
                    material.SetFloat("_ZWrite", 1f);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = -1;
            }
        }

        static Shader FindShader(bool textured, bool transparent)
        {
            var shader = transparent ? Shader.Find("Universal Render Pipeline/Unlit") : Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null && transparent)
                shader = Shader.Find("Unlit/Transparent");
            if (shader == null && textured)
                shader = Shader.Find("Unlit/Texture");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            return shader;
        }

        static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
                return;

            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
