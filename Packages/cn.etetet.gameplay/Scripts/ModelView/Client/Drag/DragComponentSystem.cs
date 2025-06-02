using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(DragComponent))]
    public static partial class DragComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.DragComponent self)
        {

        }
        
        // 当前拖拽中的实体
        [StaticField]
        private static Entity currentDraggingEntity;

        [EntitySystem]
        private static void Update(this DragComponent self)
        {
            // 鼠标按下时检测可拖拽实体
            if (Input.GetMouseButtonDown(0))
            {
                self.TryStartDrag();
            }

            // 拖拽中更新位置
            if (currentDraggingEntity != null)
            {
                self.UpdateDragPosition();

                // 鼠标释放时结束拖拽
                if (Input.GetMouseButtonUp(0))
                {
                    self.EndDrag();
                }
            }
        }

        public static void TryStartDrag(this DragComponent self)
        {
            // 从主摄像机发射射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 使用Physics2D.Raycast用于2D场景
            RaycastHit2D hit = Physics2D.Raycast(
                ray.origin,
                ray.direction,
                Mathf.Infinity,
                LayerMask.GetMask("Drag")
            );

            if (hit.collider != null)
            {
                // 通过EntityLink获取关联的ET实体
                GameObjectEntityRef gameObjectEntityRef = hit.collider.GetComponent<GameObjectEntityRef>();
                if (gameObjectEntityRef != null && gameObjectEntityRef.Entity != null)
                {
                    Entity entity = gameObjectEntityRef.Entity;

                    // 检查实体是否有可拖拽组件
                    if (entity.HasComponent<DraggableTag>() && entity.HasComponent<DragComponent>())
                    {
                        self.StartDrag(entity, hit.point);
                    }
                }
            }
        }

        public static void StartDrag(this DragComponent self, Entity entity, Vector3 hitPoint)
        {
            //绑定当前拖拽实体
            currentDraggingEntity = entity;

            self.IsDragging = true;
            self.StartWorldPos = hitPoint;

            // 发布拖拽开始事件
            EventSystem.Instance.Publish(currentDraggingEntity.Root(), new DragStartEvent { Entity = entity, StartPosition = hitPoint });
        }

        public static void UpdateDragPosition(this DragComponent self)
        {
            // 获取鼠标在场景中的位置
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0;

            // 发布拖拽更新事件
            EventSystem.Instance.Publish(currentDraggingEntity.Root(), new DragUpdateEvent { Entity = currentDraggingEntity, CurrentPosition = worldPos });
        }

        public static void EndDrag(this DragComponent self)
        {
            self.IsDragging = false;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0;

            // 发布拖拽结束事件
            EventSystem.Instance.Publish(currentDraggingEntity.Root(), new DragEndEvent { Entity = currentDraggingEntity, EndPosition = worldPos });

            currentDraggingEntity = null;
        }
    }
}
