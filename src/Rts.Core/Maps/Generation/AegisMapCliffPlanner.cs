using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapCliffPlanner
    {
        public bool ShouldPlaceCliff(AegisMapGenerationProfile profile, int seed, int x, int y)
        {
            return AegisMapTerrainPlanner.LayeredNoise(seed ^ 0x735C9, x, y, 17, 9, 4) >= profile.CliffNoiseThreshold;
        }

        public void ApplyCliff(TerrainKind[] terrain, int width, int x, int y)
        {
            terrain[y * width + x] = TerrainKind.Cliff;
        }
    }
}
