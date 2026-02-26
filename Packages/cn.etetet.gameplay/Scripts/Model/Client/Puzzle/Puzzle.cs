using System;
using System.Collections;
using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(Grid))]
    public partial class Puzzle : Entity, IAwake<int , FloatVector2>
    {
        /// <summary>
        /// 配置ConfigId
        /// </summary>
        public int configId;

        /// <summary>
        /// 当前拼图旋转
        /// </summary>
        public int rotate;
        
        /// <summary>
        /// 当前拼图移动模式
        /// </summary>
        public PuzzleMoveModeType moveMode;

        /// <summary>
        /// 当前整个拼图是否在Grid内
        /// </summary>
        public bool isInGrid;

        /// <summary>
        /// 原点slot
        /// </summary>
        public Slot originSlot => slots[0];
        
        /// <summary>
        /// 场景初始坐标
        /// </summary>
        public FloatVector2 viewPosition;
        
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
        
        /// <summary>
        /// 吸附slot
        /// </summary>
        public List<EntityRef<Slot>> adsorptionSlots = new List<EntityRef<Slot>>();
    }
    
    public enum PuzzleMoveModeType
    {
        /// <summary>
        /// 普通移动模式：Grid外自由拖拽
        /// </summary>
        Normal,
        
        /// <summary>
        /// 吸附移动模式：Grid内按格子吸附移动
        /// </summary>
        Adsorption,
        
        /// <summary>
        /// 准备吸附模式：刚进入Grid，执行对齐动画的过渡状态
        /// 动画完成后自动切换为Adsorption
        /// </summary>
        ReadyAdsorption,
    }
    
    
    /// <summary>
    /// 吸附方向枚举
    /// </summary>
    public enum SnapDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        UpRight,
        UpLeft,
        DownRight,
        DownLeft,
    }
}