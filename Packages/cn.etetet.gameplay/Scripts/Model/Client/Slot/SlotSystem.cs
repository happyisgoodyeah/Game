namespace ET
{
    [EntitySystemOf(typeof(Slot))]
    public static partial class SlotSystem
    {
        public static SlotConfig Config(this Slot self)
        {
            return SlotConfigCategory.Instance.Get(self.configId);
        }

        [EntitySystem]
        private static void Awake(this ET.Slot self, int configId)
        {
            self.configId = configId;
        }
    }
}