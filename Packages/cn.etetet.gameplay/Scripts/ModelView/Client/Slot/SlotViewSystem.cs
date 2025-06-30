using Sirenix.OdinInspector;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(SlotView))]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Grid))]
    [FriendOf(typeof(Puzzle))]
    public static partial class SlotViewSystem
    {
        [EntitySystem]
        private static void Awake(this ET.SlotView self, Transform transform)
        {
            self.transform = transform;
            self.parentTransform = transform.parent;
            self.AddComponent<SpriteRenderComponent , GameObject>(transform.Find("Square").gameObject);
            self.UpdatePosition();
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        /// <param name="self"></param>
        public static void UpdatePosition(this ET.SlotView self)
        {
            Slot slot = self.GetParent<Slot>();
            var spriteSize = self.GetComponent<SpriteRenderComponent>().SpriteSize;

            if (self.GetParent<Grid>() != null) //grid的slot
            {
                Grid grid = slot.GetParent<Grid>();
                var gridSize = grid.gridSize;
            
                // (int)不然会出现一个警告
                var index = new Vector2((slot.position.X - (int)(gridSize.X / 2)) * spriteSize.x ,
                    (slot.position.Y - (int)(gridSize.Y / 2)) * spriteSize.y);
                self.transform.position = index;    
            }
            else if (self.GetParent<Puzzle>() != null) //puzzle的slot
            {
                Puzzle puzzle = self.GetParent<Puzzle>();
                
                var index = new Vector2(slot.position.X * spriteSize.x , slot.position.Y * spriteSize.y);
                self.transform.position = index;
            }
        }

        /// <summary>
        /// 判定Puzzle是否在Slot内
        /// </summary>
        /// <returns></returns>
        public static bool CheckPuzzle(this ET.SlotView self , PuzzleView puzzleView)
        {
            Vector3 position = puzzleView.transform.position;
            if (Vector3.Distance(self.transform.position, position) <= self.GetComponent<SpriteRenderComponent>().SpriteSize.x / 2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置当前Puzzle
        /// </summary>
        public static void SetPuzzle(this ET.SlotView self , Puzzle puzzle)
        {
            //当前已经绑定有Puzzle
            var nowPuzzle = self.GetParent<Slot>().puzzleRef.Entity;
            if (nowPuzzle != null)
            {
                nowPuzzle.GetComponent<PuzzleView>().BackToOriginPosition();
            }
            puzzle.ResetBindSlots();
            puzzle.slots.Add(self.GetParent<Slot>());
            self.GetParent<Slot>().puzzleRef = puzzle;
            EventSystem.Instance.Publish(self.Root() , new SlotSetPuzzle{slot = self.GetParent<Slot>()});
        }

        /// <summary>
        /// slot检测
        /// </summary>
        public static (bool isPass , Slot slot) SlotCheck(this ET.SlotView self)
        {
            // 从主摄像机发射射线
            var position = Camera.main.WorldToScreenPoint(self.transform.position);
            Ray ray = Camera.main.ScreenPointToRay(position);

            // 使用Physics2D.Raycast用于2D场景
            RaycastHit2D hit = Physics2D.Raycast(
                ray.origin,
                ray.direction,
                Mathf.Infinity,
                LayerMask.GetMask("Slot")
            );

            if (hit.collider != null)
            {
                // 通过EntityLink获取关联的ET实体
                GameObjectEntityRef gameObjectEntityRef = hit.collider.GetComponent<GameObjectEntityRef>();
                if (gameObjectEntityRef != null && gameObjectEntityRef.Entity != null)
                {
                    if (gameObjectEntityRef.Entity is SlotView slotView)
                    {
                        var slot = slotView.GetParent<Slot>();
                        if (slot.puzzleRef.Entity == null)
                        {
                            return (true , slot);
                        }
                    }
                }
            }

            return (false , null);
        }
    }
}