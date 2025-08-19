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
        /// 方向检测
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static SnapDirection GetSnapDirection(this ET.SlotView self , float x , float y)
        {
            // 计算法线角度
            float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        
            // 标准化角度
            if (angle < 0) angle += 360;
        
            // 划分为8个方向
            if (angle >= 337.5 || angle < 22.5) return SnapDirection.Right;
            if (angle >= 22.5 && angle < 67.5) return SnapDirection.UpRight;
            if (angle >= 67.5 && angle < 112.5) return SnapDirection.Up;
            if (angle >= 112.5 && angle < 157.5) return SnapDirection.UpLeft;
            if (angle >= 157.5 && angle < 202.5) return SnapDirection.Left;
            if (angle >= 202.5 && angle < 247.5) return SnapDirection.DownLeft;
            if (angle >= 247.5 && angle < 292.5) return SnapDirection.Down;
            return SnapDirection.DownRight;
        }
    }
}