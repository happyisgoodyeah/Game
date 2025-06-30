namespace ET
{
    /// <summary>
    /// 创建完Grid数据层
    /// </summary>
    public struct AfterCreateGrid
    {
        public Grid grid;
    }
    

    /// <summary>
    /// 创建完Puzzle数据层
    /// </summary>
    public struct AfterCreatePuzzle
    {
        public Puzzle puzzle;
        public int index;
    }
    
    /// <summary>
    /// 创建完Slot数据层
    /// </summary>
    public struct AfterCreateSlot
    {
        public Slot slot;
        public int count;
    }
}
