namespace ET
{
    [EntitySystemOf(typeof(SlotSpawnComponent))]
    public static partial class SlotSpawnComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.SlotSpawnComponent self)
        {
        }

        /// <summary>
        /// 生成Puzzle
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spawnPosition"></param>
        public static Slot SpawnSlot(this SlotSpawnComponent self, int slotId , IntVector2 spawnPosition)
        {
            var grid = self.GetParent<Grid>();
            var slot = grid.AddChild<Slot, int , IntVector2>(slotId , spawnPosition);
            EventSystem.Instance.Publish(self.Scene(), new AfterCreateSlot(){slot = slot});
            return slot;
        }
    }
}