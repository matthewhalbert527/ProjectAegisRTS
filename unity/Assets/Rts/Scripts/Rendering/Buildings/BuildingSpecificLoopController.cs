using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingSpecificLoopController : MonoBehaviour
    {
        BuildingPlaceholderPartFactory.PartSet parts;
        BuildingVisualProfile profile;
        string actorTypeId = string.Empty;
        float phase;

        public float LoopPhase { get { return phase; } }

        public void Initialize(BuildingPlaceholderPartFactory.PartSet partSet, BuildingVisualProfile activeProfile, string typeId)
        {
            parts = partSet;
            profile = activeProfile;
            actorTypeId = typeId ?? string.Empty;
        }

        public void TickVisual(float deltaTime, BuildingPowerVisualState powerState, bool machineryActive, bool isProducing)
        {
            if (parts == null)
                return;

            var activeScale = powerState == BuildingPowerVisualState.Normal && machineryActive ? 1f : 0.25f;
            if (powerState == BuildingPowerVisualState.Offline || powerState == BuildingPowerVisualState.Disabled)
                activeScale = 0f;
            phase = Mathf.Repeat(phase + deltaTime * activeScale, 1f);

            if (actorTypeId == "fabrication_hub" && parts.CraneArm != null)
                parts.CraneArm.localRotation = Quaternion.Euler(0f, Mathf.Sin(phase * Mathf.PI * 2f) * 38f, 0f);
            else if ((actorTypeId == "gun_tower" || actorTypeId == "cannon_turret" || actorTypeId == "advanced_gun_tower") && parts.TurretOrBarrel != null)
                parts.TurretOrBarrel.localRotation = Quaternion.Euler(0f, Mathf.Sin(phase * Mathf.PI * 2f * (profile == null ? 0.7f : profile.turretIdleSweepSpeed)) * 18f, 0f);
            else if (actorTypeId == "dual_helipad" && parts.ProductionIndicator != null)
                parts.ProductionIndicator.localRotation = Quaternion.Euler(0f, phase * 360f, 0f);

            if (isProducing && parts.ProductionIndicator != null)
                parts.ProductionIndicator.Rotate(Vector3.up, 90f * deltaTime, Space.Self);
        }
    }
}
