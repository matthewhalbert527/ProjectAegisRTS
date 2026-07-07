using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class UnityAiTankVisualGenerator
    {
        public const string ScenePath = "Assets/Rts/Scenes/UnityAI_TankVisualReview.unity";
        public const string ScreenshotRelativePath = "build/screenshots/unity_ai_tank_visual_review.png";
        const string AssetFolder = "Assets/Rts/Art/UnityAITankSlate";
        const string PrefabFolder = AssetFolder + "/Prefabs";
        const string MaterialFolder = AssetFolder + "/Materials";
        const string TextureFolder = AssetFolder + "/Textures";
        const string DefinitionFolder = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions";
        const string Stage33FallbackFolder = "Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks";
        const int TextureSize = 512;

        static readonly ActorPrefabSocketKind[] RequiredSockets =
        {
            ActorPrefabSocketKind.Root,
            ActorPrefabSocketKind.BodyRoot,
            ActorPrefabSocketKind.VisualRoot,
            ActorPrefabSocketKind.SelectionAnchor,
            ActorPrefabSocketKind.HealthBarAnchor,
            ActorPrefabSocketKind.UiAnchor,
            ActorPrefabSocketKind.TurretRoot,
            ActorPrefabSocketKind.BarrelRoot,
            ActorPrefabSocketKind.MuzzlePrimary,
            ActorPrefabSocketKind.TrackLeft,
            ActorPrefabSocketKind.TrackRight,
            ActorPrefabSocketKind.VfxSmoke,
            ActorPrefabSocketKind.VfxExplosion
        };

        static readonly TankSpec[] Specs =
        {
            new TankSpec("light_tank", "Light Tank", 4, 4, 1.74f, 1.18f, 0.58f, 4, 1, false,
                C(0.28f, 0.37f, 0.23f), C(0.12f, 0.14f, 0.12f), C(0.88f, 0.32f, 0.12f), C(0.10f, 0.45f, 0.48f)),
            new TankSpec("medium_tank", "Medium Tank", 5, 4, 2.12f, 1.42f, 0.68f, 5, 1, false,
                C(0.25f, 0.31f, 0.22f), C(0.10f, 0.12f, 0.10f), C(0.86f, 0.28f, 0.11f), C(0.08f, 0.40f, 0.47f)),
            new TankSpec("heavy_tank", "Heavy Tank", 6, 5, 2.58f, 1.74f, 0.82f, 6, 2, true,
                C(0.21f, 0.27f, 0.20f), C(0.075f, 0.085f, 0.075f), C(0.78f, 0.25f, 0.09f), C(0.07f, 0.36f, 0.42f))
        };

        [MenuItem("ProjectAegisRTS/Unity AI/Build Tank Visual Slate")]
        public static void BuildTankSlateMenu()
        {
            BuildTankSlate(false);
            Debug.Log("Unity AI tank visual slate generated.");
        }

        [MenuItem("ProjectAegisRTS/Unity AI/Build And Capture Tank Visual Slate")]
        public static void BuildAndCaptureTankSlateMenu()
        {
            BuildTankSlate(true);
            Debug.Log("Unity AI tank visual slate generated and captured.");
        }

        public static void BuildAndCaptureBatch()
        {
            try
            {
                BuildAndCaptureTankSlateMenu();
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateBatch()
        {
            try
            {
                ValidateGeneratedAssets();
                Debug.Log("Unity AI tank visual validation passed for 3 prefabs.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        static void BuildTankSlate(bool capture)
        {
            EnsureFolderRecursive(AssetFolder);
            EnsureFolderRecursive(PrefabFolder);
            EnsureFolderRecursive(MaterialFolder);
            EnsureFolderRecursive(TextureFolder);
            EnsureFolderRecursive(DefinitionFolder);

            var materials = CreateMaterials();
            var prefabs = new Dictionary<string, GameObject>();
            for (int i = 0; i < Specs.Length; i++)
            {
                var prefab = CreateOrUpdatePrefab(Specs[i], materials);
                UpdateVisualDefinition(Specs[i], prefab);
                prefabs[Specs[i].Id] = prefab;
            }

            CreateReviewScene(prefabs, materials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateGeneratedAssets();
            if (capture)
                CaptureScreenshot();
        }

        static MaterialBundle CreateMaterials()
        {
            var bundle = new MaterialBundle();
            for (int i = 0; i < Specs.Length; i++)
            {
                string id = Specs[i].Id;
                bundle.Armor[id] = CreateMaterial(id + "_olive_armor_ai", CreateAlbedoTexture(id + "_olive_armor_ai", Specs[i].Armor, Specs[i].Accent, TexturePattern.Armor), true, 0.18f, 0.48f);
            }

            bundle.Track = CreateMaterial("black_track_tread_ai", CreateAlbedoTexture("black_track_tread_ai", C(0.028f, 0.030f, 0.030f), C(0.18f, 0.18f, 0.16f), TexturePattern.Track), true, 0.05f, 0.76f);
            bundle.Rubber = CreateMaterial("deep_rubber_roadwheels_ai", CreateAlbedoTexture("deep_rubber_roadwheels_ai", C(0.020f, 0.020f, 0.018f), C(0.12f, 0.11f, 0.09f), TexturePattern.Rubber), false, 0.02f, 0.88f);
            bundle.Steel = CreateMaterial("worn_gunmetal_barrel_ai", CreateAlbedoTexture("worn_gunmetal_barrel_ai", C(0.25f, 0.27f, 0.25f), C(0.44f, 0.45f, 0.41f), TexturePattern.Steel), true, 0.48f, 0.38f);
            bundle.Accent = CreateMaterial("orange_safety_hardware_ai", CreateAlbedoTexture("orange_safety_hardware_ai", C(0.78f, 0.24f, 0.08f), C(0.98f, 0.50f, 0.20f), TexturePattern.Accent), false, 0.05f, 0.42f);
            bundle.Glass = CreateMaterial("blue_armored_optics_ai", CreateAlbedoTexture("blue_armored_optics_ai", C(0.05f, 0.20f, 0.22f), C(0.28f, 0.75f, 0.78f), TexturePattern.Glass), true, 0.0f, 0.18f);
            bundle.MuzzleFlash = CreateEmissionMaterial("realistic_muzzle_flash_ai", C(1.0f, 0.48f, 0.12f), C(1.0f, 0.86f, 0.35f));
            bundle.Smoke = CreateMaterial("soft_muzzle_smoke_ai", CreateAlbedoTexture("soft_muzzle_smoke_ai", C(0.22f, 0.22f, 0.20f), C(0.48f, 0.46f, 0.40f), TexturePattern.Smoke), false, 0.0f, 0.9f);
            return bundle;
        }

        static GameObject CreateOrUpdatePrefab(TankSpec spec, MaterialBundle materials)
        {
            var root = new GameObject(spec.Id + "_unity_ai_tank_art");
            root.transform.position = Vector3.zero;

            var descriptor = root.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = spec.Id;
            descriptor.category = ActorArtCategory.Vehicle;
            descriptor.productionStatus = ActorArtProductionStatus.Animated;
            descriptor.generatedByStage8 = false;
            descriptor.requiredSocketsPresent = true;
            descriptor.declaredRequiredSockets = new List<ActorPrefabSocketKind>(RequiredSockets);
            descriptor.notes = "Unity AI/Codex generated high-detail textured tank production visual. Stage33 socket fallback is preserved.";

            var source = root.AddComponent<Stage33TankSourceDescriptor>();
            source.actorTypeId = spec.Id;
            source.displayName = spec.DisplayName;
            source.visualTier = "UnityAITexturedProduction";
            source.fineFootprint = new Vector2Int(spec.FootprintX, spec.FootprintY);
            source.visualLengthMeters = spec.Length;
            source.visualWidthMeters = spec.Width;
            source.visualHeightMeters = spec.Height;
            source.questSafeProxy = false;
            source.notes = "Textured Unity AI tank visual with animated track, turret, recoil, and muzzle-flash hooks.";
            source.root = root.transform;

            AddStageTags(root, spec);

            AddSocket(root.transform, Stage33TankSocketKind.Root, ActorPrefabSocketKind.Root, "Root", spec.Id, Vector3.zero);
            var visualRoot = AddSocket(root.transform, Stage33TankSocketKind.VisualRoot, ActorPrefabSocketKind.VisualRoot, "VisualRoot", spec.Id, Vector3.zero);
            var bodyRoot = AddSocket(visualRoot, Stage33TankSocketKind.BodyRoot, ActorPrefabSocketKind.BodyRoot, "BodyRoot", spec.Id, Vector3.zero);
            source.visualRoot = visualRoot;
            source.bodyRoot = bodyRoot;

            var armor = materials.Armor[spec.Id];
            BuildHull(bodyRoot, spec, armor, materials);
            var trackRenderers = BuildTracks(bodyRoot, spec, materials);
            source.trackLeft = FindChild(bodyRoot, "TrackLeft");
            source.trackRight = FindChild(bodyRoot, "TrackRight");
            var turretRoot = BuildTurret(bodyRoot, spec, armor, materials, source);
            BuildSockets(root.transform, spec, source);

            var rig = root.AddComponent<TankVisualRigController>();
            rig.bodyRoot = bodyRoot;
            rig.turretRoot = turretRoot;
            rig.trackLeftRoot = source.trackLeft;
            rig.trackRightRoot = source.trackRight;
            rig.trackRenderers = trackRenderers.ToArray();
            rig.wheelLeft = FindTransforms(bodyRoot, "WheelLeft_");
            rig.wheelRight = FindTransforms(bodyRoot, "WheelRight_");
            rig.barrelRoot = source.barrelRoot;
            rig.muzzleFlashRoot = FindChild(source.muzzlePrimary, "MuzzleFlash");
            rig.muzzleFlashLight = rig.muzzleFlashRoot == null ? null : rig.muzzleFlashRoot.GetComponentInChildren<Light>(true);
            rig.driveTurretFromDesiredAim = false;
            rig.trackScrollUnitsPerMeter = spec.Id == "heavy_tank" ? 2.1f : 2.6f;
            rig.wheelRotationDegreesPerMeter = spec.Id == "light_tank" ? 820f : 680f;
            rig.barrelRecoilDistance = spec.BarrelCount > 1 ? 0.06f : 0.075f;

            AddLodGroup(root);

            string path = PrefabFolder + "/" + spec.Id + "_unity_ai_tank.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            EditorUtility.SetDirty(prefab);
            return prefab;
        }

        static void BuildHull(Transform parent, TankSpec spec, Material armor, MaterialBundle materials)
        {
            float length = spec.Length;
            float width = spec.Width;
            CreateBox(parent, "GridAccurateShadowBase", V3(0f, 0.025f, 0f), V3(width * 1.06f, 0.05f, length * 1.05f), materials.Track);
            CreateBox(parent, "LowerArmoredChassis", V3(0f, 0.20f, 0f), V3(width * 0.88f, 0.30f, length * 0.83f), armor);
            CreateBox(parent, "UpperSlopedArmorDeck", V3(0f, 0.43f, 0.03f), V3(width * 0.66f, 0.22f, length * 0.50f), armor);
            CreateBox(parent, "FrontWedgeGlacis", V3(0f, 0.42f, length * 0.34f), V3(width * 0.74f, 0.16f, length * 0.22f), armor, Q(0f, 0f, 0f));
            CreateBox(parent, "RearEngineDeck", V3(0f, 0.50f, -length * 0.37f), V3(width * 0.74f, 0.10f, length * 0.19f), materials.Steel);

            for (int side = -1; side <= 1; side += 2)
            {
                CreateBox(parent, SideName(side, "ArmorSkirt"), V3(side * width * 0.55f, 0.29f, 0f), V3(0.11f, 0.30f, length * 0.77f), armor);
                CreateBox(parent, SideName(side, "ReactiveArmorLong"), V3(side * width * 0.47f, 0.55f, -length * 0.08f), V3(0.10f, 0.10f, length * 0.34f), armor);
                for (int i = 0; i < 4; i++)
                {
                    float z = Mathf.Lerp(-length * 0.34f, length * 0.34f, i / 3f);
                    CreateBox(parent, SideName(side, "OrangeTieDown_" + i), V3(side * width * 0.43f, 0.63f, z), V3(0.04f, 0.035f, 0.14f), materials.Accent);
                }
            }

            int panels = spec.Id == "light_tank" ? 5 : 7;
            for (int i = 0; i < panels; i++)
            {
                float x = Mathf.Lerp(-width * 0.25f, width * 0.25f, panels == 1 ? 0.5f : i / (panels - 1f));
                float z = Mathf.Lerp(-length * 0.27f, length * 0.28f, i % 3 / 2f);
                CreateBox(parent, "TopPanel_" + i, V3(x, 0.57f + 0.006f * (i % 2), z), V3(width * 0.16f, 0.030f, length * 0.115f), armor);
            }

            CreateRivetLine(parent, "FrontArmorBolts", -width * 0.30f, width * 0.30f, 0.58f, length * 0.43f, 6, materials.Steel);
            CreateRivetLine(parent, "RearDeckBolts", -width * 0.28f, width * 0.28f, 0.61f, -length * 0.39f, 5, materials.Steel);
            CreateRivetColumn(parent, "LeftSkirtBolts", -width * 0.58f, -length * 0.35f, length * 0.35f, 0.42f, 6, materials.Steel);
            CreateRivetColumn(parent, "RightSkirtBolts", width * 0.58f, -length * 0.35f, length * 0.35f, 0.42f, 6, materials.Steel);

            CreateBox(parent, "LeftHeadlamp", V3(-width * 0.28f, 0.41f, length * 0.49f), V3(0.16f, 0.055f, 0.04f), materials.Glass);
            CreateBox(parent, "RightHeadlamp", V3(width * 0.28f, 0.41f, length * 0.49f), V3(0.16f, 0.055f, 0.04f), materials.Glass);
            CreateBox(parent, "FrontTowHookLeft", V3(-width * 0.38f, 0.20f, length * 0.53f), V3(0.06f, 0.10f, 0.08f), materials.Accent);
            CreateBox(parent, "FrontTowHookRight", V3(width * 0.38f, 0.20f, length * 0.53f), V3(0.06f, 0.10f, 0.08f), materials.Accent);

            if (spec.Id == "medium_tank" || spec.Id == "heavy_tank")
            {
                CreateBox(parent, "RearStowageRack", V3(0f, 0.66f, -length * 0.47f), V3(width * 0.62f, 0.13f, 0.12f), materials.Steel);
                CreateBox(parent, "LeftFuelCanister", V3(-width * 0.31f, 0.63f, -length * 0.33f), V3(0.13f, 0.12f, 0.20f), materials.Accent);
                CreateBox(parent, "RightFuelCanister", V3(width * 0.31f, 0.63f, -length * 0.33f), V3(0.13f, 0.12f, 0.20f), materials.Accent);
            }

            if (spec.HasMissilePod)
                BuildMissilePod(parent, spec, materials);
        }

        static List<Renderer> BuildTracks(Transform parent, TankSpec spec, MaterialBundle materials)
        {
            var renderers = new List<Renderer>();
            for (int side = -1; side <= 1; side += 2)
            {
                var track = CreateBox(parent, side < 0 ? "TrackLeft" : "TrackRight", V3(side * spec.Width * 0.56f, 0.19f, 0f), V3(0.22f, 0.22f, spec.Length * 0.84f), materials.Track);
                renderers.Add(track.GetComponent<Renderer>());
                AddSocket(track.transform, side < 0 ? Stage33TankSocketKind.TrackLeft : Stage33TankSocketKind.TrackRight, side < 0 ? ActorPrefabSocketKind.TrackLeft : ActorPrefabSocketKind.TrackRight, side < 0 ? "TrackLeft" : "TrackRight", spec.Id, Vector3.zero);

                for (int i = 0; i < spec.Wheels; i++)
                {
                    float z = Mathf.Lerp(-spec.Length * 0.34f, spec.Length * 0.34f, spec.Wheels == 1 ? 0.5f : i / (spec.Wheels - 1f));
                    var wheel = CreateCylinder(parent, SideName(side, "Wheel_" + i), V3(side * spec.Width * 0.60f, 0.20f, z), 0.13f, 0.065f, materials.Rubber, Axis.X);
                    wheel.name = side < 0 ? "WheelLeft_" + i : "WheelRight_" + i;
                    CreateCylinder(wheel.transform, "OrangeHub", Vector3.zero, 0.055f, 0.072f, materials.Accent, Axis.X);
                }

                int cleats = spec.Wheels * 5;
                for (int i = 0; i < cleats; i++)
                {
                    float z = Mathf.Lerp(-spec.Length * 0.44f, spec.Length * 0.44f, cleats == 1 ? 0.5f : i / (cleats - 1f));
                    CreateBox(parent, SideName(side, "TrackCleat_" + i), V3(side * spec.Width * 0.60f, 0.06f, z), V3(0.26f, 0.035f, 0.035f), materials.Rubber);
                    CreateBox(parent, SideName(side, "UpperTrackPlate_" + i), V3(side * spec.Width * 0.59f, 0.33f, z), V3(0.24f, 0.028f, 0.036f), materials.Steel);
                }
            }

            return renderers;
        }

        static Transform BuildTurret(Transform parent, TankSpec spec, Material armor, MaterialBundle materials, Stage33TankSourceDescriptor source)
        {
            var turretRoot = AddSocket(parent, Stage33TankSocketKind.TurretRoot, ActorPrefabSocketKind.TurretRoot, "TurretRoot", spec.Id, V3(0f, spec.Height * 0.79f, spec.Length * 0.08f));
            source.turretRoot = turretRoot;

            CreateBox(turretRoot, "TurretArmoredCore", Vector3.zero, V3(spec.Width * 0.50f, 0.24f, spec.Length * 0.28f), armor);
            CreateBox(turretRoot, "TurretRearBustle", V3(0f, 0.02f, -spec.Length * 0.17f), V3(spec.Width * 0.46f, 0.19f, spec.Length * 0.20f), armor);
            CreateBox(turretRoot, "LeftTurretCheek", V3(-spec.Width * 0.22f, 0.0f, spec.Length * 0.05f), V3(0.14f, 0.21f, spec.Length * 0.24f), armor);
            CreateBox(turretRoot, "RightTurretCheek", V3(spec.Width * 0.22f, 0.0f, spec.Length * 0.05f), V3(0.14f, 0.21f, spec.Length * 0.24f), armor);
            CreateBox(turretRoot, "BlueGunnerOptic", V3(spec.Width * 0.18f, 0.14f, spec.Length * 0.12f), V3(0.12f, 0.07f, 0.06f), materials.Glass);
            CreateCylinder(turretRoot, "CommanderHatch", V3(-spec.Width * 0.14f, 0.16f, -spec.Length * 0.04f), 0.11f, 0.04f, armor, Axis.Y);
            CreateCylinder(turretRoot, "Antenna", V3(spec.Width * 0.16f, 0.30f, -spec.Length * 0.14f), 0.012f, 0.45f, materials.Steel, Axis.Y);

            var barrelRoot = AddSocket(turretRoot, Stage33TankSocketKind.BarrelRoot, ActorPrefabSocketKind.BarrelRoot, "BarrelRoot", spec.Id, V3(0f, 0.02f, spec.Length * 0.18f));
            source.barrelRoot = barrelRoot;

            float spacing = spec.BarrelCount > 1 ? spec.Width * 0.15f : 0f;
            for (int i = 0; i < spec.BarrelCount; i++)
            {
                float x = spec.BarrelCount == 1 ? 0f : (i == 0 ? -spacing : spacing);
                CreateCylinder(barrelRoot, "MainGunBarrel_" + i, V3(x, 0f, spec.BarrelLength * 0.50f), 0.052f, spec.BarrelLength, materials.Steel, Axis.Z);
                CreateCylinder(barrelRoot, "MuzzleBrake_" + i, V3(x, 0f, spec.BarrelLength + 0.02f), 0.078f, 0.12f, materials.Steel, Axis.Z);
                CreateCylinder(barrelRoot, "DarkMuzzleBore_" + i, V3(x, 0f, spec.BarrelLength + 0.087f), 0.046f, 0.026f, materials.Track, Axis.Z);
            }

            var muzzle = AddSocket(barrelRoot, Stage33TankSocketKind.MuzzlePrimary, ActorPrefabSocketKind.MuzzlePrimary, "MuzzlePrimary", spec.Id, V3(0f, 0f, spec.BarrelLength + 0.12f));
            source.muzzlePrimary = muzzle;
            CreateMuzzleFlash(muzzle, materials);
            return turretRoot;
        }

        static void BuildMissilePod(Transform parent, TankSpec spec, MaterialBundle materials)
        {
            var podRoot = new GameObject("RearRightMissileBattery");
            podRoot.transform.SetParent(parent, false);
            podRoot.transform.localPosition = V3(spec.Width * 0.28f, 0.78f, -spec.Length * 0.30f);
            podRoot.transform.localScale = Vector3.one;
            CreateBox(podRoot.transform, "MissilePodCasing", Vector3.zero, V3(spec.Width * 0.36f, 0.36f, spec.Length * 0.22f), materials.Steel);
            CreateBox(podRoot.transform, "MissilePodDarkFace", V3(0f, 0f, -spec.Length * 0.115f), V3(spec.Width * 0.31f, 0.29f, 0.026f), materials.Track);

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    float lx = Mathf.Lerp(-spec.Width * 0.12f, spec.Width * 0.12f, x / 3f);
                    float ly = Mathf.Lerp(-0.08f, 0.08f, y);
                    CreateCylinder(podRoot.transform, "MissileTube_" + y + "_" + x, V3(lx, ly, -spec.Length * 0.130f), 0.041f, 0.040f, materials.Accent, Axis.Z);
                    CreateCylinder(podRoot.transform, "MissileTubeDarkBore_" + y + "_" + x, V3(lx, ly, -spec.Length * 0.154f), 0.026f, 0.018f, materials.Track, Axis.Z);
                }
            }
        }

        static void CreateRivetLine(Transform parent, string prefix, float x0, float x1, float y, float z, int count, Material material)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (count - 1f);
                CreateCylinder(parent, prefix + "_" + i, V3(Mathf.Lerp(x0, x1, t), y, z), 0.018f, 0.012f, material, Axis.Y);
            }
        }

        static void CreateRivetColumn(Transform parent, string prefix, float x, float z0, float z1, float y, int count, Material material)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (count - 1f);
                CreateCylinder(parent, prefix + "_" + i, V3(x, y, Mathf.Lerp(z0, z1, t)), 0.016f, 0.010f, material, Axis.X);
            }
        }

        static void BuildSockets(Transform root, TankSpec spec, Stage33TankSourceDescriptor source)
        {
            source.selectionAnchor = AddSocket(root, Stage33TankSocketKind.SelectionAnchor, ActorPrefabSocketKind.SelectionAnchor, "SelectionAnchor", spec.Id, Vector3.zero);
            source.healthBarAnchor = AddSocket(root, Stage33TankSocketKind.HealthBarAnchor, ActorPrefabSocketKind.HealthBarAnchor, "HealthBarAnchor", spec.Id, V3(0f, spec.Height + 0.70f, 0f));
            source.uiAnchor = AddSocket(root, Stage33TankSocketKind.UiAnchor, ActorPrefabSocketKind.UiAnchor, "UiAnchor", spec.Id, V3(0f, spec.Height + 0.95f, 0f));
            source.vfxSmoke = AddSocket(root, Stage33TankSocketKind.VfxSmoke, ActorPrefabSocketKind.VfxSmoke, "VfxSmoke", spec.Id, V3(0f, spec.Height + 0.18f, -spec.Length * 0.48f));
            source.vfxExplosion = AddSocket(root, Stage33TankSocketKind.VfxExplosion, ActorPrefabSocketKind.VfxExplosion, "VfxExplosion", spec.Id, V3(0f, spec.Height * 0.62f, 0f));
        }

        static void CreateMuzzleFlash(Transform muzzle, MaterialBundle materials)
        {
            var root = new GameObject("MuzzleFlash");
            root.transform.SetParent(muzzle, false);
            root.transform.localPosition = Vector3.zero;
            root.SetActive(false);

            CreateCylinder(root.transform, "FlashCore", V3(0f, 0f, 0.05f), 0.11f, 0.20f, materials.MuzzleFlash, Axis.Z);
            CreateBox(root.transform, "FlashStarHorizontal", V3(0f, 0f, 0.04f), V3(0.30f, 0.035f, 0.08f), materials.MuzzleFlash);
            CreateBox(root.transform, "FlashStarVertical", V3(0f, 0f, 0.04f), V3(0.035f, 0.30f, 0.08f), materials.MuzzleFlash);
            CreateCylinder(root.transform, "SmokePuff", V3(0f, 0f, 0.16f), 0.13f, 0.05f, materials.Smoke, Axis.Z);
            var lightObj = new GameObject("MuzzleFlashLight");
            lightObj.transform.SetParent(root.transform, false);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 2.0f;
            light.intensity = 0f;
            light.color = C(1.0f, 0.64f, 0.28f);
            light.enabled = false;
        }

        static void AddStageTags(GameObject root, TankSpec spec)
        {
            var tag = root.AddComponent<ProductionVisualValidationTag>();
            tag.actorTypeId = spec.Id;
            tag.visualTier = ProductionVisualTier.ProductionModel;
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
            tag.replacementNotes = "Unity AI tank slate production visual. Preserve sockets, tracks, turret, barrel, and muzzle transforms for future artist-authored mesh replacement.";
            tag.notes = "High-detail textured tank with distinct silhouette, moving track material hooks, turret aim, recoil, and muzzle flash.";

            var detail = root.AddComponent<Stage29VisualDetailTag>();
            detail.actorTypeId = spec.Id;
            detail.hasRealisticMaterialPass = true;
            detail.hasSilhouetteBreakup = true;
            detail.hasFineGridGrounding = true;
            detail.hasReadableTopProfile = true;
            detail.hasFrontSideRearCues = true;
            detail.preservesStage20Sockets = true;
            detail.preservesAnimationHooks = true;
            detail.questSafePrimitiveBudget = true;
            detail.notes = "Layered armor panels, exposed tread/wheels, optics, hardware accent, and 360-degree cues.";

            var readability = root.AddComponent<Stage30VisualReadabilityTag>();
            readability.actorTypeId = spec.Id;
            readability.hasGroundContrastOutline = true;
            readability.hasTopDownIdentityAccent = true;
            readability.hasForwardReadabilityCue = true;
            readability.preservesStage29Detail = true;
            readability.questSafeOverlayBudget = true;
            readability.notes = "Black track mass, orange accents, forward optics and muzzle distinguish tank direction at RTS zoom.";
        }

        static void UpdateVisualDefinition(TankSpec spec, GameObject prefab)
        {
            string path = DefinitionFolder + "/" + spec.Id + "_visual.asset";
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ActorVisualDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.actorTypeId = spec.Id;
            definition.displayName = spec.DisplayName;
            definition.safeDisplayName = spec.DisplayName;
            definition.category = ActorArtCategory.Vehicle;
            definition.productionStatus = ActorArtProductionStatus.Animated;
            definition.productionPrefab = prefab;
            definition.preferredPrefabMode = ActorVisualPrefabMode.ProductionPrefab;
            definition.motionProfileId = spec.Id == "heavy_tank" ? "tracked_heavy" : (spec.Id == "medium_tank" ? "tracked_medium" : "tracked_light");
            definition.useVehicleMotionController = true;
            definition.useTurretVisualController = true;
            definition.footprintWidth = spec.FootprintX;
            definition.footprintHeight = spec.FootprintY;
            definition.selectionRadius = Mathf.Max(spec.Width, spec.Length) * 0.45f;
            definition.selectionHeight = 0.08f;
            definition.visualScale = 1f;
            definition.prefabHeightOffset = 0f;
            definition.ipReviewRequired = false;
            definition.notes = "Production visual now uses Unity AI/Codex generated textured tank prefab. Stage33 source prefab remains fallback.";

            var fallback = AssetDatabase.LoadAssetAtPath<GameObject>(Stage33FallbackFolder + "/" + spec.Id + "_tank_source.prefab");
            if (fallback != null)
                definition.fallbackPrefab = fallback;
            definition.requiredSockets = new List<ActorPrefabSocketKind>(RequiredSockets);
            EditorUtility.SetDirty(definition);
        }

        static void CreateReviewScene(Dictionary<string, GameObject> prefabs, MaterialBundle materials)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "UnityAI_TankVisualReview";

            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3.25f;
            cameraObj.transform.position = V3(0f, 5.6f, -4.9f);
            cameraObj.transform.rotation = Quaternion.Euler(56f, 0f, 0f);
            cameraObj.tag = "MainCamera";

            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            lightObj.transform.rotation = Quaternion.Euler(48f, 32f, 0f);

            var fillObj = new GameObject("Warm Fill Light");
            var fill = fillObj.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.35f;
            fill.color = C(1f, 0.86f, 0.70f);
            fillObj.transform.rotation = Quaternion.Euler(26f, -145f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Dark Matte Review Ground";
            ground.transform.position = V3(0f, -0.04f, 0f);
            ground.transform.localScale = V3(8.7f, 0.04f, 3.8f);
            var groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
                groundRenderer.sharedMaterial = CreateFlatMaterial("unity_ai_tank_review_ground", C(0.10f, 0.13f, 0.12f), 0f, 0.7f);
            var groundCollider = ground.GetComponent<Collider>();
            if (groundCollider != null)
                UnityEngine.Object.DestroyImmediate(groundCollider);

            for (int i = 0; i < Specs.Length; i++)
            {
                GameObject prefab;
                if (!prefabs.TryGetValue(Specs[i].Id, out prefab) || prefab == null)
                    continue;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = Specs[i].DisplayName + " Animated Review";
                float x = (i - 1) * 2.85f;
                instance.transform.position = V3(x, 0f, 0f);
                instance.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
                var demo = instance.AddComponent<TankVisualDemoController>();
                demo.rig = instance.GetComponent<TankVisualRigController>();
                demo.orbitCenter = V3(x, 0f, 0f);
                demo.orbitRadius = 0.34f + i * 0.05f;
                demo.fireInterval = 1.25f + i * 0.28f;
            }

            CreateLabel("Light: compact scout chassis", V3(-2.85f, 0.02f, -1.55f));
            CreateLabel("Medium: broader armor panels", V3(0f, 0.02f, -1.55f));
            CreateLabel("Heavy: twin guns + missile pod", V3(2.85f, 0.02f, -1.55f));

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static string CaptureScreenshot()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera camera = null;
            var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
                if (cameras[i] != null && cameras[i].gameObject.name == "Main Camera")
                    camera = cameras[i];
            if (camera == null)
                throw new InvalidOperationException("Review scene has no Main Camera.");

            string repoRoot = GetRepoRootPath();
            string outputPath = Path.Combine(repoRoot, ScreenshotRelativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32);
            try
            {
                camera.targetTexture = rt;
                RenderTexture.active = rt;
                camera.Render();
                var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log("Unity AI tank visual screenshot captured: " + outputPath);
            return outputPath;
        }

        static void ValidateGeneratedAssets()
        {
            var failures = new List<string>();
            for (int i = 0; i < Specs.Length; i++)
            {
                var spec = Specs[i];
                string path = PrefabFolder + "/" + spec.Id + "_unity_ai_tank.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    failures.Add("Missing tank prefab: " + path);
                    continue;
                }

                var descriptor = prefab.GetComponent<ActorPrefabDescriptor>();
                if (descriptor == null || descriptor.productionStatus != ActorArtProductionStatus.Animated)
                    failures.Add("Missing animated ActorPrefabDescriptor: " + path);
                var source = prefab.GetComponent<Stage33TankSourceDescriptor>();
                if (source == null || !source.HasRequiredSockets())
                    failures.Add("Missing Stage33 socket descriptor: " + path);
                var rig = prefab.GetComponent<TankVisualRigController>();
                if (rig == null || rig.trackRenderers == null || rig.trackRenderers.Length < 2 || rig.muzzleFlashRoot == null)
                    failures.Add("Missing tank rig track/muzzle hooks: " + path);

                var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length < 55)
                    failures.Add("Prefab has too little visible detail: " + path + " renderers=" + renderers.Length);

                int texturedMaterials = CountTexturedMaterials(renderers);
                if (texturedMaterials < 5)
                    failures.Add("Prefab does not use enough textured materials: " + path + " textured=" + texturedMaterials);

                var detail = prefab.GetComponent<Stage29VisualDetailTag>();
                if (detail == null || !detail.IsComplete())
                    failures.Add("Stage29 detail tag incomplete: " + path);
                var readability = prefab.GetComponent<Stage30VisualReadabilityTag>();
                if (readability == null || !readability.IsComplete())
                    failures.Add("Stage30 readability tag incomplete: " + path);

                var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(DefinitionFolder + "/" + spec.Id + "_visual.asset");
                if (definition == null || definition.productionPrefab != prefab || definition.preferredPrefabMode != ActorVisualPrefabMode.ProductionPrefab)
                    failures.Add("ActorVisualDefinition does not point to tank production prefab: " + spec.Id);
            }

            if (!File.Exists(Path.Combine(Application.dataPath, ScenePath.Substring("Assets/".Length)).Replace('\\', '/')))
                failures.Add("Missing review scene: " + ScenePath);

            if (failures.Count > 0)
                throw new InvalidOperationException("Unity AI tank visual validation failed:\n" + string.Join("\n", failures.ToArray()));
        }

        static int CountTexturedMaterials(Renderer[] renderers)
        {
            var paths = new HashSet<string>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var mats = renderers[i].sharedMaterials;
                if (mats == null)
                    continue;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null || mats[m].mainTexture == null)
                        continue;
                    string path = AssetDatabase.GetAssetPath(mats[m].mainTexture);
                    if (!string.IsNullOrEmpty(path))
                        paths.Add(path);
                }
            }
            return paths.Count;
        }

        static Material CreateMaterial(string name, Texture2D albedo, bool createNormal, float metallic, float roughness)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            string path = MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.mainTexture = albedo;
            SetTexture(material, "_BaseMap", albedo);
            SetTexture(material, "_MainTex", albedo);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughness));

            if (createNormal)
            {
                var normal = CreateDerivedNormalTexture(name + "_derived_normal", albedo);
                SetTexture(material, "_BumpMap", normal);
                if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 0.45f);
                material.EnableKeyword("_NORMALMAP");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateFlatMaterial(string name, Color color, float metallic, float roughness)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            string path = MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughness));
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material CreateEmissionMaterial(string name, Color baseColor, Color emissionColor)
        {
            var material = CreateFlatMaterial(name, baseColor, 0f, 0.18f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", emissionColor * 2.6f);
                material.EnableKeyword("_EMISSION");
            }
            return material;
        }

        static Texture2D CreateAlbedoTexture(string name, Color baseColor, Color accent, TexturePattern pattern)
        {
            string path = TextureFolder + "/" + name + ".png";
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, true);
            texture.name = name;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = x / (TextureSize - 1f);
                    float v = y / (TextureSize - 1f);
                    float noise = FractalNoise(x, y, name.GetHashCode());
                    Color color = Color.Lerp(baseColor * 0.72f, baseColor * 1.25f, noise);
                    color = ApplyPattern(color, accent, u, v, noise, pattern);
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply(true, false);
            SaveTexture(path, texture, false);
            UnityEngine.Object.DestroyImmediate(texture);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D CreateDerivedNormalTexture(string name, Texture2D source)
        {
            string path = TextureFolder + "/" + name + ".png";
            var src = source;
            if (src == null)
                throw new InvalidOperationException("Cannot create normal map without source texture: " + name);
            var readable = ReadTexturePixels(AssetDatabase.GetAssetPath(src));
            var normal = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, true, true);
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float l = Luminance(readable.GetPixel(Mathf.Max(0, x - 1), y));
                    float r = Luminance(readable.GetPixel(Mathf.Min(TextureSize - 1, x + 1), y));
                    float d = Luminance(readable.GetPixel(x, Mathf.Max(0, y - 1)));
                    float u = Luminance(readable.GetPixel(x, Mathf.Min(TextureSize - 1, y + 1)));
                    Vector3 n = new Vector3((l - r) * 1.7f, (d - u) * 1.7f, 1f).normalized;
                    normal.SetPixel(x, y, new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f));
                }
            }
            normal.Apply(true, false);
            SaveTexture(path, normal, true);
            UnityEngine.Object.DestroyImmediate(readable);
            UnityEngine.Object.DestroyImmediate(normal);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Color ApplyPattern(Color color, Color accent, float u, float v, float noise, TexturePattern pattern)
        {
            if (pattern == TexturePattern.Armor)
            {
                float panelU = Mathf.Abs(Mathf.Repeat(u * 5.0f, 1f) - 0.5f);
                float panelV = Mathf.Abs(Mathf.Repeat(v * 5.0f, 1f) - 0.5f);
                if (panelU > 0.475f || panelV > 0.475f)
                    color *= 0.52f;
                if ((u > 0.08f && u < 0.13f && v > 0.10f && v < 0.88f) || (v > 0.74f && v < 0.79f && u > 0.18f && u < 0.82f))
                    color = Color.Lerp(color, accent, 0.65f);
                if (Hash01((int)(u * 70f), (int)(v * 70f), 19) > 0.94f)
                    color = Color.Lerp(color, Color.black, 0.55f);
            }
            else if (pattern == TexturePattern.Track)
            {
                float stripe = Mathf.Abs(Mathf.Repeat(v * 14f, 1f) - 0.5f);
                if (stripe > 0.38f)
                    color *= 1.55f;
                if (Mathf.Abs(u - 0.5f) < 0.035f)
                    color *= 0.45f;
            }
            else if (pattern == TexturePattern.Steel)
            {
                if (Mathf.Abs(Mathf.Repeat(v * 9f + u * 2f, 1f) - 0.5f) > 0.46f)
                    color *= 0.65f;
                color = Color.Lerp(color, Color.white, Mathf.Clamp01(noise - 0.68f) * 0.28f);
            }
            else if (pattern == TexturePattern.Glass)
            {
                float flare = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(u, v), new Vector2(0.35f, 0.68f)) * 3.5f);
                color = Color.Lerp(color, accent, flare);
            }
            else if (pattern == TexturePattern.Accent)
            {
                if (Mathf.Abs(Mathf.Repeat(u * 6f, 1f) - 0.5f) > 0.44f)
                    color *= 0.72f;
            }
            else if (pattern == TexturePattern.Smoke)
            {
                color.a = Mathf.Lerp(0.45f, 0.85f, noise);
            }
            return new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), Mathf.Clamp01(color.a));
        }

        static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            return CreateBox(parent, name, localPosition, localScale, material, Quaternion.identity);
        }

        static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material, Quaternion localRotation)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = localScale;
            AssignMaterial(go, material);
            StripCollider(go);
            return go;
        }

        static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, float radius, float length, Material material, Axis axis)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = V3(radius, length * 0.5f, radius);
            if (axis == Axis.X)
                go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            else if (axis == Axis.Z)
                go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            AssignMaterial(go, material);
            StripCollider(go);
            return go;
        }

        static Transform AddSocket(Transform parent, Stage33TankSocketKind stageKind, ActorPrefabSocketKind projectKind, string name, string actorTypeId, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var stageSocket = go.AddComponent<Stage33TankSocket>();
            stageSocket.socketKind = stageKind;
            stageSocket.socketName = name;
            stageSocket.actorTypeId = actorTypeId;
            stageSocket.required = true;
            var socket = go.AddComponent<ActorPrefabSocket>();
            socket.socketKind = projectKind;
            socket.socketName = name;
            socket.actorTypeId = actorTypeId;
            socket.notes = "Unity AI tank visual socket. Preserve name, pivot, and role.";
            return go.transform;
        }

        static void AddLodGroup(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var lod = root.GetComponent<LODGroup>();
            if (lod == null)
                lod = root.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(0.32f, renderers), new LOD(0.09f, renderers) });
            lod.RecalculateBounds();
            var tag = root.GetComponent<ProductionVisualValidationTag>();
            if (tag != null)
                tag.hasLodGroup = true;
        }

        static void AssignMaterial(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }

        static void StripCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }

        static Transform[] FindTransforms(Transform root, string prefix)
        {
            var list = new List<Transform>();
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null && transforms[i].name.StartsWith(prefix, StringComparison.Ordinal))
                    list.Add(transforms[i]);
            return list.ToArray();
        }

        static Transform FindChild(Transform root, string name)
        {
            if (root == null)
                return null;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null && transforms[i].name == name)
                    return transforms[i];
            return null;
        }

        static void CreateLabel(string text, Vector3 position)
        {
            var go = new GameObject("Label - " + text);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = 0.105f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = C(0.80f, 0.86f, 0.80f);
        }

        static void SetTexture(Material material, string propertyName, Texture texture)
        {
            if (material != null && texture != null && material.HasProperty(propertyName))
                material.SetTexture(propertyName, texture);
        }

        static void SaveTexture(string assetPath, Texture2D texture, bool normalMap)
        {
            string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normalMap;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 8;
                importer.maxTextureSize = TextureSize;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        static Texture2D ReadTexturePixels(string assetPath)
        {
            string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('\\', '/');
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            if (!texture.LoadImage(File.ReadAllBytes(fullPath)))
                throw new InvalidOperationException("Could not read texture pixels: " + assetPath);
            return texture;
        }

        static float Luminance(Color c)
        {
            return c.r * 0.2126f + c.g * 0.7152f + c.b * 0.0722f;
        }

        static float FractalNoise(int x, int y, int seed)
        {
            float n = 0f;
            float amp = 0.5f;
            int scale = 5;
            for (int i = 0; i < 4; i++)
            {
                n += Hash01(x / scale, y / scale, seed + i * 101) * amp;
                amp *= 0.5f;
                scale *= 2;
            }
            return Mathf.Clamp01(n);
        }

        static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                return (h & 0x7fffffff) / 2147483647f;
            }
        }

        static void EnsureFolderRecursive(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static string GetRepoRootPath()
        {
            DirectoryInfo assetsDirectory = new DirectoryInfo(Application.dataPath);
            DirectoryInfo unityDirectory = assetsDirectory.Parent;
            DirectoryInfo repoDirectory = unityDirectory != null ? unityDirectory.Parent : null;
            return repoDirectory != null ? repoDirectory.FullName : Directory.GetCurrentDirectory();
        }

        static Color C(float r, float g, float b)
        {
            return new Color(r, g, b, 1f);
        }

        static Vector3 V3(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        static Quaternion Q(float x, float y, float z)
        {
            return Quaternion.Euler(x, y, z);
        }

        static string SideName(int side, string suffix)
        {
            return side < 0 ? "Left" + suffix : "Right" + suffix;
        }

        sealed class MaterialBundle
        {
            public readonly Dictionary<string, Material> Armor = new Dictionary<string, Material>();
            public Material Track;
            public Material Rubber;
            public Material Steel;
            public Material Accent;
            public Material Glass;
            public Material MuzzleFlash;
            public Material Smoke;
        }

        struct TankSpec
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly int FootprintX;
            public readonly int FootprintY;
            public readonly float Length;
            public readonly float Width;
            public readonly float Height;
            public readonly int Wheels;
            public readonly int BarrelCount;
            public readonly bool HasMissilePod;
            public readonly float BarrelLength;
            public readonly Color Armor;
            public readonly Color Dark;
            public readonly Color Accent;
            public readonly Color Glass;

            public TankSpec(string id, string displayName, int footprintX, int footprintY, float length, float width, float height, int wheels, int barrelCount, bool hasMissilePod, Color armor, Color dark, Color accent, Color glass)
            {
                Id = id;
                DisplayName = displayName;
                FootprintX = footprintX;
                FootprintY = footprintY;
                Length = length;
                Width = width;
                Height = height;
                Wheels = wheels;
                BarrelCount = barrelCount;
                HasMissilePod = hasMissilePod;
                BarrelLength = id == "heavy_tank" ? 1.22f : (id == "medium_tank" ? 1.02f : 0.82f);
                Armor = armor;
                Dark = dark;
                Accent = accent;
                Glass = glass;
            }
        }

        enum TexturePattern
        {
            Armor,
            Track,
            Rubber,
            Steel,
            Accent,
            Glass,
            Smoke
        }

        enum Axis
        {
            X,
            Y,
            Z
        }
    }
}
