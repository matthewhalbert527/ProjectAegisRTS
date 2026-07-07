namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapGenerationProfile
    {
        public int ResourceClustersPerPlayer { get; private set; }
        public int ExpansionResourceClusters { get; private set; }
        public int ResourceAmountPerCell { get; private set; }
        public int ResourceClusterRadius { get; private set; }
        public int BaseClearRadius { get; private set; }
        public int MinimumBuildPadsPerStart { get; private set; }
        public int CliffNoiseThreshold { get; private set; }
        public int RockNoiseThreshold { get; private set; }
        public int WaterNoiseThreshold { get; private set; }
        public int ForestNoiseThreshold { get; private set; }
        public int RoughNoiseThreshold { get; private set; }
        public int RegenerationRatePerTick { get; private set; }
        public int RegenerationDelayTicks { get; private set; }

        AegisMapGenerationProfile()
        {
        }

        public static AegisMapGenerationProfile FromRequest(AegisMapGenerationRequest request)
        {
            var profile = new AegisMapGenerationProfile();
            profile.ResourceClustersPerPlayer = 1 + DensityStep(request.ResourceDensity);
            profile.ExpansionResourceClusters = request.ResolveWidth() >= 200 ? 2 + DensityStep(request.ResourceDensity) : 1;
            profile.ResourceAmountPerCell = 180 + DensityStep(request.ResourceDensity) * 70;
            profile.ResourceClusterRadius = request.ResourceDensity == AegisMapIntensity.VeryHigh ? 4 : 3;
            profile.BaseClearRadius = request.ResolveWidth() >= 400 ? 14 : request.ResolveWidth() >= 200 ? 12 : 10;
            profile.MinimumBuildPadsPerStart = request.ResolveWidth() >= 400 ? 8 : 6;
            profile.CliffNoiseThreshold = ThresholdFor(request.CliffDensity, 995, 930, 865, 800, 735, 690);
            profile.RockNoiseThreshold = ThresholdFor(request.Rockiness, 995, 925, 850, 770, 690, 640);
            profile.WaterNoiseThreshold = WaterThresholdFor(request.WaterAmount);
            profile.ForestNoiseThreshold = ForestThresholdFor(request.Biome);
            profile.RoughNoiseThreshold = RoughThresholdFor(request.Biome);
            profile.RegenerationRatePerTick = request.OreRegenerationEnabled ? request.OreRegenerationRatePerTick : 0;
            profile.RegenerationDelayTicks = request.OreRegenerationEnabled ? request.OreRegenerationDelayTicks : 0;

            if (request.GameplayProfile == AegisMapGameplayProfile.ResourceRich)
            {
                profile.ResourceClustersPerPlayer += 1;
                profile.ExpansionResourceClusters += 2;
                profile.RegenerationRatePerTick += request.OreRegenerationEnabled ? 1 : 0;
            }
            else if (request.GameplayProfile == AegisMapGameplayProfile.Scarce)
            {
                profile.ResourceClustersPerPlayer = profile.ResourceClustersPerPlayer > 1 ? profile.ResourceClustersPerPlayer - 1 : 1;
                profile.ExpansionResourceClusters = profile.ExpansionResourceClusters > 1 ? profile.ExpansionResourceClusters - 1 : 1;
                profile.RegenerationDelayTicks += request.OreRegenerationEnabled ? 60 : 0;
            }
            else if (request.GameplayProfile == AegisMapGameplayProfile.Chokepoint)
            {
                profile.CliffNoiseThreshold -= 35;
                profile.RockNoiseThreshold -= 20;
            }
            else if (request.GameplayProfile == AegisMapGameplayProfile.Open)
            {
                profile.CliffNoiseThreshold += 45;
                profile.RockNoiseThreshold += 30;
            }

            if (profile.CliffNoiseThreshold < 600)
                profile.CliffNoiseThreshold = 600;
            if (profile.RockNoiseThreshold < 560)
                profile.RockNoiseThreshold = 560;

            return profile;
        }

        static int DensityStep(AegisMapIntensity intensity)
        {
            switch (intensity)
            {
                case AegisMapIntensity.VeryLow:
                    return 0;
                case AegisMapIntensity.Low:
                    return 1;
                case AegisMapIntensity.High:
                    return 3;
                case AegisMapIntensity.VeryHigh:
                case AegisMapIntensity.Extreme:
                    return 4;
                default:
                    return 2;
            }
        }

        static int ThresholdFor(AegisMapIntensity intensity, int none, int low, int medium, int high, int veryHigh, int extreme)
        {
            switch (intensity)
            {
                case AegisMapIntensity.None:
                    return none;
                case AegisMapIntensity.Low:
                case AegisMapIntensity.VeryLow:
                    return low;
                case AegisMapIntensity.High:
                    return high;
                case AegisMapIntensity.VeryHigh:
                    return veryHigh;
                case AegisMapIntensity.Extreme:
                    return extreme;
                default:
                    return medium;
            }
        }

        static int WaterThresholdFor(AegisMapWaterAmount amount)
        {
            switch (amount)
            {
                case AegisMapWaterAmount.Low:
                    return 950;
                case AegisMapWaterAmount.Medium:
                    return 895;
                case AegisMapWaterAmount.High:
                    return 835;
                default:
                    return 1001;
            }
        }

        static int ForestThresholdFor(AegisMapBiome biome)
        {
            if (biome == AegisMapBiome.Forest)
                return 690;
            if (biome == AegisMapBiome.Grassland)
                return 865;
            if (biome == AegisMapBiome.Tundra)
                return 920;
            return 1001;
        }

        static int RoughThresholdFor(AegisMapBiome biome)
        {
            if (biome == AegisMapBiome.Desert)
                return 805;
            if (biome == AegisMapBiome.Rocky || biome == AegisMapBiome.Volcanic || biome == AegisMapBiome.Wasteland)
                return 690;
            return 860;
        }
    }
}
