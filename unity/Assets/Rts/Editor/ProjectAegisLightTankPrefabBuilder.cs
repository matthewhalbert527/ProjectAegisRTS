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

public static class ProjectAegisLightTankPrefabBuilder
{
    private const string UnitId = "light_tank";
    private const string Source = "Assets/Rts/Art/Source/Units/light_tank";
    private const string Textures = "Assets/Rts/Art/Textures/Units/light_tank";
    private const string Materials = "Assets/Rts/Art/Materials/Units/light_tank";
    private const string IconPath = "Assets/Rts/Art/Icons/light_tank_icon.png";
    private const string PrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/Units/light_tank";
    private const string PrefabPath = PrefabFolder + "/light_tank.prefab";
    private const string VisualDefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset";
    private const string ReviewScenePath = "Assets/Rts/Scenes/light_tank_UnitReview.unity";
    private const float BarrelLengthScale = 2f / 3f;
    private const float MuzzlePrimaryZ = 3.40f * BarrelLengthScale;

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
        ActorPrefabSocketKind.BarrelRoot,
        ActorPrefabSocketKind.TrackLeft,
        ActorPrefabSocketKind.TrackRight,
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

    [MenuItem("ProjectAegisRTS/Art/Build Light Tank Prefab")]
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
        string[] models = {
            Source + "/light_tank_hull.obj", Source + "/light_tank_track_left.obj", Source + "/light_tank_track_right.obj",
            Source + "/light_tank_turret.obj", Source + "/light_tank_barrel.obj", Source + "/light_tank_team_body.obj",
            Source + "/light_tank_team_turret.obj", Source + "/light_tank_emissive_body.obj", Source + "/light_tank_emissive_turret.obj",
            Source + "/light_tank_glass_body.obj", Source + "/light_tank_bolts.obj", Source + "/light_tank_lod1.obj", Source + "/light_tank_lod2.obj"
        };
        foreach (string path in models)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return false;
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(Textures + "/light_tank_albedo.png") != null;
    }

    private static void Build(bool selectResult, bool createReviewScene)
    {
        ConfigureImportSettings();
        EnsureFolder(Materials);
        EnsureFolder(PrefabFolder);
        Material body = Material("mat_light_tank_body", new Color(0.36f, 0.41f, 0.31f), false, 0.10f, 0.38f);
        Material tracks = Material("mat_light_tank_tracks", new Color(0.03f, 0.04f, 0.04f), false, 0.15f, 0.24f);
        Material weapon = Material("mat_light_tank_weapon", new Color(0.17f, 0.19f, 0.18f), false, 0.18f, 0.32f);
        Material team = Material("mat_light_tank_team_color", Color.white, false, 0.08f, 0.42f);
        Material emissive = Material("mat_light_tank_emissive", new Color(1.0f, 0.46f, 0.08f), true, 0.0f, 0.50f);
        Material glass = Material("mat_light_tank_glass", new Color(0.10f, 0.45f, 0.50f, 0.75f), true, 0.0f, 0.62f);
        Material bolts = Material("mat_light_tank_bolts", new Color(0.09f, 0.10f, 0.09f), false, 0.12f, 0.28f);
        Material muzzle = Material("mat_light_tank_muzzle_flash", new Color(1.0f, 0.58f, 0.18f), true, 0.0f, 0.35f);

        GameObject actor = new GameObject("light_tank");
        ConfigureDescriptor(actor.AddComponent<ActorPrefabDescriptor>(), null);
        var lodGroup = actor.AddComponent<LODGroup>();

        Transform root = Socket(actor.transform, "Root", Vector3.zero);
        Transform visualRoot = Socket(root, "VisualRoot", Vector3.zero);
        Transform bodyRoot = Socket(visualRoot, "BodyRoot", Vector3.zero);
        Transform lod0 = Child(bodyRoot, "LOD0_Meshes", Vector3.zero);
        Transform lod1 = Child(bodyRoot, "LOD1_Meshes", Vector3.zero);
        Transform lod2 = Child(bodyRoot, "LOD2_Meshes", Vector3.zero);

        GameObject hull = InstantiateModel("HullArmor", Source + "/light_tank_hull.obj", lod0, new Vector3(0f, 0.80f, 0f), body);
        GameObject teamBody = InstantiateModel("TeamColorPanels_Body", Source + "/light_tank_team_body.obj", lod0, Vector3.zero, team);
        GameObject emissiveBody = InstantiateModel("Emissive_Body", Source + "/light_tank_emissive_body.obj", lod0, Vector3.zero, emissive);
        GameObject glassBody = InstantiateModel("Glass_Body", Source + "/light_tank_glass_body.obj", lod0, Vector3.zero, glass);
        GameObject boltMesh = InstantiateModel("BoltsAndFasteners", Source + "/light_tank_bolts.obj", lod0, Vector3.zero, bolts);

        Transform trackLeft = Socket(visualRoot, "TrackLeft", new Vector3(-1.14f, 0.52f, 0f));
        Transform trackRight = Socket(visualRoot, "TrackRight", new Vector3(1.14f, 0.52f, 0f));
        GameObject left = InstantiateModel("TrackLeftMesh", Source + "/light_tank_track_left.obj", trackLeft, Vector3.zero, tracks);
        GameObject right = InstantiateModel("TrackRightMesh", Source + "/light_tank_track_right.obj", trackRight, Vector3.zero, tracks);

        Transform turret = Socket(visualRoot, "TurretRoot", new Vector3(0f, 1.36f, -0.08f));
        GameObject turretMesh = InstantiateModel("TurretArmor", Source + "/light_tank_turret.obj", turret, Vector3.zero, body);
        GameObject teamTurret = InstantiateModel("TeamColorPanels_Turret", Source + "/light_tank_team_turret.obj", turret, Vector3.zero, team);
        GameObject emissiveTurret = InstantiateModel("Emissive_Turret", Source + "/light_tank_emissive_turret.obj", turret, Vector3.zero, emissive);
        Transform barrel = Socket(turret, "BarrelRoot", new Vector3(0f, 0.10f, 0.66f));
        GameObject barrelMesh = InstantiateModel("BarrelMesh", Source + "/light_tank_barrel.obj", barrel, Vector3.zero, weapon);
        barrelMesh.transform.localScale = new Vector3(1f, 1f, BarrelLengthScale);
        Transform muzzlePrimary = Socket(barrel, "MuzzlePrimary", new Vector3(0f, 0f, MuzzlePrimaryZ));
        Transform muzzleFlash = CreateMuzzleFlash(muzzlePrimary, muzzle);

        Socket(root, "SelectionAnchor", new Vector3(0f, 0.05f, 0f));
        Socket(root, "HealthBarAnchor", new Vector3(0f, 2.65f, 0f));
        Socket(root, "UiAnchor", new Vector3(0f, 2.45f, -0.15f));
        Socket(visualRoot, "VfxSmoke", new Vector3(0.65f, 1.70f, -1.45f));
        Socket(visualRoot, "VfxExplosion", new Vector3(0f, 1.10f, 0f));

        GameObject low1 = InstantiateModel("LOD1_Static", Source + "/light_tank_lod1.obj", lod1, Vector3.zero, body);
        GameObject low2 = InstantiateModel("LOD2_Static", Source + "/light_tank_lod2.obj", lod2, Vector3.zero, body);
        lod1.gameObject.SetActive(true);
        lod2.gameObject.SetActive(true);

        var controller = actor.AddComponent<ProjectAegisLightTankVisualRig>();
        controller.turretRoot = turret;
        controller.barrelRoot = barrel;
        controller.trackLeft = trackLeft;
        controller.trackRight = trackRight;
        controller.muzzlePrimary = muzzlePrimary;
        controller.teamColorRenderers = new Renderer[] { GetRenderer(teamBody), GetRenderer(teamTurret) }.Where(r => r != null).ToArray();

        var tankRig = actor.AddComponent<TankVisualRigController>();
        tankRig.bodyRoot = bodyRoot;
        tankRig.turretRoot = turret;
        tankRig.barrelRoot = barrel;
        tankRig.trackLeftRoot = trackLeft;
        tankRig.trackRightRoot = trackRight;
        tankRig.trackRenderers = new Renderer[] { GetRenderer(left), GetRenderer(right) }.Where(r => r != null).ToArray();
        tankRig.muzzleFlashRoot = muzzleFlash;
        tankRig.muzzleFlashLight = muzzleFlash.GetComponentInChildren<Light>(true);
        tankRig.trackScrollUnitsPerMeter = 2.6f;
        tankRig.wheelRotationDegreesPerMeter = 700f;
        tankRig.barrelRecoilDistance = 0.08f;
        tankRig.muzzleFlashDuration = 0.09f;

        BoxCollider collider = actor.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 1.08f, 0.05f);
        collider.size = new Vector3(3.10f, 2.20f, 4.25f);

        var lod0Renderers = new List<Renderer>();
        lod0Renderers.AddRange(lod0.GetComponentsInChildren<Renderer>(true));
        lod0Renderers.AddRange(turret.GetComponentsInChildren<Renderer>(true));
        lod0Renderers.AddRange(trackLeft.GetComponentsInChildren<Renderer>(true));
        lod0Renderers.AddRange(trackRight.GetComponentsInChildren<Renderer>(true));
        lodGroup.SetLODs(new[] {
            new LOD(0.30f, lod0Renderers.ToArray()),
            new LOD(0.09f, lod1.GetComponentsInChildren<Renderer>(true)),
            new LOD(0.025f, lod2.GetComponentsInChildren<Renderer>(true)),
            new LOD(0.006f, Array.Empty<Renderer>())
        });
        lodGroup.RecalculateBounds();

        PrefabUtility.SaveAsPrefabAsset(actor, PrefabPath);
        UnityEngine.Object.DestroyImmediate(actor);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        ConfigureVisualDefinition(prefab);
        ConfigureSavedDescriptor(prefab);
        if (createReviewScene)
            CreateReviewScene(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (selectResult) Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Debug.Log("Built ProjectAegisRTS light_tank prefab at " + PrefabPath);
    }

    private static Transform Child(Transform parent, string name, Vector3 localPosition)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        return go.transform;
    }

    private static Transform Socket(Transform parent, string kindName, Vector3 localPosition)
    {
        Transform t = Child(parent, kindName, localPosition);
        AddSocketComponent(t.gameObject, kindName);
        return t;
    }

    private static GameObject InstantiateModel(string name, string path, Transform parent, Vector3 localPosition, Material material)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        foreach (Renderer r in instance.GetComponentsInChildren<Renderer>(true))
        {
            int count = Mathf.Max(1, r.sharedMaterials.Length);
            var mats = new Material[count];
            for (int i = 0; i < count; i++) mats[i] = material;
            r.sharedMaterials = mats;
            r.shadowCastingMode = ShadowCastingMode.On;
            r.receiveShadows = true;
        }
        return instance;
    }

    private static Renderer GetRenderer(GameObject go) => go == null ? null : go.GetComponentInChildren<Renderer>(true);

    private static Transform CreateMuzzleFlash(Transform muzzlePrimary, Material material)
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

    private static Material Material(string name, Color baseColor, bool emission, float metallic, float smoothness)
    {
        string path = Materials + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (mat == null)
        {
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.shader = shader;
        Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(Textures + "/light_tank_albedo.png");
        Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(Textures + "/light_tank_normal.png");
        Texture2D orm = AssetDatabase.LoadAssetAtPath<Texture2D>(Textures + "/light_tank_orm.png");
        Texture2D em = AssetDatabase.LoadAssetAtPath<Texture2D>(Textures + "/light_tank_emission.png");
        SetTex(mat, "_BaseMap", albedo); SetTex(mat, "_MainTex", albedo);
        SetTex(mat, "_BumpMap", normal); mat.EnableKeyword("_NORMALMAP");
        SetTex(mat, "_OcclusionMap", orm);
        SetColor(mat, "_BaseColor", baseColor); SetColor(mat, "_Color", baseColor);
        SetFloat(mat, "_Metallic", metallic); SetFloat(mat, "_Smoothness", smoothness); SetFloat(mat, "_Glossiness", smoothness);
        if (emission)
        {
            SetTex(mat, "_EmissionMap", em); SetColor(mat, "_EmissionColor", baseColor * 2.5f); mat.EnableKeyword("_EMISSION");
        }
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void SetTex(Material mat, string prop, Texture t) { if (mat.HasProperty(prop)) mat.SetTexture(prop, t); }
    private static void SetColor(Material mat, string prop, Color c) { if (mat.HasProperty(prop)) mat.SetColor(prop, c); }
    private static void SetFloat(Material mat, string prop, float v) { if (mat.HasProperty(prop)) mat.SetFloat(prop, v); }

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
        descriptor.notes = "Light Tank production art imported from ProjectAegis light tank Unity asset packet. Gameplay values remain sourced from Rts.Core DemoRules.";
    }

    private static void AddSocketComponent(GameObject go, string socketKindName)
    {
        var socket = go.GetComponent<ActorPrefabSocket>() ?? go.AddComponent<ActorPrefabSocket>();
        ActorPrefabSocketKind kind;
        if (Enum.TryParse(socketKindName, out kind))
            socket.socketKind = kind;
        socket.socketName = socketKindName;
        socket.actorTypeId = UnitId;
        socket.notes = "Light Tank packet socket: " + socketKindName;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\','/');
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void ConfigureImportSettings()
    {
        ConfigureTexture(Textures + "/light_tank_albedo.png", TextureImporterType.Default, true, true);
        ConfigureTexture(Textures + "/light_tank_normal.png", TextureImporterType.NormalMap, false, true);
        ConfigureTexture(Textures + "/light_tank_orm.png", TextureImporterType.Default, false, true);
        ConfigureTexture(Textures + "/light_tank_emission.png", TextureImporterType.Default, true, true);
        ConfigureTexture(Textures + "/light_tank_team_mask.png", TextureImporterType.Default, false, true);
        ConfigureTexture(Textures + "/light_tank_muzzle_flash_sheet.png", TextureImporterType.Default, true, true);
        ConfigureTexture(Textures + "/light_tank_shell_projectile.png", TextureImporterType.Default, true, true);
        ConfigureTexture(IconPath, TextureImporterType.Sprite, true, false);
        AssetDatabase.ImportAsset(Source + "/light_tank.obj", ImportAssetOptions.ForceUpdate);
    }

    private static void ConfigureTexture(string path, TextureImporterType type, bool srgb, bool mipmaps)
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
        definition.displayName = "Light Tank";
        definition.safeDisplayName = "Light Tank";
        definition.category = ActorArtCategory.Vehicle;
        definition.productionStatus = ActorArtProductionStatus.Animated;
        definition.icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
        definition.productionPrefab = prefab;
        definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
        definition.motionProfileId = "tracked_light";
        definition.selectionRadius = 1.70f;
        definition.selectionHeight = 2.40f;
        definition.footprintWidth = Math.Max(4, definition.footprintWidth);
        definition.footprintHeight = Math.Max(4, definition.footprintHeight);
        definition.visualScale = 0.34f;
        definition.prefabHeightOffset = 0f;
        definition.useBuildingVisualController = false;
        definition.useVehicleMotionController = true;
        definition.useInfantryMotionController = false;
        definition.useAircraftMotionController = false;
        definition.useTurretVisualController = true;
        definition.requiredSockets.Clear();
        definition.requiredSockets.AddRange(RequiredSockets);
        definition.ipReviewRequired = false;
        definition.notes = "Production visual uses the ProjectAegis light tank Unity asset packet. Rts.Core gameplay values are preserved from DemoRules.";
        EditorUtility.SetDirty(definition);
    }

    private static void CreateReviewScene(GameObject prefab)
    {
        if (prefab == null)
            return;

        EnsureFolder(Path.GetDirectoryName(ReviewScenePath).Replace('\\', '/'));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Light Tank Unit Review");

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "light_tank_final_prefab";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
        instance.transform.SetParent(root.transform);
        var rig = instance.GetComponent<TankVisualRigController>();
        var demo = instance.GetComponent<TankVisualDemoController>() ?? instance.AddComponent<TankVisualDemoController>();
        demo.rig = rig;
        demo.orbitCenter = Vector3.zero;
        demo.orbitRadius = 1.35f;
        demo.orbitSpeed = 0.55f;
        demo.fireInterval = 1.45f;
        demo.aimSweepDegrees = 60f;

        CreateGround();
        CreateLabel("Light Tank final mesh prefab - press Play to watch it move", new Vector3(-3.9f, 0.05f, -3.0f));
        CreateLabel("Tracks, turret sweep, barrel recoil, and muzzle flash are rigged", new Vector3(-4.0f, 0.05f, 3.0f));
        CreateSocketMarkers(instance);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 12f);

        var cameraGo = new GameObject("Main Camera");
        var camera = cameraGo.AddComponent<Camera>();
        cameraGo.tag = "MainCamera";
        cameraGo.transform.position = new Vector3(5.4f, 4.3f, -7.2f);
        cameraGo.transform.rotation = Quaternion.Euler(56f, -36f, 0f);
        camera.fieldOfView = 36f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.10f, 0.13f, 0.13f);

        EditorSceneManager.SaveScene(scene, ReviewScenePath);
    }

    private static void CreateGround()
    {
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Review Ground";
        plane.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
        var renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = new Color(0.22f, 0.28f, 0.25f);
            renderer.sharedMaterial = material;
        }
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
            marker.transform.localScale = Vector3.one * 0.08f;
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
}
