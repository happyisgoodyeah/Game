namespace ET
{
    [ChildOf(typeof(Scene))]
    public partial class Grid : Entity , IAwake<int,int,int>
    {
        public IntVector2 gridSize;
        public int cellSize;
    }
}
