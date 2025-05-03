namespace ET
{
    [ChildOf(typeof(Scene))]
    public partial class Grid : Entity, IAwake<int>
    {
        //配置id
        public int configId;

        public IntVector2 gridSize;
        public int cellSize;
    }
}