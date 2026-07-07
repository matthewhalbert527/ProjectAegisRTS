using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapTerrainPlanner
    {
        public TerrainKind PlanTerrain(AegisMapGenerationRequest request, AegisMapGenerationProfile profile, int seed, int x, int y)
        {
            var waterNoise = LayeredNoise(seed ^ 0x51C51, x, y, 16, 8, 4);
            if (waterNoise >= profile.WaterNoiseThreshold)
                return TerrainKind.Water;

            var forestNoise = LayeredNoise(seed ^ 0x2F7D1, x, y, 13, 7, 3);
            if (forestNoise >= profile.ForestNoiseThreshold)
                return TerrainKind.Forest;

            var roughNoise = LayeredNoise(seed ^ 0x1A3B7, x, y, 11, 5, 2);
            if (roughNoise >= profile.RoughNoiseThreshold)
                return TerrainKind.Rough;

            if (request.Biome == AegisMapBiome.Desert && roughNoise > 740)
                return TerrainKind.Rough;
            if (request.Biome == AegisMapBiome.Tundra && forestNoise > 930)
                return TerrainKind.Forest;

            return TerrainKind.Clear;
        }

        public static int LayeredNoise(int seed, int x, int y, int scaleA, int scaleB, int scaleC)
        {
            return (ValueNoise(seed, x, y, scaleA) * 5 +
                ValueNoise(seed ^ 0x4A39, x, y, scaleB) * 3 +
                ValueNoise(seed ^ 0x7F21, x, y, scaleC) * 2) / 10;
        }

        static int ValueNoise(int seed, int x, int y, int scale)
        {
            scale = scale < 1 ? 1 : scale;
            var x0 = x / scale;
            var y0 = y / scale;
            var fx = x % scale;
            var fy = y % scale;

            var a = HashToThousand(seed, x0, y0);
            var b = HashToThousand(seed, x0 + 1, y0);
            var c = HashToThousand(seed, x0, y0 + 1);
            var d = HashToThousand(seed, x0 + 1, y0 + 1);
            var top = Lerp(a, b, fx, scale);
            var bottom = Lerp(c, d, fx, scale);
            return Lerp(top, bottom, fy, scale);
        }

        public static int HashToThousand(int seed, int x, int y)
        {
            unchecked
            {
                var h = (uint)seed;
                h ^= (uint)(x * 374761393);
                h = (h << 13) | (h >> 19);
                h ^= (uint)(y * 668265263);
                h *= 1274126177u;
                h ^= h >> 16;
                return (int)(h % 1000u);
            }
        }

        static int Lerp(int a, int b, int numerator, int denominator)
        {
            return a + (b - a) * numerator / (denominator < 1 ? 1 : denominator);
        }
    }
}
