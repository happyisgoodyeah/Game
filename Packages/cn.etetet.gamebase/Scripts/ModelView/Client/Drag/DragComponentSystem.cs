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

        [EntitySystem]
        private static void Update(this DragComponent self)
        {
            if (self.IsClickDown)
            {
                //处于选中状态，接下来判断鼠标松开时的距离，决定进入拖拽状态，或进入点击旋转事件
                Vector3 currMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

                float dis = Vector3.Distance(self.DragStartPos, currMousePos);
                if (dis > 0.5f)
                {
                    //触发会自动解开ClickDown选中状态，不会再次进入该逻辑当中
                    self.StartDrag(self.CurrentSelectedEntity.Entity, self.DragStartPos);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    //选中状态后处于鼠标单击事件会触发旋转2逻辑
                    //并且重置所有当前状态
                    self.StartRotate(self.CurrentSelectedEntity.Entity);
                    self.CurrentSelectedEntity = default;
                    self.IsClickDown = false;
                    self.IsDragging = false;
                }
            }
            else if (self.IsDragging)
            {
                //理论上处于IsDragger状态下currentSelectedEntity都应该部位Null.
                //处于单击选中状态时，鼠标右键（android为Input.GetTouch）处理旋转逻辑
                if (Input.GetMouseButtonDown(1))
                {
                    self.StartRotate(self.CurrentSelectedEntity.Entity);
                }

                //鼠标释放，且命中物体不是正在拖拽就是点击事件
                if (Input.GetMouseButtonUp(0))
                {
                    self.EndDrag();
                }
            }
            else
            {
                //空状态
                if (Input.GetMouseButtonDown(0))
                {
                    // 鼠标按下时检测可拖拽实体
                    if (self.CurrentSelectedEntity == default)
                    {
                        self.TryStartDrag();
                    }
                }
            }

            // 拖拽中更新位置
            if (self.CurrentSelectedEntity != default && self.IsDragging)
            {
                self.UpdateDragPosition();
            }
        }

        public static void TryStartDrag(this DragComponent self)
        {
            Vector3 hitpos = self.RayHitDragComponent();

            if (hitpos != Vector3.zero)
            {
                //记录点击坐标
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(hitpos.x, hitpos.y, 0));

                self.DragStartPos = worldPos;
                self.IsClickDown = true;
            }
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
                        self.CurrentSelectedEntity = entity;
                        //self.StartDrag(entity, hit.point);

                        return Input.mousePosition;
                    }
                }
            }

            return Vector3.zero;
        }

        public static void StartRotate(this DragComponent self, Entity currEntity)
        {
            Log.Info($"Rotate 90 .");
            // 发布鼠标点击事件 -- 目前默认点一下旋转90度
            EventSystem.Instance.Publish(currEntity.Root(),
                new ClickRotateEvent() { Entity = currEntity, Angle = 90 });
        }

        public static void StartDrag(this DragComponent self, Entity entity, Vector3 hitPoint)
        {
            //进入拖拽状态取消isClickDown状态
            //绑定当前拖拽实体
            self.CurrentSelectedEntity = entity;
            self.IsClickDown = false;
            self.IsDragging = true;
            self.StartWorldPos = hitPoint;

            // 发布拖拽开始事件
            EventSystem.Instance.Publish(self.CurrentSelectedEntity.Entity.Root(), new DragStartEvent { Entity = entity, StartPosition = hitPoint });
        }

        public static void UpdateDragPosition(this DragComponent self)
        {
            // 获取鼠标在场景中的位置
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0;

            float zDistance = (0 - Camera.main.transform.position.z) / Camera.main.transform.forward.z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, zDistance));
            worldPos.z = 0;

            // 发布拖拽更新事件
            EventSystem.Instance.Publish(self.CurrentSelectedEntity.Entity.Root(),
                new DragUpdateEvent { Entity = self.CurrentSelectedEntity.Entity, CurrentPosition = worldPos });
        }

        public static void EndDrag(this DragComponent self)
        {
            self.IsDragging = false;
            self.IsClickDown = false;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0;

            float zDistance = (0 - Camera.main.transform.position.z) / Camera.main.transform.forward.z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, zDistance));
            worldPos.z = 0;

            // 发布拖拽结束事件
            EventSystem.Instance.Publish(self.CurrentSelectedEntity.Entity.Root(), new DragEndEvent { Entity = self.CurrentSelectedEntity.Entity, EndPosition = worldPos });

            self.CurrentSelectedEntity = default;
        }
    }
}