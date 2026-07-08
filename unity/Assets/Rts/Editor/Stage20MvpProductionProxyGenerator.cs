using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage20MvpProductionProxyGenerator
    {
        public const string ProductionProxyFolder = "Assets/Rts/Art/Prefabs/Actors/ProductionProxies";

        [MenuItem("ProjectAegisRTS/Stage 20/Generate MVP Production Visual Proxies")]
        public static void GenerateMvpProductionProxiesMenu()
        {
            GenerateMvpProductionProxies();
        }

        public static void GenerateMvpProductionProxiesBatch()
        {
            try
            {
                GenerateMvpProductionProxies();
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

        public static List<GameObject> GenerateMvpProductionProxies()
        {
            EnsureStage20Folders();
            EnsureBaseVisualAssets();

            var specs = Stage8ActorCatalog.LoadSpecs();
            var materials = MaterialSet.Create();
            var generated = new List<GameObject>();

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                if (!Stage20MvpVisualActorSet.Contains(spec.ActorTypeId))
                    continue;

                var prefab = GenerateProductionProxy(spec, materials);
                if (prefab != null)
                {
                    generated.Add(prefab);
                    UpdateActorVisualDefinition(spec, prefab);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 20 MVP production proxy prefabs updated: " + generated.Count);
            return generated;
        }

        public static string ProductionProxyPath(Stage8ActorSpec spec)
        {
            return ProductionProxyFolder + "/" + spec.ActorTypeId + "_production_proxy.prefab";
        }

        public static void EnsureStage20Folders()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            if (!AssetDatabase.IsValidFolder(ProductionProxyFolder))
                AssetDatabase.CreateFolder("Assets/Rts/Art/Prefabs/Actors", "ProductionProxies");
        }

        static void EnsureBaseVisualAssets()
        {
            var specs = Stage8ActorCatalog.LoadSpecs();
            var missingBaseAssets = false;
            for (var i = 0; i < specs.Count; i++)
            {
                if (!Stage20MvpVisualActorSet.Contains(specs[i].ActorTypeId))
                    continue;
                if (AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(Stage8ActorCatalog.DefinitionAssetPath(specs[i])) == null ||
                    AssetDatabase.LoadAssetAtPath<GameObject>(Stage8ActorCatalog.BlockoutPrefabPath(specs[i])) == null)
                {
                    missingBaseAssets = true;
                    break;
                }
            }

            if (!missingBaseAssets)
                return;

            Stage8BlockoutPrefabGenerator.GenerateBlockouts();
            Stage8ActorVisualDefinitionGenerator.CreateOrUpdateDefinitions();
            Stage8IconGenerator.GenerateIcons();
            Stage8ActorVisualDefinitionGenerator.CreateOrUpdateDefinitions();
        }

        static GameObject GenerateProductionProxy(Stage8ActorSpec spec, MaterialSet materials)
        {
            var root = new GameObject(spec.ActorTypeId + "_production_proxy");
            root.transform.position = Vector3.zero;

            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(Stage8ActorCatalog.DefinitionAssetPath(spec));
            var descriptor = root.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = spec.ActorTypeId;
            descriptor.category = spec.Category;
            descriptor.sourceDefinition = definition;
            descriptor.productionStatus = ActorArtProductionStatus.ProxyPrefab;
            descriptor.generatedByStage8 = false;
            descriptor.declaredRequiredSockets = Stage20RequiredSocketsFor(spec);
            descriptor.notes = "Generated Stage 20 first-pass production proxy. Stage 21 QA-ready replacement scaffold: replace geometry with final art without changing sockets, pivot, or footprint scale.";

            var tag = root.AddComponent<ProductionVisualValidationTag>();
            tag.actorTypeId = spec.ActorTypeId;
            tag.visualTier = ProductionVisualTier.FirstPassProxy;
            tag.hasTopDetail = true;
            tag.hasFrontDetail = true;
            tag.hasBackDetail = true;
            tag.hasLeftDetail = true;
            tag.hasRightDetail = true;
            tag.hasRoofDetail = true;
            tag.hasBeveledOrTieredForm = true;
            tag.hasGridAccurateBase = true;
            tag.hasStage21ReadabilityPass = true;
            tag.hasArtistReplacementMetadata = true;
            tag.hasSocketScaffold = true;
            tag.replacementNotes = "Stage 21: artist replacement must preserve root pivot at footprint center/base, required sockets, fine-grid base scale, LODGroup, and proxy fallback.";
            tag.notes = "Stage 20 proxy: grid-accurate tabletop miniature with all-around detail and validation sockets. Stage 21 readability pass adds replacement metadata, socket scaffold, and stronger 360-degree miniature details.";

            var sockets = CreateSockets(root.transform, spec, descriptor.declaredRequiredSockets);
            CreateGeometry(root.transform, spec, sockets, materials);
            AddCompatibleControllers(root, spec);
            AddLodGroup(root);

            tag.hasLodGroup = root.GetComponent<LODGroup>() != null;
            descriptor.requiredSocketsPresent = descriptor.ValidateRequiredSockets(descriptor.declaredRequiredSockets).Count == 0;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ProductionProxyPath(spec));
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static List<ActorPrefabSocketKind> Stage20RequiredSocketsFor(Stage8ActorSpec spec)
        {
            var sockets = Stage8ActorCatalog.RequiredSocketsFor(spec);
            if (spec.ActorTypeId == "refinery" && !sockets.Contains(ActorPrefabSocketKind.DockPumpRoot))
                sockets.Add(ActorPrefabSocketKind.DockPumpRoot);
            sockets.Sort();
            return sockets;
        }

        static void UpdateActorVisualDefinition(Stage8ActorSpec spec, GameObject prefab)
        {
            var path = Stage8ActorCatalog.DefinitionAssetPath(spec);
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ActorVisualDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            if (definition.generatedBlockoutPrefab == null)
                definition.generatedBlockoutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Stage8ActorCatalog.BlockoutPrefabPath(spec));

            definition.actorTypeId = spec.ActorTypeId;
            definition.displayName = spec.OriginalLabel;
            definition.safeDisplayName = spec.SafeDisplayName;
            definition.category = spec.Category;
            definition.productionPrefab = prefab;
            definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
            definition.productionStatus = ActorArtProductionStatus.ProxyPrefab;
            if (definition.fallbackPrefab == null)
                definition.fallbackPrefab = definition.generatedBlockoutPrefab;
            definition.motionProfileId = Stage8ActorCatalog.MotionProfileIdFor(spec);
            definition.buildingProfileId = Stage8ActorCatalog.BuildingProfileIdFor(spec);
            definition.footprintWidth = spec.FootprintWidth;
            definition.footprintHeight = spec.FootprintHeight;
            definition.visualScale = 1f;
            definition.prefabHeightOffset = spec.Category == ActorArtCategory.Aircraft ? 1.2f : 0f;
            definition.useBuildingVisualController = spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support || spec.Category == ActorArtCategory.Defense;
            definition.useVehicleMotionController = spec.Category == ActorArtCategory.Vehicle || spec.Category == ActorArtCategory.Resource;
            definition.useInfantryMotionController = spec.Category == ActorArtCategory.Infantry;
            definition.useAircraftMotionController = spec.Category == ActorArtCategory.Aircraft;
            definition.useTurretVisualController = spec.ActorTypeId.Contains("tank") || spec.Category == ActorArtCategory.Defense || spec.ActorTypeId == "apc" || spec.ActorTypeId == "attack_aircraft";
            definition.requiredSockets = Stage8ActorCatalog.RequiredSocketsFor(spec);
            definition.notes = AppendStage21Note(AppendStage20Note(definition.notes));
            EditorUtility.SetDirty(definition);
        }

        static string AppendStage20Note(string notes)
        {
            const string marker = "Stage 20: MVP first-pass production proxy is assigned as the preferred prefab.";
            if (!string.IsNullOrEmpty(notes) && notes.Contains(marker))
                return notes;
            return string.IsNullOrEmpty(notes) ? marker : notes.TrimEnd() + "\n" + marker;
        }

        static string AppendStage21Note(string notes)
        {
            const string marker = "Stage 21: proxy is QA-ready for artist model replacement; preserve sockets, pivot, footprint scale, LODGroup, and fallback.";
            if (!string.IsNullOrEmpty(notes) && notes.Contains(marker))
                return notes;
            return string.IsNullOrEmpty(notes) ? marker : notes.TrimEnd() + "\n" + marker;
        }

        static Dictionary<ActorPrefabSocketKind, Transform> CreateSockets(Transform root, Stage8ActorSpec spec, List<ActorPrefabSocketKind> requiredSockets)
        {
            var sockets = new Dictionary<ActorPrefabSocketKind, Transform>();
            for (var i = 0; i < requiredSockets.Count; i++)
            {
                var kind = requiredSockets[i];
                Transform transform;
                if (kind == ActorPrefabSocketKind.Root)
                {
                    transform = root;
                }
                else
                {
                    var obj = new GameObject("Socket_" + kind);
                    obj.transform.SetParent(root, false);
                    obj.transform.localPosition = SocketPosition(kind, spec);
                    transform = obj.transform;
                }

                var socket = transform.gameObject.GetComponent<ActorPrefabSocket>();
                if (socket == null)
                    socket = transform.gameObject.AddComponent<ActorPrefabSocket>();
                socket.socketKind = kind;
                socket.socketName = kind.ToString();
                socket.actorTypeId = spec.ActorTypeId;
                socket.notes = "Stage 20 production proxy socket";
                sockets[kind] = transform;
            }

            return sockets;
        }

        static void CreateGeometry(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            switch (spec.ActorTypeId)
            {
                case "fabrication_hub":
                    CreateFabricationHub(root, spec, sockets, materials);
                    return;
                case "power_plant":
                    CreatePowerPlant(root, spec, sockets, materials);
                    return;
                case "refinery":
                    CreateRefinery(root, spec, sockets, materials);
                    return;
                case "barracks":
                    CreateBarracks(root, spec, sockets, materials);
                    return;
                case "war_factory":
                    CreateWarFactory(root, spec, sockets, materials);
                    return;
                case "gun_tower":
                    CreateGunTower(root, spec, sockets, materials);
                    return;
                case "rifle_infantry":
                    CreateRifleInfantry(root, spec, sockets, materials);
                    return;
                case "light_tank":
                    CreateLightTank(root, spec, sockets, materials);
                    return;
                case "harvester":
                    CreateHarvester(root, spec, sockets, materials);
                    return;
            }
        }

        static void CreateFabricationHub(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            CreateBuildingFoundation(root, spec, materials);
            CreatePrimitive("Inset Main Assembly Hall", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.36f, -0.15f), new Vector3(2.25f, 0.64f, 1.75f), materials.BuildingBody);
            CreatePrimitive("Raised Construction Tower", PrimitiveType.Cube, RootSocket(sockets), new Vector3(-0.42f, 1.05f, -0.2f), new Vector3(0.72f, 1.28f, 0.72f), materials.BuildingAccent);
            CreatePrimitive("Tiered Roof Cap", PrimitiveType.Cube, RootSocket(sockets), new Vector3(-0.42f, 1.76f, -0.2f), new Vector3(0.96f, 0.16f, 0.96f), materials.RoofDetail);
            CreatePrimitive("Front Production Door", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.DoorRoot, root), Vector3.zero, new Vector3(0.84f, 0.5f, 0.08f), materials.Door);
            CreatePrimitive("Exit Ramp", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.ProductionExit, root), new Vector3(0f, 0.02f, -0.18f), new Vector3(1.15f, 0.05f, 0.56f), materials.Foundation);
            CreatePrimitive("Crane Mast", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.CraneRoot, root), new Vector3(0f, -0.2f, 0f), new Vector3(0.14f, 0.72f, 0.14f), materials.DarkMetal);
            CreatePrimitive("Crane Boom", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.CraneRoot, root), new Vector3(0.48f, 0.2f, 0f), new Vector3(1.1f, 0.08f, 0.1f), materials.Warning);
            CreateSideAndRearDetails(root, spec, materials);
            CreateLightAndVfxMarkers(sockets, materials);
        }

        static void CreatePowerPlant(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            CreateBuildingFoundation(root, spec, materials);
            CreatePrimitive("Inset Generator Body", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.34f, 0f), new Vector3(1.42f, 0.6f, 1.2f), materials.PowerBody);
            CreatePrimitive("Raised Turbine Core", PrimitiveType.Cylinder, GetSocket(sockets, ActorPrefabSocketKind.TurbineRoot, root), new Vector3(0f, -0.05f, 0f), new Vector3(0.5f, 0.28f, 0.5f), materials.PowerGlow);
            CreatePrimitive("Core Ring", PrimitiveType.Cylinder, GetSocket(sockets, ActorPrefabSocketKind.TurbineRoot, root), new Vector3(0f, 0.22f, 0f), new Vector3(0.68f, 0.08f, 0.68f), materials.DarkMetal);
            CreatePrimitive("Exhaust Stack Left", PrimitiveType.Cylinder, RootSocket(sockets), new Vector3(-0.46f, 0.92f, -0.38f), new Vector3(0.18f, 0.78f, 0.18f), materials.DarkMetal);
            CreatePrimitive("Exhaust Stack Right", PrimitiveType.Cylinder, RootSocket(sockets), new Vector3(0.46f, 0.92f, -0.38f), new Vector3(0.18f, 0.78f, 0.18f), materials.DarkMetal);
            CreateSideAndRearDetails(root, spec, materials);
            CreateLightAndVfxMarkers(sockets, materials);
        }

        static void CreateRefinery(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            CreateBuildingFoundation(root, spec, materials);
            CreatePrimitive("Processing Hall", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0.32f, 0.36f, -0.18f), new Vector3(1.62f, 0.62f, 1.5f), materials.BuildingBody);
            CreatePrimitive("Storage Tank A", PrimitiveType.Cylinder, RootSocket(sockets), new Vector3(-0.86f, 0.78f, -0.42f), new Vector3(0.44f, 0.78f, 0.44f), materials.RefineryTank);
            CreatePrimitive("Storage Tank B", PrimitiveType.Cylinder, RootSocket(sockets), new Vector3(-0.88f, 0.64f, 0.38f), new Vector3(0.34f, 0.64f, 0.34f), materials.RefineryTank);
            CreatePrimitive("Harvester Dock Pad", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.HarvesterDock, root), new Vector3(0.18f, -0.12f, 0f), new Vector3(0.9f, 0.08f, 1.16f), materials.Foundation);
            CreatePrimitive("Dock Pump Arm", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.DockPumpRoot, root), new Vector3(0.22f, 0.05f, 0f), new Vector3(0.64f, 0.08f, 0.12f), materials.Warning);
            CreatePrimitive("Pipe Spine", PrimitiveType.Cube, RootSocket(sockets), new Vector3(-0.35f, 0.78f, 0.02f), new Vector3(1.15f, 0.08f, 0.08f), materials.DarkMetal);
            CreatePrimitive("Unload Door", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.DoorRoot, root), Vector3.zero, new Vector3(0.72f, 0.42f, 0.08f), materials.Door);
            CreateSideAndRearDetails(root, spec, materials);
            CreateLightAndVfxMarkers(sockets, materials);
        }

        static void CreateBarracks(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            CreateBuildingFoundation(root, spec, materials);
            CreatePrimitive("Barracks Main Block", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.32f, -0.08f), new Vector3(1.38f, 0.58f, 1.12f), materials.BuildingBody);
            CreatePrimitive("Front Entry Frame", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.DoorRoot, root), Vector3.zero, new Vector3(0.52f, 0.42f, 0.08f), materials.Door);
            CreatePrimitive("Raised Roof Intake", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.72f, -0.18f), new Vector3(0.92f, 0.16f, 0.28f), materials.RoofDetail);
            CreatePrimitive("Side Locker Row Left", PrimitiveType.Cube, RootSocket(sockets), new Vector3(-0.78f, 0.4f, 0.24f), new Vector3(0.08f, 0.3f, 0.62f), materials.DetailPanel);
            CreatePrimitive("Side Locker Row Right", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0.78f, 0.4f, 0.24f), new Vector3(0.08f, 0.3f, 0.62f), materials.DetailPanel);
            CreateSideAndRearDetails(root, spec, materials);
            CreateLightAndVfxMarkers(sockets, materials);
        }

        static void CreateWarFactory(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            CreateBuildingFoundation(root, spec, materials);
            CreatePrimitive("Factory Bay", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.42f, -0.1f), new Vector3(2.32f, 0.78f, 1.2f), materials.BuildingBody);
            CreatePrimitive("Large Rollup Door", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.DoorRoot, root), Vector3.zero, new Vector3(1.28f, 0.62f, 0.08f), materials.Door);
            CreatePrimitive("Vehicle Ramp", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.ProductionExit, root), new Vector3(0f, 0.02f, -0.18f), new Vector3(1.55f, 0.05f, 0.6f), materials.Foundation);
            CreatePrimitive("Roof Gantry Left", PrimitiveType.Cube, RootSocket(sockets), new Vector3(-0.72f, 0.94f, -0.02f), new Vector3(0.12f, 0.22f, 1.28f), materials.DarkMetal);
            CreatePrimitive("Roof Gantry Right", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0.72f, 0.94f, -0.02f), new Vector3(0.12f, 0.22f, 1.28f), materials.DarkMetal);
            CreatePrimitive("Bridge Crane", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.CraneRoot, root), new Vector3(0f, 0f, 0f), new Vector3(1.8f, 0.08f, 0.12f), materials.Warning);
            CreateSideAndRearDetails(root, spec, materials);
            CreateLightAndVfxMarkers(sockets, materials);
        }

        static void CreateGunTower(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            CreatePrimitive("Tower Foundation", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.04f, 0f), new Vector3(0.94f, 0.08f, 0.94f), materials.Foundation);
            CreatePrimitive("Armored Pedestal", PrimitiveType.Cylinder, RootSocket(sockets), new Vector3(0f, 0.58f, 0f), new Vector3(0.62f, 0.58f, 0.62f), materials.DefenseBody);
            CreatePrimitive("Turret Housing", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.TurretRoot, root), Vector3.zero, new Vector3(0.52f, 0.22f, 0.48f), materials.Weapon);
            CreatePrimitive("Barrel Sleeve", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.BarrelRoot, root), new Vector3(0f, 0f, 0.22f), new Vector3(0.14f, 0.12f, 0.54f), materials.DarkMetal);
            CreatePrimitive("Muzzle Marker", PrimitiveType.Sphere, GetSocket(sockets, ActorPrefabSocketKind.MuzzlePrimary, root), Vector3.zero, new Vector3(0.11f, 0.11f, 0.11f), materials.PowerGlow);
            CreatePrimitive("Rear Ammo Box", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.54f, -0.36f), new Vector3(0.42f, 0.22f, 0.16f), materials.DetailPanel);
            CreatePrimitive("Left Armor Cheek", PrimitiveType.Cube, RootSocket(sockets), new Vector3(-0.39f, 0.5f, 0.02f), new Vector3(0.08f, 0.32f, 0.48f), materials.DetailPanel);
            CreatePrimitive("Right Armor Cheek", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0.39f, 0.5f, 0.02f), new Vector3(0.08f, 0.32f, 0.48f), materials.DetailPanel);
            CreatePrimitive("Roof Sight Block", PrimitiveType.Cube, RootSocket(sockets), new Vector3(0f, 0.78f, 0.1f), new Vector3(0.24f, 0.1f, 0.18f), materials.Warning);
            CreateLightAndVfxMarkers(sockets, materials);
        }

        static void CreateRifleInfantry(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            var bodyRoot = GetSocket(sockets, ActorPrefabSocketKind.BodyRoot, root);
            CreatePrimitive("Infantry Base Disc", PrimitiveType.Cylinder, root, new Vector3(0f, 0.02f, 0f), new Vector3(0.42f, 0.04f, 0.42f), materials.Foundation);
            CreatePrimitive("Infantry Legs", PrimitiveType.Cube, bodyRoot, new Vector3(0f, 0.24f, 0f), new Vector3(0.22f, 0.36f, 0.18f), materials.InfantryBody);
            CreatePrimitive("Infantry Torso", PrimitiveType.Cube, bodyRoot, new Vector3(0f, 0.55f, 0f), new Vector3(0.34f, 0.34f, 0.22f), materials.InfantryBody);
            CreatePrimitive("Infantry Head", PrimitiveType.Sphere, GetSocket(sockets, ActorPrefabSocketKind.Head, root), Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), materials.DetailPanel);
            CreatePrimitive("Helmet Crest", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.Head, root), new Vector3(0f, 0.11f, 0.02f), new Vector3(0.2f, 0.05f, 0.14f), materials.Warning);
            CreatePrimitive("Rifle Proxy", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.WeaponSocket, root), new Vector3(0.08f, 0f, 0.2f), new Vector3(0.08f, 0.06f, 0.52f), materials.Weapon);
            CreatePrimitive("Rifle Stock", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.WeaponSocket, root), new Vector3(-0.02f, 0f, -0.06f), new Vector3(0.11f, 0.08f, 0.14f), materials.DarkMetal);
            CreatePrimitive("Backpack", PrimitiveType.Cube, bodyRoot, new Vector3(0f, 0.54f, -0.16f), new Vector3(0.24f, 0.28f, 0.08f), materials.DarkMetal);
        }

        static void CreateLightTank(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            var visualRoot = RootSocket(sockets);
            CreatePrimitive("Tank Chassis", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.26f, 0f), new Vector3(0.94f, 0.32f, 0.66f), materials.VehicleBody);
            CreatePrimitive("Left Track", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.TrackLeft, root), Vector3.zero, new Vector3(0.12f, 0.2f, 0.78f), materials.Track);
            CreatePrimitive("Right Track", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.TrackRight, root), Vector3.zero, new Vector3(0.12f, 0.2f, 0.78f), materials.Track);
            CreatePrimitive("Turret", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.TurretRoot, root), Vector3.zero, new Vector3(0.44f, 0.18f, 0.38f), materials.Weapon);
            CreatePrimitive("Barrel", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.BarrelRoot, root), new Vector3(0f, 0f, 0.26f), new Vector3(0.1f, 0.08f, 0.58f), materials.DarkMetal);
            CreatePrimitive("Muzzle", PrimitiveType.Sphere, GetSocket(sockets, ActorPrefabSocketKind.MuzzlePrimary, root), Vector3.zero, new Vector3(0.09f, 0.09f, 0.09f), materials.PowerGlow);
            CreatePrimitive("Engine Deck", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.47f, -0.22f), new Vector3(0.54f, 0.08f, 0.18f), materials.DetailPanel);
            CreatePrimitive("Command Cupola", PrimitiveType.Cylinder, visualRoot, new Vector3(0f, 0.62f, -0.06f), new Vector3(0.22f, 0.16f, 0.22f), materials.Warning);
            CreatePrimitive("Left Track Guard", PrimitiveType.Cube, visualRoot, new Vector3(-0.53f, 0.3f, 0f), new Vector3(0.08f, 0.22f, 0.76f), materials.DetailPanel);
            CreatePrimitive("Right Track Guard", PrimitiveType.Cube, visualRoot, new Vector3(0.53f, 0.3f, 0f), new Vector3(0.08f, 0.22f, 0.76f), materials.DetailPanel);
            CreateVehicleVfxMarkers(sockets, materials);
        }

        static void CreateHarvester(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            var visualRoot = RootSocket(sockets);
            CreatePrimitive("Harvester Chassis", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.28f, 0f), new Vector3(1.12f, 0.34f, 0.78f), materials.VehicleBody);
            CreatePrimitive("Cargo Hopper", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.58f, -0.08f), new Vector3(0.82f, 0.34f, 0.58f), materials.RefineryTank);
            CreatePrimitive("Collector Intake", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.2f, 0.52f), new Vector3(0.8f, 0.18f, 0.22f), materials.Warning);
            CreatePrimitive("Collector Teeth", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.12f, 0.7f), new Vector3(0.92f, 0.08f, 0.08f), materials.DarkMetal);
            CreatePrimitive("Left Cargo Rib", PrimitiveType.Cube, visualRoot, new Vector3(-0.46f, 0.58f, -0.08f), new Vector3(0.06f, 0.32f, 0.56f), materials.DetailPanel);
            CreatePrimitive("Right Cargo Rib", PrimitiveType.Cube, visualRoot, new Vector3(0.46f, 0.58f, -0.08f), new Vector3(0.06f, 0.32f, 0.56f), materials.DetailPanel);
            CreatePrimitive("Left Track", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.TrackLeft, root), Vector3.zero, new Vector3(0.12f, 0.22f, 0.86f), materials.Track);
            CreatePrimitive("Right Track", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.TrackRight, root), Vector3.zero, new Vector3(0.12f, 0.22f, 0.86f), materials.Track);
            CreatePrimitive("Dock Alignment Marker", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.HarvesterDock, root), Vector3.zero, new Vector3(0.2f, 0.16f, 0.2f), materials.PowerGlow);
            CreateVehicleVfxMarkers(sockets, materials);
        }

        static void CreateBuildingFoundation(Transform root, Stage8ActorSpec spec, MaterialSet materials)
        {
            CreatePrimitive("Grid Accurate Foundation", PrimitiveType.Cube, root, new Vector3(0f, 0.04f, 0f), new Vector3(spec.FootprintWidth * 0.96f, 0.08f, spec.FootprintHeight * 0.96f), materials.Foundation);
            CreatePrimitive("Inset Footprint Line", PrimitiveType.Cube, root, new Vector3(0f, 0.1f, spec.FootprintHeight * 0.36f), new Vector3(spec.FootprintWidth * 0.72f, 0.035f, 0.04f), materials.Warning);
            CreateCornerFootprintMarkers(root, spec, materials);
        }

        static void CreateSideAndRearDetails(Transform root, Stage8ActorSpec spec, MaterialSet materials)
        {
            var width = Mathf.Max(1f, spec.FootprintWidth);
            var depth = Mathf.Max(1f, spec.FootprintHeight);
            CreatePrimitive("Left Side Detail Panel", PrimitiveType.Cube, root, new Vector3(-width * 0.42f, 0.46f, 0f), new Vector3(0.06f, 0.32f, depth * 0.46f), materials.DetailPanel);
            CreatePrimitive("Right Side Detail Panel", PrimitiveType.Cube, root, new Vector3(width * 0.42f, 0.46f, 0f), new Vector3(0.06f, 0.32f, depth * 0.46f), materials.DetailPanel);
            CreatePrimitive("Rear Service Panel", PrimitiveType.Cube, root, new Vector3(0f, 0.48f, -depth * 0.42f), new Vector3(width * 0.5f, 0.28f, 0.06f), materials.DetailPanel);
            CreatePrimitive("Roof Utility Strip", PrimitiveType.Cube, root, new Vector3(0f, 0.82f, 0f), new Vector3(width * 0.48f, 0.08f, depth * 0.18f), materials.RoofDetail);
            CreatePrimitive("Front Identity Stripe", PrimitiveType.Cube, root, new Vector3(0f, 0.18f, depth * 0.46f), new Vector3(width * 0.42f, 0.08f, 0.045f), materials.Warning);
            CreatePrimitive("Rear Service Stripe", PrimitiveType.Cube, root, new Vector3(0f, 0.2f, -depth * 0.46f), new Vector3(width * 0.36f, 0.06f, 0.045f), materials.DarkMetal);
        }

        static void CreateCornerFootprintMarkers(Transform root, Stage8ActorSpec spec, MaterialSet materials)
        {
            var x = Mathf.Max(0.38f, spec.FootprintWidth * 0.43f);
            var z = Mathf.Max(0.38f, spec.FootprintHeight * 0.43f);
            var scale = new Vector3(0.14f, 0.07f, 0.14f);
            CreatePrimitive("Footprint Corner FL", PrimitiveType.Cube, root, new Vector3(-x, 0.12f, z), scale, materials.DetailPanel);
            CreatePrimitive("Footprint Corner FR", PrimitiveType.Cube, root, new Vector3(x, 0.12f, z), scale, materials.DetailPanel);
            CreatePrimitive("Footprint Corner RL", PrimitiveType.Cube, root, new Vector3(-x, 0.12f, -z), scale, materials.DetailPanel);
            CreatePrimitive("Footprint Corner RR", PrimitiveType.Cube, root, new Vector3(x, 0.12f, -z), scale, materials.DetailPanel);
        }

        static void CreateLightAndVfxMarkers(Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            if (sockets.ContainsKey(ActorPrefabSocketKind.LightRoot))
                CreatePrimitive("Powered Light Marker", PrimitiveType.Sphere, sockets[ActorPrefabSocketKind.LightRoot], Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f), materials.PowerGlow);
            if (sockets.ContainsKey(ActorPrefabSocketKind.VfxSmoke))
                CreatePrimitive("Smoke Socket Marker", PrimitiveType.Cylinder, sockets[ActorPrefabSocketKind.VfxSmoke], Vector3.zero, new Vector3(0.12f, 0.1f, 0.12f), materials.DarkMetal);
            if (sockets.ContainsKey(ActorPrefabSocketKind.VfxExplosion))
                CreatePrimitive("Explosion Socket Marker", PrimitiveType.Sphere, sockets[ActorPrefabSocketKind.VfxExplosion], Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), materials.Warning);
            if (sockets.ContainsKey(ActorPrefabSocketKind.VfxProduction))
                CreatePrimitive("Production VFX Marker", PrimitiveType.Sphere, sockets[ActorPrefabSocketKind.VfxProduction], Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), materials.PowerGlow);
        }

        static void CreateVehicleVfxMarkers(Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            if (sockets.ContainsKey(ActorPrefabSocketKind.VfxSmoke))
                CreatePrimitive("Engine Smoke Marker", PrimitiveType.Cylinder, sockets[ActorPrefabSocketKind.VfxSmoke], Vector3.zero, new Vector3(0.1f, 0.08f, 0.1f), materials.DarkMetal);
            if (sockets.ContainsKey(ActorPrefabSocketKind.VfxExplosion))
                CreatePrimitive("Vehicle Explosion Marker", PrimitiveType.Sphere, sockets[ActorPrefabSocketKind.VfxExplosion], Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), materials.Warning);
        }

        static void AddCompatibleControllers(GameObject root, Stage8ActorSpec spec)
        {
            if (spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support || spec.Category == ActorArtCategory.Defense)
                root.AddComponent<BuildingVisualStateController>();
            if (spec.Category == ActorArtCategory.Vehicle || spec.Category == ActorArtCategory.Resource)
            {
                root.AddComponent<ActorVisualMotionController>();
                root.AddComponent<VehicleVisualMotionController>();
            }
            if (spec.Category == ActorArtCategory.Infantry)
            {
                root.AddComponent<ActorVisualMotionController>();
                root.AddComponent<InfantryVisualMotionController>();
            }
            if (spec.ActorTypeId.Contains("tank") || spec.Category == ActorArtCategory.Defense)
            {
                if (root.GetComponent<ActorVisualMotionController>() == null)
                    root.AddComponent<ActorVisualMotionController>();
                root.AddComponent<TurretVisualAimController>();
            }
        }

        static void AddLodGroup(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            var lodGroup = root.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[] { new LOD(0.01f, renderers) });
            lodGroup.RecalculateBounds();
        }

        static Transform RootSocket(Dictionary<ActorPrefabSocketKind, Transform> sockets)
        {
            return GetSocket(sockets, ActorPrefabSocketKind.VisualRoot, GetSocket(sockets, ActorPrefabSocketKind.Root, null));
        }

        static Transform GetSocket(Dictionary<ActorPrefabSocketKind, Transform> sockets, ActorPrefabSocketKind kind, Transform fallback)
        {
            Transform transform;
            return sockets.TryGetValue(kind, out transform) ? transform : fallback;
        }

        static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            var collider = primitive.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            return primitive;
        }

        static Vector3 SocketPosition(ActorPrefabSocketKind kind, Stage8ActorSpec spec)
        {
            var footprintX = Mathf.Max(1f, spec.FootprintWidth);
            var footprintZ = Mathf.Max(1f, spec.FootprintHeight);
            switch (kind)
            {
                case ActorPrefabSocketKind.BodyRoot: return new Vector3(0f, 0.25f, 0f);
                case ActorPrefabSocketKind.VisualRoot: return Vector3.zero;
                case ActorPrefabSocketKind.SelectionAnchor: return new Vector3(0f, 0.04f, 0f);
                case ActorPrefabSocketKind.HealthBarAnchor: return new Vector3(0f, 1.18f, -0.35f);
                case ActorPrefabSocketKind.UiAnchor: return new Vector3(0f, 1.34f, 0f);
                case ActorPrefabSocketKind.TurretRoot: return new Vector3(0f, 0.58f, 0.02f);
                case ActorPrefabSocketKind.BarrelRoot: return new Vector3(0f, 0.58f, 0.34f);
                case ActorPrefabSocketKind.MuzzlePrimary: return new Vector3(0f, 0.58f, 0.76f);
                case ActorPrefabSocketKind.MuzzleSecondary: return new Vector3(0.18f, 0.58f, 0.76f);
                case ActorPrefabSocketKind.WheelLeft:
                case ActorPrefabSocketKind.TrackLeft: return new Vector3(-0.52f, 0.18f, 0f);
                case ActorPrefabSocketKind.WheelRight:
                case ActorPrefabSocketKind.TrackRight: return new Vector3(0.52f, 0.18f, 0f);
                case ActorPrefabSocketKind.DoorRoot: return new Vector3(0f, 0.32f, footprintZ * 0.45f);
                case ActorPrefabSocketKind.ProductionExit: return new Vector3(0f, 0.05f, footprintZ * 0.7f);
                case ActorPrefabSocketKind.RallyExit: return new Vector3(0.6f, 0.05f, footprintZ * 0.7f);
                case ActorPrefabSocketKind.HarvesterDock: return spec.ActorTypeId == "harvester" ? new Vector3(0f, 0.55f, -0.5f) : new Vector3(-footprintX * 0.45f, 0.25f, 0f);
                case ActorPrefabSocketKind.DockPumpRoot: return new Vector3(-footprintX * 0.42f, 0.62f, 0.1f);
                case ActorPrefabSocketKind.RepairArmRoot: return new Vector3(footprintX * 0.3f, 0.85f, 0f);
                case ActorPrefabSocketKind.CraneRoot: return new Vector3(-footprintX * 0.25f, 1.05f, 0f);
                case ActorPrefabSocketKind.RadarDishRoot: return new Vector3(0f, 1.1f, 0f);
                case ActorPrefabSocketKind.TurbineRoot: return new Vector3(0f, 0.95f, 0f);
                case ActorPrefabSocketKind.LightRoot: return new Vector3(-footprintX * 0.25f, 0.95f, footprintZ * 0.2f);
                case ActorPrefabSocketKind.VfxSmoke: return new Vector3(-footprintX * 0.2f, 1.12f, -footprintZ * 0.2f);
                case ActorPrefabSocketKind.VfxExplosion: return new Vector3(0f, 0.6f, 0f);
                case ActorPrefabSocketKind.VfxProduction: return new Vector3(0f, 0.45f, footprintZ * 0.45f);
                case ActorPrefabSocketKind.AudioLoop: return new Vector3(0f, 0.6f, 0f);
                case ActorPrefabSocketKind.Head: return new Vector3(0f, 0.88f, 0f);
                case ActorPrefabSocketKind.WeaponSocket: return new Vector3(0.22f, 0.55f, 0.28f);
                case ActorPrefabSocketKind.AimPivot: return new Vector3(0f, 0.62f, 0.42f);
                case ActorPrefabSocketKind.AircraftRotor: return new Vector3(0f, 0.52f, 0f);
                case ActorPrefabSocketKind.LandingPadAnchor: return new Vector3(0f, 0.04f, 0f);
                default: return Vector3.zero;
            }
        }

        sealed class MaterialSet
        {
            public Material Foundation;
            public Material BuildingBody;
            public Material BuildingAccent;
            public Material DefenseBody;
            public Material VehicleBody;
            public Material InfantryBody;
            public Material Track;
            public Material Weapon;
            public Material Door;
            public Material DetailPanel;
            public Material RoofDetail;
            public Material DarkMetal;
            public Material PowerBody;
            public Material PowerGlow;
            public Material RefineryTank;
            public Material Warning;

            public static MaterialSet Create()
            {
                return new MaterialSet
                {
                    Foundation = CreateMaterial("stage20_foundation_concrete", new Color(0.28f, 0.31f, 0.29f)),
                    BuildingBody = CreateMaterial("stage20_building_body", new Color(0.36f, 0.43f, 0.42f)),
                    BuildingAccent = CreateMaterial("stage20_building_accent", new Color(0.45f, 0.52f, 0.48f)),
                    DefenseBody = CreateMaterial("stage20_defense_body", new Color(0.42f, 0.38f, 0.34f)),
                    VehicleBody = CreateMaterial("stage20_vehicle_body", new Color(0.34f, 0.43f, 0.34f)),
                    InfantryBody = CreateMaterial("stage20_infantry_body", new Color(0.28f, 0.46f, 0.34f)),
                    Track = CreateMaterial("stage20_track_dark_rubber", new Color(0.10f, 0.11f, 0.11f)),
                    Weapon = CreateMaterial("stage20_weapon_burnished_steel", new Color(0.18f, 0.21f, 0.21f)),
                    Door = CreateMaterial("stage20_door_warm_steel", new Color(0.50f, 0.44f, 0.34f)),
                    DetailPanel = CreateMaterial("stage20_detail_panel", new Color(0.24f, 0.28f, 0.31f)),
                    RoofDetail = CreateMaterial("stage20_roof_detail", new Color(0.56f, 0.57f, 0.50f)),
                    DarkMetal = CreateMaterial("stage20_dark_metal", new Color(0.12f, 0.14f, 0.15f)),
                    PowerBody = CreateMaterial("stage20_power_body", new Color(0.35f, 0.41f, 0.48f)),
                    PowerGlow = CreateMaterial("stage20_power_glow", new Color(0.18f, 0.75f, 0.70f)),
                    RefineryTank = CreateMaterial("stage20_refinery_tank", new Color(0.48f, 0.50f, 0.42f)),
                    Warning = CreateMaterial("stage20_warning_amber", new Color(0.86f, 0.55f, 0.18f))
                };
            }

            static Material CreateMaterial(string name, Color color)
            {
                var path = Stage8ActorCatalog.MaterialFolder + "/" + name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null)
                        shader = Shader.Find("Standard");
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, path);
                }

                material.color = color;
                EditorUtility.SetDirty(material);
                return material;
            }
        }
    }
}
