namespace ET
{
    [ChildOf(typeof(Grid))]
    public partial class Puzzle : Entity, IAwake<int>
    {
        public int configId;
    }
}