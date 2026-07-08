using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8BlockoutPrefabGenerator
    {
        [MenuItem("ProjectAegisRTS/Stage 8/Generate Blockout Prefabs")]
        public static void GenerateBlockoutsMenu()
        {
            GenerateBlockouts();
        }

        public static void GenerateBlockoutsBatch()
        {
            try
            {
                GenerateBlockouts();
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

        public static List<GameObject> GenerateBlockouts()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            var specs = Stage8ActorCatalog.LoadSpecs();
            var prefabs = new List<GameObject>();
            var materials = MaterialSet.Create();

            for (var i = 0; i < specs.Count; i++)
            {
                var prefab = GenerateBlockout(specs[i], materials);
                if (prefab != null)
                    prefabs.Add(prefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 8 generated blockout prefabs updated: " + prefabs.Count);
            return prefabs;
        }

        static GameObject GenerateBlockout(Stage8ActorSpec spec, MaterialSet materials)
        {
            var root = new GameObject(spec.ActorTypeId + "_blockout");
            root.transform.position = Vector3.zero;
            var descriptor = root.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = spec.ActorTypeId;
            descriptor.category = spec.Category;
            descriptor.productionStatus = spec.IpReviewRequired ? ActorArtProductionStatus.NeedsIPReview : ActorArtProductionStatus.BlockoutGenerated;
            descriptor.generatedByStage8 = true;
            descriptor.declaredRequiredSockets = Stage8ActorCatalog.RequiredSocketsFor(spec);
            descriptor.notes = "Generated Stage 8 blockout. Replace geometry under sockets with production art.";

            var sockets = CreateSockets(root.transform, spec, descriptor.declaredRequiredSockets);
            CreateGeometry(root.transform, spec, sockets, materials);
            AddCompatibleControllers(root, spec);

            descriptor.requiredSocketsPresent = descriptor.ValidateRequiredSockets(descriptor.declaredRequiredSockets).Count == 0;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Stage8ActorCatalog.BlockoutPrefabPath(spec));
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
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
                socket.notes = "Stage 8 required socket";
                sockets[kind] = transform;
            }

            return sockets;
        }

        static void CreateGeometry(Transform root, Stage8ActorSpec spec, Dictionary<ActorPrefabSocketKind, Transform> sockets, MaterialSet materials)
        {
            var visualRoot = GetSocket(sockets, ActorPrefabSocketKind.VisualRoot, root);
            if (spec.Category == ActorArtCategory.Infantry)
            {
                CreatePrimitive("Body", PrimitiveType.Capsule, visualRoot, new Vector3(0f, 0.35f, 0f), new Vector3(0.34f, 0.55f, 0.34f), materials.Infantry);
                CreatePrimitive("Head", PrimitiveType.Sphere, GetSocket(sockets, ActorPrefabSocketKind.Head, visualRoot), Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), materials.Detail);
                CreatePrimitive("Weapon", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.WeaponSocket, visualRoot), new Vector3(0f, 0f, 0.18f), new Vector3(0.12f, 0.08f, 0.42f), materials.Weapon);
                return;
            }

            if (spec.Category == ActorArtCategory.Aircraft)
            {
                CreatePrimitive("Fuselage", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.35f, 0f), new Vector3(1.1f, 0.22f, 0.68f), materials.Aircraft);
                CreatePrimitive("Wings", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.34f, 0f), new Vector3(1.8f, 0.08f, 0.22f), materials.Detail);
                CreatePrimitive("Rotor", PrimitiveType.Cylinder, GetSocket(sockets, ActorPrefabSocketKind.AircraftRotor, visualRoot), Vector3.zero, new Vector3(0.55f, 0.04f, 0.55f), materials.Weapon);
                if (sockets.ContainsKey(ActorPrefabSocketKind.MuzzlePrimary))
                    CreatePrimitive("Muzzle", PrimitiveType.Sphere, sockets[ActorPrefabSocketKind.MuzzlePrimary], Vector3.zero, new Vector3(0.08f, 0.08f, 0.08f), materials.Warning);
                return;
            }

            if (spec.Category == ActorArtCategory.Vehicle || spec.Category == ActorArtCategory.Resource)
            {
                var bodyScale = spec.ActorTypeId == "harvester" ? new Vector3(1.15f, 0.38f, 0.78f) : new Vector3(0.95f, 0.34f, 0.66f);
                CreatePrimitive("Chassis", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.28f, 0f), bodyScale, materials.Vehicle);
                if (sockets.ContainsKey(ActorPrefabSocketKind.TrackLeft))
                {
                    CreatePrimitive("Track Left", PrimitiveType.Cube, sockets[ActorPrefabSocketKind.TrackLeft], Vector3.zero, new Vector3(0.1f, 0.16f, 0.72f), materials.Track);
                    CreatePrimitive("Track Right", PrimitiveType.Cube, sockets[ActorPrefabSocketKind.TrackRight], Vector3.zero, new Vector3(0.1f, 0.16f, 0.72f), materials.Track);
                }
                if (sockets.ContainsKey(ActorPrefabSocketKind.WheelLeft))
                {
                    CreatePrimitive("Wheel Left", PrimitiveType.Cylinder, sockets[ActorPrefabSocketKind.WheelLeft], Vector3.zero, new Vector3(0.2f, 0.08f, 0.2f), materials.Track);
                    CreatePrimitive("Wheel Right", PrimitiveType.Cylinder, sockets[ActorPrefabSocketKind.WheelRight], Vector3.zero, new Vector3(0.2f, 0.08f, 0.2f), materials.Track);
                }
                if (sockets.ContainsKey(ActorPrefabSocketKind.TurretRoot))
                {
                    CreatePrimitive("Turret", PrimitiveType.Cube, sockets[ActorPrefabSocketKind.TurretRoot], Vector3.zero, new Vector3(0.38f, 0.16f, 0.38f), materials.Weapon);
                    CreatePrimitive("Barrel", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.BarrelRoot, visualRoot), new Vector3(0f, 0f, 0.28f), new Vector3(0.12f, 0.1f, 0.58f), materials.Weapon);
                }
                return;
            }

            if (spec.Category == ActorArtCategory.Defense)
            {
                CreatePrimitive("Defense Base", PrimitiveType.Cylinder, visualRoot, new Vector3(0f, 0.28f, 0f), new Vector3(0.76f, 0.56f, 0.76f), materials.Defense);
                CreatePrimitive("Turret", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.TurretRoot, visualRoot), Vector3.zero, new Vector3(0.42f, 0.2f, 0.42f), materials.Weapon);
                CreatePrimitive("Barrel", PrimitiveType.Cube, GetSocket(sockets, ActorPrefabSocketKind.BarrelRoot, visualRoot), new Vector3(0f, 0f, 0.32f), new Vector3(0.14f, 0.12f, 0.68f), materials.Weapon);
                return;
            }

            var height = spec.ActorTypeId == "fabrication_hub" ? 1.05f : 0.74f;
            var footprint = new Vector3(Mathf.Max(0.9f, spec.FootprintWidth * 0.82f), height, Mathf.Max(0.9f, spec.FootprintHeight * 0.82f));
            CreatePrimitive("Building Blockout", PrimitiveType.Cube, visualRoot, new Vector3(0f, height * 0.5f, 0f), footprint, materials.Building);
            CreatePrimitive("Roof Detail", PrimitiveType.Cube, visualRoot, new Vector3(0f, height + 0.08f, 0f), new Vector3(footprint.x * 0.6f, 0.14f, footprint.z * 0.45f), materials.Detail);
            if (sockets.ContainsKey(ActorPrefabSocketKind.DoorRoot))
                CreatePrimitive("Door", PrimitiveType.Cube, sockets[ActorPrefabSocketKind.DoorRoot], Vector3.zero, new Vector3(0.46f, 0.28f, 0.08f), materials.Weapon);
            if (sockets.ContainsKey(ActorPrefabSocketKind.TurbineRoot))
                CreatePrimitive("Turbine", PrimitiveType.Cylinder, sockets[ActorPrefabSocketKind.TurbineRoot], Vector3.zero, new Vector3(0.38f, 0.18f, 0.38f), materials.Power);
            if (sockets.ContainsKey(ActorPrefabSocketKind.RadarDishRoot))
                CreatePrimitive("Radar Dish", PrimitiveType.Cylinder, sockets[ActorPrefabSocketKind.RadarDishRoot], Vector3.zero, new Vector3(0.36f, 0.06f, 0.36f), materials.Power);
            if (sockets.ContainsKey(ActorPrefabSocketKind.CraneRoot))
                CreatePrimitive("Crane Arm", PrimitiveType.Cube, sockets[ActorPrefabSocketKind.CraneRoot], new Vector3(0.4f, 0f, 0f), new Vector3(0.8f, 0.08f, 0.08f), materials.Weapon);
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
            if (spec.Category == ActorArtCategory.Aircraft)
            {
                root.AddComponent<ActorVisualMotionController>();
                root.AddComponent<AircraftVisualMotionController>();
            }
            if (spec.ActorTypeId.Contains("tank") || spec.Category == ActorArtCategory.Defense || spec.ActorTypeId == "apc" || spec.ActorTypeId == "attack_aircraft")
            {
                if (root.GetComponent<ActorVisualMotionController>() == null)
                    root.AddComponent<ActorVisualMotionController>();
                root.AddComponent<TurretVisualAimController>();
            }
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
                case ActorPrefabSocketKind.HealthBarAnchor: return new Vector3(0f, 1.1f, -0.35f);
                case ActorPrefabSocketKind.UiAnchor: return new Vector3(0f, 1.25f, 0f);
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
                case ActorPrefabSocketKind.HarvesterDock: return new Vector3(-footprintX * 0.45f, 0.25f, 0f);
                case ActorPrefabSocketKind.RepairArmRoot: return new Vector3(footprintX * 0.3f, 0.85f, 0f);
                case ActorPrefabSocketKind.CraneRoot: return new Vector3(-footprintX * 0.25f, 1.05f, 0f);
                case ActorPrefabSocketKind.RadarDishRoot: return new Vector3(0f, 1.1f, 0f);
                case ActorPrefabSocketKind.TurbineRoot: return new Vector3(0f, 0.95f, 0f);
                case ActorPrefabSocketKind.LightRoot: return new Vector3(-footprintX * 0.25f, 0.95f, footprintZ * 0.2f);
                case ActorPrefabSocketKind.VfxSmoke: return new Vector3(-footprintX * 0.2f, 1.1f, -footprintZ * 0.2f);
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

        sealed class MaterialSet
        {
            public Material Building;
            public Material Defense;
            public Material Vehicle;
            public Material Infantry;
            public Material Aircraft;
            public Material Track;
            public Material Weapon;
            public Material Detail;
            public Material Power;
            public Material Warning;

            public static MaterialSet Create()
            {
                return new MaterialSet
                {
                    Building = CreateMaterial("stage8_building_blockout", new Color(0.45f, 0.55f, 0.58f)),
                    Defense = CreateMaterial("stage8_defense_blockout", new Color(0.62f, 0.34f, 0.32f)),
                    Vehicle = CreateMaterial("stage8_vehicle_blockout", new Color(0.56f, 0.48f, 0.36f)),
                    Infantry = CreateMaterial("stage8_infantry_blockout", new Color(0.33f, 0.62f, 0.42f)),
                    Aircraft = CreateMaterial("stage8_aircraft_blockout", new Color(0.36f, 0.55f, 0.72f)),
                    Track = CreateMaterial("stage8_track_blockout", new Color(0.14f, 0.16f, 0.17f)),
                    Weapon = CreateMaterial("stage8_weapon_socket", new Color(0.18f, 0.22f, 0.24f)),
                    Detail = CreateMaterial("stage8_detail_blockout", new Color(0.72f, 0.69f, 0.54f)),
                    Power = CreateMaterial("stage8_power_socket", new Color(0.22f, 0.68f, 0.72f)),
                    Warning = CreateMaterial("stage8_warning_socket", new Color(0.9f, 0.32f, 0.24f))
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
