#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProjectAegisHarvesterPrefabBuilder
{
    private const string UnitId = "harvester";
    private const string Source = "Assets/Rts/Art/Source/Units/harvester";
    private const string Textures = "Assets/Rts/Art/Textures/Units/harvester";
    private const string Materials = "Assets/Rts/Art/Materials/Units/harvester";
    private const string IconPath = "Assets/Rts/Art/Icons/harvester_icon.png";
    private const string PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/harvester";
    private const string PrefabPath = PrefabFolder + "/harvester.prefab";
    private const string VisualDefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/harvester_visual.asset";
    private const string ReviewScenePath = "Assets/Rts/Scenes/harvester_UnitReview.unity";

    private static readonly Vector3 TrackLeftPosition = new Vector3(-1.86f, 0.42f, 0.02f);
    private static readonly Vector3 TrackRightPosition = new Vector3(1.86f, 0.42f, 0.02f);
    private static readonly Vector3 ResourceIntakePosition = new Vector3(0f, 0.72f, 4.18f);
    private static readonly Vector3 CutterDrumPosition = new Vector3(0f, 0.68f, 4.55f);
    private static readonly Vector3 ConveyorPosition = new Vector3(0f, 1.18f, 1.25f);
    private static readonly Vector3 HarvesterDockPosition = new Vector3(0f, 0.90f, -3.58f);
    private static readonly Vector3 SelectionAnchorPosition = new Vector3(0f, 0.05f, 0f);
    private static readonly Vector3 HealthBarAnchorPosition = new Vector3(0f, 3.30f, -0.35f);
    private static readonly Vector3 UiAnchorPosition = new Vector3(0f, 3.50f, -0.35f);
    private static readonly Vector3 VfxSmokePosition = new Vector3(0.62f, 3.08f, -3.05f);
    private static readonly Vector3 VfxExplosionPosition = new Vector3(0f, 1.25f, 0f);
    private static readonly Vector3 OreCargoAnchorPosition = new Vector3(0f, 2.92f, -1.82f);
    private static readonly Vector3 RefineryUnloadVfxPosition = new Vector3(0f, 0.92f, -3.72f);
    private static readonly Vector3 LightRootPosition = new Vector3(0f, 1.05f, 3.38f);
    private static readonly Vector3 MachineGunRootPosition = new Vector3(0f, 2.92f, 0.80f);
    private static readonly Vector3 MuzzleDefenseLocalPosition = new Vector3(0f, 0.11f, 0.84f);

    private static readonly ActorPrefabSocketKind[] RequiredSockets =
    {
        ActorPrefabSocketKind.Root,
        ActorPrefabSocketKind.VisualRoot,
        ActorPrefabSocketKind.BodyRoot,
        ActorPrefabSocketKind.SelectionAnchor,
        ActorPrefabSocketKind.HealthBarAnchor,
        ActorPrefabSocketKind.UiAnchor,
        ActorPrefabSocketKind.MuzzlePrimary,
        ActorPrefabSocketKind.TurretRoot,
        ActorPrefabSocketKind.TrackLeft,
        ActorPrefabSocketKind.TrackRight,
        ActorPrefabSocketKind.HarvesterDock,
        ActorPrefabSocketKind.LightRoot,
        ActorPrefabSocketKind.VfxSmoke,
        ActorPrefabSocketKind.VfxExplosion
    };

    [InitializeOnLoadMethod]
    private static void AutoBuildWhenImported()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null && AssetsReady())
                Build(false, false);
        };
    }

    [MenuItem("ProjectAegisRTS/Art/Build Harvester Prefab")]
    public static void BuildFromMenu()
    {
        Build(true, true);
    }

    public static void BuildBatch()
    {
        Build(false, true);
    }

    private static bool AssetsReady()
    {
        string[] models =
        {
            Part("hull.obj"),
            Part("cab.obj"),
            Part("track_left.obj"),
            Part("track_right.obj"),
            Part("intake_cutter.obj"),
            Part("conveyor.obj"),
            Part("ore_tank.obj"),
            Part("pipes.obj"),
            Part("team_body.obj"),
            Part("team_tank.obj"),
            Part("emissive.obj"),
            Part("glass.obj"),
            Part("bolts.obj"),
            Part("ore_cargo.obj"),
            Part("machine_gun.obj"),
            Part("lod1.obj"),
            Part("lod2.obj")
        };

        for (var i = 0; i < models.Length; i++)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(models[i]) == null)
                return false;

        return AssetDatabase.LoadAssetAtPath<Texture2D>(Texture("albedo.png")) != null;
    }

    private static void Build(bool selectResult, bool createReviewScene)
    {
        AssetDatabase.Refresh();
        ConfigureImportSettings();
        EnsureFolder(Materials);
        EnsureFolder(PrefabFolder);

        var body = Material("body", new Color(0.35f, 0.38f, 0.30f, 1f), false, 0.08f, 0.42f);
        var tracks = Material("tracks", new Color(0.04f, 0.045f, 0.04f, 1f), false, 0.18f, 0.24f);
        var metal = Material("metal", new Color(0.18f, 0.19f, 0.17f, 1f), false, 0.25f, 0.30f);
        var team = Material("team_color", new Color(0.12f, 0.70f, 0.88f, 1f), false, 0.10f, 0.48f);
        var emissive = Material("emissive", new Color(0.11f, 0.82f, 0.72f, 1f), true, 0.0f, 0.60f);
        var glass = Material("glass", new Color(0.08f, 0.42f, 0.48f, 0.78f), true, 0.0f, 0.66f);
        var bolts = Material("bolts", new Color(0.08f, 0.085f, 0.075f, 1f), false, 0.22f, 0.25f);
        var ore = Material("ore_cargo_blue", new Color(0.18f, 0.52f, 1.0f, 1f), true, 0.0f, 0.58f);
        var grinderDrum = Material("grinder_drum", new Color(0.52f, 0.56f, 0.50f, 1f), false, 0.28f, 0.32f);
        var grinderTeeth = Material("grinder_teeth", new Color(0.86f, 0.82f, 0.68f, 1f), false, 0.34f, 0.24f);

        var actor = new GameObject(UnitId);
        ConfigureDescriptor(actor.AddComponent<ActorPrefabDescriptor>(), null);
        var lodGroup = actor.AddComponent<LODGroup>();

        var root = Socket(actor.transform, "Root", ActorPrefabSocketKind.Root, Vector3.zero);
        var visualRoot = Socket(root, "VisualRoot", ActorPrefabSocketKind.VisualRoot, Vector3.zero);
        var bodyRoot = Socket(visualRoot, "BodyRoot", ActorPrefabSocketKind.BodyRoot, Vector3.zero);
        var lod0 = Child(bodyRoot, "LOD0_Meshes", Vector3.zero);
        var lod1 = Child(bodyRoot, "LOD1_Meshes", Vector3.zero);
        var lod2 = Child(bodyRoot, "LOD2_Meshes", Vector3.zero);

        InstantiateModel("HullArmor", Part("hull.obj"), lod0, Vector3.zero, body);
        InstantiateModel("CabArmor", Part("cab.obj"), lod0, Vector3.zero, body);
        InstantiateModel("OreStorageAndProcessing", Part("ore_tank.obj"), lod0, Vector3.zero, body);
        InstantiateModel("PipesAndDockHardware", Part("pipes.obj"), lod0, Vector3.zero, metal);
        InstantiateModel("AttachedBoltsAndSurfaceDetails", Part("bolts.obj"), lod0, Vector3.zero, bolts);
        var teamBody = InstantiateModel("TeamColorPanels_Body", Part("team_body.obj"), lod0, Vector3.zero, team);
        var teamTank = InstantiateModel("TeamColorPanels_Tank", Part("team_tank.obj"), lod0, Vector3.zero, team);
        InstantiateModel("Emissive_WorkLights", Part("emissive.obj"), lod0, Vector3.zero, emissive);
        InstantiateModel("GlassAndOptics", Part("glass.obj"), lod0, Vector3.zero, glass);

        var trackLeft = Socket(visualRoot, "TrackLeft", ActorPrefabSocketKind.TrackLeft, TrackLeftPosition);
        var trackRight = Socket(visualRoot, "TrackRight", ActorPrefabSocketKind.TrackRight, TrackRightPosition);
        var leftTrackMesh = InstantiateModel("TrackLeftMesh", Part("track_left.obj"), trackLeft, -TrackLeftPosition, tracks);
        var rightTrackMesh = InstantiateModel("TrackRightMesh", Part("track_right.obj"), trackRight, -TrackRightPosition, tracks);

        var resourceIntake = NamedHook(visualRoot, "ResourceIntake", ResourceIntakePosition);
        var cutterDrum = NamedHook(visualRoot, "CutterDrumRoot", CutterDrumPosition);
        var conveyor = NamedHook(visualRoot, "ConveyorRoot", ConveyorPosition);
        var oreCargo = NamedHook(visualRoot, "OreCargoAnchor", OreCargoAnchorPosition);
        var refineryUnload = NamedHook(visualRoot, "RefineryUnloadVfx", RefineryUnloadVfxPosition);
        CreateStaticIntakeFrame(resourceIntake, metal, bolts);
        CreateCutterDrumAssembly(cutterDrum, grinderDrum, grinderTeeth);
        InstantiateModel("ConveyorAssembly", Part("conveyor.obj"), conveyor, -ConveyorPosition, tracks);
        InstantiateModel("VisibleOreCargo", Part("ore_cargo.obj"), oreCargo, -OreCargoAnchorPosition, ore);

        var machineGunRoot = Socket(visualRoot, "MachineGunRoot", ActorPrefabSocketKind.TurretRoot, MachineGunRootPosition);
        InstantiateModel("MachineGunDefense", Part("machine_gun.obj"), machineGunRoot, -MachineGunRootPosition, metal);
        Socket(machineGunRoot, "MuzzleDefense", ActorPrefabSocketKind.MuzzlePrimary, MuzzleDefenseLocalPosition);

        Socket(visualRoot, "HarvesterDock", ActorPrefabSocketKind.HarvesterDock, HarvesterDockPosition);
        Socket(root, "SelectionAnchor", ActorPrefabSocketKind.SelectionAnchor, SelectionAnchorPosition);
        Socket(root, "HealthBarAnchor", ActorPrefabSocketKind.HealthBarAnchor, HealthBarAnchorPosition);
        Socket(root, "UiAnchor", ActorPrefabSocketKind.UiAnchor, UiAnchorPosition);
        Socket(visualRoot, "LightRoot", ActorPrefabSocketKind.LightRoot, LightRootPosition);
        Socket(visualRoot, "VfxSmoke", ActorPrefabSocketKind.VfxSmoke, VfxSmokePosition);
        Socket(visualRoot, "VfxExplosion", ActorPrefabSocketKind.VfxExplosion, VfxExplosionPosition);

        InstantiateModel("LOD1_Static", Part("lod1.obj"), lod1, Vector3.zero, body);
        InstantiateModel("LOD2_Static", Part("lod2.obj"), lod2, Vector3.zero, body);
        lod1.gameObject.SetActive(true);
        lod2.gameObject.SetActive(true);

        var harvesterRig = actor.AddComponent<ProjectAegisHarvesterVisualRig>();
        harvesterRig.teamColor = new Color(0.12f, 0.70f, 0.88f, 1f);
        harvesterRig.teamRenderers = Renderers(teamBody, teamTank);
        harvesterRig.cutterDrumRoot = cutterDrum;
        harvesterRig.conveyorRoot = conveyor;
        harvesterRig.trackLeft = trackLeft;
        harvesterRig.trackRight = trackRight;
        harvesterRig.oreCargoAnchor = oreCargo;
        harvesterRig.machineGunRoot = machineGunRoot;
        harvesterRig.cutterSpinDegreesPerSecond = 540f;
        harvesterRig.machineGunIdleSweepDegrees = 7f;
        harvesterRig.SetCargoFill(0.65f);

        var teamRig = actor.AddComponent<ProjectAegisTankTeamColorRig>();
        teamRig.teamColorRenderers = Renderers(teamBody, teamTank);
        teamRig.fallbackTeamColor = new Color(0.12f, 0.70f, 0.88f, 1f);

        var tankRig = actor.AddComponent<TankVisualRigController>();
        tankRig.bodyRoot = bodyRoot;
        tankRig.trackLeftRoot = trackLeft;
        tankRig.trackRightRoot = trackRight;
        tankRig.trackRenderers = Renderers(leftTrackMesh, rightTrackMesh);
        tankRig.trackScrollUnitsPerMeter = 1.15f;
        tankRig.wheelRotationDegreesPerMeter = 0f;
        tankRig.suspensionBobMeters = 0.018f;
        tankRig.estimateMotionFromTransform = true;

        var collider = actor.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 1.56f, 0.08f);
        collider.size = new Vector3(4.66f, 3.13f, 8.46f);

        ConfigureValidationTag(actor.AddComponent<ProductionVisualValidationTag>());
        ConfigureLods(lodGroup, lod0, lod1, lod2, trackLeft, trackRight, resourceIntake, cutterDrum, conveyor, oreCargo, machineGunRoot);

        PrefabUtility.SaveAsPrefabAsset(actor, PrefabPath);
        UnityEngine.Object.DestroyImmediate(actor);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        ConfigureVisualDefinition(prefab);
        ConfigureSavedDescriptor(prefab);
        if (createReviewScene)
            CreateReviewScene(prefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (selectResult)
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        Debug.Log("Built ProjectAegisRTS harvester prefab at " + PrefabPath);
    }

    private static void ConfigureLods(LODGroup lodGroup, params Transform[] roots)
    {
        var lod0Renderers = new List<Renderer>();
        for (var i = 0; i < roots.Length; i++)
        {
            if (roots[i] == null || roots[i].name.StartsWith("LOD1_", StringComparison.Ordinal) || roots[i].name.StartsWith("LOD2_", StringComparison.Ordinal))
                continue;
            lod0Renderers.AddRange(roots[i].GetComponentsInChildren<Renderer>(true));
        }

        var lod1 = roots.FirstOrDefault(t => t != null && t.name == "LOD1_Meshes");
        var lod2 = roots.FirstOrDefault(t => t != null && t.name == "LOD2_Meshes");
        lodGroup.SetLODs(new[]
        {
            new LOD(0.30f, lod0Renderers.Distinct().ToArray()),
            new LOD(0.09f, lod1 == null ? Array.Empty<Renderer>() : lod1.GetComponentsInChildren<Renderer>(true)),
            new LOD(0.025f, lod2 == null ? Array.Empty<Renderer>() : lod2.GetComponentsInChildren<Renderer>(true)),
            new LOD(0.006f, Array.Empty<Renderer>())
        });
        lodGroup.RecalculateBounds();
    }

    private static Transform Child(Transform parent, string name, Vector3 localPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private static Transform NamedHook(Transform parent, string name, Vector3 localPosition)
    {
        return Child(parent, name, localPosition);
    }

    private static Transform Socket(Transform parent, string name, ActorPrefabSocketKind kind, Vector3 localPosition)
    {
        var transform = Child(parent, name, localPosition);
        AddSocketComponent(transform.gameObject, name, kind);
        return transform;
    }

    private static GameObject InstantiateModel(string name, string path, Transform parent, Vector3 localPosition, Material material)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (source == null)
            throw new FileNotFoundException("Missing harvester model asset: " + path);

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            var count = Mathf.Max(1, renderer.sharedMaterials.Length);
            var mats = new Material[count];
            for (var i = 0; i < count; i++)
                mats[i] = material;
            renderer.sharedMaterials = mats;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        return instance;
    }

    private static Renderer[] Renderers(params GameObject[] objects)
    {
        return objects
            .Where(go => go != null)
            .SelectMany(go => go.GetComponentsInChildren<Renderer>(true))
            .Where(renderer => renderer != null)
            .Distinct()
            .ToArray();
    }

    private static void CreateCutterDrumAssembly(Transform parent, Material drumMaterial, Material toothMaterial)
    {
        var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        drum.name = "AnimatedCutterDrum";
        drum.transform.SetParent(parent, false);
        drum.transform.localPosition = Vector3.zero;
        drum.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        drum.transform.localScale = new Vector3(0.36f, 2.28f, 0.36f);
        SetPrimitiveMaterial(drum, drumMaterial);

        const int toothCount = 18;
        for (var i = 0; i < toothCount; i++)
        {
            var angle = i * Mathf.PI * 2f / toothCount;
            var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tooth.name = "CutterTooth_" + i.ToString("00");
            tooth.transform.SetParent(parent, false);
            tooth.transform.localPosition = new Vector3(0f, Mathf.Sin(angle) * 0.43f, Mathf.Cos(angle) * 0.43f);
            tooth.transform.localRotation = Quaternion.Euler(i * 360f / toothCount, 0f, 0f);
            tooth.transform.localScale = new Vector3(2.64f, 0.065f, 0.15f);
            SetPrimitiveMaterial(tooth, toothMaterial);
        }

        var leftCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftCap.name = "CutterAxleLeftCap";
        leftCap.transform.SetParent(parent, false);
        leftCap.transform.localPosition = new Vector3(-1.38f, 0f, 0f);
        leftCap.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        leftCap.transform.localScale = new Vector3(0.45f, 0.10f, 0.45f);
        SetPrimitiveMaterial(leftCap, toothMaterial);

        var rightCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightCap.name = "CutterAxleRightCap";
        rightCap.transform.SetParent(parent, false);
        rightCap.transform.localPosition = new Vector3(1.38f, 0f, 0f);
        rightCap.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        rightCap.transform.localScale = new Vector3(0.45f, 0.10f, 0.45f);
        SetPrimitiveMaterial(rightCap, toothMaterial);
    }

    private static void CreateStaticIntakeFrame(Transform parent, Material frameMaterial, Material accentMaterial)
    {
        CreateBox("IntakeLeftSideArm", parent, new Vector3(-1.55f, 0.00f, 0.12f), new Vector3(0.16f, 0.25f, 1.34f), frameMaterial);
        CreateBox("IntakeRightSideArm", parent, new Vector3(1.55f, 0.00f, 0.12f), new Vector3(0.16f, 0.25f, 1.34f), frameMaterial);
        CreateBox("IntakeRearCrossbar", parent, new Vector3(0f, 0.16f, -0.50f), new Vector3(3.28f, 0.17f, 0.14f), frameMaterial);
        CreateBox("IntakeLowerSkid", parent, new Vector3(0f, -0.30f, 0.24f), new Vector3(3.00f, 0.10f, 1.18f), frameMaterial);
        CreateBox("IntakeLeftGuardPlate", parent, new Vector3(-1.45f, 0.36f, 0.30f), new Vector3(0.18f, 0.48f, 0.84f), accentMaterial);
        CreateBox("IntakeRightGuardPlate", parent, new Vector3(1.45f, 0.36f, 0.30f), new Vector3(0.18f, 0.48f, 0.84f), accentMaterial);
        CreateBox("IntakeTopBrace", parent, new Vector3(0f, 0.55f, -0.02f), new Vector3(2.86f, 0.08f, 0.12f), frameMaterial);
    }

    private static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPosition;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = localScale;
        SetPrimitiveMaterial(box, material);
        return box;
    }

    private static void SetPrimitiveMaterial(GameObject go, Material material)
    {
        var collider = go.GetComponent<Collider>();
        if (collider != null)
            UnityEngine.Object.DestroyImmediate(collider);

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
    }

    private static void ConfigureDescriptor(ActorPrefabDescriptor descriptor, ActorVisualDefinition definition)
    {
        if (descriptor == null)
            return;

        descriptor.actorTypeId = UnitId;
        descriptor.category = ActorArtCategory.Vehicle;
        descriptor.sourceDefinition = definition;
        descriptor.productionStatus = ActorArtProductionStatus.Animated;
        descriptor.generatedByStage8 = false;
        descriptor.requiredSocketsPresent = true;
        descriptor.declaredRequiredSockets.Clear();
        descriptor.declaredRequiredSockets.AddRange(RequiredSockets);
        descriptor.notes = "Harvester production art imported from the Defense MG Unity asset packet. Gameplay values remain sourced from Rts.Core DemoRules.";
    }

    private static void ConfigureValidationTag(ProductionVisualValidationTag tag)
    {
        if (tag == null)
            return;

        tag.actorTypeId = UnitId;
        tag.visualTier = ProductionVisualTier.ProductionModel;
        tag.hasTopDetail = true;
        tag.hasFrontDetail = true;
        tag.hasBackDetail = true;
        tag.hasLeftDetail = true;
        tag.hasRightDetail = true;
        tag.hasRoofDetail = true;
        tag.hasBeveledOrTieredForm = true;
        tag.hasGridAccurateBase = true;
        tag.hasLodGroup = true;
        tag.hasStage21ReadabilityPass = true;
        tag.hasArtistReplacementMetadata = true;
        tag.hasSocketScaffold = true;
        tag.replacementNotes = "Packet mesh prefab with separate hull, cab, tracks, cutter, conveyor, ore tank, team-color panels, glass, emissives, and defense MG.";
        tag.notes = "Production harvester visual packet imported without touching Rts.Core gameplay.";
    }

    private static void AddSocketComponent(GameObject go, string socketName, ActorPrefabSocketKind kind)
    {
        var socket = go.GetComponent<ActorPrefabSocket>() ?? go.AddComponent<ActorPrefabSocket>();
        socket.socketKind = kind;
        socket.socketName = socketName;
        socket.actorTypeId = UnitId;
        socket.notes = "Harvester packet socket: " + socketName;
    }

    private static void ConfigureSavedDescriptor(GameObject prefab)
    {
        if (prefab == null)
            return;

        var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(VisualDefinitionPath);
        var descriptor = prefab.GetComponent<ActorPrefabDescriptor>();
        ConfigureDescriptor(descriptor, definition);
        if (descriptor != null)
        {
            descriptor.ValidateRequiredSockets(RequiredSockets);
            EditorUtility.SetDirty(descriptor);
        }

        var tag = prefab.GetComponent<ProductionVisualValidationTag>();
        ConfigureValidationTag(tag);
        if (tag != null)
            EditorUtility.SetDirty(tag);
    }

    private static void ConfigureVisualDefinition(GameObject prefab)
    {
        EnsureFolder(Path.GetDirectoryName(VisualDefinitionPath).Replace('\\', '/'));
        var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(VisualDefinitionPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<ActorVisualDefinition>();
            AssetDatabase.CreateAsset(definition, VisualDefinitionPath);
        }

        definition.actorTypeId = UnitId;
        definition.displayName = "Harvester";
        definition.safeDisplayName = "Harvester";
        definition.category = ActorArtCategory.Vehicle;
        definition.productionStatus = ActorArtProductionStatus.Animated;
        definition.icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
        definition.productionPrefab = prefab;
        definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
        definition.motionProfileId = "tracked_harvester";
        definition.selectionRadius = 2.80f;
        definition.selectionHeight = 3.45f;
        definition.footprintWidth = Math.Max(6, definition.footprintWidth);
        definition.footprintHeight = Math.Max(8, definition.footprintHeight);
        definition.visualScale = 0.34f;
        definition.prefabHeightOffset = 0f;
        definition.useBuildingVisualController = false;
        definition.useVehicleMotionController = true;
        definition.useInfantryMotionController = false;
        definition.useAircraftMotionController = false;
        definition.useTurretVisualController = false;
        definition.requiredSockets.Clear();
        definition.requiredSockets.AddRange(RequiredSockets);
        definition.ipReviewRequired = false;
        definition.notes = "Production visual uses the ProjectAegis harvester Defense MG Unity asset packet. Rts.Core gameplay remains unchanged.";
        EditorUtility.SetDirty(definition);
    }

    private static Material Material(string suffix, Color baseColor, bool emission, float metallic, float smoothness)
    {
        var path = Materials + "/mat_harvester_" + suffix + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (material == null)
        {
            material = new Material(shader) { name = "mat_harvester_" + suffix };
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture("albedo.png"));
        var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture("normal.png"));
        var orm = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture("orm.png"));
        var emissionMap = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture("emission.png"));
        SetTex(material, "_BaseMap", albedo);
        SetTex(material, "_MainTex", albedo);
        SetTex(material, "_BumpMap", normal);
        SetTex(material, "_OcclusionMap", orm);
        SetColor(material, "_BaseColor", baseColor);
        SetColor(material, "_Color", baseColor);
        SetFloat(material, "_Metallic", metallic);
        SetFloat(material, "_Smoothness", smoothness);
        SetFloat(material, "_Glossiness", smoothness);
        material.EnableKeyword("_NORMALMAP");
        if (emission)
        {
            SetTex(material, "_EmissionMap", emissionMap);
            SetColor(material, "_EmissionColor", baseColor * 2.4f);
            material.EnableKeyword("_EMISSION");
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }

        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetTex(Material material, string prop, Texture texture)
    {
        if (material.HasProperty(prop))
            material.SetTexture(prop, texture);
    }

    private static void SetColor(Material material, string prop, Color color)
    {
        if (material.HasProperty(prop))
            material.SetColor(prop, color);
    }

    private static void SetFloat(Material material, string prop, float value)
    {
        if (material.HasProperty(prop))
            material.SetFloat(prop, value);
    }

    private static void ConfigureImportSettings()
    {
        ConfigureTexture(Texture("albedo.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(Texture("normal.png"), TextureImporterType.NormalMap, false, true);
        ConfigureTexture(Texture("orm.png"), TextureImporterType.Default, false, true);
        ConfigureTexture(Texture("emission.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(Texture("team_mask.png"), TextureImporterType.Default, false, true);
        ConfigureTexture(Texture("resource_dust_sheet.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(IconPath, TextureImporterType.Sprite, true, false);

        foreach (var objPath in Directory.GetFiles(Path.Combine(Application.dataPath, "Rts/Art/Source/Units/harvester"), "*.obj"))
        {
            var assetPath = "Assets" + objPath.Substring(Application.dataPath.Length).Replace('\\', '/');
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer != null)
            {
                importer.globalScale = 1f;
                importer.SaveAndReimport();
            }
            else
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    private static void ConfigureTexture(string path, TextureImporterType type, bool srgb, bool mipmaps)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = type;
        importer.sRGBTexture = srgb;
        importer.mipmapEnabled = mipmaps;
        importer.alphaIsTransparency = path.EndsWith("_icon.png", StringComparison.OrdinalIgnoreCase) || path.Contains("_dust_", StringComparison.Ordinal);
        importer.SaveAndReimport();
    }

    private static void CreateReviewScene(GameObject prefab)
    {
        if (prefab == null)
            return;

        EnsureFolder(Path.GetDirectoryName(ReviewScenePath).Replace('\\', '/'));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Harvester Unit Review");

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "harvester_final_prefab";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.Euler(0f, 32f, 0f);
        instance.transform.SetParent(root.transform);

        CreateGround();
        CreateLabel("Harvester final mesh prefab - press Play for cutter and MG idle motion", new Vector3(-5.7f, 0.05f, -4.2f));
        CreateLabel("Hooks: ResourceIntake, CutterDrumRoot, ConveyorRoot, HarvesterDock, OreCargoAnchor", new Vector3(-5.7f, 0.05f, 4.2f));
        CreateSocketMarkers(instance);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.22f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 12f);

        var fillGo = new GameObject("Fill Light");
        var fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.range = 9f;
        fill.intensity = 0.8f;
        fill.color = new Color(0.48f, 0.70f, 0.88f);
        fillGo.transform.position = new Vector3(-3.5f, 3.0f, -4.2f);

        var cameraGo = new GameObject("Main Camera");
        var camera = cameraGo.AddComponent<Camera>();
        cameraGo.tag = "MainCamera";
        cameraGo.transform.position = new Vector3(7.4f, 5.5f, -9.2f);
        cameraGo.transform.rotation = Quaternion.Euler(55f, -38f, 0f);
        camera.fieldOfView = 36f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.10f, 0.13f, 0.13f);

        EditorSceneManager.SaveScene(scene, ReviewScenePath);
    }

    private static void CreateGround()
    {
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Review Ground";
        plane.transform.localScale = new Vector3(1.7f, 1f, 1.7f);
        var renderer = plane.GetComponent<Renderer>();
        if (renderer == null)
            return;

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        material.color = new Color(0.21f, 0.27f, 0.23f);
        renderer.sharedMaterial = material;
    }

    private static void CreateSocketMarkers(GameObject instance)
    {
        var descriptor = instance.GetComponent<ActorPrefabDescriptor>();
        if (descriptor == null)
            return;

        for (var i = 0; i < RequiredSockets.Length; i++)
        {
            Transform socket;
            if (!descriptor.TryGetSocket(RequiredSockets[i], out socket) || socket == null)
                continue;

            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Socket Marker " + RequiredSockets[i];
            marker.transform.position = socket.position;
            marker.transform.localScale = Vector3.one * 0.095f;
            var collider = marker.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static void CreateLabel(string text, Vector3 position)
    {
        var label = new GameObject("Label");
        label.transform.position = position;
        label.transform.rotation = Quaternion.Euler(65f, 0f, 0f);
        var mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.anchor = TextAnchor.MiddleLeft;
        mesh.characterSize = 0.16f;
        mesh.color = Color.white;
    }

    private static string Part(string suffix)
    {
        return Source + "/" + UnitId + "_" + suffix;
    }

    private static string Texture(string suffix)
    {
        return Textures + "/" + UnitId + "_" + suffix;
    }

    private static void EnsureFolder(string path)
    {
        path = path.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        var name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
