namespace ET
{
    [EntitySystemOf(typeof(Slot))]
    public static partial class SlotSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Slot self, int configId, ET.IntVector2 position)
        {
            self.configId = configId;
            self.position = position;
        }
        public static SlotConfig Config(this Slot self)
        {
            return SlotConfigCategory.Instance.Get(self.configId);
        }
    }
}