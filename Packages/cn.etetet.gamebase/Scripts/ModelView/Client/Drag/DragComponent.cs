using System.Numerics;
using Vector3 = UnityEngine.Vector3;

namespace ET
{
    [ComponentOf()]
    public class DragComponent : Entity, IAwake, IUpdate
    {
        public bool IsDragging;
        public bool IsClickDown;
        public Vector3 StartWorldPos;
        public Vector3 DragStartPos;

        //当前选中的entity
        public EntityRef<Entity> CurrentSelectedEntity;
    }
}