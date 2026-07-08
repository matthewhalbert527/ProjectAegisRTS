using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class PlayerDisplaySettingsInitializer : MonoBehaviour
    {
        public PlayerDisplaySettings settings;
        public bool applyOnAwake = true;

        void Awake()
        {
            if (applyOnAwake)
                Apply();
        }

        public void Apply()
        {
            if (settings == null)
                settings = FindAnyObjectByType<PlayerDisplaySettings>();
            if (settings == null)
                settings = gameObject.AddComponent<PlayerDisplaySettings>();

            settings.ApplyStartupDisplaySettings();
        }
    }
}
