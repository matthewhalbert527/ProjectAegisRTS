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

public static class ProjectAegisMediumTankPrefabBuilder
{
    [InitializeOnLoadMethod]
    static void AutoBuildWhenImported()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ProjectAegisTankPacketPrefabBuilder.Medium.PrefabPath) == null &&
                ProjectAegisTankPacketPrefabBuilder.AssetsReady(ProjectAegisTankPacketPrefabBuilder.Medium))
                ProjectAegisTankPacketPrefabBuilder.Build(ProjectAegisTankPacketPrefabBuilder.Medium, false, false);
        };
    }

    [MenuItem("ProjectAegisRTS/Art/Build Medium Tank Prefab")]
    public static void BuildFromMenu()
    {
        ProjectAegisTankPacketPrefabBuilder.Build(ProjectAegisTankPacketPrefabBuilder.Medium, true, true);
    }

    public static void BuildBatch()
    {
        ProjectAegisTankPacketPrefabBuilder.Build(ProjectAegisTankPacketPrefabBuilder.Medium, false, true);
    }
}

public static class ProjectAegisHeavyTankPrefabBuilder
{
    [InitializeOnLoadMethod]
    static void AutoBuildWhenImported()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ProjectAegisTankPacketPrefabBuilder.Heavy.PrefabPath) == null &&
                ProjectAegisTankPacketPrefabBuilder.AssetsReady(ProjectAegisTankPacketPrefabBuilder.Heavy))
                ProjectAegisTankPacketPrefabBuilder.Build(ProjectAegisTankPacketPrefabBuilder.Heavy, false, false);
        };
    }

    [MenuItem("ProjectAegisRTS/Art/Build Heavy Tank Prefab")]
    public static void BuildFromMenu()
    {
        ProjectAegisTankPacketPrefabBuilder.Build(ProjectAegisTankPacketPrefabBuilder.Heavy, true, true);
    }

    public static void BuildBatch()
    {
        ProjectAegisTankPacketPrefabBuilder.Build(ProjectAegisTankPacketPrefabBuilder.Heavy, false, true);
    }
}

public static class ProjectAegisTankPacketPrefabBuilder
{
    public sealed class TankSpec
    {
        public string UnitId;
        public string DisplayName;
        public string MotionProfileId;
        public string Source;
        public string Textures;
        public string Materials;
        public string IconPath;
        public string PrefabFolder;
        public string PrefabPath;
        public string VisualDefinitionPath;
        public string ReviewScenePath;
        public Vector3 TrackLeft;
        public Vector3 TrackRight;
        public Vector3 TurretRoot;
        public Vector3 BarrelRoot;
        public Vector3 MuzzlePrimary;
        public Vector3 MuzzleSecondary;
        public Vector3 SelectionAnchor;
        public Vector3 HealthBarAnchor;
        public Vector3 UiAnchor;
        public Vector3 VfxSmoke;
        public Vector3 VfxExplosion;
        public Vector3 ColliderCenter;
        public Vector3 ColliderSize;
        public float SelectionRadius;
        public float SelectionHeight;
        public int FootprintWidth;
        public int FootprintHeight;
        public float TrackScrollUnitsPerMeter;
        public float WheelRotationDegreesPerMeter;
        public float BarrelRecoilDistance;
        public float VisualScale = 1f;
        public bool HasTwinBarrels;
        public bool UseSingleTwinBarrelMesh;
        public float TwinBarrelHalfSpacing;
        public bool HasSquaredTurretArmor;
        public bool HasMissileLauncher;
        public Vector3 MissileLauncherRoot;
        public Vector3[] MissileMuzzles;
        public Color BodyColor;
        public Color TeamColor;
        public Color EmissiveColor;
        public Color GlassColor;
    }

    public static readonly TankSpec Medium = new TankSpec
    {
        UnitId = "medium_tank",
        DisplayName = "Medium Tank",
        MotionProfileId = "tracked_medium",
        Source = "Assets/Rts/Art/Source/Units/medium_tank",
        Textures = "Assets/Rts/Art/Textures/Units/medium_tank",
        Materials = "Assets/Rts/Art/Materials/Units/medium_tank",
        IconPath = "Assets/Rts/Art/Icons/medium_tank_icon.png",
        PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/medium_tank",
        PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/medium_tank/medium_tank.prefab",
        VisualDefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/medium_tank_visual.asset",
        ReviewScenePath = "Assets/Rts/Scenes/medium_tank_UnitReview.unity",
        TrackLeft = new Vector3(-1.34f, 0.58f, 0.00f),
        TrackRight = new Vector3(1.34f, 0.58f, 0.00f),
        TurretRoot = new Vector3(0.00f, 1.52f, 0.12f),
        BarrelRoot = new Vector3(0.00f, 0.12f, 1.02f),
        MuzzlePrimary = new Vector3(0.00f, 0.00f, 3.88f),
        SelectionAnchor = new Vector3(0.00f, 0.05f, 0.00f),
        HealthBarAnchor = new Vector3(0.00f, 3.00f, 0.00f),
        UiAnchor = new Vector3(0.00f, 2.78f, -0.15f),
        VfxSmoke = new Vector3(1.05f, 1.62f, -2.20f),
        VfxExplosion = new Vector3(0.00f, 1.18f, 0.00f),
        ColliderCenter = new Vector3(0f, 1.35f, 0f),
        ColliderSize = new Vector3(3.58f, 2.70f, 5.40f),
        SelectionRadius = 1.95f,
        SelectionHeight = 2.95f,
        FootprintWidth = 5,
        FootprintHeight = 4,
        TrackScrollUnitsPerMeter = 2.15f,
        WheelRotationDegreesPerMeter = 610f,
        BarrelRecoilDistance = 0.10f,
        VisualScale = 0.36f,
        BodyColor = new Color(0.34f, 0.38f, 0.27f, 1f),
        TeamColor = new Color(0.12f, 0.70f, 0.88f, 1f),
        EmissiveColor = new Color(1.0f, 0.48f, 0.10f, 1f),
        GlassColor = new Color(0.13f, 0.62f, 0.70f, 0.75f)
    };

