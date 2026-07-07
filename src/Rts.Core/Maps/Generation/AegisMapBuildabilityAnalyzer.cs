using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisBuildingFootprint
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public AegisBuildingFootprint(int width, int height)
        {
            Width = Clamp(width);
            Height = Clamp(height);
        }

        public static AegisBuildingFootprint Square(int size)
        {
            return new AegisBuildingFootprint(size, size);
        }

        static int Clamp(int value)
        {
            if (value < 1)
                return 1;
            if (value > 5)
                return 5;
            return value;
        }
    }

    public sealed class AegisMapBuildabilityAnalyzer
    {
        public bool CanPlace(AegisMapDocument document, AegisBuildingFootprint footprint, int topLeftX, int topLeftY)
        {
            if (document == null || footprint == null)
                return false;

            if (topLeftX < 0 || topLeftY < 0 || topLeftX + footprint.Width > document.Width || topLeftY + footprint.Height > document.Height)
                return false;

            var blockers = BuildBlockerSet(document);
            var resources = BuildResourceSet(document);
            var terrain = BuildTerrainMap(document);

            for (var y = 0; y < footprint.Height; y++)
                for (var x = 0; x < footprint.Width; x++)
                {
                    var cellX = topLeftX + x;
                    var cellY = topLeftY + y;
                    var key = Key(document.Width, cellX, cellY);
                    if (blockers.Contains(key) || resources.Contains(key))
                        return false;

                    string terrainId;
                    if (!terrain.TryGetValue(key, out terrainId))
                        terrainId = document.DefaultTerrainId;

                    if (!IsConstructionTerrain(terrainId))
                        return false;
                }

            return true;
        }

        public AegisMapBuildabilityReport Analyze(AegisMapDocument document, int minimumPadsPerStart)
        {
            var spots = new List<AegisMapBuildSpot>();
            var warnings = new List<string>();
            var starts = document == null ? null : document.PlayerStarts;
            if (starts == null)
                return new AegisMapBuildabilityReport(spots, warnings);

            var footprint = AegisBuildingFootprint.Square(4);
            for (var i = 0; i < starts.Count; i++)
            {
                var start = starts[i];
                var before = spots.Count;
                FindPadsForStart(document, start, footprint, minimumPadsPerStart, spots);
                var count = spots.Count - before;
                if (count < minimumPadsPerStart)
                    warnings.Add("BuildPadsLow:Player " + start.PlayerId + " has " + count + " clean pads, expected " + minimumPadsPerStart + ".");
            }

            return new AegisMapBuildabilityReport(spots, warnings);
        }

        void FindPadsForStart(AegisMapDocument document, AegisPlayerStart start, AegisBuildingFootprint footprint, int desiredCount, List<AegisMapBuildSpot> spots)
        {
            for (var radius = 0; radius <= 18 && CountForPlayer(spots, start.PlayerId) < desiredCount; radius++)
            {
                for (var dy = -radius; dy <= radius && CountForPlayer(spots, start.PlayerId) < desiredCount; dy++)
                    for (var dx = -radius; dx <= radius && CountForPlayer(spots, start.PlayerId) < desiredCount; dx++)
                    {
                        if (Abs(dx) != radius && Abs(dy) != radius)
                            continue;

                        var x = start.X + dx - footprint.Width / 2;
                        var y = start.Y + dy - footprint.Height / 2;
                        if (!CanPlace(document, footprint, x, y) || OverlapsExisting(spots, x, y, footprint))
                            continue;

                        spots.Add(new AegisMapBuildSpot(start.PlayerId, new Int2(x, y), footprint));
                    }
            }
        }

        static int CountForPlayer(List<AegisMapBuildSpot> spots, int playerId)
        {
            var count = 0;
            for (var i = 0; i < spots.Count; i++)
                if (spots[i].PlayerId == playerId)
                    count++;
            return count;
        }

        static bool OverlapsExisting(List<AegisMapBuildSpot> spots, int x, int y, AegisBuildingFootprint footprint)
        {
            for (var i = 0; i < spots.Count; i++)
            {
                var other = spots[i];
                if (x + footprint.Width <= other.TopLeft.X || other.TopLeft.X + other.Footprint.Width <= x)
                    continue;
                if (y + footprint.Height <= other.TopLeft.Y || other.TopLeft.Y + other.Footprint.Height <= y)
                    continue;
                return true;
            }
            return false;
        }

        static Dictionary<int, string> BuildTerrainMap(AegisMapDocument document)
        {
            var map = new Dictionary<int, string>();
            AddTerrain(map, document.Width, document.TerrainBase);
            AddTerrain(map, document.Width, document.TerrainOverlay);
            return map;
        }

        static void AddTerrain(Dictionary<int, string> map, int width, List<AegisTerrainCell> cells)
        {
            if (cells == null)
                return;
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                map[Key(width, cell.X, cell.Y)] = cell.TerrainId;
            }
        }

        static HashSet<int> BuildBlockerSet(AegisMapDocument document)
        {
            var set = new HashSet<int>();
            if (document.Blockers == null)
                return set;
            for (var i = 0; i < document.Blockers.Count; i++)
                if (document.Blockers[i].BlocksGround)
                    set.Add(Key(document.Width, document.Blockers[i].X, document.Blockers[i].Y));
            return set;
        }

        static HashSet<int> BuildResourceSet(AegisMapDocument document)
        {
            var set = new HashSet<int>();
            if (document.Resources == null)
                return set;
            for (var i = 0; i < document.Resources.Count; i++)
                if (document.Resources[i].Amount > 0)
                    set.Add(Key(document.Width, document.Resources[i].X, document.Resources[i].Y));
            return set;
        }

        static bool IsConstructionTerrain(string terrainId)
        {
            var normalized = AegisMapTerrainIds.Normalize(terrainId);
            return normalized == AegisMapTerrainIds.Clear || normalized == AegisMapTerrainIds.Road;
        }

        static int Key(int width, int x, int y)
        {
            return y * width + x;
        }

        static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }
    }
}
