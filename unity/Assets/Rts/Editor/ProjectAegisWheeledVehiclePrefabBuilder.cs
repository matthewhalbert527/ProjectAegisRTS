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
    public static class ProjectAegisScoutRoverPrefabBuilder
    {
        [MenuItem("ProjectAegisRTS/Art/Build Scout Rover Prefab")]
        public static void BuildFromMenu()
        {
            ProjectAegisWheeledVehiclePrefabBuilder.Build(ProjectAegisWheeledVehiclePrefabBuilder.ScoutRover, true, true);
        }

        public static void BuildBatch()
        {
            ProjectAegisWheeledVehiclePrefabBuilder.Build(ProjectAegisWheeledVehiclePrefabBuilder.ScoutRover, false, true);
        }
    }

    public static class ProjectAegisApcPrefabBuilder
    {
        [MenuItem("ProjectAegisRTS/Art/Build APC Prefab")]
        public static void BuildFromMenu()
        {
            ProjectAegisWheeledVehiclePrefabBuilder.Build(ProjectAegisWheeledVehiclePrefabBuilder.Apc, true, true);
        }

        public static void BuildBatch()
        {
            ProjectAegisWheeledVehiclePrefabBuilder.Build(ProjectAegisWheeledVehiclePrefabBuilder.Apc, false, true);
        }
    }

    public static class ProjectAegisWheeledVehiclePrefabBuilder
    {
        public sealed class WheeledVehicleSpec
        {
            public string UnitId;
            public string DisplayName;
            public string SafeDisplayName;
            public string MotionProfileId;
            public string Source;
            public string Textures;
            public string Materials;
            public string IconPath;
            public string PrefabFolder;
            public string PrefabPath;
            public string VisualDefinitionPath;
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
            public float TrackScrollUnitsPerMeter;
            public float BarrelRecoilDistance;
            public int ExpectedWheelCountPerSide;
            public Color BodyColor;
            public Color TeamColor;
            public Color EmissiveColor;
            public Color GlassColor;
            public int ExpectedLod0Triangles;
        }

        public static readonly WheeledVehicleSpec ScoutRover = new WheeledVehicleSpec
        {
            UnitId = "scout_rover",
            DisplayName = "Scout Rover",
            SafeDisplayName = "Scout Rover",
            MotionProfileId = "wheeled_scout",
            Source = "Assets/Rts/Art/Source/Units/scout_rover",
            Textures = "Assets/Rts/Art/Textures/Units/scout_rover",
            Materials = "Assets/Rts/Art/Materials/Units/scout_rover",
            IconPath = "Assets/Rts/Art/Icons/scout_rover_icon.png",
            PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover",
            PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover/scout_rover.prefab",
            VisualDefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/scout_rover_visual.asset",
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
            TrackScrollUnitsPerMeter = 2.4f,
            BarrelRecoilDistance = 0.05f,
            ExpectedWheelCountPerSide = 2,
            BodyColor = new Color(0.31f, 0.36f, 0.25f, 1f),
            TeamColor = new Color(0.12f, 0.70f, 0.88f, 1f),
            EmissiveColor = new Color(1.0f, 0.47f, 0.12f, 1f),
            GlassColor = new Color(0.10f, 0.55f, 0.64f, 0.72f),
            ExpectedLod0Triangles = 2552
        };

        public static readonly WheeledVehicleSpec Apc = new WheeledVehicleSpec
        {
            UnitId = "apc",
            DisplayName = "APC",
            SafeDisplayName = "APC",
            MotionProfileId = "wheeled_apc",
            Source = "Assets/Rts/Art/Source/Units/apc",
            Textures = "Assets/Rts/Art/Textures/Units/apc",
            Materials = "Assets/Rts/Art/Materials/Units/apc",
            IconPath = "Assets/Rts/Art/Icons/apc_icon.png",
            PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc",
            PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab",
            VisualDefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/apc_visual.asset",
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
            TrackScrollUnitsPerMeter = 2.0f,
            BarrelRecoilDistance = 0.05f,
            ExpectedWheelCountPerSide = 3,
            BodyColor = new Color(0.32f, 0.37f, 0.27f, 1f),
            TeamColor = new Color(0.12f, 0.70f, 0.88f, 1f),
            EmissiveColor = new Color(1.0f, 0.48f, 0.10f, 1f),
            GlassColor = new Color(0.11f, 0.58f, 0.66f, 0.74f),
            ExpectedLod0Triangles = 3372
        };

        public static void Build(WheeledVehicleSpec spec, bool selectPrefab, bool createReviewScene)
        {
            EnsureFolders(spec);
            ConfigureImports(spec);

            var body = Material(spec, "body", spec.BodyColor, false, 0.25f, 0.50f);
            var metal = Material(spec, "dark_metal", new Color(0.12f, 0.15f, 0.13f, 1f), false, 0.55f, 0.36f);
            var bolts = Material(spec, "bolts", new Color(0.52f, 0.54f, 0.48f, 1f), false, 0.65f, 0.42f);
            var rubber = Material(spec, "rubber_wheels", new Color(0.035f, 0.038f, 0.035f, 1f), false, 0.05f, 0.30f);
            var team = Material(spec, "team", spec.TeamColor, false, 0.30f, 0.48f);
            var glass = Material(spec, "glass", spec.GlassColor, false, 0.05f, 0.78f);
            var emissive = Material(spec, "emissive", spec.EmissiveColor, true, 0.05f, 0.72f);
            var weapon = Material(spec, "weapon", new Color(0.22f, 0.23f, 0.20f, 1f), false, 0.62f, 0.40f);
            var muzzle = Material(spec, "muzzle_flash", new Color(1.0f, 0.60f, 0.12f, 1f), true, 0f, 0.20f);

            var actor = new GameObject(spec.UnitId);
            var descriptor = actor.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = spec.UnitId;
            descriptor.category = ActorArtCategory.Vehicle;
            descriptor.productionStatus = ActorArtProductionStatus.Animated;
            descriptor.generatedByStage8 = false;

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
            var teamBody = InstantiateOptionalModel(spec, "TeamColorPanels_Body", "team_body.obj", lod0, -spec.BodyRoot, team);

            var wheelLeft = Socket(visualRoot, "WheelLeft", ActorPrefabSocketKind.WheelLeft, spec.WheelLeft, spec);
            var wheelRight = Socket(visualRoot, "WheelRight", ActorPrefabSocketKind.WheelRight, spec.WheelRight, spec);
            var leftWheelRig = CreateIndividualWheelMeshes(spec, "Left", "wheel_left.obj", wheelLeft, spec.WheelLeft, rubber);
            var rightWheelRig = CreateIndividualWheelMeshes(spec, "Right", "wheel_right.obj", wheelRight, spec.WheelRight, rubber);

            var turret = Socket(visualRoot, "TurretRoot", ActorPrefabSocketKind.TurretRoot, spec.TurretRoot, spec);
            InstantiateModel(spec, "TurretArmor", "turret.obj", turret, -spec.TurretRoot, body);
            var teamTurret = InstantiateOptionalModel(spec, "TeamColorPanels_Turret", "team_turret.obj", turret, -spec.TurretRoot, team);
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

            var teamRig = actor.AddComponent<ProjectAegisTankTeamColorRig>();
            teamRig.teamColorRenderers = new[] { GetRenderer(teamBody), GetRenderer(teamTurret) }.Where(r => r != null).ToArray();
            teamRig.fallbackTeamColor = spec.TeamColor;

            var tankRig = actor.AddComponent<TankVisualRigController>();
            tankRig.bodyRoot = bodyRoot;
            tankRig.turretRoot = turret;
            tankRig.barrelRoot = barrel;
            tankRig.trackLeftRoot = wheelLeft;
            tankRig.trackRightRoot = wheelRight;
            tankRig.wheelLeft = leftWheelRig.WheelPivots.ToArray();
            tankRig.wheelRight = rightWheelRig.WheelPivots.ToArray();
            tankRig.trackRenderers = new Renderer[0];
            tankRig.muzzleFlashRoot = muzzleFlash;
            tankRig.turretTurnDegreesPerSecond = spec.UnitId == "scout_rover" ? 240f : 175f;
            tankRig.barrelRecoilDistance = spec.BarrelRecoilDistance;
            tankRig.wheelRotationDegreesPerMeter = spec.WheelRotationDegreesPerMeter;
            tankRig.trackScrollUnitsPerMeter = spec.TrackScrollUnitsPerMeter;
            tankRig.suspensionBobMeters = spec.UnitId == "scout_rover" ? 0.038f : 0.026f;
            tankRig.suspensionFrequency = spec.UnitId == "scout_rover" ? 6.5f : 4.8f;

            var collider = actor.AddComponent<BoxCollider>();
            collider.center = spec.ColliderCenter;
            collider.size = spec.ColliderSize;

            ConfigureDescriptor(descriptor, spec);
            ConfigureLodGroup(lodGroup, lod0, lod1, lod2);

            PrefabUtility.SaveAsPrefabAsset(actor, spec.PrefabPath);
            UnityEngine.Object.DestroyImmediate(actor);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            ConfigureVisualDefinition(spec, prefab);
            ConfigureSavedDescriptor(spec, prefab);

            if (createReviewScene)
                CreateReviewScene(spec, prefab);

            AssetDatabase.SaveAssets();
            if (selectPrefab)
                UnityEditor.Selection.activeObject = prefab;

            Debug.Log("Built ProjectAegisRTS " + spec.UnitId + " production prefab at " + spec.PrefabPath);
        }

        static void ConfigureDescriptor(ActorPrefabDescriptor descriptor, WheeledVehicleSpec spec)
        {
            descriptor.requiredSocketsPresent = true;
            descriptor.declaredRequiredSockets.Clear();
            descriptor.declaredRequiredSockets.AddRange(RequiredSockets(spec));
            descriptor.notes = spec.DisplayName + " production art imported from Ironclad wheel-clear packet. Gameplay remains sourced from Rts.Core.";
        }

        static void ConfigureSavedDescriptor(WheeledVehicleSpec spec, GameObject prefab)
        {
            var descriptor = prefab.GetComponent<ActorPrefabDescriptor>();
            if (descriptor == null)
                return;

            descriptor.sourceDefinition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(spec.VisualDefinitionPath);
            ConfigureDescriptor(descriptor, spec);
            EditorUtility.SetDirty(descriptor);
        }

        static void ConfigureVisualDefinition(WheeledVehicleSpec spec, GameObject prefab)
        {
            EnsureFolder(Path.GetDirectoryName(spec.VisualDefinitionPath).Replace('\\', '/'));
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(spec.VisualDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ActorVisualDefinition>();
                AssetDatabase.CreateAsset(definition, spec.VisualDefinitionPath);
            }

            definition.actorTypeId = spec.UnitId;
            definition.displayName = spec.DisplayName;
            definition.safeDisplayName = spec.SafeDisplayName;
            definition.category = ActorArtCategory.Vehicle;
            definition.productionStatus = ActorArtProductionStatus.Animated;
            definition.icon = Stage8ActorCatalog.LoadSpriteAtPath(spec.IconPath);
            definition.productionPrefab = prefab;
            definition.fallbackPrefab = definition.fallbackPrefab == null ? definition.generatedBlockoutPrefab : definition.fallbackPrefab;
            definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
            definition.motionProfileId = spec.MotionProfileId;
            definition.selectionRadius = spec.SelectionRadius;
            definition.selectionHeight = spec.SelectionHeight;
            definition.footprintWidth = Math.Max(1, definition.footprintWidth);
            definition.footprintHeight = Math.Max(1, definition.footprintHeight);
            definition.visualScale = spec.VisualScale;
            definition.prefabHeightOffset = 0f;
            definition.useBuildingVisualController = false;
            definition.useVehicleMotionController = true;
            definition.useTurretVisualController = true;
            definition.requiredSockets.Clear();
            definition.requiredSockets.AddRange(RequiredSockets(spec));
            definition.ipReviewRequired = false;
            definition.notes = spec.DisplayName + " production visual uses the Ironclad wheel-clear Unity asset packet with outboard wheel meshes, turret/barrel hooks, team panels, LODs, and muzzle flash hook.";
            EditorUtility.SetDirty(definition);
        }

        static List<ActorPrefabSocketKind> RequiredSockets(WheeledVehicleSpec spec)
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

        static Transform Socket(Transform parent, string socketName, ActorPrefabSocketKind socketKind, Vector3 localPosition, WheeledVehicleSpec spec)
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

        static GameObject InstantiateModel(WheeledVehicleSpec spec, string name, string suffix, Transform parent, Vector3 localPosition, Material material)
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

        static GameObject InstantiateOptionalModel(WheeledVehicleSpec spec, string name, string suffix, Transform parent, Vector3 localPosition, Material material)
        {
            var path = Part(spec, suffix);
            if (!File.Exists(AbsoluteAssetPath(path)))
                return null;
            return InstantiateModel(spec, name, suffix, parent, localPosition, material);
        }

        sealed class WheelRigBuildResult
        {
            public readonly List<Transform> WheelPivots = new List<Transform>();
            public readonly List<Renderer> Renderers = new List<Renderer>();
        }

        sealed class ObjWheelMeshData
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<int[]> Faces = new List<int[]>();
        }

        sealed class WheelComponent
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

        static WheelRigBuildResult CreateIndividualWheelMeshes(WheeledVehicleSpec spec, string sideName, string suffix, Transform socket, Vector3 socketLocalPosition, Material material)
        {
            var assetPath = Part(spec, suffix);
            var absolutePath = AbsoluteAssetPath(assetPath);
            if (!File.Exists(absolutePath))
                throw new FileNotFoundException("Missing " + spec.UnitId + " wheel source mesh: " + assetPath);

            var data = LoadObjWheelMesh(absolutePath);
            var components = FindWheelComponents(data);
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
                var pivot = Child(socket, sideName + "Wheel_" + number + "_SpinPivot", groups[i].Center - socketLocalPosition);
                var meshObject = new GameObject(sideName + "Wheel_" + number + "_Mesh");
                meshObject.transform.SetParent(pivot, false);
                meshObject.transform.localPosition = Vector3.zero;
                meshObject.transform.localRotation = Quaternion.identity;
                meshObject.transform.localScale = Vector3.one;

                var mesh = CreateWheelMeshAsset(spec, sideName, i, data, groups[i]);
                var filter = meshObject.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                var renderer = meshObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;

                result.WheelPivots.Add(pivot);
                result.Renderers.Add(renderer);
            }

            return result;
        }

        static ObjWheelMeshData LoadObjWheelMesh(string absolutePath)
        {
            var data = new ObjWheelMeshData();
            foreach (var rawLine in File.ReadLines(absolutePath))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                if (line.StartsWith("v ", StringComparison.Ordinal))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4)
                        continue;
                    data.Vertices.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    continue;
                }

                if (!line.StartsWith("f ", StringComparison.Ordinal))
                    continue;

                var faceParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (faceParts.Length < 4)
                    continue;

                var indices = new List<int>();
                for (var i = 1; i < faceParts.Length; i++)
                    indices.Add(ParseObjVertexIndex(faceParts[i], data.Vertices.Count));

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

        static List<WheelComponent> FindWheelComponents(ObjWheelMeshData data)
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

            var components = new List<WheelComponent>();
            var visited = new bool[data.Faces.Count];
            var queue = new Queue<int>();
            for (var start = 0; start < data.Faces.Count; start++)
            {
                if (visited[start])
                    continue;

                var component = new WheelComponent();
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

        static Bounds BuildBounds(ObjWheelMeshData data, HashSet<int> vertexIndices)
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

        static List<Vector3> InferWheelCenters(List<WheelComponent> components, int expectedCount)
        {
            var major = components.Where(c => c.FaceCount >= 40).OrderBy(c => c.Center.z).ToList();
            var clustered = ClusterWheelCenters(major, 0.32f);
            if (clustered.Count == expectedCount)
                return clustered;

            var candidates = components.Where(c => c.FaceCount >= 6).ToList();
            if (candidates.Count == 0)
                candidates = components.ToList();
            return KMeansWheelCenters(candidates, expectedCount);
        }

        static List<Vector3> ClusterWheelCenters(List<WheelComponent> components, float zTolerance)
        {
            var clusters = new List<List<WheelComponent>>();
            for (var i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (clusters.Count == 0 || Mathf.Abs(component.Center.z - WeightedCenter(clusters[clusters.Count - 1]).z) > zTolerance)
                    clusters.Add(new List<WheelComponent>());
                clusters[clusters.Count - 1].Add(component);
            }

            return clusters.Select(WeightedCenter).OrderBy(c => c.z).ToList();
        }

        static List<Vector3> KMeansWheelCenters(List<WheelComponent> components, int expectedCount)
        {
            if (expectedCount <= 0)
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
                var clusters = new List<WheelComponent>[expectedCount];
                for (var i = 0; i < clusters.Length; i++)
                    clusters[i] = new List<WheelComponent>();

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

        static Vector3 WeightedCenter(List<WheelComponent> components)
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

        static Mesh CreateWheelMeshAsset(WheeledVehicleSpec spec, string sideName, int index, ObjWheelMeshData data, WheelGroup group)
        {
            var mesh = new Mesh();
            mesh.name = spec.UnitId + "_" + sideName.ToLowerInvariant() + "_wheel_" + (index + 1).ToString("00", CultureInfo.InvariantCulture);

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var remap = new Dictionary<int, int>();
            for (var i = 0; i < group.FaceIndices.Count; i++)
            {
                var face = data.Faces[group.FaceIndices[i]];
                for (var j = 0; j < face.Length; j++)
                {
                    int mapped;
                    if (!remap.TryGetValue(face[j], out mapped))
                    {
                        mapped = vertices.Count;
                        remap.Add(face[j], mapped);
                        vertices.Add(data.Vertices[face[j]] - group.Center);
                    }
                    triangles.Add(mapped);
                }
            }

            mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            var folder = spec.Source + "/GeneratedWheelMeshes";
            EnsureFolder(folder);
            var assetPath = folder + "/" + mesh.name + ".asset";
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(mesh, assetPath);
            return mesh;
        }

        static string Part(WheeledVehicleSpec spec, string suffix)
        {
            return spec.Source + "/" + spec.UnitId + "_" + suffix;
        }

        static string TexturePath(WheeledVehicleSpec spec, string suffix)
        {
            return spec.Textures + "/" + spec.UnitId + "_" + suffix + ".png";
        }

        static string AbsoluteAssetPath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                return assetPath;
            return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('/', Path.DirectorySeparatorChar);
        }

        static Material Material(WheeledVehicleSpec spec, string suffix, Color baseColor, bool emission, float metallic, float smoothness)
        {
            var path = spec.Materials + "/mat_" + spec.UnitId + "_" + suffix + ".mat";
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

            var albedo = Texture(spec, "albedo");
            if (albedo != null && suffix != "team")
                SetTexture(material, "_BaseMap", "_MainTex", albedo);
            var normal = Texture(spec, "normal");
            if (normal != null && suffix != "team")
            {
                SetTexture(material, "_BumpMap", "_BumpMap", normal);
                SetFloat(material, "_BumpScale", 0.6f);
                material.EnableKeyword("_NORMALMAP");
            }
            var orm = Texture(spec, "orm");
            if (orm != null && suffix != "team")
            {
                SetTexture(material, "_MetallicGlossMap", "_MetallicGlossMap", orm);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            var emissionMap = Texture(spec, "emission");
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
                SetFloat(material, "_AlphaClip", 0f);
                material.renderQueue = (int)RenderQueue.Transparent;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        static Texture2D Texture(WheeledVehicleSpec spec, string suffix)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(spec, suffix));
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

        static Renderer GetRenderer(GameObject go)
        {
            return go == null ? null : go.GetComponentInChildren<Renderer>(true);
        }

        static void ConfigureLodGroup(LODGroup lodGroup, Transform lod0, Transform lod1, Transform lod2)
        {
            var lod0Renderers = lod0.GetComponentsInChildren<Renderer>(true);
            var lod1Renderers = lod1.GetComponentsInChildren<Renderer>(true);
            var lod2Renderers = lod2.GetComponentsInChildren<Renderer>(true);
            lodGroup.SetLODs(new[]
            {
                new LOD(0.62f, lod0Renderers),
                new LOD(0.24f, lod1Renderers),
                new LOD(0.08f, lod2Renderers)
            });
            lodGroup.RecalculateBounds();
        }

        static Transform CreateMuzzleFlash(Transform parent, Material material)
        {
            var flash = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flash.name = "MuzzleFlash";
            flash.transform.SetParent(parent, false);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;
            flash.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
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

        static void EnsureFolders(WheeledVehicleSpec spec)
        {
            EnsureFolder(spec.Source);
            EnsureFolder(spec.Textures);
            EnsureFolder(spec.Materials);
            EnsureFolder(spec.PrefabFolder);
            EnsureFolder(Path.GetDirectoryName(spec.IconPath).Replace('\\', '/'));
            EnsureFolder(Path.GetDirectoryName(spec.VisualDefinitionPath).Replace('\\', '/'));
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

        static void ConfigureImports(WheeledVehicleSpec spec)
        {
            ConfigureTexture(TexturePath(spec, "albedo"), TextureImporterType.Default, true);
            ConfigureTexture(TexturePath(spec, "emission"), TextureImporterType.Default, true);
            ConfigureTexture(TexturePath(spec, "team_mask"), TextureImporterType.Default, false);
            ConfigureTexture(TexturePath(spec, "orm"), TextureImporterType.Default, false);
            ConfigureTexture(TexturePath(spec, "normal"), TextureImporterType.NormalMap, false);
            ConfigureTexture(TexturePath(spec, "muzzle_flash_sheet"), TextureImporterType.Default, true, true);
            ConfigureTexture(TexturePath(spec, "projectile_tracer"), TextureImporterType.Default, true, true);
            ConfigureTexture(spec.IconPath, TextureImporterType.Sprite, true, true);

            foreach (var objPath in Directory.GetFiles(AbsoluteAssetPath(spec.Source), "*.obj"))
            {
                var assetPath = "Assets" + objPath.Substring(Application.dataPath.Length).Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                {
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                }
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

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                importer = AssetImporter.GetAtPath(path) as TextureImporter;
            }
            if (importer == null)
                return;

            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.mipmapEnabled = true;
            importer.alphaSource = alpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = alpha;
            importer.SaveAndReimport();
        }

        static void CreateReviewScene(WheeledVehicleSpec spec, GameObject prefab)
        {
            EnsureFolder(Path.GetDirectoryName(spec.ReviewScenePath).Replace('\\', '/'));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(spec.DisplayName + " Unit Review");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = spec.UnitId + "_final_prefab";
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            instance.transform.localScale = Vector3.one * spec.VisualScale;
            instance.transform.SetParent(root.transform);
            var demo = instance.GetComponent<TankVisualDemoController>() ?? instance.AddComponent<TankVisualDemoController>();
            demo.rig = instance.GetComponent<TankVisualRigController>();
            demo.orbitRadius = 0.32f;
            demo.orbitSpeed = spec.UnitId == "scout_rover" ? 0.55f : 0.34f;
            demo.fireInterval = spec.UnitId == "scout_rover" ? 1.15f : 1.45f;
            demo.aimSweepDegrees = 48f;

            CreateGround();
            CreateLabel(spec.DisplayName + " production mesh prefab - wheels, turret, barrel, team color, LODs", new Vector3(-4.8f, 0.05f, -3.35f));
            CreateLabel("Playable visual scale: " + spec.VisualScale.ToString("0.00") + " | Source triangles LOD0: " + spec.ExpectedLod0Triangles, new Vector3(-4.8f, 0.05f, 3.35f));
            CreateSocketMarkers(instance);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
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

        static void CreateSocketMarkers(GameObject instance)
        {
            var sockets = instance.GetComponentsInChildren<ActorPrefabSocket>(true);
            for (var i = 0; i < sockets.Length; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Socket Marker " + sockets[i].socketName;
                marker.transform.position = sockets[i].transform.position;
                marker.transform.localScale = Vector3.one * 0.055f;
                var collider = marker.GetComponent<Collider>();
                if (collider != null)
                    UnityEngine.Object.DestroyImmediate(collider);
            }
        }
    }
}
