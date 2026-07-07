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

        AegisMapGenerationResult(
            bool success,
            AegisMapDocument document,
            AegisMapGenerationRequest request,
            int seed,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings,
            AegisMapBuildabilityReport buildability,
            AegisMapBalanceReport balance)
        {
            Success = success;
            Document = document;
            Request = request;
            Seed = seed;
            Errors = errors ?? new string[0];
            Warnings = warnings ?? new string[0];
            Buildability = buildability;
            Balance = balance;
        }

        public static AegisMapGenerationResult Ok(AegisMapDocument document, AegisMapGenerationRequest request, int seed, IReadOnlyList<string> warnings, AegisMapBuildabilityReport buildability, AegisMapBalanceReport balance)
        {
            return new AegisMapGenerationResult(true, document, request, seed, new string[0], warnings, buildability, balance);
        }

        public static AegisMapGenerationResult Fail(AegisMapGenerationRequest request, int seed, IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            return new AegisMapGenerationResult(false, null, request, seed, errors, warnings, null, null);
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
        public IReadOnlyList<string> Warnings { get; private set; }

        public AegisMapBalanceReport(bool startsConnected, int resourceCellCount, int blockerCellCount, IReadOnlyList<string> warnings)
        {
            StartsConnected = startsConnected;
            ResourceCellCount = resourceCellCount;
            BlockerCellCount = blockerCellCount;
            Warnings = warnings ?? new string[0];
        }
    }
}
