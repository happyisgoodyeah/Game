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
                
                var objEntityRefSlot = self.slotTransform.gameObject.AddComponent<GameObjectEntityRef>();
                objEntityRefSlot.Entity = self;
                
                var objEntityRefPuzzle = self.puzzleTransform.gameObject.AddComponent<GameObjectEntityRef>();
                objEntityRefPuzzle.Entity = self;
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

        /// <summary>
        /// 根据碰撞点返回对应的SlotView
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static SlotView GetSlotViewByClosePoint(this GridView self , Vector3 closePoint)
        {
            Grid grid = self.GetParent<Grid>();
            SlotView index = null;
            float indexDistance = 999f;
            foreach (var gridAdsorptionSlot in grid.adsorptionSlots)
            {
                SlotView slotView = gridAdsorptionSlot.Entity.GetComponent<SlotView>();
                var distance = Vector2.Distance(closePoint, slotView.transform.position);
                index = distance < indexDistance ? slotView : index;
                indexDistance = Mathf.Min(distance, indexDistance);
            }
            return index;
        }
    }
}