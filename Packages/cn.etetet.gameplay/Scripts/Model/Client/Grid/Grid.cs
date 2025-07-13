using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(Scene))]
    public partial class Grid : Entity, IAwake<int>
    {
        /// <summary>
        /// 配置id
        /// </summary>
        public int configId;
        
        /// <summary>
        /// Grid行列大小
        /// </summary>
        public IntVector2 gridSize;
        
        /// <summary>
        /// 单元格大小
        /// </summary>
        public int cellSize;
        
        /// <summary>
        /// SlotDic
        /// </summary>
        public DictionaryComponent<IntVector2 , EntityRef<Slot>> slotDic = new DictionaryComponent<IntVector2, EntityRef<Slot>>();
        
        /// <summary>
        /// PuzzleDic
        /// </summary>
        public DictionaryComponent<long , EntityRef<Puzzle>> PuzzleDic = new DictionaryComponent<long, EntityRef<Puzzle>>();
    }
}