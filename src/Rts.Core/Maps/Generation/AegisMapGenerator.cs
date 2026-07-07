using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapGenerator
    {
        readonly AegisMapTerrainPlanner terrainPlanner;
        readonly AegisMapCliffPlanner cliffPlanner;
        readonly AegisMapRockinessPlanner rockinessPlanner;
        readonly AegisMapResourcePlanner resourcePlanner;
        readonly AegisMapBuildabilityAnalyzer buildabilityAnalyzer;
        readonly AegisMapBalanceAnalyzer balanceAnalyzer;

        public AegisMapGenerator()
        {
            terrainPlanner = new AegisMapTerrainPlanner();
            cliffPlanner = new AegisMapCliffPlanner();
            rockinessPlanner = new AegisMapRockinessPlanner();
            resourcePlanner = new AegisMapResourcePlanner();
            buildabilityAnalyzer = new AegisMapBuildabilityAnalyzer();
            balanceAnalyzer = new AegisMapBalanceAnalyzer();
        }

        public AegisMapGenerationResult Generate(AegisMapGenerationRequest request)
        {
            request = request ?? AegisMapGenerationRequest.CreateDefault();
            request.Normalize();
            var seed = request.ResolveSeed();
            var width = request.ResolveWidth();
            var height = request.ResolveHeight();
            var warnings = new List<string>();
            var errors = new List<string>();

            if (width < AegisMapDocument.MinWidth || height < AegisMapDocument.MinHeight)
                errors.Add("GeneratedMapTooSmall:Maps must be at least 100x100.");
            if (width > AegisMapDocument.MaxWidth || height > AegisMapDocument.MaxHeight)
                errors.Add("GeneratedMapTooLarge:Maps must be at most 400x400.");
            if (errors.Count > 0)
                return AegisMapGenerationResult.Fail(request, seed, errors, warnings);

            var profile = AegisMapGenerationProfile.FromRequest(request);
            var document = AegisMapDocument.CreateEmpty(width, height, "generated_" + width + "x" + height + "_seed_" + seed);
            document.DisplayName = "Generated " + request.Biome + " " + width + "x" + height;
            document.StartingCredits = 5000;
            document.Properties["generatedBy"] = "AegisMapGenerator";
            document.Properties["generationSeed"] = seed.ToString();
            document.Properties["sizePreset"] = request.SizePreset.ToString();
            document.Properties["biome"] = request.Biome.ToString();
            document.Properties["resourceDensity"] = request.ResourceDensity.ToString();
            document.Properties["cliffDensity"] = request.CliffDensity.ToString();
            document.Properties["rockiness"] = request.Rockiness.ToString();
            document.Properties["waterAmount"] = request.WaterAmount.ToString();
            document.Properties["symmetry"] = request.Symmetry.ToString();
            document.Properties["gameplayProfile"] = request.GameplayProfile.ToString();
            if (!string.IsNullOrEmpty(request.PromptText))
                document.Properties["prompt"] = request.PromptText;

            var terrain = new TerrainKind[width * height];
            var protectedCells = new bool[width * height];
            FillTerrain(request, profile, seed, width, height, terrain);

            var starts = CreatePlayerStarts(request, width, height);
            for (var i = 0; i < starts.Count; i++)
            {
                document.PlayerStarts.Add(starts[i]);
                ClearBaseArea(terrain, protectedCells, width, height, starts[i], profile.BaseClearRadius);
                document.Regions.Add(new AegisRegion("base_area_p" + starts[i].PlayerId, starts[i].X - profile.BaseClearRadius, starts[i].Y - profile.BaseClearRadius, profile.BaseClearRadius * 2 + 1, profile.BaseClearRadius * 2 + 1, "generated_base_area"));
            }

            PaintRoadNetwork(terrain, protectedCells, width, height, starts, request);
            ApplyCliffsAndRocks(request, profile, seed, terrain, protectedCells, width, height);
            PaintRoadNetwork(terrain, protectedCells, width, height, starts, request);
            resourcePlanner.PlaceResources(document, terrain, protectedCells, request, profile);
            RemoveResourceCollisions(document, protectedCells);
            WriteTerrainAndBlockers(document, terrain, protectedCells);

            var buildability = buildabilityAnalyzer.Analyze(document, profile.MinimumBuildPadsPerStart);
            if (buildability.Warnings.Count > 0)
                warnings.AddRange(buildability.Warnings);
            AddBuildSpotRegions(document, buildability);

            var validation = new AegisMapDocumentValidator().Validate(document);
            if (!validation.Success)
                return AegisMapGenerationResult.Fail(request, seed, validation.Errors, warnings);
            warnings.AddRange(validation.Warnings);

            var balance = balanceAnalyzer.Analyze(document);
            if (balance.Warnings.Count > 0)
                warnings.AddRange(balance.Warnings);

            return AegisMapGenerationResult.Ok(document, request, seed, warnings, buildability, balance);
        }

        void FillTerrain(AegisMapGenerationRequest request, AegisMapGenerationProfile profile, int seed, int width, int height, TerrainKind[] terrain)
        {
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        terrain[Index(width, x, y)] = TerrainKind.Cliff;
                        continue;
                    }

                    var canonical = CanonicalCell(request.Symmetry, width, height, x, y);
                    terrain[Index(width, x, y)] = terrainPlanner.PlanTerrain(request, profile, seed, canonical.X, canonical.Y);
                }
        }

        void ApplyCliffsAndRocks(AegisMapGenerationRequest request, AegisMapGenerationProfile profile, int seed, TerrainKind[] terrain, bool[] protectedCells, int width, int height)
        {
            for (var y = 1; y < height - 1; y++)
                for (var x = 1; x < width - 1; x++)
                {
                    var key = Index(width, x, y);
                    if (protectedCells[key] || terrain[key] == TerrainKind.Water || terrain[key] == TerrainKind.OreField)
                        continue;

                    var canonical = CanonicalCell(request.Symmetry, width, height, x, y);
                    if (cliffPlanner.ShouldPlaceCliff(profile, seed, canonical.X, canonical.Y))
                    {
                        cliffPlanner.ApplyCliff(terrain, width, x, y);
                        continue;
                    }

                    if (rockinessPlanner.ShouldPlaceRock(profile, seed, canonical.X, canonical.Y))
                        rockinessPlanner.ApplyRock(terrain, width, x, y);
                }
        }

        static List<AegisPlayerStart> CreatePlayerStarts(AegisMapGenerationRequest request, int width, int height)
        {
            var starts = new List<AegisPlayerStart>();
            var margin = Clamp(Math.Min(width, height) / 7, 14, 42);
            var midX = width / 2;
            var midY = height / 2;
            var playerCount = AegisMapGenerationRequest.NormalizePlayerCount(request.PlayerCount);

            starts.Add(new AegisPlayerStart(1, margin, midY, "Player 1"));
            starts.Add(new AegisPlayerStart(2, width - margin - 1, midY, "Player 2"));
            if (playerCount >= 4)
            {
                starts[0] = new AegisPlayerStart(1, margin, margin, "Player 1");
                starts[1] = new AegisPlayerStart(2, width - margin - 1, height - margin - 1, "Player 2");
                starts.Add(new AegisPlayerStart(3, width - margin - 1, margin, "Player 3"));
                starts.Add(new AegisPlayerStart(4, margin, height - margin - 1, "Player 4"));
            }
            if (playerCount >= 6)
            {
                starts.Add(new AegisPlayerStart(5, midX, margin, "Player 5"));
                starts.Add(new AegisPlayerStart(6, midX, height - margin - 1, "Player 6"));
            }
            if (playerCount >= 8)
            {
                starts.Add(new AegisPlayerStart(7, margin, midY, "Player 7"));
                starts.Add(new AegisPlayerStart(8, width - margin - 1, midY, "Player 8"));
            }

            starts.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));
            return starts;
        }

        static void ClearBaseArea(TerrainKind[] terrain, bool[] protectedCells, int width, int height, AegisPlayerStart start, int radius)
        {
            for (var y = -radius; y <= radius; y++)
                for (var x = -radius; x <= radius; x++)
                {
                    var cellX = start.X + x;
                    var cellY = start.Y + y;
                    if (cellX <= 0 || cellY <= 0 || cellX >= width - 1 || cellY >= height - 1)
                        continue;

                    var key = Index(width, cellX, cellY);
                    terrain[key] = TerrainKind.Clear;
                    protectedCells[key] = true;
                }
        }

        static void PaintRoadNetwork(TerrainKind[] terrain, bool[] protectedCells, int width, int height, List<AegisPlayerStart> starts, AegisMapGenerationRequest request)
        {
            var center = new Int2(width / 2, height / 2);
            for (var i = 0; i < starts.Count; i++)
            {
                var start = new Int2(starts[i].X, starts[i].Y);
                PaintRoadLine(terrain, protectedCells, width, height, start, new Int2(center.X, start.Y));
                PaintRoadLine(terrain, protectedCells, width, height, new Int2(center.X, start.Y), center);
            }

            if (request.GameplayProfile == AegisMapGameplayProfile.Chokepoint || request.GameplayProfile == AegisMapGameplayProfile.Defensive)
            {
                PaintRoadLine(terrain, protectedCells, width, height, new Int2(center.X - 6, center.Y), new Int2(center.X + 6, center.Y));
                PaintRoadLine(terrain, protectedCells, width, height, new Int2(center.X, center.Y - 6), new Int2(center.X, center.Y + 6));
            }
        }

        static void PaintRoadLine(TerrainKind[] terrain, bool[] protectedCells, int width, int height, Int2 a, Int2 b)
        {
            var x = a.X;
            var y = a.Y;
            var dx = Sign(b.X - a.X);
            var dy = Sign(b.Y - a.Y);
            while (x != b.X)
            {
                PaintRoadCell(terrain, protectedCells, width, height, x, y);
                x += dx;
            }
            while (y != b.Y)
            {
                PaintRoadCell(terrain, protectedCells, width, height, x, y);
                y += dy;
            }
            PaintRoadCell(terrain, protectedCells, width, height, b.X, b.Y);
        }

        static void PaintRoadCell(TerrainKind[] terrain, bool[] protectedCells, int width, int height, int x, int y)
        {
            for (var yy = -1; yy <= 1; yy++)
                for (var xx = -1; xx <= 1; xx++)
                {
                    var cellX = x + xx;
                    var cellY = y + yy;
                    if (cellX <= 0 || cellY <= 0 || cellX >= width - 1 || cellY >= height - 1)
                        continue;

                    var key = Index(width, cellX, cellY);
                    terrain[key] = xx == 0 || yy == 0 ? TerrainKind.Road : TerrainKind.Clear;
                    protectedCells[key] = true;
                }
        }

        static void RemoveResourceCollisions(AegisMapDocument document, bool[] protectedCells)
        {
            for (var i = document.Resources.Count - 1; i >= 0; i--)
            {
                var resource = document.Resources[i];
                if (protectedCells[Index(document.Width, resource.X, resource.Y)])
                    document.Resources.RemoveAt(i);
            }
        }

        static void WriteTerrainAndBlockers(AegisMapDocument document, TerrainKind[] terrain, bool[] protectedCells)
        {
            for (var y = 0; y < document.Height; y++)
                for (var x = 0; x < document.Width; x++)
                {
                    var key = Index(document.Width, x, y);
                    var kind = terrain[key];
                    if (kind != TerrainKind.Clear)
                        document.TerrainBase.Add(new AegisTerrainCell(x, y, AegisMapTerrainIds.ToTerrainId(kind)));
                    if (kind == TerrainKind.Cliff || (!protectedCells[key] && kind == TerrainKind.Rough && AegisMapTerrainPlanner.HashToThousand(0xB10C, x, y) > 940))
                        document.Blockers.Add(new AegisBlockerCell(x, y, true, kind == TerrainKind.Cliff ? "generated_cliff" : "generated_rock"));
                }
        }

        static void AddBuildSpotRegions(AegisMapDocument document, AegisMapBuildabilityReport buildability)
        {
            if (buildability == null)
                return;
            for (var i = 0; i < buildability.BuildSpots.Count; i++)
            {
                var spot = buildability.BuildSpots[i];
                document.Regions.Add(new AegisRegion(
                    "build_pad_p" + spot.PlayerId + "_" + i,
                    spot.TopLeft.X,
                    spot.TopLeft.Y,
                    spot.Footprint.Width,
                    spot.Footprint.Height,
                    "generated_build_pad"));
            }
        }

        static Int2 CanonicalCell(AegisMapSymmetryMode symmetry, int width, int height, int x, int y)
        {
            if (symmetry == AegisMapSymmetryMode.Horizontal)
                return new Int2(Math.Min(x, width - 1 - x), y);
            if (symmetry == AegisMapSymmetryMode.Vertical)
                return new Int2(x, Math.Min(y, height - 1 - y));
            if (symmetry == AegisMapSymmetryMode.Rotational)
            {
                var rx = width - 1 - x;
                var ry = height - 1 - y;
                if (ry < y || (ry == y && rx < x))
                    return new Int2(rx, ry);
            }
            if (symmetry == AegisMapSymmetryMode.Radial)
                return new Int2(Math.Min(x, width - 1 - x), Math.Min(y, height - 1 - y));
            return new Int2(x, y);
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        static int Sign(int value)
        {
            if (value < 0)
                return -1;
            if (value > 0)
                return 1;
            return 0;
        }

        static int Index(int width, int x, int y)
        {
            return y * width + x;
        }
    }
}
