using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class LightingProfileApplier : MonoBehaviour
    {
        public LightingProfile profile;
        public Light directionalLight;
        public Camera targetCamera;
        public bool applyOnAwake = true;

        void Awake()
        {
            if (applyOnAwake)
                ApplyProfile();
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                ApplyProfile();
        }

        public void ApplyProfile()
        {
            if (profile == null)
                return;

            if (directionalLight == null)
                directionalLight = FindFirstObjectByType<Light>();
            if (targetCamera == null)
                targetCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = profile.ambientColor;
            RenderSettings.fog = profile.fogEnabled;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = profile.fogColor;
            RenderSettings.fogDensity = profile.fogDensity;

            if (directionalLight != null)
            {
                directionalLight.type = LightType.Directional;
                directionalLight.color = profile.directionalColor;
                directionalLight.intensity = profile.directionalIntensity;
                directionalLight.transform.rotation = Quaternion.Euler(profile.directionalEuler);
            }

            if (targetCamera != null)
                targetCamera.backgroundColor = profile.cameraBackground;
        }
    }
}
