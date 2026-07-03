using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage30VisualReadabilityAssetCreator
    {
        public const string ReadabilityMaterialFolder = "Assets/Rts/Art/Materials/Readability";
        public const string ReadabilityProfileFolder = "Assets/Rts/ScriptableObjects/Art/Readability";
        public const string ReadabilityProfilePath = ReadabilityProfileFolder + "/stage30_visual_readability_profile.asset";
        public const string ReadabilityLayerName = "Stage30 Readability Layer";

        [MenuItem("ProjectAegisRTS/Stage 30/Generate Visual Readability Assets")]
        public static void GenerateStage30AssetsMenu()
        {
            EnsureStage30Assets();
        }

        public static Stage30AssetSummary EnsureStage30Assets()
        {
            Stage29BattlefieldVisualAssetCreator.EnsureStage29Assets();
            EnsureFolders();
            var materials = CreateMaterials();
            CreateReadabilityProfile();
            var proxyCount = ApplyReadabilityOverlay(materials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 30 visual readability assets updated. Proxy readability overlays: " + proxyCount);
            return new Stage30AssetSummary { ProxyReadabilityCount = proxyCount };
        }

        public static Stage30ReadabilityProfile LoadReadabilityProfile()
        {
            return AssetDatabase.LoadAssetAtPath<Stage30ReadabilityProfile>(ReadabilityProfilePath);
        }

        static void EnsureFolders()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            EnsureFolder("Assets/Rts/Art/Materials", "Readability");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "Readability");
        }

        static Stage30MaterialSet CreateMaterials()
        {
            return new Stage30MaterialSet
            {
                Outline = CreateMaterial(ReadabilityMaterialFolder + "/stage30_ground_contrast_outline.mat", new Color(0.035f, 0.045f, 0.04f, 1f), 0.12f, 0f),
                PlayerTrim = CreateMaterial(ReadabilityMaterialFolder + "/stage30_player_readability_trim.mat", new Color(0.20f, 0.72f, 0.68f, 1f), 0.30f, 0f),
                EnemyTrim = CreateMaterial(ReadabilityMaterialFolder + "/stage30_enemy_readability_trim.mat", new Color(0.78f, 0.26f, 0.18f, 1f), 0.26f, 0f),
                ResourcePop = CreateMaterial(ReadabilityMaterialFolder + "/stage30_resource_readability_pop.mat", new Color(0.18f, 0.95f, 0.74f, 1f), 0.58f, 0f),
                GridCue = CreateMaterial(ReadabilityMaterialFolder + "/stage30_fine_grid_readability_cue.mat", new Color(0.48f, 0.82f, 0.78f, 0.50f), 0.14f, 0f)
            };
        }

        static void CreateReadabilityProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<Stage30ReadabilityProfile>(ReadabilityProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<Stage30ReadabilityProfile>();
                AssetDatabase.CreateAsset(profile, ReadabilityProfilePath);
            }

            profile.ConfigureStage30Default();
            EditorUtility.SetDirty(profile);
        }

        static int ApplyReadabilityOverlay(Stage30MaterialSet materials)
        {
            var specs = Stage8ActorCatalog.LoadSpecs();
            var byId = new Dictionary<string, Stage8ActorSpec>();
            for (var i = 0; i < specs.Count; i++)
                byId[specs[i].ActorTypeId] = specs[i];

            var count = 0;
            for (var i = 0; i < Stage20MvpVisualActorSet.ActorTypeIds.Length; i++)
            {
                var actorTypeId = Stage20MvpVisualActorSet.ActorTypeIds[i];
                Stage8ActorSpec spec;
                if (!byId.TryGetValue(actorTypeId, out spec))
                    continue;

                var prefabPath = Stage20MvpProductionProxyGenerator.ProductionProxyPath(spec);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                    continue;

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ApplyProxyReadability(root, spec, materials);
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    count++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            return count;
        }

        static void ApplyProxyReadability(GameObject root, Stage8ActorSpec spec, Stage30MaterialSet materials)
        {
            var existing = root.transform.Find(ReadabilityLayerName);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);

            var layer = new GameObject(ReadabilityLayerName);
            layer.transform.SetParent(root.transform, false);

            var width = Mathf.Max(1f, spec.FootprintWidth);
            var depth = Mathf.Max(1f, spec.FootprintHeight);
            CreatePrimitive(layer.transform, "Stage30 Dark Silhouette Ground Cut", PrimitiveType.Cube, new Vector3(0f, -0.046f, 0f), new Vector3(width * 1.10f, 0.022f, depth * 1.10f), materials.Outline);
            CreatePrimitive(layer.transform, "Stage30 Front Direction Cue", PrimitiveType.Cube, new Vector3(0f, 0.075f, depth * 0.57f), new Vector3(width * 0.58f, 0.060f, 0.060f), materials.PlayerTrim);
            CreatePrimitive(layer.transform, "Stage30 Top Identity Stripe", PrimitiveType.Cube, new Vector3(0f, ReadabilityHeightFor(spec), 0f), new Vector3(width * 0.42f, 0.045f, 0.090f), materials.PlayerTrim);

            if (spec.ActorTypeId == "refinery" || spec.ActorTypeId == "harvester")
                CreatePrimitive(layer.transform, "Stage30 Resource Relationship Pop", PrimitiveType.Sphere, new Vector3(-width * 0.34f, ReadabilityHeightFor(spec) + 0.05f, depth * 0.24f), new Vector3(0.16f, 0.16f, 0.16f), materials.ResourcePop);
            if (spec.ActorTypeId == "gun_tower" || spec.ActorTypeId == "light_tank" || spec.ActorTypeId == "rifle_infantry")
                CreatePrimitive(layer.transform, "Stage30 Combat Role Accent", PrimitiveType.Cube, new Vector3(width * 0.34f, ReadabilityHeightFor(spec) + 0.04f, depth * 0.18f), new Vector3(0.18f, 0.06f, 0.18f), materials.EnemyTrim);

            var tag = root.GetComponent<Stage30VisualReadabilityTag>();
            if (tag == null)
                tag = root.AddComponent<Stage30VisualReadabilityTag>();
            tag.actorTypeId = spec.ActorTypeId;
            tag.hasGroundContrastOutline = true;
            tag.hasTopDownIdentityAccent = true;
            tag.hasForwardReadabilityCue = true;
            tag.preservesStage29Detail = root.GetComponentInChildren<Stage29VisualDetailTag>(true) != null && root.transform.Find(Stage29BattlefieldVisualAssetCreator.DetailRootName) != null;
            tag.questSafeOverlayBudget = layer.GetComponentsInChildren<Renderer>(true).Length <= 6;
            tag.notes = "Stage 30 additive readability overlay: dark ground cut, top identity stripe, forward cue, and role/resource accents. Gameplay data and Stage 29 detail are preserved.";

            EditorUtility.SetDirty(root);
        }

        static float ReadabilityHeightFor(Stage8ActorSpec spec)
        {
            if (spec.Category == ActorArtCategory.Infantry)
                return 1.04f;
            if (spec.Category == ActorArtCategory.Vehicle)
                return 0.86f;
            return 1.22f;
        }

        static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = localScale;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            return obj;
        }

        static Material CreateMaterial(string path, Color color, float smoothness, float metallic)
        {
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
            EditorUtility.SetDirty(material);
            return material;
        }

        static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        sealed class Stage30MaterialSet
        {
            public Material Outline;
            public Material PlayerTrim;
            public Material EnemyTrim;
            public Material ResourcePop;
            public Material GridCue;
        }
    }

    public sealed class Stage30AssetSummary
    {
        public int ProxyReadabilityCount;
    }
}
