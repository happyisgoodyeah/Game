using System;
using System.Collections.Generic;

namespace ET
{
    [ChildOf()]
    public class ArchiveCompont : Entity, IAwake
    {
        public List<ArchiveData> archiveList = new List<ArchiveData>();
        public string savePath;
    }
}