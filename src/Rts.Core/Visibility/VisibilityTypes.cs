using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Visibility
{
    public enum CellVisibility
    {
        Unexplored,
        Explored,
        Visible
    }

    public sealed class SightDefinition
    {
        public int RadiusCells { get; private set; }

        public SightDefinition(int radiusCells)
        {
            RadiusCells = radiusCells < 0 ? 0 : radiusCells;
        }
    }

    public sealed class RadarDefinition
    {
        public bool ProvidesRadar { get; private set; }
        public int RadiusCells { get; private set; }

        public RadarDefinition(bool providesRadar, int radiusCells)
        {
            ProvidesRadar = providesRadar;
            RadiusCells = radiusCells < 0 ? 0 : radiusCells;
        }
    }

    public sealed class PlayerVisibilityState
    {
        readonly CellVisibility[] cells;

        public int PlayerId { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public PlayerVisibilityState(int playerId, int width, int height)
        {
            PlayerId = playerId;
            Width = width;
            Height = height;
            cells = new CellVisibility[width * height];
        }

        public void BeginUpdate()
        {
            for (var i = 0; i < cells.Length; i++)
                if (cells[i] == CellVisibility.Visible)
                    cells[i] = CellVisibility.Explored;
        }

        public void RevealCell(Int2 cell)
        {
            if (!Contains(cell))
                return;

            cells[Index(cell)] = CellVisibility.Visible;
        }

        public CellVisibility GetCell(Int2 cell)
        {
            return Contains(cell) ? cells[Index(cell)] : CellVisibility.Unexplored;
        }

        public bool IsVisible(Int2 cell)
        {
            return GetCell(cell) == CellVisibility.Visible;
        }

        public bool IsExploredOrVisible(Int2 cell)
        {
            var visibility = GetCell(cell);
            return visibility == CellVisibility.Explored || visibility == CellVisibility.Visible;
        }

        public CellVisibility[] CopyCells()
        {
            var copy = new CellVisibility[cells.Length];
            for (var i = 0; i < cells.Length; i++)
                copy[i] = cells[i];
            return copy;
        }

        bool Contains(Int2 cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }

        int Index(Int2 cell)
        {
            return cell.Y * Width + cell.X;
        }
    }
}
