using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.MapGeneration
{
    public enum GeneratedMapStyle
    {
        Frontier,
        Woodlands,
        Wetlands,
        Badlands
    }

    public enum GeneratedMapSymmetry
    {
        None,
        LeftRightMirror,
        FourWayMirror
    }

    public sealed class MapGenerationSettings
    {
        public int Seed { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PlayerCount { get; private set; }
        public GeneratedMapStyle Style { get; private set; }
        public GeneratedMapSymmetry Symmetry { get; private set; }
        public int ResourceClustersPerPlayer { get; private set; }
        public int ResourceAmountPerCell { get; private set; }

        public MapGenerationSettings(
            int seed,
            int width,
            int height,
            int playerCount,
            GeneratedMapStyle style,
            GeneratedMapSymmetry symmetry,
            int resourceClustersPerPlayer,
            int resourceAmountPerCell)
        {
            Seed = seed;
            Width = Math.Max(24, width);
            Height = Math.Max(24, height);
            PlayerCount = NormalizePlayerCount(playerCount);
            Style = style;
            Symmetry = symmetry;
            ResourceClustersPerPlayer = Math.Max(1, resourceClustersPerPlayer);
            ResourceAmountPerCell = Math.Max(25, resourceAmountPerCell);
        }

        public static MapGenerationSettings CreateDefaultSkirmish(int seed)
        {
            return new MapGenerationSettings(
                seed,
                40,
                40,
                2,
                GeneratedMapStyle.Frontier,
                GeneratedMapSymmetry.LeftRightMirror,
                2,
                150);
        }

        static int NormalizePlayerCount(int playerCount)
        {
            if (playerCount <= 2)
                return 2;
            return 4;
        }
    }

    public sealed class GeneratedMapSpawn
    {
        public int PlayerId { get; private set; }
        public Int2 Cell { get; private set; }

        public GeneratedMapSpawn(int playerId, Int2 cell)
        {
            PlayerId = playerId;
            Cell = cell;
        }
    }

    public sealed class GeneratedMapResourceCell
    {
        public Int2 Cell { get; private set; }
        public ResourceKind Kind { get; private set; }
        public int Amount { get; private set; }

        public GeneratedMapResourceCell(Int2 cell, ResourceKind kind, int amount)
        {
            Cell = cell;
            Kind = kind;
            Amount = amount;
        }
    }

    public sealed class GeneratedMapResult
    {
        public int Seed { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public GeneratedMapStyle Style { get; private set; }
        public GeneratedMapSymmetry Symmetry { get; private set; }
        public IReadOnlyList<TerrainCellState> TerrainCells { get; private set; }
        public IReadOnlyList<GeneratedMapSpawn> Spawns { get; private set; }
        public IReadOnlyList<GeneratedMapResourceCell> Resources { get; private set; }

        public GeneratedMapResult(
            int seed,
            int width,
            int height,
            GeneratedMapStyle style,
            GeneratedMapSymmetry symmetry,
            IReadOnlyList<TerrainCellState> terrainCells,
            IReadOnlyList<GeneratedMapSpawn> spawns,
            IReadOnlyList<GeneratedMapResourceCell> resources)
        {
            Seed = seed;
            Width = width;
            Height = height;
            Style = style;
            Symmetry = symmetry;
            TerrainCells = terrainCells ?? new TerrainCellState[0];
            Spawns = spawns ?? new GeneratedMapSpawn[0];
            Resources = resources ?? new GeneratedMapResourceCell[0];
        }

        public TerrainKind TerrainAt(Int2 cell)
        {
            if (cell.X < 0 || cell.Y < 0 || cell.X >= Width || cell.Y >= Height)
                return TerrainKind.Cliff;

            return TerrainCells[cell.Y * Width + cell.X].Kind;
        }
    }

    public sealed class DeterministicRandom
    {
        uint state;

        public DeterministicRandom(int seed)
        {
            state = seed == 0 ? 0x6D2B79F5u : unchecked((uint)seed);
        }

        public int Next()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return unchecked((int)(state & 0x7FFFFFFF));
        }

        public int Next(int exclusiveMax)
        {
            if (exclusiveMax <= 1)
                return 0;

            return Next() % exclusiveMax;
        }

        public int Next(int inclusiveMin, int exclusiveMax)
        {
            if (exclusiveMax <= inclusiveMin)
                return inclusiveMin;

            return inclusiveMin + Next(exclusiveMax - inclusiveMin);
        }
    }
}
