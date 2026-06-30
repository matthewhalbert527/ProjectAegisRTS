using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Simulation
{
    public sealed class GridMap
    {
        readonly bool[] blocked;
        readonly TerrainKind[] terrainKinds;
        readonly Dictionary<Int2, ActorId> buildingOccupancy;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            blocked = new bool[width * height];
            terrainKinds = new TerrainKind[width * height];
            buildingOccupancy = new Dictionary<Int2, ActorId>();
        }

        public bool Contains(Int2 cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }

        public bool IsBlocked(Int2 cell)
        {
            return !Contains(cell) || blocked[Index(cell)];
        }

        public void SetBlocked(Int2 cell, bool value)
        {
            if (Contains(cell))
                blocked[Index(cell)] = value;
        }

        public int GetTerrainFlags(Int2 cell)
        {
            return Contains(cell) ? (int)terrainKinds[Index(cell)] : 0;
        }

        public void SetTerrainFlags(Int2 cell, int flags)
        {
            if (Contains(cell))
                terrainKinds[Index(cell)] = (TerrainKind)flags;
        }

        public TerrainKind GetTerrainKind(Int2 cell)
        {
            return Contains(cell) ? terrainKinds[Index(cell)] : TerrainKind.Cliff;
        }

        public void SetTerrainKind(Int2 cell, TerrainKind kind)
        {
            if (Contains(cell))
                terrainKinds[Index(cell)] = kind;
        }

        public bool HasBuildingAt(Int2 cell)
        {
            return buildingOccupancy.ContainsKey(cell);
        }

        public bool TryGetBuildingAt(Int2 cell, out ActorId actorId)
        {
            return buildingOccupancy.TryGetValue(cell, out actorId);
        }

        public void OccupyBuilding(Int2 topLeft, Int2 footprint, ActorId actorId)
        {
            for (var y = 0; y < footprint.Y; y++)
                for (var x = 0; x < footprint.X; x++)
                    buildingOccupancy[new Int2(topLeft.X + x, topLeft.Y + y)] = actorId;
        }

        public void ClearBuilding(Int2 topLeft, Int2 footprint)
        {
            for (var y = 0; y < footprint.Y; y++)
                for (var x = 0; x < footprint.X; x++)
                    buildingOccupancy.Remove(new Int2(topLeft.X + x, topLeft.Y + y));
        }

        public bool IsPassableForUnit(Int2 cell)
        {
            return IsPassableForUnit(cell, MovementClass.Wheeled, null);
        }

        public bool IsPassableForUnit(Int2 cell, MovementClass movementClass, RtsRules rules)
        {
            if (!Contains(cell) || IsBlocked(cell) || HasBuildingAt(cell))
                return false;

            return TerrainAllows(cell, movementClass, rules);
        }

        public bool IsBuildableCell(Int2 cell, RtsRules rules)
        {
            return Contains(cell) && !IsBlocked(cell) && !HasBuildingAt(cell) && TerrainAllows(cell, MovementClass.Building, rules);
        }

        public int GetMovementCost(Int2 cell, MovementClass movementClass, RtsRules rules)
        {
            var definition = GetTerrainDefinition(cell, rules);
            return definition.CostFor(movementClass);
        }

        public IReadOnlyList<TerrainCellState> CopyTerrainCells()
        {
            var cells = new List<TerrainCellState>();
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = new Int2(x, y);
                    cells.Add(new TerrainCellState(cell, GetTerrainKind(cell)));
                }

            return cells;
        }

        bool TerrainAllows(Int2 cell, MovementClass movementClass, RtsRules rules)
        {
            return GetTerrainDefinition(cell, rules).Allows(movementClass);
        }

        TerrainDefinition GetTerrainDefinition(Int2 cell, RtsRules rules)
        {
            var kind = GetTerrainKind(cell);
            TerrainDefinition definition;
            if (rules != null && rules.TryGetTerrainDefinition(kind, out definition))
                return definition;

            foreach (var fallback in TerrainCatalog.CreateDefaultDefinitions())
                if (fallback.Kind == kind)
                    return fallback;

            return new TerrainDefinition(TerrainKind.Cliff, "Unknown", 99, PassabilityMask.None, "unknown");
        }

        int Index(Int2 cell)
        {
            return cell.Y * Width + cell.X;
        }
    }
}
