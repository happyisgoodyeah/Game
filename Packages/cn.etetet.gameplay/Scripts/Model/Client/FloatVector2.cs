using System;
using MemoryPack;

namespace ET
{
    // 二维浮点向量
    public partial struct FloatVector2
    {
        public float X;
        public float Y;
        
        public FloatVector2(float x, float y)
        {
            X = x;
            Y = y;
        }
        
        public static FloatVector2 operator +(FloatVector2 a, FloatVector2 b) 
            => new FloatVector2(a.X + b.X, a.Y + b.Y);
        
        public static FloatVector2 operator -(FloatVector2 a, FloatVector2 b) 
            => new FloatVector2(a.X - b.X, a.Y - b.Y);
        
        public static FloatVector2 operator *(FloatVector2 a, float scalar) 
            => new FloatVector2(a.X * scalar, a.Y * scalar);
        
        public static bool operator ==(FloatVector2 a, FloatVector2 b) 
            => Math.Abs(a.X - b.X) < float.Epsilon && 
                    Math.Abs(a.Y - b.Y) < float.Epsilon;
        
        public static bool operator !=(FloatVector2 a, FloatVector2 b) 
            => !(a == b);
        
        public override bool Equals(object obj) 
            => obj is FloatVector2 other && this == other;
        
        public override int GetHashCode() 
            => X.GetHashCode() ^ Y.GetHashCode();
        
        public override string ToString() 
            => $"({X:F2}, {Y:F2})";
        
        [StaticField]
        public static readonly FloatVector2 Zero = new FloatVector2(0, 0);
        
        [StaticField]
        public static readonly FloatVector2 One = new FloatVector2(1, 1);
    }
}
