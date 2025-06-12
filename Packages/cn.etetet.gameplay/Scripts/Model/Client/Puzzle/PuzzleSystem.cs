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
        }
        public static PuzzleConfig Config(this Puzzle self)
        {
            return PuzzleConfigCategory.Instance.Get(self.configId);
        }

        public static void InitComponent(this Puzzle self, IntVector2 position, int rotation)
        {
            //self.AddComponent<PuzzleDataComponent, IntVector2, int>(new IntVector2(0, 0), 0);
        }
        /// <summary>
        /// 重置拼图绑定的所有格子
        /// </summary>
        /// <param name="self"></param>
        public static void ResetSlots(this ET.Puzzle self)
        {
            
            foreach (var slotRef in self.slots)
            {
                var slot = slotRef.Entity;
                slot.puzzleRef = default;
            }
            self.slots.Clear();
        }
    }
}