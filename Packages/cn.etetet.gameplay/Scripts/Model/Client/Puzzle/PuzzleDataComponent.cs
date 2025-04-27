namespace ET
{
    [ComponentOf(typeof(Puzzle))]
    public class PuzzleDataComponent : Entity , IAwake<IntVector2,int>
    {
        public IntVector2 puzzlePosition; // 逻辑坐标
        public int rotation;
    }
}
