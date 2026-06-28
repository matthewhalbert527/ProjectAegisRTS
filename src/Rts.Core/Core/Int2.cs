using System;

namespace ProjectAegisRTS.Core
{
    public readonly struct Int2 : IEquatable<Int2>
    {
        public readonly int X;
        public readonly int Y;

        public Int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Int2 Zero { get { return new Int2(0, 0); } }

        public static Int2 operator +(Int2 a, Int2 b)
        {
            return new Int2(a.X + b.X, a.Y + b.Y);
        }

        public static Int2 operator -(Int2 a, Int2 b)
        {
            return new Int2(a.X - b.X, a.Y - b.Y);
        }

        public int ManhattanDistanceTo(Int2 other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public bool Equals(Int2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Int2 && Equals((Int2)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return X.ToString() + "," + Y.ToString();
        }
    }
}
