namespace ET
{   
    /// <summary>
    /// 存档验证Handle接口
    /// </summary>
    public interface ISaveValidationHandler
    {
        bool Handle(Entity entity);
    }    
}