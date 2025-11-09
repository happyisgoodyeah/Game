using System;

namespace ET
{
    [ComponentOf(typeof(Scene))]    
    public class SaveManagerComponent : Entity , IAwake , IDestroy
    {
        /// <summary>
        /// 游戏版本
        /// </summary>
        public string GameVersion;
        
        /// <summary>
        /// 存档目录
        /// </summary>
        public string SaveDirectory { get; set; } = string.Empty;
        
        /// <summary>
        /// 当前存档
        /// </summary>
        public GameSaveData CurrentSaveData { get; set; }
        
        /// <summary>
        /// 自动保存间隔（秒）
        /// </summary>
        public float AutoSaveInterval { get; set; } = 300f; // 5分钟
        
        /// <summary>
        /// 最后自动保存时间
        /// </summary>
        public DateTime LastAutoSaveTime { get; set; }
        
        /// <summary>
        /// 是否启用自动保存
        /// </summary>
        public bool AutoSaveEnabled { get; set; } = false;
    }
}
