using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Stage32/Terrain Set Dressing Library")]
    public sealed class TerrainSetDressingLibrary : ScriptableObject
    {
        public List<TerrainSetDressingProfile> profiles = new List<TerrainSetDressingProfile>();
        public string defaultProfileId = "stage32_player_facing";

        readonly Dictionary<string, TerrainSetDressingProfile> byId = new Dictionary<string, TerrainSetDressingProfile>();
        bool initialized;

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            byId.Clear();
            if (profiles == null)
                profiles = new List<TerrainSetDressingProfile>();

            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];
                if (profile != null && !string.IsNullOrEmpty(profile.profileId) && !byId.ContainsKey(profile.profileId))
                    byId.Add(profile.profileId, profile);
            }
        }

        public void RebuildLookup()
        {
            initialized = false;
            EnsureInitialized();
        }

        public TerrainSetDressingProfile GetProfile(string profileId)
        {
            EnsureInitialized();
            TerrainSetDressingProfile profile;
            return !string.IsNullOrEmpty(profileId) && byId.TryGetValue(profileId, out profile) ? profile : null;
        }

        public TerrainSetDressingProfile GetDefaultProfile()
        {
            EnsureInitialized();
            var profile = GetProfile(defaultProfileId);
            if (profile != null)
                return profile;
            return profiles != null && profiles.Count > 0 ? profiles[0] : null;
        }
    }
}
