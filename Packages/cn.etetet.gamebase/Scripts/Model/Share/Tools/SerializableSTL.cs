using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 可序列化字典 - 用于MemoryPack序列化
    /// </summary>
    [EnableClass]
    [MemoryPackable]
    public partial class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
    {
        [MemoryPackIgnore] // 忽略 Comparer 属性
        public new IEqualityComparer<TKey> Comparer => base.Comparer;
        
        [MemoryPackConstructor]
        public SerializableDictionary() : base() { }
        public SerializableDictionary(int capacity) : base(capacity) { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    }

    /// <summary>
    /// 可序列化列表 - 用于MemoryPack序列化
    /// </summary>
    [EnableClass]
    [MemoryPackable(SerializeLayout.Explicit)]
    public partial class SerializableList<T> : List<T>
    {
        // 保留旧格式前两个成员的位置，使已有存档仍可被读取。
        [MemoryPackOrder(0)]
        [BsonIgnore]
        public new int Capacity
        {
            get => base.Capacity;
            set => base.Capacity = value;
        }

        [MemoryPackOrder(1)]
        [BsonIgnore]
        public new int Count => base.Count;

        /// <summary>
        /// MemoryPack 默认将 List 的派生类作为普通对象处理，需要显式保存元素。
        /// 旧存档只有 Capacity 和 Count，无法恢复其中从未写入的元素。
        /// </summary>
        [MemoryPackOrder(2)]
        [BsonIgnore]
        public T[] SerializedItems
        {
            get => this.ToArray();
            set
            {
                this.Clear();
                if (value != null)
                {
                    this.AddRange(value);
                }
            }
        }

        [MemoryPackConstructor]
        public SerializableList() : base() { }
        public SerializableList(int capacity) : base(capacity) { }
        public SerializableList(IEnumerable<T> collection) : base(collection) { }
    }
}
