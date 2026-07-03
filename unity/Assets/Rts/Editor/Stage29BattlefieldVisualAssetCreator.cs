using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage29BattlefieldVisualAssetCreator
    {
        public const string TerrainMaterialFolder = "Assets/Rts/Art/Materials/Terrain";
        public const string EnvironmentMaterialFolder = "Assets/Rts/Art/Materials/Environment";
        public const string TerrainProfileFolder = "Assets/Rts/ScriptableObjects/Art/TerrainMaterialProfiles";
        public const string LightingProfileFolder = "Assets/Rts/ScriptableObjects/Art/Lighting";
        public const string LightingProfilePath = LightingProfileFolder + "/stage29_realistic_battlefield_lighting.asset";
        public const string DetailRootName = "Stage29 Realistic Visual Detail";

        public static readonly string[] RequiredTerrainKinds =
        {
            "GrassDirt",
            "CompactedBase",
            "ConcretePad",
            "RoadPath",
            "ResourceField",
            "RockBlocked",
            "Water",
            "FogExplored"
        };

        [MenuItem("ProjectAegisRTS/Stage 29/Generate Battlefield Visual Assets")]
        public static void GenerateStage29AssetsMenu()
        {
            EnsureStage29Assets();
        }

        public static Stage29AssetSummary EnsureStage29Assets()
        {
            Stage20MvpProductionProxyGenerator.GenerateMvpProductionProxies();
            EnsureFolders();
            var materials = CreateMaterials();
            var profileCount = CreateTerrainProfiles(materials);
            CreateLightingProfile();
            var proxyCount = ApplyStage29ProxyDetails(materials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 29 battlefield visual assets updated. Terrain profiles: " + profileCount + ", proxy detail pass: " + proxyCount);
            return new Stage29AssetSummary { TerrainProfileCount = profileCount, ProxyDetailCount = proxyCount };
        }

        public static LightingProfile LoadLightingProfile()
        {
            return AssetDatabase.LoadAssetAtPath<LightingProfile>(LightingProfilePath);
        }

        public static string TerrainProfilePath(string terrainKind)
        {
            return TerrainProfileFolder + "/stage29_" + ToAssetName(terrainKind) + "_profile.asset";
        }

        static void EnsureFolders()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            EnsureFolder("Assets/Rts/Art/Materials", "Terrain");
            EnsureFolder("Assets/Rts/Art/Materials", "Environment");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "TerrainMaterialProfiles");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "Lighting");
        }

        static MaterialSet CreateMaterials()
        {
            return new MaterialSet
            {
                GrassDirt = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_grass_dirt.mat", new Color(0.24f, 0.34f, 0.22f, 1f), 0.12f, 0f),
                CompactedBase = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_compacted_base.mat", new Color(0.30f, 0.29f, 0.24f, 1f), 0.18f, 0f),
                ConcretePad = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_concrete_pad.mat", new Color(0.46f, 0.48f, 0.43f, 1f), 0.22f, 0f),
                RoadPath = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_road_path.mat", new Color(0.25f, 0.24f, 0.21f, 1f), 0.10f, 0f),
                ResourceField = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_resource_field.mat", new Color(0.21f, 0.49f, 0.40f, 1f), 0.30f, 0f),
                RockBlocked = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_rock_blocked.mat", new Color(0.27f, 0.26f, 0.24f, 1f), 0.08f, 0f),
                Water = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_water.mat", new Color(0.10f, 0.22f, 0.31f, 0.92f), 0.58f, 0f),
                FogExplored = CreateMaterial(TerrainMaterialFolder + "/stage29_terrain_fog_explored.mat", new Color(0.16f, 0.19f, 0.19f, 0.82f), 0.05f, 0f),
                FoundationEdge = CreateMaterial(EnvironmentMaterialFolder + "/stage29_foundation_edge.mat", new Color(0.20f, 0.21f, 0.19f, 1f), 0.16f, 0f),
                WornMetal = CreateMaterial(EnvironmentMaterialFolder + "/stage29_worn_metal.mat", new Color(0.29f, 0.32f, 0.31f, 1f), 0.32f, 0.10f),
                DarkMetal = CreateMaterial(EnvironmentMaterialFolder + "/stage29_dark_metal.mat", new Color(0.10f, 0.11f, 0.11f, 1f), 0.18f, 0.08f),
                WarmLight = CreateMaterial(EnvironmentMaterialFolder + "/stage29_warm_light.mat", new Color(0.95f, 0.70f, 0.28f, 1f), 0.45f, 0f),
                CautionPaint = CreateMaterial(EnvironmentMaterialFolder + "/stage29_caution_paint.mat", new Color(0.86f, 0.55f, 0.18f, 1f), 0.28f, 0f),
                ResourceCrystal = CreateMaterial(EnvironmentMaterialFolder + "/stage29_resource_crystal.mat", new Color(0.14f, 0.86f, 0.68f, 1f), 0.62f, 0f),
                UnitArmor = CreateMaterial(EnvironmentMaterialFolder + "/stage29_unit_armor.mat", new Color(0.25f, 0.34f, 0.29f, 1f), 0.25f, 0.05f),
                InfantryCloth = CreateMaterial(EnvironmentMaterialFolder + "/stage29_infantry_cloth.mat", new Color(0.23f, 0.36f, 0.27f, 1f), 0.15f, 0f),
                FineGridGuide = CreateMaterial(EnvironmentMaterialFolder + "/stage29_fine_grid_guide.mat", new Color(0.44f, 0.64f, 0.62f, 0.55f), 0.10f, 0f)
            };
        }

        static int CreateTerrainProfiles(MaterialSet materials)
        {
            var count = 0;
            CreateTerrainProfile("GrassDirt", "Grass / Dirt Field", materials.GrassDirt, new Color(0.24f, 0.34f, 0.22f, 1f), new Color(0.44f, 0.38f, 0.25f, 1f), false, false, true, "Mixed grass and dirt creates a realistic battlefield base while keeping the fine-grid overlay readable.");
            count++;
            CreateTerrainProfile("CompactedBase", "Compacted Base Ground", materials.CompactedBase, new Color(0.30f, 0.29f, 0.24f, 1f), new Color(0.45f, 0.43f, 0.36f, 1f), false, false, true, "Compacted base ground grounds buildings and distinguishes the starting build zone.");
            count++;
            CreateTerrainProfile("ConcretePad", "Concrete Pad", materials.ConcretePad, new Color(0.46f, 0.48f, 0.43f, 1f), new Color(0.70f, 0.69f, 0.62f, 1f), false, false, true, "Concrete pads show safe building placement scale and foundation boundaries.");
            count++;
            CreateTerrainProfile("RoadPath", "Road / Path", materials.RoadPath, new Color(0.25f, 0.24f, 0.21f, 1f), new Color(0.50f, 0.46f, 0.36f, 1f), false, false, false, "Road paths shape scouting and early enemy-pressure direction without becoming gameplay authority.");
            count++;
            CreateTerrainProfile("ResourceField", "Resource Field", materials.ResourceField, new Color(0.21f, 0.49f, 0.40f, 1f), new Color(0.75f, 0.68f, 0.32f, 1f), false, true, false, "Resource fields use a distinct mineral tint and shards so harvestable space reads from the PC camera.");
            count++;
            CreateTerrainProfile("RockBlocked", "Rock / Blocked Terrain", materials.RockBlocked, new Color(0.27f, 0.26f, 0.24f, 1f), new Color(0.47f, 0.45f, 0.39f, 1f), true, false, false, "Blocked rock terrain uses dark clusters and contrast edges to communicate no-build/no-ground movement.");
            count++;
            CreateTerrainProfile("Water", "Water", materials.Water, new Color(0.10f, 0.22f, 0.31f, 1f), new Color(0.26f, 0.45f, 0.54f, 1f), true, false, false, "Water is visually separate from passable ground and remains compatible with simple Quest-safe materials.");
            count++;
            CreateTerrainProfile("FogExplored", "Fog / Explored Tint", materials.FogExplored, new Color(0.16f, 0.19f, 0.19f, 0.82f), new Color(0.28f, 0.34f, 0.34f, 1f), false, false, false, "Explored fog is muted so actors, selection, and placement previews remain readable.");
            count++;
            return count;
        }

        static void CreateTerrainProfile(string kind, string label, Material material, Color baseTint, Color accentTint, bool blocked, bool resource, bool placement, string notes)
        {
            var path = TerrainProfilePath(kind);
            var profile = AssetDatabase.LoadAssetAtPath<TerrainMaterialProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<TerrainMaterialProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.Configure(kind, label, material, baseTint, accentTint, blocked, resource, placement, notes);
            EditorUtility.SetDirty(profile);
        }

        static void CreateLightingProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<LightingProfile>(LightingProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<LightingProfile>();
                AssetDatabase.CreateAsset(profile, LightingProfilePath);
            }

            profile.ConfigureStage29Default();
            EditorUtility.SetDirty(profile);
        }

        static int ApplyStage29ProxyDetails(MaterialSet materials)
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
                    ApplyProxyDetails(root, spec, materials);
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

        static void ApplyProxyDetails(GameObject root, Stage8ActorSpec spec, MaterialSet materials)
        {
            var existing = root.transform.Find(DetailRootName);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);

            RemapExistingMaterials(root, spec, materials);

            var detailRoot = new GameObject(DetailRootName);
            detailRoot.transform.SetParent(root.transform, false);
            CreateGrounding(detailRoot.transform, spec, materials);

            if (IsBuildingLike(spec))
                CreateBuildingRealismDetails(detailRoot.transform, spec, materials);
            else if (spec.ActorTypeId == "rifle_infantry")
                CreateInfantryDetails(detailRoot.transform, materials);
            else
                CreateVehicleDetails(detailRoot.transform, spec, materials);

            AddTypeSpecificDetails(detailRoot.transform, spec, materials);
            UpdateStage29Tag(root, spec);
            AppendNotes(root, spec);
            RefreshLod(root);
            EditorUtility.SetDirty(root);
        }

        static void RemapExistingMaterials(GameObject root, Stage8ActorSpec spec, MaterialSet materials)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.sharedMaterial == null)
                    continue;

                var materialName = renderer.sharedMaterial.name.ToLowerInvariant();
                if (materialName.Contains("foundation"))
                    renderer.sharedMaterial = materials.FoundationEdge;
                else if (materialName.Contains("track"))
                    renderer.sharedMaterial = materials.DarkMetal;
                else if (materialName.Contains("weapon"))
                    renderer.sharedMaterial = materials.WornMetal;
                else if (materialName.Contains("power") || materialName.Contains("glow"))
                    renderer.sharedMaterial = materials.WarmLight;
                else if (materialName.Contains("infantry"))
                    renderer.sharedMaterial = materials.InfantryCloth;
                else if (spec.Category == ActorArtCategory.Vehicle)
                    renderer.sharedMaterial = materials.UnitArmor;
            }
        }

        static void CreateGrounding(Transform parent, Stage8ActorSpec spec, MaterialSet materials)
        {
            var width = Mathf.Max(1f, spec.FootprintWidth);
            var depth = Mathf.Max(1f, spec.FootprintHeight);
            CreatePrimitive(parent, "Stage29 Dirt Contact Shadow", PrimitiveType.Cube, new Vector3(0f, -0.012f, 0f), new Vector3(width * 1.05f, 0.018f, depth * 1.05f), materials.CompactedBase);
            CreatePrimitive(parent, "Stage29 Fine Grid Foundation Edge", PrimitiveType.Cube, new Vector3(0f, 0.035f, depth * 0.49f), new Vector3(width * 0.88f, 0.045f, 0.055f), materials.FineGridGuide);
            CreatePrimitive(parent, "Stage29 Rear Foundation Edge", PrimitiveType.Cube, new Vector3(0f, 0.035f, -depth * 0.49f), new Vector3(width * 0.64f, 0.04f, 0.05f), materials.FoundationEdge);
        }

        static void CreateBuildingRealismDetails(Transform parent, Stage8ActorSpec spec, MaterialSet materials)
        {
            var width = Mathf.Max(1f, spec.FootprintWidth);
            var depth = Mathf.Max(1f, spec.FootprintHeight);
            CreatePrimitive(parent, "Stage29 Left Wall Service Run", PrimitiveType.Cube, new Vector3(-width * 0.44f, 0.66f, 0f), new Vector3(0.075f, 0.10f, depth * 0.62f), materials.WornMetal);
            CreatePrimitive(parent, "Stage29 Right Wall Cable Tray", PrimitiveType.Cube, new Vector3(width * 0.44f, 0.58f, 0.05f), new Vector3(0.07f, 0.08f, depth * 0.54f), materials.DarkMetal);
            CreatePrimitive(parent, "Stage29 Roof Vent A", PrimitiveType.Cube, new Vector3(-width * 0.18f, 1.02f, -depth * 0.10f), new Vector3(width * 0.22f, 0.10f, 0.18f), materials.DarkMetal);
            CreatePrimitive(parent, "Stage29 Roof Vent B", PrimitiveType.Cube, new Vector3(width * 0.18f, 1.03f, depth * 0.12f), new Vector3(width * 0.18f, 0.08f, 0.16f), materials.WornMetal);
            CreatePrimitive(parent, "Stage29 Front Identity Light", PrimitiveType.Sphere, new Vector3(-width * 0.28f, 0.74f, depth * 0.48f), new Vector3(0.12f, 0.12f, 0.12f), materials.WarmLight);
            CreatePrimitive(parent, "Stage29 Rear Service Beacon", PrimitiveType.Sphere, new Vector3(width * 0.25f, 0.70f, -depth * 0.48f), new Vector3(0.10f, 0.10f, 0.10f), materials.CautionPaint);
        }

        static void CreateVehicleDetails(Transform parent, Stage8ActorSpec spec, MaterialSet materials)
        {
            CreatePrimitive(parent, "Stage29 Hull Nose Plate", PrimitiveType.Cube, new Vector3(0f, 0.35f, 0.43f), new Vector3(0.72f, 0.08f, 0.10f), materials.WornMetal);
            CreatePrimitive(parent, "Stage29 Engine Grille", PrimitiveType.Cube, new Vector3(0f, 0.54f, -0.36f), new Vector3(0.56f, 0.055f, 0.22f), materials.DarkMetal);
            CreatePrimitive(parent, "Stage29 Left Running Gear", PrimitiveType.Cube, new Vector3(-0.58f, 0.19f, 0f), new Vector3(0.08f, 0.16f, 0.78f), materials.DarkMetal);
            CreatePrimitive(parent, "Stage29 Right Running Gear", PrimitiveType.Cube, new Vector3(0.58f, 0.19f, 0f), new Vector3(0.08f, 0.16f, 0.78f), materials.DarkMetal);
            CreatePrimitive(parent, "Stage29 Rear Dust Flap", PrimitiveType.Cube, new Vector3(0f, 0.24f, -0.53f), new Vector3(0.68f, 0.18f, 0.06f), materials.FoundationEdge);
        }

        static void CreateInfantryDetails(Transform parent, MaterialSet materials)
        {
            CreatePrimitive(parent, "Stage29 Rifle Sling", PrimitiveType.Cube, new Vector3(-0.16f, 0.62f, 0.05f), new Vector3(0.035f, 0.40f, 0.035f), materials.DarkMetal);
            CreatePrimitive(parent, "Stage29 Shoulder Pad Left", PrimitiveType.Cube, new Vector3(-0.23f, 0.66f, 0.04f), new Vector3(0.16f, 0.08f, 0.12f), materials.UnitArmor);
            CreatePrimitive(parent, "Stage29 Shoulder Pad Right", PrimitiveType.Cube, new Vector3(0.23f, 0.66f, 0.04f), new Vector3(0.16f, 0.08f, 0.12f), materials.UnitArmor);
            CreatePrimitive(parent, "Stage29 Backpack Radio", PrimitiveType.Cube, new Vector3(0f, 0.58f, -0.23f), new Vector3(0.24f, 0.30f, 0.08f), materials.WornMetal);
            CreatePrimitive(parent, "Stage29 Muzzle Highlight", PrimitiveType.Cube, new Vector3(0.28f, 0.55f, 0.58f), new Vector3(0.06f, 0.05f, 0.12f), materials.WarmLight);
        }

        static void AddTypeSpecificDetails(Transform parent, Stage8ActorSpec spec, MaterialSet materials)
        {
            switch (spec.ActorTypeId)
            {
                case "fabrication_hub":
                    CreatePrimitive(parent, "Stage29 Crane Counterweight", PrimitiveType.Cube, new Vector3(-0.92f, 1.52f, -0.12f), new Vector3(0.34f, 0.18f, 0.24f), materials.CautionPaint);
                    CreatePrimitive(parent, "Stage29 Scaffold Upright Left", PrimitiveType.Cube, new Vector3(-1.15f, 0.78f, 0.58f), new Vector3(0.08f, 1.0f, 0.08f), materials.WornMetal);
                    CreatePrimitive(parent, "Stage29 Scaffold Upright Right", PrimitiveType.Cube, new Vector3(1.15f, 0.78f, 0.58f), new Vector3(0.08f, 1.0f, 0.08f), materials.WornMetal);
                    break;
                case "power_plant":
                    CreatePrimitive(parent, "Stage29 Cable Bundle A", PrimitiveType.Cube, new Vector3(-0.78f, 0.30f, 0.05f), new Vector3(0.10f, 0.10f, 1.05f), materials.DarkMetal);
                    CreatePrimitive(parent, "Stage29 Turbine Glow Cap", PrimitiveType.Cylinder, new Vector3(0f, 1.28f, 0f), new Vector3(0.44f, 0.08f, 0.44f), materials.WarmLight);
                    break;
                case "refinery":
                    CreatePrimitive(parent, "Stage29 Ore Bin Glow", PrimitiveType.Cube, new Vector3(-1.02f, 0.92f, 0.52f), new Vector3(0.38f, 0.16f, 0.38f), materials.ResourceCrystal);
                    CreatePrimitive(parent, "Stage29 Pipe Elbow", PrimitiveType.Cylinder, new Vector3(-0.68f, 0.86f, -0.36f), new Vector3(0.12f, 0.40f, 0.12f), materials.WornMetal);
                    break;
                case "barracks":
                    CreatePrimitive(parent, "Stage29 Bunk Roof Ridge", PrimitiveType.Cube, new Vector3(0f, 0.94f, -0.08f), new Vector3(1.15f, 0.10f, 0.16f), materials.WornMetal);
                    CreatePrimitive(parent, "Stage29 Muster Light", PrimitiveType.Sphere, new Vector3(0.52f, 0.58f, 0.65f), new Vector3(0.10f, 0.10f, 0.10f), materials.WarmLight);
                    break;
                case "war_factory":
                    CreatePrimitive(parent, "Stage29 Overhead Gantry", PrimitiveType.Cube, new Vector3(0f, 1.18f, 0.18f), new Vector3(2.15f, 0.10f, 0.12f), materials.CautionPaint);
                    CreatePrimitive(parent, "Stage29 Bay Floor Track", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0.62f), new Vector3(1.55f, 0.04f, 0.10f), materials.DarkMetal);
                    break;
                case "gun_tower":
                    CreatePrimitive(parent, "Stage29 Turret Shield", PrimitiveType.Cube, new Vector3(0f, 0.92f, 0.22f), new Vector3(0.68f, 0.22f, 0.10f), materials.UnitArmor);
                    CreatePrimitive(parent, "Stage29 Sensor Lens", PrimitiveType.Sphere, new Vector3(0.25f, 0.93f, 0.48f), new Vector3(0.10f, 0.10f, 0.10f), materials.WarmLight);
                    break;
                case "light_tank":
                    CreatePrimitive(parent, "Stage29 Turret Optic", PrimitiveType.Sphere, new Vector3(0.18f, 0.72f, 0.30f), new Vector3(0.10f, 0.10f, 0.10f), materials.WarmLight);
                    CreatePrimitive(parent, "Stage29 Side Stowage Box", PrimitiveType.Cube, new Vector3(-0.48f, 0.48f, -0.14f), new Vector3(0.14f, 0.18f, 0.34f), materials.FoundationEdge);
                    break;
                case "harvester":
                    CreatePrimitive(parent, "Stage29 Hopper Ore Glint", PrimitiveType.Cube, new Vector3(0f, 0.82f, -0.10f), new Vector3(0.62f, 0.08f, 0.32f), materials.ResourceCrystal);
                    CreatePrimitive(parent, "Stage29 Collector Tooth Row", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0.82f), new Vector3(1.00f, 0.08f, 0.08f), materials.CautionPaint);
                    break;
            }
        }

        static void UpdateStage29Tag(GameObject root, Stage8ActorSpec spec)
        {
            var tag = root.GetComponent<Stage29VisualDetailTag>();
            if (tag == null)
                tag = root.AddComponent<Stage29VisualDetailTag>();

            tag.actorTypeId = spec.ActorTypeId;
            tag.hasRealisticMaterialPass = true;
            tag.hasSilhouetteBreakup = true;
            tag.hasFineGridGrounding = true;
            tag.hasReadableTopProfile = true;
            tag.hasFrontSideRearCues = true;
            tag.preservesStage20Sockets = HasRequiredSockets(root, spec);
            tag.preservesAnimationHooks = HasAnimationHooks(root, spec);
            tag.questSafePrimitiveBudget = root.GetComponentsInChildren<Renderer>(true).Length <= 64;
            tag.notes = "Stage 29 additive detail pass: material remap, grounded contact shadow, fine-grid footprint edge, top silhouette breakup, and front/side/rear readable cues. Gameplay sockets and controllers are preserved.";
        }

        static void AppendNotes(GameObject root, Stage8ActorSpec spec)
        {
            var descriptor = root.GetComponent<ActorPrefabDescriptor>();
            if (descriptor != null)
                descriptor.notes = AppendOnce(descriptor.notes, "Stage 29: realistic battlefield detail pass applied; preserve sockets, pivot, footprint, LODGroup, and visual replacement metadata.");

            var tag = root.GetComponentInChildren<ProductionVisualValidationTag>(true);
            if (tag != null)
                tag.notes = AppendOnce(tag.notes, "Stage 29: proxy has material/readability upgrade for realistic battlefield review.");
        }

        static string AppendOnce(string notes, string marker)
        {
            if (!string.IsNullOrEmpty(notes) && notes.Contains(marker))
                return notes;
            return string.IsNullOrEmpty(notes) ? marker : notes.TrimEnd() + "\n" + marker;
        }

        static bool HasRequiredSockets(GameObject root, Stage8ActorSpec spec)
        {
            var descriptor = root.GetComponentInChildren<ActorPrefabDescriptor>(true);
            if (descriptor == null)
                return false;
            var required = Stage8ActorCatalog.RequiredSocketsFor(spec);
            if (spec.ActorTypeId == "refinery" && !required.Contains(ActorPrefabSocketKind.DockPumpRoot))
                required.Add(ActorPrefabSocketKind.DockPumpRoot);
            return descriptor.ValidateRequiredSockets(required).Count == 0;
        }

        static bool HasAnimationHooks(GameObject root, Stage8ActorSpec spec)
        {
            if (IsBuildingLike(spec) && root.GetComponentInChildren<BuildingVisualStateController>(true) == null)
                return false;
            if ((spec.Category == ActorArtCategory.Vehicle || spec.ActorTypeId == "harvester") && root.GetComponentInChildren<VehicleVisualMotionController>(true) == null)
                return false;
            if (spec.Category == ActorArtCategory.Infantry && root.GetComponentInChildren<InfantryVisualMotionController>(true) == null)
                return false;
            if ((spec.ActorTypeId == "light_tank" || spec.ActorTypeId == "gun_tower") && root.GetComponentInChildren<TurretVisualAimController>(true) == null)
                return false;
            return true;
        }

        static void RefreshLod(GameObject root)
        {
            var lod = root.GetComponent<LODGroup>();
            if (lod == null)
                lod = root.AddComponent<LODGroup>();
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                lod.SetLODs(new[] { new LOD(0.01f, renderers) });
                lod.RecalculateBounds();
            }
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

        static bool IsBuildingLike(Stage8ActorSpec spec)
        {
            return spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support || spec.Category == ActorArtCategory.Defense;
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
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        static string ToAssetName(string terrainKind)
        {
            var chars = new List<char>();
            for (var i = 0; i < terrainKind.Length; i++)
            {
                var c = terrainKind[i];
                if (char.IsUpper(c) && i > 0)
                    chars.Add('_');
                chars.Add(char.ToLowerInvariant(c));
            }
            return new string(chars.ToArray());
        }

        sealed class MaterialSet
        {
            public Material GrassDirt;
            public Material CompactedBase;
            public Material ConcretePad;
            public Material RoadPath;
            public Material ResourceField;
            public Material RockBlocked;
            public Material Water;
            public Material FogExplored;
            public Material FoundationEdge;
            public Material WornMetal;
            public Material DarkMetal;
            public Material WarmLight;
            public Material CautionPaint;
            public Material ResourceCrystal;
            public Material UnitArmor;
            public Material InfantryCloth;
            public Material FineGridGuide;
        }
    }

    public sealed class Stage29AssetSummary
    {
        public int TerrainProfileCount;
        public int ProxyDetailCount;
    }
}
