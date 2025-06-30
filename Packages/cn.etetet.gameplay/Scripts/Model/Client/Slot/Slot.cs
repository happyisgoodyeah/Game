namespace ET
{
    [ChildOf()]
    public partial class Slot : Entity, IAwake<int , IntVector2>
    {
        /// <summary>
        /// 配置ConfigId
        /// </summary>
        public int configId;
        
        /// <summary>
        /// 坐标
        /// </summary>
        public IntVector2 position;
        
        /// <summary>
        /// 当前绑定的Puzzle
        /// </summary>
        public EntityRef<Puzzle> puzzleRef;
    }
}