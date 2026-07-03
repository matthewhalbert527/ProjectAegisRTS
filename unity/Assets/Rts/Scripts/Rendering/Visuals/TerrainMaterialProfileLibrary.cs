using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class TerrainMaterialProfileLibrary : MonoBehaviour
    {
        public List<TerrainMaterialProfile> profiles = new List<TerrainMaterialProfile>();
        public BattlefieldMaterialLibrary materialLibrary;

        readonly Dictionary<string, TerrainMaterialProfile> byKind = new Dictionary<string, TerrainMaterialProfile>();
        bool initialized;

        public int ProfileCount
        {
            get
            {
                EnsureInitialized();
                return byKind.Count;
            }
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            byKind.Clear();
            if (profiles == null)
                profiles = new List<TerrainMaterialProfile>();
            if (profiles.Count == 0)
                CreateRuntimeDefaults();

            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];
                if (profile != null && !string.IsNullOrEmpty(profile.terrainKind) && !byKind.ContainsKey(profile.terrainKind))
                    byKind.Add(profile.terrainKind, profile);
            }
        }

        public void RebuildLookup()
        {
            initialized = false;
            EnsureInitialized();
        }

        public TerrainMaterialProfile GetProfile(string terrainKind)
        {
            EnsureInitialized();
            TerrainMaterialProfile profile;
            return !string.IsNullOrEmpty(terrainKind) && byKind.TryGetValue(terrainKind, out profile) ? profile : null;
        }

        public IReadOnlyList<TerrainMaterialProfile> GetProfiles()
        {
            EnsureInitialized();
            return profiles;
        }

        void CreateRuntimeDefaults()
        {
            if (materialLibrary == null)
                materialLibrary = GetComponent<BattlefieldMaterialLibrary>();
            if (materialLibrary == null)
                materialLibrary = gameObject.AddComponent<BattlefieldMaterialLibrary>();
            materialLibrary.EnsureRuntimeDefaults();

            AddRuntimeProfile("GrassDirt", "Grass / Dirt Field", materialLibrary.grassDirt, new Color(0.24f, 0.34f, 0.22f, 1f), new Color(0.44f, 0.38f, 0.25f, 1f), false, false, true, "Mixed grass and dirt breaks up the board surface while keeping the fine placement grid legible.");
            AddRuntimeProfile("CompactedBase", "Compacted Base Ground", materialLibrary.compactedBaseGround, new Color(0.30f, 0.29f, 0.24f, 1f), new Color(0.45f, 0.43f, 0.36f, 1f), false, false, true, "Base ground gives buildings a grounded construction zone without hiding placement footprints.");
            AddRuntimeProfile("ConcretePad", "Concrete Pad", materialLibrary.concretePad, new Color(0.46f, 0.48f, 0.43f, 1f), new Color(0.70f, 0.69f, 0.62f, 1f), false, false, true, "Concrete pads identify buildable hardstand tiles and show actor footprint scale.");
            AddRuntimeProfile("RoadPath", "Road / Path", materialLibrary.roadPath, new Color(0.25f, 0.24f, 0.21f, 1f), new Color(0.50f, 0.46f, 0.36f, 1f), false, false, false, "Road paths guide early scouting and enemy pressure direction.");
            AddRuntimeProfile("ResourceField", "Resource Field", materialLibrary.resourceField, new Color(0.21f, 0.49f, 0.40f, 1f), new Color(0.75f, 0.68f, 0.32f, 1f), false, true, false, "Resource tiles use a distinct mineral tint so harvestable space reads from the main camera.");
            AddRuntimeProfile("RockBlocked", "Rock / Blocked Terrain", materialLibrary.rockBlocked, new Color(0.27f, 0.26f, 0.24f, 1f), new Color(0.47f, 0.45f, 0.39f, 1f), true, false, false, "Blocked terrain uses dark rock clusters and high contrast edges.");
            AddRuntimeProfile("Water", "Water", materialLibrary.water, new Color(0.10f, 0.22f, 0.31f, 1f), new Color(0.26f, 0.45f, 0.54f, 1f), true, false, false, "Water is visually separated from ground pathing and remains Quest-safe.");
            AddRuntimeProfile("FogExplored", "Fog / Explored Tint", materialLibrary.fogExplored, new Color(0.16f, 0.19f, 0.19f, 0.82f), new Color(0.28f, 0.34f, 0.34f, 1f), false, false, false, "Explored fog is muted so player-owned actors and placement previews stay readable.");
        }

        void AddRuntimeProfile(string kind, string label, Material material, Color baseTint, Color accentTint, bool blocked, bool resource, bool placement, string notes)
        {
            var profile = ScriptableObject.CreateInstance<TerrainMaterialProfile>();
            profile.hideFlags = HideFlags.DontSave;
            profile.Configure(kind, label, material, baseTint, accentTint, blocked, resource, placement, notes);
            profiles.Add(profile);
        }
    }
}
