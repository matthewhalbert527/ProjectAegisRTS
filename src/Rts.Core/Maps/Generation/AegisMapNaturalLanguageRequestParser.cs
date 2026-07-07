using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapNaturalLanguageParseResult
    {
        public AegisMapGenerationRequest Request { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; }

        public AegisMapNaturalLanguageParseResult(AegisMapGenerationRequest request, IReadOnlyList<string> warnings)
        {
            Request = request;
            Warnings = warnings ?? new string[0];
        }
    }

    public sealed class AegisMapNaturalLanguageRequestParser
    {
        public AegisMapNaturalLanguageParseResult Parse(string text)
        {
            var request = AegisMapGenerationRequest.CreateDefault();
            request.HasExplicitSeed = false;
            request.PromptText = text ?? string.Empty;
            var warnings = new List<string>();
            var normalized = Normalize(text);

            if (string.IsNullOrEmpty(normalized))
                warnings.Add("PromptEmpty:Using default balanced small map settings.");

            ParseSize(normalized, request);
            ParsePlayerCount(normalized, request);
            ParseBiome(normalized, request);
            ParseDensities(normalized, request);
            ParseWater(normalized, request);
            ParseSymmetry(normalized, request);
            ParseGameplayProfile(normalized, request);
            ParseSeed(normalized, request);
            ParseRegeneration(normalized, request);

            if (!ContainsKnownMapWord(normalized))
                warnings.Add("PromptUnknown:No recognized map keywords were found; defaults were used.");

            request.Normalize();
            return new AegisMapNaturalLanguageParseResult(request, warnings);
        }

        static void ParseSize(string text, AegisMapGenerationRequest request)
        {
            if (Contains(text, "small"))
                request.SizePreset = AegisMapGenerationPreset.Small;
            if (Contains(text, "medium"))
                request.SizePreset = AegisMapGenerationPreset.Medium;
            if (Contains(text, "large"))
                request.SizePreset = AegisMapGenerationPreset.Large;

            var match = Regex.Match(text, @"\b(?<w>\d{2,3})\s*(x|by)\s*(?<h>\d{2,3})\b");
            if (match.Success)
            {
                request.CustomWidth = int.Parse(match.Groups["w"].Value);
                request.CustomHeight = int.Parse(match.Groups["h"].Value);
                request.SizePreset = AegisMapGenerationRequest.PresetForSize(request.CustomWidth, request.CustomHeight);
            }
        }

        static void ParsePlayerCount(string text, AegisMapGenerationRequest request)
        {
            var match = Regex.Match(text, @"\b(?<count>[2468])\s*(player|players)\b");
            if (match.Success)
                request.PlayerCount = int.Parse(match.Groups["count"].Value);
            else if (Contains(text, "four player") || Contains(text, "4p"))
                request.PlayerCount = 4;
            else if (Contains(text, "six player") || Contains(text, "6p"))
                request.PlayerCount = 6;
            else if (Contains(text, "eight player") || Contains(text, "8p"))
                request.PlayerCount = 8;
        }

        static void ParseBiome(string text, AegisMapGenerationRequest request)
        {
            if (Contains(text, "desert"))
                request.Biome = AegisMapBiome.Desert;
            else if (Contains(text, "tundra") || Contains(text, "snow"))
                request.Biome = AegisMapBiome.Tundra;
            else if (Contains(text, "volcanic") || Contains(text, "lava"))
                request.Biome = AegisMapBiome.Volcanic;
            else if (Contains(text, "rocky"))
                request.Biome = AegisMapBiome.Rocky;
            else if (Contains(text, "forest") || Contains(text, "woodland"))
                request.Biome = AegisMapBiome.Forest;
            else if (Contains(text, "wasteland"))
                request.Biome = AegisMapBiome.Wasteland;
            else if (Contains(text, "grass") || Contains(text, "grassland"))
                request.Biome = AegisMapBiome.Grassland;
        }

        static void ParseDensities(string text, AegisMapGenerationRequest request)
        {
            request.ResourceDensity = ParseIntensityNear(text, "resource", request.ResourceDensity);
            request.ResourceDensity = ParseIntensityNear(text, "ore", request.ResourceDensity);
            if (Contains(text, "lots of ore") || Contains(text, "rich ore") || Contains(text, "many resources"))
                request.ResourceDensity = AegisMapIntensity.High;
            if (Contains(text, "low resources") || Contains(text, "scarce resources"))
                request.ResourceDensity = AegisMapIntensity.Low;

            request.CliffDensity = ParseIntensityNear(text, "cliff", request.CliffDensity);
            request.Rockiness = ParseIntensityNear(text, "rockiness", request.Rockiness);
            request.Rockiness = ParseIntensityNear(text, "rocky", request.Rockiness);
            if (Contains(text, "many choke"))
                request.CliffDensity = AegisMapIntensity.High;
        }

        static void ParseWater(string text, AegisMapGenerationRequest request)
        {
            if (Contains(text, "no water") || Contains(text, "dry"))
                request.WaterAmount = AegisMapWaterAmount.None;
            else if (Contains(text, "low water") || Contains(text, "little water"))
                request.WaterAmount = AegisMapWaterAmount.Low;
            else if (Contains(text, "medium water"))
                request.WaterAmount = AegisMapWaterAmount.Medium;
            else if (Contains(text, "high water") || Contains(text, "islands"))
                request.WaterAmount = AegisMapWaterAmount.High;
        }

        static void ParseSymmetry(string text, AegisMapGenerationRequest request)
        {
            if (Contains(text, "no symmetry") || Contains(text, "asymmetric"))
                request.Symmetry = AegisMapSymmetryMode.None;
            else if (Contains(text, "vertical symmetry"))
                request.Symmetry = AegisMapSymmetryMode.Vertical;
            else if (Contains(text, "rotational"))
                request.Symmetry = AegisMapSymmetryMode.Rotational;
            else if (Contains(text, "radial"))
                request.Symmetry = AegisMapSymmetryMode.Radial;
            else if (Contains(text, "horizontal") || Contains(text, "balanced"))
                request.Symmetry = AegisMapSymmetryMode.Horizontal;
        }

        static void ParseGameplayProfile(string text, AegisMapGenerationRequest request)
        {
            if (Contains(text, "open"))
                request.GameplayProfile = AegisMapGameplayProfile.Open;
            if (Contains(text, "choke"))
                request.GameplayProfile = AegisMapGameplayProfile.Chokepoint;
            if (Contains(text, "defensive"))
                request.GameplayProfile = AegisMapGameplayProfile.Defensive;
            if (Contains(text, "resource rich") || Contains(text, "resource-rich") || Contains(text, "rich"))
                request.GameplayProfile = AegisMapGameplayProfile.ResourceRich;
            if (Contains(text, "scarce"))
                request.GameplayProfile = AegisMapGameplayProfile.Scarce;
            if (Contains(text, "tournament"))
                request.GameplayProfile = AegisMapGameplayProfile.Tournament;
        }

        static void ParseSeed(string text, AegisMapGenerationRequest request)
        {
            var match = Regex.Match(text, @"\bseed\s*(?<seed>-?\d+)\b");
            if (!match.Success)
                return;

            int seed;
            if (int.TryParse(match.Groups["seed"].Value, out seed))
            {
                request.Seed = seed;
                request.HasExplicitSeed = true;
            }
        }

        static void ParseRegeneration(string text, AegisMapGenerationRequest request)
        {
            if (Contains(text, "no regen") || Contains(text, "no regeneration"))
                request.OreRegenerationEnabled = false;
            if (Contains(text, "regenerating ore") || Contains(text, "ore regeneration") || Contains(text, "regenerates"))
                request.OreRegenerationEnabled = true;
        }

        static AegisMapIntensity ParseIntensityNear(string text, string keyword, AegisMapIntensity fallback)
        {
            if (!Contains(text, keyword))
                return fallback;

            var window = WindowAround(text, keyword);
            if (Contains(window, "very high") || Contains(window, "lots") || Contains(window, "many"))
                return AegisMapIntensity.VeryHigh;
            if (Contains(window, "extreme"))
                return AegisMapIntensity.Extreme;
            if (Contains(window, "high"))
                return AegisMapIntensity.High;
            if (Contains(window, "medium"))
                return AegisMapIntensity.Medium;
            if (Contains(window, "very low"))
                return AegisMapIntensity.VeryLow;
            if (Contains(window, "low") || Contains(window, "little"))
                return AegisMapIntensity.Low;
            if (Contains(window, "none") || Contains(window, "no "))
                return AegisMapIntensity.None;

            return fallback;
        }

        static string WindowAround(string text, string keyword)
        {
            var index = text.IndexOf(keyword, StringComparison.Ordinal);
            if (index < 0)
                return text;
            var start = index - 24;
            if (start < 0)
                start = 0;
            var length = keyword.Length + 48;
            if (start + length > text.Length)
                length = text.Length - start;
            return text.Substring(start, length);
        }

        static bool ContainsKnownMapWord(string text)
        {
            return Contains(text, "map") || Contains(text, "small") || Contains(text, "medium") || Contains(text, "large") ||
                Contains(text, "ore") || Contains(text, "resource") || Contains(text, "cliff") || Contains(text, "rock") ||
                Contains(text, "forest") || Contains(text, "desert") || Contains(text, "player") || Contains(text, "seed");
        }

        static string Normalize(string text)
        {
            return (text ?? string.Empty).ToLowerInvariant().Replace(",", " ").Replace(".", " ").Replace(";", " ");
        }

        static bool Contains(string text, string value)
        {
            return text.IndexOf(value, StringComparison.Ordinal) >= 0;
        }
    }
}
