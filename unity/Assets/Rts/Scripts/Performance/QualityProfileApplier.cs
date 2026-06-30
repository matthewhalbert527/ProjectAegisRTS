using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class QualityProfileApplier : MonoBehaviour
    {
        public PerformanceBudgetLibrary budgetLibrary;
        public string selectedProfileId = "quest";
        public bool applyOnStart = true;
        public bool applyQualitySettingsInEditMode;

        public string AppliedProfileId { get; private set; }
        public int AppliedTargetFrameRate { get; private set; }

        void Start()
        {
            if (applyOnStart)
                ApplySelectedProfile();
        }

        public void Initialize(PerformanceBudgetLibrary library)
        {
            budgetLibrary = library;
        }

        public void ApplySelectedProfile()
        {
            var profile = budgetLibrary == null ? null : budgetLibrary.GetProfile(selectedProfileId);
            ApplyProfile(profile);
        }

        public void ApplyProfile(PerformanceBudgetProfile profile)
        {
            if (profile == null)
                return;

            if (Application.isPlaying || applyQualitySettingsInEditMode)
            {
                if (profile.qualityLevel >= 0 && profile.qualityLevel < QualitySettings.names.Length)
                    QualitySettings.SetQualityLevel(profile.qualityLevel, false);

                if (profile.preferVSyncOff)
                    QualitySettings.vSyncCount = 0;

                QualitySettings.antiAliasing = Mathf.Max(0, profile.antiAliasing);
                QualitySettings.pixelLightCount = Mathf.Max(0, profile.pixelLightCount);
                QualitySettings.shadowDistance = Mathf.Max(0f, profile.shadowDistance);
                QualitySettings.lodBias = Mathf.Max(0.1f, profile.lodBias);
            }
            Application.targetFrameRate = profile.targetFrameRate;
            AppliedProfileId = profile.profileId;
            AppliedTargetFrameRate = profile.targetFrameRate;
        }
    }
}
