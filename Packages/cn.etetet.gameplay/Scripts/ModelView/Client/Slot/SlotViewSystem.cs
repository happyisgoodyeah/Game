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

        public static Vector2 GetClosestPointOnSquare(Vector2 pos , float r)
        {
            // 计算到各边的距离
            float dx = Mathf.Min(Mathf.Abs(pos.x - r), Mathf.Abs(pos.x + r));
            float dy = Mathf.Min(Mathf.Abs(pos.y - r), Mathf.Abs(pos.y + r));
        
            // 确定最近点在哪个边界上
            if (dx < dy)
            {
                // 更靠近左右边界
                if (Mathf.Abs(pos.x - r) < Mathf.Abs(pos.x + r))
                    return new Vector2(r, Mathf.Clamp(pos.y, -r, r)); // 右边界
                else
                    return new Vector2(-r, Mathf.Clamp(pos.y, -r, r)); // 左边界
            }
            else
            {
                // 更靠近上下边界
                if (Mathf.Abs(pos.y - r) < Mathf.Abs(pos.y + r))
                    return new Vector2(Mathf.Clamp(pos.x, -r, r), r); // 上边界
                else
                    return new Vector2(Mathf.Clamp(pos.x, -r, r), -r); // 下边界
            }
        }

        // 检查点是否在正方形内部（不包括边界）
        public static bool IsInsideSquare(Vector2 pos  , float r)
        {
            return Mathf.Abs(pos.x) < r && Mathf.Abs(pos.y) < r;
        }
        
        /// <summary>
        /// 方向检测
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static SnapDirection GetSnapDirection(this ET.SlotView self , Vector2 pos, float r , bool isDistinction = false)
        {
            // 1. 将点转换到以A为中心的坐标系
            Vector2 relativePoint = pos - new Vector2(self.transform.position.x , self.transform.position.y);
            
            if (IsInsideSquare(relativePoint , r))
            {
                return SnapDirection.None;
            }
            
            var closePoint = GetClosestPointOnSquare(relativePoint , r);
            
            // 计算法线角度
            float angle = Mathf.Atan2(closePoint.y, closePoint.x) * Mathf.Rad2Deg;
        
            // 标准化角度
            if (angle < 0) angle += 360;
        
            // 角度分区定义（以正右方为0度，逆时针旋转）
            const float cornerRange = 22.5f; // 角区域占22.5度
            const float edgeRange = 45f;     // 边区域占45度
        
            // 调整角度，使0度指向正右方
            angle = (angle + 360) % 360;
        
            // 右上角区域 (22.5° - 45°)
            if (angle > 22.5f && angle <= 45f)
            {
                return isDistinction ? SnapDirection.Right : SnapDirection.UpRight;    
            }
            
            // 右上角区域 (45f° - 67.5°)
            if (angle > 45f && angle <= 67.5f)
            {
                return isDistinction ? SnapDirection.Up : SnapDirection.UpRight;    
            }
        
            // 上边缘区域 (67.5° - 112.5°)
            if (angle > 67.5f && angle <= 112.5f)
                return SnapDirection.Up;
        
            // 左上角区域 (112.5° - 135f.5°)
            if (angle > 112.5f && angle <= 135f)
            {
                return isDistinction ? SnapDirection.Up : SnapDirection.UpLeft;    
            }
            
            // 左上角区域 (135f° - 157.5°)
            if (angle > 135f && angle <= 157.5f)
            {
                return isDistinction ? SnapDirection.Left : SnapDirection.UpLeft;    
            }
        
            // 左边缘区域 (157.5° - 202.5°)
            if (angle > 157.5f && angle <= 202.5f)
                return SnapDirection.Left;
        
            // 左下角区域 (202.5° - 225°)
            if (angle > 202.5f && angle <= 225f)
            {
                return isDistinction ? SnapDirection.Left : SnapDirection.DownLeft;    
            }
            
            // 左下角区域 (202.5° - 247.5°)
            if (angle > 225f && angle <= 247.5f)
            {
                return isDistinction ? SnapDirection.Down : SnapDirection.DownLeft;    
            }
        
            // 下边缘区域 (247.5° - 292.5°)
            if (angle > 247.5f && angle <= 292.5f)
                return SnapDirection.Down;
        
            // 右下角区域 (292.5° - 315°)
            if (angle > 292.5f && angle <= 315f)
            {
                return isDistinction ? SnapDirection.Down : SnapDirection.DownRight;   
            }
            
            // 右下角区域 (315° - 337.5°)
            if (angle > 315f && angle <= 337.5f)
            {
                return isDistinction ? SnapDirection.Right : SnapDirection.DownRight;   
            }
        
            // 右边缘区域 (337.5° - 22.5°)
            return SnapDirection.Right;
        }
    }
}