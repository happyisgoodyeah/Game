using System;

namespace ET
{
    [SaveValidation(typeof(SaveDataHeaderComponent))]
    public class SaveHeaderValidationHandler : ISaveValidationHandler
    {
        public bool Handle(Entity entity)
        {
            SaveDataHeaderComponent header = entity as SaveDataHeaderComponent;
            if (header == null) return false;

            // 验证必要字段
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
    }    
}