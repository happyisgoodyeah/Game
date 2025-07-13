using System;
using System.Collections;
using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(Grid))]
    public partial class Puzzle : Entity, IAwake<int, int>
    {
        /// <summary>
        /// 配置ConfigId
        /// </summary>
        public int configId;

        /// <summary>
        /// 下标位置 先这样随便写了
        /// </summary>
        public int positionIndex;

        /// <summary>
        /// 当前puzzle的slot
        /// </summary>
        public List<EntityRef<Slot>> slots = new List<EntityRef<Slot>>();

        /// <summary>
        /// slot偏移量数组
        /// </summary>
        public List<IntVector2> slotOffset = new List<IntVector2>();

        /// <summary>
        /// 当前绑定的Slot
        /// </summary>
        public List<EntityRef<Slot>> bindSlots = new List<EntityRef<Slot>>();
    }
}