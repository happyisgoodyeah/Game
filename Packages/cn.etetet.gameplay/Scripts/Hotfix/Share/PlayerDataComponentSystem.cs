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
        public static bool CheckLevelPass(this ET.PlayerDataComponent self , long id)
        {
            return self.PassLevels.Contains(id);
        }
        
        /// <summary>
        /// 是否通关关卡
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool CheckLevelUnlock(this ET.PlayerDataComponent self , long id)
        {
            return self.UnlockedLevels.Contains(id);
        }
    }
}