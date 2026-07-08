using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Motion
{
    public sealed class Stage6MotionShowcase : MonoBehaviour
    {
        public VisualMotionProfileLibrary profileLibrary;
        public MovementPathPreview movementPathPreview;
        public bool animateInPlayMode = true;

        float phase;
        Vector3 vehicleOrigin = new Vector3(18f, 0f, 7f);
        Vector3 infantryOrigin = new Vector3(20f, 0f, 9f);
        Vector3 aircraftOrigin = new Vector3(22f, 1.4f, 7f);

        public ActorVisualMotionController VehicleMotion { get; private set; }
        public ActorVisualMotionController InfantryMotion { get; private set; }
        public ActorVisualMotionController AircraftMotion { get; private set; }
        public TurretVisualAimController TurretAim { get; private set; }
        public VehicleVisualMotionController VehicleDetails { get; private set; }
        public InfantryVisualMotionController InfantryDetails { get; private set; }
        public AircraftVisualMotionController AircraftDetails { get; private set; }

        void Awake()
        {
            EnsureShowcase();
        }

        void Update()
        {
            if (animateInPlayMode)
                TickShowcase(Time.deltaTime);
        }

        public void EnsureShowcase()
        {
            if (profileLibrary == null)
                profileLibrary = Object.FindFirstObjectByType<VisualMotionProfileLibrary>();
            if (movementPathPreview == null)
                movementPathPreview = Object.FindFirstObjectByType<MovementPathPreview>();
            if (profileLibrary != null)
                profileLibrary.EnsureInitialized();

            if (VehicleMotion == null)
                VehicleMotion = CreateVehicle();
            if (InfantryMotion == null)
                InfantryMotion = CreateInfantry();
            if (AircraftMotion == null)
                AircraftMotion = CreateAircraft();

            ApplyShowcaseTargets(0f);
        }

        public void TickShowcase(float deltaTime)
        {
            EnsureShowcase();
            phase += Mathf.Max(0.01f, deltaTime);
            ApplyShowcaseTargets(deltaTime);

            VehicleMotion.TickVisual(deltaTime);
            InfantryMotion.TickVisual(deltaTime);
            AircraftMotion.TickVisual(deltaTime);

            if (VehicleDetails != null)
                VehicleDetails.TickVisual(deltaTime);
            if (InfantryDetails != null)
                InfantryDetails.TickVisual(deltaTime);
            if (AircraftDetails != null)
                AircraftDetails.TickVisual(deltaTime);
            if (TurretAim != null)
                TurretAim.TickVisual(deltaTime);
        }

        ActorVisualMotionController CreateVehicle()
        {
            var root = EnsureChild("Stage6 Vehicle Motion Placeholder", vehicleOrigin);
            EnsurePrimitive(root.transform, "Vehicle Body", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), new Vector3(0.9f, 0.35f, 0.65f), new Color(0.28f, 0.55f, 0.82f));
            var leftTrack = EnsurePrimitive(root.transform, "Left Track Placeholder", PrimitiveType.Cube, new Vector3(-0.46f, 0.14f, 0f), new Vector3(0.09f, 0.17f, 0.66f), new Color(0.08f, 0.1f, 0.12f));
            var rightTrack = EnsurePrimitive(root.transform, "Right Track Placeholder", PrimitiveType.Cube, new Vector3(0.46f, 0.14f, 0f), new Vector3(0.09f, 0.17f, 0.66f), new Color(0.08f, 0.1f, 0.12f));
            var turret = EnsurePrimitive(root.transform, "Vehicle Turret", PrimitiveType.Cube, new Vector3(0f, 0.5f, 0.1f), new Vector3(0.4f, 0.16f, 0.38f), new Color(0.7f, 0.75f, 0.82f));

            var motion = GetOrAdd<ActorVisualMotionController>(root);
            motion.Initialize(9001, "light_tank", GetProfile("light_tank", VisualMotionCategory.Vehicle, "tracked_light"));
            motion.ApplySnapshot(CreateSnapshot(9001, "light_tank", new Int2(18, 7), "tracked_light", 0), vehicleOrigin);

            VehicleDetails = GetOrAdd<VehicleVisualMotionController>(root);
            VehicleDetails.leftTrackPlaceholder = leftTrack.transform;
            VehicleDetails.rightTrackPlaceholder = rightTrack.transform;
            VehicleDetails.suspensionRoot = root.transform.Find("Vehicle Body");
            VehicleDetails.Initialize(motion);

            TurretAim = GetOrAdd<TurretVisualAimController>(root);
            TurretAim.Initialize(motion, turret.transform);
            return motion;
        }

        ActorVisualMotionController CreateInfantry()
        {
            var root = EnsureChild("Stage6 Infantry Motion Placeholder", infantryOrigin);
            var body = EnsurePrimitive(root.transform, "Infantry Body", PrimitiveType.Capsule, new Vector3(0f, 0.32f, 0f), new Vector3(0.32f, 0.48f, 0.32f), new Color(0.42f, 0.68f, 0.38f));
            var motion = GetOrAdd<ActorVisualMotionController>(root);
            motion.Initialize(9002, "rifle_infantry", GetProfile("rifle_infantry", VisualMotionCategory.Infantry, "infantry_basic"));
            motion.ApplySnapshot(CreateSnapshot(9002, "rifle_infantry", new Int2(20, 9), "infantry_basic", 45), infantryOrigin);

            InfantryDetails = GetOrAdd<InfantryVisualMotionController>(root);
            InfantryDetails.bodyRoot = body.transform;
            InfantryDetails.Initialize(motion);
            return motion;
        }

        ActorVisualMotionController CreateAircraft()
        {
            var root = EnsureChild("Stage6 Aircraft Motion Placeholder", aircraftOrigin);
            var body = EnsurePrimitive(root.transform, "Aircraft Body", PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.18f, 0.58f), new Color(0.68f, 0.5f, 0.9f));
            EnsurePrimitive(root.transform, "Aircraft Wing", PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(1.45f, 0.08f, 0.18f), new Color(0.78f, 0.74f, 0.92f));
            var motion = GetOrAdd<ActorVisualMotionController>(root);
            motion.Initialize(9003, "attack_aircraft", GetProfile("attack_aircraft", VisualMotionCategory.Aircraft, "aircraft_attack"));
            motion.ApplySnapshot(CreateSnapshot(9003, "attack_aircraft", new Int2(22, 7), "aircraft_attack", 90), aircraftOrigin);

            AircraftDetails = GetOrAdd<AircraftVisualMotionController>(root);
            AircraftDetails.aircraftRoot = body.transform;
            AircraftDetails.Initialize(motion);
            return motion;
        }

        void ApplyShowcaseTargets(float deltaTime)
        {
            var vehicleTarget = vehicleOrigin + new Vector3(Mathf.Sin(phase * 0.8f) * 1.5f, 0f, Mathf.Cos(phase * 0.8f) * 0.8f);
            var infantryTarget = infantryOrigin + new Vector3(Mathf.Sin(phase * 1.2f) * 0.9f, 0f, Mathf.Cos(phase * 1.2f) * 0.6f);
            var aircraftTarget = aircraftOrigin + new Vector3(Mathf.Sin(phase * 0.65f) * 1.4f, 0f, Mathf.Cos(phase * 0.65f) * 1f);

            VehicleMotion.ApplySnapshot(CreateSnapshot(9001, "light_tank", WorldToCell(vehicleTarget), "tracked_light", HeadingTo(vehicleTarget, VehicleMotion.VisualPosition)), vehicleTarget);
            InfantryMotion.ApplySnapshot(CreateSnapshot(9002, "rifle_infantry", WorldToCell(infantryTarget), "infantry_basic", HeadingTo(infantryTarget, InfantryMotion.VisualPosition)), infantryTarget);
            AircraftMotion.ApplySnapshot(CreateSnapshot(9003, "attack_aircraft", WorldToCell(aircraftTarget), "aircraft_attack", HeadingTo(aircraftTarget, AircraftMotion.VisualPosition)), aircraftTarget);

            if (movementPathPreview != null)
                movementPathPreview.SetPreviewWorld(VehicleMotion.VisualPosition, vehicleTarget);
        }

        VisualMotionProfile GetProfile(string typeId, VisualMotionCategory category, string profileId)
        {
            return profileLibrary == null ? VisualMotionProfile.CreateRuntimeDefault(profileId, category) : profileLibrary.GetProfile(typeId, category, profileId);
        }

        GameObject EnsureChild(string childName, Vector3 localPosition)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var obj = new GameObject(childName);
                obj.transform.SetParent(transform, false);
                child = obj.transform;
            }

            child.localPosition = localPosition;
            return child.gameObject;
        }

        static GameObject EnsurePrimitive(Transform parent, string childName, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var child = parent.Find(childName);
            GameObject primitive;
            if (child == null)
            {
                primitive = GameObject.CreatePrimitive(type);
                primitive.name = childName;
                primitive.transform.SetParent(parent, false);
                var collider = primitive.GetComponent<Collider>();
                if (collider != null)
                    DestroyUnityObject(collider);
            }
            else
            {
                primitive = child.gameObject;
            }

            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            var renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = renderer.sharedMaterial;
                if (material == null || material.name.Contains("Default-Material"))
                    material = new Material(Shader.Find("Standard"));
                material.color = color;
                renderer.sharedMaterial = material;
            }

            return primitive;
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        static ActorSnapshot CreateSnapshot(int actorId, string typeId, Int2 cell, string profileId, int facingDegrees)
        {
            return new ActorSnapshot(
                actorId,
                typeId,
                1,
                cell,
                FixedMath.CellCenter(cell),
                facingDegrees,
                100,
                false,
                true,
                false,
                true,
                true,
                false,
                0,
                "stage6_visual_showcase",
                profileId,
                128,
                1000,
                30,
                "moving");
        }

        static Int2 WorldToCell(Vector3 worldPosition)
        {
            return new Int2(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.z));
        }

        static int HeadingTo(Vector3 target, Vector3 current)
        {
            var delta = target - current;
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.0001f)
                return 0;

            return Mathf.RoundToInt(Quaternion.LookRotation(delta.normalized, Vector3.up).eulerAngles.y);
        }

        static void DestroyUnityObject(Object target)
        {
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
