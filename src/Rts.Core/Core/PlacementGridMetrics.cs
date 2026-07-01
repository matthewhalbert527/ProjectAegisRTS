namespace ProjectAegisRTS.Core
{
    public static class PlacementGridMetrics
    {
        public const int PlacementGridScale = 2;
        public const int PlacementCellFixedScale = FixedMath.CellScale / PlacementGridScale;

        public static Int2 CoarseCellToPlacementCell(Int2 coarseCell)
        {
            return new Int2(coarseCell.X * PlacementGridScale, coarseCell.Y * PlacementGridScale);
        }

        public static Int2 PlacementCellToCoarseCell(Int2 placementCell)
        {
            return new Int2(placementCell.X / PlacementGridScale, placementCell.Y / PlacementGridScale);
        }

        public static Int2 CoarseFootprintToPlacementFootprint(Int2 coarseFootprint)
        {
            return new Int2(coarseFootprint.X * PlacementGridScale, coarseFootprint.Y * PlacementGridScale);
        }

        public static Int2 PlacementCellCenterFixed(Int2 placementCell)
        {
            return new Int2(
                placementCell.X * PlacementCellFixedScale + PlacementCellFixedScale / 2,
                placementCell.Y * PlacementCellFixedScale + PlacementCellFixedScale / 2);
        }

        public static Int2 PlacementFootprintCenterFixed(Int2 topLeftPlacementCell, Int2 placementFootprint)
        {
            return new Int2(
                topLeftPlacementCell.X * PlacementCellFixedScale + placementFootprint.X * PlacementCellFixedScale / 2,
                topLeftPlacementCell.Y * PlacementCellFixedScale + placementFootprint.Y * PlacementCellFixedScale / 2);
        }
    }
}
