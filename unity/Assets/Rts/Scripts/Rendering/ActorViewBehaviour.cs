using System.Collections.Generic;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Utilities;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering
{
    public sealed class ActorViewBehaviour : MonoBehaviour
    {
        readonly List<GameObject> generatedObjects = new List<GameObject>();
        string configuredTypeId = string.Empty;
        GameObject body;
        GameObject turretMarker;
        GameObject selectionMarker;
        GameObject healthBar;
        GameObject lightMarker;
        GameObject machineryMarker;
        GameObject productionMarker;
        Vector3 previousSnapshotPosition;
        Vector3 targetSnapshotPosition;
        bool hasPosition;
        bool smoothVisuals;
        bool machineryActive;
        bool producing;
        float interpolationAlpha;
        float ticksPerSecond = 20f;
        int lastSnapshotTick = -1;

        public int ActorId { get; private set; }
        public Vector3 PreviousSnapshotPosition { get { return previousSnapshotPosition; } }
        public Vector3 TargetSnapshotPosition { get { return targetSnapshotPosition; } }
        public float VisualInterpolationAlpha { get { return interpolationAlpha; } }
        public int FacingDegrees { get; private set; }
        public int NormalizedSpeed { get; private set; }
        public string VisualMotionProfileId { get; private set; }
        public string ActorTypeCategory { get; private set; }

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
            int snapshotTick)
        {
            EnsureVisuals(definition, materials);

            var target = mapper.ActorToWorldPosition(snapshot, definition) + Vector3.up * BaseElevation(definition, snapshot.TypeId);
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
            transform.rotation = Quaternion.Euler(0f, snapshot.FacingDegrees, 0f);

            if (selectionMarker != null)
                selectionMarker.SetActive(selected);

            UpdateMaterials(definition, materials, snapshot);
            UpdateHealth(definition, snapshot);
            UpdateStateMarkers(definition, materials, snapshot);
        }

        public void TickVisual(float deltaTime)
        {
            if (!hasPosition)
                return;

            if (smoothVisuals)
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
        }

        void EnsureVisuals(ActorDefinition definition, Stage1MaterialLibrary materials)
        {
            if (configuredTypeId == definition.TypeId)
                return;

            configuredTypeId = definition.TypeId;
            ClearGeneratedObjects();
            ActorTypeCategory = DetermineCategory(definition.TypeId, definition);

            if (definition is BuildingDefinition)
                CreateBuildingVisual((BuildingDefinition)definition, materials);
            else
                CreateUnitVisual(definition.TypeId, materials);

            CreateCommonMarkers(definition, materials);
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
            turretMarker = CreatePrimitive("Vehicle Turret", PrimitiveType.Cube, new Vector3(0f, 0.49f, 0.10f), new Vector3(0.38f, 0.16f, 0.38f), materials.Machinery);
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
                Destroy(collider);

            generatedObjects.Add(primitive);
            return primitive;
        }

        void ClearGeneratedObjects()
        {
            for (var i = generatedObjects.Count - 1; i >= 0; i--)
                if (generatedObjects[i] != null)
                    Destroy(generatedObjects[i]);

            generatedObjects.Clear();
            body = null;
            turretMarker = null;
            selectionMarker = null;
            healthBar = null;
            lightMarker = null;
            machineryMarker = null;
            productionMarker = null;
        }

        static void SetMaterial(GameObject target, Material material)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
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
