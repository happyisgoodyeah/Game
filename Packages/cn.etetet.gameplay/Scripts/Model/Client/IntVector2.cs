using MemoryPack;

namespace ET
{
    [MemoryPackable]
    public partial struct IntVector2
    {
        [MemoryPackIgnore]
        public int X;
        [MemoryPackIgnore]
        public int Y;
        
        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        // 手动序列化字段
        [MemoryPackInclude]
        private int SerializedX => X;
        [MemoryPackInclude]
        private int SerializedY => Y;
    }
}
