using System.Collections.Generic;
using MemoryPack;

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
    [MemoryPackable]
    public partial class SerializableList<T> : List<T>
    {
        [MemoryPackConstructor]
        public SerializableList() : base() { }
        public SerializableList(int capacity) : base(capacity) { }
        public SerializableList(IEnumerable<T> collection) : base(collection) { }
    }
}
