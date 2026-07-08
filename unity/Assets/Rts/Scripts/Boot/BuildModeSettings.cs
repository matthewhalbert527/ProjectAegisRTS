using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class BuildModeSettings : MonoBehaviour
    {
        public bool showDebugPanelsByDefault;
        public bool startInBootMenu = true;
        public bool defaultCleanHud = true;
        public bool enableDeveloperHotkeys = true;
    }
}
