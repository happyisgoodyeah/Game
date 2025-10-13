using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
                if (currentDraggingEntity == null)
                {
                    self.TryStartDrag();    
                }
            }

            //鼠标左键按下+鼠标移动触发拖拽，否则触发点击事件
            if (currentDraggingEntity != null && self.IsClickDown)
            {
                float zDistance = (0 - Camera.main.transform.position.z) / Camera.main.transform.forward.z;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x , Input.mousePosition.y , zDistance));
                worldPos.z = 0;
                
                float dis = Vector3.Distance(self.DragStartPos, Input.mousePosition);
                if (dis > 15)
                {
                    self.StartDrag(currentDraggingEntity, self.DragStartPos);
                }
            }

            //处理所有鼠标施放事件
            //鼠标释放，且命中物体不是正在拖拽就是点击事件
            if (Input.GetMouseButtonUp(0) && currentDraggingEntity != null)
            {
                if (self.IsDragging)
                {
                    self.EndDrag();
                }
                else
                {
                    //点击事件处理旋转逻辑
                    self.StartClick();
                }
            }

            // 拖拽中更新位置
            if (currentDraggingEntity != null && self.IsDragging)
            {
                self.UpdateDragPosition();
            }
        }

        public static void TryStartDrag(this DragComponent self)
        {
            Vector3 hitpos = self.RayHitDragComponent();
            // if (hitpos != Vector3.zero)
            // {
            //     float zDistance = (0 - Camera.main.transform.position.z) / Camera.main.transform.forward.z;
            //     Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(hitpos.x , hitpos.y , zDistance));
            //     worldPos.z = 0;
            //     
            //     self.DragStartPos = worldPos;
            //     self.IsClickDown = true;
            // }
        }

        public static Vector3 RayHitDragComponent(this DragComponent self)
        {
            // 从主摄像机发射射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 使用Physics2D.Raycast用于2D场景
            RaycastHit2D hit = Physics2D.Raycast(ray.origin,
                ray.direction,
                Mathf.Infinity,
                LayerMask.GetMask("Drag"));
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
                        currentDraggingEntity = entity;
                        self.StartDrag(entity, hit.point);
                        return Input.mousePosition;
                    }
                }
            }

            return Vector3.zero;
        }

        public static void StartClick(this DragComponent self)
        {
            Log.Info($"Rotate 90 .");
            // 发布鼠标点击事件 -- 目前默认点一下旋转90度
            EventSystem.Instance.Publish(currentDraggingEntity.Root(), new ClickRotateEvent() { Entity = currentDraggingEntity, Angle = 90 });
            //触发完点击事件将当前命中物体清零
            currentDraggingEntity = null;
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

            float zDistance = (0 - Camera.main.transform.position.z) / Camera.main.transform.forward.z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x , mousePos.y , zDistance));
            worldPos.z = 0;

            // 发布拖拽更新事件
            EventSystem.Instance.Publish(currentDraggingEntity.Root(),
                new DragUpdateEvent { Entity = currentDraggingEntity, CurrentPosition = worldPos });
        }

        public static void EndDrag(this DragComponent self)
        {
            self.IsDragging = false;
            self.IsClickDown = false;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0;

            float zDistance = (0 - Camera.main.transform.position.z) / Camera.main.transform.forward.z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x , mousePos.y , zDistance));
            worldPos.z = 0;

            // 发布拖拽结束事件
            EventSystem.Instance.Publish(currentDraggingEntity.Root(), new DragEndEvent { Entity = currentDraggingEntity, EndPosition = worldPos });

            currentDraggingEntity = null;
        }
    }
}