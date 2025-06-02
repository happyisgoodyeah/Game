using UnityEngine;

namespace ET
{
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
}