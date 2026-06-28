using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Simulation
{
    public sealed class GridMap
    {
        readonly bool[] blocked;
        readonly int[] terrainFlags;
        readonly Dictionary<Int2, ActorId> buildingOccupancy;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            blocked = new bool[width * height];
            terrainFlags = new int[width * height];
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
            return Contains(cell) ? terrainFlags[Index(cell)] : 0;
        }

        public void SetTerrainFlags(Int2 cell, int flags)
        {
            if (Contains(cell))
                terrainFlags[Index(cell)] = flags;
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
            return Contains(cell) && !IsBlocked(cell) && !HasBuildingAt(cell);
        }

        int Index(Int2 cell)
        {
            return cell.Y * Width + cell.X;
        }
    }
}
