namespace ProjectAegisRTS.Maps.Generation
{
    public enum AegisMapGenerationPreset
    {
        Custom,
        Small,
        Medium,
        Large
    }

    public enum AegisMapBiome
    {
        Grassland,
        Desert,
        Tundra,
        Volcanic,
        Rocky,
        Forest,
        Wasteland
    }

    public enum AegisMapIntensity
    {
        None,
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Extreme
    }

    public enum AegisMapWaterAmount
    {
        None,
        Low,
        Medium,
        High
    }

    public enum AegisMapSymmetryMode
    {
        None,
        Horizontal,
        Vertical,
        Rotational,
        Radial
    }

    public enum AegisMapGameplayProfile
    {
        Open,
        Balanced,
        Chokepoint,
        Defensive,
        ResourceRich,
        Scarce,
        Tournament
    }
}
