namespace ET
{
    [SaveValidation(typeof(PlayerDataComponent))]
    public class PlayerDataValidationHandler : ISaveValidationHandler
    {
        public bool Handle(Entity entity)
        {
            PlayerDataComponent playerData = entity as PlayerDataComponent;
            if (playerData == null) return false;

            // 验证必要字段
            if (string.IsNullOrEmpty(playerData.PlayerId))
            {
                Log.Error("玩家ID为空");
                return false;
            }

            if (string.IsNullOrEmpty(playerData.PlayerName))
            {
                Log.Error("玩家名为空");
                return false;
            }
            
            return true;
        }
    }    
}