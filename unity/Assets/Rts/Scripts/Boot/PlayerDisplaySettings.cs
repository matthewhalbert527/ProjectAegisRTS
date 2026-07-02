using System;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class PlayerDisplaySettings : MonoBehaviour
    {
        public const string WidthKey = "ProjectAegisRTS.Display.Width";
        public const string HeightKey = "ProjectAegisRTS.Display.Height";
        public const string FullscreenModeKey = "ProjectAegisRTS.Display.FullscreenMode";

        public int defaultWindowWidth = 1600;
        public int defaultWindowHeight = 900;
        public int minimumWindowWidth = 1280;
        public int minimumWindowHeight = 720;
        public FullScreenMode preferredFullscreenMode = FullScreenMode.Windowed;
        public bool preserveValidPlayerPreferences = true;
        public bool applyInEditor;
        public bool logStartupDisplayMetrics = true;

        bool appliedStartupSettings;

        public bool AppliedStartupSettings
        {
            get { return appliedStartupSettings; }
        }

        public void ApplyStartupDisplaySettings()
        {
            if (!ShouldApplyRuntimeResolution())
            {
                LogDisplayMetrics("skipped", 0, 0, preferredFullscreenMode, false, "non-player");
                return;
            }

            var resetRequested = HasArgument("-aegis-reset-display") || HasArgument("-reset-display-prefs");
            if (resetRequested)
                ClearSavedPreference();

            var savedWidth = 0;
            var savedHeight = 0;
            var savedMode = FullScreenMode.Windowed;
            var hasSaved = preserveValidPlayerPreferences && TryLoadSavedPreference(out savedWidth, out savedHeight, out savedMode);

            var targetWidth = hasSaved ? savedWidth : defaultWindowWidth;
            var targetHeight = hasSaved ? savedHeight : defaultWindowHeight;
            var targetMode = hasSaved ? savedMode : preferredFullscreenMode;

            if (HasArgument("-aegis-force-1080p") || HasArgument("-force-1920x1080"))
            {
                targetWidth = 1920;
                targetHeight = 1080;
            }

            if (HasArgument("-aegis-windowed"))
                targetMode = FullScreenMode.Windowed;
            if (HasArgument("-aegis-fullscreen") || HasArgument("-aegis-fullscreen-window"))
                targetMode = FullScreenMode.FullScreenWindow;

            targetWidth = Mathf.Max(targetWidth, minimumWindowWidth);
            targetHeight = Mathf.Max(targetHeight, minimumWindowHeight);

            var clampedCurrent = Screen.width < minimumWindowWidth || Screen.height < minimumWindowHeight;
            var explicitOverride = resetRequested || HasArgument("-aegis-force-1080p") || HasArgument("-force-1920x1080") || HasArgument("-aegis-windowed") || HasArgument("-aegis-fullscreen") || HasArgument("-aegis-fullscreen-window");
            var shouldApply = clampedCurrent || explicitOverride || hasSaved;

            if (shouldApply)
            {
                Screen.SetResolution(targetWidth, targetHeight, targetMode);
                appliedStartupSettings = true;
            }

            LogDisplayMetrics(shouldApply ? "applied" : "kept-current", targetWidth, targetHeight, targetMode, clampedCurrent, hasSaved ? "saved" : "default");
        }

        public void ApplyAndSaveDisplaySettings(int width, int height, FullScreenMode mode)
        {
            var safeWidth = Mathf.Max(width, minimumWindowWidth);
            var safeHeight = Mathf.Max(height, minimumWindowHeight);
            SavePreference(safeWidth, safeHeight, mode);

            if (ShouldApplyRuntimeResolution())
                Screen.SetResolution(safeWidth, safeHeight, mode);

            LogDisplayMetrics("options-apply", safeWidth, safeHeight, mode, width < minimumWindowWidth || height < minimumWindowHeight, "options");
        }

        public void ResetDisplayPreferencesAndApply()
        {
            ClearSavedPreference();
            if (ShouldApplyRuntimeResolution())
                Screen.SetResolution(defaultWindowWidth, defaultWindowHeight, preferredFullscreenMode);
            LogDisplayMetrics("options-reset", defaultWindowWidth, defaultWindowHeight, preferredFullscreenMode, false, "reset");
        }

        public bool TryGetSavedPreference(out int width, out int height, out FullScreenMode mode)
        {
            return TryLoadSavedPreference(out width, out height, out mode);
        }

        public static void SavePreference(int width, int height, FullScreenMode mode)
        {
            PlayerPrefs.SetInt(WidthKey, width);
            PlayerPrefs.SetInt(HeightKey, height);
            PlayerPrefs.SetInt(FullscreenModeKey, (int)mode);
            PlayerPrefs.Save();
        }

        public static void ClearSavedPreference()
        {
            PlayerPrefs.DeleteKey(WidthKey);
            PlayerPrefs.DeleteKey(HeightKey);
            PlayerPrefs.DeleteKey(FullscreenModeKey);
            PlayerPrefs.Save();
        }

        public static bool TryLoadSavedPreference(out int width, out int height, out FullScreenMode mode)
        {
            width = PlayerPrefs.GetInt(WidthKey, 0);
            height = PlayerPrefs.GetInt(HeightKey, 0);
            mode = (FullScreenMode)PlayerPrefs.GetInt(FullscreenModeKey, (int)FullScreenMode.Windowed);

            if (width < 1280 || height < 720)
                return false;

            if (!Enum.IsDefined(typeof(FullScreenMode), mode))
            {
                mode = FullScreenMode.Windowed;
                return false;
            }

            return true;
        }

        bool ShouldApplyRuntimeResolution()
        {
            if (Application.isEditor && !applyInEditor)
                return false;

            return Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.LinuxPlayer ||
                (Application.isEditor && applyInEditor);
        }

        void LogDisplayMetrics(string action, int requestedWidth, int requestedHeight, FullScreenMode requestedMode, bool clamped, string source)
        {
            if (!logStartupDisplayMetrics)
                return;

            var current = Screen.currentResolution;
            Debug.Log("[Stage21.5 Display] action=" + action +
                " source=" + source +
                " Screen.width=" + Screen.width +
                " Screen.height=" + Screen.height +
                " Screen.currentResolution=" + current.width + "x" + current.height + "@" + current.refreshRateRatio +
                " Screen.fullScreen=" + Screen.fullScreen +
                " Screen.fullScreenMode=" + Screen.fullScreenMode +
                " requestedResolution=" + requestedWidth + "x" + requestedHeight +
                " requestedFullscreenMode=" + requestedMode +
                " clampedDisplaySetting=" + clamped +
                " minimumResolution=" + minimumWindowWidth + "x" + minimumWindowHeight +
                " defaultResolution=" + defaultWindowWidth + "x" + defaultWindowHeight);
        }

        static bool HasArgument(string argument)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
                if (string.Equals(args[i], argument, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
