using System;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [MemoryPackable]
    [ComponentOf(typeof(GameSaveData))]
    public partial class SaveDataHeaderComponent : Entity , IAwake , ISerializeToEntity , ISerialize , IDeserialize , ISaveDataComponent
    {
        /// <summary>
        /// 数据版本
        /// </summary>
        [MemoryPackOrder(0)]
        [BsonElement]
        public string SaveVersion { get; set; } = "1.0.0";
        
        /// <summary>
        /// 游戏版本
        /// </summary>
        [MemoryPackOrder(1)]
        [BsonElement]
        public string GameVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        [MemoryPackOrder(2)]
        [BsonElement]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 最后保存时间
        /// </summary>
        [MemoryPackOrder(3)]
        [BsonElement]
        public DateTime LastSaveTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 玩家id
        /// </summary>
        [MemoryPackOrder(4)]
        [BsonElement]
        public string PlayerId { get; set; } = string.Empty;
        
        /// <summary>
        /// 存档位
        /// </summary>
        [MemoryPackOrder(5)]
        [BsonElement]
        public string SaveSlot { get; set; } = "default";
        
        /// <summary>
        /// 总计游戏时长
        /// </summary>
        [MemoryPackOrder(6)]
        [BsonElement]
        public double TotalPlayTime { get; set; } = 0;
        
        /// <summary>
        /// 存档大小
        /// </summary>
        [MemoryPackOrder(7)]
        [BsonElement]
        public long SaveSize { get; set; } = 0;
        
        /// <summary>
        /// 数据校验和
        /// </summary>
        [MemoryPackOrder(8)]
        [BsonElement]
        public string Checksum { get; set; } = string.Empty;
        
        [MemoryPackOrder(9)]
        [BsonElement]
        public string Description { get; set; } = string.Empty;
        
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
