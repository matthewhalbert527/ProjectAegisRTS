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
        readonly Dictionary<Int2, ActorId> placementBuildingOccupancy;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PlacementGridScale { get { return PlacementGridMetrics.PlacementGridScale; } }
        public int PlacementWidth { get { return Width * PlacementGridScale; } }
        public int PlacementHeight { get { return Height * PlacementGridScale; } }

        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            blocked = new bool[width * height];
            terrainKinds = new TerrainKind[width * height];
            buildingOccupancy = new Dictionary<Int2, ActorId>();
            placementBuildingOccupancy = new Dictionary<Int2, ActorId>();
        }

        public bool Contains(Int2 cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }

        public bool ContainsPlacementCell(Int2 placementCell)
        {
            return placementCell.X >= 0 &&
                placementCell.Y >= 0 &&
                placementCell.X < PlacementWidth &&
                placementCell.Y < PlacementHeight;
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

        public bool HasBuildingAtPlacementCell(Int2 placementCell)
        {
            return placementBuildingOccupancy.ContainsKey(placementCell);
        }

        public bool TryGetBuildingAt(Int2 cell, out ActorId actorId)
        {
            return buildingOccupancy.TryGetValue(cell, out actorId);
        }

        public bool TryGetBuildingAtPlacementCell(Int2 placementCell, out ActorId actorId)
        {
            return placementBuildingOccupancy.TryGetValue(placementCell, out actorId);
        }

        public void OccupyBuilding(Int2 topLeft, Int2 footprint, ActorId actorId)
        {
            OccupyBuildingAtPlacement(
                PlacementGridMetrics.CoarseCellToPlacementCell(topLeft),
                PlacementGridMetrics.CoarseFootprintToPlacementFootprint(footprint),
                actorId);
        }

        public void OccupyBuildingAtPlacement(Int2 topLeftPlacementCell, Int2 placementFootprint, ActorId actorId)
        {
            for (var y = 0; y < placementFootprint.Y; y++)
                for (var x = 0; x < placementFootprint.X; x++)
                    placementBuildingOccupancy[new Int2(topLeftPlacementCell.X + x, topLeftPlacementCell.Y + y)] = actorId;

            RebuildCoarseBuildingOccupancy();
        }

        public void ClearBuilding(Int2 topLeft, Int2 footprint)
        {
            ClearBuildingAtPlacement(
                PlacementGridMetrics.CoarseCellToPlacementCell(topLeft),
                PlacementGridMetrics.CoarseFootprintToPlacementFootprint(footprint));
        }

        public void ClearBuildingAtPlacement(Int2 topLeftPlacementCell, Int2 placementFootprint)
        {
            for (var y = 0; y < placementFootprint.Y; y++)
                for (var x = 0; x < placementFootprint.X; x++)
                    placementBuildingOccupancy.Remove(new Int2(topLeftPlacementCell.X + x, topLeftPlacementCell.Y + y));

            RebuildCoarseBuildingOccupancy();
        }

        public bool IsPassableForUnit(Int2 cell)
        {
            return IsPassableForUnit(cell, MovementClass.Wheeled, null);
        }

        public bool IsPassableForUnit(Int2 cell, MovementClass movementClass, RtsRules rules)
        {
            if (!Contains(cell) || IsBlocked(cell))
                return false;

            if (movementClass != MovementClass.Aircraft && HasBuildingAt(cell))
                return false;

            return TerrainAllows(cell, movementClass, rules);
        }

        public bool IsBuildableCell(Int2 cell, RtsRules rules)
        {
            return Contains(cell) && !IsBlocked(cell) && !HasBuildingAt(cell) && TerrainAllows(cell, MovementClass.Building, rules);
        }

        public bool IsBuildablePlacementCell(Int2 placementCell, RtsRules rules)
        {
            if (!ContainsPlacementCell(placementCell) || HasBuildingAtPlacementCell(placementCell))
                return false;

            var coarseCell = PlacementGridMetrics.PlacementCellToCoarseCell(placementCell);
            return Contains(coarseCell) && !IsBlocked(coarseCell) && TerrainAllows(coarseCell, MovementClass.Building, rules);
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

        void RebuildCoarseBuildingOccupancy()
        {
            buildingOccupancy.Clear();
            foreach (var pair in placementBuildingOccupancy)
                buildingOccupancy[PlacementGridMetrics.PlacementCellToCoarseCell(pair.Key)] = pair.Value;
        }

        int Index(Int2 cell)
        {
            return cell.Y * Width + cell.X;
        }
    }
}
