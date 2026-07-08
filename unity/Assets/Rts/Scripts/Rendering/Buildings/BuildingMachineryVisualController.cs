using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingMachineryVisualController : MonoBehaviour
    {
        BuildingPlaceholderPartFactory.PartSet parts;
        BuildingVisualProfile profile;

        public float CurrentMachinerySpeed { get; private set; }
        public float CurrentLoopPhase { get; private set; }
        public bool IsMachineryActive { get; private set; }

        public void Initialize(BuildingPlaceholderPartFactory.PartSet partSet, BuildingVisualProfile activeProfile)
        {
            parts = partSet;
            profile = activeProfile;
        }

        public void TickVisual(float deltaTime, BuildingPowerVisualState powerState, bool machineryActive)
        {
            if (parts == null)
                return;

            var speed = profile == null ? 1f : profile.machineryLoopSpeed;
            if (powerState == BuildingPowerVisualState.LowPower)
                speed *= profile == null ? 0.25f : profile.lowPowerMachinerySpeedScale;
            if (powerState == BuildingPowerVisualState.Offline || powerState == BuildingPowerVisualState.Disabled || !machineryActive)
                speed = 0f;

            CurrentMachinerySpeed = speed;
            IsMachineryActive = speed > 0.001f;
            CurrentLoopPhase = Mathf.Repeat(CurrentLoopPhase + speed * deltaTime, 1f);

            if (parts.Machinery != null)
                parts.Machinery.Rotate(Vector3.up, 180f * speed * deltaTime, Space.Self);
            if (parts.Turbine != null)
                parts.Turbine.Rotate(Vector3.up, (profile == null ? 140f : profile.turbineSpinSpeed) * speed * deltaTime, Space.Self);
            if (parts.ExtraTurbines != null)
                for (var i = 0; i < parts.ExtraTurbines.Count; i++)
                    if (parts.ExtraTurbines[i] != null)
                        parts.ExtraTurbines[i].Rotate(Vector3.up, (i % 2 == 0 ? -1f : 1f) * (profile == null ? 140f : profile.turbineSpinSpeed) * speed * deltaTime, Space.Self);
            if (parts.RadarDish != null)
                parts.RadarDish.Rotate(Vector3.up, (profile == null ? 35f : profile.radarDishSpinSpeed) * speed * deltaTime, Space.Self);
            if (parts.CraneArm != null)
            {
                var degrees = profile == null ? 35f : profile.craneSweepDegrees;
                var sweepSpeed = profile == null ? 1f : profile.craneSweepSpeed;
                parts.CraneArm.localRotation = Quaternion.Euler(0f, Mathf.Sin(CurrentLoopPhase * Mathf.PI * 2f * sweepSpeed) * degrees, 0f);
            }
            if (parts.RepairArmLeft != null)
                parts.RepairArmLeft.localRotation = Quaternion.Euler(Mathf.Sin(CurrentLoopPhase * Mathf.PI * 2f) * 20f, 0f, 0f);
            if (parts.RepairArmRight != null)
                parts.RepairArmRight.localRotation = Quaternion.Euler(Mathf.Sin(CurrentLoopPhase * Mathf.PI * 2f + Mathf.PI) * 20f, 0f, 0f);
            if (parts.DockPump != null)
            {
                var local = parts.DockPump.localPosition;
                parts.DockPump.localPosition = new Vector3(local.x, 0.32f + Mathf.Sin(CurrentLoopPhase * Mathf.PI * 2f) * 0.08f, local.z);
            }
        }
    }
}
