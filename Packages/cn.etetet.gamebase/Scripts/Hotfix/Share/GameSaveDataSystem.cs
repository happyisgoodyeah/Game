using System;

namespace ET
{
    [EntitySystemOf(typeof(GameSaveData))]
    public static partial class GameSaveDataSystem
    {
        [EntitySystem]
        private static void Awake(this ET.GameSaveData self, string playerId, string saveSlot , string gameVersion)
        {
            // 初始化存档头
            var header = self.AddComponent<SaveDataHeaderComponent>();
            header.PlayerId = playerId;
            header.SaveSlot = saveSlot;
            header.CreateTime = DateTime.Now;
            header.LastSaveTime = DateTime.Now;
            header.GameVersion = gameVersion;
            header.SaveVersion = "1.0.0";
            
            // 初始化玩家数据
            var playerData = self.AddComponent<PlayerDataComponent>();
            playerData.PlayerId = playerId;
            playerData.PlayerName = $"Player_{playerId}";
        }
        
        [EntitySystem]
        private static void Serialize(this ET.GameSaveData self)
        {
        
        }
        
        [EntitySystem]
        private static void Deserialize(this ET.GameSaveData self)
        {
        
        }
        
        /// <summary>
        /// 获取存档头信息
        /// </summary>
        public static SaveDataHeaderComponent GetHeader(this GameSaveData self)
        {
            return self.GetComponent<SaveDataHeaderComponent>();
        }
        
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public static PlayerDataComponent GetPlayerData(this GameSaveData self)
        {
            return self.GetComponent<PlayerDataComponent>();
        }
    }
}