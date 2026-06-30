using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Terrain
{
    public enum TerrainKind
    {
        Clear,
        Road,
        Rough,
        Forest,
        Water,
        Cliff,
        OreField
    }

    public enum MovementClass
    {
        Infantry,
        Wheeled,
        Tracked,
        Harvester,
        Aircraft,
        Building
    }

    [Flags]
    public enum PassabilityMask
    {
        None = 0,
        Infantry = 1,
        Wheeled = 2,
        Tracked = 4,
        Harvester = 8,
        Aircraft = 16,
        Building = 32,
        Ground = Infantry | Wheeled | Tracked | Harvester,
        All = Ground | Aircraft | Building
    }

    public sealed class TerrainDefinition
    {
        public TerrainKind Kind { get; private set; }
        public string DisplayName { get; private set; }
        public int MovementCost { get; private set; }
        public PassabilityMask Passability { get; private set; }
        public string DebugColorId { get; private set; }

        public TerrainDefinition(TerrainKind kind, string displayName, int movementCost, PassabilityMask passability, string debugColorId)
        {
            Kind = kind;
            DisplayName = displayName;
            MovementCost = movementCost < 1 ? 1 : movementCost;
            Passability = passability;
            DebugColorId = debugColorId ?? string.Empty;
        }

        public bool Allows(MovementClass movementClass)
        {
            return (Passability & MaskFor(movementClass)) != 0;
        }

        public int CostFor(MovementClass movementClass)
        {
            if (movementClass == MovementClass.Aircraft)
                return 1;
            return MovementCost;
        }

        public static PassabilityMask MaskFor(MovementClass movementClass)
        {
            switch (movementClass)
            {
                case MovementClass.Infantry:
                    return PassabilityMask.Infantry;
                case MovementClass.Wheeled:
                    return PassabilityMask.Wheeled;
                case MovementClass.Tracked:
                    return PassabilityMask.Tracked;
                case MovementClass.Harvester:
                    return PassabilityMask.Harvester;
                case MovementClass.Aircraft:
                    return PassabilityMask.Aircraft;
                case MovementClass.Building:
                    return PassabilityMask.Building;
                default:
                    return PassabilityMask.None;
            }
        }
    }

    public sealed class TerrainCellState
    {
        public Int2 Cell { get; private set; }
        public TerrainKind Kind { get; set; }

        public TerrainCellState(Int2 cell, TerrainKind kind)
        {
            Cell = cell;
            Kind = kind;
        }
    }

    public sealed class MapAuthoringData
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IReadOnlyList<TerrainCellState> TerrainCells { get; private set; }

        public MapAuthoringData(int width, int height, IReadOnlyList<TerrainCellState> terrainCells)
        {
            Width = width;
            Height = height;
            TerrainCells = terrainCells ?? new TerrainCellState[0];
        }
    }

    public sealed class MapValidationResult
    {
        public IReadOnlyList<string> Errors { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; }

        public bool Success
        {
            get { return Errors.Count == 0; }
        }

        public MapValidationResult(IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            Errors = errors ?? new string[0];
            Warnings = warnings ?? new string[0];
        }
    }

    public static class TerrainCatalog
    {
        public static IReadOnlyList<TerrainDefinition> CreateDefaultDefinitions()
        {
            return new[]
            {
                new TerrainDefinition(TerrainKind.Clear, "Clear", 1, PassabilityMask.All, "clear"),
                new TerrainDefinition(TerrainKind.Road, "Road", 1, PassabilityMask.All, "road"),
                new TerrainDefinition(TerrainKind.Rough, "Rough", 3, PassabilityMask.Infantry | PassabilityMask.Tracked | PassabilityMask.Harvester | PassabilityMask.Aircraft, "rough"),
                new TerrainDefinition(TerrainKind.Forest, "Forest", 2, PassabilityMask.Infantry | PassabilityMask.Harvester | PassabilityMask.Aircraft, "forest"),
                new TerrainDefinition(TerrainKind.Water, "Water", 6, PassabilityMask.Aircraft, "water"),
                new TerrainDefinition(TerrainKind.Cliff, "Cliff", 8, PassabilityMask.Aircraft, "cliff"),
                new TerrainDefinition(TerrainKind.OreField, "Ore Field", 2, PassabilityMask.Ground | PassabilityMask.Aircraft, "ore")
            };
        }
    }
}
