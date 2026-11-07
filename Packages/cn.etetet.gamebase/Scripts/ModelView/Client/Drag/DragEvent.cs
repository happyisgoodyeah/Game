using UnityEngine;

namespace ET
{
    /// <summary>指针按下选中或释放，用于表现反馈；不代表开始移动或解除占格。</summary>
    public struct DragSelectionChangedEvent
    {
        public Entity Entity;
        public bool Selected;
    }

    //拖拽开始事件
    public struct DragStartEvent
    {
        public Entity Entity;
        public Vector3 StartPosition;
    }

    //拖拽更新事件
    public struct DragUpdateEvent
    {
        public Entity Entity;
        public Vector3 CurrentPosition;
    }

    //拖拽结束事件
    public struct DragEndEvent 
    {
        public Entity Entity;
        public Vector3 EndPosition;
    }
    
    /// <summary>
    /// 单击旋转事件
    /// RotType = 0 为拖拽的时候旋转；RotType = 1 为放置后旋转
    /// </summary>
    public struct ClickRotateEvent
    {
        public Entity Entity;
        public float Angle;
    }
}
