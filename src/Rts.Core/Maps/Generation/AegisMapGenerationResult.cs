using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapGenerationResult
    {
        public bool Success { get; private set; }
        public AegisMapDocument Document { get; private set; }
        public AegisMapGenerationRequest Request { get; private set; }
        public int Seed { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; }
        public AegisMapBuildabilityReport Buildability { get; private set; }
        public AegisMapBalanceReport Balance { get; private set; }
        public AegisMapGenerationSummary Summary { get; private set; }

        AegisMapGenerationResult(
            bool success,
            AegisMapDocument document,
            AegisMapGenerationRequest request,
            int seed,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings,
            AegisMapBuildabilityReport buildability,
            AegisMapBalanceReport balance,
            AegisMapGenerationSummary summary)
        {
            Success = success;
            Document = document;
            Request = request;
            Seed = seed;
            Errors = errors ?? new string[0];
            Warnings = warnings ?? new string[0];
            Buildability = buildability;
            Balance = balance;
            Summary = summary ?? AegisMapGenerationSummary.FromRequest(request, seed, Errors, Warnings);
        }

        public static AegisMapGenerationResult Ok(AegisMapDocument document, AegisMapGenerationRequest request, int seed, IReadOnlyList<string> warnings, AegisMapBuildabilityReport buildability, AegisMapBalanceReport balance)
        {
            var errors = new string[0];
            var summary = AegisMapGenerationSummary.FromDocument(document, request, seed, buildability, errors, warnings);
            return new AegisMapGenerationResult(true, document, request, seed, errors, warnings, buildability, balance, summary);
        }

        public static AegisMapGenerationResult Fail(AegisMapGenerationRequest request, int seed, IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            return new AegisMapGenerationResult(false, null, request, seed, errors, warnings, null, null, AegisMapGenerationSummary.FromRequest(request, seed, errors, warnings));
        }
    }

    public sealed class AegisMapGenerationSummary
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Seed { get; private set; }
        public int PlayerCount { get; private set; }
        public AegisMapBiome Biome { get; private set; }
        public AegisMapIntensity ResourceDensity { get; private set; }
        public AegisMapIntensity CliffDensity { get; private set; }
        public AegisMapIntensity Rockiness { get; private set; }
        public AegisMapWaterAmount WaterAmount { get; private set; }
        public AegisMapSymmetryMode Symmetry { get; private set; }
        public AegisMapGameplayProfile GameplayProfile { get; private set; }
        public int ResourceFieldCount { get; private set; }
        public int TotalResourceAmount { get; private set; }
        public int BlockerCellCount { get; private set; }
        public int CliffBlockerCount { get; private set; }
        public int RockPlacementCount { get; private set; }
        public int BuildPadCount { get; private set; }
        public int WarningCount { get; private set; }
        public int ValidationErrorCount { get; private set; }

        AegisMapGenerationSummary()
        {
        }

        public static AegisMapGenerationSummary FromDocument(
            AegisMapDocument document,
            AegisMapGenerationRequest request,
            int seed,
            AegisMapBuildabilityReport buildability,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings)
        {
            var summary = FromRequest(request, seed, errors, warnings);
            if (document == null)
                return summary;

            summary.Width = document.Width;
            summary.Height = document.Height;
            summary.PlayerCount = document.PlayerStarts == null ? summary.PlayerCount : document.PlayerStarts.Count;
            summary.ResourceFieldCount = document.Resources == null ? 0 : document.Resources.Count;
            summary.TotalResourceAmount = SumResourceAmount(document);
            summary.BlockerCellCount = document.Blockers == null ? 0 : document.Blockers.Count;
            summary.CliffBlockerCount = CountBlockers(document, "cliff");
            summary.RockPlacementCount = CountBlockers(document, "rock");
            summary.BuildPadCount = buildability == null || buildability.BuildSpots == null ? 0 : buildability.BuildSpots.Count;
            return summary;
        }

        public static AegisMapGenerationSummary FromRequest(
            AegisMapGenerationRequest request,
            int seed,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings)
        {
            request = request ?? AegisMapGenerationRequest.CreateDefault();
            var summary = new AegisMapGenerationSummary();
            summary.Width = request.ResolveWidth();
            summary.Height = request.ResolveHeight();
            summary.Seed = seed;
            summary.PlayerCount = AegisMapGenerationRequest.NormalizePlayerCount(request.PlayerCount);
            summary.Biome = request.Biome;
            summary.ResourceDensity = request.ResourceDensity;
            summary.CliffDensity = request.CliffDensity;
            summary.Rockiness = request.Rockiness;
            summary.WaterAmount = request.WaterAmount;
            summary.Symmetry = request.Symmetry;
            summary.GameplayProfile = request.GameplayProfile;
            summary.WarningCount = warnings == null ? 0 : warnings.Count;
            summary.ValidationErrorCount = errors == null ? 0 : errors.Count;
            return summary;
        }

        static int SumResourceAmount(AegisMapDocument document)
        {
            var total = 0;
            if (document.Resources == null)
                return total;
            for (var i = 0; i < document.Resources.Count; i++)
                total += document.Resources[i].Amount;
            return total;
        }

        static int CountBlockers(AegisMapDocument document, string token)
        {
            var count = 0;
            if (document.Blockers == null)
                return count;
            for (var i = 0; i < document.Blockers.Count; i++)
            {
                var reason = document.Blockers[i].Reason ?? string.Empty;
                if (reason.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    count++;
            }
            return count;
        }
    }

    public sealed class AegisMapBuildSpot
    {
        public int PlayerId { get; private set; }
        public Int2 TopLeft { get; private set; }
        public AegisBuildingFootprint Footprint { get; private set; }

        public AegisMapBuildSpot(int playerId, Int2 topLeft, AegisBuildingFootprint footprint)
        {
            PlayerId = playerId;
            TopLeft = topLeft;
            Footprint = footprint;
        }
    }

    public sealed class AegisMapBuildabilityReport
    {
        public IReadOnlyList<AegisMapBuildSpot> BuildSpots { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; }

        public AegisMapBuildabilityReport(IReadOnlyList<AegisMapBuildSpot> buildSpots, IReadOnlyList<string> warnings)
        {
            BuildSpots = buildSpots ?? new AegisMapBuildSpot[0];
            Warnings = warnings ?? new string[0];
        }

        public int CountForPlayer(int playerId)
        {
            var count = 0;
            for (var i = 0; i < BuildSpots.Count; i++)
                if (BuildSpots[i].PlayerId == playerId)
                    count++;
            return count;
        }
    }

    public sealed class AegisMapBalanceReport
    {
        public bool StartsConnected { get; private set; }
        public int ResourceCellCount { get; private set; }
        public int BlockerCellCount { get; private set; }
        public int ConnectedStartPairs { get; private set; }
        public int TotalStartPairs { get; private set; }
        public int MinStartDistance { get; private set; }
        public int MaxStartDistance { get; private set; }
        public IReadOnlyList<int> UnreachablePlayerIds { get; private set; }
        public IReadOnlyList<AegisPlayerResourceMetric> NearbyResourcesByPlayer { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; }

        public AegisMapBalanceReport(bool startsConnected, int resourceCellCount, int blockerCellCount, IReadOnlyList<string> warnings)
            : this(startsConnected, resourceCellCount, blockerCellCount, 0, 0, 0, 0, new int[0], new AegisPlayerResourceMetric[0], warnings)
        {
        }

        public AegisMapBalanceReport(
            bool startsConnected,
            int resourceCellCount,
            int blockerCellCount,
            int connectedStartPairs,
            int totalStartPairs,
            int minStartDistance,
            int maxStartDistance,
            IReadOnlyList<int> unreachablePlayerIds,
            IReadOnlyList<AegisPlayerResourceMetric> nearbyResourcesByPlayer,
            IReadOnlyList<string> warnings)
        {
            StartsConnected = startsConnected;
            ResourceCellCount = resourceCellCount;
            BlockerCellCount = blockerCellCount;
            ConnectedStartPairs = connectedStartPairs;
            TotalStartPairs = totalStartPairs;
            MinStartDistance = minStartDistance;
            MaxStartDistance = maxStartDistance;
            UnreachablePlayerIds = unreachablePlayerIds ?? new int[0];
            NearbyResourcesByPlayer = nearbyResourcesByPlayer ?? new AegisPlayerResourceMetric[0];
            Warnings = warnings ?? new string[0];
        }
    }

    public sealed class AegisPlayerResourceMetric
    {
        public int PlayerId { get; private set; }
        public int NearbyResourceAmount { get; private set; }
        public int NearbyResourceCells { get; private set; }

        public AegisPlayerResourceMetric(int playerId, int nearbyResourceAmount, int nearbyResourceCells)
        {
            PlayerId = playerId;
            NearbyResourceAmount = nearbyResourceAmount;
            NearbyResourceCells = nearbyResourceCells;
        }
    }
}
