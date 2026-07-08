using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapRockinessPlanner
    {
        public bool ShouldPlaceRock(AegisMapGenerationProfile profile, int seed, int x, int y)
        {
            return AegisMapTerrainPlanner.LayeredNoise(seed ^ 0x4935B, x, y, 8, 5, 3) >= profile.RockNoiseThreshold;
        }

        public void ApplyRock(TerrainKind[] terrain, int width, int x, int y)
        {
            if (terrain[y * width + x] == TerrainKind.Clear)
                terrain[y * width + x] = TerrainKind.Rough;
        }
    }
}
