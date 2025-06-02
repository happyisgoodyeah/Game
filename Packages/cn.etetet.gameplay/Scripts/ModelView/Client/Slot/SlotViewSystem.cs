using Sirenix.OdinInspector;
using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(SlotView))]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Grid))]
    public static partial class SlotViewSystem
    {
        [EntitySystem]
        private static void Awake(this ET.SlotView self, Transform transform)
        {
            self.transform = transform;
            self.parentTransform = transform.parent;

            Slot slot = self.GetParent<Slot>();
            Grid grid = slot.GetParent<Grid>();

            var gridSize = grid.gridSize;
            
            // (int)不然会出现一个警告
            var index = new Vector2((slot.position.X - (int)(gridSize.X / 2)) * (200 / 100f) , (slot.position.Y - (int)(gridSize.Y / 2)) * (200 / 100f));
            self.transform.position = index;
        }

        /// <summary>
        /// 判定Puzzle是否在Slot内
        /// </summary>
        /// <returns></returns>
        public static bool CheckPuzzle(this ET.SlotView self , PuzzleView puzzleView)
        {
            Vector3 position = puzzleView.transform.position;
            if (Vector3.Distance(self.transform.position, position) <= (200 / 100f))
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

            self.GetParent<Slot>().puzzleRef = puzzle;
            EventSystem.Instance.Publish(self.Root() , new SlotSetPuzzle{slot = self.GetParent<Slot>()});
        }
    }
}