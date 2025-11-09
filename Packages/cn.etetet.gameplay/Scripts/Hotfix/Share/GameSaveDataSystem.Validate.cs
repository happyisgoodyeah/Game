using System;

namespace ET
{
    public static partial class GameSaveDataSystem
    {
        /// <summary>
        /// 验证存档数据完整性
        /// </summary>
        public static bool Validate(this GameSaveData self)
        {
            try
            {
                var header = self.GetHeader();
                if (header == null)
                {
                    Log.Error("存档头信息缺失");
                    return false;
                }

                // 验证头信息
                if (!ValidateHeader(header))
                    return false;

                // 验证玩家数据
                var playerData = self.GetPlayerData();
                if (playerData == null)
                {
                    Log.Error("玩家数据缺失");
                    return false;
                }

                if (!ValidatePlayerData(playerData))
                    return false;

                // 验证玩家ID一致性
                if (header.PlayerId != playerData.PlayerId)
                {
                    Log.Error($"玩家ID不一致: Header={header.PlayerId}, PlayerData={playerData.PlayerId}");
                    return false;
                }

                Log.Info("存档数据验证通过");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"存档验证异常: {e}");
                return false;
            }
        }
        
        /// <summary>
        /// 存档头数据验证
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private static bool ValidateHeader(SaveDataHeaderComponent header)
        {
            if (string.IsNullOrEmpty(header.PlayerId))
            {
                Log.Error("存档玩家ID为空");
                return false;
            }

            if (string.IsNullOrEmpty(header.SaveSlot))
            {
                Log.Error("存档槽位为空");
                return false;
            }

            if (header.CreateTime > DateTime.Now)
            {
                Log.Error("存档创建时间异常");
                return false;
            }

            if (header.LastSaveTime > DateTime.Now)
            {
                Log.Error("存档最后保存时间异常");
                return false;
            }

            if (header.TotalPlayTime < 0)
            {
                Log.Error("总游戏时间为负数");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// 玩家数据验证
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        private static bool ValidatePlayerData(PlayerDataComponent playerData)
        {
            if (string.IsNullOrEmpty(playerData.PlayerId))
            {
                Log.Error("玩家ID为空");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// 更新最后保存时间
        /// </summary>
        public static void UpdateSaveTime(this GameSaveData self)
        {
            var header = self.GetHeader();
            if (header != null)
            {
                header.LastSaveTime = DateTime.Now;
                header.LastModified = DateTime.Now;
            }

            // 更新所有数据组件的最后修改时间
            foreach (var component in self.Components.Values)
            {
                if (component is ISaveDataComponent saveComponent)
                {
                    saveComponent.LastModified = DateTime.Now;
                }
            }
        }
    }
}