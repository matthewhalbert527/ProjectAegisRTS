using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    [CreateAssetMenu(fileName = "BuildingVisualProfile", menuName = "ProjectAegisRTS/Building Visual Profile")]
    public sealed class BuildingVisualProfile : ScriptableObject
    {
        public string profileId = "default_building";
        public string displayName = "Default Building";
        public string actorTypeId = "default_building";
        public BuildingVisualCategory category = BuildingVisualCategory.Unknown;
        public int footprintWidth = 2;
        public int footprintHeight = 2;
        public bool hasLights = true;
        public int lightCount = 2;
        public float lightPulseSpeed = 2f;
        public bool hasMachinery = true;
        public float machineryLoopSpeed = 1f;
        public bool hasDoor;
        public float doorOpenDistance = 0.35f;
        public float doorOpenSpeed = 4f;
        public bool hasProductionBay;
        public float productionPulseSpeed = 3f;
        public bool hasTurbine;
        public float turbineSpinSpeed = 140f;
        public bool hasRadarDish;
        public float radarDishSpinSpeed = 35f;
        public bool hasCrane;
        public float craneSweepDegrees = 35f;
        public float craneSweepSpeed = 1.2f;
        public bool hasRepairArms;
        public float repairArmSpeed = 2.2f;
        public bool hasDock;
        public float dockPumpSpeed = 2.5f;
        public bool hasTurretOrBarrel;
        public float turretIdleSweepSpeed = 0.7f;
        public float lowPowerLightScale = 0.35f;
        public float offlineLightScale = 0f;
        public float lowPowerMachinerySpeedScale = 0.25f;
        public float damagedHealthThreshold01 = 0.5f;
        public float destroyedHealthThreshold01 = 0.05f;
        public bool damageSmokePlaceholderEnabled = true;
        public bool warningLightEnabled = true;

        public void ConfigureDefault(string id, string actorId, BuildingVisualCategory visualCategory, int width, int height)
        {
            profileId = id;
            actorTypeId = actorId;
            displayName = ToDisplayName(actorId);
            category = visualCategory;
            footprintWidth = Mathf.Max(1, width);
            footprintHeight = Mathf.Max(1, height);

            hasLights = true;
            lightCount = visualCategory == BuildingVisualCategory.Power ? 4 : 2;
            lightPulseSpeed = 2f;
            hasMachinery = true;
            machineryLoopSpeed = 1f;
            hasDoor = false;
            doorOpenDistance = 0.35f;
            doorOpenSpeed = 4f;
            hasProductionBay = false;
            productionPulseSpeed = 3f;
            hasTurbine = false;
            turbineSpinSpeed = 140f;
            hasRadarDish = false;
            radarDishSpinSpeed = 35f;
            hasCrane = false;
            craneSweepDegrees = 35f;
            craneSweepSpeed = 1.2f;
            hasRepairArms = false;
            repairArmSpeed = 2.2f;
            hasDock = false;
            dockPumpSpeed = 2.5f;
            hasTurretOrBarrel = false;
            turretIdleSweepSpeed = 0.7f;
            lowPowerLightScale = 0.35f;
            offlineLightScale = 0f;
            lowPowerMachinerySpeedScale = 0.25f;
            damagedHealthThreshold01 = 0.5f;
            destroyedHealthThreshold01 = 0.05f;
            damageSmokePlaceholderEnabled = true;
            warningLightEnabled = true;

            ApplyCategoryDefaults();
            ApplyTypeDefaults(actorId);
        }

        public static BuildingVisualProfile CreateRuntimeDefault(string actorTypeId, BuildingVisualCategory category)
        {
            var profile = CreateInstance<BuildingVisualProfile>();
            profile.hideFlags = HideFlags.DontSave;
            profile.ConfigureDefault("runtime_" + actorTypeId, actorTypeId, category, 2, 2);
            return profile;
        }

        void ApplyCategoryDefaults()
        {
            if (category == BuildingVisualCategory.Power)
            {
                hasTurbine = true;
                machineryLoopSpeed = 1.25f;
                turbineSpinSpeed = 190f;
                lowPowerMachinerySpeedScale = 0.18f;
            }
            else if (category == BuildingVisualCategory.Production)
            {
                hasDoor = true;
                hasProductionBay = true;
                hasMachinery = true;
            }
            else if (category == BuildingVisualCategory.Refinery)
            {
                hasDock = true;
                hasMachinery = true;
                machineryLoopSpeed = 0.85f;
            }
            else if (category == BuildingVisualCategory.Defense)
            {
                hasTurretOrBarrel = true;
                lightCount = 1;
            }
            else if (category == BuildingVisualCategory.Repair)
            {
                hasRepairArms = true;
                hasDoor = true;
            }
            else if (category == BuildingVisualCategory.Tech)
            {
                hasRadarDish = true;
                lightCount = 3;
            }
            else if (category == BuildingVisualCategory.Airfield)
            {
                hasProductionBay = true;
                lightCount = 5;
            }
        }

        void ApplyTypeDefaults(string actorId)
        {
            if (actorId == "fabrication_hub")
            {
                hasCrane = true;
                hasProductionBay = true;
                hasDoor = true;
                lightCount = 4;
            }
            else if (actorId == "advanced_power_plant")
            {
                hasTurbine = true;
                turbineSpinSpeed = 230f;
                lightCount = 5;
            }
            else if (actorId == "barracks")
            {
                hasDoor = true;
                hasProductionBay = true;
            }
            else if (actorId == "war_factory")
            {
                hasDoor = true;
                hasProductionBay = true;
                hasMachinery = true;
                doorOpenDistance = 0.5f;
            }
            else if (actorId == "comm_center")
            {
                hasRadarDish = true;
                radarDishSpinSpeed = 50f;
            }
            else if (actorId == "field_hospital")
            {
                lightCount = 3;
                machineryLoopSpeed = 0.45f;
            }
            else if (actorId == "dual_helipad")
            {
                hasProductionBay = true;
                lightCount = 6;
                machineryLoopSpeed = 0.6f;
            }
        }

        static string ToDisplayName(string id)
        {
            return string.IsNullOrEmpty(id) ? "Building Visual" : id.Replace('_', ' ');
        }
    }
}
