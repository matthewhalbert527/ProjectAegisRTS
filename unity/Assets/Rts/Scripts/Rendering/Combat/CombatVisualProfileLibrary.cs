using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class CombatVisualProfileLibrary : MonoBehaviour
    {
        public List<CombatVisualProfile> profiles = new List<CombatVisualProfile>();
        readonly Dictionary<string, CombatVisualProfile> lookup = new Dictionary<string, CombatVisualProfile>();

        public int ProfileCount { get; private set; }

        public void EnsureInitialized()
        {
            RebuildLookup();
        }

        public void RebuildLookup()
        {
            lookup.Clear();
            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];
                if (profile == null || string.IsNullOrEmpty(profile.profileId))
                    continue;
                lookup[profile.profileId] = profile;
            }

            ProfileCount = lookup.Count;
        }

        public CombatVisualProfile GetProfile(string profileId)
        {
            EnsureInitialized();
            CombatVisualProfile profile;
            if (!string.IsNullOrEmpty(profileId) && lookup.TryGetValue(profileId, out profile))
                return profile;
            if (lookup.TryGetValue("impact_placeholder", out profile))
                return profile;
            return profiles.Count == 0 ? null : profiles[0];
        }

        public CombatVisualProfile GetProfileForWeapon(string weaponId)
        {
            EnsureInitialized();
            CombatVisualProfile profile;
            if (!string.IsNullOrEmpty(weaponId) && lookup.TryGetValue(weaponId, out profile))
                return profile;
            if (!string.IsNullOrEmpty(weaponId) && weaponId.Contains("rocket") && lookup.TryGetValue("rocket_placeholder", out profile))
                return profile;
            if (!string.IsNullOrEmpty(weaponId) && weaponId.Contains("bullet") && lookup.TryGetValue("rifle_bullet", out profile))
                return profile;
            if (!string.IsNullOrEmpty(weaponId) && weaponId.Contains("tower") && lookup.TryGetValue("tower_shell", out profile))
                return profile;
            if (lookup.TryGetValue("tank_shell", out profile))
                return profile;
            return GetProfile("impact_placeholder");
        }
    }
}
