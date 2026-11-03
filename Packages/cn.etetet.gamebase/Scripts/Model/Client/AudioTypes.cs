namespace ET
{
    public enum AudioGroup
    {
        Music = 0,
        Sound = 1,
        UI = 2,
        Ambient = 3,
    }

    public enum AudioPlayStatus
    {
        Failed = 0,
        Started,
        AlreadyPlaying,
        InvalidConfig,
        Cooldown,
        CapacityReached,
        Cancelled,
    }

    /// <summary>Handle 为本次播放实体 ID；0 表示没有可控制的播放实例。</summary>
    public struct AudioPlayResult
    {
        public long Handle;
        public AudioPlayStatus Status;
    }

    /// <summary>供 Hotfix 发布，由客户端 HotfixView 消费。只包含配置名称。</summary>
    public struct PlayAudioRequested
    {
        public string Key;
    }

    /// <summary>旧接口兼容。新业务使用配置 Key 或 ID，不再扩展该枚举。</summary>
    public enum AUDIO
    {
        bg1 = 0,
        bg2 = 1,
    }
}