    public static readonly TankSpec Heavy = new TankSpec
    {
        UnitId = "heavy_tank",
        DisplayName = "Heavy Tank",
        MotionProfileId = "tracked_heavy",
        Source = "Assets/Rts/Art/Source/Units/heavy_tank",
        Textures = "Assets/Rts/Art/Textures/Units/heavy_tank",
        Materials = "Assets/Rts/Art/Materials/Units/heavy_tank",
        IconPath = "Assets/Rts/Art/Icons/heavy_tank_icon.png",
        PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/heavy_tank",
        PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/heavy_tank/heavy_tank.prefab",
        VisualDefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/heavy_tank_visual.asset",
        ReviewScenePath = "Assets/Rts/Scenes/heavy_tank_UnitReview.unity",
        TrackLeft = new Vector3(-1.83f, 0.55f, 0.10f),
        TrackRight = new Vector3(1.83f, 0.55f, 0.10f),
        TurretRoot = new Vector3(0.00f, 1.74f, 0.78f),
        BarrelRoot = new Vector3(0.00f, 0.12f, 1.12f),
        MuzzlePrimary = new Vector3(-0.42f, 0.00f, 4.92f),
        MuzzleSecondary = new Vector3(0.42f, 0.00f, 4.92f),
        SelectionAnchor = new Vector3(0.00f, 0.05f, 0.10f),
        HealthBarAnchor = new Vector3(0.00f, 4.20f, 0.10f),
        UiAnchor = new Vector3(0.00f, 3.95f, -0.20f),
        VfxSmoke = new Vector3(1.75f, 1.92f, -2.75f),
        VfxExplosion = new Vector3(0.00f, 1.35f, 0.10f),
        ColliderCenter = new Vector3(0f, 1.70f, 0.18f),
        ColliderSize = new Vector3(4.72f, 3.38f, 6.92f),
        SelectionRadius = 2.75f,
        SelectionHeight = 4.20f,
        FootprintWidth = 6,
        FootprintHeight = 5,
        TrackScrollUnitsPerMeter = 1.75f,
        WheelRotationDegreesPerMeter = 540f,
        BarrelRecoilDistance = 0.18f,
        VisualScale = 0.38f,
        HasTwinBarrels = true,
        UseSingleTwinBarrelMesh = true,
        TwinBarrelHalfSpacing = 0.42f,
        HasSquaredTurretArmor = false,
        HasMissileLauncher = true,
        MissileLauncherRoot = new Vector3(1.10f, 1.86f, -1.72f),
        MissileMuzzles = new[]
        {
            new Vector3(-0.63f, 0.72f, 0.83f),
            new Vector3(-0.21f, 0.72f, 0.83f),
            new Vector3(0.21f, 0.72f, 0.83f),
            new Vector3(0.63f, 0.72f, 0.83f),
            new Vector3(-0.63f, 1.08f, 0.83f),
            new Vector3(-0.21f, 1.08f, 0.83f),
            new Vector3(0.21f, 1.08f, 0.83f),
            new Vector3(0.63f, 1.08f, 0.83f)
        },
        BodyColor = new Color(0.31f, 0.35f, 0.25f, 1f),
        TeamColor = new Color(0.12f, 0.70f, 0.88f, 1f),
        EmissiveColor = new Color(1.0f, 0.48f, 0.10f, 1f),
        GlassColor = new Color(0.13f, 0.62f, 0.70f, 0.75f)
    };

