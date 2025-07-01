using MemoryPack;

namespace ET
{
    // 二维整数向量
    public partial struct IntVector2
    {
        public int X;
        public int Y;
        
        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public static IntVector2 operator +(IntVector2 a, IntVector2 b) 
            => new IntVector2(a.X + b.X, a.Y + b.Y);
        
        public static IntVector2 operator -(IntVector2 a, IntVector2 b) 
            => new IntVector2(a.X - b.X, a.Y - b.Y);
        
        public static bool operator ==(IntVector2 a, IntVector2 b) 
            => a.X == b.X && a.Y == b.Y;
        
        public static bool operator !=(IntVector2 a, IntVector2 b) 
            => !(a == b);
        
        public override bool Equals(object obj) 
            => obj is IntVector2 other && this == other;
        
        public override int GetHashCode() 
            => (X << 16) | Y;
        
        public override string ToString() 
            => $"({X}, {Y})";
        
        [StaticField]
        public static readonly IntVector2 Zero = new IntVector2(0, 0);
        [StaticField]
        public static readonly IntVector2 One = new IntVector2(1, 1);
        [StaticField]
        public static readonly IntVector2 Up = new IntVector2(0, 1);
        [StaticField]
        public static readonly IntVector2 Down = new IntVector2(0, -1);
        [StaticField]
        public static readonly IntVector2 Left = new IntVector2(-1, 0);
        [StaticField]
        public static readonly IntVector2 Right = new IntVector2(1, 0);
    }
}
