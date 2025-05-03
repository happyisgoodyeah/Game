namespace ET
{
    [ChildOf(typeof(Grid))]
    public partial class Slot : Entity, IAwake<int>
    {
        public int configId;
    }
}