    static readonly ActorPrefabSocketKind[] RequiredSockets =
    {
        ActorPrefabSocketKind.Root,
        ActorPrefabSocketKind.VisualRoot,
        ActorPrefabSocketKind.BodyRoot,
        ActorPrefabSocketKind.SelectionAnchor,
        ActorPrefabSocketKind.HealthBarAnchor,
        ActorPrefabSocketKind.UiAnchor,
        ActorPrefabSocketKind.MuzzlePrimary,
        ActorPrefabSocketKind.TurretRoot,
        ActorPrefabSocketKind.BarrelRoot,
        ActorPrefabSocketKind.TrackLeft,
        ActorPrefabSocketKind.TrackRight,
        ActorPrefabSocketKind.VfxSmoke,
        ActorPrefabSocketKind.VfxExplosion
    };

    public static bool AssetsReady(TankSpec spec)
    {
        var models = new List<string>
        {
            Part(spec, "hull.obj"),
            Part(spec, "track_left.obj"),
            Part(spec, "track_right.obj"),
            Part(spec, "turret.obj"),
            Part(spec, "barrel.obj"),
            Part(spec, "team_body.obj"),
            Part(spec, "team_turret.obj"),
            Part(spec, "emissive_body.obj"),
            Part(spec, "emissive_turret.obj"),
            Part(spec, "glass_body.obj"),
            Part(spec, "bolts.obj"),
            Part(spec, "lod1.obj"),
            Part(spec, "lod2.obj")
        };
        if (spec.HasMissileLauncher)
        {
            models.Add(Part(spec, "missile_pod.obj"));
            models.Add(Part(spec, "missile_tubes.obj"));
            models.Add(Part(spec, "team_missile_pod.obj"));
            models.Add(Part(spec, "emissive_missile_pod.obj"));
            models.Add(Part(spec, "missile_pod_bolts.obj"));
            models.Add(Part(spec, "orange_accents_body.obj"));
            models.Add(Part(spec, "orange_accents_turret.obj"));
            models.Add(Part(spec, "glass_turret.obj"));
            models.Add(Part(spec, "turret_bolts.obj"));
        }

        for (var i = 0; i < models.Count; i++)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(models[i]) == null)
                return false;

