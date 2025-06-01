namespace ET
{
    [ChildOf(typeof(Grid))]
    public partial class Slot : Entity, IAwake<int , IntVector2>
    {
        public int configId;
        public IntVector2 position;
        
        /// <summary>
        /// 当前绑定的Puzzle
        /// </summary>
        public EntityRef<Puzzle> puzzleRef;
    }
}