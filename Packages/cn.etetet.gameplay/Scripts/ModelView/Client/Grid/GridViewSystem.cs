using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(GridView))]
    [FriendOf(typeof(Grid))]
    public static partial class GridViewSystem
    {
        [EntitySystem]
        private static void Awake(this ET.GridView self , Transform transform)
        {
            self.transform = transform;
            self.parentTransform = transform.parent;

            if (self.transform != null)
            {
                self.slotTransform = transform.Find("SlotTransform");
                self.puzzleTransform = transform.Find("PuzzleTransform");;
            }
        }

        /// <summary>
        /// 检测当前puzzle是否合法
        /// </summary>
        /// <param name="self"></param>
        /// <param name="puzzleView"></param>
        /// <returns></returns>
        public static (bool , SlotView) CheckPuzzleInSlot(this GridView self , PuzzleView puzzleView)
        {
            Grid grid = self.GetParent<Grid>();
            foreach (var slotRef in grid.slotDic.Values)
            {
                Slot slot = slotRef.Entity;
                SlotView slotView = slot.GetComponent<SlotView>();
                
                if (slotView.CheckPuzzle(puzzleView))
                {
                    return (true, slotView);
                }
            }
            return (false, null);
        }
    }
}