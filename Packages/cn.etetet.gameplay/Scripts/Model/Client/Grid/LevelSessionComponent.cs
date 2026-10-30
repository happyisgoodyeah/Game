namespace ET
{
    /// <summary>
    /// 保存当前关卡及其加载状态。运行中的关卡由本组件统一管理。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public partial class LevelSessionComponent : Entity, IAwake
    {
        public EntityRef<Grid> CurrentGrid;
        public long Version;
        public bool IsLoading;
        public bool IsReturning;
        public bool IsCompleting;
    }
}
