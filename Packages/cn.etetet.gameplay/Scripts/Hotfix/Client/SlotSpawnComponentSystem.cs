namespace ET
{
    [EntitySystemOf(typeof(SlotSpawnComponent))]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(Grid))]
    public static partial class SlotSpawnComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.SlotSpawnComponent self)
        {
        }

        /// <summary>
        /// Grid生成Slot
        /// </summary>
        /// <param name="self"></param>
        /// <param name="configId"></param>
        /// <param name="spawnPosition"></param>
        public static Slot GridSpawnSlot(this SlotSpawnComponent self, int configId , IntVector2 spawnPosition)
        {
            var grid = self.GetParent<Grid>();
            var slot = grid.AddChild<Slot, int , IntVector2>(configId , spawnPosition);
            EventSystem.Instance.Publish(self.Scene(), new AfterCreateSlot(){slot = slot});
            return slot;
        }
        
        /// <summary>
        /// Puzzle生成Slot
        /// </summary>
        /// <param name="self"></param>
        /// <param name="configId"></param>
        /// <param name="offset"></param>
        public static Slot PuzzleSpawnSlot(this SlotSpawnComponent self, int configId , IntVector2 offset)
        {
            var puzzle = self.GetParent<Puzzle>();
            var slot = puzzle.AddChild<Slot, int , IntVector2>(configId , offset);
            puzzle.slotOffset.Add(offset);
            EventSystem.Instance.Publish(self.Scene(), new AfterCreateSlot(){slot = slot});
            return slot;
        }
    }
}