        return AssetDatabase.LoadAssetAtPath<Texture2D>(Texture(spec, "albedo.png")) != null;
    }

    public static void Build(TankSpec spec, bool selectResult, bool createReviewScene)
    {
        ConfigureImportSettings(spec);
        EnsureFolder(spec.Materials);
        EnsureFolder(spec.PrefabFolder);

        var body = Material(spec, "body", spec.BodyColor, false, 0.10f, 0.42f);
        var tracks = Material(spec, "tracks", new Color(0.03f, 0.04f, 0.04f, 1f), false, 0.15f, 0.28f);
        var weapon = Material(spec, "weapon", new Color(0.18f, 0.19f, 0.18f, 1f), false, 0.18f, 0.34f);
        var team = Material(spec, "team_color", spec.TeamColor, false, 0.08f, 0.48f);
        var emissive = Material(spec, "emissive", spec.EmissiveColor, true, 0.0f, 0.55f);
        var emissiveCyan = spec.HasMissileLauncher ? Material(spec, "emissive_cyan", new Color(0.05f, 0.70f, 0.78f, 1f), true, 0.0f, 0.58f) : emissive;
        var emissiveOrange = spec.HasMissileLauncher ? Material(spec, "emissive_orange", new Color(1.0f, 0.32f, 0.08f, 1f), true, 0.0f, 0.58f) : emissive;
        var glass = Material(spec, "glass", spec.GlassColor, true, 0.0f, 0.64f);
        var bolts = Material(spec, "bolts", new Color(0.09f, 0.10f, 0.09f, 1f), false, 0.12f, 0.30f);
        var muzzle = Material(spec, "muzzle_flash", new Color(1.0f, 0.58f, 0.18f, 1f), true, 0.0f, 0.35f);

        var actor = new GameObject(spec.UnitId);
        ConfigureDescriptor(actor.AddComponent<ActorPrefabDescriptor>(), spec, null);
        var lodGroup = actor.AddComponent<LODGroup>();

        var root = Socket(actor.transform, "Root", Vector3.zero, spec);
        var visualRoot = Socket(root, "VisualRoot", Vector3.zero, spec);
        var bodyRoot = Socket(visualRoot, "BodyRoot", Vector3.zero, spec);
        var lod0 = Child(bodyRoot, "LOD0_Meshes", Vector3.zero);
        var lod1 = Child(bodyRoot, "LOD1_Meshes", Vector3.zero);
        var lod2 = Child(bodyRoot, "LOD2_Meshes", Vector3.zero);

        var hull = InstantiateModel("HullArmor", Part(spec, "hull.obj"), lod0, Vector3.zero, body);
        var teamBody = InstantiateModel("TeamColorPanels_Body", Part(spec, "team_body.obj"), lod0, Vector3.zero, team);
        var emissiveBody = InstantiateModel(spec.HasMissileLauncher ? "Emissive_Body_Cyan" : "Emissive_Body", Part(spec, "emissive_body.obj"), lod0, Vector3.zero, spec.HasMissileLauncher ? emissiveCyan : emissive);
        var glassBody = InstantiateModel("Glass_Body", Part(spec, "glass_body.obj"), lod0, Vector3.zero, glass);
        var boltMesh = InstantiateModel("BoltsAndFasteners", Part(spec, "bolts.obj"), lod0, Vector3.zero, bolts);
        if (spec.HasMissileLauncher)
            InstantiateModel("OrangeAccents_Body", Part(spec, "orange_accents_body.obj"), lod0, Vector3.zero, emissiveOrange);

        var trackLeft = Socket(visualRoot, "TrackLeft", spec.TrackLeft, spec);
        var trackRight = Socket(visualRoot, "TrackRight", spec.TrackRight, spec);
        var left = InstantiateModel("TrackLeftMesh", Part(spec, "track_left.obj"), trackLeft, Vector3.zero, tracks);
        var right = InstantiateModel("TrackRightMesh", Part(spec, "track_right.obj"), trackRight, Vector3.zero, tracks);

        var turret = Socket(visualRoot, "TurretRoot", spec.TurretRoot, spec);
        var turretMesh = InstantiateModel("TurretArmor", Part(spec, "turret.obj"), turret, Vector3.zero, body);
        if (spec.HasSquaredTurretArmor)
            CreateSquaredTurretArmor(turret, body);
        var teamTurret = InstantiateModel("TeamColorPanels_Turret", Part(spec, "team_turret.obj"), turret, Vector3.zero, team);
        var emissiveTurret = InstantiateModel(spec.HasMissileLauncher ? "Emissive_Turret_Cyan" : "Emissive_Turret", Part(spec, "emissive_turret.obj"), turret, Vector3.zero, spec.HasMissileLauncher ? emissiveCyan : emissive);
        if (spec.HasMissileLauncher)
        {
            InstantiateModel("Glass_Turret", Part(spec, "glass_turret.obj"), turret, Vector3.zero, glass);
            InstantiateModel("TurretBolts", Part(spec, "turret_bolts.obj"), turret, Vector3.zero, bolts);
            InstantiateModel("OrangeAccents_Turret", Part(spec, "orange_accents_turret.obj"), turret, Vector3.zero, emissiveOrange);
        }
        var barrel = Socket(turret, "BarrelRoot", spec.BarrelRoot, spec);
        if (spec.HasTwinBarrels && spec.UseSingleTwinBarrelMesh)
        {
            InstantiateModel("TwinBarrelMesh", Part(spec, "barrel.obj"), barrel, Vector3.zero, weapon);
        }
        else if (spec.HasTwinBarrels)
        {
            InstantiateModel("BarrelLeftMesh", Part(spec, "barrel.obj"), barrel, new Vector3(-spec.TwinBarrelHalfSpacing, 0f, 0f), weapon);
            InstantiateModel("BarrelRightMesh", Part(spec, "barrel.obj"), barrel, new Vector3(spec.TwinBarrelHalfSpacing, 0f, 0f), weapon);
        }
        else
        {
            InstantiateModel("BarrelMesh", Part(spec, "barrel.obj"), barrel, Vector3.zero, weapon);
        }
        var muzzlePrimary = Socket(barrel, "MuzzlePrimary", spec.MuzzlePrimary, spec);
        var muzzleFlash = CreateMuzzleFlash(muzzlePrimary, muzzle);
        Transform muzzleFlashSecondary = null;
        if (spec.HasTwinBarrels)
        {
            var muzzleSecondary = Socket(barrel, "MuzzleSecondary", spec.MuzzleSecondary, spec);
            muzzleFlashSecondary = CreateMuzzleFlash(muzzleSecondary, muzzle);
            muzzleFlashSecondary.name = "MuzzleFlashSecondary";
        }

        Transform missileLauncher = null;
        GameObject teamMissilePod = null;
        if (spec.HasMissileLauncher)
        {
            missileLauncher = Child(turret, "MissileLauncherRoot", spec.MissileLauncherRoot - spec.TurretRoot);
            CreateMissileLauncherCase(missileLauncher, body, team, bolts);
            InstantiateModel("MissileTubes", Part(spec, "missile_tubes.obj"), missileLauncher, Vector3.zero, weapon);
            teamMissilePod = InstantiateModel("TeamColorPanels_MissilePod", Part(spec, "team_missile_pod.obj"), missileLauncher, Vector3.zero, team);
            InstantiateModel("Emissive_MissilePod_Orange", Part(spec, "emissive_missile_pod.obj"), missileLauncher, Vector3.zero, emissiveOrange);
            InstantiateModel("MissilePodBolts", Part(spec, "missile_pod_bolts.obj"), missileLauncher, Vector3.zero, bolts);
            for (var i = 0; i < spec.MissileMuzzles.Length; i++)
                Child(missileLauncher, "MuzzleMissile" + (i + 1).ToString("00"), spec.MissileMuzzles[i]);
        }

        Socket(root, "SelectionAnchor", spec.SelectionAnchor, spec);
        Socket(root, "HealthBarAnchor", spec.HealthBarAnchor, spec);
        Socket(root, "UiAnchor", spec.UiAnchor, spec);
        Socket(visualRoot, "VfxSmoke", spec.VfxSmoke, spec);
        Socket(visualRoot, "VfxExplosion", spec.VfxExplosion, spec);

        InstantiateModel("LOD1_Static", Part(spec, "lod1.obj"), lod1, Vector3.zero, body);
        InstantiateModel("LOD2_Static", Part(spec, "lod2.obj"), lod2, Vector3.zero, body);

        var teamRig = actor.AddComponent<ProjectAegisTankTeamColorRig>();
        teamRig.teamColorRenderers = TeamRenderers(teamBody, teamTurret, teamMissilePod);
        teamRig.fallbackTeamColor = spec.TeamColor;

        var tankRig = actor.AddComponent<TankVisualRigController>();
        tankRig.bodyRoot = bodyRoot;
        tankRig.turretRoot = turret;
        tankRig.barrelRoot = barrel;
        tankRig.trackLeftRoot = trackLeft;
        tankRig.trackRightRoot = trackRight;
        tankRig.trackRenderers = new[] { GetRenderer(left), GetRenderer(right) }.Where(r => r != null).ToArray();
        tankRig.muzzleFlashRoot = muzzleFlash;
        tankRig.muzzleFlashLight = muzzleFlash.GetComponentInChildren<Light>(true);
        if (muzzleFlashSecondary != null)
        {
            tankRig.muzzleFlashRoots = new[] { muzzleFlashSecondary };
            tankRig.muzzleFlashLights = new[] { muzzleFlashSecondary.GetComponentInChildren<Light>(true) };
        }
        tankRig.trackScrollUnitsPerMeter = spec.TrackScrollUnitsPerMeter;
        tankRig.wheelRotationDegreesPerMeter = spec.WheelRotationDegreesPerMeter;
        tankRig.barrelRecoilDistance = spec.BarrelRecoilDistance;
        tankRig.muzzleFlashDuration = 0.09f;

        var collider = actor.AddComponent<BoxCollider>();
        collider.center = spec.ColliderCenter;
        collider.size = spec.ColliderSize;

        var lod0Renderers = new List<Renderer>();
        lod0Renderers.AddRange(lod0.GetComponentsInChildren<Renderer>(true));
        lod0Renderers.AddRange(turret.GetComponentsInChildren<Renderer>(true));
        lod0Renderers.AddRange(trackLeft.GetComponentsInChildren<Renderer>(true));
        lod0Renderers.AddRange(trackRight.GetComponentsInChildren<Renderer>(true));
        lodGroup.SetLODs(new[]
        {
            new LOD(0.32f, lod0Renderers.ToArray()),
            new LOD(0.10f, lod1.GetComponentsInChildren<Renderer>(true)),
            new LOD(0.025f, lod2.GetComponentsInChildren<Renderer>(true)),
            new LOD(0.006f, Array.Empty<Renderer>())
        });
        lodGroup.RecalculateBounds();

        PrefabUtility.SaveAsPrefabAsset(actor, spec.PrefabPath);
        UnityEngine.Object.DestroyImmediate(actor);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
        ConfigureVisualDefinition(prefab, spec);
        ConfigureSavedDescriptor(prefab, spec);
        if (createReviewScene)
            CreateReviewScene(prefab, spec);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (selectResult)
            Selection.activeObject = prefab;

        Debug.Log("Built ProjectAegisRTS " + spec.UnitId + " prefab at " + spec.PrefabPath);
    }

    static string Part(TankSpec spec, string suffix)
    {
        return spec.Source + "/" + spec.UnitId + "_" + suffix;
    }

    static string Texture(TankSpec spec, string suffix)
    {
        return spec.Textures + "/" + spec.UnitId + "_" + suffix;
    }

    static Transform Child(Transform parent, string name, Vector3 localPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        return go.transform;
    }

    static Transform Socket(Transform parent, string kindName, Vector3 localPosition, TankSpec spec)
    {
        var t = Child(parent, kindName, localPosition);
        AddSocketComponent(t.gameObject, kindName, spec.UnitId);
        return t;
    }

    static GameObject InstantiateModel(string name, string path, Transform parent, Vector3 localPosition, Material material)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (source == null)
            throw new InvalidOperationException("Missing tank model source: " + path);

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
        {
            var count = Mathf.Max(1, r.sharedMaterials.Length);
            var mats = new Material[count];
            for (var i = 0; i < count; i++)
                mats[i] = material;
            r.sharedMaterials = mats;
            r.shadowCastingMode = ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        return instance;
    }

    static void CreateSquaredTurretArmor(Transform turret, Material material)
    {
        CreateArmorBox("SquaredTurretDeckPlate", turret, new Vector3(0f, 0.22f, 0.08f), new Vector3(1.86f, 0.16f, 1.44f), material);
        CreateArmorBox("SquaredTurretLeftCheek", turret, new Vector3(-0.94f, 0.03f, 0.12f), new Vector3(0.24f, 0.42f, 1.34f), material);
        CreateArmorBox("SquaredTurretRightCheek", turret, new Vector3(0.94f, 0.03f, 0.12f), new Vector3(0.24f, 0.42f, 1.34f), material);
        CreateArmorBox("SquaredTurretRearArmor", turret, new Vector3(0f, 0.02f, -0.72f), new Vector3(1.58f, 0.42f, 0.30f), material);
        CreateArmorBox("SquaredTurretFrontCollar", turret, new Vector3(0f, 0.02f, 0.87f), new Vector3(1.46f, 0.30f, 0.20f), material);
    }

    static void CreateMissileLauncherCase(Transform missileLauncher, Material armorMaterial, Material teamMaterial, Material boltMaterial)
    {
        CreateArmorBox("MissileCaseBottomCradle", missileLauncher, new Vector3(0f, 0.31f, 0.10f), new Vector3(1.80f, 0.18f, 1.18f), armorMaterial);
        CreateArmorBox("MissileCaseLeftArmor", missileLauncher, new Vector3(-0.98f, 0.82f, 0.11f), new Vector3(0.18f, 0.98f, 1.22f), armorMaterial);
        CreateArmorBox("MissileCaseRightArmor", missileLauncher, new Vector3(0.98f, 0.82f, 0.11f), new Vector3(0.18f, 0.98f, 1.22f), armorMaterial);
        CreateArmorBox("MissileCaseRearArmor", missileLauncher, new Vector3(0f, 0.82f, -0.55f), new Vector3(1.92f, 0.98f, 0.18f), armorMaterial);
        CreateArmorBox("MissileCaseTopStrap", missileLauncher, new Vector3(0f, 1.36f, 0.08f), new Vector3(1.88f, 0.16f, 1.12f), armorMaterial);
        CreateArmorBox("MissileCaseFrontLip", missileLauncher, new Vector3(0f, 0.50f, 0.77f), new Vector3(1.78f, 0.16f, 0.18f), armorMaterial);

        CreateArmorBox("MissileCaseTeamPanelLeft", missileLauncher, new Vector3(-1.08f, 0.88f, 0.12f), new Vector3(0.04f, 0.34f, 0.72f), teamMaterial);
        CreateArmorBox("MissileCaseTeamPanelRight", missileLauncher, new Vector3(1.08f, 0.88f, 0.12f), new Vector3(0.04f, 0.34f, 0.72f), teamMaterial);
        CreateArmorBox("MissileCaseRearBoltBar", missileLauncher, new Vector3(0f, 1.16f, -0.66f), new Vector3(1.50f, 0.08f, 0.08f), boltMaterial);
    }

    static void CreateArmorBox(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;

        var collider = go.GetComponent<Collider>();
        if (collider != null)
            UnityEngine.Object.DestroyImmediate(collider);

        var renderer = go.GetComponent<Renderer>();
        if (renderer == null)
            return;

        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }

    static Renderer GetRenderer(GameObject go)
    {
        return go == null ? null : go.GetComponentInChildren<Renderer>(true);
    }

    static Renderer[] TeamRenderers(params GameObject[] roots)
    {
        return roots.Where(r => r != null)
            .SelectMany(r => r.GetComponentsInChildren<Renderer>(true))
            .Where(r => r != null)
            .ToArray();
    }

    static Transform CreateMuzzleFlash(Transform muzzlePrimary, Material material)
    {
        var flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "MuzzleFlash";
        flash.transform.SetParent(muzzlePrimary, false);
        flash.transform.localPosition = Vector3.zero;
        flash.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);

        var collider = flash.GetComponent<Collider>();
        if (collider != null)
            UnityEngine.Object.DestroyImmediate(collider);

        var renderer = flash.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;

        var light = flash.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.62f, 0.25f);
        light.range = 2.5f;
        light.intensity = 0f;

        flash.SetActive(false);
        return flash.transform;
    }

    static Material Material(TankSpec spec, string materialRole, Color baseColor, bool emission, float metallic, float smoothness)
    {
        var path = spec.Materials + "/mat_" + spec.UnitId + "_" + materialRole + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (mat == null)
        {
            mat = new Material(shader) { name = "mat_" + spec.UnitId + "_" + materialRole };
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.shader = shader;
        var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture(spec, "albedo.png"));
        var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture(spec, "normal.png"));
        var orm = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture(spec, "orm.png"));
        var em = AssetDatabase.LoadAssetAtPath<Texture2D>(Texture(spec, "emission.png"));
        SetTex(mat, "_BaseMap", albedo);
        SetTex(mat, "_MainTex", albedo);
        SetTex(mat, "_BumpMap", normal);
        mat.EnableKeyword("_NORMALMAP");
        SetTex(mat, "_OcclusionMap", orm);
        SetTex(mat, "_MaskMap", orm);
        SetColor(mat, "_BaseColor", baseColor);
        SetColor(mat, "_Color", baseColor);
        SetFloat(mat, "_Metallic", metallic);
        SetFloat(mat, "_Smoothness", smoothness);
        SetFloat(mat, "_Glossiness", smoothness);
        if (emission)
        {
            SetTex(mat, "_EmissionMap", em);
            SetColor(mat, "_EmissionColor", baseColor * 2.5f);
            mat.EnableKeyword("_EMISSION");
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }

        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void SetTex(Material mat, string prop, Texture t)
    {
        if (mat.HasProperty(prop))
            mat.SetTexture(prop, t);
    }

    static void SetColor(Material mat, string prop, Color c)
    {
        if (mat.HasProperty(prop))
            mat.SetColor(prop, c);
    }

    static void SetFloat(Material mat, string prop, float v)
    {
        if (mat.HasProperty(prop))
            mat.SetFloat(prop, v);
    }

    static void ConfigureDescriptor(ActorPrefabDescriptor descriptor, TankSpec spec, ActorVisualDefinition definition)
    {
        if (descriptor == null)
            return;

        descriptor.actorTypeId = spec.UnitId;
        descriptor.category = ActorArtCategory.Vehicle;
        descriptor.sourceDefinition = definition;
        descriptor.productionStatus = ActorArtProductionStatus.Animated;
        descriptor.generatedByStage8 = false;
        descriptor.requiredSocketsPresent = true;
        descriptor.declaredRequiredSockets.Clear();
        descriptor.declaredRequiredSockets.AddRange(RequiredSocketsFor(spec));
        descriptor.notes = spec.DisplayName + " production art imported from ProjectAegis tank Unity asset packet. Gameplay values remain sourced from Rts.Core DemoRules.";
    }

    static void AddSocketComponent(GameObject go, string socketKindName, string unitId)
    {
        var socket = go.GetComponent<ActorPrefabSocket>() ?? go.AddComponent<ActorPrefabSocket>();
        ActorPrefabSocketKind kind;
        if (Enum.TryParse(socketKindName, out kind))
            socket.socketKind = kind;
        socket.socketName = socketKindName;
        socket.actorTypeId = unitId;
        socket.notes = unitId + " packet socket: " + socketKindName;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        var name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    static void ConfigureImportSettings(TankSpec spec)
    {
        ConfigureTexture(Texture(spec, "albedo.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(Texture(spec, "normal.png"), TextureImporterType.NormalMap, false, true);
        ConfigureTexture(Texture(spec, "orm.png"), TextureImporterType.Default, false, true);
        ConfigureTexture(Texture(spec, "emission.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(Texture(spec, "team_mask.png"), TextureImporterType.Default, false, true);
        ConfigureTexture(Texture(spec, "muzzle_flash_sheet.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(Texture(spec, "shell_projectile.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(Texture(spec, "missile_projectile.png"), TextureImporterType.Default, true, true);
        ConfigureTexture(spec.IconPath, TextureImporterType.Sprite, true, false);

        var modelPaths = new List<string>
        {
            spec.Source + "/" + spec.UnitId + ".obj",
            Part(spec, "hull.obj"),
            Part(spec, "track_left.obj"),
            Part(spec, "track_right.obj"),
            Part(spec, "turret.obj"),
            Part(spec, "barrel.obj"),
            Part(spec, "team_body.obj"),
            Part(spec, "team_turret.obj"),
            Part(spec, "emissive_body.obj"),
            Part(spec, "emissive_turret.obj"),
            Part(spec, "glass_body.obj"),
            Part(spec, "bolts.obj"),
            Part(spec, "lod1.obj"),
            Part(spec, "lod2.obj")
        };

        if (spec.HasMissileLauncher)
        {
            modelPaths.Add(Part(spec, "missile_pod.obj"));
            modelPaths.Add(Part(spec, "missile_tubes.obj"));
            modelPaths.Add(Part(spec, "team_missile_pod.obj"));
            modelPaths.Add(Part(spec, "emissive_missile_pod.obj"));
            modelPaths.Add(Part(spec, "missile_pod_bolts.obj"));
            modelPaths.Add(Part(spec, "orange_accents_body.obj"));
            modelPaths.Add(Part(spec, "orange_accents_turret.obj"));
            modelPaths.Add(Part(spec, "glass_turret.obj"));
            modelPaths.Add(Part(spec, "turret_bolts.obj"));
            modelPaths.Add(Part(spec, "missile_projectile.obj"));
        }

        for (var i = 0; i < modelPaths.Count; i++)
            AssetDatabase.ImportAsset(modelPaths[i], ImportAssetOptions.ForceUpdate);
    }

    static void ConfigureTexture(string path, TextureImporterType type, bool srgb, bool mipmaps)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = type;
        importer.sRGBTexture = srgb;
        importer.mipmapEnabled = mipmaps;
        importer.alphaIsTransparency = path.EndsWith("_icon.png", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("muzzle_flash") ||
            path.Contains("projectile");
        importer.SaveAndReimport();
    }

    static void ConfigureSavedDescriptor(GameObject prefab, TankSpec spec)
    {
        if (prefab == null)
            return;

        var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(spec.VisualDefinitionPath);
        var descriptor = prefab.GetComponent<ActorPrefabDescriptor>();
        ConfigureDescriptor(descriptor, spec, definition);
        if (descriptor != null)
        {
            descriptor.ValidateRequiredSockets(RequiredSocketsFor(spec));
            EditorUtility.SetDirty(descriptor);
        }
    }

    static void ConfigureVisualDefinition(GameObject prefab, TankSpec spec)
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
        definition.safeDisplayName = spec.DisplayName;
        definition.category = ActorArtCategory.Vehicle;
        definition.productionStatus = ActorArtProductionStatus.Animated;
        definition.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spec.IconPath);
        definition.productionPrefab = prefab;
        definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
        definition.motionProfileId = spec.MotionProfileId;
        definition.selectionRadius = spec.SelectionRadius;
        definition.selectionHeight = spec.SelectionHeight;
        definition.footprintWidth = Math.Max(spec.FootprintWidth, definition.footprintWidth);
        definition.footprintHeight = Math.Max(spec.FootprintHeight, definition.footprintHeight);
        definition.visualScale = spec.VisualScale;
        definition.prefabHeightOffset = 0f;
        definition.useBuildingVisualController = false;
        definition.useVehicleMotionController = true;
        definition.useInfantryMotionController = false;
        definition.useAircraftMotionController = false;
        definition.useTurretVisualController = true;
        definition.requiredSockets.Clear();
        definition.requiredSockets.AddRange(RequiredSocketsFor(spec));
        definition.ipReviewRequired = false;
        definition.notes = "Production visual uses the ProjectAegis " + spec.DisplayName + " Unity asset packet. Hull-attached team-color panels are runtime recolorable. Rts.Core gameplay values are preserved from DemoRules." +
            (spec.HasTwinBarrels ? " Heavy tank Unity assembly has twin visible barrels and a larger silhouette." : string.Empty) +
            (spec.HasMissileLauncher ? " Blocky Mammoth replacement includes a rear missile rack with eight helper muzzle transforms, a boxy armored launcher case without the circular pod base, and extra hull/turret detail meshes." : string.Empty);
        EditorUtility.SetDirty(definition);
    }

    static ActorPrefabSocketKind[] RequiredSocketsFor(TankSpec spec)
    {
        if (spec == null || !spec.HasTwinBarrels)
            return RequiredSockets;

        var sockets = new List<ActorPrefabSocketKind>(RequiredSockets);
        if (!sockets.Contains(ActorPrefabSocketKind.MuzzleSecondary))
            sockets.Add(ActorPrefabSocketKind.MuzzleSecondary);
        return sockets.ToArray();
    }

    static void CreateReviewScene(GameObject prefab, TankSpec spec)
    {
        if (prefab == null)
            return;

        EnsureFolder(Path.GetDirectoryName(spec.ReviewScenePath).Replace('\\', '/'));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject(spec.DisplayName + " Unit Review");

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = spec.UnitId + "_final_prefab";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
        instance.transform.SetParent(root.transform);
        var rig = instance.GetComponent<TankVisualRigController>();
        var demo = instance.GetComponent<TankVisualDemoController>() ?? instance.AddComponent<TankVisualDemoController>();
        demo.rig = rig;
        demo.orbitCenter = Vector3.zero;
        demo.orbitRadius = 1.45f;
        demo.orbitSpeed = spec.UnitId == "heavy_tank" ? 0.42f : 0.50f;
        demo.fireInterval = spec.UnitId == "heavy_tank" ? 1.80f : 1.60f;
        demo.aimSweepDegrees = 55f;

        CreateGround();
        CreateLabel(spec.DisplayName + " final mesh prefab - press Play to watch it move", new Vector3(-4.3f, 0.05f, -3.1f));
        CreateLabel("Hull team-color panels are attached to body sides, deck, front, and rear", new Vector3(-4.3f, 0.05f, 3.15f));
        CreateSocketMarkers(instance);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 12f);

        var cameraGo = new GameObject("Main Camera");
        var camera = cameraGo.AddComponent<Camera>();
        cameraGo.tag = "MainCamera";
        cameraGo.transform.position = new Vector3(6.2f, 5.0f, -8.4f);
        cameraGo.transform.rotation = Quaternion.Euler(56f, -36f, 0f);
        camera.fieldOfView = 36f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.10f, 0.13f, 0.13f);

        EditorSceneManager.SaveScene(scene, spec.ReviewScenePath);
    }

    static void CreateGround()
    {
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Review Ground";
        plane.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
        var renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = new Color(0.22f, 0.28f, 0.25f);
            renderer.sharedMaterial = material;
        }
    }

    static void CreateSocketMarkers(GameObject instance)
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
            marker.transform.localScale = Vector3.one * 0.08f;
            var collider = marker.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    static void CreateLabel(string text, Vector3 position)
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
}
