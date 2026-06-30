using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class PerformanceBudgetLibrary : MonoBehaviour
    {
        public PerformanceBudgetProfile[] profiles;
        public PerformanceBudgetProfile defaultProfile;

        public int ProfileCount { get; private set; }

        public void EnsureInitialized()
        {
            ProfileCount = profiles == null ? 0 : profiles.Length;
            if (defaultProfile == null && ProfileCount > 0)
                defaultProfile = profiles[0];
        }

        public PerformanceBudgetProfile GetProfile(string profileId)
        {
            EnsureInitialized();
            if (profiles == null)
                return defaultProfile;

            for (var i = 0; i < profiles.Length; i++)
            {
                var profile = profiles[i];
                if (profile != null && profile.profileId == profileId)
                    return profile;
            }

            return defaultProfile;
        }
    }
}
