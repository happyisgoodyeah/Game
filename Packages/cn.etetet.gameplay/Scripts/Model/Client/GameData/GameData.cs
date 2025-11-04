using MemoryPack;

namespace ET
{
    [EnableClass]
    [MemoryPackable]
    public partial class GameData
    {
        public string name { get; set; }
        public string descript { get; set; }
    }
    
    [MemoryPackable]
    public partial struct ArchiveData
    {
        public bool isUnlock { get; set; }
    }
}