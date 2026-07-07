using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.Utilities;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering
{
    public sealed class ActorViewBehaviour : MonoBehaviour
    {
        readonly List<GameObject> generatedObjects = new List<GameObject>();
        string configuredTypeId = string.Empty;
        GameObject configuredPrefab;
        GameObject prefabInstance;
        GameObject body;
        GameObject turretMarker;
        GameObject selectionMarker;
        GameObject healthBar;
        GameObject lightMarker;
        GameObject machineryMarker;
        GameObject productionMarker;
        GameObject leftTrackMarker;
        GameObject rightTrackMarker;
        TankVisualRigController tankRig;
        Vector3 previousSnapshotPosition;
        Vector3 targetSnapshotPosition;
        bool hasPosition;
        bool smoothVisuals;
        bool machineryActive;
        bool producing;
        float interpolationAlpha;
        float ticksPerSecond = 20f;
        int lastSnapshotTick = -1;
        int lastTankWeaponCooldownRemaining = -1;
        string lastTankWeaponId = string.Empty;

        public int ActorId { get; private set; }
        public Vector3 PreviousSnapshotPosition { get { return previousSnapshotPosition; } }
        public Vector3 TargetSnapshotPosition { get { return targetSnapshotPosition; } }
        public float VisualInterpolationAlpha { get { return interpolationAlpha; } }
        public int FacingDegrees { get; private set; }
        public int NormalizedSpeed { get; private set; }
        public string VisualMotionProfileId { get; private set; }
        public string ActorTypeCategory { get; private set; }
        public string ActorTypeId { get; private set; }
        public ActorVisualMotionController ActorVisualMotion { get; private set; }
        public VehicleVisualMotionController VehicleMotion { get; private set; }
        public InfantryVisualMotionController InfantryMotion { get; private set; }
        public AircraftVisualMotionController AircraftMotion { get; private set; }
        public TurretVisualAimController TurretAim { get; private set; }
        public BuildingVisualStateController BuildingVisual { get; private set; }
        public ActorVisualDefinition ActiveVisualDefinition { get; private set; }
        public ActorPrefabDescriptor ActivePrefabDescriptor { get; private set; }
        public bool UsesResolvedPrefab { get; private set; }
        public bool HasResolvedVisualDefinition { get; private set; }
        public bool UsedFallbackPrimitive { get; private set; }
        public string MotionControllerSummary
        {
            get
            {
                var summary = ActorVisualMotion == null ? "base=missing" : "base=" + ActorVisualMotion.CurrentMotionState;
                if (VehicleMotion != null)
                    summary += ", vehicle";
                if (InfantryMotion != null)
                    summary += ", infantry";
                if (AircraftMotion != null)
                    summary += ", aircraft";
                if (TurretAim != null)
                    summary += ", turret";
                return summary;
            }
        }

        public void Initialize(int actorId)
        {
            ActorId = actorId;
        }

        public void ApplySnapshot(
            ActorSnapshot snapshot,
            ActorDefinition definition,
            BoardCoordinateMapper mapper,
            Stage1MaterialLibrary materials,
            bool selected,
            bool enableSmoothVisuals,
            int simulationTicksPerSecond,
            int snapshotTick,
            VisualMotionProfile motionProfile,
            BuildingVisualProfile buildingProfile,
            ActorVisualDefinition visualDefinition = null,
            GameObject resolvedPrefab = null,
            AircraftSnapshot aircraftSnapshot = null)
        {
            EnsureVisuals(definition, materials, visualDefinition, resolvedPrefab);
            EnsureMotionControllers(definition, motionProfile);
            EnsureBuildingVisualController(definition, buildingProfile);

            var logicAltitudeOffset = AircraftAltitudeOffset(aircraftSnapshot, mapper);
            if (AircraftMotion != null && AircraftMotion.enabled && aircraftSnapshot != null)
                AircraftMotion.ApplyLogicAltitude(logicAltitudeOffset, aircraftSnapshot.DockedAirfieldActorId > 0);

            var target = mapper.ActorToWorldPosition(snapshot, definition) + Vector3.up * BaseElevation(definition, snapshot.TypeId, aircraftSnapshot);
            smoothVisuals = enableSmoothVisuals;
            ticksPerSecond = Mathf.Max(1f, simulationTicksPerSecond);

            if (!hasPosition)
            {
                previousSnapshotPosition = target;
                targetSnapshotPosition = target;
                transform.position = target;
                hasPosition = true;
                interpolationAlpha = 1f;
            }
            else if (snapshotTick != lastSnapshotTick && target != targetSnapshotPosition)
            {
                previousSnapshotPosition = smoothVisuals ? transform.position : target;
                targetSnapshotPosition = target;
                interpolationAlpha = smoothVisuals ? 0f : 1f;
            }

            lastSnapshotTick = snapshotTick;
            FacingDegrees = snapshot.FacingDegrees;
            NormalizedSpeed = snapshot.NormalizedSpeed;
            VisualMotionProfileId = snapshot.VisualMotionProfileId;
            if (smoothVisuals && ActorVisualMotion != null)
                ActorVisualMotion.ApplySnapshot(snapshot, target);
            else
                transform.rotation = Quaternion.Euler(0f, snapshot.FacingDegrees, 0f);

            if (selectionMarker != null)
                selectionMarker.SetActive(selected);

            UpdateMaterials(definition, materials, snapshot);
            UpdateHealth(definition, snapshot);
            UpdateStateMarkers(definition, materials, snapshot);
            if (BuildingVisual != null && BuildingVisual.enabled)
                BuildingVisual.ApplySnapshot(snapshot, definition.MaxHealth);
            UpdateTankVisualRig(snapshot, mapper);
        }

        public void TickVisual(float deltaTime)
        {
            if (!hasPosition)
                return;

            if (smoothVisuals && ActorVisualMotion != null)
            {
                ActorVisualMotion.TickVisual(deltaTime);
                interpolationAlpha = Mathf.Clamp01(interpolationAlpha + deltaTime * ticksPerSecond);

                if (VehicleMotion != null && VehicleMotion.enabled)
                    VehicleMotion.TickVisual(deltaTime);
                if (InfantryMotion != null && InfantryMotion.enabled)
                    InfantryMotion.TickVisual(deltaTime);
                if (AircraftMotion != null && AircraftMotion.enabled)
                    AircraftMotion.TickVisual(deltaTime);
                if (TurretAim != null && TurretAim.enabled)
                    TurretAim.TickVisual(deltaTime);
            }
            else if (smoothVisuals)
            {
                interpolationAlpha = Mathf.Clamp01(interpolationAlpha + deltaTime * ticksPerSecond);
                transform.position = Vector3.Lerp(previousSnapshotPosition, targetSnapshotPosition, interpolationAlpha);
            }
            else
            {
                interpolationAlpha = 1f;
                transform.position = targetSnapshotPosition;
            }

            if (machineryActive && machineryMarker != null)
                machineryMarker.transform.Rotate(Vector3.up, 120f * deltaTime, Space.Self);

            if (producing && productionMarker != null)
                productionMarker.transform.Rotate(Vector3.up, 180f * deltaTime, Space.Self);

            if (BuildingVisual != null && BuildingVisual.enabled)
                BuildingVisual.TickVisual(deltaTime);
        }

        void EnsureVisuals(ActorDefinition definition, Stage1MaterialLibrary materials, ActorVisualDefinition visualDefinition, GameObject resolvedPrefab)
        {
            if (configuredTypeId == definition.TypeId && configuredPrefab == resolvedPrefab)
                return;

            configuredTypeId = definition.TypeId;
            configuredPrefab = resolvedPrefab;
            ActorTypeId = definition.TypeId;
            ClearGeneratedObjects();
            ActorTypeCategory = DetermineCategory(definition.TypeId, definition);
            ActiveVisualDefinition = visualDefinition;
            HasResolvedVisualDefinition = visualDefinition != null;
            UsesResolvedPrefab = resolvedPrefab != null;
            UsedFallbackPrimitive = resolvedPrefab == null;

            if (resolvedPrefab != null)
                CreatePrefabVisual(definition, resolvedPrefab);
            else if (definition is BuildingDefinition)
                CreateBuildingVisual((BuildingDefinition)definition, materials);
            else
                CreateUnitVisual(definition.TypeId, materials);

            CreateCommonMarkers(definition, materials);
        }

        void CreatePrefabVisual(ActorDefinition definition, GameObject resolvedPrefab)
        {
            prefabInstance = Instantiate(resolvedPrefab);
            prefabInstance.name = "Stage8 Visual " + definition.TypeId;
            prefabInstance.transform.SetParent(transform, false);
            prefabInstance.transform.localPosition = Vector3.zero;
            prefabInstance.transform.localRotation = Quaternion.identity;
            prefabInstance.transform.localScale = Vector3.one;

            ActivePrefabDescriptor = prefabInstance.GetComponentInChildren<ActorPrefabDescriptor>(true);
            body = SocketGameObject(ActorPrefabSocketKind.VisualRoot);
            if (body == null)
                body = SocketGameObject(ActorPrefabSocketKind.BodyRoot);
            if (body == null)
                body = prefabInstance;

            turretMarker = SocketGameObject(ActorPrefabSocketKind.TurretRoot);
            if (turretMarker == null)
                turretMarker = SocketGameObject(ActorPrefabSocketKind.BarrelRoot);
            if (turretMarker == null)
                turretMarker = SocketGameObject(ActorPrefabSocketKind.MuzzlePrimary);

            leftTrackMarker = SocketGameObject(ActorPrefabSocketKind.TrackLeft);
            if (leftTrackMarker == null)
                leftTrackMarker = SocketGameObject(ActorPrefabSocketKind.WheelLeft);
            rightTrackMarker = SocketGameObject(ActorPrefabSocketKind.TrackRight);
            if (rightTrackMarker == null)
                rightTrackMarker = SocketGameObject(ActorPrefabSocketKind.WheelRight);
            tankRig = prefabInstance.GetComponentInChildren<TankVisualRigController>(true);
        }

        void UpdateTankVisualRig(ActorSnapshot snapshot, BoardCoordinateMapper mapper)
        {
            if (tankRig == null)
                return;

            if (snapshot.IsAttacking && mapper != null)
            {
                var targetWorld = mapper.CellToWorldCenter(snapshot.AttackTargetCell);
                var direction = targetWorld - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    tankRig.driveTurretFromDesiredAim = true;
                    tankRig.SetAimDirection(direction);
                }
            }
            else
            {
                tankRig.driveTurretFromDesiredAim = false;
            }

            bool weaponActive = !string.IsNullOrEmpty(snapshot.ActiveWeaponId) && snapshot.WeaponCooldownRemaining > 0;
            bool cooldownReset = lastTankWeaponCooldownRemaining >= 0 && snapshot.WeaponCooldownRemaining > lastTankWeaponCooldownRemaining + 1;
            bool newWeapon = !string.Equals(snapshot.ActiveWeaponId, lastTankWeaponId, System.StringComparison.Ordinal);
            if (weaponActive && (lastTankWeaponCooldownRemaining <= 0 || cooldownReset || newWeapon))
                tankRig.TriggerRecoil(1f);

            lastTankWeaponCooldownRemaining = snapshot.WeaponCooldownRemaining;
            lastTankWeaponId = snapshot.ActiveWeaponId;
        }

        void CreateBuildingVisual(BuildingDefinition definition, Stage1MaterialLibrary materials)
        {
            if (ActorTypeCategory == "defense")
            {
                body = CreatePrimitive("Defense Base", PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(0.75f, 0.7f, 0.75f), materials.Defense);
                turretMarker = CreatePrimitive("Defense Barrel", PrimitiveType.Cube, new Vector3(0f, 0.78f, 0.42f), new Vector3(0.18f, 0.16f, 0.85f), materials.Machinery);
                return;
            }

            var footprint = definition.FootprintCells;
            var height = definition.TypeId == "fabrication_hub" ? 1.1f : 0.75f;
            var scale = new Vector3(Mathf.Max(0.85f, footprint.X * 0.86f), height, Mathf.Max(0.85f, footprint.Y * 0.86f));
            body = CreatePrimitive("Building Body", PrimitiveType.Cube, new Vector3(0f, height * 0.5f, 0f), scale, materials.Building);

            if (definition.TypeId == "war_factory" || definition.TypeId == "refinery" || definition.TypeId == "barracks")
                turretMarker = CreatePrimitive("Exit Marker", PrimitiveType.Cube, new Vector3(0f, 0.12f, scale.z * 0.52f), new Vector3(0.42f, 0.12f, 0.12f), materials.Machinery);

            if (definition.Airfield != null)
                CreateAirfieldPadMarkers(definition, materials);
        }

        void CreateUnitVisual(string typeId, Stage1MaterialLibrary materials)
        {
            if (ActorTypeCategory == "infantry")
            {
                body = CreatePrimitive("Infantry Body", PrimitiveType.Capsule, new Vector3(0f, 0.32f, 0f), new Vector3(0.32f, 0.48f, 0.32f), materials.Infantry);
                return;
            }

            if (ActorTypeCategory == "aircraft")
            {
                body = CreatePrimitive("Aircraft Body", PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(0.85f, 0.18f, 0.55f), materials.Aircraft);
                turretMarker = CreatePrimitive("Aircraft Wing", PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(1.35f, 0.08f, 0.18f), materials.Machinery);
                return;
            }

            body = CreatePrimitive("Vehicle Body", PrimitiveType.Cube, new Vector3(0f, 0.24f, 0f), new Vector3(0.85f, 0.34f, 0.62f), materials.Vehicle);
            leftTrackMarker = CreatePrimitive("Left Track Placeholder", PrimitiveType.Cube, new Vector3(-0.43f, 0.13f, 0f), new Vector3(0.08f, 0.16f, 0.64f), materials.Machinery);
            rightTrackMarker = CreatePrimitive("Right Track Placeholder", PrimitiveType.Cube, new Vector3(0.43f, 0.13f, 0f), new Vector3(0.08f, 0.16f, 0.64f), materials.Machinery);
            turretMarker = CreatePrimitive("Vehicle Turret", PrimitiveType.Cube, new Vector3(0f, 0.49f, 0.10f), new Vector3(0.38f, 0.16f, 0.38f), materials.Machinery);
        }

        void EnsureMotionControllers(ActorDefinition definition, VisualMotionProfile motionProfile)
        {
            var category = VisualMotionProfileLibrary.CategoryForActor(definition.TypeId, definition);
            if (ActorVisualMotion == null)
                ActorVisualMotion = GetOrAdd<ActorVisualMotionController>();

            var activeProfile = motionProfile != null ? motionProfile : VisualMotionProfile.CreateRuntimeDefault("runtime_" + definition.TypeId, category);
            ActorVisualMotion.Initialize(ActorId, definition.TypeId, activeProfile);

            if (category == VisualMotionCategory.Vehicle || category == VisualMotionCategory.Harvester)
            {
                if (VehicleMotion == null)
                    VehicleMotion = GetOrAdd<VehicleVisualMotionController>();
                VehicleMotion.enabled = true;
                VehicleMotion.leftTrackPlaceholder = leftTrackMarker == null ? null : leftTrackMarker.transform;
                VehicleMotion.rightTrackPlaceholder = rightTrackMarker == null ? null : rightTrackMarker.transform;
                VehicleMotion.suspensionRoot = body == null ? transform : body.transform;
                VehicleMotion.Initialize(ActorVisualMotion);
            }
            else if (VehicleMotion != null)
                VehicleMotion.enabled = false;

            if (category == VisualMotionCategory.Infantry)
            {
                if (InfantryMotion == null)
                    InfantryMotion = GetOrAdd<InfantryVisualMotionController>();
                InfantryMotion.enabled = true;
                InfantryMotion.bodyRoot = body == null ? transform : body.transform;
                InfantryMotion.Initialize(ActorVisualMotion);
            }
            else if (InfantryMotion != null)
                InfantryMotion.enabled = false;

            if (category == VisualMotionCategory.Aircraft)
            {
                if (AircraftMotion == null)
                    AircraftMotion = GetOrAdd<AircraftVisualMotionController>();
                AircraftMotion.enabled = true;
                AircraftMotion.aircraftRoot = body == null ? transform : body.transform;
                AircraftMotion.Initialize(ActorVisualMotion);
            }
            else if (AircraftMotion != null)
                AircraftMotion.enabled = false;

            if (turretMarker != null)
            {
                if (TurretAim == null)
                    TurretAim = GetOrAdd<TurretVisualAimController>();
                TurretAim.enabled = true;
                TurretAim.Initialize(ActorVisualMotion, turretMarker.transform);
            }
            else if (TurretAim != null)
                TurretAim.enabled = false;
        }

        void EnsureBuildingVisualController(ActorDefinition definition, BuildingVisualProfile buildingProfile)
        {
            var building = definition as BuildingDefinition;
            if (building == null)
            {
                if (BuildingVisual != null)
                    BuildingVisual.enabled = false;
                return;
            }

            if (BuildingVisual == null)
            {
                if (prefabInstance != null)
                    BuildingVisual = prefabInstance.GetComponentInChildren<BuildingVisualStateController>(true);
                if (BuildingVisual == null)
                    BuildingVisual = GetOrAdd<BuildingVisualStateController>();
            }

            BuildingVisual.enabled = true;
            var activeProfile = buildingProfile != null ? buildingProfile : BuildingVisualProfile.CreateRuntimeDefault(definition.TypeId, BuildingVisualProfileLibrary.CategoryForActor(definition.TypeId));
            BuildingVisual.Initialize(ActorId, definition.TypeId, activeProfile);
        }

        void CreateCommonMarkers(ActorDefinition definition, Stage1MaterialLibrary materials)
        {
            var radius = definition is BuildingDefinition ? Mathf.Max(((BuildingDefinition)definition).FootprintCells.X, ((BuildingDefinition)definition).FootprintCells.Y) * 0.55f : 0.58f;
            selectionMarker = CreatePrimitive("Selection Marker", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, 0f), new Vector3(radius, 0.025f, radius), materials.Selection);
            selectionMarker.SetActive(false);

            healthBar = CreatePrimitive("Health Bar", PrimitiveType.Cube, new Vector3(0f, MarkerHeight(definition) + 0.18f, -0.18f), new Vector3(0.7f, 0.055f, 0.08f), materials.Health);
            lightMarker = CreatePrimitive("Power Light", PrimitiveType.Sphere, new Vector3(-0.25f, MarkerHeight(definition) + 0.25f, 0.16f), new Vector3(0.16f, 0.16f, 0.16f), materials.LightActive);
            machineryMarker = CreatePrimitive("Machinery Marker", PrimitiveType.Cylinder, new Vector3(0.25f, MarkerHeight(definition) + 0.18f, 0.16f), new Vector3(0.18f, 0.12f, 0.18f), materials.Machinery);
            productionMarker = CreatePrimitive("Production Marker", PrimitiveType.Cube, new Vector3(0f, MarkerHeight(definition) + 0.38f, 0f), new Vector3(0.24f, 0.24f, 0.24f), materials.Production);
            productionMarker.SetActive(false);
        }

        void UpdateMaterials(ActorDefinition definition, Stage1MaterialLibrary materials, ActorSnapshot snapshot)
        {
            if (body == null)
                return;

            if (definition is BuildingDefinition)
                SetMaterial(body, snapshot.LightsActive && snapshot.MachineryActive ? materials.Building : materials.BuildingLowPower);
        }

        void UpdateHealth(ActorDefinition definition, ActorSnapshot snapshot)
        {
            if (healthBar == null)
                return;

            var fraction = definition.MaxHealth <= 0 ? 1f : Mathf.Clamp01(snapshot.Health / (float)definition.MaxHealth);
            healthBar.transform.localScale = new Vector3(Mathf.Max(0.08f, 0.72f * fraction), 0.055f, 0.08f);
        }

        void UpdateStateMarkers(ActorDefinition definition, Stage1MaterialLibrary materials, ActorSnapshot snapshot)
        {
            var isBuilding = definition is BuildingDefinition;

            if (lightMarker != null)
            {
                lightMarker.SetActive(isBuilding);
                SetMaterial(lightMarker, snapshot.LightsActive ? materials.LightActive : materials.LightInactive);
            }

            if (machineryMarker != null)
            {
                var showMachinery = isBuilding || turretMarker != null;
                machineryMarker.SetActive(showMachinery && (!isBuilding || snapshot.MachineryActive));
            }

            machineryActive = snapshot.MachineryActive;
            producing = snapshot.IsProducing;

            if (productionMarker != null)
                productionMarker.SetActive(snapshot.IsProducing);
        }

        GameObject CreatePrimitive(string objectName, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(transform, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            SetMaterial(primitive, material);

            var collider = primitive.GetComponent<Collider>();
            if (collider != null)
                DestroyObject(collider);

            generatedObjects.Add(primitive);
            return primitive;
        }

        void ClearGeneratedObjects()
        {
            if (prefabInstance != null)
                DestroyObject(prefabInstance);
            prefabInstance = null;
            ActivePrefabDescriptor = null;
            ActiveVisualDefinition = null;
            UsesResolvedPrefab = false;
            HasResolvedVisualDefinition = false;
            UsedFallbackPrimitive = true;

            for (var i = generatedObjects.Count - 1; i >= 0; i--)
                if (generatedObjects[i] != null)
                    DestroyObject(generatedObjects[i]);

            generatedObjects.Clear();
            body = null;
            turretMarker = null;
            selectionMarker = null;
            healthBar = null;
            lightMarker = null;
            machineryMarker = null;
            productionMarker = null;
            leftTrackMarker = null;
            rightTrackMarker = null;
            tankRig = null;
            lastTankWeaponCooldownRemaining = -1;
            lastTankWeaponId = string.Empty;
            if (BuildingVisual != null)
                BuildingVisual.ResetVisualState();
            BuildingVisual = null;
        }

        GameObject SocketGameObject(ActorPrefabSocketKind kind)
        {
            if (ActivePrefabDescriptor == null)
                return null;

            Transform socketTransform;
            return ActivePrefabDescriptor.TryGetSocket(kind, out socketTransform) && socketTransform != null ? socketTransform.gameObject : null;
        }

        T GetOrAdd<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();
            return component;
        }

        static void SetMaterial(GameObject target, Material material)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }

        static void DestroyObject(Object target)
        {
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }

        static float MarkerHeight(ActorDefinition definition)
        {
            if (definition.TypeId == "fabrication_hub")
                return 1.15f;
            if (definition is BuildingDefinition)
                return 0.85f;
            return 0.62f;
        }

        static float BaseElevation(ActorDefinition definition, string typeId)
        {
            return typeId.Contains("aircraft") ? 1.4f : 0f;
        }

        static float BaseElevation(ActorDefinition definition, string typeId, AircraftSnapshot aircraftSnapshot)
        {
            if (aircraftSnapshot != null)
                return 0f;
            return BaseElevation(definition, typeId);
        }

        static float AircraftAltitudeOffset(AircraftSnapshot aircraftSnapshot, BoardCoordinateMapper mapper)
        {
            if (aircraftSnapshot == null || mapper == null)
                return 0f;
            return Mathf.Max(0f, aircraftSnapshot.AltitudeSubCells / (float)FixedMath.CellScale * mapper.CellSizeMeters);
        }

        void CreateAirfieldPadMarkers(BuildingDefinition definition, Stage1MaterialLibrary materials)
        {
            var airfield = definition.Airfield;
            if (airfield == null)
                return;

            for (var i = 0; i < airfield.PadCount; i++)
            {
                var offset = airfield.PadOffsets.Count > i ? airfield.PadOffsets[i] : Int2.Zero;
                var localX = offset.X - definition.FootprintCells.X * 0.5f + 0.5f;
                var localZ = offset.Y - definition.FootprintCells.Y * 0.5f + 0.5f;
                CreatePrimitive("Helipad Pad " + (i + 1), PrimitiveType.Cylinder, new Vector3(localX, 0.42f, localZ), new Vector3(0.42f, 0.035f, 0.42f), materials.Selection);
            }
        }

        static string DetermineCategory(string typeId, ActorDefinition definition)
        {
            if (typeId.Contains("aircraft"))
                return "aircraft";
            if (typeId.Contains("infantry") || typeId == "engineer")
                return "infantry";
            if (typeId.Contains("tower") || typeId.Contains("turret"))
                return "defense";
            if (definition is BuildingDefinition)
                return "building";
            return "vehicle";
        }
    }
}
