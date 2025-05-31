using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(Scene))]
    public partial class Grid : Entity, IAwake<int>
    {
        //配置id
        public int configId;

        public IntVector2 gridSize;
        public int cellSize;
        
        /// <summary>
        /// SlotDic
        /// </summary>
        public DictionaryComponent<long , EntityRef<Slot>> slotDic = new DictionaryComponent<long, EntityRef<Slot>>();
        
        /// <summary>
        /// PuzzleDic
        /// </summary>
        public DictionaryComponent<long , EntityRef<Puzzle>> PuzzleDic = new DictionaryComponent<long, EntityRef<Puzzle>>();
    }
}