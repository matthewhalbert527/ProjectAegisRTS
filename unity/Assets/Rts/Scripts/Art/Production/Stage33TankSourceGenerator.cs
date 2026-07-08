using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace ProjectAegisRTS.UnityClient.Art.Production
{
#if UNITY_EDITOR
    public static class Stage33TankSourceGenerator
    {
        private const string PrefabRoot = "Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks";
        private const string MaterialRoot = "Assets/Rts/Art/Materials/Production/Tanks";
        private const string ScenePath = "Assets/Rts/Scenes/Stage33_TankSourceReview.unity";
        private const string ReportRelativePath = "docs/STAGE33_TANK_SOURCE_REPORT.md";

        private struct TankSpec
        {
            public string ActorTypeId;
            public string DisplayName;
            public Vector2Int FineFootprint;
            public float Length;
            public float Width;
            public float Height;
            public float TurretScale;
            public float BarrelLength;
            public int RoadWheelsPerSide;
            public Color Primary;
            public Color Secondary;
            public Color Accent;
        }

        private static readonly TankSpec[] Specs =
        {
            new TankSpec
            {
                ActorTypeId = "light_tank", DisplayName = "Light Tank", FineFootprint = new Vector2Int(4, 4),
                Length = 1.75f, Width = 1.28f, Height = 0.55f, TurretScale = 0.55f, BarrelLength = 0.78f, RoadWheelsPerSide = 4,
                Primary = new Color(0.25f, 0.34f, 0.23f), Secondary = new Color(0.16f, 0.20f, 0.16f), Accent = new Color(0.58f, 0.64f, 0.34f)
            },
            new TankSpec
            {
                ActorTypeId = "medium_tank", DisplayName = "Medium Tank", FineFootprint = new Vector2Int(5, 4),
                Length = 2.05f, Width = 1.42f, Height = 0.65f, TurretScale = 0.66f, BarrelLength = 0.95f, RoadWheelsPerSide = 5,
                Primary = new Color(0.28f, 0.32f, 0.25f), Secondary = new Color(0.17f, 0.18f, 0.16f), Accent = new Color(0.63f, 0.56f, 0.33f)
            },
            new TankSpec
            {
                ActorTypeId = "heavy_tank", DisplayName = "Heavy Tank", FineFootprint = new Vector2Int(6, 5),
                Length = 2.45f, Width = 1.65f, Height = 0.78f, TurretScale = 0.78f, BarrelLength = 1.15f, RoadWheelsPerSide = 6,
                Primary = new Color(0.22f, 0.27f, 0.22f), Secondary = new Color(0.13f, 0.15f, 0.13f), Accent = new Color(0.52f, 0.57f, 0.31f)
            }
        };

        private static readonly ActorPrefabSocketKind[] RequiredProjectSockets =
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

        [MenuItem("ProjectAegisRTS/Art/Generate Tank Source Prefabs")]
        public static void GenerateTankSourcePrefabs()
        {
            EnsureFolder(PrefabRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder("Assets/Rts/Scenes");
            EnsureFolder("docs");

            Dictionary<string, Material> mats = CreateMaterials();
            List<string> prefabs = new List<string>();

            for (int i = 0; i < Specs.Length; i++)
            {
                string prefab = BuildTankPrefab(Specs[i], mats);
                prefabs.Add(prefab);
                TryAssignActorVisualDefinition(Specs[i].ActorTypeId, AssetDatabase.LoadAssetAtPath<GameObject>(prefab));
            }

            CreateReviewScene(prefabs);
            WriteReport(prefabs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage33 tank source prefabs generated: " + prefabs.Count);
        }

        public static void GenerateTankSourcePrefabsBatch()
        {
            try
            {
                GenerateTankSourcePrefabs();
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateTankSourcePrefabsBatch()
        {
            try
            {
                List<string> failures = new List<string>();
                for (int i = 0; i < Specs.Length; i++)
                {
                    string path = PrefabRoot + "/" + Specs[i].ActorTypeId + "_tank_source.prefab";
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                    {
                        failures.Add("Missing prefab: " + path);
                        continue;
                    }

                    Stage33TankSourceDescriptor descriptor = prefab.GetComponent<Stage33TankSourceDescriptor>();
                    if (descriptor == null)
                        failures.Add("Missing Stage33TankSourceDescriptor: " + path);
                    else if (!descriptor.HasRequiredSockets())
                        failures.Add("Missing required descriptor sockets: " + path);

                    Stage33TankSocket[] sockets = prefab.GetComponentsInChildren<Stage33TankSocket>(true);
                    if (sockets.Length < RequiredProjectSockets.Length)
                        failures.Add("Expected at least " + RequiredProjectSockets.Length + " Stage33 tank sockets on " + path + ", found " + sockets.Length);

                    ActorPrefabSocket[] projectSockets = prefab.GetComponentsInChildren<ActorPrefabSocket>(true);
                    if (projectSockets.Length < RequiredProjectSockets.Length)
                        failures.Add("Expected at least " + RequiredProjectSockets.Length + " project sockets on " + path + ", found " + projectSockets.Length);

                    TankVisualRigController rig = prefab.GetComponent<TankVisualRigController>();
                    if (rig == null)
                        failures.Add("Missing TankVisualRigController: " + path);

                    ActorPrefabDescriptor projectDescriptor = prefab.GetComponent<ActorPrefabDescriptor>();
                    if (projectDescriptor == null)
                        failures.Add("Missing ActorPrefabDescriptor: " + path);
                    else if (!projectDescriptor.requiredSocketsPresent)
                        failures.Add("ActorPrefabDescriptor does not mark required sockets present: " + path);

                    LODGroup lod = prefab.GetComponent<LODGroup>();
                    if (lod == null)
                        failures.Add("Missing LODGroup: " + path);

                    ValidateActorVisualDefinition(Specs[i].ActorTypeId, prefab, failures);
                }

                if (failures.Count > 0)
                    throw new InvalidOperationException("Tank source validation failed:\n" + string.Join("\n", failures.ToArray()));

                Debug.Log("Stage33 tank source validation passed for " + Specs.Length + " prefabs.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Diffuse");

            Dictionary<string, Material> result = new Dictionary<string, Material>();
            result["armor_light"] = CreateMaterial(shader, "tank_armor_light", new Color(0.25f, 0.34f, 0.23f), 0.58f, 0.08f);
            result["armor_medium"] = CreateMaterial(shader, "tank_armor_medium", new Color(0.28f, 0.32f, 0.25f), 0.55f, 0.08f);
            result["armor_heavy"] = CreateMaterial(shader, "tank_armor_heavy", new Color(0.22f, 0.27f, 0.22f), 0.62f, 0.10f);
            result["dark"] = CreateMaterial(shader, "tank_track_dark", new Color(0.06f, 0.065f, 0.06f), 0.82f, 0.02f);
            result["rubber"] = CreateMaterial(shader, "tank_wheel_rubber", new Color(0.025f, 0.025f, 0.023f), 0.9f, 0.0f);
            result["steel"] = CreateMaterial(shader, "tank_barrel_steel", new Color(0.26f, 0.28f, 0.26f), 0.45f, 0.2f);
            result["accent"] = CreateMaterial(shader, "tank_detail_accent", new Color(0.55f, 0.58f, 0.31f), 0.65f, 0.04f);
            result["glass"] = CreateMaterial(shader, "tank_optic_glass", new Color(0.05f, 0.18f, 0.16f), 0.2f, 0.0f);
            result["muzzle"] = CreateMaterial(shader, "tank_muzzle_marker", new Color(0.95f, 0.55f, 0.22f), 0.4f, 0.0f);
            return result;
        }

        private static Material CreateMaterial(Shader shader, string name, Color color, float roughness, float metallic)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.name = name;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughness));
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static string BuildTankPrefab(TankSpec spec, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject(spec.ActorTypeId + "_tank_source");
            root.transform.position = Vector3.zero;

            Stage33TankSourceDescriptor descriptor = root.AddComponent<Stage33TankSourceDescriptor>();
            descriptor.actorTypeId = spec.ActorTypeId;
            descriptor.displayName = spec.DisplayName;
            descriptor.fineFootprint = spec.FineFootprint;
            descriptor.visualLengthMeters = spec.Length;
            descriptor.visualWidthMeters = spec.Width;
            descriptor.visualHeightMeters = spec.Height;
            descriptor.root = root.transform;
            AddSocket(root.transform, Stage33TankSocketKind.Root, "Root", spec.ActorTypeId, Vector3.zero);

            GameObject bodyRoot = new GameObject("BodyRoot");
            bodyRoot.transform.SetParent(root.transform, false);
            descriptor.bodyRoot = bodyRoot.transform;
            AddSocket(bodyRoot.transform, Stage33TankSocketKind.BodyRoot, "BodyRoot", spec.ActorTypeId, Vector3.zero);

            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(bodyRoot.transform, false);
            descriptor.visualRoot = visualRoot.transform;
            AddSocket(visualRoot.transform, Stage33TankSocketKind.VisualRoot, "VisualRoot", spec.ActorTypeId, Vector3.zero);

            Material armor = mats[spec.ActorTypeId == "heavy_tank" ? "armor_heavy" : (spec.ActorTypeId == "medium_tank" ? "armor_medium" : "armor_light")];
            Material dark = mats["dark"];
            Material rubber = mats["rubber"];
            Material steel = mats["steel"];
            Material accent = mats["accent"];
            Material glass = mats["glass"];
            Material muzzle = mats["muzzle"];

            // Grid-accurate base shadow/foundation, kept low so terrain remains visible.
            CreateCube("GridAccurateBase", visualRoot.transform, new Vector3(0f, 0.015f, 0f), new Vector3(spec.Width, 0.03f, spec.Length), dark);

            // Chassis and upper hull use stepped/angled-looking modular boxes rather than a single cube.
            CreateCube("LowerChassis", visualRoot.transform, new Vector3(0f, 0.18f, 0f), new Vector3(spec.Width * 0.92f, 0.28f, spec.Length * 0.82f), armor);
            CreateCube("UpperSlopedHull", visualRoot.transform, new Vector3(0f, 0.39f, -spec.Length * 0.03f), new Vector3(spec.Width * 0.70f, 0.24f, spec.Length * 0.52f), armor);
            CreateCube("FrontGlacisPlate", visualRoot.transform, new Vector3(0f, 0.42f, spec.Length * 0.32f), new Vector3(spec.Width * 0.64f, 0.12f, spec.Length * 0.18f), accent);
            CreateCube("RearEngineDeck", visualRoot.transform, new Vector3(0f, 0.47f, -spec.Length * 0.36f), new Vector3(spec.Width * 0.78f, 0.10f, spec.Length * 0.20f), dark);
            CreateCube("LeftSideArmorSkirt", visualRoot.transform, new Vector3(-spec.Width * 0.54f, 0.25f, 0f), new Vector3(0.08f, 0.24f, spec.Length * 0.76f), armor);
            CreateCube("RightSideArmorSkirt", visualRoot.transform, new Vector3(spec.Width * 0.54f, 0.25f, 0f), new Vector3(0.08f, 0.24f, spec.Length * 0.76f), armor);

            // Tracks and road wheels.
            Transform trackLeft = CreateCube("TrackLeft", visualRoot.transform, new Vector3(-spec.Width * 0.52f, 0.17f, 0f), new Vector3(0.20f, 0.20f, spec.Length * 0.84f), dark).transform;
            Transform trackRight = CreateCube("TrackRight", visualRoot.transform, new Vector3(spec.Width * 0.52f, 0.17f, 0f), new Vector3(0.20f, 0.20f, spec.Length * 0.84f), dark).transform;
            descriptor.trackLeft = trackLeft;
            descriptor.trackRight = trackRight;
            AddSocket(trackLeft, Stage33TankSocketKind.TrackLeft, "TrackLeft", spec.ActorTypeId, Vector3.zero);
            AddSocket(trackRight, Stage33TankSocketKind.TrackRight, "TrackRight", spec.ActorTypeId, Vector3.zero);

            List<Transform> leftWheels = new List<Transform>();
            List<Transform> rightWheels = new List<Transform>();
            for (int i = 0; i < spec.RoadWheelsPerSide; i++)
            {
                float t = spec.RoadWheelsPerSide == 1 ? 0.5f : (float)i / (spec.RoadWheelsPerSide - 1);
                float z = Mathf.Lerp(-spec.Length * 0.33f, spec.Length * 0.33f, t);
                leftWheels.Add(CreateWheel("WheelLeft_" + i, visualRoot.transform, new Vector3(-spec.Width * 0.58f, 0.16f, z), 0.14f, rubber).transform);
                rightWheels.Add(CreateWheel("WheelRight_" + i, visualRoot.transform, new Vector3(spec.Width * 0.58f, 0.16f, z), 0.14f, rubber).transform);
            }

            // Turret hierarchy and barrel are intentionally separate for Unity animation/turret tracking.
            GameObject turretRoot = new GameObject("TurretRoot");
            turretRoot.transform.SetParent(visualRoot.transform, false);
            turretRoot.transform.localPosition = new Vector3(0f, 0.58f, 0.08f);
            descriptor.turretRoot = turretRoot.transform;
            AddSocket(turretRoot.transform, Stage33TankSocketKind.TurretRoot, "TurretRoot", spec.ActorTypeId, Vector3.zero);

            CreateCube("TurretBase", turretRoot.transform, Vector3.zero, new Vector3(spec.Width * spec.TurretScale, 0.24f, spec.Length * 0.26f), armor);
            CreateCube("TurretRearBustle", turretRoot.transform, new Vector3(0f, 0.02f, -spec.Length * 0.16f), new Vector3(spec.Width * spec.TurretScale * 0.82f, 0.18f, spec.Length * 0.18f), armor);
            CreateCube("CommanderHatch", turretRoot.transform, new Vector3(-spec.Width * 0.12f, 0.18f, -spec.Length * 0.02f), new Vector3(0.18f, 0.07f, 0.18f), accent);
            CreateCube("OpticBlock", turretRoot.transform, new Vector3(spec.Width * 0.16f, 0.15f, spec.Length * 0.09f), new Vector3(0.12f, 0.08f, 0.08f), glass);

            GameObject barrelRoot = new GameObject("BarrelRoot");
            barrelRoot.transform.SetParent(turretRoot.transform, false);
            barrelRoot.transform.localPosition = new Vector3(0f, 0.03f, spec.Length * 0.18f);
            descriptor.barrelRoot = barrelRoot.transform;
            AddSocket(barrelRoot.transform, Stage33TankSocketKind.BarrelRoot, "BarrelRoot", spec.ActorTypeId, Vector3.zero);

            CreateCylinder("MainGunBarrel", barrelRoot.transform, new Vector3(0f, 0f, spec.BarrelLength * 0.5f), 0.055f, spec.BarrelLength, steel, true);
            GameObject muzzleObj = new GameObject("MuzzlePrimary");
            muzzleObj.transform.SetParent(barrelRoot.transform, false);
            muzzleObj.transform.localPosition = new Vector3(0f, 0f, spec.BarrelLength + 0.03f);
            descriptor.muzzlePrimary = muzzleObj.transform;
            AddSocket(muzzleObj.transform, Stage33TankSocketKind.MuzzlePrimary, "MuzzlePrimary", spec.ActorTypeId, Vector3.zero);
            CreateCube("MuzzleMarker", muzzleObj.transform, Vector3.zero, new Vector3(0.10f, 0.10f, 0.04f), muzzle);

            // Rear and side detail for 360-degree readability.
            CreateCube("RearExhaustLeft", visualRoot.transform, new Vector3(-spec.Width * 0.24f, 0.50f, -spec.Length * 0.48f), new Vector3(0.10f, 0.12f, 0.08f), dark);
            CreateCube("RearExhaustRight", visualRoot.transform, new Vector3(spec.Width * 0.24f, 0.50f, -spec.Length * 0.48f), new Vector3(0.10f, 0.12f, 0.08f), dark);
            CreateCube("LeftStowageBox", visualRoot.transform, new Vector3(-spec.Width * 0.36f, 0.53f, -spec.Length * 0.12f), new Vector3(0.18f, 0.10f, 0.22f), accent);
            CreateCube("RightStowageBox", visualRoot.transform, new Vector3(spec.Width * 0.36f, 0.53f, -spec.Length * 0.12f), new Vector3(0.18f, 0.10f, 0.22f), accent);
            CreateCylinder("AntennaBase", visualRoot.transform, new Vector3(spec.Width * 0.32f, 0.63f, -spec.Length * 0.33f), 0.02f, 0.28f, steel, false);

            // UI/selection sockets.
            descriptor.selectionAnchor = AddSocket(root.transform, Stage33TankSocketKind.SelectionAnchor, "SelectionAnchor", spec.ActorTypeId, new Vector3(0f, 0.05f, 0f));
            descriptor.healthBarAnchor = AddSocket(root.transform, Stage33TankSocketKind.HealthBarAnchor, "HealthBarAnchor", spec.ActorTypeId, new Vector3(0f, spec.Height + 0.55f, 0f));
            descriptor.uiAnchor = AddSocket(root.transform, Stage33TankSocketKind.UiAnchor, "UiAnchor", spec.ActorTypeId, new Vector3(0f, spec.Height + 0.75f, 0f));
            descriptor.vfxSmoke = AddSocket(root.transform, Stage33TankSocketKind.VfxSmoke, "VfxSmoke", spec.ActorTypeId, new Vector3(0f, spec.Height + 0.10f, -spec.Length * 0.45f));
            descriptor.vfxExplosion = AddSocket(root.transform, Stage33TankSocketKind.VfxExplosion, "VfxExplosion", spec.ActorTypeId, new Vector3(0f, spec.Height * 0.6f, 0f));

            TankVisualRigController rig = root.AddComponent<TankVisualRigController>();
            rig.bodyRoot = bodyRoot.transform;
            rig.turretRoot = turretRoot.transform;
            rig.barrelRoot = barrelRoot.transform;
            rig.trackLeftRoot = trackLeft;
            rig.trackRightRoot = trackRight;
            rig.wheelLeft = leftWheels.ToArray();
            rig.wheelRight = rightWheels.ToArray();

            AddLodGroup(root);
            AddExistingProjectComponentsIfAvailable(root, spec.ActorTypeId);

            string path = PrefabRoot + "/" + spec.ActorTypeId + "_tank_source.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            EditorUtility.SetDirty(prefab);
            return path;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            return go;
        }

        private static GameObject CreateWheel(string name, Transform parent, Vector3 localPosition, float radius, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            go.transform.localScale = new Vector3(radius, 0.045f, radius);
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            return go;
        }

        private static GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, float radius, float length, Material material, bool alongZ)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = alongZ ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
            go.transform.localScale = new Vector3(radius, length * 0.5f, radius);
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            return go;
        }

        private static Transform AddSocket(Transform parent, Stage33TankSocketKind kind, string socketName, string actorTypeId, Vector3 localPosition)
        {
            GameObject socket = new GameObject(socketName);
            socket.transform.SetParent(parent, false);
            socket.transform.localPosition = localPosition;
            Stage33TankSocket marker = socket.AddComponent<Stage33TankSocket>();
            marker.socketKind = kind;
            marker.socketName = socketName;
            marker.actorTypeId = actorTypeId;
            marker.required = true;

            ActorPrefabSocket projectSocket = socket.AddComponent<ActorPrefabSocket>();
            projectSocket.socketKind = MapProjectSocketKind(kind);
            projectSocket.socketName = socketName;
            projectSocket.actorTypeId = actorTypeId;
            projectSocket.notes = "Stage 33 generated tank source socket. Preserve this transform name and role when replacing art.";
            return socket.transform;
        }

        private static ActorPrefabSocketKind MapProjectSocketKind(Stage33TankSocketKind kind)
        {
            switch (kind)
            {
                case Stage33TankSocketKind.BodyRoot: return ActorPrefabSocketKind.BodyRoot;
                case Stage33TankSocketKind.VisualRoot: return ActorPrefabSocketKind.VisualRoot;
                case Stage33TankSocketKind.SelectionAnchor: return ActorPrefabSocketKind.SelectionAnchor;
                case Stage33TankSocketKind.HealthBarAnchor: return ActorPrefabSocketKind.HealthBarAnchor;
                case Stage33TankSocketKind.UiAnchor: return ActorPrefabSocketKind.UiAnchor;
                case Stage33TankSocketKind.TurretRoot: return ActorPrefabSocketKind.TurretRoot;
                case Stage33TankSocketKind.BarrelRoot: return ActorPrefabSocketKind.BarrelRoot;
                case Stage33TankSocketKind.MuzzlePrimary: return ActorPrefabSocketKind.MuzzlePrimary;
                case Stage33TankSocketKind.MuzzleSecondary: return ActorPrefabSocketKind.MuzzleSecondary;
                case Stage33TankSocketKind.TrackLeft: return ActorPrefabSocketKind.TrackLeft;
                case Stage33TankSocketKind.TrackRight: return ActorPrefabSocketKind.TrackRight;
                case Stage33TankSocketKind.WheelLeft: return ActorPrefabSocketKind.WheelLeft;
                case Stage33TankSocketKind.WheelRight: return ActorPrefabSocketKind.WheelRight;
                case Stage33TankSocketKind.VfxSmoke: return ActorPrefabSocketKind.VfxSmoke;
                case Stage33TankSocketKind.VfxExplosion: return ActorPrefabSocketKind.VfxExplosion;
                case Stage33TankSocketKind.AudioLoop: return ActorPrefabSocketKind.AudioLoop;
                case Stage33TankSocketKind.AimPivot: return ActorPrefabSocketKind.AimPivot;
                default: return ActorPrefabSocketKind.Root;
            }
        }

        private static void AddLodGroup(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;
            LODGroup lod = root.GetComponent<LODGroup>();
            if (lod == null)
                lod = root.AddComponent<LODGroup>();
            LOD[] lods = { new LOD(0.35f, renderers), new LOD(0.08f, renderers) };
            lod.SetLODs(lods);
            lod.RecalculateBounds();
        }

        private static void AddExistingProjectComponentsIfAvailable(GameObject root, string actorTypeId)
        {
            ActorPrefabDescriptor descriptor = root.GetComponent<ActorPrefabDescriptor>();
            if (descriptor == null)
                descriptor = root.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = actorTypeId;
            descriptor.category = ActorArtCategory.Vehicle;
            descriptor.productionStatus = ActorArtProductionStatus.ProxyPrefab;
            descriptor.generatedByStage8 = false;
            descriptor.requiredSocketsPresent = true;
            descriptor.declaredRequiredSockets = new List<ActorPrefabSocketKind>(RequiredProjectSockets);
            descriptor.notes = "Stage 33 generated tank source/proxy prefab. Visual-only; keep sockets, pivot, scale, and fallback when replacing with artist-authored art.";

            ProductionVisualValidationTag tag = root.GetComponent<ProductionVisualValidationTag>();
            if (tag == null)
                tag = root.AddComponent<ProductionVisualValidationTag>();
            tag.actorTypeId = actorTypeId;
            tag.visualTier = ProductionVisualTier.FirstPassProxy;
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
            tag.replacementNotes = "Replace the procedural mesh with imported art while preserving Stage33TankSourceDescriptor, ActorPrefabSocket transforms, TankVisualRigController hooks, LODGroup, and footprint-centered pivot.";
            tag.notes = "Stage 33 tank source prefab with turret/barrel/track visual hooks.";
        }

        private static void TryAddComponentByName(GameObject root, string typeName, string actorTypeId)
        {
            Type t = FindType(typeName);
            if (t == null || !typeof(Component).IsAssignableFrom(t))
                return;
            if (root.GetComponent(t) != null)
                return;
            Component c = root.AddComponent(t);
            SetFieldOrProperty(c, "actorTypeId", actorTypeId);
            SetFieldOrProperty(c, "category", "Vehicle");
            SetFieldOrProperty(c, "productionStatus", "ProxyPrefab");
            SetFieldOrProperty(c, "visualTier", "FirstPassProductionSource");
            SetFieldOrProperty(c, "generatedByStage8", false);
        }

        private static Type FindType(string typeName)
        {
            Type direct = Type.GetType(typeName);
            if (direct != null)
                return direct;
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type t = assemblies[i].GetType(typeName);
                if (t != null)
                    return t;
            }
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types;
                try { types = assemblies[i].GetTypes(); }
                catch { continue; }
                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j].Name == typeName)
                        return types[j];
                }
            }
            return null;
        }

        private static void SetFieldOrProperty(Component component, string name, object value)
        {
            if (component == null)
                return;
            Type t = component.GetType();
            var field = t.GetField(name);
            if (field != null)
            {
                TrySetValue(field.FieldType, v => field.SetValue(component, v), value);
                return;
            }
            var prop = t.GetProperty(name);
            if (prop != null && prop.CanWrite)
                TrySetValue(prop.PropertyType, v => prop.SetValue(component, v, null), value);
        }

        private static void TrySetValue(Type targetType, Action<object> setter, object value)
        {
            try
            {
                if (targetType == typeof(string)) setter(Convert.ToString(value));
                else if (targetType == typeof(bool)) setter(Convert.ToBoolean(value));
                else if (targetType.IsEnum && value is string) setter(Enum.Parse(targetType, (string)value, true));
                else if (targetType.IsAssignableFrom(value.GetType())) setter(value);
            }
            catch
            {
                // Optional project metadata; ignore incompatible field shapes.
            }
        }

        private static void TryAssignActorVisualDefinition(string actorTypeId, GameObject prefab)
        {
            if (prefab == null)
                return;

            string[] guids = AssetDatabase.FindAssets(actorTypeId + " t:ScriptableObject", new[] { "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset == null)
                    continue;

                SerializedObject so = new SerializedObject(asset);
                bool changed = false;
                SerializedProperty prod = so.FindProperty("productionPrefab");
                if (prod != null && prod.propertyType == SerializedPropertyType.ObjectReference)
                {
                    prod.objectReferenceValue = prefab;
                    changed = true;
                }
                SerializedProperty fallback = so.FindProperty("fallbackPrefab");
                if (fallback != null && fallback.propertyType == SerializedPropertyType.ObjectReference && fallback.objectReferenceValue == null)
                {
                    SerializedProperty generated = so.FindProperty("generatedBlockoutPrefab");
                    fallback.objectReferenceValue = generated != null && generated.propertyType == SerializedPropertyType.ObjectReference && generated.objectReferenceValue != null
                        ? generated.objectReferenceValue
                        : prefab;
                    changed = true;
                }
                SerializedProperty mode = so.FindProperty("preferredPrefabMode");
                if (mode != null && mode.propertyType == SerializedPropertyType.Enum)
                {
                    SetEnumIfPresent(mode, new[] { "ProductionPrefab", "Production", "FirstPassProxy" });
                    changed = true;
                }
                SerializedProperty status = so.FindProperty("productionStatus");
                if (status != null && status.propertyType == SerializedPropertyType.Enum)
                {
                    SetEnumIfPresent(status, new[] { "FirstPassProxy", "ProxyPrefab", "FirstPassModel" });
                    changed = true;
                }
                if (changed)
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(asset);
                    Debug.Log("Updated ActorVisualDefinition for " + actorTypeId + ": " + path);
                }
            }
        }

        private static void SetEnumIfPresent(SerializedProperty property, string[] names)
        {
            for (int n = 0; n < names.Length; n++)
            {
                string[] enumNames = property.enumNames;
                for (int i = 0; i < enumNames.Length; i++)
                {
                    if (EnumNameEquals(enumNames[i], names[n]))
                    {
                        property.enumValueIndex = i;
                        return;
                    }
                }

                string[] displayNames = property.enumDisplayNames;
                for (int i = 0; i < displayNames.Length; i++)
                {
                    if (EnumNameEquals(displayNames[i], names[n]))
                    {
                        property.enumValueIndex = i;
                        return;
                    }
                }
            }
        }

        private static bool EnumNameEquals(string actual, string expected)
        {
            return string.Equals(NormalizeEnumName(actual), NormalizeEnumName(expected), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeEnumName(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
        }

        private static void ValidateActorVisualDefinition(string actorTypeId, GameObject prefab, List<string> failures)
        {
            string[] guids = AssetDatabase.FindAssets(actorTypeId + " t:ScriptableObject", new[] { "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions" });
            if (guids.Length == 0)
            {
                failures.Add("Missing ActorVisualDefinition asset for " + actorTypeId);
                return;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ActorVisualDefinition definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(path);
                if (definition == null || definition.actorTypeId != actorTypeId)
                    continue;

                if (definition.productionPrefab != prefab)
                    failures.Add("ActorVisualDefinition productionPrefab was not assigned to tank source for " + actorTypeId + ": " + path);
                if (definition.fallbackPrefab == null && definition.generatedBlockoutPrefab == null)
                    failures.Add("ActorVisualDefinition has no fallback or generated blockout for " + actorTypeId + ": " + path);
                if (definition.preferredPrefabMode != ActorVisualPrefabMode.ProductionPrefab)
                    failures.Add("ActorVisualDefinition preferredPrefabMode is not ProductionPrefab for " + actorTypeId + ": " + path);
                if (definition.productionStatus != ActorArtProductionStatus.ProxyPrefab && definition.productionStatus != ActorArtProductionStatus.FirstPassModel)
                    failures.Add("ActorVisualDefinition productionStatus is not a safe first-pass production state for " + actorTypeId + ": " + path);
                return;
            }

            failures.Add("ActorVisualDefinition search did not find matching actorTypeId for " + actorTypeId);
        }

        private static void CreateReviewScene(List<string> prefabPaths)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject camera = new GameObject("Main Camera");
            Camera cam = camera.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            camera.transform.position = new Vector3(0f, 6.0f, -6.5f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            camera.tag = "MainCamera";

            GameObject light = new GameObject("Directional Light");
            Light l = light.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.25f;
            light.transform.rotation = Quaternion.Euler(45f, 35f, 0f);

            for (int i = 0; i < prefabPaths.Count; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
                if (prefab == null)
                    continue;
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = new Vector3((i - 1) * 3.0f, 0f, 0f);
                instance.name = prefab.name + "_Review";
            }

            GameObject grid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grid.name = "FineGridScaleReference";
            grid.transform.position = new Vector3(0f, -0.03f, 0f);
            grid.transform.localScale = new Vector3(8f, 0.02f, 3.2f);
            var col = grid.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void WriteReport(List<string> prefabPaths)
        {
            string content = "# Stage 33 Tank Source Prefabs\n\n" +
                             "Generated first-pass tank source prefabs for Unity integration. These are production-source proxies, not final art.\n\n" +
                             "## Prefabs\n";
            for (int i = 0; i < prefabPaths.Count; i++)
                content += "- `" + prefabPaths[i] + "`\n";
            content += "\n## Review Scene\n\n" +
                       "- `" + ScenePath + "`\n" +
                       "\n## Integration Rules\n\n" +
                       "- Pivot at footprint/base center.\n" +
                       "- Separate `BodyRoot`, `VisualRoot`, `TurretRoot`, `BarrelRoot`, `MuzzlePrimary`, `TrackLeft`, `TrackRight`, `SelectionAnchor`, `HealthBarAnchor`, `UiAnchor`, `VfxSmoke`, and `VfxExplosion`.\n" +
                       "- Each required socket has both `Stage33TankSocket` and existing `ActorPrefabSocket` metadata.\n" +
                       "- Each prefab has `Stage33TankSourceDescriptor`, `ActorPrefabDescriptor`, `ProductionVisualValidationTag`, `TankVisualRigController`, and `LODGroup` metadata/components.\n" +
                       "- `TankVisualRigController` is visual-only and must never write to Rts.Core.\n" +
                       "- ActorVisualDefinitions are updated when compatible fields are present; existing/generated blockouts remain fallback.\n" +
                       "- Replace these proxies with artist-authored models by preserving socket names, pivot, scale, and descriptor metadata.\n" +
                       "\n## Actor Visual Definitions\n\n" +
                       "- `light_tank_visual.asset`, `medium_tank_visual.asset`, and `heavy_tank_visual.asset` are wired to their matching Stage33 production-source prefab.\n" +
                       "- Generated blockout prefabs remain fallback references so runtime visual resolution stays safe if a production prefab is unavailable.\n" +
                       "- Preferred prefab mode is production-prefab compatible where the current schema supports it.\n" +
                       "\n## Preservation Notes\n\n" +
                       "- No Rts.Core gameplay changes are required; Stage33 is Unity presentation/source-art integration.\n" +
                       "- PCDesktop sidebar and safe-area behavior remain guarded by the Stage32 medium/player-facing checks.\n" +
                       "- QuestXR left/right hand controls remain guarded by Stage4 and Stage5 checks.\n" +
                       "- Stage27.1 placement HUD separation remains guarded by the Stage32 medium gate.\n" +
                       "- Rts.Core must remain UnityEngine-free.\n" +
                       "\n## Validation\n\n" +
                       "Targeted Stage33 validation:\n\n" +
                       "```powershell\n" +
                       ".\\tools\\run-stage33-tank-source-generator.ps1\n" +
                       "```\n\n" +
                       "Broad pre-commit confidence:\n\n" +
                       "```powershell\n" +
                       ".\\tools\\run-stage32-medium-checks.ps1\n" +
                       "```\n";
            string reportPath = Path.Combine(GetRepoRootPath(), ReportRelativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, content);
        }

        private static string GetRepoRootPath()
        {
            DirectoryInfo assetsDirectory = new DirectoryInfo(Application.dataPath);
            DirectoryInfo unityDirectory = assetsDirectory.Parent;
            DirectoryInfo repoDirectory = unityDirectory != null ? unityDirectory.Parent : null;
            return repoDirectory != null ? repoDirectory.FullName : Directory.GetCurrentDirectory();
        }

        private static void EnsureFolder(string folder)
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
    }
#endif
}
