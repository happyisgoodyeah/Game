using MemoryPack;

namespace ET
{
    [MemoryPackable]
    [ChildOf(typeof(SaveManagerComponent))]
    public partial class GameSaveData : Entity , IAwake<string , string , string> , ISerialize , IDeserialize , ISerializeToEntity
    {
        
    }
}
