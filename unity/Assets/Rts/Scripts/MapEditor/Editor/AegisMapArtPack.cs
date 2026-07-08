#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.MapEditor;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    static class AegisMapArtPack
    {
        public const string Root = "Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1";

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

            var prefab = LoadPrefab(relativePath);
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
            AssignFallbackMaterial(instance, fallbackMaterial);
            StripColliders(instance);
            return true;
        }

        public static Texture2D LoadTexture(string relativePath, bool normalMap, bool transparent)
        {
            if (string.IsNullOrEmpty(relativePath) || !IsAvailable)
                return null;

            var path = Root + "/" + relativePath.Replace('\\', '/');
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                var dirty = false;
                if (normalMap && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    dirty = true;
                }
                else if (!normalMap && importer.textureType == TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.Default;
                    dirty = true;
                }

                if (transparent && !importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }

                if (dirty)
                    importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static GameObject LoadPrefab(string relativePath)
        {
            var path = Root + "/" + relativePath.Replace('\\', '/');
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
                    materials[j] = fallbackMaterial;
                renderers[i].sharedMaterials = materials;
            }
        }

        static void StripColliders(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
                UnityEngine.Object.DestroyImmediate(colliders[i]);
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
