using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    [CreateAssetMenu(menuName = "ProjectAegisRTS/Stage32/Terrain Piece Material Library")]
    public sealed class Stage32TerrainPieceMaterialLibrary : ScriptableObject
    {
        public List<Stage32TerrainPieceMaterialProfile> profiles = new List<Stage32TerrainPieceMaterialProfile>();

        readonly Dictionary<string, Stage32TerrainPieceMaterialProfile> byId = new Dictionary<string, Stage32TerrainPieceMaterialProfile>();
        bool initialized;

        public void RebuildLookup()
        {
            initialized = false;
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            byId.Clear();
            if (profiles == null)
                profiles = new List<Stage32TerrainPieceMaterialProfile>();

            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];
                if (profile == null || string.IsNullOrEmpty(profile.profileId) || byId.ContainsKey(profile.profileId))
                    continue;
                byId.Add(profile.profileId, profile);
            }
        }

        public Material MaterialFor(string profileId)
        {
            EnsureInitialized();
            Stage32TerrainPieceMaterialProfile profile;
            return !string.IsNullOrEmpty(profileId) && byId.TryGetValue(profileId, out profile) ? profile.material : null;
        }

        public bool Contains(string profileId)
        {
            EnsureInitialized();
            return !string.IsNullOrEmpty(profileId) && byId.ContainsKey(profileId);
        }
    }

    [Serializable]
    public sealed class Stage32TerrainPieceMaterialProfile
    {
        public string profileId;
        public string displayName;
        public Material material;
        public Color baseColor = Color.white;
        public Color accentColor = Color.gray;
        public string notes;
    }
}
