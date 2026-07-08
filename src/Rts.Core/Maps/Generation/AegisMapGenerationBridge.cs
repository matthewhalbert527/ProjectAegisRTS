using System;
using System.Collections.Generic;
using ProjectAegisRTS.Maps.Tiled;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapGenerationBridgeRequest
    {
        public string PromptText { get; set; }
        public string SizePreset { get; set; }
        public int CustomWidth { get; set; }
        public int CustomHeight { get; set; }
        public int PlayerCount { get; set; }
        public string Biome { get; set; }
        public string ResourceDensity { get; set; }
        public string CliffDensity { get; set; }
        public string Rockiness { get; set; }
        public string WaterAmount { get; set; }
        public string Symmetry { get; set; }
        public bool HasExplicitSeed { get; set; }
        public int Seed { get; set; }
        public string GameplayProfile { get; set; }
        public bool OreRegenerationEnabled { get; set; }
        public int OreRegenerationRatePerTick { get; set; }
        public int OreRegenerationDelayTicks { get; set; }

        public AegisMapGenerationBridgeRequest()
        {
            PromptText = string.Empty;
            SizePreset = "small";
            CustomWidth = AegisMapDocument.SmallMapWidth;
            CustomHeight = AegisMapDocument.SmallMapHeight;
            PlayerCount = 2;
            Biome = "grassland";
            ResourceDensity = "medium";
            CliffDensity = "low";
            Rockiness = "low";
            WaterAmount = "none";
            Symmetry = "horizontal";
            HasExplicitSeed = true;
            Seed = 1337;
            GameplayProfile = "balanced";
            OreRegenerationEnabled = true;
            OreRegenerationRatePerTick = 2;
            OreRegenerationDelayTicks = 60;
        }
    }

    public sealed class AegisMapGenerationBridgeResult
    {
        public bool Success { get; private set; }
        public string AegisMapJson { get; private set; }
        public string TiledJson { get; private set; }
        public string SummaryText { get; private set; }
        public int FairnessScore { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; }

        AegisMapGenerationBridgeResult(bool success, string aegisMapJson, string tiledJson, string summaryText, int fairnessScore, IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            Success = success;
            AegisMapJson = aegisMapJson ?? string.Empty;
            TiledJson = tiledJson ?? string.Empty;
            SummaryText = summaryText ?? string.Empty;
            FairnessScore = fairnessScore;
            Errors = errors ?? new string[0];
            Warnings = warnings ?? new string[0];
        }

        public static AegisMapGenerationBridgeResult Ok(string aegisMapJson, string tiledJson, string summaryText, int fairnessScore, IReadOnlyList<string> warnings)
        {
            return new AegisMapGenerationBridgeResult(true, aegisMapJson, tiledJson, summaryText, fairnessScore, new string[0], warnings);
        }

        public static AegisMapGenerationBridgeResult Fail(IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            return new AegisMapGenerationBridgeResult(false, string.Empty, string.Empty, string.Empty, 0, errors, warnings);
        }
    }

    public sealed class AegisMapGenerationBridge
    {
        public AegisMapGenerationBridgeResult Generate(AegisMapGenerationBridgeRequest bridgeRequest)
        {
            bridgeRequest = bridgeRequest ?? new AegisMapGenerationBridgeRequest();
            var warnings = new List<string>();
            var parser = new AegisMapNaturalLanguageRequestParser();
            var parseResult = parser.Parse(bridgeRequest.PromptText);
            warnings.AddRange(parseResult.Warnings);

            var request = parseResult.Request;
            ApplyOverrides(bridgeRequest, request, warnings);

            var result = new AegisMapGenerator().Generate(request);
            warnings.AddRange(result.Warnings);
            if (!result.Success)
                return AegisMapGenerationBridgeResult.Fail(result.Errors, warnings);

            var validation = new AegisMapDocumentValidator().Validate(result.Document);
            if (!validation.Success)
                return AegisMapGenerationBridgeResult.Fail(validation.Errors, warnings);
            warnings.AddRange(validation.Warnings);

            var aegisJson = AegisMapDocumentJson.Serialize(result.Document);
            var tiledJson = new AegisTiledMapExporter().ExportToJson(result.Document);
            var summaryText = BuildSummaryText(result);
            var fairnessScore = result.Balance == null ? 0 : result.Balance.FairnessScore;
            return AegisMapGenerationBridgeResult.Ok(aegisJson, tiledJson, summaryText, fairnessScore, warnings);
        }

        static void ApplyOverrides(AegisMapGenerationBridgeRequest source, AegisMapGenerationRequest request, List<string> warnings)
        {
            request.PromptText = source.PromptText ?? string.Empty;
            request.SizePreset = ParseEnum(source.SizePreset, request.SizePreset, "SizePreset", warnings);
            request.CustomWidth = source.CustomWidth;
            request.CustomHeight = source.CustomHeight;
            request.PlayerCount = source.PlayerCount;
            request.Biome = ParseEnum(source.Biome, request.Biome, "Biome", warnings);
            request.ResourceDensity = ParseEnum(source.ResourceDensity, request.ResourceDensity, "ResourceDensity", warnings);
            request.CliffDensity = ParseEnum(source.CliffDensity, request.CliffDensity, "CliffDensity", warnings);
            request.Rockiness = ParseEnum(source.Rockiness, request.Rockiness, "Rockiness", warnings);
            request.WaterAmount = ParseEnum(source.WaterAmount, request.WaterAmount, "WaterAmount", warnings);
            request.Symmetry = ParseEnum(source.Symmetry, request.Symmetry, "Symmetry", warnings);
            request.HasExplicitSeed = source.HasExplicitSeed;
            request.Seed = source.Seed;
            request.GameplayProfile = ParseEnum(source.GameplayProfile, request.GameplayProfile, "GameplayProfile", warnings);
            request.OreRegenerationEnabled = source.OreRegenerationEnabled;
            request.OreRegenerationRatePerTick = source.OreRegenerationRatePerTick;
            request.OreRegenerationDelayTicks = source.OreRegenerationDelayTicks;
            request.Normalize();
        }

        static T ParseEnum<T>(string value, T fallback, string fieldName, List<string> warnings) where T : struct
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            var normalized = value.Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty);
            T parsed;
            if (Enum.TryParse<T>(normalized, true, out parsed))
                return parsed;

            warnings.Add("BridgeOverrideInvalid:" + fieldName + ":" + value);
            return fallback;
        }

        static string BuildSummaryText(AegisMapGenerationResult result)
        {
            var summary = result.Summary;
            var balance = result.Balance;
            var buildability = result.Buildability;
            return "Size: " + summary.Width + "x" + summary.Height + "\n" +
                "Seed: " + summary.Seed + "\n" +
                "Players: " + summary.PlayerCount + "\n" +
                "Biome: " + summary.Biome + "\n" +
                "Resources: " + summary.ResourceDensity + " (" + summary.ResourceFieldCount + " cells, " + summary.TotalResourceAmount + " total)\n" +
                "Cliffs: " + summary.CliffDensity + " (" + summary.CliffBlockerCount + " cliff blockers)\n" +
                "Rockiness: " + summary.Rockiness + " (" + summary.RockPlacementCount + " rock blockers)\n" +
                "Water: " + summary.WaterAmount + "\n" +
                "Symmetry: " + summary.Symmetry + "\n" +
                "Profile: " + summary.GameplayProfile + "\n" +
                "Build pads: " + summary.BuildPadCount + "\n" +
                "Fairness: " + (balance == null ? 0 : balance.FairnessScore) + "/100\n" +
                "Connected pairs: " + (balance == null ? 0 : balance.ConnectedStartPairs) + "/" + (balance == null ? 0 : balance.TotalStartPairs) + "\n" +
                "Path distance range: " + (balance == null ? 0 : balance.MinStartDistance) + "-" + (balance == null ? 0 : balance.MaxStartDistance) + "\n" +
                "Build pad range: " + MinBuildPads(buildability) + "-" + MaxBuildPads(buildability);
        }

        static int MinBuildPads(AegisMapBuildabilityReport buildability)
        {
            if (buildability == null || buildability.BuildPadsByPlayer.Count == 0)
                return 0;
            var min = buildability.BuildPadsByPlayer[0].BuildPadCount;
            for (var i = 1; i < buildability.BuildPadsByPlayer.Count; i++)
                if (buildability.BuildPadsByPlayer[i].BuildPadCount < min)
                    min = buildability.BuildPadsByPlayer[i].BuildPadCount;
            return min;
        }

        static int MaxBuildPads(AegisMapBuildabilityReport buildability)
        {
            if (buildability == null || buildability.BuildPadsByPlayer.Count == 0)
                return 0;
            var max = buildability.BuildPadsByPlayer[0].BuildPadCount;
            for (var i = 1; i < buildability.BuildPadsByPlayer.Count; i++)
                if (buildability.BuildPadsByPlayer[i].BuildPadCount > max)
                    max = buildability.BuildPadsByPlayer[i].BuildPadCount;
            return max;
        }
    }
}
