using MemoryPack;

namespace ET
{
    [EntitySystemOf(typeof(PlayerDataComponent))]
    public static partial class PlayerDataComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.PlayerDataComponent self)
        {
            self.UnlockedLevels = new SerializableList<long>() { GridConfigCategory.Instance.DataList[0].Id };
        }

        /// <summary>
        /// 是否通关关卡
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool CheckLevelPass(this ET.PlayerDataComponent self, long id)
        {
            return self.PassLevels.Contains(id);
        }

        /// <summary>
        /// 是否解锁关卡
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool CheckLevelUnlock(this ET.PlayerDataComponent self, long id)
        {
            return self.UnlockedLevels.Contains(id);
        }

        /// <summary>
        /// 添加解锁的关卡到列表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        public static void UnlockLevel(this ET.PlayerDataComponent self, long id)
        {
            if (!self.UnlockedLevels.Contains(id))
            {
                self.UnlockedLevels.Add(id);    
            }
        }

        public static void PassLevel(this ET.PlayerDataComponent self, long id)
        {
            self.PassLevels.Add(id);
            //一般来说过关关卡会解锁下一关卡
            UnlockLevel(self, id + 1);
        }
    }
}