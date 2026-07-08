using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public static class BuildingPlaceholderPartFactory
    {
        public sealed class PartSet
        {
            public GameObject Root;
            public readonly List<Transform> Lights = new List<Transform>();
            public Transform Machinery;
            public Transform Door;
            public Vector3 DoorOpenLocalDirection = Vector3.down;
            public Transform ProductionIndicator;
            public Transform Turbine;
            public readonly List<Transform> ExtraTurbines = new List<Transform>();
            public Transform RadarDish;
            public Transform CraneArm;
            public Transform RepairArmLeft;
            public Transform RepairArmRight;
            public Transform DockPump;
            public Transform TurretOrBarrel;
            public Transform WarningLight;
            public Transform DamageSmoke;
        }

        public static PartSet CreateParts(Transform parent, BuildingVisualProfile profile, Stage7BuildingMaterialLibrary materials)
        {
            var existing = parent.Find("Stage7 Building Visual Parts");
            if (existing != null)
                DestroyUnityObject(existing.gameObject);

            var set = new PartSet();
            set.Root = new GameObject("Stage7 Building Visual Parts");
            set.Root.transform.SetParent(parent, false);

            var width = Mathf.Max(1f, profile == null ? 2f : profile.footprintWidth);
            var height = Mathf.Max(1f, profile == null ? 2f : profile.footprintHeight);
            var top = profile != null && profile.actorTypeId == "fabrication_hub" ? 1.25f : 0.92f;

            if (profile == null || profile.hasLights)
            {
                var count = Mathf.Clamp(profile == null ? 2 : profile.lightCount, 1, 8);
                for (var i = 0; i < count; i++)
                {
                    var angle = (Mathf.PI * 2f * i) / count;
                    var local = new Vector3(Mathf.Cos(angle) * width * 0.3f, top + 0.12f, Mathf.Sin(angle) * height * 0.3f);
                    set.Lights.Add(CreatePrimitive(set.Root.transform, "Powered Light " + (i + 1), PrimitiveType.Sphere, local, Vector3.one * 0.14f, materials.PoweredLight).transform);
                }
            }

            if (profile == null || profile.hasMachinery)
                set.Machinery = CreatePrimitive(set.Root.transform, "Machinery Loop", PrimitiveType.Cylinder, new Vector3(0.24f, top + 0.05f, 0.18f), new Vector3(0.24f, 0.12f, 0.24f), materials.Machinery).transform;
            if (profile != null && profile.hasDoor)
                set.Door = CreatePrimitive(set.Root.transform, "Door Placeholder", PrimitiveType.Cube, new Vector3(0f, 0.18f, height * 0.45f), new Vector3(width * 0.34f, 0.22f, 0.08f), materials.Door).transform;
            if (profile != null && profile.hasProductionBay)
                set.ProductionIndicator = CreatePrimitive(set.Root.transform, "Production Activity Indicator", PrimitiveType.Cube, new Vector3(0f, top + 0.35f, 0f), Vector3.one * 0.23f, materials.ProductionIndicator).transform;
            if (profile != null && profile.hasTurbine)
                set.Turbine = CreatePrimitive(set.Root.transform, "Turbine Placeholder", PrimitiveType.Cylinder, new Vector3(0f, top + 0.2f, 0f), new Vector3(0.36f, 0.1f, 0.36f), materials.RadarTurbineCrane).transform;
            if (profile != null && profile.hasRadarDish)
                set.RadarDish = CreatePrimitive(set.Root.transform, "Radar Dish Placeholder", PrimitiveType.Cylinder, new Vector3(0f, top + 0.32f, 0f), new Vector3(0.44f, 0.05f, 0.44f), materials.RadarTurbineCrane).transform;
            if (profile != null && profile.hasCrane)
                set.CraneArm = CreatePrimitive(set.Root.transform, "Construction Crane Arm", PrimitiveType.Cube, new Vector3(0f, top + 0.42f, 0.05f), new Vector3(width * 0.72f, 0.07f, 0.08f), materials.RadarTurbineCrane).transform;
            if (profile != null && profile.hasRepairArms)
            {
                set.RepairArmLeft = CreatePrimitive(set.Root.transform, "Repair Arm Left", PrimitiveType.Cube, new Vector3(-width * 0.28f, top + 0.15f, 0f), new Vector3(0.08f, 0.08f, 0.56f), materials.RadarTurbineCrane).transform;
                set.RepairArmRight = CreatePrimitive(set.Root.transform, "Repair Arm Right", PrimitiveType.Cube, new Vector3(width * 0.28f, top + 0.15f, 0f), new Vector3(0.08f, 0.08f, 0.56f), materials.RadarTurbineCrane).transform;
            }
            if (profile != null && profile.hasDock)
                set.DockPump = CreatePrimitive(set.Root.transform, "Refinery Dock Pump", PrimitiveType.Cube, new Vector3(width * 0.32f, 0.32f, height * 0.35f), new Vector3(0.18f, 0.28f, 0.18f), materials.RadarTurbineCrane).transform;
            if (profile != null && profile.hasTurretOrBarrel)
                set.TurretOrBarrel = CreatePrimitive(set.Root.transform, "Building Turret Barrel", PrimitiveType.Cube, new Vector3(0f, top + 0.18f, 0.42f), new Vector3(0.14f, 0.12f, 0.78f), materials.Machinery).transform;
            if (profile == null || profile.warningLightEnabled)
                set.WarningLight = CreatePrimitive(set.Root.transform, "Warning Light", PrimitiveType.Sphere, new Vector3(-width * 0.32f, top + 0.28f, -height * 0.25f), Vector3.one * 0.16f, materials.WarningDamaged).transform;
            if (profile == null || profile.damageSmokePlaceholderEnabled)
                set.DamageSmoke = CreatePrimitive(set.Root.transform, "Damage Smoke Placeholder", PrimitiveType.Sphere, new Vector3(width * 0.22f, top + 0.38f, -height * 0.2f), new Vector3(0.28f, 0.2f, 0.28f), materials.SmokePlaceholder).transform;

            return set;
        }

        static GameObject CreatePrimitive(Transform parent, string objectName, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;

            var renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            var collider = primitive.GetComponent<Collider>();
            if (collider != null)
                DestroyUnityObject(collider);

            return primitive;
        }

        public static void DestroyUnityObject(Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(target);
            else
                Object.DestroyImmediate(target);
        }
    }
}
