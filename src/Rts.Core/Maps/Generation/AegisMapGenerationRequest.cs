using System;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapGenerationRequest
    {
        public AegisMapGenerationPreset SizePreset { get; set; }
        public int CustomWidth { get; set; }
        public int CustomHeight { get; set; }
        public int PlayerCount { get; set; }
        public AegisMapBiome Biome { get; set; }
        public AegisMapIntensity ResourceDensity { get; set; }
        public AegisMapIntensity CliffDensity { get; set; }
        public AegisMapIntensity Rockiness { get; set; }
        public AegisMapWaterAmount WaterAmount { get; set; }
        public AegisMapSymmetryMode Symmetry { get; set; }
        public bool HasExplicitSeed { get; set; }
        public int Seed { get; set; }
        public AegisMapGameplayProfile GameplayProfile { get; set; }
        public bool OreRegenerationEnabled { get; set; }
        public int OreRegenerationRatePerTick { get; set; }
        public int OreRegenerationDelayTicks { get; set; }
        public string PromptText { get; set; }

        public AegisMapGenerationRequest()
        {
            SizePreset = AegisMapGenerationPreset.Small;
            CustomWidth = AegisMapDocument.SmallMapWidth;
            CustomHeight = AegisMapDocument.SmallMapHeight;
            PlayerCount = 2;
            Biome = AegisMapBiome.Grassland;
            ResourceDensity = AegisMapIntensity.Medium;
            CliffDensity = AegisMapIntensity.Low;
            Rockiness = AegisMapIntensity.Low;
            WaterAmount = AegisMapWaterAmount.None;
            Symmetry = AegisMapSymmetryMode.Horizontal;
            HasExplicitSeed = true;
            Seed = 1337;
            GameplayProfile = AegisMapGameplayProfile.Balanced;
            OreRegenerationEnabled = true;
            OreRegenerationRatePerTick = 2;
            OreRegenerationDelayTicks = 60;
            PromptText = string.Empty;
        }

        public static AegisMapGenerationRequest CreateDefault()
        {
            return new AegisMapGenerationRequest();
        }

        public int ResolveWidth()
        {
            if (SizePreset == AegisMapGenerationPreset.Small)
                return AegisMapDocument.SmallMapWidth;
            if (SizePreset == AegisMapGenerationPreset.Medium)
                return AegisMapDocument.MediumMapWidth;
            if (SizePreset == AegisMapGenerationPreset.Large)
                return AegisMapDocument.LargeMapWidth;
            return CustomWidth;
        }

        public int ResolveHeight()
        {
            if (SizePreset == AegisMapGenerationPreset.Small)
                return AegisMapDocument.SmallMapHeight;
            if (SizePreset == AegisMapGenerationPreset.Medium)
                return AegisMapDocument.MediumMapHeight;
            if (SizePreset == AegisMapGenerationPreset.Large)
                return AegisMapDocument.LargeMapHeight;
            return CustomHeight;
        }

        public int ResolveSeed()
        {
            if (HasExplicitSeed)
                return Seed;

            unchecked
            {
                var h = 2166136261u;
                h = Mix(h, ResolveWidth());
                h = Mix(h, ResolveHeight());
                h = Mix(h, PlayerCount);
                h = Mix(h, (int)Biome);
                h = Mix(h, (int)ResourceDensity);
                h = Mix(h, (int)CliffDensity);
                h = Mix(h, (int)Rockiness);
                h = Mix(h, (int)WaterAmount);
                h = Mix(h, (int)Symmetry);
                h = Mix(h, (int)GameplayProfile);
                return (int)(h & 0x7FFFFFFF);
            }
        }

        static uint Mix(uint hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 16777619u;
                return hash;
            }
        }

        public void Normalize()
        {
            PlayerCount = NormalizePlayerCount(PlayerCount);
            if (OreRegenerationRatePerTick < 0)
                OreRegenerationRatePerTick = 0;
            if (OreRegenerationDelayTicks < 0)
                OreRegenerationDelayTicks = 0;
        }

        public static int NormalizePlayerCount(int playerCount)
        {
            if (playerCount <= 2)
                return 2;
            if (playerCount <= 4)
                return 4;
            if (playerCount <= 6)
                return 6;
            return 8;
        }

        public static AegisMapGenerationPreset PresetForSize(int width, int height)
        {
            if (width == AegisMapDocument.SmallMapWidth && height == AegisMapDocument.SmallMapHeight)
                return AegisMapGenerationPreset.Small;
            if (width == AegisMapDocument.MediumMapWidth && height == AegisMapDocument.MediumMapHeight)
                return AegisMapGenerationPreset.Medium;
            if (width == AegisMapDocument.LargeMapWidth && height == AegisMapDocument.LargeMapHeight)
                return AegisMapGenerationPreset.Large;
            return AegisMapGenerationPreset.Custom;
        }
    }
}
