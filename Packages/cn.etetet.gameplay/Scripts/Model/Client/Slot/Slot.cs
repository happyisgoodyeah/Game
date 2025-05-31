namespace ET
{
    [ChildOf(typeof(Grid))]
    public partial class Slot : Entity, IAwake<int , IntVector2>
    {
        public int configId;
        public IntVector2 position;
    }
}