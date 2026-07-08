using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Buildings
{
    public sealed class BuildingLightVisualController : MonoBehaviour
    {
        BuildingPlaceholderPartFactory.PartSet parts;
        BuildingVisualProfile profile;
        Stage7BuildingMaterialLibrary materials;
        float phase;

        public float LightIntensity01 { get; private set; }
        public bool IsBlinking { get; private set; }
        public int ActiveLightCount { get; private set; }

        public void Initialize(BuildingPlaceholderPartFactory.PartSet partSet, BuildingVisualProfile activeProfile, Stage7BuildingMaterialLibrary materialLibrary)
        {
            parts = partSet;
            profile = activeProfile;
            materials = materialLibrary;
        }

        public void TickVisual(float deltaTime, BuildingPowerVisualState powerState, bool lightsActive, bool warning)
        {
            if (parts == null)
                return;

            phase += Mathf.Max(0f, deltaTime) * Mathf.Max(0.1f, profile == null ? 2f : profile.lightPulseSpeed);
            var scale = 1f;
            if (powerState == BuildingPowerVisualState.LowPower)
                scale = profile == null ? 0.35f : profile.lowPowerLightScale;
            else if (powerState == BuildingPowerVisualState.Offline || powerState == BuildingPowerVisualState.Disabled || !lightsActive)
                scale = profile == null ? 0f : profile.offlineLightScale;

            var pulse = 0.78f + Mathf.Sin(phase * Mathf.PI * 2f) * 0.22f;
            LightIntensity01 = Mathf.Clamp01(scale * pulse);
            ActiveLightCount = 0;
            IsBlinking = warning || powerState == BuildingPowerVisualState.LowPower;

            for (var i = 0; i < parts.Lights.Count; i++)
            {
                var light = parts.Lights[i];
                if (light == null)
                    continue;

                var visible = LightIntensity01 > 0.01f;
                light.gameObject.SetActive(visible);
                if (visible)
                    ActiveLightCount++;
                light.localScale = Vector3.one * Mathf.Lerp(0.07f, 0.18f, LightIntensity01);
                SetMaterial(light, powerState == BuildingPowerVisualState.LowPower ? materials.LowPowerLight : materials.PoweredLight);
            }

            if (parts.WarningLight != null)
            {
                var warningOn = warning && Mathf.Sin(phase * Mathf.PI * 5f) > 0f;
                parts.WarningLight.gameObject.SetActive(warningOn);
                SetMaterial(parts.WarningLight, materials.WarningDamaged);
            }
        }

        static void SetMaterial(Transform target, Material material)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }
    }
}
