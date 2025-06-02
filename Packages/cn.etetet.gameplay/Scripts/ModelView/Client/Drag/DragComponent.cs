using UnityEngine;

namespace ET
{
    [ComponentOf()]
    public class DragComponent : Entity, IAwake, IUpdate
    {
        public bool IsDragging;
        public Vector3 StartWorldPos;
    }
}