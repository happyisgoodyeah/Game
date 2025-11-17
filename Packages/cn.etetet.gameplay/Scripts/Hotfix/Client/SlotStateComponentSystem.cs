namespace ET
{
    [EntitySystemOf(typeof(SlotStateComponent))]
    public static partial class SlotStateComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.SlotStateComponent self, bool allowPlace)
        {
            self.AllowPlace = allowPlace;
        }

        /// <summary>
        /// 能否放置
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool GetCanPlace(this ET.SlotStateComponent self)
        {
            return self.AllowPlace;
        }
    }
}