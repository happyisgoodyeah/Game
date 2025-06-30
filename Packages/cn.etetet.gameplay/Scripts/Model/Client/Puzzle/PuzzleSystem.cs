namespace ET
{
    [EntitySystemOf(typeof(Puzzle))]
    [FriendOf((typeof(Slot)))]
    public static partial class PuzzleSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Puzzle self, int configId, int positionId)
        {
            self.configId = configId;
            self.positionIndex = positionId;
            self.Init();
        }
        public static PuzzleConfig Config(this Puzzle self)
        {
            return PuzzleConfigCategory.Instance.Get(self.configId);
        }

        /// <summary>
        /// 初始化 根据配置同步信息 生成slot数据层
        /// </summary>
        /// <param name="self"></param>
        public static void Init(this Puzzle self)
        {
            var spawn = self.AddComponent<SlotSpawnComponent>();
            var list = self.Config().SlotOffset;
            for (int i = 0; i < list.Count; i++)
            {
                //拼图用slot ConfigID为1000 偏移量为二维数组坐标
                spawn.PuzzleSpawnSlot(1000 , new IntVector2(list[i][0] , list[i][1]));
            }
        }
        
        /// <summary>
        /// 重置拼图绑定的所有格子
        /// </summary>
        /// <param name="self"></param>
        public static void ResetBindSlots(this ET.Puzzle self)
        {
            foreach (var slotRef in self.bindSlots)
            {
                var slot = slotRef.Entity; 
                slot.puzzleRef = default;
            }
            self.bindSlots.Clear();
        }
    }
}