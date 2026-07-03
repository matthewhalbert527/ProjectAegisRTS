using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class BattlefieldAtmosphereController : MonoBehaviour
    {
        public LightingProfile lightingProfile;
        public LightingProfileApplier lightingProfileApplier;
        public BattlefieldMaterialLibrary materialLibrary;
        public bool applyOnStart = true;
        public bool fogShouldStaySubtle = true;

        void Start()
        {
            if (applyOnStart)
                ApplyAtmosphere();
        }

        public void ApplyAtmosphere()
        {
            if (lightingProfileApplier == null)
                lightingProfileApplier = FindFirstObjectByType<LightingProfileApplier>();
            if (materialLibrary == null)
                materialLibrary = FindFirstObjectByType<BattlefieldMaterialLibrary>();

            if (lightingProfileApplier != null)
            {
                if (lightingProfileApplier.profile == null)
                    lightingProfileApplier.profile = lightingProfile;
                lightingProfileApplier.ApplyProfile();
            }

            if (materialLibrary != null)
                materialLibrary.EnsureRuntimeDefaults();

            if (fogShouldStaySubtle && lightingProfile != null && RenderSettings.fogDensity > lightingProfile.fogDensity)
                RenderSettings.fogDensity = lightingProfile.fogDensity;
        }
    }
}
