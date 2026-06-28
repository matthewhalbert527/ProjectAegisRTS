namespace ProjectAegisRTS.Core
{
    public static class FixedMath
    {
        public const int CellScale = 1024;

        public static int CellToWorld(int cell)
        {
            return cell * CellScale + CellScale / 2;
        }

        public static Int2 CellCenter(Int2 cell)
        {
            return new Int2(CellToWorld(cell.X), CellToWorld(cell.Y));
        }

        public static int WorldToCell(int world)
        {
            return world / CellScale;
        }

        public static int ClampStep(int delta, int maxStep)
        {
            if (delta > maxStep)
                return maxStep;
            if (delta < -maxStep)
                return -maxStep;
            return delta;
        }
    }
}
