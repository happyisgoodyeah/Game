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
    }
}