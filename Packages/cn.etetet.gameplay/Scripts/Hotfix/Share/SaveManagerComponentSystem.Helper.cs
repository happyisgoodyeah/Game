namespace ET
{
    /// <summary>
    /// 统一各数据组件方法到此 避免调用链过长
    /// </summary>
    public static partial class SaveManagerComponentSystem_Helper
    {
        /// <summary>
        /// 获取存档头数据
        /// </summary>
        public static SaveDataHeaderComponent GetHeader(this SaveManagerComponent self)
        {
            return self.CurrentSaveData.GetComponent<SaveDataHeaderComponent>();
        }
        
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public static PlayerDataComponent GetPlayerData(this SaveManagerComponent self)
        {
            return self.CurrentSaveData.GetComponent<PlayerDataComponent>();
        }
    }    
}