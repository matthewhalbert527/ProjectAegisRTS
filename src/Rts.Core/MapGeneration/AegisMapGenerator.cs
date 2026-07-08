using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.MapGeneration
{
    public sealed class AegisMapGenerator
    {
        const int BorderThickness = 1;
        const int BaseClearRadius = 6;
        const int ResourceClusterRadius = 2;

        public GeneratedMapResult Generate(MapGenerationSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            var width = settings.Width;
            var height = settings.Height;
            var terrain = new TerrainKind[width * height];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    terrain[Index(width, x, y)] = GenerateTerrainKind(settings, x, y);

            var spawns = CreateSpawns(settings);
            PaintRoadNetwork(settings, terrain, spawns);
            for (var i = 0; i < spawns.Count; i++)
                ClearDisc(terrain, width, height, spawns[i].Cell, BaseClearRadius, TerrainKind.Clear);

            var resources = new List<GeneratedMapResourceCell>();
            PlacePlayerResources(settings, terrain, spawns, resources);
            PlaceExpansionResources(settings, terrain, resources);
            ApplyTerrainSymmetry(settings, terrain);
            SynchronizeResourcesFromTerrain(settings, terrain, resources);

            var terrainCells = new List<TerrainCellState>(width * height);
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    terrainCells.Add(new TerrainCellState(new Int2(x, y), terrain[Index(width, x, y)]));

            resources.Sort((a, b) =>
            {
                var yCompare = a.Cell.Y.CompareTo(b.Cell.Y);
                return yCompare != 0 ? yCompare : a.Cell.X.CompareTo(b.Cell.X);
            });

            return new GeneratedMapResult(
                settings.Seed,
                width,
                height,
                settings.Style,
                settings.Symmetry,
                terrainCells,
                spawns,
                resources);
        }

        TerrainKind GenerateTerrainKind(MapGenerationSettings settings, int x, int y)
        {
            if (x < BorderThickness || y < BorderThickness || x >= settings.Width - BorderThickness || y >= settings.Height - BorderThickness)
                return TerrainKind.Cliff;

            var canonical = CanonicalCell(settings, x, y);
            var centerY = settings.Height / 2;
            if (Math.Abs(y - centerY) <= 1)
                return TerrainKind.Clear;

            var waterNoise = LayeredNoise(settings.Seed ^ 0x51C51, canonical.X, canonical.Y, 14, 7, 3);
            var roughNoise = LayeredNoise(settings.Seed ^ 0x1A3B7, canonical.X, canonical.Y, 11, 5, 2);
            var forestNoise = LayeredNoise(settings.Seed ^ 0x2F7D1, canonical.X, canonical.Y, 9, 4, 2);
            var cliffNoise = LayeredNoise(settings.Seed ^ 0x735C9, canonical.X, canonical.Y, 13, 6, 3);

            int waterThreshold;
            int roughThreshold;
            int forestThreshold;
            int cliffThreshold;
            ThresholdsFor(settings.Style, out waterThreshold, out roughThreshold, out forestThreshold, out cliffThreshold);

            if (waterNoise >= waterThreshold)
                return TerrainKind.Water;
            if (cliffNoise >= cliffThreshold)
                return TerrainKind.Cliff;
            if (forestNoise >= forestThreshold)
                return TerrainKind.Forest;
            if (roughNoise >= roughThreshold)
                return TerrainKind.Rough;

            return TerrainKind.Clear;
        }

        static void ThresholdsFor(GeneratedMapStyle style, out int water, out int rough, out int forest, out int cliff)
        {
            switch (style)
            {
                case GeneratedMapStyle.Woodlands:
                    water = 960;
                    rough = 850;
                    forest = 660;
                    cliff = 970;
                    return;
                case GeneratedMapStyle.Wetlands:
                    water = 825;
                    rough = 900;
                    forest = 780;
                    cliff = 980;
                    return;
                case GeneratedMapStyle.Badlands:
                    water = 985;
                    rough = 630;
                    forest = 940;
                    cliff = 760;
                    return;
                default:
                    water = 930;
                    rough = 780;
                    forest = 850;
                    cliff = 935;
                    return;
            }
        }

        List<GeneratedMapSpawn> CreateSpawns(MapGenerationSettings settings)
        {
            var result = new List<GeneratedMapSpawn>();
            var random = new DeterministicRandom(settings.Seed ^ 0x3E51A);
            var margin = Clamp(Math.Min(settings.Width, settings.Height) / 6, 6, 9);

            if (settings.PlayerCount == 2)
            {
                var yOffsetLimit = Math.Max(1, settings.Height / 10);
                var spawnY = Clamp(settings.Height / 2 + random.Next(-yOffsetLimit, yOffsetLimit + 1), margin, settings.Height - margin - 1);
                result.Add(new GeneratedMapSpawn(1, new Int2(margin, spawnY)));
                result.Add(new GeneratedMapSpawn(2, new Int2(settings.Width - margin - 1, spawnY)));
                return result;
            }

            result.Add(new GeneratedMapSpawn(1, new Int2(margin, margin)));
            result.Add(new GeneratedMapSpawn(2, new Int2(settings.Width - margin - 1, margin)));
            result.Add(new GeneratedMapSpawn(3, new Int2(margin, settings.Height - margin - 1)));
            result.Add(new GeneratedMapSpawn(4, new Int2(settings.Width - margin - 1, settings.Height - margin - 1)));
            return result;
        }

        void PaintRoadNetwork(MapGenerationSettings settings, TerrainKind[] terrain, IReadOnlyList<GeneratedMapSpawn> spawns)
        {
            var center = new Int2(settings.Width / 2, settings.Height / 2);
            if (spawns.Count == 2)
            {
                PaintRoadLine(terrain, settings.Width, settings.Height, spawns[0].Cell, new Int2(spawns[1].Cell.X, spawns[0].Cell.Y));
                PaintRoadLine(terrain, settings.Width, settings.Height, new Int2(center.X, spawns[0].Cell.Y), center);
                return;
            }

            for (var i = 0; i < spawns.Count; i++)
            {
                PaintRoadLine(terrain, settings.Width, settings.Height, spawns[i].Cell, new Int2(center.X, spawns[i].Cell.Y));
                PaintRoadLine(terrain, settings.Width, settings.Height, new Int2(center.X, spawns[i].Cell.Y), center);
            }
        }

        void PlacePlayerResources(MapGenerationSettings settings, TerrainKind[] terrain, IReadOnlyList<GeneratedMapSpawn> spawns, List<GeneratedMapResourceCell> resources)
        {
            for (var i = 0; i < spawns.Count; i++)
            {
                var spawn = spawns[i].Cell;
                var towardCenterX = Sign(settings.Width / 2 - spawn.X);
                var towardCenterY = Sign(settings.Height / 2 - spawn.Y);
                if (towardCenterX == 0)
                    towardCenterX = 1;

                var first = ClampCell(settings, new Int2(spawn.X + towardCenterX * 11, spawn.Y - 5));
                var second = ClampCell(settings, new Int2(spawn.X + towardCenterX * 11, spawn.Y + 5));
                PlaceResourceCluster(settings, terrain, resources, first, ResourceClusterRadius);
                if (settings.ResourceClustersPerPlayer > 1)
                    PlaceResourceCluster(settings, terrain, resources, second, ResourceClusterRadius);

                if (settings.PlayerCount == 4 && settings.ResourceClustersPerPlayer > 2)
                    PlaceResourceCluster(settings, terrain, resources, ClampCell(settings, new Int2(spawn.X + towardCenterX * 6, spawn.Y + towardCenterY * 6)), ResourceClusterRadius);
            }
        }

        void PlaceExpansionResources(MapGenerationSettings settings, TerrainKind[] terrain, List<GeneratedMapResourceCell> resources)
        {
            var center = new Int2(settings.Width / 2, settings.Height / 2);
            PlaceResourceCluster(settings, terrain, resources, center, ResourceClusterRadius + 1);

            if (settings.PlayerCount == 2)
            {
                PlaceResourceCluster(settings, terrain, resources, new Int2(center.X, Clamp(center.Y - 8, 4, settings.Height - 5)), ResourceClusterRadius);
                PlaceResourceCluster(settings, terrain, resources, new Int2(center.X, Clamp(center.Y + 8, 4, settings.Height - 5)), ResourceClusterRadius);
            }
        }

        void PlaceResourceCluster(MapGenerationSettings settings, TerrainKind[] terrain, List<GeneratedMapResourceCell> resources, Int2 center, int radius)
        {
            ClearDisc(terrain, settings.Width, settings.Height, center, radius + 1, TerrainKind.Clear);

            for (var y = -radius; y <= radius; y++)
                for (var x = -radius; x <= radius; x++)
                {
                    if (Math.Abs(x) + Math.Abs(y) > radius + 1)
                        continue;

                    var cell = new Int2(center.X + x, center.Y + y);
                    if (cell.X <= 1 || cell.Y <= 1 || cell.X >= settings.Width - 2 || cell.Y >= settings.Height - 2)
                        continue;

                    terrain[Index(settings.Width, cell.X, cell.Y)] = TerrainKind.OreField;
                    if (!ContainsResource(resources, cell))
                        resources.Add(new GeneratedMapResourceCell(cell, ResourceKind.Ore, settings.ResourceAmountPerCell));
                }
        }

        static bool ContainsResource(IReadOnlyList<GeneratedMapResourceCell> resources, Int2 cell)
        {
            for (var i = 0; i < resources.Count; i++)
                if (resources[i].Cell.Equals(cell))
                    return true;

            return false;
        }

        static Int2 CanonicalCell(MapGenerationSettings settings, int x, int y)
        {
            if (settings.Symmetry == GeneratedMapSymmetry.LeftRightMirror)
                return new Int2(Math.Min(x, settings.Width - 1 - x), y);
            if (settings.Symmetry == GeneratedMapSymmetry.FourWayMirror)
                return new Int2(Math.Min(x, settings.Width - 1 - x), Math.Min(y, settings.Height - 1 - y));

            return new Int2(x, y);
        }

        static void ApplyTerrainSymmetry(MapGenerationSettings settings, TerrainKind[] terrain)
        {
            if (settings.Symmetry == GeneratedMapSymmetry.None)
                return;

            if (settings.Symmetry == GeneratedMapSymmetry.LeftRightMirror || settings.Symmetry == GeneratedMapSymmetry.FourWayMirror)
            {
                for (var y = 0; y < settings.Height; y++)
                    for (var x = 0; x < settings.Width / 2; x++)
                    {
                        var left = Index(settings.Width, x, y);
                        var right = Index(settings.Width, settings.Width - 1 - x, y);
                        var merged = MergeMirroredTerrain(terrain[left], terrain[right]);
                        terrain[left] = merged;
                        terrain[right] = merged;
                    }
            }

            if (settings.Symmetry == GeneratedMapSymmetry.FourWayMirror)
            {
                for (var y = 0; y < settings.Height / 2; y++)
                    for (var x = 0; x < settings.Width; x++)
                    {
                        var top = Index(settings.Width, x, y);
                        var bottom = Index(settings.Width, x, settings.Height - 1 - y);
                        var merged = MergeMirroredTerrain(terrain[top], terrain[bottom]);
                        terrain[top] = merged;
                        terrain[bottom] = merged;
                    }
            }
        }

        static TerrainKind MergeMirroredTerrain(TerrainKind a, TerrainKind b)
        {
            if (a == TerrainKind.OreField || b == TerrainKind.OreField)
                return TerrainKind.OreField;
            if (a == TerrainKind.Road || b == TerrainKind.Road)
                return TerrainKind.Road;
            return a;
        }

        static void SynchronizeResourcesFromTerrain(MapGenerationSettings settings, TerrainKind[] terrain, List<GeneratedMapResourceCell> resources)
        {
            var amountByCell = new Dictionary<Int2, int>();
            for (var i = 0; i < resources.Count; i++)
                amountByCell[resources[i].Cell] = resources[i].Amount;

            resources.Clear();
            for (var y = 0; y < settings.Height; y++)
                for (var x = 0; x < settings.Width; x++)
                {
                    if (terrain[Index(settings.Width, x, y)] != TerrainKind.OreField)
                        continue;

                    var cell = new Int2(x, y);
                    int amount;
                    if (!amountByCell.TryGetValue(cell, out amount))
                        amount = settings.ResourceAmountPerCell;

                    resources.Add(new GeneratedMapResourceCell(cell, ResourceKind.Ore, amount));
                }
        }

        static int LayeredNoise(int seed, int x, int y, int scaleA, int scaleB, int scaleC)
        {
            return (ValueNoise(seed, x, y, scaleA) * 5 +
                ValueNoise(seed ^ 0x4A39, x, y, scaleB) * 3 +
                ValueNoise(seed ^ 0x7F21, x, y, scaleC) * 2) / 10;
        }

        static int ValueNoise(int seed, int x, int y, int scale)
        {
            scale = Math.Max(1, scale);
            var x0 = FloorDiv(x, scale);
            var y0 = FloorDiv(y, scale);
            var fx = PositiveMod(x, scale);
            var fy = PositiveMod(y, scale);

            var a = HashToThousand(seed, x0, y0);
            var b = HashToThousand(seed, x0 + 1, y0);
            var c = HashToThousand(seed, x0, y0 + 1);
            var d = HashToThousand(seed, x0 + 1, y0 + 1);
            var top = Lerp(a, b, fx, scale);
            var bottom = Lerp(c, d, fx, scale);
            return Lerp(top, bottom, fy, scale);
        }

        static int HashToThousand(int seed, int x, int y)
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
            return a + (b - a) * numerator / Math.Max(1, denominator);
        }

        static int FloorDiv(int value, int divisor)
        {
            if (value >= 0)
                return value / divisor;
            return -((-value + divisor - 1) / divisor);
        }

        static int PositiveMod(int value, int divisor)
        {
            var result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        static void PaintRoadLine(TerrainKind[] terrain, int width, int height, Int2 a, Int2 b)
        {
            var x = a.X;
            var y = a.Y;
            var dx = Sign(b.X - a.X);
            var dy = Sign(b.Y - a.Y);

            while (x != b.X)
            {
                PaintRoadCell(terrain, width, height, x, y);
                x += dx;
            }

            while (y != b.Y)
            {
                PaintRoadCell(terrain, width, height, x, y);
                y += dy;
            }

            PaintRoadCell(terrain, width, height, b.X, b.Y);
        }

        static void PaintRoadCell(TerrainKind[] terrain, int width, int height, int x, int y)
        {
            for (var yy = -1; yy <= 1; yy++)
                for (var xx = -1; xx <= 1; xx++)
                {
                    var cx = x + xx;
                    var cy = y + yy;
                    if (cx <= 0 || cy <= 0 || cx >= width - 1 || cy >= height - 1)
                        continue;

                    terrain[Index(width, cx, cy)] = xx == 0 || yy == 0 ? TerrainKind.Road : TerrainKind.Clear;
                }
        }

        static void ClearDisc(TerrainKind[] terrain, int width, int height, Int2 center, int radius, TerrainKind kind)
        {
            var radiusSq = radius * radius;
            for (var y = -radius; y <= radius; y++)
                for (var x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radiusSq)
                        continue;

                    var cx = center.X + x;
                    var cy = center.Y + y;
                    if (cx <= 0 || cy <= 0 || cx >= width - 1 || cy >= height - 1)
                        continue;

                    terrain[Index(width, cx, cy)] = kind;
                }
        }

        static Int2 ClampCell(MapGenerationSettings settings, Int2 cell)
        {
            return new Int2(
                Clamp(cell.X, 2, settings.Width - 3),
                Clamp(cell.Y, 2, settings.Height - 3));
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
