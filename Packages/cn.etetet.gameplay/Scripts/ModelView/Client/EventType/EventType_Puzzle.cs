using UnityEngine;

namespace ET
{
    /// <summary>
    /// 拖拽拼图结束事件
    /// </summary>
    public struct DragPuzzleStartEvent
    {
        public Puzzle puzzle;
        public Vector3 worldPosition;
    }
    
    /// <summary>
    /// 拖拽拼图结束事件
    /// </summary>
    public struct DragPuzzleEndEvent
    {
        public Puzzle puzzle;
        public Vector3 worldPosition;
    }

    /// <summary>
    /// 单机旋转事件
    /// </summary>
    public struct ClickPuzzleRotateEvent
    {
        public Puzzle puzzle;
        public float angle;
    }
}
