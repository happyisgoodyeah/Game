using System;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ComponentOf(typeof(GameSaveData))]
    [MemoryPackable]
    public partial class PlayerDataComponent : Entity , IAwake, ISerializeToEntity , ISaveDataComponent
    {
        [MemoryPackOrder(0)]
        [BsonElement]
        public string PlayerId { get; set; } = string.Empty;
        
        [MemoryPackOrder(1)]
        [BsonElement]
        public string PlayerName { get; set; } = "New Player";
        
        /// <summary>
        /// 解锁的成就id
        /// </summary>
        [MemoryPackOrder(2)]
        [BsonElement]
        public SerializableList<string> UnlockedAchievements { get; set; } = new();
        
        /// <summary>
        /// 解锁的成关卡id
        /// </summary>
        [MemoryPackOrder(2)]
        [BsonElement]
        public SerializableList<string> UnlockedLevels { get; set; } = new();

        #region ISaveDataComponent接口实现

        /// <summary>
        /// 获取组件类型标识
        /// </summary>
        [MemoryPackIgnore]
        [BsonIgnore]
        public string ComponentType { get; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [MemoryPackIgnore]
        [BsonIgnore]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// 数据版本
        /// </summary>
        [MemoryPackIgnore]
        [BsonIgnore]
        public string DataVersion { get; }

        #endregion
    }
}
