#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class ProjectAegisWheeledVehiclePacketBuilder
    {
        sealed class VehicleSpec
        {
            public string UnitId;
            public string DisplayName;
            public string MotionProfileId;
            public string SourceFolder;
            public string TextureFolder;
            public string MaterialFolder;
            public string IconPath;
            public string PrefabFolder;
            public string PrefabPath;
            public string DefinitionPath;
            public string ReviewScenePath;
            public Vector3 BodyRoot;
            public Vector3 TurretRoot;
            public Vector3 BarrelRoot;
            public Vector3 MuzzlePrimary;
            public Vector3 WheelLeft;
            public Vector3 WheelRight;
            public Vector3 SelectionAnchor;
            public Vector3 HealthBarAnchor;
            public Vector3 UiAnchor;
            public Vector3 VfxSmoke;
            public Vector3 VfxExplosion;
            public Vector3 LightRoot;
            public Vector3 AimPivot;
            public bool HasRearRamp;
            public Vector3 RearRamp;
            public Vector3 TransportExit;
            public Vector3 ColliderCenter;
            public Vector3 ColliderSize;
            public float SelectionRadius;
            public float SelectionHeight;
            public float VisualScale;
            public float WheelRotationDegreesPerMeter;
            public float BarrelRecoilDistance;
            public int ExpectedWheelCountPerSide;
            public int MinimumVertices;
            public Color BodyColor;
            public Color TeamColor;
            public Color EmissiveColor;
            public Color GlassColor;
        }

        static readonly VehicleSpec ScoutRover = new VehicleSpec
        {
            UnitId = "scout_rover",
            DisplayName = "Scout Rover",
            MotionProfileId = "wheeled_scout",
            SourceFolder = "Assets/Rts/Art/Source/Units/scout_rover",
            TextureFolder = "Assets/Rts/Art/Textures/Units/scout_rover",
            MaterialFolder = "Assets/Rts/Art/Materials/Units/scout_rover",
            IconPath = "Assets/Rts/Art/Icons/scout_rover_icon.png",
            PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover",
            PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover/scout_rover.prefab",
            DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/scout_rover_visual.asset",
            ReviewScenePath = "Assets/Rts/Scenes/scout_rover_UnitReview.unity",
            BodyRoot = new Vector3(0.00f, 0.0308f, -0.18f),
            TurretRoot = new Vector3(0.00f, 2.2108f, -0.03f),
            BarrelRoot = new Vector3(0.00f, 2.4908f, 0.42f),
            MuzzlePrimary = new Vector3(0.00f, 2.4908f, 1.24f),
            WheelLeft = new Vector3(-1.34f, 0.6008f, -0.14f),
            WheelRight = new Vector3(1.34f, 0.6008f, -0.14f),
            SelectionAnchor = new Vector3(0.00f, 0.0808f, -0.18f),
            HealthBarAnchor = new Vector3(0.00f, 3.1108f, -0.30f),
            UiAnchor = new Vector3(0.00f, 3.3508f, -0.30f),
            VfxSmoke = new Vector3(0.76f, 1.7508f, -1.90f),
            VfxExplosion = new Vector3(0.00f, 1.0808f, -0.18f),
            LightRoot = new Vector3(0.00f, 0.8608f, 2.18f),
            AimPivot = new Vector3(0.00f, 2.3408f, 0.17f),
            ColliderCenter = new Vector3(0f, 1.35f, 0.02f),
            ColliderSize = new Vector3(4.34f, 2.72f, 4.88f),
            SelectionRadius = 1.95f,
            SelectionHeight = 3.35f,
            VisualScale = 0.34f,
            WheelRotationDegreesPerMeter = 720f,
            BarrelRecoilDistance = 0.05f,
            ExpectedWheelCountPerSide = 2,
            MinimumVertices = 400,
            BodyColor = new Color(0.31f, 0.36f, 0.25f, 1f),
            TeamColor = new Color(0.12f, 0.70f, 0.88f, 1f),
            EmissiveColor = new Color(1.0f, 0.47f, 0.12f, 1f),
            GlassColor = new Color(0.10f, 0.55f, 0.64f, 0.72f)
        };

        static readonly VehicleSpec Apc = new VehicleSpec
        {
            UnitId = "apc",
            DisplayName = "APC",
            MotionProfileId = "wheeled_apc",
            SourceFolder = "Assets/Rts/Art/Source/Units/apc",
            TextureFolder = "Assets/Rts/Art/Textures/Units/apc",
            MaterialFolder = "Assets/Rts/Art/Materials/Units/apc",
            IconPath = "Assets/Rts/Art/Icons/apc_icon.png",
            PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc",
            PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab",
            DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/apc_visual.asset",
            ReviewScenePath = "Assets/Rts/Scenes/apc_UnitReview.unity",
            BodyRoot = new Vector3(0.00f, 0.0552f, 0.1652f),
            TurretRoot = new Vector3(0.00f, 2.5152f, 0.5452f),
            BarrelRoot = new Vector3(0.00f, 2.7952f, 0.8652f),
            MuzzlePrimary = new Vector3(0.00f, 2.7952f, 1.6852f),
            WheelLeft = new Vector3(-1.68f, 0.6752f, 0.2152f),
            WheelRight = new Vector3(1.68f, 0.6752f, 0.2152f),
            SelectionAnchor = new Vector3(0.00f, 0.1052f, 0.1652f),
            HealthBarAnchor = new Vector3(0.00f, 3.2552f, -0.0348f),
            UiAnchor = new Vector3(0.00f, 3.5052f, -0.0348f),
            VfxSmoke = new Vector3(0.96f, 2.2552f, -1.9748f),
            VfxExplosion = new Vector3(0.00f, 1.2552f, 0.1652f),
            LightRoot = new Vector3(0.00f, 0.9952f, 2.8852f),
            AimPivot = new Vector3(0.00f, 2.5752f, 0.6652f),
            HasRearRamp = true,
            RearRamp = new Vector3(0.00f, 0.8152f, -2.9148f),
            TransportExit = new Vector3(0.00f, 0.4052f, -3.3548f),
            ColliderCenter = new Vector3(0f, 1.46f, 0.08f),
            ColliderSize = new Vector3(5.10f, 2.92f, 6.50f),
            SelectionRadius = 2.50f,
            SelectionHeight = 3.50f,
            VisualScale = 0.34f,
            WheelRotationDegreesPerMeter = 640f,
            BarrelRecoilDistance = 0.05f,
            ExpectedWheelCountPerSide = 3,
            MinimumVertices = 500,
            BodyColor = new Color(0.32f, 0.37f, 0.27f, 1f),
            TeamColor = new Color(0.12f, 0.70f, 0.88f, 1f),
            EmissiveColor = new Color(1.0f, 0.48f, 0.10f, 1f),
            GlassColor = new Color(0.11f, 0.58f, 0.66f, 0.74f)
        };

        [MenuItem("ProjectAegisRTS/Art/Build Wheeled Vehicle Packets")]
        public static void BuildAllMenu()
        {
            BuildAll(false);
        }

        public static void BuildAllBatch()
        {
            try
            {
                BuildAll(true);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateAllBatch()
        {
            try
            {
                Validate(ScoutRover);
                Validate(Apc);
                Debug.Log("ProjectAegisRTS wheeled vehicle packets validated with individual wheel spin pivots.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        static void BuildAll(bool batch)
        {
            Build(ScoutRover, true);
            Build(Apc, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate(ScoutRover);
            Validate(Apc);
            Debug.Log("Built ProjectAegisRTS scout_rover and apc packet prefabs with individual wheel spin pivots.");
        }

        static void Build(VehicleSpec spec, bool createReviewScene)
        {
            EnsureFolders(spec);
            ConfigureImports(spec);

            var body = CreateMaterial(spec, "body", spec.BodyColor, false, 0.25f, 0.50f);
            var metal = CreateMaterial(spec, "dark_metal", new Color(0.12f, 0.15f, 0.13f, 1f), false, 0.55f, 0.36f);
            var bolts = CreateMaterial(spec, "bolts", new Color(0.52f, 0.54f, 0.48f, 1f), false, 0.65f, 0.42f);
            var rubber = CreateMaterial(spec, "rubber_wheels", new Color(0.035f, 0.038f, 0.035f, 1f), false, 0.05f, 0.30f);
            var team = CreateMaterial(spec, "team", spec.TeamColor, false, 0.30f, 0.48f);
            var glass = CreateMaterial(spec, "glass", spec.GlassColor, false, 0.05f, 0.78f);
            var emissive = CreateMaterial(spec, "emissive", spec.EmissiveColor, true, 0.05f, 0.72f);
            var weapon = CreateMaterial(spec, "weapon", new Color(0.22f, 0.23f, 0.20f, 1f), false, 0.62f, 0.40f);
            var muzzle = CreateMaterial(spec, "muzzle_flash", new Color(1.0f, 0.60f, 0.12f, 1f), true, 0f, 0.20f);

            var actor = new GameObject(spec.UnitId);
            var descriptor = actor.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = spec.UnitId;
            descriptor.category = ActorArtCategory.Vehicle;
            descriptor.productionStatus = ActorArtProductionStatus.Animated;
            descriptor.generatedByStage8 = false;
            descriptor.requiredSocketsPresent = true;
            descriptor.declaredRequiredSockets.Clear();
            descriptor.declaredRequiredSockets.AddRange(RequiredSockets(spec));
            descriptor.notes = spec.DisplayName + " production visual imported from Ironclad WheelClearV2 packet. WheelLeft/WheelRight are stable sockets; child spin pivots animate each wheel independently.";

            var validationTag = actor.AddComponent<ProductionVisualValidationTag>();
            validationTag.actorTypeId = spec.UnitId;
            validationTag.visualTier = ProductionVisualTier.FinalCandidate;
            validationTag.hasTopDetail = true;
            validationTag.hasFrontDetail = true;
            validationTag.hasBackDetail = true;
            validationTag.hasLeftDetail = true;
            validationTag.hasRightDetail = true;
            validationTag.hasRoofDetail = true;
            validationTag.hasBeveledOrTieredForm = true;
            validationTag.hasGridAccurateBase = true;
            validationTag.hasLodGroup = true;
            validationTag.hasArtistReplacementMetadata = true;
            validationTag.hasSocketScaffold = true;
            validationTag.notes = "Imported unit packet with individual per-wheel spin pivots.";

            var lodGroup = actor.AddComponent<LODGroup>();
            var root = Socket(actor.transform, "Root", ActorPrefabSocketKind.Root, Vector3.zero, spec);
            var visualRoot = Socket(root, "VisualRoot", ActorPrefabSocketKind.VisualRoot, Vector3.zero, spec);
            var bodyRoot = Socket(visualRoot, "BodyRoot", ActorPrefabSocketKind.BodyRoot, spec.BodyRoot, spec);
            var lod0 = Child(bodyRoot, "LOD0_Meshes", Vector3.zero);
            var lod1 = Child(bodyRoot, "LOD1_Meshes", Vector3.zero);
            var lod2 = Child(bodyRoot, "LOD2_Meshes", Vector3.zero);

            InstantiateModel(spec, "HullArmor", "hull.obj", lod0, -spec.BodyRoot, body);
            InstantiateOptionalModel(spec, "DarkPanelDetails", "dark_panel_details.obj", lod0, -spec.BodyRoot, metal);
            InstantiateOptionalModel(spec, "BoltsAndFasteners", "bolts_details.obj", lod0, -spec.BodyRoot, bolts);
            InstantiateOptionalModel(spec, "GlassAndOptics", "glass.obj", lod0, -spec.BodyRoot, glass);
            InstantiateOptionalModel(spec, "EmissiveLights", "emissive.obj", lod0, -spec.BodyRoot, emissive);
            InstantiateOptionalModel(spec, "TeamColorPanels_Body", "team_body.obj", lod0, -spec.BodyRoot, team);

            var wheelLeft = Socket(visualRoot, "WheelLeft", ActorPrefabSocketKind.WheelLeft, spec.WheelLeft, spec);
            var wheelRight = Socket(visualRoot, "WheelRight", ActorPrefabSocketKind.WheelRight, spec.WheelRight, spec);
            var leftWheelRig = CreateIndividualWheelMeshes(spec, "Left", "wheel_left.obj", wheelLeft, spec.WheelLeft, rubber, metal, bolts);
            var rightWheelRig = CreateIndividualWheelMeshes(spec, "Right", "wheel_right.obj", wheelRight, spec.WheelRight, rubber, metal, bolts);

            var turret = Socket(visualRoot, "TurretRoot", ActorPrefabSocketKind.TurretRoot, spec.TurretRoot, spec);
            InstantiateModel(spec, "TurretArmor", "turret.obj", turret, -spec.TurretRoot, body);
            InstantiateOptionalModel(spec, "TeamColorPanels_Turret", "team_turret.obj", turret, -spec.TurretRoot, team);
            var barrel = Socket(turret, "BarrelRoot", ActorPrefabSocketKind.BarrelRoot, spec.BarrelRoot - spec.TurretRoot, spec);
            InstantiateModel(spec, "BarrelMesh", "barrel.obj", barrel, -spec.BarrelRoot, weapon);
            var muzzlePrimary = Socket(barrel, "MuzzlePrimary", ActorPrefabSocketKind.MuzzlePrimary, spec.MuzzlePrimary - spec.BarrelRoot, spec);
            var muzzleFlash = CreateMuzzleFlash(muzzlePrimary, muzzle);

            if (spec.HasRearRamp)
            {
                var ramp = Socket(visualRoot, "RearRamp", ActorPrefabSocketKind.DoorRoot, spec.RearRamp, spec);
                InstantiateOptionalModel(spec, "RearRampMesh", "rear_ramp.obj", ramp, -spec.RearRamp, metal);
                Socket(visualRoot, "TransportExit", ActorPrefabSocketKind.ProductionExit, spec.TransportExit, spec);
            }

            Socket(root, "SelectionAnchor", ActorPrefabSocketKind.SelectionAnchor, spec.SelectionAnchor, spec);
            Socket(root, "HealthBarAnchor", ActorPrefabSocketKind.HealthBarAnchor, spec.HealthBarAnchor, spec);
            Socket(root, "UiAnchor", ActorPrefabSocketKind.UiAnchor, spec.UiAnchor, spec);
            Socket(visualRoot, "VfxSmoke", ActorPrefabSocketKind.VfxSmoke, spec.VfxSmoke, spec);
            Socket(visualRoot, "VfxExplosion", ActorPrefabSocketKind.VfxExplosion, spec.VfxExplosion, spec);
            var lightRoot = Socket(visualRoot, "LightRoot", ActorPrefabSocketKind.LightRoot, spec.LightRoot, spec);
            Socket(visualRoot, "AimPivot", ActorPrefabSocketKind.AimPivot, spec.AimPivot, spec);
            CreateHeadlight(lightRoot, spec.EmissiveColor);

            InstantiateModel(spec, "LOD1_Static", "lod1.obj", lod1, -spec.BodyRoot, body);
            InstantiateModel(spec, "LOD2_Static", "lod2.obj", lod2, -spec.BodyRoot, body);

            var rig = actor.AddComponent<TankVisualRigController>();
            rig.bodyRoot = bodyRoot;
            rig.turretRoot = turret;
            rig.barrelRoot = barrel;
            rig.trackLeftRoot = wheelLeft;
            rig.trackRightRoot = wheelRight;
            rig.wheelLeft = leftWheelRig.WheelPivots.ToArray();
            rig.wheelRight = rightWheelRig.WheelPivots.ToArray();
            rig.trackRenderers = new Renderer[0];
            rig.muzzleFlashRoot = muzzleFlash;
            rig.turretTurnDegreesPerSecond = spec.UnitId == "scout_rover" ? 240f : 175f;
            rig.barrelRecoilDistance = spec.BarrelRecoilDistance;
            rig.wheelRotationDegreesPerMeter = spec.WheelRotationDegreesPerMeter;
            rig.trackScrollUnitsPerMeter = 0f;
            rig.suspensionBobMeters = spec.UnitId == "scout_rover" ? 0.038f : 0.026f;
            rig.suspensionFrequency = spec.UnitId == "scout_rover" ? 6.5f : 4.8f;

            var collider = actor.AddComponent<BoxCollider>();
            collider.center = spec.ColliderCenter;
            collider.size = spec.ColliderSize;

            ConfigureLodGroup(lodGroup, lod0, lod1, lod2);

            var prefab = PrefabUtility.SaveAsPrefabAsset(actor, spec.PrefabPath);
            UnityEngine.Object.DestroyImmediate(actor);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UpdateVisualDefinition(spec, prefab);
            if (createReviewScene)
                CreateReviewScene(spec, prefab);
        }

        static void Validate(VehicleSpec spec)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException(spec.UnitId + ": missing prefab " + spec.PrefabPath);

            var rig = prefab.GetComponent<TankVisualRigController>();
            if (rig == null)
                throw new InvalidOperationException(spec.UnitId + ": missing TankVisualRigController.");
            ValidateWheelSide(spec, "left", "WheelLeft", rig.wheelLeft);
            ValidateWheelSide(spec, "right", "WheelRight", rig.wheelRight);

            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            var vertexCount = meshFilters.Where(m => m.sharedMesh != null).Sum(m => m.sharedMesh.vertexCount);
            if (vertexCount < spec.MinimumVertices)
                throw new InvalidOperationException(spec.UnitId + ": suspiciously low vertex count " + vertexCount + ".");

            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(spec.DefinitionPath);
            if (definition == null || definition.productionPrefab != prefab || definition.preferredPrefabMode != ActorVisualPrefabMode.ProductionPrefab)
                throw new InvalidOperationException(spec.UnitId + ": visual definition is not wired to production prefab.");
        }

        static void ValidateWheelSide(VehicleSpec spec, string sideName, string semanticSocketName, Transform[] pivots)
        {
            if (pivots == null || pivots.Length < spec.ExpectedWheelCountPerSide)
                throw new InvalidOperationException(spec.UnitId + ": expected " + spec.ExpectedWheelCountPerSide + " " + sideName + " wheel spin pivots.");

            for (var i = 0; i < pivots.Length; i++)
            {
                if (pivots[i] == null)
                    throw new InvalidOperationException(spec.UnitId + ": null " + sideName + " wheel pivot " + i + ".");
                if (pivots[i].name == semanticSocketName)
                    throw new InvalidOperationException(spec.UnitId + ": semantic socket " + semanticSocketName + " is still wired as a rotating wheel.");
                if (pivots[i].GetComponentInChildren<MeshRenderer>(true) == null)
                    throw new InvalidOperationException(spec.UnitId + ": " + pivots[i].name + " has no wheel mesh renderer child.");
            }
        }

        static void UpdateVisualDefinition(VehicleSpec spec, GameObject prefab)
        {
            EnsureFolder(Path.GetDirectoryName(spec.DefinitionPath).Replace('\\', '/'));
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(spec.DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ActorVisualDefinition>();
                AssetDatabase.CreateAsset(definition, spec.DefinitionPath);
            }

            definition.actorTypeId = spec.UnitId;
            definition.displayName = spec.DisplayName;
            definition.safeDisplayName = spec.DisplayName;
            definition.category = ActorArtCategory.Vehicle;
            definition.productionStatus = ActorArtProductionStatus.Animated;
            definition.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spec.IconPath);
            definition.productionPrefab = prefab;
            definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
            definition.motionProfileId = spec.MotionProfileId;
            definition.selectionRadius = spec.SelectionRadius;
            definition.selectionHeight = spec.SelectionHeight;
            definition.visualScale = spec.VisualScale;
            definition.prefabHeightOffset = 0f;
            definition.useVehicleMotionController = true;
            definition.useTurretVisualController = true;
            definition.useBuildingVisualController = false;
            definition.requiredSockets.Clear();
            definition.requiredSockets.AddRange(RequiredSockets(spec));
            definition.ipReviewRequired = false;
            definition.notes = spec.DisplayName + " imported from Ironclad WheelClearV2 packet. Wheel animation uses individual per-wheel spin pivots under stable WheelLeft/WheelRight sockets.";
            EditorUtility.SetDirty(definition);
        }

        static List<ActorPrefabSocketKind> RequiredSockets(VehicleSpec spec)
        {
            var sockets = new List<ActorPrefabSocketKind>
            {
                ActorPrefabSocketKind.Root,
                ActorPrefabSocketKind.VisualRoot,
                ActorPrefabSocketKind.BodyRoot,
                ActorPrefabSocketKind.SelectionAnchor,
                ActorPrefabSocketKind.HealthBarAnchor,
                ActorPrefabSocketKind.UiAnchor,
                ActorPrefabSocketKind.WheelLeft,
                ActorPrefabSocketKind.WheelRight,
                ActorPrefabSocketKind.TurretRoot,
                ActorPrefabSocketKind.BarrelRoot,
                ActorPrefabSocketKind.MuzzlePrimary,
                ActorPrefabSocketKind.LightRoot,
                ActorPrefabSocketKind.VfxSmoke,
                ActorPrefabSocketKind.VfxExplosion,
                ActorPrefabSocketKind.AimPivot
            };
            if (spec.HasRearRamp)
            {
                sockets.Add(ActorPrefabSocketKind.DoorRoot);
                sockets.Add(ActorPrefabSocketKind.ProductionExit);
            }
            return sockets;
        }

        static Transform Socket(Transform parent, string socketName, ActorPrefabSocketKind socketKind, Vector3 localPosition, VehicleSpec spec)
        {
            var go = new GameObject(socketName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            var socket = go.AddComponent<ActorPrefabSocket>();
            socket.socketKind = socketKind;
            socket.socketName = socketName;
            socket.actorTypeId = spec.UnitId;
            socket.notes = spec.DisplayName + " packet socket: " + socketName;
            return go.transform;
        }

        static Transform Child(Transform parent, string name, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }

        static GameObject InstantiateModel(VehicleSpec spec, string name, string suffix, Transform parent, Vector3 localPosition, Material material)
        {
            var path = Part(spec, suffix);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null)
                throw new FileNotFoundException("Missing " + spec.UnitId + " model asset: " + path);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);

            return instance;
        }

        static GameObject InstantiateOptionalModel(VehicleSpec spec, string name, string suffix, Transform parent, Vector3 localPosition, Material material)
        {
            var path = Part(spec, suffix);
            if (!File.Exists(AbsoluteAssetPath(path)))
                return null;
            return InstantiateModel(spec, name, suffix, parent, localPosition, material);
        }

        sealed class WheelRigBuildResult
        {
            public readonly List<Transform> WheelPivots = new List<Transform>();
        }

        sealed class ObjMeshData
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int[]> Faces = new List<int[]>();
        }

        sealed class MeshComponent
        {
            public readonly List<int> FaceIndices = new List<int>();
            public readonly HashSet<int> VertexIndices = new HashSet<int>();
            public Bounds Bounds;
            public Vector3 Center;
            public int FaceCount { get { return FaceIndices.Count; } }
        }

        sealed class WheelGroup
        {
            public Vector3 Center;
            public readonly List<int> FaceIndices = new List<int>();
        }

        sealed class WheelGeometry
        {
            public Vector3 Center;
            public float Radius;
            public float HalfWidth;
        }

        static WheelRigBuildResult CreateIndividualWheelMeshes(VehicleSpec spec, string sideName, string suffix, Transform socket, Vector3 socketLocalPosition, Material tireMaterial, Material capMaterial, Material boltMaterial)
        {
            var data = LoadObjMesh(AbsoluteAssetPath(Part(spec, suffix)));
            var components = FindComponents(data);
            var centers = InferWheelCenters(components, spec.ExpectedWheelCountPerSide);
            if (centers.Count != spec.ExpectedWheelCountPerSide)
                throw new InvalidOperationException(spec.UnitId + " " + sideName + " wheel split expected " + spec.ExpectedWheelCountPerSide + " wheels, found " + centers.Count + ".");

            var groups = centers.Select(c => new WheelGroup { Center = c }).ToList();
            for (var i = 0; i < components.Count; i++)
            {
                var nearest = 0;
                var nearestDistance = float.MaxValue;
                for (var j = 0; j < groups.Count; j++)
                {
                    var distance = Mathf.Abs(components[i].Center.z - groups[j].Center.z);
                    if (distance < nearestDistance)
                    {
                        nearest = j;
                        nearestDistance = distance;
                    }
                }
                groups[nearest].FaceIndices.AddRange(components[i].FaceIndices);
            }

            var result = new WheelRigBuildResult();
            for (var i = 0; i < groups.Count; i++)
            {
                var number = (i + 1).ToString("00", CultureInfo.InvariantCulture);
                var geometry = CalculateWheelGeometry(data, groups[i]);
                var sideSign = sideName == "Left" ? -1f : 1f;
                var wheelLocalPosition = new Vector3(sideSign * 0.16f, geometry.Center.y - socketLocalPosition.y, geometry.Center.z - socketLocalPosition.z);
                var pivot = Child(socket, sideName + "Wheel_" + number + "_SpinPivot", wheelLocalPosition);
                var meshObject = new GameObject(sideName + "Wheel_" + number + "_CleanWheelMesh");
                meshObject.transform.SetParent(pivot, false);
                meshObject.transform.localPosition = Vector3.zero;
                meshObject.transform.localRotation = Quaternion.identity;
                meshObject.transform.localScale = Vector3.one;

                var filter = meshObject.AddComponent<MeshFilter>();
                filter.sharedMesh = CreateWheelTreadBandMeshAsset(spec, sideName, i, geometry);
                var renderer = meshObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = tireMaterial;
                result.WheelPivots.Add(pivot);
            }

            return result;
        }

        static ObjMeshData LoadObjMesh(string absolutePath)
        {
            if (!File.Exists(absolutePath))
                throw new FileNotFoundException("Missing wheel OBJ: " + absolutePath);

            var data = new ObjMeshData();
            foreach (var rawLine in File.ReadLines(absolutePath))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                if (line.StartsWith("v ", StringComparison.Ordinal))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    data.Vertices.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    continue;
                }

                if (!line.StartsWith("f ", StringComparison.Ordinal))
                    continue;

                var parts2 = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var indices = new List<int>();
                for (var i = 1; i < parts2.Length; i++)
                    indices.Add(ParseObjVertexIndex(parts2[i], data.Vertices.Count));
                for (var i = 1; i < indices.Count - 1; i++)
                    data.Faces.Add(new[] { indices[0], indices[i], indices[i + 1] });
            }

            if (data.Vertices.Count == 0 || data.Faces.Count == 0)
                throw new InvalidOperationException("Wheel OBJ had no readable mesh data: " + absolutePath);
            return data;
        }

        static int ParseObjVertexIndex(string token, int vertexCount)
        {
            var slash = token.IndexOf('/');
            var vertexToken = slash >= 0 ? token.Substring(0, slash) : token;
            var index = int.Parse(vertexToken, CultureInfo.InvariantCulture);
            if (index > 0)
                return index - 1;
            if (index < 0)
                return vertexCount + index;
            throw new InvalidOperationException("OBJ vertex indices are 1-based.");
        }

        static List<MeshComponent> FindComponents(ObjMeshData data)
        {
            var facesByVertex = new Dictionary<int, List<int>>();
            for (var faceIndex = 0; faceIndex < data.Faces.Count; faceIndex++)
            {
                var face = data.Faces[faceIndex];
                for (var i = 0; i < face.Length; i++)
                {
                    List<int> faces;
                    if (!facesByVertex.TryGetValue(face[i], out faces))
                    {
                        faces = new List<int>();
                        facesByVertex.Add(face[i], faces);
                    }
                    faces.Add(faceIndex);
                }
            }

            var components = new List<MeshComponent>();
            var visited = new bool[data.Faces.Count];
            var queue = new Queue<int>();
            for (var start = 0; start < data.Faces.Count; start++)
            {
                if (visited[start])
                    continue;

                var component = new MeshComponent();
                visited[start] = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    var faceIndex = queue.Dequeue();
                    component.FaceIndices.Add(faceIndex);
                    var face = data.Faces[faceIndex];
                    for (var i = 0; i < face.Length; i++)
                    {
                        var vertexIndex = face[i];
                        component.VertexIndices.Add(vertexIndex);
                        var neighbors = facesByVertex[vertexIndex];
                        for (var j = 0; j < neighbors.Count; j++)
                        {
                            var neighbor = neighbors[j];
                            if (visited[neighbor])
                                continue;
                            visited[neighbor] = true;
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                component.Bounds = BuildBounds(data, component.VertexIndices);
                component.Center = component.Bounds.center;
                components.Add(component);
            }
            return components;
        }

        static Bounds BuildBounds(ObjMeshData data, HashSet<int> vertexIndices)
        {
            var bounds = new Bounds();
            var initialized = false;
            foreach (var vertexIndex in vertexIndices)
            {
                var vertex = data.Vertices[vertexIndex];
                if (!initialized)
                {
                    bounds = new Bounds(vertex, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(vertex);
                }
            }
            return bounds;
        }

        static List<Vector3> InferWheelCenters(List<MeshComponent> components, int expectedCount)
        {
            var major = components.Where(c => c.FaceCount >= 40).OrderBy(c => c.Center.z).ToList();
            var clustered = ClusterWheelCenters(major, 0.32f);
            if (clustered.Count == expectedCount)
                return clustered;
            return KMeansWheelCenters(components.Where(c => c.FaceCount >= 6).ToList(), expectedCount);
        }

        static List<Vector3> ClusterWheelCenters(List<MeshComponent> components, float zTolerance)
        {
            var clusters = new List<List<MeshComponent>>();
            for (var i = 0; i < components.Count; i++)
            {
                if (clusters.Count == 0 || Mathf.Abs(components[i].Center.z - WeightedCenter(clusters[clusters.Count - 1]).z) > zTolerance)
                    clusters.Add(new List<MeshComponent>());
                clusters[clusters.Count - 1].Add(components[i]);
            }
            return clusters.Select(WeightedCenter).OrderBy(c => c.z).ToList();
        }

        static List<Vector3> KMeansWheelCenters(List<MeshComponent> components, int expectedCount)
        {
            if (expectedCount <= 0 || components.Count == 0)
                return new List<Vector3>();

            var sorted = components.OrderBy(c => c.Center.z).ToList();
            var centers = new List<Vector3>();
            for (var i = 0; i < expectedCount; i++)
            {
                var index = Mathf.Clamp(Mathf.RoundToInt(((i + 0.5f) / expectedCount) * (sorted.Count - 1)), 0, sorted.Count - 1);
                centers.Add(sorted[index].Center);
            }

            for (var iteration = 0; iteration < 12; iteration++)
            {
                var clusters = new List<MeshComponent>[expectedCount];
                for (var i = 0; i < clusters.Length; i++)
                    clusters[i] = new List<MeshComponent>();
                for (var i = 0; i < components.Count; i++)
                {
                    var nearest = 0;
                    var nearestDistance = float.MaxValue;
                    for (var j = 0; j < centers.Count; j++)
                    {
                        var distance = Mathf.Abs(components[i].Center.z - centers[j].z);
                        if (distance < nearestDistance)
                        {
                            nearest = j;
                            nearestDistance = distance;
                        }
                    }
                    clusters[nearest].Add(components[i]);
                }
                for (var i = 0; i < clusters.Length; i++)
                    if (clusters[i].Count > 0)
                        centers[i] = WeightedCenter(clusters[i]);
            }
            return centers.OrderBy(c => c.z).ToList();
        }

        static Vector3 WeightedCenter(List<MeshComponent> components)
        {
            var totalWeight = 0f;
            var center = Vector3.zero;
            for (var i = 0; i < components.Count; i++)
            {
                var weight = Mathf.Max(1, components[i].FaceCount);
                center += components[i].Center * weight;
                totalWeight += weight;
            }
            return totalWeight <= 0f ? Vector3.zero : center / totalWeight;
        }

        static WheelGeometry CalculateWheelGeometry(ObjMeshData data, WheelGroup group)
        {
            var bounds = BuildBoundsFromFaces(data, group.FaceIndices);
            return new WheelGeometry
            {
                Center = group.Center,
                Radius = Mathf.Max(0.18f, Mathf.Max(bounds.extents.y, bounds.extents.z) * 1.02f),
                HalfWidth = Mathf.Max(0.08f, bounds.extents.x * 0.92f)
            };
        }

        static Bounds BuildBoundsFromFaces(ObjMeshData data, List<int> faceIndices)
        {
            var bounds = new Bounds();
            var initialized = false;
            for (var i = 0; i < faceIndices.Count; i++)
            {
                var face = data.Faces[faceIndices[i]];
                for (var j = 0; j < face.Length; j++)
                {
                    var vertex = data.Vertices[face[j]];
                    if (!initialized)
                    {
                        bounds = new Bounds(vertex, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(vertex);
                    }
                }
            }
            return bounds;
        }

        static Mesh CreateWheelTreadBandMeshAsset(VehicleSpec spec, string sideName, int index, WheelGeometry geometry)
        {
            var mesh = new Mesh();
            mesh.name = spec.UnitId + "_" + sideName.ToLowerInvariant() + "_clean_wheel_" + (index + 1).ToString("00", CultureInfo.InvariantCulture);

            const int segments = 32;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var radius = Mathf.Clamp(geometry.Radius * 0.94f, 0.28f, 0.58f);
            var halfWidth = Mathf.Clamp(geometry.HalfWidth * 0.60f, 0.11f, 0.24f);
            for (var i = 0; i < segments; i++)
            {
                var angle = Mathf.PI * 2f * i / segments;
                var y = Mathf.Sin(angle) * radius;
                var z = Mathf.Cos(angle) * radius;
                vertices.Add(new Vector3(-halfWidth, y, z));
                vertices.Add(new Vector3(halfWidth, y, z));
            }

            for (var i = 0; i < segments; i++)
            {
                var next = (i + 1) % segments;
                var a = i * 2;
                var b = a + 1;
                var c = next * 2;
                var d = c + 1;
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }

            var leftCenter = vertices.Count;
            vertices.Add(new Vector3(-halfWidth, 0f, 0f));
            var rightCenter = vertices.Count;
            vertices.Add(new Vector3(halfWidth, 0f, 0f));
            for (var i = 0; i < segments; i++)
            {
                var next = (i + 1) % segments;
                var leftA = i * 2;
                var leftB = next * 2;
                var rightA = i * 2 + 1;
                var rightB = next * 2 + 1;
                triangles.Add(leftCenter);
                triangles.Add(leftA);
                triangles.Add(leftB);
                triangles.Add(rightCenter);
                triangles.Add(rightB);
                triangles.Add(rightA);
            }

            mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            SaveWheelMeshAsset(spec, mesh);
            return mesh;
        }

        static void SaveWheelMeshAsset(VehicleSpec spec, Mesh mesh)
        {
            var folder = spec.SourceFolder + "/GeneratedWheelMeshes";
            EnsureFolder(folder);
            var assetPath = folder + "/" + mesh.name + ".asset";
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(mesh, assetPath);
        }

        static void CreateStationaryWheelHub(VehicleSpec spec, string sideName, string number, Transform socket, Vector3 socketLocalPosition, WheelGeometry geometry, Material capMaterial, Material boltMaterial)
        {
            var sideSign = sideName == "Left" ? -1f : 1f;
            var localCenter = geometry.Center - socketLocalPosition;
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = sideName + "Wheel_" + number + "_StaticHubCap";
            cap.transform.SetParent(socket, false);
            cap.transform.localPosition = localCenter + Vector3.right * sideSign * geometry.HalfWidth * 1.04f;
            cap.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            cap.transform.localScale = new Vector3(geometry.Radius * 0.78f, 0.018f, geometry.Radius * 0.78f);
            var collider = cap.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            var renderer = cap.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = capMaterial;

            for (var i = 0; i < 4; i++)
            {
                var angle = Mathf.PI * 2f * i / 4f + Mathf.PI * 0.25f;
                var bolt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bolt.name = sideName + "Wheel_" + number + "_StaticHubBolt_" + (i + 1).ToString("00", CultureInfo.InvariantCulture);
                bolt.transform.SetParent(socket, false);
                var y = Mathf.Sin(angle) * geometry.Radius * 0.26f;
                var z = Mathf.Cos(angle) * geometry.Radius * 0.26f;
                bolt.transform.localPosition = localCenter + Vector3.right * sideSign * geometry.HalfWidth * 1.09f + new Vector3(0f, y, z);
                bolt.transform.localRotation = Quaternion.identity;
                var boltSize = Mathf.Max(0.035f, geometry.Radius * 0.10f);
                bolt.transform.localScale = new Vector3(0.018f, boltSize, boltSize);
                var boltCollider = bolt.GetComponent<Collider>();
                if (boltCollider != null)
                    UnityEngine.Object.DestroyImmediate(boltCollider);
                var boltRenderer = bolt.GetComponent<Renderer>();
                if (boltRenderer != null)
                    boltRenderer.sharedMaterial = boltMaterial;
            }
        }

        static string Part(VehicleSpec spec, string suffix)
        {
            return spec.SourceFolder + "/" + spec.UnitId + "_" + suffix;
        }

        static string TexturePath(VehicleSpec spec, string suffix)
        {
            return spec.TextureFolder + "/" + spec.UnitId + "_" + suffix + ".png";
        }

        static Material CreateMaterial(VehicleSpec spec, string suffix, Color baseColor, bool emission, float metallic, float smoothness)
        {
            var path = spec.MaterialFolder + "/mat_" + spec.UnitId + "_" + suffix + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader) { name = "mat_" + spec.UnitId + "_" + suffix };
                AssetDatabase.CreateAsset(material, path);
            }

            SetColor(material, "_BaseColor", baseColor);
            SetColor(material, "_Color", baseColor);
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_Glossiness", smoothness);

            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(spec, "albedo"));
            if (albedo != null && suffix != "team")
                SetTexture(material, "_BaseMap", "_MainTex", albedo);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(spec, "normal"));
            if (normal != null && suffix != "team")
            {
                SetTexture(material, "_BumpMap", "_BumpMap", normal);
                SetFloat(material, "_BumpScale", 0.6f);
                material.EnableKeyword("_NORMALMAP");
            }
            var orm = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(spec, "orm"));
            if (orm != null && suffix != "team")
            {
                SetTexture(material, "_MetallicGlossMap", "_MetallicGlossMap", orm);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            var emissionMap = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(spec, "emission"));
            if (emission)
            {
                SetColor(material, "_EmissionColor", baseColor * 1.7f);
                if (emissionMap != null)
                    SetTexture(material, "_EmissionMap", "_EmissionMap", emissionMap);
                material.EnableKeyword("_EMISSION");
            }
            if (suffix == "glass")
            {
                material.SetOverrideTag("RenderType", "Transparent");
                SetFloat(material, "_Surface", 1f);
                material.renderQueue = (int)RenderQueue.Transparent;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        static void SetColor(Material material, string property, Color color)
        {
            if (material.HasProperty(property))
                material.SetColor(property, color);
        }

        static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property))
                material.SetFloat(property, value);
        }

        static void SetTexture(Material material, string primary, string fallback, Texture texture)
        {
            if (material.HasProperty(primary))
                material.SetTexture(primary, texture);
            else if (material.HasProperty(fallback))
                material.SetTexture(fallback, texture);
        }

        static Transform CreateMuzzleFlash(Transform parent, Material material)
        {
            var flash = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flash.name = "MuzzleFlash";
            flash.transform.SetParent(parent, false);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;
            flash.transform.localScale = Vector3.one * 0.24f;
            var collider = flash.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            var renderer = flash.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            flash.SetActive(false);
            return flash.transform;
        }

        static void CreateHeadlight(Transform parent, Color color)
        {
            var go = new GameObject("HeadlightGlow");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = 2.8f;
            light.intensity = 0.85f;
        }

        static void ConfigureLodGroup(LODGroup lodGroup, Transform lod0, Transform lod1, Transform lod2)
        {
            lodGroup.SetLODs(new[]
            {
                new LOD(0.62f, lod0.GetComponentsInChildren<Renderer>(true)),
                new LOD(0.24f, lod1.GetComponentsInChildren<Renderer>(true)),
                new LOD(0.08f, lod2.GetComponentsInChildren<Renderer>(true))
            });
            lodGroup.RecalculateBounds();
        }

        static void CreateReviewScene(VehicleSpec spec, GameObject prefab)
        {
            EnsureFolder(Path.GetDirectoryName(spec.ReviewScenePath).Replace('\\', '/'));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(spec.DisplayName + " Unit Review");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = spec.UnitId + "_packet_prefab";
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            instance.transform.localScale = Vector3.one * spec.VisualScale;
            instance.transform.SetParent(root.transform);
            var demo = instance.AddComponent<TankVisualDemoController>();
            demo.rig = instance.GetComponent<TankVisualRigController>();
            demo.orbitRadius = 0.30f;
            demo.orbitSpeed = spec.UnitId == "scout_rover" ? 0.55f : 0.34f;
            demo.fireInterval = spec.UnitId == "scout_rover" ? 1.15f : 1.45f;
            demo.aimSweepDegrees = 48f;

            CreateGround();
            CreateLabel(spec.DisplayName + " - individual wheel spin pivots", new Vector3(-4.8f, 0.05f, -3.35f));
            CreateLabel("Wheel pivots per side: " + spec.ExpectedWheelCountPerSide, new Vector3(-4.8f, 0.05f, 3.35f));

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(3.8f, 5.2f, -5.8f);
            camera.transform.rotation = Quaternion.Euler(58f, -34f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 4.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.10f, 1f);

            var sun = new GameObject("Directional Light");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 18f);

            EditorSceneManager.SaveScene(scene, spec.ReviewScenePath);
        }

        static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Scale Ground";
            ground.transform.position = new Vector3(0f, -0.035f, 0f);
            ground.transform.localScale = new Vector3(6.0f, 0.04f, 6.0f);
            var collider = ground.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
                {
                    color = new Color(0.18f, 0.25f, 0.22f, 1f)
                };
        }

        static void CreateLabel(string text, Vector3 position)
        {
            var go = new GameObject("Label");
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = 0.12f;
            mesh.anchor = TextAnchor.MiddleLeft;
            mesh.color = new Color(0.90f, 0.95f, 0.90f, 1f);
        }

        static void EnsureFolders(VehicleSpec spec)
        {
            EnsureFolder(spec.SourceFolder);
            EnsureFolder(spec.TextureFolder);
            EnsureFolder(spec.MaterialFolder);
            EnsureFolder(spec.PrefabFolder);
            EnsureFolder(Path.GetDirectoryName(spec.IconPath).Replace('\\', '/'));
            EnsureFolder(Path.GetDirectoryName(spec.DefinitionPath).Replace('\\', '/'));
            EnsureFolder(Path.GetDirectoryName(spec.ReviewScenePath).Replace('\\', '/'));
        }

        static void EnsureFolder(string assetFolder)
        {
            if (string.IsNullOrEmpty(assetFolder) || AssetDatabase.IsValidFolder(assetFolder))
                return;

            var normalized = assetFolder.Replace('\\', '/');
            var parts = normalized.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static void ConfigureImports(VehicleSpec spec)
        {
            ConfigureTexture(TexturePath(spec, "albedo"), TextureImporterType.Default, true);
            ConfigureTexture(TexturePath(spec, "emission"), TextureImporterType.Default, true);
            ConfigureTexture(TexturePath(spec, "team_mask"), TextureImporterType.Default, false);
            ConfigureTexture(TexturePath(spec, "orm"), TextureImporterType.Default, false);
            ConfigureTexture(TexturePath(spec, "normal"), TextureImporterType.NormalMap, false);
            ConfigureTexture(TexturePath(spec, "muzzle_flash_sheet"), TextureImporterType.Default, true, true);
            ConfigureTexture(TexturePath(spec, "projectile_tracer"), TextureImporterType.Default, true, true);
            ConfigureTexture(spec.IconPath, TextureImporterType.Sprite, true, true);

            if (!Directory.Exists(AbsoluteAssetPath(spec.SourceFolder)))
                return;

            foreach (var objPath in Directory.GetFiles(AbsoluteAssetPath(spec.SourceFolder), "*.obj"))
            {
                var assetPath = "Assets" + objPath.Substring(Application.dataPath.Length).Replace('\\', '/');
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                    continue;
                importer.globalScale = 1f;
                importer.importNormals = ModelImporterNormals.Import;
                importer.importTangents = ModelImporterTangents.CalculateMikk;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        static void ConfigureTexture(string path, TextureImporterType type, bool srgb, bool alpha = false)
        {
            if (!File.Exists(AbsoluteAssetPath(path)))
                return;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.mipmapEnabled = true;
            importer.alphaSource = alpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = alpha;
            importer.SaveAndReimport();
        }

        static string AbsoluteAssetPath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                return assetPath;
            return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('/', Path.DirectorySeparatorChar);
        }
    }

    public static class ProjectAegisWheeledVehiclePreviewCapture
    {
        const int FrameCount = 48;
        const int FrameWidth = 960;
        const int FrameHeight = 640;

        [MenuItem("ProjectAegisRTS/Art/Capture Wheeled Vehicle Motion GIF Frames")]
        public static void CaptureMenu()
        {
            CaptureBatch();
        }

        public static void CaptureBatch()
        {
            try
            {
                Capture("scout_rover", "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover/scout_rover.prefab", 4.25f);
                Capture("apc", "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab", 4.85f);
                AssetDatabase.Refresh();
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

        public static void CaptureScoutBatch()
        {
            CaptureSingleBatch("scout_rover", "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover/scout_rover.prefab", 4.25f);
        }

        public static void CaptureApcBatch()
        {
            CaptureSingleBatch("apc", "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab", 4.85f);
        }

        static void CaptureSingleBatch(string unitId, string prefabPath, float cameraSize)
        {
            try
            {
                Capture(unitId, prefabPath, cameraSize);
                AssetDatabase.Refresh();
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

        static void Capture(string unitId, string prefabPath, float cameraSize)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateGround();
            var camera = CreateCamera(cameraSize);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Missing prefab: " + prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = unitId + "_motion_preview";
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.Euler(0f, 36f, 0f);

            var rig = instance.GetComponent<TankVisualRigController>();
            if (rig == null)
                throw new InvalidOperationException(unitId + " missing TankVisualRigController.");
            rig.driveTurretFromDesiredAim = true;
            rig.estimateMotionFromTransform = false;

            var outputFolder = FullPath("build/animations/" + unitId + "_individual_wheel_motion_frames");
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);

            const float dt = 1f / 12f;
            var baseRotation = Quaternion.Euler(0f, 180f, 0f);
            var baseForward = baseRotation * Vector3.forward;
            var baseRight = baseRotation * Vector3.right;
            var previousPosition = MotionPoint(-1f / (FrameCount - 1), baseForward, baseRight, unitId);
            for (var frame = 0; frame < FrameCount; frame++)
            {
                var phase = frame / (float)(FrameCount - 1);
                var position = MotionPoint(phase, baseForward, baseRight, unitId);
                var velocity = (position - previousPosition) / dt;
                instance.transform.position = position;
                instance.transform.rotation = velocity.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(Vector3.ProjectOnPlane(velocity, Vector3.up).normalized, Vector3.up)
                    : baseRotation;
                rig.SetAimDirection(baseRotation * Quaternion.Euler(0f, Mathf.Lerp(-45f, 45f, Mathf.PingPong(phase * 1.5f, 1f)), 0f) * Vector3.forward);
                if (frame == 10 || frame == 29)
                    rig.TriggerRecoil(1f);
                rig.ApplyVisualMotion(velocity, dt);
                RenderCameraToPng(camera, Path.Combine(outputFolder, "frame_" + frame.ToString("000", CultureInfo.InvariantCulture) + ".png").Replace('\\', '/'), FrameWidth, FrameHeight);
                previousPosition = position;
            }
        }

        static Vector3 MotionPoint(float phase, Vector3 forward, Vector3 right, string unitId)
        {
            var pathLength = unitId == "apc" ? 3.55f : 3.10f;
            var laneDrift = unitId == "apc" ? 0.07f : 0.08f;
            var along = Mathf.Lerp(-pathLength * 0.5f, pathLength * 0.5f, phase);
            var lateral = Mathf.Sin(phase * Mathf.PI * 2f) * laneDrift;
            return forward * along + right * lateral;
        }

        static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "preview ground";
            ground.transform.position = new Vector3(0f, -0.06f, 0f);
            ground.transform.localScale = new Vector3(10.4f, 0.05f, 5.2f);
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                material.color = new Color(0.16f, 0.22f, 0.18f, 1f);
                renderer.sharedMaterial = material;
            }
        }

        static void CreateLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.38f, 0.36f, 1f);
            var key = new GameObject("key light");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.25f;
            key.transform.rotation = Quaternion.Euler(48f, -30f, 0f);
            var fill = new GameObject("fill light");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.35f;
            fill.transform.rotation = Quaternion.Euler(62f, 150f, 0f);
        }

        static Camera CreateCamera(float orthographicSize)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(2.6f, 4.9f, -7.4f);
            cameraObject.transform.LookAt(new Vector3(0f, 0.72f, -0.10f));
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.070f, 0.066f, 1f);
            return camera;
        }

        static void RenderCameraToPng(Camera camera, string outputPath, int width, int height)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        static string FullPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../..", relativePath)).Replace('\\', '/');
        }
    }
}
#endif
