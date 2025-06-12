using System;
using System.Collections;
using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(Grid))]
    public partial class Puzzle : Entity, IAwake<int,int>
    {
        public int configId;
        
        /// <summary>
        /// 下标位置 先这样随便写了
        /// </summary>
        public int positionIndex;

        /// <summary>
        /// 当前绑定的Slot
        /// </summary>
        public List<EntityRef<Slot>> slots = new List<EntityRef<Slot>>();
    }
}