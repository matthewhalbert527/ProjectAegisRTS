using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class UnityAiBuildingSlateGenerator
    {
        public const string ScenePath = "Assets/Rts/Scenes/UnityAI_BuildingSlateReview.unity";
        public const string ScreenshotRelativePath = "build/screenshots/unity_ai_building_slate_review.png";
        const string AssetFolder = "Assets/Rts/Art/UnityAIBuildingSlate";
        const string PrefabFolder = AssetFolder + "/Prefabs";
        const string MaterialFolder = AssetFolder + "/Materials";
        const string TextureFolder = AssetFolder + "/Textures";
        const string SourceTextureFolder = AssetFolder + "/SourceTextures";
        const string DefinitionFolder = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions";
        const string ProfileFolder = "Assets/Rts/ScriptableObjects/BuildingProfiles";
        const int TextureSize = 512;

        static readonly BuildingSpec[] BuildingSpecs =
        {
            new BuildingSpec("fabrication_hub", "Fabrication Hub", ActorArtCategory.Building, BuildingVisualCategory.Construction, 4, 3).WithCrane().WithDoor(0.42f).WithProduction(),
            new BuildingSpec("power_plant", "Power Plant", ActorArtCategory.Building, BuildingVisualCategory.Power, 2, 2).WithTurbine(),
            new BuildingSpec("advanced_power_plant", "Advanced Power Plant", ActorArtCategory.Building, BuildingVisualCategory.Power, 3, 2).WithTurbine(),
            new BuildingSpec("barracks", "Barracks", ActorArtCategory.Building, BuildingVisualCategory.Production, 2, 2).WithDoor(0.62f).WithProduction(),
            new BuildingSpec("war_factory", "War Factory", ActorArtCategory.Building, BuildingVisualCategory.Production, 3, 2).WithDoor(0.86f).WithProduction(),
            new BuildingSpec("refinery", "Refinery", ActorArtCategory.Building, BuildingVisualCategory.Refinery, 3, 3).WithDock(),
            new BuildingSpec("field_hospital", "Field Hospital", ActorArtCategory.Building, BuildingVisualCategory.Medical, 2, 2).WithDoor(0.62f).WithProduction(),
            new BuildingSpec("comm_center", "Comm Center", ActorArtCategory.Building, BuildingVisualCategory.Tech, 2, 2).WithRadar(),
            new BuildingSpec("repair_bay", "Repair Bay", ActorArtCategory.Building, BuildingVisualCategory.Repair, 3, 2).WithDoor(0.32f).WithRepairArms(),
            new BuildingSpec("tech_center", "Tech Center", ActorArtCategory.Building, BuildingVisualCategory.Tech, 3, 2).WithRadar(),
            new BuildingSpec("cannon_turret", "Cannon Turret", ActorArtCategory.Defense, BuildingVisualCategory.Defense, 1, 1).WithTurret(),
            new BuildingSpec("gun_tower", "Gun Tower", ActorArtCategory.Defense, BuildingVisualCategory.Defense, 1, 1).WithTurret(),
            new BuildingSpec("advanced_gun_tower", "Advanced Gun Tower", ActorArtCategory.Defense, BuildingVisualCategory.Defense, 1, 1).WithTurret(),
            new BuildingSpec("dual_helipad", "Dual Helipad", ActorArtCategory.Building, BuildingVisualCategory.Airfield, 3, 2).WithProduction()
        };

        [MenuItem("ProjectAegisRTS/Unity AI/Build Building Slate")]
        public static void BuildBuildingSlateMenu()
        {
            BuildSlate();
            Debug.Log("Unity AI building slate generated.");
        }

        [MenuItem("ProjectAegisRTS/Unity AI/Build And Capture Building Slate")]
        public static void BuildAndCaptureBuildingSlateMenu()
        {
            BuildSlate();
            var path = CaptureScreenshot();
            Debug.Log("Unity AI building slate generated and captured: " + path);
        }

        public static void BuildAndCaptureBatch()
        {
            BuildAndCaptureBuildingSlateMenu();
        }

        static void BuildSlate()
        {
            EnsureFolderRecursive(AssetFolder);
            EnsureFolderRecursive(PrefabFolder);
            EnsureFolderRecursive(MaterialFolder);
            EnsureFolderRecursive(TextureFolder);
            EnsureFolderRecursive(SourceTextureFolder);
            EnsureFolderRecursive(DefinitionFolder);
            EnsureFolderRecursive(ProfileFolder);

            var materials = CreateMaterials();
            var createdPrefabs = new Dictionary<string, GameObject>();
            var createdProfiles = new Dictionary<string, BuildingVisualProfile>();
            for (var i = 0; i < BuildingSpecs.Length; i++)
            {
                var profile = CreateOrUpdateProfile(BuildingSpecs[i]);
                var prefab = CreateOrUpdatePrefab(BuildingSpecs[i], profile, materials);
                UpdateVisualDefinition(BuildingSpecs[i], profile, prefab);
                createdPrefabs[BuildingSpecs[i].Id] = prefab;
                createdProfiles[BuildingSpecs[i].Id] = profile;
            }

            CreateReviewScene(createdPrefabs, createdProfiles, materials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static BuildingVisualProfile CreateOrUpdateProfile(BuildingSpec spec)
        {
            var path = ProfileFolder + "/" + spec.Id + "_building_visual.asset";
            var profile = AssetDatabase.LoadAssetAtPath<BuildingVisualProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BuildingVisualProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.ConfigureDefault(spec.Id + "_building_visual", spec.Id, spec.VisualCategory, spec.Width, spec.Height);
            profile.displayName = spec.DisplayName;
            profile.hasDoor = spec.HasDoor;
            profile.doorOpenDistance = spec.DoorOpenDistance;
            profile.doorOpenSpeed = spec.Id == "war_factory" ? 2.8f : 4.2f;
            profile.hasProductionBay = spec.HasProduction;
            profile.productionPulseSpeed = spec.Id == "war_factory" ? 1.75f : 2.6f;
            profile.hasTurbine = spec.HasTurbine;
            profile.turbineSpinSpeed = spec.Id == "advanced_power_plant" ? 245f : 190f;
            profile.hasRadarDish = spec.HasRadar;
            profile.radarDishSpinSpeed = spec.Id == "comm_center" ? 52f : 34f;
            profile.hasCrane = spec.HasCrane;
            profile.craneSweepDegrees = 34f;
            profile.craneSweepSpeed = 0.85f;
            profile.hasRepairArms = spec.HasRepairArms;
            profile.repairArmSpeed = 2.4f;
            profile.hasDock = spec.HasDock;
            profile.dockPumpSpeed = 2.5f;
            profile.hasTurretOrBarrel = spec.HasTurret;
            profile.lightCount = Mathf.Clamp(spec.Width + spec.Height, 2, 6);
            profile.lowPowerLightScale = 0.32f;
            profile.lowPowerMachinerySpeedScale = spec.VisualCategory == BuildingVisualCategory.Power ? 0.18f : 0.25f;
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static GameObject CreateOrUpdatePrefab(BuildingSpec spec, BuildingVisualProfile profile, MaterialBundle materials)
        {
            var root = new GameObject(spec.Id + "_unity_ai_building_art");
            var descriptor = root.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = spec.Id;
            descriptor.category = spec.ArtCategory;
            descriptor.productionStatus = ActorArtProductionStatus.Animated;
            descriptor.generatedByStage8 = false;
            descriptor.requiredSocketsPresent = true;
            descriptor.notes = "Generated high-detail classic RTS building art. Uses BuildingVisualRig for powered movement and production animation hooks.";

            var rig = root.AddComponent<BuildingVisualRig>();
            root.AddComponent<BuildingVisualStateController>();

            var visualRoot = AddSocket(root.transform, "VisualRoot", ActorPrefabSocketKind.VisualRoot, spec.Id, Vector3.zero);
            rig.visualRoot = visualRoot;
            var bodyRoot = AddSocket(visualRoot, "BodyRoot", ActorPrefabSocketKind.BodyRoot, spec.Id, Vector3.zero);
            AddSocket(root.transform, "SelectionAnchor", ActorPrefabSocketKind.SelectionAnchor, spec.Id, Vector3.zero);
            AddSocket(root.transform, "HealthBarAnchor", ActorPrefabSocketKind.HealthBarAnchor, spec.Id, new Vector3(0f, 1.75f, -0.2f));
            AddSocket(root.transform, "UiAnchor", ActorPrefabSocketKind.UiAnchor, spec.Id, new Vector3(0f, 2.1f, 0f));
            AddSocket(root.transform, "VfxSmoke", ActorPrefabSocketKind.VfxSmoke, spec.Id, new Vector3(spec.Width * 0.24f, 1.4f, -spec.Height * 0.18f));
            AddSocket(root.transform, "VfxExplosion", ActorPrefabSocketKind.VfxExplosion, spec.Id, Vector3.up * 0.8f);
            var productionVfxPosition = spec.Id == "war_factory"
                ? new Vector3(0f, 0.32f, spec.Height * 0.24f)
                : new Vector3(0f, 0.4f, spec.Height * 0.48f);
            AddSocket(root.transform, "VfxProduction", ActorPrefabSocketKind.VfxProduction, spec.Id, productionVfxPosition);

            BuildCommonFoundation(bodyRoot, spec, materials);
            BuildBuildingBody(bodyRoot, spec, rig, materials);
            AddSocket(root.transform, "ProductionExit", ActorPrefabSocketKind.ProductionExit, spec.Id, new Vector3(0f, 0.08f, spec.Height * 0.58f));
            AddSocket(root.transform, "RallyExit", ActorPrefabSocketKind.RallyExit, spec.Id, new Vector3(0f, 0.08f, spec.Height * 0.82f));

            descriptor.declaredRequiredSockets.Clear();
            descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.VisualRoot);
            descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.BodyRoot);
            descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.SelectionAnchor);
            descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.HealthBarAnchor);
            descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.UiAnchor);
            descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.VfxSmoke);
            descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.VfxExplosion);
            if (spec.HasDoor)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.DoorRoot);
            if (spec.HasProduction)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.ProductionExit);
            if (spec.HasDock)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.HarvesterDock);
            if (spec.HasRepairArms)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.RepairArmRoot);
            if (spec.HasCrane)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.CraneRoot);
            if (spec.HasRadar)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.RadarDishRoot);
            if (spec.HasTurbine)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.TurbineRoot);
            if (spec.HasTurret)
                descriptor.declaredRequiredSockets.Add(ActorPrefabSocketKind.TurretRoot);

            descriptor.ValidateRequiredSockets(descriptor.declaredRequiredSockets);
            var path = PrefabFolder + "/" + spec.Id + "_unity_ai_building.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static void BuildCommonFoundation(Transform parent, BuildingSpec spec, MaterialBundle materials)
        {
            var width = spec.Width * 0.92f;
            var depth = spec.Height * 0.92f;
            if (spec.HasTurret)
            {
                Cube(parent, "octagonal defense foundation core", new Vector3(0f, 0.06f, 0f), new Vector3(width * 0.86f, 0.12f, depth * 0.86f), materials.Foundation);
                Cube(parent, "defense diagonal pad north east", new Vector3(width * 0.27f, 0.07f, depth * 0.27f), new Vector3(width * 0.28f, 0.11f, depth * 0.28f), materials.Foundation).transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                Cube(parent, "defense diagonal pad south west", new Vector3(-width * 0.27f, 0.07f, -depth * 0.27f), new Vector3(width * 0.28f, 0.11f, depth * 0.28f), materials.Foundation).transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                Cube(parent, "dark inset defense plate", new Vector3(0f, 0.14f, 0f), new Vector3(width * 0.58f, 0.035f, depth * 0.58f), materials.DarkConcrete);
            }
            else if (spec.Id == "fabrication_hub")
            {
                Cube(parent, "oversized L shape construction yard long slab", new Vector3(-width * 0.10f, 0.06f, 0f), new Vector3(width * 0.90f, 0.12f, depth), materials.Foundation);
                Cube(parent, "oversized L shape construction yard side annex", new Vector3(width * 0.35f, 0.07f, -depth * 0.24f), new Vector3(width * 0.38f, 0.12f, depth * 0.54f), materials.Foundation);
                Cube(parent, "large open fabrication pit inset", new Vector3(-width * 0.12f, 0.14f, depth * 0.12f), new Vector3(width * 0.58f, 0.04f, depth * 0.52f), materials.DarkConcrete);
                Cube(parent, "wide front construction apron worn concrete", new Vector3(-width * 0.08f, 0.16f, depth * 0.42f), new Vector3(width * 0.66f, 0.035f, depth * 0.22f), materials.Concrete);
            }
            else if (spec.Id == "advanced_power_plant")
            {
                Cube(parent, "wide T shape advanced power foundation spine", new Vector3(0f, 0.06f, -depth * 0.08f), new Vector3(width * 0.40f, 0.12f, depth * 0.92f), materials.Foundation);
                Cube(parent, "extra wide advanced power reactor crossbar", new Vector3(0f, 0.07f, depth * 0.20f), new Vector3(width, 0.12f, depth * 0.46f), materials.Foundation);
                Cube(parent, "advanced power dark service trench", new Vector3(0f, 0.14f, -depth * 0.20f), new Vector3(width * 0.32f, 0.04f, depth * 0.50f), materials.DarkConcrete);
                Cube(parent, "advanced power wide side service apron", new Vector3(0f, 0.15f, -depth * 0.42f), new Vector3(width * 0.82f, 0.035f, depth * 0.18f), materials.Concrete);
            }
            else if (spec.Id == "barracks")
            {
                Cube(parent, "L shape barracks dormitory slab", new Vector3(-width * 0.16f, 0.06f, -depth * 0.05f), new Vector3(width * 0.70f, 0.12f, depth * 0.88f), materials.Foundation);
                Cube(parent, "L shape barracks command porch slab", new Vector3(width * 0.28f, 0.07f, depth * 0.24f), new Vector3(width * 0.42f, 0.12f, depth * 0.42f), materials.Foundation);
                Cube(parent, "barracks muster strip", new Vector3(0f, 0.155f, depth * 0.46f), new Vector3(width * 0.72f, 0.035f, depth * 0.16f), materials.Concrete);
            }
            else if (spec.Id == "war_factory")
            {
                Cube(parent, "U shape vehicle bay left slab", new Vector3(-width * 0.30f, 0.06f, 0f), new Vector3(width * 0.34f, 0.12f, depth), materials.Foundation);
                Cube(parent, "U shape vehicle bay right slab", new Vector3(width * 0.30f, 0.06f, 0f), new Vector3(width * 0.34f, 0.12f, depth), materials.Foundation);
                Cube(parent, "U shape vehicle bay rear slab", new Vector3(0f, 0.07f, -depth * 0.30f), new Vector3(width, 0.12f, depth * 0.36f), materials.Foundation);
                Cube(parent, "open vehicle bay dark floor", new Vector3(0f, 0.145f, depth * 0.16f), new Vector3(width * 0.46f, 0.04f, depth * 0.68f), materials.DarkConcrete);
                Cube(parent, "front vehicle apron worn concrete", new Vector3(0f, 0.16f, depth * 0.50f), new Vector3(width * 0.62f, 0.035f, depth * 0.20f), materials.Concrete);
            }
            else if (spec.Id == "repair_bay")
            {
                Cube(parent, "repair bay rear workshop foundation slab", new Vector3(0f, 0.06f, -depth * 0.26f), new Vector3(width * 0.92f, 0.12f, depth * 0.48f), materials.Foundation);
                Cube(parent, "repair bay flat vehicle repair pad foundation", new Vector3(0f, 0.065f, depth * 0.24f), new Vector3(width * 0.82f, 0.12f, depth * 0.58f), materials.Foundation);
                Cube(parent, "repair bay pull-on pad dark service inset", new Vector3(0f, 0.15f, depth * 0.26f), new Vector3(width * 0.60f, 0.04f, depth * 0.48f), materials.DarkConcrete);
                Cube(parent, "repair bay front approach ramp", new Vector3(0f, 0.155f, depth * 0.55f), new Vector3(width * 0.58f, 0.035f, depth * 0.16f), materials.Asphalt);
                Cube(parent, "repair bay left pad curb", new Vector3(-width * 0.36f, 0.19f, depth * 0.25f), new Vector3(0.08f, 0.12f, depth * 0.56f), materials.EdgeMetal);
                Cube(parent, "repair bay right pad curb", new Vector3(width * 0.36f, 0.19f, depth * 0.25f), new Vector3(0.08f, 0.12f, depth * 0.56f), materials.EdgeMetal);
            }
            else if (spec.Id == "refinery")
            {
                Cube(parent, "L shape refinery processing slab", new Vector3(-width * 0.18f, 0.06f, -depth * 0.02f), new Vector3(width * 0.70f, 0.12f, depth * 0.92f), materials.Foundation);
                Cube(parent, "L shape refinery harvester dock slab", new Vector3(width * 0.30f, 0.07f, depth * 0.30f), new Vector3(width * 0.44f, 0.12f, depth * 0.42f), materials.Foundation);
                Cube(parent, "ore stained dock floor", new Vector3(width * 0.30f, 0.145f, depth * 0.34f), new Vector3(width * 0.32f, 0.04f, depth * 0.34f), materials.DarkConcrete);
            }
            else if (spec.Id == "field_hospital")
            {
                Cube(parent, "cross shape hospital main slab", new Vector3(0f, 0.06f, 0f), new Vector3(width * 0.52f, 0.12f, depth), materials.Foundation);
                Cube(parent, "cross shape hospital triage wings", new Vector3(0f, 0.07f, depth * 0.05f), new Vector3(width, 0.12f, depth * 0.46f), materials.Foundation);
                Cube(parent, "hospital clean triage apron", new Vector3(0f, 0.155f, depth * 0.44f), new Vector3(width * 0.72f, 0.035f, depth * 0.18f), materials.Concrete);
            }
            else if (spec.Id == "comm_center")
            {
                Cube(parent, "offset communications square foundation", new Vector3(-width * 0.06f, 0.06f, 0f), new Vector3(width * 0.78f, 0.12f, depth * 0.78f), materials.Foundation);
                Cube(parent, "communications antenna service pad", new Vector3(width * 0.34f, 0.07f, -depth * 0.32f), new Vector3(width * 0.34f, 0.12f, depth * 0.36f), materials.Foundation);
                Cube(parent, "communications dark cable trench", new Vector3(width * 0.18f, 0.145f, -depth * 0.20f), new Vector3(width * 0.52f, 0.04f, depth * 0.12f), materials.DarkConcrete);
            }
            else if (spec.Id == "tech_center")
            {
                Cube(parent, "T shape tech lab foundation spine", new Vector3(0f, 0.06f, -depth * 0.04f), new Vector3(width * 0.48f, 0.12f, depth * 0.92f), materials.Foundation);
                Cube(parent, "T shape tech lab research wing", new Vector3(0f, 0.07f, -depth * 0.22f), new Vector3(width * 0.96f, 0.12f, depth * 0.38f), materials.Foundation);
                Cube(parent, "tech center sealed front sensor platform", new Vector3(0f, 0.155f, depth * 0.42f), new Vector3(width * 0.64f, 0.040f, depth * 0.20f), materials.DarkConcrete);
                Cube(parent, "tech center front coolant conduit left", new Vector3(-width * 0.20f, 0.190f, depth * 0.42f), new Vector3(0.055f, 0.030f, depth * 0.22f), materials.PowerGlow);
                Cube(parent, "tech center front coolant conduit right", new Vector3(width * 0.20f, 0.190f, depth * 0.42f), new Vector3(0.055f, 0.030f, depth * 0.22f), materials.PowerGlow);
            }
            else if (spec.Id == "dual_helipad")
            {
                Cube(parent, "H shape airfield central taxiway slab", new Vector3(0f, 0.06f, 0f), new Vector3(width * 0.26f, 0.12f, depth * 0.90f), materials.Foundation);
                Cube(parent, "H shape airfield left pad slab", new Vector3(-width * 0.34f, 0.07f, 0f), new Vector3(width * 0.38f, 0.12f, depth * 0.70f), materials.Foundation);
                Cube(parent, "H shape airfield right pad slab", new Vector3(width * 0.34f, 0.07f, 0f), new Vector3(width * 0.38f, 0.12f, depth * 0.70f), materials.Foundation);
                Cube(parent, "airfield dark taxi stripe", new Vector3(0f, 0.145f, 0f), new Vector3(width * 0.16f, 0.04f, depth * 0.72f), materials.DarkConcrete);
            }
            else
            {
                Cube(parent, "beveled concrete foundation slab", new Vector3(0f, 0.06f, 0f), new Vector3(width, 0.12f, depth), materials.Foundation);
                Cube(parent, "dark inset service yard", new Vector3(0f, 0.135f, 0f), new Vector3(width * 0.82f, 0.035f, depth * 0.82f), materials.DarkConcrete);
                Cube(parent, "front apron worn concrete", new Vector3(0f, 0.155f, depth * 0.36f), new Vector3(width * 0.72f, 0.035f, depth * 0.18f), materials.Concrete);
            }

            if (spec.Id == "dual_helipad")
                AddAirfieldCenterFoundationRails(parent, width, depth, materials);
            else
                AddFoundationRails(parent, width, depth, materials);
        }

        static void AddFoundationRails(Transform parent, float width, float depth, MaterialBundle materials)
        {
            Cube(parent, "rear foundation lip", new Vector3(0f, 0.22f, -depth * 0.49f), new Vector3(width, 0.18f, 0.10f), materials.EdgeMetal);
            Cube(parent, "left foundation lip", new Vector3(-width * 0.49f, 0.22f, 0f), new Vector3(0.10f, 0.18f, depth), materials.EdgeMetal);
            Cube(parent, "right foundation lip", new Vector3(width * 0.49f, 0.22f, 0f), new Vector3(0.10f, 0.18f, depth), materials.EdgeMetal);
        }

        static void BuildBuildingBody(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials)
        {
            var width = spec.Width * 0.76f;
            var depth = spec.Height * 0.76f;
            var accent = ChooseAccentMaterial(spec, materials);
            var statusGlow = ChooseStatusGlowMaterial(spec, materials);

            if (spec.HasTurret)
            {
                var baseHeight = spec.Id == "advanced_gun_tower" ? 0.88f : 0.62f;
                Cube(parent, "defense concrete plinth", new Vector3(0f, 0.26f, 0f), new Vector3(1.28f, 0.22f, 1.18f), materials.Concrete);
                Cube(parent, "defense front armor skirt", new Vector3(0f, 0.40f, 0.55f), new Vector3(1.08f, 0.30f, 0.12f), materials.EdgeMetal);
                Cube(parent, "defense rear armor skirt", new Vector3(0f, 0.40f, -0.55f), new Vector3(1.08f, 0.30f, 0.12f), materials.EdgeMetal);
                Cube(parent, "defense left armor skirt", new Vector3(-0.58f, 0.40f, 0f), new Vector3(0.12f, 0.30f, 1.02f), materials.EdgeMetal);
                Cube(parent, "defense right armor skirt", new Vector3(0.58f, 0.40f, 0f), new Vector3(0.12f, 0.30f, 1.02f), materials.EdgeMetal);
                Cylinder(parent, "armored tower base", new Vector3(0f, 0.62f, 0f), new Vector3(0.84f, baseHeight * 0.5f, 0.84f), materials.DarkMetal);
                Cylinder(parent, "textured turret top ring bearing cover", new Vector3(0f, 1.03f, 0f), new Vector3(0.64f, 0.08f, 0.64f), materials.TurretTop);
                var turret = new GameObject("Turret sweep assembly default yaw 2 oclock").transform;
                turret.SetParent(parent, false);
                turret.localPosition = new Vector3(0f, 1.17f, 0f);
                turret.localRotation = Quaternion.Euler(0f, -120f, 0f);
                if (spec.Id == "gun_tower")
                {
                    Cube(turret, "gun tower high contrast compact armored head", Vector3.zero, new Vector3(0.68f, 0.24f, 0.50f), materials.TurretArmor);
                    AddTurretTopTextureDetails(turret, "gun tower", 0.58f, 0.42f, 0.145f, materials);
                    Cube(turret, "gun tower twin machine cannon left", new Vector3(-0.18f, 0.02f, 0.64f), new Vector3(0.10f, 0.10f, 1.08f), materials.TurretBarrel);
                    Cube(turret, "gun tower twin machine cannon right", new Vector3(0.18f, 0.02f, 0.64f), new Vector3(0.10f, 0.10f, 1.08f), materials.TurretBarrel);
                    Cube(turret, "gun tower ammo feed box", new Vector3(0f, -0.02f, -0.26f), new Vector3(0.54f, 0.12f, 0.12f), materials.DefenseAccent);
                    Cube(turret, "gun tower high visibility muzzle tip pair", new Vector3(0f, 0.02f, 1.22f), new Vector3(0.52f, 0.13f, 0.14f), materials.TurretMuzzle);
                }
                else if (spec.Id == "advanced_gun_tower")
                {
                    Cube(turret, "advanced gun tower high contrast tall sensor head", Vector3.zero, new Vector3(0.72f, 0.34f, 0.58f), materials.TurretArmor);
                    AddTurretTopTextureDetails(turret, "advanced gun tower", 0.62f, 0.48f, 0.195f, materials);
                    AddAdvancedMissileBattery(turret, "left", -1f, materials);
                    AddAdvancedMissileBattery(turret, "right", 1f, materials);
                    Cube(turret, "advanced gun tower amber targeting window", new Vector3(0f, 0.16f, -0.30f), new Vector3(0.46f, 0.08f, 0.06f), materials.Warning);
                }
                else
                {
                    Cube(turret, "cannon turret high contrast broad armored head", Vector3.zero, new Vector3(0.84f, 0.28f, 0.58f), materials.TurretArmor);
                    AddTurretTopTextureDetails(turret, "cannon turret", 0.72f, 0.48f, 0.165f, materials);
                    Cube(turret, "turret red armor plate", new Vector3(0f, 0.03f, -0.30f), new Vector3(0.58f, 0.08f, 0.06f), materials.DefenseAccent);
                    Cube(turret, "cannon turret heavy single barrel", new Vector3(0f, 0f, 0.58f), new Vector3(0.25f, 0.18f, 1.12f), materials.TurretBarrel);
                    Cube(turret, "cannon turret high visibility muzzle brake", new Vector3(0f, 0f, 1.20f), new Vector3(0.34f, 0.22f, 0.16f), materials.TurretMuzzle);
                }
                AddSocket(turret, "TurretRoot", ActorPrefabSocketKind.TurretRoot, spec.Id, Vector3.zero);
                AddSocket(turret, "BarrelRoot", ActorPrefabSocketKind.BarrelRoot, spec.Id, new Vector3(0f, 0f, 0.46f));
                AddSocket(turret, "MuzzlePrimary", ActorPrefabSocketKind.MuzzlePrimary, spec.Id, new Vector3(0f, 0f, 1.04f));
                rig.turretOrBarrel = turret;
                rig.warningLight = Sphere(parent, "defense warning beacon", new Vector3(-0.46f, 1.06f, -0.38f), 0.07f, materials.Warning).transform;
                AddPerimeterLights(parent, spec, rig, statusGlow, 1.16f);
                return;
            }

            AddPrimaryMassing(parent, spec, materials, width, depth, accent);
            if (spec.VisualCategory == BuildingVisualCategory.Airfield)
            {
                AddAirfieldCenterRoofPanel(parent, width, depth, materials);
                AddAirfieldCenterStatusLights(parent, spec, rig, statusGlow, width, depth);
            }
            else
            {
                AddArmoredCorners(parent, width, depth, materials);
                AddPanelStrip(parent, "left service panels", new Vector3(-width * 0.52f, 0.62f, -depth * 0.04f), false, spec.Height + 1, materials.Panel);
                AddPanelStrip(parent, "right service panels", new Vector3(width * 0.52f, 0.62f, -depth * 0.04f), false, spec.Height + 1, materials.Panel);
                AddRoofPanelGrid(parent, width, depth, materials);
                AddRoofVents(parent, spec, materials);
                AddPerimeterLights(parent, spec, rig, statusGlow, 1.16f);
            }
            AddSignatureDetails(parent, spec, rig, materials, width, depth);
            AddBuildingIdentityPanels(parent, spec, materials, width, depth, accent);

            if (spec.HasDoor)
            {
                if (spec.Id == "field_hospital")
                    AddFieldHospitalClinicDoor(parent, spec, rig, materials, width, depth);
                else if (spec.Id == "barracks")
                    AddBarracksPersonnelDoor(parent, spec, rig, materials, width, depth, accent);
                else if (spec.Id == "war_factory")
                    AddWarFactoryVehicleDoor(parent, spec, rig, materials, width, depth);
                else
                    AddRibbedProductionDoor(parent, spec, rig, materials, width, depth, accent);
            }

            if (spec.HasProduction)
            {
                var indicator = Cube(parent, "powered production bay indicator", new Vector3(0f, 1.18f, depth * 0.18f), new Vector3(0.36f, 0.08f, 0.36f), statusGlow);
                rig.productionIndicator = indicator.transform;
            }

            if (spec.HasCrane)
                AddCrane(parent, spec, rig, materials);
            if (spec.HasTurbine)
                AddTurbines(parent, spec, rig, materials);
            if (spec.HasRadar)
                AddRadar(parent, spec, rig, materials);
            if (spec.HasDock)
                AddRefinery(parent, spec, rig, materials);
            if (spec.HasRepairArms)
                AddRepairBay(parent, spec, rig, materials);
            if (spec.VisualCategory == BuildingVisualCategory.Medical)
                AddMedicalDetails(parent, spec, materials);
            if (spec.VisualCategory == BuildingVisualCategory.Airfield)
                AddHelipads(parent, spec, rig, materials);

            if (spec.VisualCategory == BuildingVisualCategory.Airfield)
            {
                AddAirfieldCenterMotion(parent, rig, materials, width, depth);
            }
            else
            {
                var machinery = Cylinder(parent, "powered machinery rotor", new Vector3(width * 0.23f, 1.18f, -depth * 0.18f), new Vector3(0.22f, 0.05f, 0.22f), materials.Gunmetal);
                rig.machinery = machinery.transform;
                rig.warningLight = Sphere(parent, "warning beacon", new Vector3(-width * 0.34f, 1.24f, -depth * 0.24f), 0.08f, materials.Warning).transform;
                rig.damageSmoke = Sphere(parent, "damage smoke volume", new Vector3(width * 0.22f, 1.38f, -depth * 0.18f), 0.13f, materials.Smoke).transform;
            }
        }

        static void AddRibbedProductionDoor(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials, float width, float depth, Material accent)
        {
            rig.doorOpenLocalDirection = Vector3.up;
            var door = Cube(parent, "ribbed production door", new Vector3(0f, 0.42f, depth * 0.40f), new Vector3(width * 0.42f, 0.52f, 0.08f), materials.Door);
            rig.door = door.transform;
            AddSocket(door.transform, "DoorRoot", ActorPrefabSocketKind.DoorRoot, spec.Id, Vector3.zero);
            Cube(parent, "door left armored track", new Vector3(-width * 0.25f, 0.46f, depth * 0.455f), new Vector3(0.07f, 0.62f, 0.09f), materials.EdgeMetal);
            Cube(parent, "door right armored track", new Vector3(width * 0.25f, 0.46f, depth * 0.455f), new Vector3(0.07f, 0.62f, 0.09f), materials.EdgeMetal);
            Cube(parent, "door upper roller housing", new Vector3(0f, 0.82f, depth * 0.452f), new Vector3(width * 0.56f, 0.10f, 0.10f), materials.Gunmetal);
            Cube(parent, "door hazard lintel", new Vector3(0f, 0.74f, depth * 0.455f), new Vector3(width * 0.52f, 0.08f, 0.07f), accent);
            Cube(parent, "door ramp", new Vector3(0f, 0.19f, depth * 0.58f), new Vector3(width * 0.58f, 0.06f, 0.46f), materials.Asphalt);
            Cube(parent, "door exit guide line", new Vector3(0f, 0.235f, depth * 0.58f), new Vector3(width * 0.42f, 0.022f, 0.045f), accent);
        }

        static void AddBarracksPersonnelDoor(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials, float width, float depth, Material accent)
        {
            rig.doorOpenLocalDirection = Vector3.left;
            var doorZ = depth * 0.705f;
            var railZ = depth * 0.692f;
            Cube(parent, "barracks ground-level dark infantry doorway opening", new Vector3(-width * 0.04f, 0.42f, depth * 0.518f), new Vector3(width * 0.32f, 0.56f, 0.065f), materials.DarkMetal);
            Cube(parent, "barracks bright blue interior floor tile", new Vector3(-width * 0.04f, 0.195f, depth * 0.590f), new Vector3(width * 0.32f, 0.040f, 0.24f), materials.InteriorFloor);
            Cube(parent, "barracks mauve interior floor stripe", new Vector3(-width * 0.04f, 0.225f, depth * 0.590f), new Vector3(0.035f, 0.018f, 0.20f), accent);
            Cube(parent, "barracks cyan interior floor seam", new Vector3(-width * 0.04f, 0.226f, depth * 0.590f), new Vector3(width * 0.28f, 0.016f, 0.026f), materials.PowerGlow);
            var door = Cube(parent, "barracks full-height closed metal personnel door panel", new Vector3(-width * 0.04f, 0.42f, doorZ), new Vector3(width * 0.30f, 0.56f, 0.060f), materials.Panel);
            rig.door = door.transform;
            AddSocket(door.transform, "DoorRoot", ActorPrefabSocketKind.DoorRoot, spec.Id, Vector3.zero);
            Cube(door.transform, "barracks personnel door lower floor-contact edge", new Vector3(0f, -0.295f, 0.010f), new Vector3(width * 0.31f, 0.035f, 0.070f), materials.Gunmetal);
            Cube(door.transform, "mauve barracks closed door face stripe", new Vector3(width * 0.070f, 0f, 0.012f), new Vector3(0.030f, 0.40f, 0.075f), accent);
            Cube(door.transform, "barracks visible open door mauve left edge trim", new Vector3(-width * 0.155f, 0f, 0.014f), new Vector3(0.030f, 0.50f, 0.075f), accent);
            Cube(door.transform, "barracks visible open door mauve right edge trim", new Vector3(width * 0.155f, 0f, 0.014f), new Vector3(0.030f, 0.50f, 0.075f), accent);
            Cube(door.transform, "barracks visible open door mauve top edge trim", new Vector3(0f, 0.285f, 0.014f), new Vector3(width * 0.32f, 0.030f, 0.075f), accent);
            Cube(parent, "barracks personnel door visible open parking rail upper", new Vector3(-width * 0.34f, 0.72f, railZ), new Vector3(width * 0.28f, 0.045f, 0.075f), materials.Gunmetal);
            Cube(parent, "barracks personnel door visible open parking rail lower", new Vector3(-width * 0.34f, 0.20f, railZ), new Vector3(width * 0.28f, 0.035f, 0.075f), materials.Gunmetal);
            Cube(parent, "barracks personnel door right full-height jamb", new Vector3(width * 0.16f, 0.43f, railZ), new Vector3(0.040f, 0.60f, 0.075f), materials.EdgeMetal);
            Cube(parent, "barracks personnel door upper slide rail", new Vector3(-width * 0.04f, 0.72f, railZ), new Vector3(width * 0.58f, 0.055f, 0.075f), materials.Gunmetal);
            Cube(parent, "barracks outside asphalt infantry exit threshold", new Vector3(-width * 0.04f, 0.175f, depth * 0.740f), new Vector3(width * 0.38f, 0.035f, 0.16f), materials.Asphalt);
            Cube(parent, "mauve barracks personnel door status light", new Vector3(-width * 0.22f, 0.60f, doorZ), new Vector3(0.045f, 0.08f, 0.03f), accent);
        }

        static void AddFieldHospitalClinicDoor(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials, float width, float depth)
        {
            rig.doorOpenLocalDirection = Vector3.left;
            var doorZ = depth * 0.705f;
            var railZ = depth * 0.692f;
            Cube(parent, "field hospital ground-level dark clinic doorway opening", new Vector3(0f, 0.42f, depth * 0.575f), new Vector3(width * 0.32f, 0.56f, 0.065f), materials.DarkMetal);
            Cube(parent, "field hospital bright teal interior floor tile", new Vector3(0f, 0.195f, depth * 0.645f), new Vector3(width * 0.32f, 0.040f, 0.24f), materials.InteriorFloor);
            Cube(parent, "field hospital green triage interior floor stripe", new Vector3(0f, 0.226f, depth * 0.645f), new Vector3(0.040f, 0.018f, 0.20f), materials.Medical);
            Cube(parent, "field hospital cyan interior floor seam", new Vector3(0f, 0.227f, depth * 0.645f), new Vector3(width * 0.28f, 0.016f, 0.026f), materials.PowerGlow);

            var door = Cube(parent, "field hospital full-height closed metal clinic personnel door panel", new Vector3(0f, 0.42f, doorZ), new Vector3(width * 0.30f, 0.56f, 0.060f), materials.Panel);
            rig.door = door.transform;
            AddSocket(door.transform, "DoorRoot", ActorPrefabSocketKind.DoorRoot, spec.Id, Vector3.zero);
            Cube(door.transform, "field hospital clinic personnel door lower floor-contact edge", new Vector3(0f, -0.295f, 0.010f), new Vector3(width * 0.31f, 0.035f, 0.070f), materials.Medical);
            Cube(door.transform, "green field hospital closed door face stripe", new Vector3(width * 0.075f, 0f, 0.012f), new Vector3(0.030f, 0.40f, 0.075f), materials.Medical);
            Cube(door.transform, "field hospital visible open door green left edge trim", new Vector3(-width * 0.155f, 0f, 0.014f), new Vector3(0.030f, 0.50f, 0.075f), materials.Medical);
            Cube(door.transform, "field hospital visible open door green right edge trim", new Vector3(width * 0.155f, 0f, 0.014f), new Vector3(0.030f, 0.50f, 0.075f), materials.Medical);
            Cube(door.transform, "field hospital visible open door green top edge trim", new Vector3(0f, 0.285f, 0.014f), new Vector3(width * 0.32f, 0.030f, 0.075f), materials.Medical);

            Cube(parent, "field hospital clinic door visible open parking rail upper", new Vector3(-width * 0.34f, 0.72f, railZ), new Vector3(width * 0.28f, 0.045f, 0.075f), materials.Gunmetal);
            Cube(parent, "field hospital clinic door visible open parking rail lower", new Vector3(-width * 0.34f, 0.20f, railZ), new Vector3(width * 0.28f, 0.035f, 0.075f), materials.Gunmetal);
            Cube(parent, "field hospital clinic door right full-height jamb", new Vector3(width * 0.16f, 0.43f, railZ), new Vector3(0.040f, 0.60f, 0.075f), materials.EdgeMetal);
            Cube(parent, "field hospital clinic door upper slide rail", new Vector3(0f, 0.72f, railZ), new Vector3(width * 0.58f, 0.055f, 0.075f), materials.Gunmetal);
            Cube(parent, "field hospital clean outside infantry entry threshold", new Vector3(0f, 0.175f, depth * 0.740f), new Vector3(width * 0.40f, 0.035f, 0.16f), materials.Asphalt);
            Cube(parent, "green field hospital clinic door status light", new Vector3(-width * 0.24f, 0.60f, doorZ), new Vector3(0.045f, 0.08f, 0.03f), materials.Medical);
        }

        static void AddWarFactoryVehicleDoor(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials, float width, float depth)
        {
            rig.doorOpenLocalDirection = Vector3.up;
            var door = new GameObject("war factory full-height overhead sliding vehicle bay door assembly").transform;
            door.SetParent(parent, false);
            door.localPosition = new Vector3(0f, 0.58f, depth * 0.505f);
            rig.door = door;
            Cube(door, "war factory full-height overhead sliding vehicle bay door panel", Vector3.zero, new Vector3(width * 0.48f, 0.76f, 0.075f), materials.Door);
            Cube(door, "war factory moving door lower floor-contact edge", new Vector3(0f, -0.39f, 0.012f), new Vector3(width * 0.50f, 0.035f, 0.09f), materials.Hazard);
            for (var i = 0; i < 4; i++)
            {
                var y = -0.22f + i * 0.15f;
                Cube(door, "war factory moving ribbed door slat " + i, new Vector3(0f, y, 0.014f), new Vector3(width * 0.48f, 0.032f, 0.092f), materials.Gunmetal);
            }

            AddSocket(door, "DoorRoot", ActorPrefabSocketKind.DoorRoot, spec.Id, Vector3.zero);
            Cube(parent, "war factory bay left clear-side full-height door track", new Vector3(-width * 0.31f, 0.58f, depth * 0.515f), new Vector3(0.06f, 0.86f, 0.08f), materials.EdgeMetal);
            Cube(parent, "war factory bay right clear-side full-height door track", new Vector3(width * 0.31f, 0.58f, depth * 0.515f), new Vector3(0.06f, 0.86f, 0.08f), materials.EdgeMetal);
            Cube(parent, "war factory high overhead door roller housing", new Vector3(0f, 0.99f, depth * 0.518f), new Vector3(width * 0.60f, 0.10f, 0.09f), materials.Gunmetal);
            Cube(parent, "war factory door bottom floor seal", new Vector3(0f, 0.205f, depth * 0.522f), new Vector3(width * 0.50f, 0.035f, 0.08f), materials.Gunmetal);
            Cube(parent, "war factory unobstructed vehicle exit lane", new Vector3(0f, 0.205f, depth * 0.64f), new Vector3(width * 0.48f, 0.045f, depth * 0.32f), materials.Asphalt);
            Cube(parent, "war factory painted drive-out centerline", new Vector3(0f, 0.238f, depth * 0.64f), new Vector3(0.055f, 0.018f, depth * 0.28f), materials.ProductionAccent);
        }

        static void AddPrimaryMassing(Transform parent, BuildingSpec spec, MaterialBundle materials, float width, float depth, Material accent)
        {
            if (spec.Id == "fabrication_hub")
            {
                Cube(parent, "fabrication hub oversized tall rear fabrication hall", new Vector3(-width * 0.14f, 0.68f, -depth * 0.18f), new Vector3(width * 0.68f, 1.02f, depth * 0.58f), materials.WornMetal);
                Cube(parent, "fabrication hub right L shaped machine annex", new Vector3(width * 0.32f, 0.54f, -depth * 0.12f), new Vector3(width * 0.30f, 0.74f, depth * 0.76f), materials.DarkMetal);
                Cube(parent, "fabrication hub broad low front assembly bay", new Vector3(-width * 0.12f, 0.40f, depth * 0.30f), new Vector3(width * 0.64f, 0.42f, depth * 0.32f), materials.Concrete);
                Cube(parent, "fabrication hub oversized roof gantry cap", new Vector3(-width * 0.14f, 1.24f, -depth * 0.18f), new Vector3(width * 0.62f, 0.16f, depth * 0.50f), materials.Concrete);
            }
            else if (spec.Id == "power_plant")
            {
                Cube(parent, "power plant compact control block", new Vector3(0f, 0.50f, depth * 0.04f), new Vector3(width * 0.62f, 0.62f, depth * 0.52f), materials.WornMetal);
                Cylinder(parent, "power plant left round generator pod", new Vector3(-width * 0.33f, 0.58f, -depth * 0.20f), new Vector3(0.22f, 0.42f, 0.22f), materials.DarkMetal);
                Cylinder(parent, "power plant right round generator pod", new Vector3(width * 0.33f, 0.58f, -depth * 0.20f), new Vector3(0.22f, 0.42f, 0.22f), materials.DarkMetal);
                Cube(parent, "power plant small roof service cap", new Vector3(0f, 0.90f, depth * 0.04f), new Vector3(width * 0.54f, 0.14f, depth * 0.44f), materials.Concrete);
            }
            else if (spec.Id == "advanced_power_plant")
            {
                Cube(parent, "advanced power central tall reactor spine", new Vector3(0f, 0.66f, -depth * 0.05f), new Vector3(width * 0.34f, 1.06f, depth * 0.84f), materials.WornMetal);
                Cube(parent, "advanced power wide left reactor wing", new Vector3(-width * 0.36f, 0.56f, depth * 0.10f), new Vector3(width * 0.30f, 0.78f, depth * 0.46f), materials.DarkMetal);
                Cube(parent, "advanced power wide right reactor wing", new Vector3(width * 0.36f, 0.56f, depth * 0.10f), new Vector3(width * 0.30f, 0.78f, depth * 0.46f), materials.DarkMetal);
                Cube(parent, "advanced power extra wide reinforced top crossbar", new Vector3(0f, 1.22f, depth * 0.10f), new Vector3(width * 0.96f, 0.14f, depth * 0.30f), materials.Concrete);
            }
            else if (spec.Id == "barracks")
            {
                Cube(parent, "barracks long dormitory wing", new Vector3(-width * 0.18f, 0.46f, -depth * 0.10f), new Vector3(width * 0.58f, 0.56f, depth * 0.78f), materials.WornMetal);
                Cube(parent, "barracks offset command office", new Vector3(width * 0.28f, 0.58f, depth * 0.18f), new Vector3(width * 0.36f, 0.78f, depth * 0.38f), materials.Concrete);
                Cube(parent, "barracks low covered muster porch", new Vector3(0f, 0.34f, depth * 0.42f), new Vector3(width * 0.74f, 0.28f, depth * 0.22f), materials.DarkConcrete);
                Cube(parent, "barracks command roof red stripe", new Vector3(width * 0.28f, 1.00f, depth * 0.18f), new Vector3(width * 0.30f, 0.04f, depth * 0.10f), accent);
            }
            else if (spec.Id == "war_factory")
            {
                Cube(parent, "war factory left heavy assembly wall", new Vector3(-width * 0.30f, 0.58f, -depth * 0.02f), new Vector3(width * 0.30f, 0.86f, depth * 0.86f), materials.WornMetal);
                Cube(parent, "war factory right heavy assembly wall", new Vector3(width * 0.30f, 0.58f, -depth * 0.02f), new Vector3(width * 0.30f, 0.86f, depth * 0.86f), materials.WornMetal);
                Cube(parent, "war factory rear crane house", new Vector3(0f, 0.74f, -depth * 0.36f), new Vector3(width * 0.78f, 1.02f, depth * 0.28f), materials.DarkMetal);
                Cube(parent, "war factory clear internal vehicle spawn bay floor", new Vector3(0f, 0.225f, depth * 0.15f), new Vector3(width * 0.44f, 0.055f, depth * 0.66f), materials.Asphalt);
                Cube(parent, "war factory clear vehicle exit throat", new Vector3(0f, 0.235f, depth * 0.48f), new Vector3(width * 0.48f, 0.045f, depth * 0.24f), materials.Asphalt);
                Cube(parent, "war factory high roof bridge", new Vector3(0f, 1.22f, -depth * 0.02f), new Vector3(width * 0.86f, 0.16f, depth * 0.20f), materials.Concrete);
            }
            else if (spec.Id == "refinery")
            {
                Cube(parent, "refinery tall processor block", new Vector3(-width * 0.24f, 0.66f, -depth * 0.08f), new Vector3(width * 0.48f, 1.04f, depth * 0.72f), materials.WornMetal);
                Cube(parent, "refinery low ore intake dock arm", new Vector3(width * 0.32f, 0.38f, depth * 0.26f), new Vector3(width * 0.42f, 0.40f, depth * 0.32f), materials.DarkMetal);
                Cube(parent, "refinery rear maintenance shed", new Vector3(width * 0.25f, 0.48f, -depth * 0.34f), new Vector3(width * 0.36f, 0.52f, depth * 0.26f), materials.Concrete);
                Cube(parent, "refinery processor roof cap", new Vector3(-width * 0.24f, 1.22f, -depth * 0.08f), new Vector3(width * 0.42f, 0.14f, depth * 0.60f), materials.Concrete);
            }
            else if (spec.Id == "field_hospital")
            {
                Cube(parent, "field hospital central clinic tower", new Vector3(0f, 0.58f, -depth * 0.02f), new Vector3(width * 0.42f, 0.82f, depth * 0.62f), materials.WornMetal);
                Cube(parent, "field hospital left triage wing", new Vector3(-width * 0.34f, 0.40f, depth * 0.08f), new Vector3(width * 0.32f, 0.44f, depth * 0.38f), materials.Concrete);
                Cube(parent, "field hospital right triage wing", new Vector3(width * 0.34f, 0.40f, depth * 0.08f), new Vector3(width * 0.32f, 0.44f, depth * 0.38f), materials.Concrete);
                Cube(parent, "field hospital raised clean roof", new Vector3(0f, 1.02f, -depth * 0.02f), new Vector3(width * 0.36f, 0.12f, depth * 0.52f), materials.Concrete);
            }
            else if (spec.Id == "comm_center")
            {
                Cube(parent, "comm center compact operations bunker", new Vector3(-width * 0.08f, 0.54f, depth * 0.06f), new Vector3(width * 0.66f, 0.76f, depth * 0.58f), materials.WornMetal);
                Cube(parent, "comm center rear antenna tower base", new Vector3(width * 0.30f, 0.78f, -depth * 0.34f), new Vector3(width * 0.28f, 1.12f, depth * 0.24f), materials.DarkMetal);
                Cube(parent, "comm center stepped roof cap", new Vector3(-width * 0.08f, 0.98f, depth * 0.06f), new Vector3(width * 0.58f, 0.14f, depth * 0.50f), materials.Concrete);
            }
            else if (spec.Id == "repair_bay")
            {
                Cube(parent, "repair bay rear half diagnostic shop", new Vector3(0f, 0.66f, -depth * 0.32f), new Vector3(width * 0.76f, 0.88f, depth * 0.34f), materials.WornMetal);
                Cube(parent, "repair bay rear equipment tower left", new Vector3(-width * 0.32f, 0.82f, -depth * 0.18f), new Vector3(width * 0.18f, 1.02f, depth * 0.22f), materials.DarkMetal);
                Cube(parent, "repair bay rear equipment tower right", new Vector3(width * 0.32f, 0.82f, -depth * 0.18f), new Vector3(width * 0.18f, 1.02f, depth * 0.22f), materials.DarkMetal);
                Cube(parent, "repair bay exposed flat vehicle pad", new Vector3(0f, 0.28f, depth * 0.26f), new Vector3(width * 0.58f, 0.14f, depth * 0.54f), materials.DarkConcrete);
                Cube(parent, "repair bay left small arm pedestal", new Vector3(-width * 0.36f, 0.48f, depth * 0.16f), new Vector3(width * 0.16f, 0.38f, depth * 0.22f), materials.Gunmetal);
                Cube(parent, "repair bay right small arm pedestal", new Vector3(width * 0.36f, 0.48f, depth * 0.16f), new Vector3(width * 0.16f, 0.38f, depth * 0.22f), materials.Gunmetal);
            }
            else if (spec.Id == "tech_center")
            {
                Cube(parent, "tech center central research tower", new Vector3(0f, 0.92f, -depth * 0.06f), new Vector3(width * 0.44f, 1.54f, depth * 0.74f), materials.WornMetal);
                Cube(parent, "tech center taller glass command crown", new Vector3(0f, 1.78f, -depth * 0.06f), new Vector3(width * 0.36f, 0.22f, depth * 0.54f), materials.PowerGlow);
                Cube(parent, "tech center left advanced lab wing", new Vector3(-width * 0.34f, 0.52f, -depth * 0.22f), new Vector3(width * 0.36f, 0.66f, depth * 0.34f), materials.Concrete);
                Cube(parent, "tech center right advanced lab wing", new Vector3(width * 0.34f, 0.52f, -depth * 0.22f), new Vector3(width * 0.36f, 0.66f, depth * 0.34f), materials.Concrete);
                Cube(parent, "tech center sealed front quantum array block", new Vector3(0f, 0.58f, depth * 0.34f), new Vector3(width * 0.64f, 0.62f, depth * 0.22f), materials.DarkMetal);
                Cube(parent, "tech center cyan front analysis screen", new Vector3(0f, 0.74f, depth * 0.47f), new Vector3(width * 0.48f, 0.18f, 0.040f), materials.PowerGlow);
                Cube(parent, "tech center front cooling fin left", new Vector3(-width * 0.34f, 0.66f, depth * 0.46f), new Vector3(0.050f, 0.54f, 0.055f), materials.EdgeMetal);
                Cube(parent, "tech center front cooling fin right", new Vector3(width * 0.34f, 0.66f, depth * 0.46f), new Vector3(0.050f, 0.54f, 0.055f), materials.EdgeMetal);
            }
            else if (spec.Id == "dual_helipad")
            {
                Cube(parent, "dual helipad center air control tower base", new Vector3(0f, 0.56f, -depth * 0.32f), new Vector3(width * 0.24f, 0.78f, depth * 0.28f), materials.WornMetal);
                Cube(parent, "dual helipad center air control glass cab", new Vector3(0f, 1.02f, -depth * 0.32f), new Vector3(width * 0.18f, 0.20f, depth * 0.20f), materials.PowerGlow);
                Cube(parent, "dual helipad center taxi control spine", new Vector3(0f, 0.30f, depth * 0.10f), new Vector3(width * 0.20f, 0.26f, depth * 0.72f), materials.DarkConcrete);
            }
            else
            {
                Cube(parent, "main armored building mass", new Vector3(0f, 0.55f, -depth * 0.05f), new Vector3(width, 0.82f, depth * 0.78f), materials.WornMetal);
                Cube(parent, "roof concrete cap", new Vector3(0f, 1.00f, -depth * 0.05f), new Vector3(width * 0.94f, 0.16f, depth * 0.70f), materials.Concrete);
            }

            Cube(parent, "faction command roof stripe", new Vector3(0f, 1.125f, depth * 0.245f), new Vector3(width * 0.50f, 0.035f, 0.08f), accent);
            if (spec.Id == "tech_center")
                Cube(parent, "tech center sealed front data facade not a door", new Vector3(0f, 0.64f, depth * 0.495f), new Vector3(width * 0.56f, 0.16f, 0.030f), materials.PowerGlow);
            else
                Cube(parent, "deep front facade shadow", new Vector3(0f, 0.64f, depth * 0.355f), new Vector3(width * 0.66f, 0.18f, 0.035f), materials.DarkMetal);
            Cube(parent, "rear service shadow", new Vector3(0f, 0.78f, -depth * 0.475f), new Vector3(width * 0.66f, 0.16f, 0.035f), materials.DarkMetal);
        }

        static Material ChooseAccentMaterial(BuildingSpec spec, MaterialBundle materials)
        {
            if (spec.VisualCategory == BuildingVisualCategory.Defense)
                return materials.DefenseAccent;
            if (spec.VisualCategory == BuildingVisualCategory.Power)
                return materials.PowerGlow;
            if (spec.VisualCategory == BuildingVisualCategory.Medical || spec.VisualCategory == BuildingVisualCategory.Repair)
                return materials.Medical;
            if (spec.VisualCategory == BuildingVisualCategory.Construction ||
                spec.VisualCategory == BuildingVisualCategory.Production ||
                spec.VisualCategory == BuildingVisualCategory.Airfield)
                return materials.ProductionAccent;
            return materials.FactionAccent;
        }

        static Material ChooseStatusGlowMaterial(BuildingSpec spec, MaterialBundle materials)
        {
            if (spec.VisualCategory == BuildingVisualCategory.Power)
                return materials.PowerGlow;
            if (spec.VisualCategory == BuildingVisualCategory.Medical || spec.VisualCategory == BuildingVisualCategory.Repair)
                return materials.Medical;
            if (spec.VisualCategory == BuildingVisualCategory.Construction ||
                spec.VisualCategory == BuildingVisualCategory.Production ||
                spec.VisualCategory == BuildingVisualCategory.Airfield)
                return materials.ProductionAccent;
            return materials.PowerGlow;
        }

        static void AddArmoredCorners(Transform parent, float width, float depth, MaterialBundle materials)
        {
            for (var sx = -1; sx <= 1; sx += 2)
            {
                for (var sz = -1; sz <= 1; sz += 2)
                {
                    Cube(parent, "vertical chamfer armor " + sx + " " + sz, new Vector3(sx * width * 0.46f, 0.66f, -depth * 0.05f + sz * depth * 0.34f), new Vector3(0.12f, 0.76f, 0.14f), materials.EdgeMetal);
                    Cube(parent, "corner grime foot " + sx + " " + sz, new Vector3(sx * width * 0.46f, 0.27f, -depth * 0.05f + sz * depth * 0.34f), new Vector3(0.20f, 0.08f, 0.22f), materials.DarkConcrete);
                }
            }

            Cube(parent, "front roof armor rail", new Vector3(0f, 1.14f, depth * 0.31f), new Vector3(width * 0.86f, 0.08f, 0.06f), materials.EdgeMetal);
            Cube(parent, "rear roof armor rail", new Vector3(0f, 1.14f, -depth * 0.41f), new Vector3(width * 0.86f, 0.08f, 0.06f), materials.EdgeMetal);
            Cube(parent, "left roof armor rail", new Vector3(-width * 0.46f, 1.14f, -depth * 0.05f), new Vector3(0.06f, 0.08f, depth * 0.68f), materials.EdgeMetal);
            Cube(parent, "right roof armor rail", new Vector3(width * 0.46f, 1.14f, -depth * 0.05f), new Vector3(0.06f, 0.08f, depth * 0.68f), materials.EdgeMetal);
        }

        static void AddAirfieldCenterFoundationRails(Transform parent, float width, float depth, MaterialBundle materials)
        {
            Cube(parent, "center-only airfield front spine rail", new Vector3(0f, 0.19f, depth * 0.42f), new Vector3(width * 0.24f, 0.08f, 0.06f), materials.EdgeMetal);
            Cube(parent, "center-only airfield rear spine rail", new Vector3(0f, 0.19f, -depth * 0.42f), new Vector3(width * 0.24f, 0.08f, 0.06f), materials.EdgeMetal);
            Cube(parent, "center-only airfield left spine lip", new Vector3(-width * 0.12f, 0.19f, 0f), new Vector3(0.045f, 0.08f, depth * 0.84f), materials.EdgeMetal);
            Cube(parent, "center-only airfield right spine lip", new Vector3(width * 0.12f, 0.19f, 0f), new Vector3(0.045f, 0.08f, depth * 0.84f), materials.EdgeMetal);
        }

        static void AddSignatureDetails(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials, float width, float depth)
        {
            if (spec.Id == "fabrication_hub")
            {
                Cube(parent, "fabricator assembly pit", new Vector3(0f, 0.28f, 0.28f), new Vector3(width * 0.64f, 0.10f, depth * 0.32f), materials.DarkConcrete);
                Cube(parent, "fabricator gantry rail left", new Vector3(-width * 0.34f, 1.22f, 0.20f), new Vector3(0.08f, 0.22f, depth * 0.62f), materials.Gunmetal);
                Cube(parent, "fabricator gantry rail right", new Vector3(width * 0.34f, 1.22f, 0.20f), new Vector3(0.08f, 0.22f, depth * 0.62f), materials.Gunmetal);
                Cube(parent, "mauve fabrication assembly glow bench", new Vector3(0f, 0.62f, 0.18f), new Vector3(width * 0.38f, 0.08f, 0.08f), materials.ProductionAccent);
            }
            else if (spec.Id == "barracks")
            {
                Cube(parent, "barracks command block", new Vector3(-width * 0.24f, 1.18f, -depth * 0.20f), new Vector3(width * 0.32f, 0.28f, depth * 0.26f), materials.Concrete);
                Cube(parent, "barracks ready room", new Vector3(width * 0.25f, 0.92f, -depth * 0.24f), new Vector3(width * 0.34f, 0.34f, depth * 0.22f), materials.DarkMetal);
                Cylinder(parent, "barracks radio mast", new Vector3(width * 0.36f, 1.42f, -depth * 0.33f), new Vector3(0.035f, 0.34f, 0.035f), materials.EdgeMetal);
                Cube(parent, "mauve barracks porch light strip", new Vector3(0f, 0.86f, depth * 0.47f), new Vector3(width * 0.36f, 0.055f, 0.06f), materials.ProductionAccent);
                Cube(parent, "barracks covered entry awning", new Vector3(0f, 0.78f, depth * 0.56f), new Vector3(width * 0.54f, 0.08f, 0.22f), materials.Concrete);
                Cube(parent, "barracks left muster rail", new Vector3(-width * 0.38f, 0.34f, depth * 0.58f), new Vector3(0.08f, 0.18f, 0.46f), materials.EdgeMetal);
                Cube(parent, "barracks right muster rail", new Vector3(width * 0.38f, 0.34f, depth * 0.58f), new Vector3(0.08f, 0.18f, 0.46f), materials.EdgeMetal);
                Cube(parent, "mauve barracks troop-ready roof panel", new Vector3(width * 0.02f, 1.18f, depth * 0.02f), new Vector3(width * 0.36f, 0.045f, depth * 0.18f), materials.ProductionAccent);
            }
            else if (spec.Id == "war_factory")
            {
                Cube(parent, "factory clear vehicle spawn marker inside bay", new Vector3(0f, 0.285f, depth * 0.12f), new Vector3(width * 0.34f, 0.026f, depth * 0.28f), materials.DarkMetal);
                Cube(parent, "factory overhead gantry", new Vector3(0f, 1.30f, 0.08f), new Vector3(width * 0.72f, 0.10f, 0.16f), materials.Gunmetal);
                Cube(parent, "mauve factory gantry moving carriage", new Vector3(width * 0.18f, 1.22f, 0.08f), new Vector3(width * 0.14f, 0.10f, 0.24f), materials.ProductionAccent);
                Cube(parent, "factory left side conveyor clear of exit lane", new Vector3(-width * 0.34f, 0.30f, 0.20f), new Vector3(0.10f, 0.10f, depth * 0.54f), materials.Asphalt);
                Cube(parent, "factory right side conveyor clear of exit lane", new Vector3(width * 0.34f, 0.30f, 0.20f), new Vector3(0.10f, 0.10f, depth * 0.54f), materials.Asphalt);
                Cube(parent, "factory painted center exit guide only", new Vector3(0f, 0.292f, 0.34f), new Vector3(0.07f, 0.018f, depth * 0.62f), materials.Hazard);
                Cylinder(parent, "factory exhaust stack left", new Vector3(-width * 0.42f, 1.20f, -depth * 0.34f), new Vector3(0.12f, 0.32f, 0.12f), materials.DarkMetal);
                Cylinder(parent, "factory exhaust stack right", new Vector3(width * 0.42f, 1.20f, -depth * 0.34f), new Vector3(0.12f, 0.32f, 0.12f), materials.DarkMetal);
                Cube(parent, "factory side armor braces left", new Vector3(-width * 0.53f, 0.74f, 0.04f), new Vector3(0.08f, 0.36f, depth * 0.50f), materials.EdgeMetal);
                Cube(parent, "factory side armor braces right", new Vector3(width * 0.53f, 0.74f, 0.04f), new Vector3(0.08f, 0.36f, depth * 0.50f), materials.EdgeMetal);
            }
            else if (spec.Id == "power_plant")
            {
                Cube(parent, "power plant heat exchanger left", new Vector3(-width * 0.34f, 1.12f, depth * 0.03f), new Vector3(width * 0.16f, 0.10f, depth * 0.40f), materials.Gunmetal);
                Cube(parent, "power plant heat exchanger right", new Vector3(width * 0.34f, 1.12f, depth * 0.03f), new Vector3(width * 0.16f, 0.10f, depth * 0.40f), materials.Gunmetal);
                Cube(parent, "power plant electric blue service band", new Vector3(0f, 0.74f, depth * 0.45f), new Vector3(width * 0.58f, 0.10f, 0.05f), materials.PowerGlow);
                Cube(parent, "power plant lower intake shadow", new Vector3(0f, 0.50f, -depth * 0.38f), new Vector3(width * 0.62f, 0.16f, 0.06f), materials.DarkMetal);
            }
            else if (spec.Id == "advanced_power_plant")
            {
                Cube(parent, "advanced reactor armored rib left", new Vector3(-width * 0.40f, 0.82f, 0.18f), new Vector3(0.10f, 0.54f, depth * 0.45f), materials.EdgeMetal);
                Cube(parent, "advanced reactor armored rib right", new Vector3(width * 0.40f, 0.82f, 0.18f), new Vector3(0.10f, 0.54f, depth * 0.45f), materials.EdgeMetal);
                Cube(parent, "advanced power core window", new Vector3(0f, 0.86f, depth * 0.44f), new Vector3(width * 0.44f, 0.18f, 0.05f), materials.PowerGlow);
                Cube(parent, "advanced reactor service spine", new Vector3(0f, 1.18f, -depth * 0.30f), new Vector3(width * 0.58f, 0.08f, 0.10f), materials.Gunmetal);
                Cube(parent, "advanced reactor electric blue service stripe", new Vector3(0f, 1.20f, -depth * 0.16f), new Vector3(width * 0.44f, 0.035f, 0.06f), materials.PowerGlow);
            }
            else if (spec.Id == "tech_center")
            {
                Cylinder(parent, "tech sensor pod left", new Vector3(-width * 0.28f, 1.42f, -depth * 0.24f), new Vector3(0.18f, 0.12f, 0.18f), materials.PowerGlow);
                Cylinder(parent, "tech sensor pod right", new Vector3(width * 0.28f, 1.42f, -depth * 0.24f), new Vector3(0.18f, 0.12f, 0.18f), materials.PowerGlow);
                Cylinder(parent, "tech center forward micro sensor turret", new Vector3(0f, 1.18f, depth * 0.36f), new Vector3(0.12f, 0.09f, 0.12f), materials.PowerGlow);
                Cylinder(parent, "tech center high spire antenna", new Vector3(0f, 2.02f, -depth * 0.06f), new Vector3(0.035f, 0.34f, 0.035f), materials.EdgeMetal);
                Cylinder(parent, "tech center secondary spire antenna left", new Vector3(-width * 0.18f, 1.78f, -depth * 0.16f), new Vector3(0.026f, 0.24f, 0.026f), materials.EdgeMetal);
                Cylinder(parent, "tech center secondary spire antenna right", new Vector3(width * 0.18f, 1.78f, -depth * 0.16f), new Vector3(0.026f, 0.24f, 0.026f), materials.EdgeMetal);
                Sphere(parent, "tech center glowing apex node", new Vector3(0f, 2.38f, -depth * 0.06f), 0.07f, materials.PowerGlow);
                Cube(parent, "tech center advanced cyan vertical data spine", new Vector3(0f, 1.22f, depth * 0.38f), new Vector3(0.09f, 0.82f, 0.045f), materials.PowerGlow);
                Cube(parent, "tech data window band", new Vector3(0f, 0.82f, depth * 0.45f), new Vector3(width * 0.58f, 0.12f, 0.05f), materials.PowerGlow);
                Cube(parent, "tech center roof quantum ring front", new Vector3(0f, 1.55f, depth * 0.20f), new Vector3(width * 0.44f, 0.035f, 0.055f), materials.PowerGlow);
                Cube(parent, "tech center roof quantum ring rear", new Vector3(0f, 1.55f, -depth * 0.30f), new Vector3(width * 0.44f, 0.035f, 0.055f), materials.PowerGlow);
            }
            else if (spec.Id == "comm_center")
            {
                Cube(parent, "communications lower window band", new Vector3(0f, 0.72f, depth * 0.46f), new Vector3(width * 0.54f, 0.10f, 0.05f), materials.PowerGlow);
                Cylinder(parent, "small comm antenna", new Vector3(-width * 0.35f, 1.34f, -depth * 0.28f), new Vector3(0.032f, 0.28f, 0.032f), materials.EdgeMetal);
            }
            else if (spec.Id == "field_hospital")
            {
                Cube(parent, "field hospital triage bay", new Vector3(0f, 0.48f, depth * 0.44f), new Vector3(width * 0.52f, 0.18f, 0.18f), materials.Medical);
                Cube(parent, "field hospital roof cross long", new Vector3(0f, 1.17f, -depth * 0.02f), new Vector3(width * 0.46f, 0.035f, 0.08f), materials.Medical);
                Cube(parent, "field hospital roof cross short", new Vector3(0f, 1.172f, -depth * 0.02f), new Vector3(0.08f, 0.036f, depth * 0.34f), materials.Medical);
                Cube(parent, "field hospital front canopy", new Vector3(0f, 0.70f, depth * 0.58f), new Vector3(width * 0.64f, 0.06f, 0.20f), materials.Concrete);
            }
            else if (spec.Id == "refinery")
            {
                Cube(parent, "refinery ore sorting grate", new Vector3(width * 0.19f, 1.12f, depth * 0.13f), new Vector3(width * 0.30f, 0.04f, depth * 0.22f), materials.Asphalt);
                Cube(parent, "refinery stained roof panel", new Vector3(-width * 0.22f, 1.125f, depth * 0.08f), new Vector3(width * 0.28f, 0.035f, depth * 0.22f), materials.DarkConcrete);
                Cube(parent, "refinery ore hopper mouth", new Vector3(width * 0.38f, 0.66f, depth * 0.46f), new Vector3(width * 0.22f, 0.22f, 0.08f), materials.DarkMetal);
                Cube(parent, "refinery red processing band", new Vector3(-width * 0.20f, 0.76f, depth * 0.46f), new Vector3(width * 0.36f, 0.08f, 0.05f), materials.FactionAccent);
            }
            else if (spec.Id == "repair_bay")
            {
                Cube(parent, "repair bay pull-on service pad", new Vector3(0f, 0.38f, depth * 0.32f), new Vector3(width * 0.60f, 0.08f, depth * 0.50f), materials.DarkConcrete);
                Cube(parent, "repair bay left wheel guide stripe", new Vector3(-width * 0.18f, 0.45f, depth * 0.32f), new Vector3(0.055f, 0.035f, depth * 0.44f), materials.Hazard);
                Cube(parent, "repair bay right wheel guide stripe", new Vector3(width * 0.18f, 0.45f, depth * 0.32f), new Vector3(0.055f, 0.035f, depth * 0.44f), materials.Hazard);
                Cube(parent, "repair bay vehicle stop bar", new Vector3(0f, 0.455f, depth * 0.08f), new Vector3(width * 0.48f, 0.036f, 0.06f), materials.Hazard);
            }
            else if (spec.Id == "dual_helipad")
            {
                Cube(parent, "air control block", new Vector3(0f, 1.12f, -depth * 0.28f), new Vector3(width * 0.34f, 0.24f, depth * 0.18f), materials.Concrete);
                Cube(parent, "mauve air control production status slit", new Vector3(0f, 1.26f, -depth * 0.16f), new Vector3(width * 0.26f, 0.04f, 0.05f), materials.ProductionAccent);
            }
        }

        static void AddBuildingIdentityPanels(Transform parent, BuildingSpec spec, MaterialBundle materials, float width, float depth, Material accent)
        {
            if (spec.Id == "fabrication_hub")
            {
                Cube(parent, "mauve fabrication hub construction L marker long", new Vector3(-width * 0.10f, 1.24f, depth * 0.16f), new Vector3(width * 0.46f, 0.035f, 0.06f), materials.ProductionAccent);
                Cube(parent, "mauve fabrication hub construction L marker short", new Vector3(width * 0.12f, 1.242f, -depth * 0.02f), new Vector3(0.06f, 0.036f, depth * 0.36f), materials.ProductionAccent);
            }
            else if (spec.Id == "power_plant")
            {
                Cube(parent, "power plant cyan reactor identity bar left", new Vector3(-width * 0.18f, 1.22f, depth * 0.02f), new Vector3(0.08f, 0.04f, depth * 0.38f), materials.PowerGlow);
                Cube(parent, "power plant cyan reactor identity bar right", new Vector3(width * 0.18f, 1.22f, depth * 0.02f), new Vector3(0.08f, 0.04f, depth * 0.38f), materials.PowerGlow);
                Cube(parent, "power plant electric blue lightning stripe", new Vector3(0f, 1.245f, -depth * 0.20f), new Vector3(width * 0.42f, 0.034f, 0.055f), materials.PowerGlow);
            }
            else if (spec.Id == "advanced_power_plant")
            {
                Cube(parent, "advanced power glowing T marker spine", new Vector3(0f, 1.34f, -depth * 0.04f), new Vector3(0.09f, 0.04f, depth * 0.58f), materials.PowerGlow);
                Cube(parent, "advanced power glowing T marker crossbar", new Vector3(0f, 1.345f, depth * 0.18f), new Vector3(width * 0.58f, 0.04f, 0.09f), materials.PowerGlow);
                Cube(parent, "advanced power electric blue service cap", new Vector3(0f, 1.37f, -depth * 0.36f), new Vector3(width * 0.48f, 0.032f, 0.055f), materials.PowerGlow);
            }
            else if (spec.Id == "barracks")
            {
                Cube(parent, "mauve barracks command chevron left", new Vector3(-width * 0.18f, 1.22f, depth * 0.18f), new Vector3(width * 0.28f, 0.035f, 0.06f), accent);
                Cube(parent, "mauve barracks command chevron right", new Vector3(width * 0.18f, 1.22f, depth * 0.18f), new Vector3(width * 0.28f, 0.035f, 0.06f), accent);
                Cube(parent, "barracks parade roof spine", new Vector3(0f, 1.225f, -depth * 0.14f), new Vector3(0.06f, 0.034f, depth * 0.38f), materials.EdgeMetal);
            }
            else if (spec.Id == "war_factory")
            {
                Cube(parent, "mauve war factory oversized bay stripe left", new Vector3(-width * 0.18f, 1.34f, depth * 0.20f), new Vector3(width * 0.30f, 0.035f, 0.06f), materials.ProductionAccent);
                Cube(parent, "mauve war factory oversized bay stripe right", new Vector3(width * 0.18f, 1.34f, depth * 0.20f), new Vector3(width * 0.30f, 0.035f, 0.06f), materials.ProductionAccent);
                Cube(parent, "war factory dark crane runway identity", new Vector3(0f, 1.37f, -depth * 0.20f), new Vector3(width * 0.68f, 0.034f, 0.075f), materials.Gunmetal);
            }
            else if (spec.Id == "refinery")
            {
                Cube(parent, "refinery ore path amber roof line", new Vector3(width * 0.18f, 1.25f, depth * 0.08f), new Vector3(width * 0.46f, 0.035f, 0.055f), materials.Hazard);
                Cube(parent, "refinery dark intake roof lane", new Vector3(width * 0.30f, 1.255f, depth * 0.28f), new Vector3(0.08f, 0.036f, depth * 0.34f), materials.DarkMetal);
            }
            else if (spec.Id == "comm_center")
            {
                Cube(parent, "comm center cyan signal dish marker", new Vector3(-width * 0.05f, 1.20f, depth * 0.12f), new Vector3(width * 0.52f, 0.035f, 0.055f), materials.PowerGlow);
                Cube(parent, "comm center signal mast roof spine", new Vector3(width * 0.22f, 1.205f, -depth * 0.18f), new Vector3(0.055f, 0.036f, depth * 0.44f), materials.EdgeMetal);
            }
            else if (spec.Id == "repair_bay")
            {
                Cube(parent, "repair bay rear shop roof caution handle", new Vector3(0f, 1.22f, -depth * 0.16f), new Vector3(width * 0.52f, 0.035f, 0.055f), materials.Hazard);
                Cube(parent, "repair bay rear shop roof wrench jaw", new Vector3(width * 0.20f, 1.225f, -depth * 0.30f), new Vector3(0.055f, 0.036f, depth * 0.28f), materials.Hazard);
            }
            else if (spec.Id == "tech_center")
            {
                Cube(parent, "tech center cyan research identity spine", new Vector3(0f, 1.36f, -depth * 0.04f), new Vector3(0.08f, 0.04f, depth * 0.54f), materials.PowerGlow);
                Cube(parent, "tech center cyan lab identity left", new Vector3(-width * 0.22f, 1.19f, -depth * 0.22f), new Vector3(width * 0.20f, 0.035f, 0.06f), materials.PowerGlow);
                Cube(parent, "tech center cyan lab identity right", new Vector3(width * 0.22f, 1.19f, -depth * 0.22f), new Vector3(width * 0.20f, 0.035f, 0.06f), materials.PowerGlow);
                Cube(parent, "tech center sealed science facade identity bar", new Vector3(0f, 0.98f, depth * 0.46f), new Vector3(width * 0.52f, 0.050f, 0.060f), materials.PowerGlow);
            }
            else if (spec.Id == "dual_helipad")
            {
                Cube(parent, "mauve dual helipad center tower identity band", new Vector3(0f, 1.16f, -depth * 0.32f), new Vector3(width * 0.20f, 0.04f, 0.06f), materials.ProductionAccent);
                Cube(parent, "mauve dual helipad center taxiway identity spine", new Vector3(0f, 0.45f, depth * 0.10f), new Vector3(0.07f, 0.035f, depth * 0.58f), materials.ProductionAccent);
            }
        }

        static void AddAirfieldCenterRoofPanel(Transform parent, float width, float depth, MaterialBundle materials)
        {
            Cube(parent, "center-only helipad tower roof panel", new Vector3(0f, 1.135f, -depth * 0.32f), new Vector3(width * 0.22f, 0.045f, depth * 0.22f), materials.Panel);
            Cube(parent, "center-only helipad tower cable tray", new Vector3(0f, 1.18f, -depth * 0.22f), new Vector3(0.07f, 0.035f, depth * 0.22f), materials.EdgeMetal);
        }

        static void AddAirfieldCenterStatusLights(Transform parent, BuildingSpec spec, BuildingVisualRig rig, Material lightMaterial, float width, float depth)
        {
            var positions = new[]
            {
                new Vector3(0f, 1.18f, -depth * 0.50f),
                new Vector3(0f, 1.18f, -depth * 0.16f),
                new Vector3(0f, 0.52f, depth * 0.34f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var light = Sphere(parent, "center-only airfield powered status light " + i, positions[i], 0.052f, lightMaterial);
                rig.poweredLights.Add(light.transform);
                AddSocket(light.transform, "LightRoot " + i, ActorPrefabSocketKind.LightRoot, spec.Id, Vector3.zero);
            }
        }

        static void AddRoofPanelGrid(Transform parent, float width, float depth, MaterialBundle materials)
        {
            var usableWidth = width * 0.78f;
            var usableDepth = depth * 0.54f;
            for (var i = -1; i <= 1; i++)
                Cube(parent, "roof recessed seam x " + i, new Vector3(i * usableWidth / 3f, 1.105f, -depth * 0.05f), new Vector3(0.026f, 0.024f, usableDepth), materials.DarkMetal);
            for (var i = -1; i <= 1; i++)
                Cube(parent, "roof recessed seam z " + i, new Vector3(0f, 1.108f, -depth * 0.05f + i * usableDepth / 3f), new Vector3(usableWidth, 0.024f, 0.026f), materials.DarkMetal);

            Cube(parent, "roof service hatch", new Vector3(-width * 0.24f, 1.145f, depth * 0.08f), new Vector3(width * 0.16f, 0.045f, depth * 0.12f), materials.Panel);
            Cube(parent, "roof cable tray", new Vector3(width * 0.24f, 1.15f, -depth * 0.18f), new Vector3(width * 0.24f, 0.045f, 0.07f), materials.EdgeMetal);
        }

        static void AddCrane(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials)
        {
            var crane = new GameObject("animated construction crane").transform;
            crane.SetParent(parent, false);
            crane.localPosition = new Vector3(0f, 1.44f, -0.15f);
            Cube(crane, "crane spine", new Vector3(0f, 0f, 0f), new Vector3(spec.Width * 0.58f, 0.08f, 0.10f), materials.EdgeMetal);
            Cube(crane, "crane counterweight", new Vector3(-spec.Width * 0.36f, -0.02f, 0f), new Vector3(0.28f, 0.18f, 0.18f), materials.DarkMetal);
            Cube(crane, "crane hook rail", new Vector3(spec.Width * 0.30f, -0.18f, 0f), new Vector3(0.08f, 0.35f, 0.08f), materials.Gunmetal);
            rig.craneArm = crane;
            AddSocket(crane, "CraneRoot", ActorPrefabSocketKind.CraneRoot, spec.Id, Vector3.zero);
        }

        static void AddTurbines(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials)
        {
            var towerOffset = spec.Id == "advanced_power_plant" ? 0.48f : 0.34f;
            Cylinder(parent, "left reactor stack", new Vector3(-towerOffset, 0.82f, -0.32f), new Vector3(0.22f, 0.58f, 0.22f), materials.DarkMetal);
            Cylinder(parent, "right reactor stack", new Vector3(towerOffset, 0.82f, -0.32f), new Vector3(0.22f, 0.58f, 0.22f), materials.DarkMetal);
            Cube(parent, "reactor glow slit", new Vector3(0f, 0.72f, 0.40f), new Vector3(0.78f, 0.14f, 0.06f), materials.PowerGlow);

            if (spec.Id == "advanced_power_plant")
            {
                var leftSpinner = AddTopBladeSpinner(parent, "advanced power left top spinning turbine blade assembly", new Vector3(-0.58f, 1.34f, 0.02f), 0.62f, materials);
                var rightSpinner = AddTopBladeSpinner(parent, "advanced power right top spinning turbine blade assembly", new Vector3(0.58f, 1.34f, 0.02f), 0.62f, materials);
                Cylinder(parent, "advanced power left spinner armored bearing", new Vector3(-0.58f, 1.24f, 0.02f), new Vector3(0.18f, 0.08f, 0.18f), materials.Gunmetal);
                Cylinder(parent, "advanced power right spinner armored bearing", new Vector3(0.58f, 1.24f, 0.02f), new Vector3(0.18f, 0.08f, 0.18f), materials.Gunmetal);
                rig.turbine = leftSpinner;
                rig.extraTurbines.Add(rightSpinner);
                AddSocket(leftSpinner, "TurbineRoot Left", ActorPrefabSocketKind.TurbineRoot, spec.Id, Vector3.zero);
                AddSocket(rightSpinner, "TurbineRoot Right", ActorPrefabSocketKind.TurbineRoot, spec.Id, Vector3.zero);
            }
            else
            {
                var turbine = AddTopBladeSpinner(parent, "power plant top spinning turbine blade assembly", new Vector3(0f, 1.32f, 0f), 0.72f, materials);
                Cylinder(parent, "power plant top spinner armored bearing", new Vector3(0f, 1.22f, 0f), new Vector3(0.20f, 0.08f, 0.20f), materials.Gunmetal);
                rig.turbine = turbine;
                AddSocket(turbine, "TurbineRoot", ActorPrefabSocketKind.TurbineRoot, spec.Id, Vector3.zero);
            }
        }

        static Transform AddTopBladeSpinner(Transform parent, string name, Vector3 localPosition, float bladeLength, MaterialBundle materials)
        {
            var spinner = new GameObject(name).transform;
            spinner.SetParent(parent, false);
            spinner.localPosition = localPosition;
            for (var i = 0; i < 4; i++)
            {
                var blade = Cube(spinner, name + " blade " + i, Vector3.zero, new Vector3(bladeLength, 0.045f, 0.085f), materials.EdgeMetal);
                blade.transform.localRotation = Quaternion.Euler(0f, i * 45f, 0f);
            }
            Sphere(spinner, name + " powered electric blue hub", Vector3.up * 0.025f, 0.12f, materials.PowerGlow);
            Cube(spinner, name + " cyan motion streak", new Vector3(0f, 0.035f, 0f), new Vector3(bladeLength * 0.88f, 0.014f, 0.018f), materials.PowerGlow);
            return spinner;
        }

        static void AddRadar(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials)
        {
            var radar = new GameObject(spec.Id == "comm_center" ? "animated comm center satellite dish" : "animated radar dish").transform;
            radar.SetParent(parent, false);
            if (spec.Id == "comm_center")
            {
                radar.localPosition = new Vector3(0.28f, 1.54f, -0.22f);
                Cylinder(radar, "satellite dish heavy azimuth bearing", Vector3.zero, new Vector3(0.20f, 0.06f, 0.20f), materials.Gunmetal);
                var mast = Cylinder(radar, "satellite dish angled support mast", new Vector3(0f, 0.22f, -0.05f), new Vector3(0.04f, 0.26f, 0.04f), materials.EdgeMetal);
                mast.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
                var dishBack = Cube(radar, "large comm center satellite dish back plate", new Vector3(0f, 0.42f, 0.20f), new Vector3(0.92f, 0.06f, 0.58f), materials.EdgeMetal);
                dishBack.transform.localRotation = Quaternion.Euler(-32f, 0f, 0f);
                var dishFace = Cube(radar, "large comm center satellite dish bright reflector face", new Vector3(0f, 0.47f, 0.25f), new Vector3(0.78f, 0.035f, 0.46f), materials.Panel);
                dishFace.transform.localRotation = Quaternion.Euler(-32f, 0f, 0f);
                var feedArm = Cube(radar, "satellite dish forward feed arm", new Vector3(0f, 0.42f, 0.62f), new Vector3(0.055f, 0.055f, 0.55f), materials.Gunmetal);
                feedArm.transform.localRotation = Quaternion.Euler(-16f, 0f, 0f);
                Sphere(radar, "satellite dish receiver node", new Vector3(0f, 0.30f, 0.86f), 0.10f, materials.PowerGlow);
                Cube(parent, "communications glass strip", new Vector3(0f, 0.88f, 0.58f), new Vector3(spec.Width * 0.34f, 0.16f, 0.06f), materials.PowerGlow);
            }
            else
            {
                radar.localPosition = new Vector3(0f, 1.46f, -0.10f);
                Cylinder(radar, "radar bearing", Vector3.zero, new Vector3(0.22f, 0.05f, 0.22f), materials.Gunmetal);
                var dish = Cube(radar, "radar angled panel", new Vector3(0f, 0.14f, 0.11f), new Vector3(0.70f, 0.08f, 0.42f), materials.EdgeMetal);
                dish.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
                Cube(parent, "communications glass strip", new Vector3(0f, 0.88f, 0.58f), new Vector3(spec.Width * 0.34f, 0.16f, 0.06f), materials.PowerGlow);
            }
            rig.radarDish = radar;
            AddSocket(radar, "RadarDishRoot", ActorPrefabSocketKind.RadarDishRoot, spec.Id, Vector3.zero);
        }

        static void AddRefinery(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials)
        {
            Cylinder(parent, "ore processing tank A", new Vector3(-0.72f, 0.92f, -0.62f), new Vector3(0.34f, 0.56f, 0.34f), materials.DarkMetal);
            Cylinder(parent, "ore processing tank B", new Vector3(-0.18f, 0.92f, -0.70f), new Vector3(0.30f, 0.48f, 0.30f), materials.DarkMetal);
            Cube(parent, "ore intake conveyor", new Vector3(0.72f, 0.42f, 0.62f), new Vector3(0.28f, 0.20f, 0.82f), materials.Asphalt);
            var pump = Cube(parent, "animated refinery dock pump", new Vector3(0.78f, 0.32f, 0.74f), new Vector3(0.22f, 0.32f, 0.22f), materials.Gunmetal);
            rig.dockPump = pump.transform;
            AddSocket(pump.transform, "DockPumpRoot", ActorPrefabSocketKind.DockPumpRoot, spec.Id, Vector3.zero);
            AddSocket(parent, "HarvesterDock", ActorPrefabSocketKind.HarvesterDock, spec.Id, new Vector3(0.82f, 0.12f, 1.36f));
        }

        static void AddRepairBay(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials)
        {
            Cube(parent, "repair bay flat vehicle pull-on repair pad", new Vector3(0f, 0.255f, 0.34f), new Vector3(1.60f, 0.07f, 0.92f), materials.DarkConcrete);
            Cube(parent, "repair pad front yellow entry stripe", new Vector3(0f, 0.31f, 0.78f), new Vector3(1.08f, 0.028f, 0.06f), materials.Hazard);
            Cube(parent, "repair pad rear stop stripe", new Vector3(0f, 0.31f, -0.02f), new Vector3(1.00f, 0.028f, 0.06f), materials.Hazard);
            var left = AddArticulatedRepairWorkerArm(parent, "left", -1f, materials);
            var right = AddArticulatedRepairWorkerArm(parent, "right", 1f, materials);
            rig.repairArmLeft = left;
            rig.repairArmRight = right;
            AddSocket(left, "RepairArmRoot", ActorPrefabSocketKind.RepairArmRoot, spec.Id, Vector3.zero);
            AddSocket(right, "RepairArmRoot Right", ActorPrefabSocketKind.RepairArmRoot, spec.Id, Vector3.zero);
        }

        static Transform AddArticulatedRepairWorkerArm(Transform parent, string sideName, float sideSign, MaterialBundle materials)
        {
            var root = new GameObject("animated articulated repair worker arm " + sideName).transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(sideSign * 0.58f, 0.70f, 0.16f);
            root.localRotation = Quaternion.Euler(0f, sideSign * 12f, 0f);

            Cylinder(root, sideName + " repair arm rotating pedestal cylinder", new Vector3(0f, -0.23f, -0.14f), new Vector3(0.12f, 0.13f, 0.12f), materials.Gunmetal);
            Sphere(root, sideName + " repair arm round shoulder joint", new Vector3(0f, -0.04f, -0.10f), 0.12f, materials.EdgeMetal);
            var upperArm = Cube(root, sideName + " repair arm upper worker boom", new Vector3(-sideSign * 0.12f, 0.04f, 0.20f), new Vector3(0.075f, 0.075f, 0.52f), materials.Gunmetal);
            upperArm.transform.localRotation = Quaternion.Euler(-8f, -sideSign * 13f, sideSign * 4f);

            Cube(root, sideName + " repair arm polished upper hydraulic piston", new Vector3(-sideSign * 0.045f, -0.02f, 0.22f), new Vector3(0.026f, 0.026f, 0.45f), materials.EdgeMetal).transform.localRotation = Quaternion.Euler(-11f, -sideSign * 11f, 0f);
            Sphere(root, sideName + " repair arm round elbow joint", new Vector3(-sideSign * 0.22f, -0.02f, 0.46f), 0.095f, materials.Medical);

            var forearm = Cube(root, sideName + " repair arm angled forearm link", new Vector3(-sideSign * 0.19f, -0.12f, 0.64f), new Vector3(0.065f, 0.065f, 0.38f), materials.Gunmetal);
            forearm.transform.localRotation = Quaternion.Euler(12f, sideSign * 9f, -sideSign * 5f);
            Cube(root, sideName + " repair arm lower hydraulic piston", new Vector3(-sideSign * 0.25f, -0.12f, 0.62f), new Vector3(0.022f, 0.022f, 0.34f), materials.EdgeMetal).transform.localRotation = Quaternion.Euler(18f, sideSign * 7f, 0f);

            Sphere(root, sideName + " repair arm compact wrist joint", new Vector3(-sideSign * 0.19f, -0.22f, 0.82f), 0.075f, materials.EdgeMetal);
            Cylinder(root, "green " + sideName + " repair worker arm precision welding nozzle", new Vector3(-sideSign * 0.19f, -0.35f, 0.82f), new Vector3(0.033f, 0.12f, 0.033f), materials.Medical);
            Sphere(root, "green " + sideName + " repair worker arm emitter lens", new Vector3(-sideSign * 0.19f, -0.44f, 0.82f), 0.052f, materials.Medical);
            var fingerA = Cube(root, sideName + " repair arm slim left tool finger", new Vector3(-sideSign * 0.245f, -0.39f, 0.84f), new Vector3(0.022f, 0.11f, 0.022f), materials.EdgeMetal);
            fingerA.transform.localRotation = Quaternion.Euler(0f, 0f, sideSign * 18f);
            var fingerB = Cube(root, sideName + " repair arm slim right tool finger", new Vector3(-sideSign * 0.135f, -0.39f, 0.84f), new Vector3(0.022f, 0.11f, 0.022f), materials.EdgeMetal);
            fingerB.transform.localRotation = Quaternion.Euler(0f, 0f, -sideSign * 18f);
            Cube(root, sideName + " repair arm green diagnostic cable", new Vector3(-sideSign * 0.12f, -0.06f, 0.42f), new Vector3(0.020f, 0.020f, 0.42f), materials.Medical).transform.localRotation = Quaternion.Euler(-16f, -sideSign * 16f, 0f);

            return root;
        }

        static void AddMedicalDetails(Transform parent, BuildingSpec spec, MaterialBundle materials)
        {
            Cube(parent, "medical light bar", new Vector3(0f, 1.22f, 0.36f), new Vector3(0.64f, 0.08f, 0.08f), materials.Medical);
            Cube(parent, "triage awning", new Vector3(0f, 0.80f, 0.68f), new Vector3(1.08f, 0.08f, 0.36f), materials.Concrete);
        }

        static void AddHelipads(Transform parent, BuildingSpec spec, BuildingVisualRig rig, MaterialBundle materials)
        {
            for (var i = 0; i < 2; i++)
            {
                var x = i == 0 ? -0.62f : 0.62f;
                Cylinder(parent, "flush clear helipad landing paint disk " + i, new Vector3(x, 0.165f, 0.10f), new Vector3(0.46f, 0.004f, 0.46f), materials.Asphalt);
                Cube(parent, "flush painted helipad H stripe " + i, new Vector3(x, 0.174f, 0.10f), new Vector3(0.60f, 0.006f, 0.035f), materials.Hazard);
                Cube(parent, "flush painted helipad H stem " + i, new Vector3(x, 0.176f, 0.10f), new Vector3(0.035f, 0.006f, 0.56f), materials.Hazard);
                AddSocket(parent, "LandingPadAnchor " + i, ActorPrefabSocketKind.LandingPadAnchor, spec.Id, new Vector3(x, 0.22f, 0.10f));
            }

            var beacon = Cylinder(parent, "rotating airfield beacon", new Vector3(0f, 1.10f, -0.48f), new Vector3(0.18f, 0.08f, 0.18f), materials.PowerGlow);
            rig.productionIndicator = beacon.transform;
        }

        static void AddAirfieldCenterMotion(Transform parent, BuildingVisualRig rig, MaterialBundle materials, float width, float depth)
        {
            var machinery = Cylinder(parent, "center-only airfield machinery rotor", new Vector3(0f, 1.28f, -depth * 0.32f), new Vector3(0.16f, 0.045f, 0.16f), materials.Gunmetal);
            rig.machinery = machinery.transform;
            rig.warningLight = Sphere(parent, "center-only airfield warning beacon", new Vector3(0f, 1.38f, -depth * 0.42f), 0.07f, materials.Warning).transform;
            rig.damageSmoke = Sphere(parent, "center-only airfield damage smoke volume", new Vector3(0f, 1.48f, -depth * 0.26f), 0.10f, materials.Smoke).transform;
        }

        static void AddAdvancedMissileBattery(Transform turret, string sideName, float sideSign, MaterialBundle materials)
        {
            var battery = new GameObject("advanced gun tower realistic " + sideName + " missile battery").transform;
            battery.SetParent(turret, false);
            battery.localPosition = new Vector3(sideSign * 0.48f, 0.04f, 0.42f);

            Cube(battery, sideName + " missile battery armored side pod housing", new Vector3(0f, 0f, 0f), new Vector3(0.24f, 0.26f, 0.62f), materials.TurretArmor);
            Cube(battery, sideName + " missile battery textured turret top armor hatch", new Vector3(0f, 0.155f, 0f), new Vector3(0.22f, 0.040f, 0.54f), materials.TurretTop);
            Cube(battery, sideName + " missile battery dark recessed launcher face", new Vector3(0f, 0f, 0.34f), new Vector3(0.21f, 0.22f, 0.05f), materials.DarkMetal);
            Cube(battery, sideName + " missile battery rear hinge bracket", new Vector3(0f, 0f, -0.34f), new Vector3(0.18f, 0.20f, 0.12f), materials.EdgeMetal);
            Cube(battery, sideName + " missile battery upper armor brow", new Vector3(0f, 0.16f, 0.05f), new Vector3(0.28f, 0.055f, 0.58f), materials.TurretBarrel);
            Cube(battery, sideName + " missile battery lower recoil rail", new Vector3(0f, -0.16f, 0.06f), new Vector3(0.26f, 0.045f, 0.54f), materials.Gunmetal);

            var tubeRows = new[] { -0.055f, 0.055f };
            var tubeColumns = new[] { -0.045f, 0.045f };
            foreach (var y in tubeRows)
            {
                foreach (var x in tubeColumns)
                {
                    var tube = Cylinder(battery, sideName + " missile launch tube", new Vector3(x, y, 0.24f), new Vector3(0.035f, 0.26f, 0.035f), materials.TurretBarrel);
                    tube.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    var rim = Cylinder(battery, sideName + " missile tube bright rim", new Vector3(x, y, 0.51f), new Vector3(0.040f, 0.018f, 0.040f), materials.EdgeMetal);
                    rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    var nose = Cylinder(battery, sideName + " high visibility missile nose cap", new Vector3(x, y, 0.56f), new Vector3(0.032f, 0.028f, 0.032f), materials.TurretMuzzle);
                    nose.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }

            Cube(battery, sideName + " missile battery side contrast stripe", new Vector3(-sideSign * 0.13f, 0.005f, 0.08f), new Vector3(0.025f, 0.18f, 0.46f), materials.DefenseAccent);
        }

        static void AddTurretTopTextureDetails(Transform turret, string label, float width, float depth, float topY, MaterialBundle materials)
        {
            Cube(turret, label + " textured turret top armor plate", new Vector3(0f, topY, 0f), new Vector3(width, 0.045f, depth), materials.TurretTop);
            Cube(turret, label + " turret top tactical panel seam forward", new Vector3(0f, topY + 0.028f, depth * 0.18f), new Vector3(width * 0.72f, 0.016f, 0.030f), materials.EdgeMetal);
            Cube(turret, label + " turret top tactical panel seam cross", new Vector3(width * 0.18f, topY + 0.030f, 0f), new Vector3(0.032f, 0.016f, depth * 0.58f), materials.EdgeMetal);
            Cube(turret, label + " raised turret top command hatch", new Vector3(-width * 0.24f, topY + 0.060f, -depth * 0.18f), new Vector3(width * 0.24f, 0.036f, depth * 0.24f), materials.TurretTop);
        }

        static void AddPanelStrip(Transform parent, string name, Vector3 origin, bool alongX, int count, Material material)
        {
            for (var i = 0; i < count; i++)
            {
                var local = origin + (alongX ? new Vector3((i - count * 0.5f) * 0.22f, 0f, 0f) : new Vector3(0f, 0f, (i - count * 0.5f) * 0.22f));
                Cube(parent, name + " " + i, local, alongX ? new Vector3(0.16f, 0.20f, 0.035f) : new Vector3(0.035f, 0.20f, 0.16f), material);
            }
        }

        static void AddRoofVents(Transform parent, BuildingSpec spec, MaterialBundle materials)
        {
            var count = Mathf.Clamp(spec.Width + spec.Height - 2, 2, 5);
            for (var i = 0; i < count; i++)
            {
                var x = (i - (count - 1) * 0.5f) * 0.32f;
                Cube(parent, "roof vent " + i, new Vector3(x, 1.13f, -0.22f), new Vector3(0.20f, 0.10f, 0.34f), materials.Gunmetal);
                Cube(parent, "vent highlight " + i, new Vector3(x, 1.19f, -0.22f), new Vector3(0.16f, 0.025f, 0.28f), materials.EdgeMetal);
            }
        }

        static void AddPerimeterLights(Transform parent, BuildingSpec spec, BuildingVisualRig rig, Material lightMaterial, float y)
        {
            var width = spec.Width * 0.54f;
            var depth = spec.Height * 0.54f;
            var count = Mathf.Clamp(spec.Width + spec.Height, 2, 6);
            for (var i = 0; i < count; i++)
            {
                var t = i / (float)Mathf.Max(1, count - 1);
                var x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);
                var z = i % 2 == 0 ? depth * 0.5f : -depth * 0.5f;
                var light = Sphere(parent, "powered status light " + i, new Vector3(x, y, z), 0.055f, lightMaterial);
                rig.poweredLights.Add(light.transform);
                AddSocket(light.transform, "LightRoot " + i, ActorPrefabSocketKind.LightRoot, spec.Id, Vector3.zero);
            }
        }

        static Transform AddSocket(Transform parent, string name, ActorPrefabSocketKind kind, string actorTypeId, Vector3 localPosition)
        {
            var socket = new GameObject(name).transform;
            socket.SetParent(parent, false);
            socket.localPosition = localPosition;
            var marker = socket.gameObject.AddComponent<ActorPrefabSocket>();
            marker.socketKind = kind;
            marker.socketName = name;
            marker.actorTypeId = actorTypeId;
            marker.notes = "Generated building slate socket.";
            return socket;
        }

        static GameObject Cube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            SetMaterialAndRemoveCollider(go, material);
            return go;
        }

        static GameObject Cylinder(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            SetMaterialAndRemoveCollider(go, material);
            return go;
        }

        static GameObject Sphere(Transform parent, string name, Vector3 localPosition, float radius, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = Vector3.one * radius;
            SetMaterialAndRemoveCollider(go, material);
            return go;
        }

        static void SetMaterialAndRemoveCollider(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }

        static void UpdateVisualDefinition(BuildingSpec spec, BuildingVisualProfile profile, GameObject prefab)
        {
            var path = DefinitionFolder + "/" + spec.Id + "_visual.asset";
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ActorVisualDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.actorTypeId = spec.Id;
            definition.displayName = spec.DisplayName;
            definition.safeDisplayName = spec.DisplayName;
            definition.category = spec.ArtCategory;
            definition.productionStatus = ActorArtProductionStatus.Animated;
            definition.productionPrefab = prefab;
            definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
            definition.motionProfileId = "building_static";
            definition.buildingProfileId = profile.profileId;
            definition.footprintWidth = spec.Width;
            definition.footprintHeight = spec.Height;
            definition.visualScale = 1f;
            definition.useBuildingVisualController = true;
            definition.selectionRadius = Mathf.Max(spec.Width, spec.Height) * 0.56f;
            definition.selectionHeight = 0.08f;
            definition.notes = "Unity AI building slate production prefab. Textured, socketed, and rigged for powered idle and production movement.";
            definition.requiredSockets.Clear();
            definition.requiredSockets.Add(ActorPrefabSocketKind.VisualRoot);
            definition.requiredSockets.Add(ActorPrefabSocketKind.BodyRoot);
            definition.requiredSockets.Add(ActorPrefabSocketKind.SelectionAnchor);
            definition.requiredSockets.Add(ActorPrefabSocketKind.HealthBarAnchor);
            definition.requiredSockets.Add(ActorPrefabSocketKind.UiAnchor);
            if (spec.HasDoor)
                definition.requiredSockets.Add(ActorPrefabSocketKind.DoorRoot);
            if (spec.HasProduction)
                definition.requiredSockets.Add(ActorPrefabSocketKind.ProductionExit);
            if (spec.HasDock)
                definition.requiredSockets.Add(ActorPrefabSocketKind.HarvesterDock);
            if (spec.HasRepairArms)
                definition.requiredSockets.Add(ActorPrefabSocketKind.RepairArmRoot);
            if (spec.HasCrane)
                definition.requiredSockets.Add(ActorPrefabSocketKind.CraneRoot);
            if (spec.HasRadar)
                definition.requiredSockets.Add(ActorPrefabSocketKind.RadarDishRoot);
            if (spec.HasTurbine)
                definition.requiredSockets.Add(ActorPrefabSocketKind.TurbineRoot);
            if (spec.HasTurret)
                definition.requiredSockets.Add(ActorPrefabSocketKind.TurretRoot);
            EditorUtility.SetDirty(definition);
        }

        static void CreateReviewScene(Dictionary<string, GameObject> prefabs, Dictionary<string, BuildingVisualProfile> profiles, MaterialBundle materials)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Unity AI Building Slate Review");
            var ground = Cube(root.transform, "textured terrain presentation ground", new Vector3(0f, -0.04f, 0f), new Vector3(24f, 0.08f, 18f), materials.ReviewGround);
            ground.isStatic = true;

            for (var i = 0; i < BuildingSpecs.Length; i++)
            {
                var spec = BuildingSpecs[i];
                GameObject prefab;
                BuildingVisualProfile profile;
                if (!prefabs.TryGetValue(spec.Id, out prefab) || !profiles.TryGetValue(spec.Id, out profile))
                    continue;

                var col = i % 5;
                var row = i / 5;
                var pos = new Vector3(-8.6f + col * 4.3f, 0f, 4.8f - row * 4.85f);
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                    continue;
                instance.name = spec.DisplayName + " showcase";
                instance.transform.SetParent(root.transform, false);
                instance.transform.position = pos;
                instance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                var controller = instance.AddComponent<BuildingArtShowcaseController>();
                controller.actorTypeId = spec.Id;
                controller.profile = profile;
                controller.cycleProductionState = spec.HasDoor || spec.HasProduction;
                controller.productionCycleSeconds = spec.Id == "war_factory" ? 6.5f : 4.8f;
                CreateLabel(root.transform, spec.DisplayName, pos + new Vector3(0f, 0.04f, -1.95f));
            }

            CreateLighting();
            CreateCamera();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static void CreateLabel(Transform parent, string text, Vector3 position)
        {
            var label = new GameObject("label " + text);
            label.transform.SetParent(parent, false);
            label.transform.position = position;
            label.transform.rotation = Quaternion.Euler(68f, 0f, 0f);
            var mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 38;
            mesh.characterSize = 0.055f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.76f, 0.82f, 0.78f, 1f);
        }

        static void CreateLighting()
        {
            var key = new GameObject("warm low sun key");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.25f;
            keyLight.color = new Color(1f, 0.92f, 0.80f, 1f);
            key.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var fill = new GameObject("cool hangar fill");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.38f;
            fillLight.color = new Color(0.58f, 0.70f, 0.90f, 1f);
            fill.transform.rotation = Quaternion.Euler(65f, 145f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.36f, 1f);
        }

        static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9.4f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.057f, 0.054f, 1f);
            cameraObject.transform.position = new Vector3(0f, 16.2f, -15.2f);
            cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        static string CaptureScreenshot()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
                throw new InvalidOperationException("Could not open building slate review scene.");

            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Building slate review scene has no camera.");

            var outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../..", ScreenshotRelativePath)).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            RenderCameraToPng(camera, outputPath, 1920, 1280);
            return outputPath;
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

        static MaterialBundle CreateMaterials()
        {
            return new MaterialBundle
            {
                Foundation = CreateMaterial("foundation_concrete", new Color(0.36f, 0.38f, 0.35f, 1f), new Color(0.65f, 0.62f, 0.52f, 1f), SurfacePattern.Concrete, 0.25f, 0f),
                Concrete = CreateMaterial("weathered_concrete", new Color(0.44f, 0.46f, 0.42f, 1f), new Color(0.68f, 0.66f, 0.58f, 1f), SurfacePattern.Concrete, 0.22f, 0f),
                DarkConcrete = CreateMaterial("dark_worn_concrete", new Color(0.17f, 0.18f, 0.16f, 1f), new Color(0.34f, 0.33f, 0.29f, 1f), SurfacePattern.Concrete, 0.18f, 0f),
                WornMetal = CreateMaterial("worn_green_gray_metal", new Color(0.28f, 0.34f, 0.31f, 1f), new Color(0.50f, 0.56f, 0.49f, 1f), SurfacePattern.MetalPanel, 0.38f, 0.15f),
                DarkMetal = CreateMaterial("dark_oiled_metal", new Color(0.10f, 0.12f, 0.12f, 1f), new Color(0.30f, 0.32f, 0.30f, 1f), SurfacePattern.MetalPanel, 0.42f, 0.20f),
                EdgeMetal = CreateMaterial("scraped_edge_metal", new Color(0.32f, 0.34f, 0.32f, 1f), new Color(0.66f, 0.62f, 0.52f, 1f), SurfacePattern.MetalPanel, 0.32f, 0.18f),
                Gunmetal = CreateMaterial("gunmetal_mechanics", new Color(0.13f, 0.15f, 0.15f, 1f), new Color(0.44f, 0.43f, 0.38f, 1f), SurfacePattern.MetalPanel, 0.46f, 0.28f),
                Door = CreateMaterial("ribbed_bay_door", new Color(0.21f, 0.23f, 0.22f, 1f), new Color(0.56f, 0.54f, 0.46f, 1f), SurfacePattern.RibbedDoor, 0.34f, 0.12f),
                Panel = CreateMaterial("small_service_panels", new Color(0.22f, 0.25f, 0.24f, 1f), new Color(0.58f, 0.62f, 0.56f, 1f), SurfacePattern.PanelDots, 0.28f, 0.10f),
                InteriorFloor = CreateMaterial("bright_blue_interior_floor_tiles", new Color(0.08f, 0.20f, 0.34f, 1f), new Color(0.20f, 0.78f, 0.95f, 1f), SurfacePattern.PanelDots, 0.62f, 0.08f),
                Hazard = CreateMaterial("muted_caution_paint", new Color(0.57f, 0.47f, 0.23f, 1f), new Color(0.95f, 0.80f, 0.36f, 1f), SurfacePattern.Hazard, 0.22f, 0f),
                Asphalt = CreateMaterial("worn_asphalt_ramp", new Color(0.16f, 0.16f, 0.14f, 1f), new Color(0.42f, 0.40f, 0.34f, 1f), SurfacePattern.Asphalt, 0.14f, 0f),
                PowerGlow = CreateMaterial("powered_cyan_glow", new Color(0.10f, 0.62f, 0.70f, 1f), new Color(0.48f, 0.92f, 0.86f, 1f), SurfacePattern.Glow, 0.45f, 0f, true),
                Warning = CreateMaterial("warning_amber_glow", new Color(0.95f, 0.48f, 0.10f, 1f), new Color(1f, 0.86f, 0.32f, 1f), SurfacePattern.Glow, 0.45f, 0f, true),
                Medical = CreateMaterial("medical_green_light", new Color(0.28f, 0.88f, 0.62f, 1f), new Color(0.78f, 1f, 0.82f, 1f), SurfacePattern.Glow, 0.34f, 0f, true),
                Smoke = CreateMaterial("soft_damage_smoke", new Color(0.10f, 0.10f, 0.09f, 0.74f), new Color(0.34f, 0.32f, 0.28f, 0.74f), SurfacePattern.Smoke, 0.08f, 0f),
                FactionAccent = CreateMaterial("classic_red_command_accent", new Color(0.48f, 0.08f, 0.045f, 1f), new Color(0.90f, 0.20f, 0.12f, 1f), SurfacePattern.MetalPanel, 0.30f, 0.06f),
                ProductionAccent = CreateMaterial("production_mauve_accent", new Color(0.48f, 0.25f, 0.50f, 1f), new Color(0.90f, 0.58f, 0.86f, 1f), SurfacePattern.Glow, 0.36f, 0f, true),
                DefenseAccent = CreateMaterial("defense_red_brown_accent", new Color(0.44f, 0.18f, 0.12f, 1f), new Color(0.78f, 0.34f, 0.22f, 1f), SurfacePattern.MetalPanel, 0.32f, 0.08f),
                TurretArmor = CreateMaterial("turret_high_contrast_armor", new Color(0.36f, 0.08f, 0.075f, 1f), new Color(0.88f, 0.20f, 0.15f, 1f), SurfacePattern.MetalPanel, 0.36f, 0.18f),
                TurretTop = CreateMaterial("turret_top_tactical_panel_texture", new Color(0.18f, 0.19f, 0.17f, 1f), new Color(0.78f, 0.23f, 0.18f, 1f), SurfacePattern.TurretTop, 0.40f, 0.22f),
                TurretBarrel = CreateMaterial("turret_bright_gunmetal", new Color(0.40f, 0.43f, 0.40f, 1f), new Color(0.88f, 0.82f, 0.68f, 1f), SurfacePattern.MetalPanel, 0.42f, 0.35f),
                TurretMuzzle = CreateMaterial("turret_high_visibility_muzzle", new Color(0.95f, 0.72f, 0.22f, 1f), new Color(1f, 0.94f, 0.58f, 1f), SurfacePattern.MetalPanel, 0.50f, 0.22f, true),
                ReviewGround = CreateMaterial("review_heather_ground", new Color(0.18f, 0.27f, 0.19f, 1f), new Color(0.49f, 0.42f, 0.30f, 1f), SurfacePattern.Ground, 0.18f, 0f)
            };
        }

        static Material CreateMaterial(string id, Color color, Color accent, SurfacePattern pattern, float smoothness, float metallic, bool emission = false)
        {
            var texturePath = TextureFolder + "/" + id + ".png";
            var promotedAlbedoPath = FindPromotedTexturePath(id, "albedo");
            var baseTexturePath = string.IsNullOrEmpty(promotedAlbedoPath) ? texturePath : promotedAlbedoPath;
            if (string.IsNullOrEmpty(promotedAlbedoPath))
                CreateTexture(texturePath, id, color, accent, pattern);
            else
                ConfigureTextureImporter(promotedAlbedoPath, false);

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            var materialPath = MaterialFolder + "/" + id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.shader = shader;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(baseTexturePath);
            var tint = string.IsNullOrEmpty(promotedAlbedoPath) ? color : Color.Lerp(Color.white, color, 0.32f);
            SetColor(material, "_BaseColor", tint);
            SetColor(material, "_Color", tint);
            SetTexture(material, "_BaseMap", texture);
            SetTexture(material, "_MainTex", texture);

            var normalPath = FindPromotedTexturePath(id, "normal");
            if (string.IsNullOrEmpty(normalPath) && !string.IsNullOrEmpty(promotedAlbedoPath))
                normalPath = CreateDerivedNormalTexture(id, promotedAlbedoPath);
            if (!string.IsNullOrEmpty(normalPath))
            {
                ConfigureTextureImporter(normalPath, true);
                var normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                SetTexture(material, "_BumpMap", normalTexture);
                SetFloat(material, "_BumpScale", 0.62f);
                material.EnableKeyword("_NORMALMAP");
            }

            var occlusionPath = FindPromotedTexturePath(id, "occlusion");
            if (string.IsNullOrEmpty(occlusionPath) && !string.IsNullOrEmpty(promotedAlbedoPath))
                occlusionPath = CreateDerivedOcclusionTexture(id, promotedAlbedoPath);
            if (!string.IsNullOrEmpty(occlusionPath))
            {
                ConfigureTextureImporter(occlusionPath, false);
                var occlusionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(occlusionPath);
                SetTexture(material, "_OcclusionMap", occlusionTexture);
                SetFloat(material, "_OcclusionStrength", 0.75f);
            }

            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_Metallic", metallic);
            if (emission)
            {
                SetColor(material, "_EmissionColor", accent * 1.4f);
                material.EnableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        static string FindPromotedTexturePath(string id, string map)
        {
            if (!AssetDatabase.IsValidFolder(SourceTextureFolder))
                return null;

            var names = new List<string>
            {
                id + "_ai_" + map,
                id + "_" + map
            };

            if (map == "albedo")
                names.Add(id);

            foreach (var name in names)
            {
                var guids = AssetDatabase.FindAssets(name + " t:Texture2D", new[] { SourceTextureFolder });
                for (var i = 0; i < guids.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    if (string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase))
                        return path;
                }
            }

            return null;
        }

        static string CreateDerivedNormalTexture(string id, string albedoAssetPath)
        {
            var source = LoadTextureFromAssetPath(albedoAssetPath);
            if (source == null)
                return null;

            var outputAssetPath = TextureFolder + "/" + id + "_derived_normal.png";
            var outputFilePath = AssetPathToFilePath(outputAssetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            var normal = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            const float strength = 2.2f;
            for (var y = 0; y < source.height; y++)
            {
                for (var x = 0; x < source.width; x++)
                {
                    var left = Luminance(source.GetPixel((x - 1 + source.width) % source.width, y));
                    var right = Luminance(source.GetPixel((x + 1) % source.width, y));
                    var down = Luminance(source.GetPixel(x, (y - 1 + source.height) % source.height));
                    var up = Luminance(source.GetPixel(x, (y + 1) % source.height));
                    var dx = (right - left) * strength;
                    var dy = (up - down) * strength;
                    var n = new Vector3(-dx, -dy, 1f).normalized;
                    normal.SetPixel(x, y, new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f));
                }
            }

            normal.Apply();
            File.WriteAllBytes(outputFilePath, normal.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(normal);
            UnityEngine.Object.DestroyImmediate(source);
            AssetDatabase.ImportAsset(outputAssetPath, ImportAssetOptions.ForceUpdate);
            return outputAssetPath;
        }

        static string CreateDerivedOcclusionTexture(string id, string albedoAssetPath)
        {
            var source = LoadTextureFromAssetPath(albedoAssetPath);
            if (source == null)
                return null;

            var outputAssetPath = TextureFolder + "/" + id + "_derived_occlusion.png";
            var outputFilePath = AssetPathToFilePath(outputAssetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            var occlusion = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            for (var y = 0; y < source.height; y++)
            {
                for (var x = 0; x < source.width; x++)
                {
                    var c = source.GetPixel(x, y);
                    var lum = Luminance(c);
                    var contrast = Mathf.Abs(lum - 0.5f);
                    var value = Mathf.Clamp01(0.52f + lum * 0.44f - contrast * 0.22f);
                    occlusion.SetPixel(x, y, new Color(value, value, value, 1f));
                }
            }

            occlusion.Apply();
            File.WriteAllBytes(outputFilePath, occlusion.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(occlusion);
            UnityEngine.Object.DestroyImmediate(source);
            AssetDatabase.ImportAsset(outputAssetPath, ImportAssetOptions.ForceUpdate);
            return outputAssetPath;
        }

        static Texture2D LoadTextureFromAssetPath(string assetPath)
        {
            var filePath = AssetPathToFilePath(assetPath);
            if (!File.Exists(filePath))
                return null;

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(File.ReadAllBytes(filePath)))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                return null;
            }

            return texture;
        }

        static string AssetPathToFilePath(string assetPath)
        {
            return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('\\', '/');
        }

        static float Luminance(Color color)
        {
            return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        }

        static void ConfigureTextureImporter(string assetPath, bool normalMap)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normalMap;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.SaveAndReimport();
        }

        static void CreateTexture(string assetPath, string seed, Color color, Color accent, SurfacePattern pattern)
        {
            var filePath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            var hash = StableHash(seed);
            for (var y = 0; y < TextureSize; y++)
            {
                var v = y / (TextureSize - 1f);
                for (var x = 0; x < TextureSize; x++)
                {
                    var u = x / (TextureSize - 1f);
                    var n1 = Mathf.PerlinNoise(u * 12.5f + hash * 0.017f, v * 12.5f - hash * 0.011f);
                    var n2 = Mathf.PerlinNoise(u * 42.0f - hash * 0.007f, v * 42.0f + hash * 0.013f);
                    var n3 = Mathf.PerlinNoise(u * 96.0f + hash * 0.003f, v * 96.0f - hash * 0.005f);
                    var c = Color.Lerp(color * 0.68f, accent, n1 * 0.35f + n2 * 0.16f);
                    c *= 0.88f + n3 * 0.20f;
                    ApplyPattern(ref c, u, v, pattern, color, accent, hash);
                    c.a = color.a;
                    texture.SetPixel(x, y, c);
                }
            }

            texture.Apply();
            File.WriteAllBytes(filePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }

        static void ApplyPattern(ref Color c, float u, float v, SurfacePattern pattern, Color baseColor, Color accent, int hash)
        {
            if (pattern == SurfacePattern.MetalPanel)
            {
                var panelX = Mathf.Repeat(u * 5f, 1f);
                var panelY = Mathf.Repeat(v * 5f, 1f);
                var seam = Mathf.Min(Mathf.Abs(panelX - 0.5f), Mathf.Abs(panelY - 0.5f));
                if (seam < 0.035f)
                    c = Color.Lerp(c * 0.68f, accent, 0.28f);
                if (panelX < 0.055f || panelY < 0.055f)
                    c *= 0.74f;
                var scratch = Mathf.Repeat(u * 31f + v * 7f + hash * 0.001f, 1f);
                if (scratch < 0.020f)
                    c = Color.Lerp(c, Color.white, 0.18f);
                var rust = Mathf.PerlinNoise(u * 58f + hash * 0.021f, v * 29f - hash * 0.017f);
                if (rust > 0.76f)
                    c = Color.Lerp(c, new Color(0.38f, 0.20f, 0.11f, c.a), 0.24f);
                var bolt = Vector2.Distance(new Vector2(panelX, panelY), new Vector2(0.12f, 0.12f));
                if (bolt < 0.035f)
                    c = Color.Lerp(c, Color.white, 0.22f);
            }
            else if (pattern == SurfacePattern.TurretTop)
            {
                var radial = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f));
                var plateX = Mathf.Repeat(u * 6f, 1f);
                var plateY = Mathf.Repeat(v * 4f, 1f);
                if (plateX < 0.045f || plateY < 0.055f)
                    c *= 0.52f;
                if (Mathf.Abs(radial - 0.32f) < 0.025f)
                    c = Color.Lerp(c, accent, 0.44f);
                if (Mathf.Abs(u - 0.5f) < 0.018f || Mathf.Abs(v - 0.5f) < 0.018f)
                    c = Color.Lerp(c, accent, 0.30f);
                var hatch = Mathf.Max(Mathf.Abs(u - 0.28f), Mathf.Abs(v - 0.34f));
                if (hatch < 0.105f)
                    c = Color.Lerp(c, new Color(0.11f, 0.12f, 0.11f, c.a), 0.56f);
                var boltA = Vector2.Distance(new Vector2(plateX, plateY), new Vector2(0.16f, 0.18f));
                if (boltA < 0.045f)
                    c = Color.Lerp(c, Color.white, 0.26f);
                var scrape = Mathf.PerlinNoise(u * 78f + hash * 0.009f, v * 38f - hash * 0.006f);
                if (scrape > 0.80f)
                    c = Color.Lerp(c, new Color(0.82f, 0.72f, 0.58f, c.a), 0.24f);
            }
            else if (pattern == SurfacePattern.RibbedDoor)
            {
                var rib = Mathf.Repeat(u * 11f, 1f);
                if (rib < 0.11f)
                    c = Color.Lerp(c, accent, 0.42f);
                if (rib > 0.84f)
                    c *= 0.66f;
                if (Mathf.Repeat(v * 6f, 1f) < 0.065f)
                    c *= 0.68f;
                var grime = Mathf.PerlinNoise(u * 18f + hash * 0.01f, v * 64f);
                if (grime > 0.72f)
                    c *= 0.76f;
            }
            else if (pattern == SurfacePattern.Hazard)
            {
                var stripe = Mathf.Repeat((u + v) * 8f, 1f);
                c = stripe < 0.45f ? accent : baseColor * 0.62f;
                var scuff = Mathf.PerlinNoise(u * 80f + hash * 0.02f, v * 20f);
                if (scuff > 0.72f)
                    c *= 0.62f;
            }
            else if (pattern == SurfacePattern.Concrete)
            {
                if (Mathf.Repeat(u * 4f, 1f) < 0.030f || Mathf.Repeat(v * 4f, 1f) < 0.030f)
                    c *= 0.60f;
                var crack = Mathf.Abs(Mathf.PerlinNoise(u * 16f + hash * 0.004f, v * 16f) - 0.5f);
                if (crack < 0.025f && Mathf.PerlinNoise(u * 72f, v * 18f + hash * 0.002f) > 0.54f)
                    c *= 0.46f;
                var stain = Mathf.PerlinNoise(u * 9f - hash * 0.01f, v * 7f + hash * 0.008f);
                if (stain > 0.78f)
                    c = Color.Lerp(c, new Color(0.12f, 0.13f, 0.11f, c.a), 0.22f);
            }
            else if (pattern == SurfacePattern.Asphalt)
            {
                if (Mathf.Repeat(u * 2.4f + v * 0.3f, 1f) < 0.028f)
                    c = Color.Lerp(c, accent, 0.40f);
                if (Mathf.Repeat(v * 13f + hash * 0.002f, 1f) < 0.045f)
                    c *= 0.76f;
                var aggregate = Mathf.PerlinNoise(u * 120f, v * 120f);
                if (aggregate > 0.70f)
                    c = Color.Lerp(c, Color.white, 0.12f);
            }
            else if (pattern == SurfacePattern.Ground)
            {
                var mottled = Mathf.PerlinNoise(u * 32f, v * 32f);
                if (mottled > 0.67f)
                    c = Color.Lerp(c, accent, 0.28f);
                var grassBlade = Mathf.Repeat(u * 45f + v * 11f, 1f);
                if (grassBlade < 0.035f)
                    c = Color.Lerp(c, new Color(0.08f, 0.20f, 0.09f, c.a), 0.32f);
            }
            else if (pattern == SurfacePattern.Glow)
            {
                var pulse = 1f - Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 1.35f;
                c = Color.Lerp(c, accent, Mathf.Clamp01(pulse));
            }
            else if (pattern == SurfacePattern.PanelDots)
            {
                var dot = Vector2.Distance(new Vector2(Mathf.Repeat(u * 8f, 1f), Mathf.Repeat(v * 8f, 1f)), new Vector2(0.5f, 0.5f));
                if (dot < 0.08f)
                    c = Color.Lerp(c, accent, 0.5f);
                if (Mathf.Repeat(u * 4f, 1f) < 0.035f || Mathf.Repeat(v * 4f, 1f) < 0.035f)
                    c *= 0.70f;
                var labelChip = Mathf.PerlinNoise(u * 70f + hash * 0.01f, v * 70f);
                if (labelChip > 0.82f)
                    c = Color.Lerp(c, Color.white, 0.20f);
            }
            else if (pattern == SurfacePattern.Smoke)
            {
                c.a *= Mathf.Clamp01(0.4f + Mathf.PerlinNoise(u * 5f, v * 5f) * 0.5f);
            }
        }

        static void SetColor(Material material, string property, Color value)
        {
            if (material.HasProperty(property))
                material.SetColor(property, value);
        }

        static void SetTexture(Material material, string property, Texture texture)
        {
            if (material.HasProperty(property))
                material.SetTexture(property, texture);
        }

        static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property))
                material.SetFloat(property, value);
        }

        static int StableHash(string value)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];
                return Mathf.Abs(hash);
            }
        }

        static void EnsureFolderRecursive(string assetFolder)
        {
            var parts = assetFolder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        sealed class BuildingSpec
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly ActorArtCategory ArtCategory;
            public readonly BuildingVisualCategory VisualCategory;
            public readonly int Width;
            public readonly int Height;
            public bool HasDoor;
            public bool HasProduction;
            public bool HasTurbine;
            public bool HasRadar;
            public bool HasCrane;
            public bool HasRepairArms;
            public bool HasDock;
            public bool HasTurret;
            public float DoorOpenDistance = 0.35f;

            public BuildingSpec(string id, string displayName, ActorArtCategory artCategory, BuildingVisualCategory visualCategory, int width, int height)
            {
                Id = id;
                DisplayName = displayName;
                ArtCategory = artCategory;
                VisualCategory = visualCategory;
                Width = width;
                Height = height;
            }

            public BuildingSpec WithDoor(float distance) { HasDoor = true; DoorOpenDistance = distance; return this; }
            public BuildingSpec WithProduction() { HasProduction = true; return this; }
            public BuildingSpec WithTurbine() { HasTurbine = true; return this; }
            public BuildingSpec WithRadar() { HasRadar = true; return this; }
            public BuildingSpec WithCrane() { HasCrane = true; return this; }
            public BuildingSpec WithRepairArms() { HasRepairArms = true; return this; }
            public BuildingSpec WithDock() { HasDock = true; return this; }
            public BuildingSpec WithTurret() { HasTurret = true; return this; }
        }

        sealed class MaterialBundle
        {
            public Material Foundation;
            public Material Concrete;
            public Material DarkConcrete;
            public Material WornMetal;
            public Material DarkMetal;
            public Material EdgeMetal;
            public Material Gunmetal;
            public Material Door;
            public Material Panel;
            public Material InteriorFloor;
            public Material Hazard;
            public Material Asphalt;
            public Material PowerGlow;
            public Material Warning;
            public Material Medical;
            public Material Smoke;
            public Material FactionAccent;
            public Material ProductionAccent;
            public Material DefenseAccent;
            public Material TurretArmor;
            public Material TurretTop;
            public Material TurretBarrel;
            public Material TurretMuzzle;
            public Material ReviewGround;
        }

        enum SurfacePattern
        {
            Concrete,
            MetalPanel,
            TurretTop,
            RibbedDoor,
            PanelDots,
            Hazard,
            Asphalt,
            Ground,
            Glow,
            Smoke
        }
    }
}
